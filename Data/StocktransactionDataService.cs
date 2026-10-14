using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.CodeDom;
using Newtonsoft.Json;
using InfyPOS.Processors;

namespace InfyPOS.Data
{
    public class Stocktransaction
    {
        public long id { get; set; }
        public long salablegoodsid { get; set; }
        public long stockid { get; set; }
        public long createdby { get; set; }
        public DateTime createdon { get; set; }
        public long locationid { get; set; }
        public string reftype { get; set; }
        public decimal qty { get; set; }
        public DateTime transactedon { get; set; }
        public TransactionType type
        {
            get
            {
                switch (reftype)
                {
                    case enumPurchase:
                        return TransactionType.Purchase;
                    case enumPurchaseReturn:
                        return TransactionType.PurchaseReturn;
                    case enumTransferIn:
                        return TransactionType.TransferIn;
                    case enumTransferOut:
                        return TransactionType.TransferOut;
                    case enumSales:
                        return TransactionType.Sale;
                    case enumJournal:
                        return TransactionType.Journal;
                    case enumDamage:
                        return TransactionType.Damage;
                    case enumAudit:
                        return TransactionType.Audit;
                    case enumMissing:
                        return TransactionType.Missing;
                }
                return TransactionType.Opening;
            }
            set
            {
                switch (value)
                {
                    case TransactionType.Purchase:
                        reftype = enumPurchase;
                        return;
                    case TransactionType.PurchaseReturn:
                        reftype = enumPurchaseReturn;
                        return;
                    case TransactionType.TransferIn:
                        reftype = enumTransferIn;
                        return;
                    case TransactionType.TransferOut:
                        reftype = enumTransferOut;
                        return;
                    case TransactionType.Sale:
                        reftype = enumSales;
                        return;
                    case TransactionType.Journal:
                        reftype = enumJournal;
                        return;
                    case TransactionType.Damage:
                        reftype = enumDamage;
                        return;
                    case TransactionType.Missing:
                        reftype = enumMissing;
                        return;
                    case TransactionType.Audit:
                        reftype = enumAudit;
                        return;
                }
                reftype = enumOpening;
            }
        }
        public static string GetType(TransactionType value)
        {
            switch (value)
            {
                case TransactionType.Purchase:
                    return enumPurchase;
                case TransactionType.PurchaseReturn:
                    return enumPurchaseReturn;
                case TransactionType.TransferIn:
                    return enumTransferIn;
                case TransactionType.TransferOut:
                    return enumTransferOut;
                case TransactionType.Sale:
                    return enumSales;
                case TransactionType.Journal:
                    return enumJournal;
                case TransactionType.Damage:
                    return enumDamage;
                case TransactionType.Missing:
                    return enumMissing;
                case TransactionType.Audit:
                    return enumAudit;
            }
            return enumOpening;
        }
        const string enumOpening = "OP";
        const string enumPurchase = "PU";
        const string enumPurchaseReturn = "PR";
        const string enumTransferIn = "TI";
        const string enumTransferOut = "TO";
        const string enumSales = "SL";
        const string enumAudit = "AU";
        const string enumMissing = "MS";
        const string enumJournal = "JN";
        const string enumDamage = "DM";
        public long prefid { get; set; }
        public long refid { get; set; }
        public enum TransactionType
        {
            Opening = 0,
            Purchase = 1,
            PurchaseReturn = 2,
            TransferIn = 3,
            TransferOut = 4,
            Sale = 5,
            Missing = 6,
            Audit = 7,
            Journal = 10,
            Damage = 13
        }
    }

    public class StockTransactionMissmatches
    {

        public long stockid { get; set; }
        public long salablegoodsid { get; set; }
        public string barcode { get; set; }

        public string locationname { get; set; }
        public string productname { get; set; }

        public decimal systemstk { get; set; }
        public decimal transactionstk { get; set; }
        public decimal diffstk { get; set; }

    }
    public class StocktransactionDataService : SAASData
    {
        const string SQL_Sync_Stock = @" 
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
                    select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,stock.purchaseqty,
                                stock.locationid,0,stock.id,stock.createdby,
                                'PU' reftype,stock.createdon
                    from stock
                    left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
                    left outer join lrinvoice on lrinvoice.id =lrinvoiceid
                    left outer join lrentry on lrentry.id = lrid
					where stock.isactive=true and stock.purchaseqty>0 ;

                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric)*-1 qty,bundles.fromlocationid,
                0 salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TO' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist on referencelist.id = fromlocationid and cast(referencelist.additionalinfo->>'iswarehouse' as bool) = true
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;
				
				

				insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric) qty,bundles.tolocationid,
                0 salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TI' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist fromref on fromref.id = fromlocationid and cast(fromref.additionalinfo->>'iswarehouse' as bool) = false
				inner join referencelist toref on toref.id = tolocationid and cast(toref.additionalinfo->>'iswarehouse' as bool) = true
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;


				insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)

                select lrinvoicereturnid,lrinvoicereturnitems.id,lrinvoicereturnitems.modifiedon,
                lrinvoicereturnitems.qty*-1,stock.locationid,
                0,stock.id,
                lrinvoicereturnitems.modifiedby,'PR' reftype,lrinvoicereturnitems.modifiedon 
				from lrinvoicereturnitems
				inner join lrinvoicereturn on lrinvoicereturn.id = lrinvoicereturnid and lrinvoicereturn.returntype<10
				inner join stock on stock.id = lrinvoicereturnitems.stockid
				inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
				where lrinvoicereturn.isactive=true and lrinvoicereturnitems.isactive=true
				and lrinvoicereturnitems.salablegoodsid = 0


			with retailpurchase
			as
			(
				select stockid from salablegoods where pqty = 0
				and stockid not in (
				select stockid from salablegoods where pqty > 0)
			),missingpurchase 
			as
			(
			select stock.qty,stock.id from stock 
			left outer join retailpurchase on retailpurchase.stockid = stock.id
			where purchaseqty = 0 and stock.isactive=true and stock.qty>0
			)			
			insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
            select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,stock.qty,
                        stock.locationid,0,stock.id,stock.createdby,
                        'PU' reftype,stock.createdon
            from stock
            inner join missingpurchase on missingpurchase.id = stock.id
            left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
            left outer join lrinvoice on lrinvoice.id =lrinvoiceid
            left outer join lrentry on lrentry.id = lrid
			where stock.isactive=true;

			with tstock
            as
            (
            select sum(qty) stk,stockid from stocktransaction where salablegoodsid=0
			and reftype ='TO'
            group by stockid
            ),mstock as
            (
            	select stock.transfered,stock.id
	            from stock 
            	left outer join tstock on tstock.stockid =stock.id
            	where transfered>0 and tstock.stockid is null --hasstock=true and 
            )
			insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)			
			select 
						lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,stock.transfered*-1,
                        stock.locationid,0,stock.id,stock.createdby,
                        'TO' reftype,stock.createdon
            from stock
            inner join mstock on mstock.id = stock.id
            left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
            left outer join lrinvoice on lrinvoice.id =lrinvoiceid
            left outer join lrentry on lrentry.id = lrid
			where stock.isactive=true;
			
			with retailreturnconflict
			as
			(
			select stock.id,returnqty from stock
			inner join salablegoods on salablegoods.stockid = stock.id
			where returnqty>0 and rqty>0
			)
			insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)			
			select 
						lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,stock.returnqty*-1,
                        stock.locationid,0,stock.id,stock.createdby,
                        'TO' reftype,stock.createdon
            from stock
            inner join retailreturnconflict on retailreturnconflict.id = stock.id
            left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
            left outer join lrinvoice on lrinvoice.id =lrinvoiceid
            left outer join lrentry on lrentry.id = lrid
			where stock.isactive=true
			
			
			with tstock
            as
            (
            select sum(qty) stk,stockid from stocktransaction where salablegoodsid=0
            group by stockid
            ),mstock as
            (
            	select barcode,stock.id,
	            stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty) autostk,
	            coalesce(stk,0) stk,
	            (stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) - coalesce(stk,0) diffstk
	            from stock 
            	left outer join tstock on tstock.stockid =stock.id
            	where --hasstock=true and 
            	(stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) != coalesce(stk,0)	
            )
            insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
            select 
						lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,diffstk,
                        stock.locationid,0,stock.id,stock.createdby,
                        'AU' reftype,stock.createdon
            from stock
            inner join mstock on mstock.id = stock.id
            left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
            left outer join lrinvoice on lrinvoice.id =lrinvoiceid
            left outer join lrentry on lrentry.id = lrid
			where stock.isactive=true;

delete from stocktransaction
where salablegoodsid = 0 and stockid in (select id from stock where isactive=false);
";

        const string SQL_Sync_Salablegoods = @"

select count(*) from salablegoods
where pqty+tiqty-toqty-rqty != iqty;

update salablegoods set tiqty = tiqty+(iqty-(pqty+tiqty-toqty-rqty))
where pqty+tiqty-toqty-rqty <iqty;

update salablegoods set toqty = toqty+(pqty+tiqty-toqty-rqty)-iqty
where pqty+tiqty-toqty-rqty >iqty;


                    insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
                    select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,salablegoods.pqty,
                                salablegoods.locationid,salablegoods.id,stock.id,salablegoods.createdby,
                                'PU' reftype,salablegoods.createdon
                    from salablegoods
					inner join stock on stock.id = stockid
                    left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
                    left outer join lrinvoice on lrinvoice.id =lrinvoiceid
                    left outer join lrentry on lrentry.id = lrid
					where stock.isactive=true and salablegoods.pqty>0 and stock.purchaseqty=0;


                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric) qty,bundles.tolocationid,
                (schemelist ->> 'toid') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TI' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist fromlocation on fromlocation.id = fromlocationid and cast(fromlocation.additionalinfo->>'iswarehouse' as bool) = true
                inner join referencelist tolocation on tolocation.id = tolocationid and cast(tolocation.additionalinfo->>'iswarehouse' as bool) = false
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;

                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric) qty,bundles.tolocationid,
                (schemelist ->> 'toid') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TI' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist fromlocation on fromlocation.id = fromlocationid and cast(fromlocation.additionalinfo->>'iswarehouse' as bool) = false
                inner join referencelist tolocation on tolocation.id = tolocationid and cast(tolocation.additionalinfo->>'iswarehouse' as bool) = false
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;

				insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric)*-1 qty,bundles.fromlocationid,
                (schemelist ->> 'id') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TO' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist fromref on fromref.id = fromlocationid and cast(fromref.additionalinfo->>'iswarehouse' as bool) = false
				inner join referencelist toref on toref.id = tolocationid and cast(toref.additionalinfo->>'iswarehouse' as bool) = true
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                inner join salablegoods on salablegoods.id = (schemelist ->> 'id') :: BIGINT
				inner join stock ON stock.id = salablegoods.stockid
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;

				insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric)*-1 qty,bundles.fromlocationid,
                (schemelist ->> 'id') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TO' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist fromref on fromref.id = fromlocationid and cast(fromref.additionalinfo->>'iswarehouse' as bool) = false
				inner join referencelist toref on toref.id = tolocationid and cast(toref.additionalinfo->>'iswarehouse' as bool) = false
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                inner join salablegoods on salablegoods.id = (schemelist ->> 'id') :: BIGINT
				inner join stock ON stock.id = salablegoods.stockid
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;

				insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)

                select lrinvoicereturnid,lrinvoicereturnitems.id,lrinvoicereturnitems.modifiedon,
                lrinvoicereturnitems.qty*-1,salablegoods.locationid,
                salablegoods.ID,salablegoods.stockid,
                lrinvoicereturnitems.modifiedby,'PR' reftype,lrinvoicereturnitems.modifiedon 
				from lrinvoicereturnitems
				inner join lrinvoicereturn on lrinvoicereturn.id = lrinvoicereturnid and lrinvoicereturn.returntype<10
				inner join stock on stock.id = lrinvoicereturnitems.stockid
				inner join salablegoods ON salablegoods.id = lrinvoicereturnitems.salablegoodsid
				inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
				where lrinvoicereturn.isactive=true and lrinvoicereturnitems.isactive=true;


				insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)

                select bill.id,billitems.id,bill.billdate,
                billitems.qty*-1,bill.locationid,
                billitems.salablegoodsid,billitems. stockid,
                billitems.modifiedby,'SL' reftype,billitems.modifiedon 
				from billitems
				inner join bill on bill.id = billid
				inner join stock ON stock.id = billitems.stockid
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                where bill.isactive=true and bill.billtype!=9 and billitems.isactive=true


                with tstock
                as
                (
					select sum(qty) stk,salablegoodsid from stocktransaction where salablegoodsid>0
					AND QTY>0
					group by salablegoodsid
                ),mstock as
                (
                select salablegoods.id,
					stk,iqty
	                from salablegoods 
                	left outer join tstock on tstock.salablegoodsid =salablegoods.id
                	where tstock.salablegoodsid is null and iqty>0
                )
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,salablegoods.iqty-(salablegoods.dqty-salablegoods.jqty),
                            salablegoods.locationid,salablegoods.id,
							salablegoods.stockid,stock.createdby,
                            'TI' reftype,stock.createdon
                from salablegoods
                inner join mstock on mstock.id = salablegoods.id
                inner join stock on stock.id = stockid
                left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
                left outer join lrinvoice on lrinvoice.id =lrinvoiceid
                left outer join lrentry on lrentry.id = lrid
                where salablegoods.isactive=true;
				
				
				
				
			with tstock
            as
            (
            select sum(qty) stk,salablegoodsid from stocktransaction where salablegoodsid>0
            group by salablegoodsid
            ),mstock as
            (
            select barcode,salablegoods.id,
	            salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) autostk,
	            coalesce(stk,0) stk,
	            (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) - coalesce(stk,0) diffstk
	            from salablegoods 
            	left outer join tstock on tstock.salablegoodsid =salablegoods.id
            where --hasstock=true and 
				(salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)) != coalesce(stk,0)	)
			insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
            select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,diffstk,
                        salablegoods.locationid,salablegoods.id salablegoodsid,salablegoods.stockid,salablegoods.createdby,
                        'AU' reftype,salablegoods.createdon
            from salablegoods
            inner join mstock on mstock.id = salablegoods.id
			inner join stock on stock.id = salablegoods.stockid
            left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
            left outer join lrinvoice on lrinvoice.id =lrinvoiceid
            left outer join lrentry on lrentry.id = lrid
			where stock.isactive=true

            delete from stocktransaction
            where salablegoodsid in (select id from salablegoods where isactive=false);";


        const string SQL_Sync_SalablegoodsOLD = @"
select count(*) from salablegoods
where pqty+tiqty-toqty-rqty != iqty;

update salablegoods set tiqty = tiqty+(iqty-(pqty+tiqty-toqty-rqty))
where pqty+tiqty-toqty-rqty <iqty;

update salablegoods set toqty = toqty+(pqty+tiqty-toqty-rqty)-iqty
where pqty+tiqty-toqty-rqty >iqty;

delete from stocktransaction;

//inward from warehouse
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric) qty,bundles.tolocationid,
                (schemelist ->> 'toid') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TI' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist on referencelist.id = fromlocationid and cast(referencelist.additionalinfo->>'iswarehouse' as bool) = true
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;
				
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric)*-1 qty,bundles.fromlocationid,
                0 salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TO' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist on referencelist.id = fromlocationid and cast(referencelist.additionalinfo->>'iswarehouse' as bool) = true
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;

//swap goods
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric) qty,bundles.tolocationid,
                (schemelist ->> 'toid') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TI' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist fromref on fromref.id = fromlocationid and cast(fromref.additionalinfo->>'iswarehouse' as bool) = false
				inner join referencelist toref on toref.id = tolocationid and cast(toref.additionalinfo->>'iswarehouse' as bool) = false
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;


                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric)*-1 qty,bundles.fromlocationid,
                (schemelist ->> 'id') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TO' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist fromref on fromref.id = fromlocationid and cast(fromref.additionalinfo->>'iswarehouse' as bool) = false
				inner join referencelist toref on toref.id = tolocationid and cast(toref.additionalinfo->>'iswarehouse' as bool) = false
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;

//inward to warehouse
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric)*-1 qty,bundles.fromlocationid,
                (schemelist ->> 'id') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TO' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist fromref on fromref.id = fromlocationid and cast(fromref.additionalinfo->>'iswarehouse' as bool) = false
				inner join referencelist toref on toref.id = tolocationid and cast(toref.additionalinfo->>'iswarehouse' as bool) = true
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                inner join salablegoods on salablegoods.id = (schemelist ->> 'id') :: BIGINT
				inner join stock ON stock.id = salablegoods.stockid
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;

				insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric) qty,bundles.tolocationid,
                0 salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TI' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist fromref on fromref.id = fromlocationid and cast(fromref.additionalinfo->>'iswarehouse' as bool) = false
				inner join referencelist toref on toref.id = tolocationid and cast(toref.additionalinfo->>'iswarehouse' as bool) = true
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;

//Sales
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)

                select bill.id,billitems.id,bill.billdate,
                billitems.qty*-1,bill.locationid,
                billitems.salablegoodsid,billitems. stockid,
                billitems.modifiedby,'SL' reftype,billitems.modifiedon 
				from billitems
				inner join bill on bill.id = billid
				inner join stock ON stock.id = billitems.stockid
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                where bill.isactive=true and bill.billtype!=9 and billitems.isactive=true;

---purchase return
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)

                select lrinvoicereturnid,lrinvoicereturnitems.id,lrinvoicereturnitems.modifiedon,
                lrinvoicereturnitems.qty,salablegoods.locationid,
                salablegoods.ID,salablegoods.stockid,
                lrinvoicereturnitems.modifiedby,'PR' reftype,lrinvoicereturnitems.modifiedon 
				from lrinvoicereturnitems
				inner join lrinvoicereturn on lrinvoicereturn.id = lrinvoicereturnid and lrinvoicereturn.returntype<10
				inner join stock on stock.id = lrinvoicereturnitems.stockid
				inner join salablegoods ON salablegoods.id = lrinvoicereturnitems.salablegoodsid
				inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
				where lrinvoicereturn.isactive=true and lrinvoicereturnitems.isactive=true;

--Inward or purchase from salablegoods
            with ststock
            as(
            select salablegoodsid,sum(qty) cst from stocktransaction
	            where salablegoodsid>0
            group by salablegoodsid
            )

            insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)

            select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,(iqty-(oqty+dqty+jqty))-cst,
            salablegoods.locationid,salablegoods.id,salablegoods.stockid,stock.createdby,
            case when pqty>0 then 'PU' else 'TI' END reftype,stock.createdon from ststock
            inner join salablegoods on salablegoods.id = ststock.salablegoodsid
            inner join stock on stock.id = stockid
            inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
            inner join lrinvoice on lrinvoice.id = lrinvoiceid
            where iqty-(oqty+dqty+jqty) > cst and lrinvoice.entrydate>'2020-01-01'
            and salablegoods.isactive=true and stock.isactive=true and lrinvoiceitems.isactive=true;


--CREATE PURCHASE FOR PURCHASE RETURN
with ststock
as(
select salablegoodsid,sum(qty) cst from stocktransaction
	where salablegoodsid>0
group by salablegoodsid
)

insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,(iqty-(oqty+dqty+jqty))-cst,
salablegoods.locationid,salablegoods.id,salablegoods.stockid,stock.createdby,
'PU' reftype,stock.createdon from ststock
inner join salablegoods on salablegoods.id = ststock.salablegoodsid
inner join stock on stock.id = stockid
inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
inner join lrinvoice on lrinvoice.id = lrinvoiceid
where iqty-(oqty+dqty+jqty) > cst and lrinvoice.entrydate>'2020-01-01';


--stock purchase 
with ststock
as(
select stockid,sum(qty) cst from stocktransaction
	where salablegoodsid=0
group by stockid
),dpurchase as
(
	select sum(pqty), stockid from salablegoods
	where pqty>0
	group by stockid
)

insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)

select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)))-cst,
stock.locationid,0,stock.id,stock.createdby,
'PU' reftype,stock.createdon
from ststock
left outer join dpurchase on dpurchase.stockid = ststock.stockid
inner join stock on stock.id = ststock.stockid
left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
left outer join lrinvoice on lrinvoice.id = lrinvoiceid
where (stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) > cst 
and dpurchase.stockid is null and stock.isactive=true and
lrinvoiceitems.isactive=true and lrinvoice.isactive=true;


--Stock transfer direct purchase to retail
with ststock
as(
select stockid,sum(qty) cst from stocktransaction
	where salablegoodsid=0
group by stockid
),dpurchase as
(
	select sum(pqty), stockid from salablegoods
	where pqty>0
	group by stockid
)

insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)

select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)))-cst,
stock.locationid,0,stock.id,stock.createdby,
'TO' reftype,stock.createdon
from ststock
left outer join dpurchase on dpurchase.stockid = ststock.stockid
inner join stock on stock.id = ststock.stockid
left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
left outer join lrinvoice on lrinvoice.id = lrinvoiceid
where (stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) != cst 
and dpurchase.stockid is not null  and stock.isactive=true and
lrinvoiceitems.isactive=true and lrinvoice.isactive=true;

--direct purchase missing
with tstock
as
(
select sum(qty) stk,salablegoodsid from stocktransaction where salablegoodsid>0
group by salablegoodsid
),mstock as
(
select barcode,salablegoods.id,
	salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) autostk,
	coalesce(stk,0) stk,
	salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) - coalesce(stk,0) diffstk
	from salablegoods 
left outer join tstock on tstock.salablegoodsid =salablegoods.id
where hasstock=true and 
salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) != coalesce(stk,0)	
)
insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,diffstk,
            salablegoods.locationid,salablegoods.id,salablegoods.stockid,stock.createdby,
            'PU' reftype,stock.createdon
from salablegoods
inner join mstock on mstock.id = salablegoods.id
inner join stock on stock.id = stockid
left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
left outer join lrinvoice on lrinvoice.id =lrinvoiceid
left outer join lrentry on lrentry.id = lrid
where lrentry.retaillocationid>0 and  diffstk>0;

--missing stock to transfer to stock and then transfer to retail
with tstock
as
(
select sum(qty) stk,salablegoodsid from stocktransaction where salablegoodsid>0
group by salablegoodsid
),mstock as
(
select barcode,salablegoods.id,
	salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) autostk,
	coalesce(stk,0) stk,
	salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) - coalesce(stk,0) diffstk
	from salablegoods 
left outer join tstock on tstock.salablegoodsid =salablegoods.id
where hasstock=true and 
salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) != coalesce(stk,0)	
)

insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,diffstk,
            stock.locationid,0,salablegoods.stockid,stock.createdby,
            'PU' reftype,stock.createdon
from salablegoods
inner join mstock on mstock.id = salablegoods.id
inner join stock on stock.id = stockid
left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
left outer join lrinvoice on lrinvoice.id =lrinvoiceid
left outer join lrentry on lrentry.id = lrid
where diffstk>0;



with tstock
as
(
select sum(qty) stk,salablegoodsid from stocktransaction where salablegoodsid>0
group by salablegoodsid
),mstock as
(
select barcode,salablegoods.id,
	salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) autostk,
	coalesce(stk,0) stk,
	salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) - coalesce(stk,0) diffstk
	from salablegoods 
left outer join tstock on tstock.salablegoodsid =salablegoods.id
where hasstock=true and 
salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) != coalesce(stk,0)	
)

insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,diffstk*-1,
            stock.locationid,0,salablegoods.stockid,stock.createdby,
            'TO' reftype,stock.createdon
from salablegoods
inner join mstock on mstock.id = salablegoods.id
inner join stock on stock.id = stockid
left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
left outer join lrinvoice on lrinvoice.id =lrinvoiceid
left outer join lrentry on lrentry.id = lrid
where diffstk>0;


with tstock
as
(
select sum(qty) stk,salablegoodsid from stocktransaction where salablegoodsid>0
group by salablegoodsid
),mstock as
(
select barcode,salablegoods.id,
	salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) autostk,
	coalesce(stk,0) stk,
	salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) - coalesce(stk,0) diffstk
	from salablegoods 
left outer join tstock on tstock.salablegoodsid =salablegoods.id
where hasstock=true and 
salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) != coalesce(stk,0)	
)
insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,diffstk,
            salablegoods.locationid,salablegoods.id,salablegoods.stockid,stock.createdby,
            'TI' reftype,stock.createdon
from salablegoods
inner join mstock on mstock.id = salablegoods.id
inner join stock on stock.id = stockid
left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
left outer join lrinvoice on lrinvoice.id =lrinvoiceid
left outer join lrentry on lrentry.id = lrid
where diffstk>0;

with tstock
as
(
select sum(qty) stk,stockid from stocktransaction where salablegoodsid=0
group by stockid
),mstock as
(
select barcode,stock.id,
	stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty) autostk,
	coalesce(stk,0) stk,
	(stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) - coalesce(stk,0) diffstk
	from stock 
left outer join tstock on tstock.stockid =stock.id
where hasstock=true and 
(stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) != coalesce(stk,0)	
)
insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,diffstk,
            stock.locationid,0,stock.id,stock.createdby,
            'PU' reftype,stock.createdon
from stock
inner join mstock on mstock.id = stock.id
left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
left outer join lrinvoice on lrinvoice.id =lrinvoiceid
left outer join lrentry on lrentry.id = lrid
where diffstk>0;

--JOURNAL WAREHOUSE
insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
				
select lrinvoice.id,lrinvoiceitems.id,stockhistory.createdon,auditqty,
            stock.locationid,0,stockhistory.stockid,
			stockhistory.createdby,'JN' reftype,stockhistory.createdon
from stockhistory
inner join stock on stock.id = stockhistory.stockid
left outer join lrinvoiceitems on lrinvoiceitems.id =stock.invoiceitemid
left outer join lrinvoice on lrinvoice.id =lrinvoiceid
left outer join lrentry on lrentry.id = lrid
where auditqty!=0;

-- JOURNAL RETAIL
insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
				
select 
lrinvoice.id,lrinvoiceitems.id,salablegoodshistory.createdon,auditqty,
            salablegoods.locationid,salablegoods.id,salablegoods.stockid,
			salablegoodshistory.createdby,'JN' reftype,salablegoodshistory.createdon
from salablegoodshistory
inner join salablegoods on salablegoods.id = salablegoodshistory.salablegoodsid
inner join stock on stock.id = salablegoods.stockid
left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
left outer join lrinvoice on lrinvoice.id =lrinvoiceid
left outer join lrentry on lrentry.id = lrid
where auditqty!=0;

--AUTO JOURNAL - WAREHOUSE
with tstock
as
(
select sum(qty) stk,stockid from stocktransaction where salablegoodsid=0
group by stockid
),mstock as
(
select barcode,stock.id,
	stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty) autostk,
	coalesce(stk,0) stk,
	(stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) - coalesce(stk,0) diffstk
	from stock 
left outer join tstock on tstock.stockid =stock.id
where hasstock=true and 
(stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) != coalesce(stk,0)	
)
insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,diffstk,
            stock.locationid,0,stock.id,stock.createdby,
            'JN' reftype,stock.createdon
from stock
inner join mstock on mstock.id = stock.id
left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
left outer join lrinvoice on lrinvoice.id =lrinvoiceid
left outer join lrentry on lrentry.id = lrid;

--AUTO JOURNAL - RETAIL
with tstock
as
(
select sum(qty) stk,salablegoodsid from stocktransaction where salablegoodsid>0
group by salablegoodsid
),mstock as
(
select barcode,salablegoods.id,
	salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) autostk,
	coalesce(stk,0) stk,
	salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) - coalesce(stk,0) diffstk
	from salablegoods 
left outer join tstock on tstock.salablegoodsid =salablegoods.id
where hasstock=true and 
salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) != coalesce(stk,0)	
)
insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,diffstk,
            salablegoods.locationid,salablegoods.id,salablegoods.stockid,stock.createdby,
            'JN' reftype,stock.createdon
from salablegoods
inner join mstock on mstock.id = salablegoods.id
inner join stock on stock.id = stockid
left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
left outer join lrinvoice on lrinvoice.id =lrinvoiceid
left outer join lrentry on lrentry.id = lrid;
";

        const string SQL_Insert = @"
        INSERT INTO public.stocktransaction(prefid,refid,transactedon,qty,locationid,
        salablegoodsid,stockid,createdby,reftype,createdon) 
        VALUES(@prefid,@refid,@transactedon,@qty,@locationid,
        @salablegoodsid,@stockid,@createdby,@reftype,@createdon) RETURNING id";

        const string SQL_DeleteAll = @"
        delete from public.Stocktransaction where reftype=@reftype and prefid=@prefid;";

        const string SQL_Delete = @"
        delete from public.Stocktransaction where reftype=@reftype and refid=@refid;";


        public Stocktransaction Insert(Stocktransaction oStocktransaction)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oStocktransaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oStocktransaction);
                throw exp;
            }

            return oStocktransaction;
        }

        public List<StockTransactionMissmatches> StockMissmatches(long locationid)
        {
            /* with tstock
             as
             (
             select sum(qty) stk,
				 sum(case when reftype != 'SL' then qty else 0 end) instk,
				 sum(case when reftype = 'SL' then qty else 0 end) outstk,
				 sum(case when reftype = 'AU' then qty else 0 end) austk,
				 salablegoodsid,locationid from stocktransaction where salablegoodsid>0
             group by salablegoodsid,locationid
             ),mstock as
             (
             select barcode,salablegoods.id salablegoodsid,salablegoods.stockid,
				 salablegoods.iqty IQTY,
				 salablegoods.Oqty OQTY,
                 salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) systemstk,
                 coalesce(stk,0) transactionstk,
				 salablegoods.locationid,
                 salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) - coalesce(stk,0) diffstk
                 from salablegoods 
                 left outer join tstock on tstock.salablegoodsid =salablegoods.id and tstock.locationid =salablegoods.locationid
                 where salablegoods.isactive=true and
                 salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) != coalesce(stk,0)	
				 --salablegoods.iqty  != coalesce(instk,0)	
             ),sttransaction as
            (
                select * from crosstab(
                'select salablegoodsid,reftype,sum(qty) qty from stocktransaction
                where salablegoodsid>0 
                group by salablegoodsid,reftype
                ORDER BY 1,2',
                'select * from unnest(ARRAY[''TI'', ''TO'',''PU'',''PR'',''SL'',''OP'',''AU'',''MS''])')
                as ct(salablegoodsid bigint,"TI" numeric,"TO" numeric,"PU" numeric,"PR" numeric,
	                  "SL" numeric,"OP" numeric,"AU" numeric,"MS" numeric)
            ),billitemtran as
			(
            select billitems.billid,billitems.id,billitems.qty,billitems.createdon,bill.billtype from billitems 
            inner join bill on bill.id=billid
            where salablegoodsid = 57478 
            and billitems.isactive=true
            and bill.isactive=true and bill.billtype != 9
            and billitems.id =60423
            order by billitems.id
            )
             select mstock.*,sttransaction.*,products.name productname,
	         referencelist.name locationname from mstock
			 inner join sttransaction on sttransaction.salablegoodsid = mstock.salablegoodsid
	         inner join stock on stock.id = stockid
	         inner join products on products.id = productid
	         inner join referencelist on referencelist.id = mstock.locationid
			 */

            string query = @" with tstock
             as
             (
             select sum(qty) stk,salablegoodsid,locationid from stocktransaction where salablegoodsid>0
             group by salablegoodsid,locationid
             ),mstock as
             (
             select barcode,salablegoods.id salablegoodsid,salablegoods.stockid,
                 salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) systemstk,
                 coalesce(stk,0) transactionstk,salablegoods.locationid,
                 salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) - coalesce(stk,0) diffstk
                 from salablegoods 
             left outer join tstock on tstock.salablegoodsid =salablegoods.id and tstock.locationid =salablegoods.locationid
             where salablegoods.isactive=true and
             salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) != coalesce(stk,0)	
             )
             select mstock.*,products.name productname,
	         referencelist.name locationname from mstock
	         inner join stock on stock.id = stockid
	         inner join products on products.id = productid
	         inner join referencelist on referencelist.id = mstock.locationid";

            if (locationid > 0) query += " where mstock.locationid = " + locationid;
            List<StockTransactionMissmatches> result = new List<StockTransactionMissmatches>();
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    StockTransactionMissmatches stockTransactionMissmatches = new StockTransactionMissmatches();
                    stockTransactionMissmatches.stockid = Convert.ToInt64(reader["stockid"]);
                    stockTransactionMissmatches.salablegoodsid = Convert.ToInt64(reader["salablegoodsid"]);
                    stockTransactionMissmatches.barcode = Convert.ToString(reader["barcode"]);
                    stockTransactionMissmatches.productname = Convert.ToString(reader["productname"]);
                    stockTransactionMissmatches.locationname = Convert.ToString(reader["locationname"]);
                    stockTransactionMissmatches.systemstk = Convert.ToDecimal(reader["systemstk"]);
                    stockTransactionMissmatches.transactionstk = Convert.ToDecimal(reader["transactionstk"]);
                    stockTransactionMissmatches.diffstk = Convert.ToDecimal(reader["diffstk"]);
                    result.Add(stockTransactionMissmatches);
                }
            }
            return result;
        }

        public Stocktransaction Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Stocktransaction oStocktransaction)
        {
            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStocktransaction.createdby;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStocktransaction.createdon;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStocktransaction.salablegoodsid;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStocktransaction.stockid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStocktransaction.locationid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStocktransaction.qty;
            command.Parameters.Add("transactedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oStocktransaction.transactedon;
            command.Parameters.Add("reftype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStocktransaction.reftype;
            command.Parameters.Add("refid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStocktransaction.refid;
            command.Parameters.Add("prefid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStocktransaction.refid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oStocktransaction.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oStocktransaction;
        }


        public bool DeleteAll(
            long prefid,
            Stocktransaction.TransactionType type,
            NpgsqlTransaction transaction,
            NpgsqlConnection conn)
        {
            NpgsqlCommand command = new NpgsqlCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("prefid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = prefid;
            command.Parameters.Add("reftype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = Stocktransaction.GetType(type);
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        public bool Delete(
    long refid,
    Stocktransaction.TransactionType type,
    NpgsqlTransaction transaction,
    NpgsqlConnection conn)
        {
            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("refid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = refid;
            command.Parameters.Add("reftype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = Stocktransaction.GetType(type);
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        internal void CreateBarcode(List<Lrinvoiceitemsbarcode> items,
            NpgsqlTransaction transaction, NpgsqlConnection conn)
        {

            /* Missing PU records on stock transation
             * 
             * 
             * 
             insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
             salablegoodsid,stockid,createdby,reftype,createdon)
             with tstock
             as
             (
                select sum(qty) stk,salablegoodsid from stocktransaction where salablegoodsid>0
                group by salablegoodsid
             ),mstock as
             (
                select barcode,salablegoods.id,salablegoods.stockid,iqty,oqty,salablegoods.barcode ibarcode,
                salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) autostk,
                coalesce(stk,0) stk,salablegoods.locationid,
                salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) - coalesce(stk,0) diffstk
                from salablegoods 
                left outer join tstock on tstock.salablegoodsid =salablegoods.id
                where salablegoods.isactive=true and
                salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) != coalesce(stk,0)	
             )
            select 
            lrinvoiceid,lrinvoiceitems.id,lrinvoice.entrydate,
            mstock.iqty qty,mstock.locationid,mstock.id,stock.id,
            stock.modifiedby,'PU',stock.modifiedon
            from mstock
            INNER JOIN STOCK ON STOCK.ID = mstock.STOCKID
            INNER JOIN LRINVOICEITEMS ON LRINVOICEITEMS.ID = STOCK.INVOICEITEMID
            inner join lrinvoice on lrinvoice.id = lrinvoiceid
            left outer join stocktransaction on stocktransaction.salablegoodsid=mstock.id
            and stocktransaction.reftype = 'PU'
            WHERE stocktransaction.salablegoodsid IS NULL
            */

            if (items.Exists(x => x.transaction != null && x.transaction.Exists(y => y.salablegoods != null &&
                 y.salablegoods.pqty > 0)))
            {
                string query = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,lrinvoice.entrydate,
                cast(schemelist->>'Item2' as numeric) qty,salablegoods.locationid,salablegoods.id,stock.id,
                stock.modifiedby,'PU',stock.modifiedon from lrinvoiceitems
                inner join lrinvoice on lrinvoice.id = lrinvoiceid
                inner join stock on stock.invoiceitemid = lrinvoiceitems.id
                inner join salablegoods on salablegoods.stockid = stock.id 
                inner join json_array_elements('{0}') schemelist ON 
                cast(schemelist->>'Item1' as bigint) = salablegoods.id";
                List<Tuple<long, decimal>> stockids = new List<Tuple<long, decimal>>();
                items.ForEach(x =>
                {
                    if (x.transaction != null)
                        stockids.AddRange(x.transaction.FindAll(y => y.salablegoods != null).ConvertAll(z => new Tuple<long, decimal>(z.salablegoods.id, z.qty)));
                });
                query = String.Format(query, JsonConvert.SerializeObject(stockids));

                var rquery = @"
                select lrinvoiceid,lrinvoiceitems.id,lrinvoice.entrydate,
                cast(schemelist->>'Item2' as numeric) qty,salablegoods.locationid,salablegoods.id,stock.id,
                stock.modifiedby,'PU',stock.modifiedon from lrinvoiceitems
                inner join lrinvoice on lrinvoice.id = lrinvoiceid
                inner join stock on stock.invoiceitemid = lrinvoiceitems.id
                inner join salablegoods on salablegoods.stockid = stock.id 
                inner join json_array_elements('{0}') schemelist ON 
                cast(schemelist->>'Item1' as bigint) = salablegoods.id";
                //var dtable = ExecuteDataSet(String.Format(rquery, JsonConvert.SerializeObject(stockids)), null, conn, transaction);

                var rows = ExecuteQuery(query, null, conn, transaction);
            }
            else
            {
                string query = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,lrinvoice.entrydate,
                cast(schemelist->>'Item2' as numeric) qty,stock.locationid,
                0 salablegoodsid,stock.id stockid,
                stock.modifiedby,'PU',stock.modifiedon from lrinvoiceitems
                inner join lrinvoice on lrinvoice.id = lrinvoiceid
                inner join stock on stock.invoiceitemid = lrinvoiceitems.id
                inner join json_array_elements('{0}') schemelist ON 
                cast(schemelist->>'Item1' as bigint) = stock.id";
                List<Tuple<long, decimal>> stockids = new List<Tuple<long, decimal>>();
                items.ForEach(x =>
                {
                    if (x.transaction != null)
                        stockids.AddRange(x.transaction.FindAll(y => y.stock != null).ConvertAll(z => new Tuple<long, decimal>(z.stock.id, z.qty)));
                });
                query = String.Format(query, JsonConvert.SerializeObject(stockids));
                var rows = ExecuteQuery(query, null, conn, transaction);
            }
        }

        internal void DeleteInvoiceItemBarcode(List<long> invoiceitemids, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            string query = "delete from stocktransaction where reftype='PU' and refid in (" + string.Join(",", invoiceitemids) + ")";
            ExecuteQuery(query, null, conn, transaction);
        }

        internal void IncreaseStock(Salablegoods salablegoods)
        {
            using (var connection = CreateConnection())
            {
                connection.Open();
                var transaction = connection.BeginTransaction();
                List<MissingStock> items = new List<MissingStock>();
                items.Add(new MissingStock()
                {
                    salablegoodsid = salablegoods.id,
                    qty = salablegoods.iqty
                });
                SaveMissingStock(items, transaction, connection);
                transaction.Commit();
                connection.Close();
            }
        }
        internal void IncreaseStock(Salablegoods salablegoods, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            List<MissingStock> items = new List<MissingStock>();
            items.Add(new MissingStock()
            {
                salablegoodsid = salablegoods.id,
                qty = salablegoods.iqty
            });
            SaveMissingStock(items, transaction, conn);
        }

        internal void AutoMissingStock(StockWithTax barcodestock,
            NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            List<MissingStock> items = new List<MissingStock>();
            items.Add(new MissingStock()
            {
                salablegoodsid = barcodestock.salablegoodsid,
                qty = barcodestock.qty
            });
            SaveMissingStock(items, transaction, conn);
        }
        internal void SaveMissingStock(List<MissingStock> items, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            if (items.Exists(x => x.salablegoodsid > 0))
            {
                string query = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select 0 prefid,0 refid,current_date,
                cast(schemelist->>'Item2' as numeric) qty,salablegoods.locationid,
                salablegoods.id,stockid,salablegoods.modifiedby,'MS',salablegoods.modifiedon from salablegoods 
                inner join json_array_elements('{0}') schemelist ON 
                cast(schemelist->>'Item1' as bigint) = salablegoods.id";

                List<Tuple<long, decimal>> stockids = new List<Tuple<long, decimal>>();
                stockids.AddRange(items.ConvertAll(x => new Tuple<long, decimal>(x.salablegoodsid, x.qty)));
                query = String.Format(query, JsonConvert.SerializeObject(stockids));
                ExecuteQuery(query, null, conn, transaction);
            }
            else
            {
                string query = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select 0 prefid,0 refid,current_date,
                cast(schemelist->>'Item2' as numeric) qty,stock.locationid,
                0 salablegoodsid,stock.id,stock.modifiedby,'MS',stock.modifiedon from stock
                inner join json_array_elements('{0}') schemelist ON 
                cast(schemelist->>'Item1' as bigint) = stock.id";

                List<Tuple<long, decimal>> stockids = new List<Tuple<long, decimal>>();
                stockids.AddRange(items.ConvertAll(x => new Tuple<long, decimal>(x.stockid, x.qty)));
                query = String.Format(query, JsonConvert.SerializeObject(stockids));
                ExecuteQuery(query, null, conn, transaction);
            }
        }

        internal void InwardFromWarehouse(Bundles oBundles, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {

            string salablegoodstransferin = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric) qty,bundles.tolocationid,
                (schemelist ->> 'toid') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TI' reftype,bundles.modifiedon 
				from bundles
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.id=" + oBundles.id;

            string stocktransferout = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric)*-1 qty,bundles.fromlocationid,
                0 salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TO' reftype,bundles.modifiedon 
				from bundles
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.id=" + oBundles.id;

            ExecuteQuery(stocktransferout, null, conn, transaction);
            ExecuteQuery(salablegoodstransferin, null, conn, transaction);
        }



        public void SwapGoods(Bundles oBundles,
            NpgsqlTransaction transaction, NpgsqlConnection conn)
        {

            string salablegoodstransferin = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric) qty,bundles.tolocationid,
                (schemelist ->> 'toid') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TI' reftype,bundles.modifiedon 
				from bundles
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.id=" + oBundles.id;

            string salablegoodstransferout = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric)*-1 qty,bundles.fromlocationid,
                (schemelist ->> 'id') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TO' reftype,bundles.modifiedon 
				from bundles
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.id=" + oBundles.id;

            ExecuteQuery(salablegoodstransferout, null, conn, transaction);
            ExecuteQuery(salablegoodstransferin, null, conn, transaction);
        }

        internal void InwardGoods2WareHouse(BundlesCreteria oBundles, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            string salablegoodstransferout = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric)*-1 qty,bundles.fromlocationid,
                (schemelist ->> 'id') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TO' reftype,bundles.modifiedon 
				from bundles
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                inner join salablegoods on salablegoods.id = (schemelist ->> 'id') :: BIGINT
				inner join stock ON stock.id = salablegoods.stockid
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.id=" + oBundles.id;

            string stocktransferin = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric) qty,bundles.tolocationid,
                0 salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TI' reftype,bundles.modifiedon 
				from bundles
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.id=" + oBundles.id;

            ExecuteQuery(stocktransferin, null, conn, transaction);
            ExecuteQuery(salablegoodstransferout, null, conn, transaction);
        }

        internal void InwardAllPendingToRetail(List<BundlesDataService.PendingStockItem> items, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            string stocktransferout = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select 0 prefid,0 refid,current_date,
                cast(schemelist->>'Item2' as numeric)*-1 qty,stock.locationid,
                0 salablegoodsid,stock.id,stock.modifiedby,'TO',stock.modifiedon from stock
                inner join json_array_elements('{0}') schemelist ON 
                cast(schemelist->>'Item1' as bigint) = stock.id";
            List<Tuple<long, decimal>> stockids = new List<Tuple<long, decimal>>();
            stockids.AddRange(items.ConvertAll(x => new Tuple<long, decimal>(x.stockid, x.iqty)));

            stocktransferout = String.Format(stocktransferout, JsonConvert.SerializeObject(stockids));
            ExecuteQuery(stocktransferout, null, conn, transaction);

            string salablegoodstransferin = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select 0 prefid,0 refid,current_date,
                cast(schemelist->>'Item2' as numeric) qty,salablegooeds.locationid,
                salablegoods.id salablegoodsid,salablegoods.stockid,salablegoods.modifiedby,'TI',
                salablegoods.modifiedon from salablegoods
                inner join json_array_elements('{0}') schemelist ON 
                cast(schemelist->>'Item1' as bigint) = salablegoods.id";
            stockids = new List<Tuple<long, decimal>>();
            stockids.AddRange(items.ConvertAll(x => new Tuple<long, decimal>(x.salablegoodsid, x.iqty)));

            salablegoodstransferin = String.Format(salablegoodstransferin, JsonConvert.SerializeObject(stockids));
            ExecuteQuery(salablegoodstransferin, null, conn, transaction);
        }

        internal void RejectGoods(Bundles oBundles, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {

            string salablegoodstransferin = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric)*-1 qty,bundles.tolocationid,
                (schemelist ->> 'toid') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TO' reftype,bundles.modifiedon 
				from bundles
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.id=" + oBundles.id;

            string salablegoodstransferout = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric) qty,bundles.fromlocationid,
                (schemelist ->> 'id') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TI' reftype,bundles.modifiedon 
				from bundles
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.id=" + oBundles.id;

            ExecuteQuery(salablegoodstransferout, null, conn, transaction);
            ExecuteQuery(salablegoodstransferin, null, conn, transaction);
        }

        internal void InsertBillItems(Bill oBill, NpgsqlTransaction transaction,
            NpgsqlConnection conn)
        {
            string billitemsout = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                
                select bill.id,billitems.id,bill.billdate,
                billitems.qty*-1,bill.locationid,
                billitems.salablegoodsid,billitems. stockid,
                billitems.modifiedby,'SL' reftype,billitems.modifiedon 
				from billitems
				inner join bill on bill.id = billid
				inner join stock ON stock.id = billitems.stockid
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where billid = " + oBill.id + " and bill.billtype != 9 and bill.isactive=true and billitems.isactive=true";

            ExecuteQuery(billitemsout, null, conn, transaction);
        }

        internal void DeleteBillItems(Bill OBILL,
            NpgsqlTransaction transaction,
            NpgsqlConnection conn,
            bool removesourcebillitem)
        {

            string billitemsout = @"
                delete from stocktransaction where reftype ='SL' and 
				prefid = " + OBILL.id;
            ExecuteQuery(billitemsout, null, conn, transaction);

            if (removesourcebillitem)
            {
                billitemsout = string.Format(@"insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                            salablegoodsid,stockid,createdby,reftype,createdon)

                            select bill.id,billitems.id,bill.billdate,
                            billitems.qty*-1,bill.locationid,
                            billitems.salablegoodsid,billitems. stockid,
                            billitems.modifiedby,'SL' reftype,billitems.modifiedon 
                            from billitems
                            inner join bill on bill.id = billid
                            inner join stock ON stock.id = billitems.stockid
                            inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                            where billitems.id in ( select billitems.returnbillitemid from bill
                            inner join billitems on billid = bill.id and billitems.isactive=true
                            where bill.id = {0} and billitems.returnbillitemid>0 and billitems.qty<0)  
                            and bill.billtype != 9 
							and bill.isactive=true and billitems.isactive=true", OBILL.id);

                ExecuteQuery(billitemsout, null, conn, transaction);
            }
        }
        internal void UpdateBillItems(Bill OBILL,
            NpgsqlTransaction transaction,
            NpgsqlConnection conn)
        {

            DeleteBillItems(OBILL, transaction, conn,false);
            InsertBillItems(OBILL, transaction, conn);
        }

        internal void SyncStock(List<Stockhistory> stockhistories, NpgsqlTransaction transaction,
            NpgsqlConnection conn)
        {
            if (stockhistories == null || stockhistories.Count == 0) return;
            string stockaudit = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)

                select lrinvoiceid,lrinvoiceitems.id,Stockhistory.modifiedon,
                Stockhistory.auditqty,stock.locationid,
                0 salablegoodsid,Stockhistory.stockid,
                Stockhistory.modifiedby,'AU' reftype,Stockhistory.modifiedon 
				from Stockhistory
				inner join stock ON stock.id = Stockhistory.stockid
				inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
				where Stockhistory.id in (" + String.Join(",", stockhistories.ConvertAll(x => x.id)) + ")";

            ExecuteQuery(stockaudit, null, conn, transaction);
        }

        internal void SyncStock(List<Salablegoodshistory> salablegoodshistory, NpgsqlTransaction transaction,
            NpgsqlConnection conn)
        {
            if (salablegoodshistory == null || salablegoodshistory.Count == 0) return;
            string stockaudit = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)

                select lrinvoiceid,lrinvoiceitems.id,Salablegoodshistory.modifiedon,
                Salablegoodshistory.auditqty,salablegoods.locationid,
                Salablegoodshistory.salablegoodsid,salablegoods.stockid,
                Salablegoodshistory.modifiedby,'AU' reftype,Salablegoodshistory.modifiedon 
				from Salablegoodshistory
				inner join salablegoods ON salablegoods.id = Salablegoodshistory.salablegoodsid
				inner join stock on stock.id = salablegoods.stockid
				inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
				where Salablegoodshistory.id in (" + String.Join(",", salablegoodshistory.ConvertAll(x => x.id)) + ")";

            ExecuteQuery(stockaudit, null, conn, transaction);
        }

        internal void PurchaseReturn(Lrinvoicereturn oLrinvoicereturn, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            if (oLrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.DebitNote)
            {
                string query = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)

                select lrinvoicereturnid,lrinvoicereturnitems.id,lrinvoicereturnitems.modifiedon,
                lrinvoicereturnitems.qty*-1,salablegoods.locationid,
                salablegoods.id,salablegoods.stockid,
                lrinvoicereturnitems.modifiedby,'PR' reftype,lrinvoicereturnitems.modifiedon 
				from lrinvoicereturnitems
				inner join lrinvoicereturn on lrinvoicereturn.id = lrinvoicereturnid and lrinvoicereturn.returntype<10
				inner join stock on stock.id = lrinvoicereturnitems.stockid
				inner join salablegoods ON salablegoods.id = lrinvoicereturnitems.salablegoodsid
				inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
				where lrinvoicereturnitems.lrinvoicereturnid = " + oLrinvoicereturn.id + " and lrinvoicereturnitems.isactive=true";
                ExecuteQuery(query, null, conn, transaction);
            }
            else if (oLrinvoicereturn.ReturnType == Lrinvoicereturn.ReturnTypes.ExcessCreditNote)
            {
                string query = @"
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)

                select lrinvoicereturnid,lrinvoicereturnitems.id,lrinvoicereturnitems.modifiedon,
                lrinvoicereturnitems.qty,salablegoods.locationid,
                salablegoods.id,salablegoods.stockid,
                lrinvoicereturnitems.modifiedby,'PR' reftype,lrinvoicereturnitems.modifiedon 
				from lrinvoicereturnitems
				inner join lrinvoicereturn on lrinvoicereturn.id = lrinvoicereturnid and lrinvoicereturn.returntype<10
				inner join stock on stock.id = lrinvoicereturnitems.stockid
				inner join salablegoods ON salablegoods.id = lrinvoicereturnitems.salablegoodsid
				inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
				where lrinvoicereturnitems.lrinvoicereturnid = " + oLrinvoicereturn.id + " and lrinvoicereturnitems.isactive=true";
                ExecuteQuery(query, null, conn, transaction);
            }
        }

        internal void PurchaseReturnDelete(long id, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            string query = "Delete from stocktransaction where reftype='PR' and prefid=" + id;
            ExecuteQuery(query, null, conn, transaction);
        }
        internal void PurchaseReturnUpdate(Lrinvoicereturn oLrinvoicereturn, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            PurchaseReturnDelete(oLrinvoicereturn.id, transaction, conn);
            PurchaseReturn(oLrinvoicereturn, transaction, conn);
        }
        internal ItemSearchResult StockHistory(ItemSearchCreteria item)
        {
            throw new NotImplementedException();
        }

        /*with billqty
        as
        (
        select SUM(case when billtype=8 then billitems.qty else 0 end) returnqty,
        SUM(case when billtype=0 then billitems.qty
         when billtype=1 and bill.customerid>0 and customer.gstno='' then billitems.qty
         else 0 end) retailqty,
        SUM(case when billtype=1 and bill.customerid>0 and customer.gstno!='' then billitems.qty else 0 end) b2bqty,
	        billdate::date from bill
        inner join billitems on billitems.billid = bill.id
        left outer join customer on customer.id =bill.customerid
        where bill.billtype!=9 and bill.isactive=true and billitems.isactive=true
        and billdate>='2024-08-19'
        group by billdate::date
        ),jobworkissues
        as
        (
        select sum(Jobworkbillitems.oqty) JISSUE,jobworkbill.entrydate::date from Jobworkbillitems
        inner join Jobworkbill on Jobworkbill.id = Jobworkbillid
        where jobworkbill.isactive=true 
        and jobworkbillitems.isactive=true
        and entrydate>='2024-08-19'
        group by jobworkbill.entrydate::date
        ),jobworkbilled
        as
        (
        select sum(jobworkbillgrnitems.qty) JRETURN,jobworkbillgrn.entrydate::date from jobworkbillgrnitems
        inner join jobworkbillgrn on jobworkbillgrn.id = jobworkbillgrnid
        where jobworkbillgrn.isactive=true 
        and jobworkbillgrnitems.isactive=true
        and entrydate>='2024-08-19'
        group by jobworkbillgrn.entrydate::date
        ),appissue as
        (
        select billdate::date,
        sum(case when billtype=10 then bulkbillitems.qty else 0 end) APPISSUED,
        sum(case when billtype=11 then bulkbillitems.qty else 0 end) APPBILLED,
        --	sum(case when billtype=13 then bulkbillitems.qty else 0 end) BILLED1,
        sum(case when billtype=12 then bulkbillitems.qty else 0 end) APPRETURNQTY,	
        sum(case when billtype=14 then bulkbillitems.qty else 0 end) APPBILLEDANDRETURN
	        from bulkbillitems
        inner join bulkbill on bulkbill.id = bulkbillitems.billid
        where bulkbill.isactive=true 
        and bulkbillitems.isactive=true
        and billdate>='2024-08-19'
        group by billdate::date
        ORDER BY billdate::date
        ),PURCHASE AS
        (
          SELECT LRENTRY.LRENTRYDATE::DATE,SUM(LRINVOICEITEMS.BUYINGQTY) PURQTY FROM LRINVOICEITEMS
          INNER JOIN LRINVOICE ON LRINVOICE.ID = LRINVOICEID
          INNER JOIN LRENTRY ON LRENTRY.ID = LRID
          WHERE LRENTRY.LRENTRYDATE>='2024-08-19'
	        AND LRINVOICEITEMS.ISACTIVE=TRUE AND LRINVOICE.ISACTIVE=TRUE AND LRENTRY.ISACTIVE=TRUE
	        GROUP BY LRENTRY.LRENTRYDATE::DATE
        ),PRETURN AS 
        (
        SELECT LRINVOICERETURN.RETURNDATE::DATE,SUM(LRINVOICERETURNITEMS.QTY) PRQTY FROM LRINVOICERETURNITEMS
	        INNER JOIN LRINVOICERETURN ON LRINVOICERETURN.ID = LRINVOICERETURNID
        WHERE LRINVOICERETURN.RETURNDATE>='2024-08-19'
	        AND LRINVOICERETURNITEMS.ISACTIVE=TRUE AND LRINVOICERETURN.ISACTIVE=TRUE 
	        GROUP BY LRINVOICERETURN.RETURNDATE::DATE
        )


        SELECT billqty.BILLDATE,billqty.returnqty salereturn,
        billqty.retailqty,billqty.b2bqty,
        PURCHASE.purqty purchaseqty,PRETURN.prqty purchasereturn,
        jobworkissues.jissue jobworkissued,jobworkbilled.jreturn jobworkreturn,
        appissue.appissued,appbilled,appreturnqty FROM billqty
        LEFT OUTER JOIN PURCHASE ON PURCHASE.LRENTRYDATE = billqty.BILLDATE
        LEFT OUTER JOIN jobworkissues ON jobworkissues.ENTRYDATE= billqty.BILLDATE
        LEFT OUTER JOIN jobworkbilled ON jobworkbilled.ENTRYDATE= billqty.BILLDATE
        LEFT OUTER JOIN appissue ON appissue.BILLDATE = billqty.BILLDATE
        LEFT OUTER JOIN PRETURN ON PRETURN.RETURNDATE =  billqty.BILLDATE
        ORDER BY billqty.BILLDATE*/

        int Percentage(int iteration, int total)
        {
            return Convert.ToInt32(Convert.ToDouble(iteration) / Convert.ToDouble(total) * 100.0);
        }
        internal void Configure(ProcessManager.ProcessData data)
        {

            int missmatchcount = 0;
            data.progress.hasstarted = true;
            data.progress.statuslist = new List<string>();
            System.Threading.Thread.Sleep(1000);
            try
            {
                using (var reader = ExecuteReader(@"
            select count(*) from salablegoods
            where pqty+tiqty-toqty-rqty != iqty", new SortedList<string, object>()))
                {
                    if (reader.Read())
                    {
                        missmatchcount = Convert.ToInt32(reader[0]);
                    }
                }

                if (missmatchcount == 0)
                {
                    using (var reader = ExecuteReader(@"
                    with tstock
                        as
                        (
                        select sum(qty) stk,salablegoodsid from stocktransaction where salablegoodsid>0
                        group by salablegoodsid
                        ),mstock as
                        (
                        select barcode,salablegoods.id,
	                        salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) autostk,
	                        coalesce(stk,0) stk,
	                        salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) - coalesce(stk,0) diffstk
	                        from salablegoods 
                        left outer join tstock on tstock.salablegoodsid =salablegoods.id
                        where --hasstock=true and 
                        salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) != coalesce(stk,0)	
                        )
                        select count(*) from mstock", new SortedList<string, object>()))
                    {
                        if (reader.Read())
                        {
                            missmatchcount = Convert.ToInt32(reader[0]);
                        }
                    }
                }

                if (missmatchcount == 0)
                {
                    data.progress.status = "Already Sync with realtime stock and transactional stock";
                    data.progress.percentage = 100;
                    data.progress.hascompleted = true;
                    return;
                }

                data.progress.AddStatus = "1/20 Validated Stock Missmatch - " + missmatchcount;
                System.Threading.Thread.Sleep(1000);
                if (missmatchcount > 0)
                {
                    ExecuteQuery(@"update salablegoods set tiqty = tiqty+(iqty-(pqty+tiqty-toqty-rqty))
                where pqty+tiqty-toqty-rqty <iqty;", new SortedList<string, object>());

                    ExecuteQuery(@"update salablegoods set toqty = toqty+(pqty+tiqty-toqty-rqty)-iqty
                where pqty+tiqty-toqty-rqty >iqty;", new SortedList<string, object>());
                }

                data.progress.percentage = Percentage(1, 20);
                System.Threading.Thread.Sleep(1000);
                data.progress.AddStatus = "2/20 Stock Transaction Cleanedup";
                ExecuteQuery(@"delete from stocktransaction;", new SortedList<string, object>());
                data.progress.percentage = 2 / 20;
                System.Threading.Thread.Sleep(1000);
                data.progress.AddStatus = "3/20 Inward from Warehouse";
                string query = @"-- inward from warehouse
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric) qty,bundles.tolocationid,
                (schemelist ->> 'toid') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TI' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist on referencelist.id = fromlocationid and cast(referencelist.additionalinfo->>'iswarehouse' as bool) = true
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;
				
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric)*-1 qty,bundles.fromlocationid,
                0 salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TO' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist on referencelist.id = fromlocationid and cast(referencelist.additionalinfo->>'iswarehouse' as bool) = true
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;";

                ExecuteQuery(query, null);
                data.progress.percentage = Percentage(3, 20);
                System.Threading.Thread.Sleep(1000);
                data.progress.AddStatus = "4/20 Swap between retail";
                query = @"-- swap goods
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric) qty,bundles.tolocationid,
                (schemelist ->> 'toid') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TI' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist fromref on fromref.id = fromlocationid and cast(fromref.additionalinfo->>'iswarehouse' as bool) = false
				inner join referencelist toref on toref.id = tolocationid and cast(toref.additionalinfo->>'iswarehouse' as bool) = false
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;


                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric)*-1 qty,bundles.fromlocationid,
                (schemelist ->> 'id') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TO' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist fromref on fromref.id = fromlocationid and cast(fromref.additionalinfo->>'iswarehouse' as bool) = false
				inner join referencelist toref on toref.id = tolocationid and cast(toref.additionalinfo->>'iswarehouse' as bool) = false
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;";

                ExecuteQuery(query, null);
                data.progress.percentage = Percentage(4, 20);
                System.Threading.Thread.Sleep(1000);
                data.progress.AddStatus = "5/20 Inward to Warehouse";

                query = @"--inward to warehouse

                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric)*-1 qty,bundles.fromlocationid,
                (schemelist ->> 'id') :: BIGINT salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TO' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist fromref on fromref.id = fromlocationid and cast(fromref.additionalinfo->>'iswarehouse' as bool) = false
				inner join referencelist toref on toref.id = tolocationid and cast(toref.additionalinfo->>'iswarehouse' as bool) = true
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
                inner join salablegoods on salablegoods.id = (schemelist ->> 'id') :: BIGINT
				inner join stock ON stock.id = salablegoods.stockid
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;

				insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoiceid,lrinvoiceitems.id,bundles.receivedon,
                cast(schemelist->>'aqty' as numeric) qty,bundles.tolocationid,
                0 salablegoodsid,stock.id stockid,
                bundles.modifiedby,'TI' reftype,bundles.modifiedon 
				from bundles
				inner join referencelist fromref on fromref.id = fromlocationid and cast(fromref.additionalinfo->>'iswarehouse' as bool) = false
				inner join referencelist toref on toref.id = tolocationid and cast(toref.additionalinfo->>'iswarehouse' as bool) = true
				LEFT JOIN LATERAL json_array_elements (case when bundles.items is null or bundles.items::text ='null' then '[]'::json else bundles.items :: json end) schemelist ON true
				inner join stock ON stock.id = (schemelist ->> 'stockid') :: BIGINT
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
				where bundles.status in (1,2) and bundles.isactive=true;";

                ExecuteQuery(query, null);
                data.progress.percentage = Percentage(5, 20);
                System.Threading.Thread.Sleep(1000);
                data.progress.AddStatus = "6/20 Purchase to warehouse";
                query = @"
                    insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
                    select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,stock.purchaseqty,
                                stock.locationid,0,stock.id,stock.createdby,
                                'PU' reftype,stock.createdon
                    from stock
                    left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
                    left outer join lrinvoice on lrinvoice.id =lrinvoiceid
                    left outer join lrentry on lrentry.id = lrid
					where stock.isactive=true and stock.purchaseqty>0 and lrentry.retaillocationid=0;
					
                    insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
                    select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,stock.transfered*-1,
                                stock.locationid,0,stock.id,stock.createdby,
                                'TO' reftype,stock.createdon
                    from stock
                    left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
                    left outer join lrinvoice on lrinvoice.id =lrinvoiceid
                    left outer join lrentry on lrentry.id = lrid
					where stock.isactive=true and stock.purchaseqty>0 and lrentry.retaillocationid=0;

                    insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
                    select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,salablegoods.pqty,
                                salablegoods.locationid,0,stock.id,salablegoods.createdby,
                                'PU' reftype,salablegoods.createdon
                    from salablegoods
					inner join stock on stock.id = stockid
                    left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
                    left outer join lrinvoice on lrinvoice.id =lrinvoiceid
                    left outer join lrentry on lrentry.id = lrid
					where stock.isactive=true and salablegoods.pqty>0;";

                ExecuteQuery(query, null);
                data.progress.percentage = Percentage(6, 20);
                System.Threading.Thread.Sleep(1000);

                data.progress.AddStatus = "7/20 Purchase to warehouse";
                query = @"--direct purchase missing
                    with tstock
                    as
                    (
                    select sum(qty) stk,salablegoodsid from stocktransaction where salablegoodsid>0
                    group by salablegoodsid
                    ),mstock as
                    (
                    select barcode,salablegoods.id,
	                    salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) autostk,
	                    coalesce(stk,0) stk,
	                    salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) - coalesce(stk,0) diffstk
	                    from salablegoods 
                    left outer join tstock on tstock.salablegoodsid =salablegoods.id
                    where hasstock=true and 
                    salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) != coalesce(stk,0)	
                    )
                    insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
                    select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,diffstk,
                                salablegoods.locationid,salablegoods.id,salablegoods.stockid,stock.createdby,
                                'PU' reftype,stock.createdon
                    from salablegoods
                    inner join mstock on mstock.id = salablegoods.id
                    inner join stock on stock.id = stockid
                    left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
                    left outer join lrinvoice on lrinvoice.id =lrinvoiceid
                    left outer join lrentry on lrentry.id = lrid
                    where lrentry.retaillocationid>0 and  diffstk>0;";

                ExecuteQuery(query, null);
                data.progress.percentage = Percentage(7, 20);
                System.Threading.Thread.Sleep(1000);

                data.progress.AddStatus = "8/20 Direct purchase missing";
                query = @"--direct purchase missing
                    with tstock
                    as
                    (
                    select sum(qty) stk,salablegoodsid from stocktransaction where salablegoodsid>0
                    group by salablegoodsid
                    ),mstock as
                    (
                    select barcode,salablegoods.id,
	                    salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) autostk,
	                    coalesce(stk,0) stk,
	                    salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) - coalesce(stk,0) diffstk
	                    from salablegoods 
                    left outer join tstock on tstock.salablegoodsid =salablegoods.id
                    where hasstock=true and 
                    salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) != coalesce(stk,0)	
                    )
                    insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
                    select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,diffstk,
                                salablegoods.locationid,salablegoods.id,salablegoods.stockid,stock.createdby,
                                'PU' reftype,stock.createdon
                    from salablegoods
                    inner join mstock on mstock.id = salablegoods.id
                    inner join stock on stock.id = stockid
                    left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
                    left outer join lrinvoice on lrinvoice.id =lrinvoiceid
                    left outer join lrentry on lrentry.id = lrid
                    where lrentry.retaillocationid>0 and  diffstk>0;";

                ExecuteQuery(query, null);
                data.progress.percentage = Percentage(8, 20);
                System.Threading.Thread.Sleep(1000);
                data.progress.AddStatus = "9/20 Sales";
                query = @"--Sales
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)

                select bill.id,billitems.id,bill.billdate,
                billitems.qty*-1,bill.locationid,
                billitems.salablegoodsid,billitems. stockid,
                billitems.modifiedby,'SL' reftype,billitems.modifiedon 
				from billitems
				inner join bill on bill.id = billid
				inner join stock ON stock.id = billitems.stockid
				inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                where bill.isactive=true and bill.billtype!=9 and billitems.isactive=true;";

                ExecuteQuery(query, null);
                data.progress.percentage = Percentage(9, 20);
                System.Threading.Thread.Sleep(1000);
                data.progress.AddStatus = "10/20 Purchase return";

                query = @"---purchase return
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                salablegoodsid,stockid,createdby,reftype,createdon)

                select lrinvoicereturnid,lrinvoicereturnitems.id,lrinvoicereturnitems.modifiedon,
                lrinvoicereturnitems.qty,salablegoods.locationid,
                salablegoods.ID,salablegoods.stockid,
                lrinvoicereturnitems.modifiedby,'PR' reftype,lrinvoicereturnitems.modifiedon 
				from lrinvoicereturnitems
				inner join lrinvoicereturn on lrinvoicereturn.id = lrinvoicereturnid and lrinvoicereturn.returntype<10
				inner join stock on stock.id = lrinvoicereturnitems.stockid
				inner join salablegoods ON salablegoods.id = lrinvoicereturnitems.salablegoodsid
				inner join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
				where lrinvoicereturn.isactive=true and lrinvoicereturnitems.isactive=true;";

                ExecuteQuery(query, null);
                data.progress.percentage = Percentage(10, 20);
                System.Threading.Thread.Sleep(1000);
                data.progress.AddStatus = "11/20 Purchase return";
                query = @"--Inward or purchase from salablegoods
            with ststock
            as(
                select salablegoodsid,sum(qty) cst from stocktransaction
	            where salablegoodsid>0
                group by salablegoodsid
            )

            insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)

            select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,(iqty-(oqty+dqty+jqty))-cst,
            salablegoods.locationid,salablegoods.id,salablegoods.stockid,stock.createdby,
            case when pqty>0 then 'PU' else 'TI' END reftype,stock.createdon from ststock
            inner join salablegoods on salablegoods.id = ststock.salablegoodsid
            inner join stock on stock.id = stockid
            inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
            inner join lrinvoice on lrinvoice.id = lrinvoiceid
            where iqty-(oqty+dqty+jqty) > cst and lrinvoice.entrydate>'2020-01-01'
            and salablegoods.isactive=true and stock.isactive=true and lrinvoiceitems.isactive=true;";

                ExecuteQuery(query, null);
                data.progress.percentage = Percentage(11, 20);
                System.Threading.Thread.Sleep(1000);
                data.progress.AddStatus = "12/20 CREATE PURCHASE FOR PURCHASE RETURN";
                query = @"--CREATE PURCHASE FOR PURCHASE RETURN
            with ststock
            as(
            select salablegoodsid,sum(qty) cst from stocktransaction
	            where salablegoodsid>0
            group by salablegoodsid
            )

            insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
            select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,(iqty-(oqty+dqty+jqty))-cst,
            salablegoods.locationid,salablegoods.id,salablegoods.stockid,stock.createdby,
            'PU' reftype,stock.createdon from ststock
            inner join salablegoods on salablegoods.id = ststock.salablegoodsid
            inner join stock on stock.id = stockid
            inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
            inner join lrinvoice on lrinvoice.id = lrinvoiceid
            where iqty-(oqty+dqty+jqty) > cst and lrinvoice.entrydate>'2020-01-01';";

                ExecuteQuery(query, null);
                data.progress.percentage = Percentage(12, 20);
                System.Threading.Thread.Sleep(1000);
                data.progress.AddStatus = "13/20 STOCK PURCHASE";
                query = @"--stock purchase 
            with ststock
            as(
            select stockid,sum(qty) cst from stocktransaction
	            where salablegoodsid=0
            group by stockid
            ),dpurchase as
            (
	            select sum(pqty), stockid from salablegoods
	            where pqty>0
	            group by stockid
            )

            insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)

            select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)))-cst,
            stock.locationid,0,stock.id,stock.createdby,
            'PU' reftype,stock.createdon
            from ststock
            left outer join dpurchase on dpurchase.stockid = ststock.stockid
            inner join stock on stock.id = ststock.stockid
            left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
            left outer join lrinvoice on lrinvoice.id = lrinvoiceid
            where (stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) > cst 
            and dpurchase.stockid is null and stock.isactive=true and
            lrinvoiceitems.isactive=true and lrinvoice.isactive=true;";

                ExecuteQuery(query, null);
                data.progress.percentage = Percentage(13, 20);
                System.Threading.Thread.Sleep(1000);
                data.progress.AddStatus = "14/20 Stock transfer direct purchase to retail";
                query = @"--Stock transfer direct purchase to retail
                with ststock
                as(
                select stockid,sum(qty) cst from stocktransaction
	                where salablegoodsid=0
                group by stockid
                ),dpurchase as
                (
	                select sum(pqty), stockid from salablegoods
	                where pqty>0
	                group by stockid
                )

                insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)

                select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,((stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)))-cst,
                stock.locationid,0,stock.id,stock.createdby,
                'TO' reftype,stock.createdon
                from ststock
                left outer join dpurchase on dpurchase.stockid = ststock.stockid
                inner join stock on stock.id = ststock.stockid
                left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                left outer join lrinvoice on lrinvoice.id = lrinvoiceid
                where (stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) != cst 
                and dpurchase.stockid is not null  and stock.isactive=true and
                lrinvoiceitems.isactive=true and lrinvoice.isactive=true;";

                ExecuteQuery(query, null);
                data.progress.percentage = Percentage(14, 20);
                System.Threading.Thread.Sleep(1000);
                data.progress.AddStatus = "15/20 missing stock to transfer to stock and then transfer to retail";

                query = @"--missing stock to transfer to stock and then transfer to retail
                with tstock
                as
                (
                select sum(qty) stk,salablegoodsid from stocktransaction where salablegoodsid>0
                group by salablegoodsid
                ),mstock as
                (
                select barcode,salablegoods.id,
	                salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) autostk,
	                coalesce(stk,0) stk,
	                salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) - coalesce(stk,0) diffstk
	                from salablegoods 
                left outer join tstock on tstock.salablegoodsid =salablegoods.id
                where hasstock=true and 
                salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) != coalesce(stk,0)	
                )

                insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,diffstk,
                            stock.locationid,0,salablegoods.stockid,stock.createdby,
                            'PU' reftype,stock.createdon
                from salablegoods
                inner join mstock on mstock.id = salablegoods.id
                inner join stock on stock.id = stockid
                left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
                left outer join lrinvoice on lrinvoice.id =lrinvoiceid
                left outer join lrentry on lrentry.id = lrid
                where diffstk>0;";
                ExecuteQuery(query, null);
                data.progress.percentage = Percentage(15, 20);
                System.Threading.Thread.Sleep(1000);
                data.progress.AddStatus = "16/20 missing stock to transfer to stock and then transfer to retail";

                query = @"--missing stock to transfer to stock and then transfer to retail
                with tstock
                as
                (
                select sum(qty) stk,salablegoodsid from stocktransaction where salablegoodsid>0
                group by salablegoodsid
                ),mstock as
                (
                select barcode,salablegoods.id,
	                salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) autostk,
	                coalesce(stk,0) stk,
	                salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) - coalesce(stk,0) diffstk
	                from salablegoods 
                left outer join tstock on tstock.salablegoodsid =salablegoods.id
                where hasstock=true and 
                salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) != coalesce(stk,0)	
                )

                insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,diffstk*-1,
                            stock.locationid,0,salablegoods.stockid,stock.createdby,
                            'TO' reftype,stock.createdon
                from salablegoods
                inner join mstock on mstock.id = salablegoods.id
                inner join stock on stock.id = stockid
                left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
                left outer join lrinvoice on lrinvoice.id =lrinvoiceid
                left outer join lrentry on lrentry.id = lrid
                where diffstk>0;

                with tstock
                as
                (
                select sum(qty) stk,salablegoodsid from stocktransaction where salablegoodsid>0
                group by salablegoodsid
                ),mstock as
                (
                select barcode,salablegoods.id,
	                salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) autostk,
	                coalesce(stk,0) stk,
	                salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) - coalesce(stk,0) diffstk
	                from salablegoods 
                left outer join tstock on tstock.salablegoodsid =salablegoods.id
                where hasstock=true and 
                salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) != coalesce(stk,0)	
                )
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,diffstk,
                            salablegoods.locationid,salablegoods.id,salablegoods.stockid,stock.createdby,
                            'TI' reftype,stock.createdon
                from salablegoods
                inner join mstock on mstock.id = salablegoods.id
                inner join stock on stock.id = stockid
                left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
                left outer join lrinvoice on lrinvoice.id =lrinvoiceid
                left outer join lrentry on lrentry.id = lrid
                where diffstk>0;

                with tstock
                as
                (
                select sum(qty) stk,stockid from stocktransaction where salablegoodsid=0
                group by stockid
                ),mstock as
                (
                select barcode,stock.id,
	                stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty) autostk,
	                coalesce(stk,0) stk,
	                (stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) - coalesce(stk,0) diffstk
	                from stock 
                left outer join tstock on tstock.stockid =stock.id
                where hasstock=true and 
                (stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) != coalesce(stk,0)	
                )
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,diffstk,
                            stock.locationid,0,stock.id,stock.createdby,
                            'PU' reftype,stock.createdon
                from stock
                inner join mstock on mstock.id = stock.id
                left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
                left outer join lrinvoice on lrinvoice.id =lrinvoiceid
                left outer join lrentry on lrentry.id = lrid
                where diffstk>0;";

                ExecuteQuery(query, null);
                data.progress.percentage = Percentage(16, 20);
                System.Threading.Thread.Sleep(1000);
                data.progress.AddStatus = "17/20 JOURNAL WAREHOUSE";
                query = @"--JOURNAL WAREHOUSE
                        insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                        salablegoodsid,stockid,createdby,reftype,createdon)
				
                        select lrinvoice.id,lrinvoiceitems.id,stockhistory.createdon,auditqty,
                        stock.locationid,0,stockhistory.stockid,
                        stockhistory.createdby,'JN' reftype,stockhistory.createdon
                        from stockhistory
                        inner join stock on stock.id = stockhistory.stockid
                        left outer join lrinvoiceitems on lrinvoiceitems.id =stock.invoiceitemid
                        left outer join lrinvoice on lrinvoice.id =lrinvoiceid
                        left outer join lrentry on lrentry.id = lrid
                        where auditqty!=0;";
                ExecuteQuery(query, null);
                data.progress.percentage = Percentage(17, 20);
                System.Threading.Thread.Sleep(1000);
                data.progress.AddStatus = "18/20 JOURNAL RETAIL";
                query = @"-- JOURNAL RETAIL
                    insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                    salablegoodsid,stockid,createdby,reftype,createdon)
				
                    select 
                    lrinvoice.id,lrinvoiceitems.id,salablegoodshistory.createdon,auditqty,
                    salablegoods.locationid,salablegoods.id,salablegoods.stockid,
                    salablegoodshistory.createdby,'JN' reftype,salablegoodshistory.createdon
                    from salablegoodshistory
                    inner join salablegoods on salablegoods.id = salablegoodshistory.salablegoodsid
                    inner join stock on stock.id = salablegoods.stockid
                    left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
                    left outer join lrinvoice on lrinvoice.id =lrinvoiceid
                    left outer join lrentry on lrentry.id = lrid
                    where auditqty!=0;";

                ExecuteQuery(query, null);
                data.progress.percentage = Percentage(18, 20);
                System.Threading.Thread.Sleep(1000);
                data.progress.AddStatus = "19/20 AUTO JOURNAL - WAREHOUSE";
                query = @"--AUTO JOURNAL - WAREHOUSE
            with tstock
            as
            (
            select sum(qty) stk,stockid from stocktransaction where salablegoodsid=0
            group by stockid
            ),mstock as
            (
            select barcode,stock.id,
	            stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty) autostk,
	            coalesce(stk,0) stk,
	            (stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) - coalesce(stk,0) diffstk
	            from stock 
            left outer join tstock on tstock.stockid =stock.id
            where --hasstock=true and 
            (stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) != coalesce(stk,0)	
            )
            insert into stocktransaction(prefid,refid,transactedon,qty,locationid, salablegoodsid,stockid,createdby,reftype,createdon)
            select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,diffstk,
                        stock.locationid,0,stock.id,stock.createdby,
                        'JN' reftype,stock.createdon
            from stock
            inner join mstock on mstock.id = stock.id
            left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
            left outer join lrinvoice on lrinvoice.id =lrinvoiceid
            left outer join lrentry on lrentry.id = lrid;";

                ExecuteQuery(query, null);
                data.progress.percentage = Percentage(19, 20);
                System.Threading.Thread.Sleep(1000);
                data.progress.AddStatus = "20/20 AUTO JOURNAL - RETAIL";
                query = @"--AUTO JOURNAL - RETAIL
                with tstock
                as
                (
                select sum(qty) stk,salablegoodsid from stocktransaction where salablegoodsid>0
                group by salablegoodsid
                ),mstock as
                (
                select barcode,salablegoods.id,
	                salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) autostk,
	                coalesce(stk,0) stk,
	                salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) - coalesce(stk,0) diffstk
	                from salablegoods 
                left outer join tstock on tstock.salablegoodsid =salablegoods.id
                where --hasstock=true and 
                salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) != coalesce(stk,0)	
                )
                insert into stocktransaction(prefid,refid,transactedon,qty,locationid,
                                salablegoodsid,stockid,createdby,reftype,createdon)
                select lrinvoice.id,lrinvoiceitems.id,lrinvoice.entrydate,diffstk,
                            salablegoods.locationid,salablegoods.id,salablegoods.stockid,stock.createdby,
                            'JN' reftype,stock.createdon
                from salablegoods
                inner join mstock on mstock.id = salablegoods.id
                inner join stock on stock.id = stockid
                left outer join lrinvoiceitems on lrinvoiceitems.id =invoiceitemid
                left outer join lrinvoice on lrinvoice.id =lrinvoiceid
                left outer join lrentry on lrentry.id = lrid;";
                ExecuteQuery(query, null);
                data.progress.hascompleted = true;
                data.progress.percentage = 100;
            }
            catch (Exception exp)
            {
                data.progress.hascompleted = false;
                data.progress.Exception = exp;
                data.progress.percentage = 100;
            }

        }
    }

}
