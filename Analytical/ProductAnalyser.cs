using Npgsql;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.Linq;

namespace InfyPOS.Analytical
{
    public class ProductAnalyser
    {
        string weeklysalesquery = @"
select extract(week from @previousmonthstart);
with filterproducts 
as
(
 select items.id,products.name as name,products.id groupid  from items
 inner join products on products.id = productid
 where products.name ilike '%shirt%'
),
salesprices
as
(
select sum(billitems.receivable) amount,
sum(billitems.qty) qty,EXTRACT(week from billdate) from bill
inner join billitems on billitems.billid = bill.id
inner join stock on stock.id = stockid
inner join filterproducts on filterproducts.id = stock.itemid
where bill.billdate>=@previousmonthstart and bill.billdate<@todate and bill.billtype != 9 and bill.isactive = true
group by EXTRACT(week from billdate)
)

select * from salesprices;";

        string quarterlysalesquery = @"
with filterproducts 
as
(
 select items.id,products.name as name,products.id groupid  from items
 inner join products on products.id = productid
 where products.name ilike '%shirt%'
),
salesprices
as
(
select sum(billitems.receivable) amount,
sum(billitems.qty) qty,EXTRACT(quarter from billdate) from bill
inner join billitems on billitems.billid = bill.id
inner join stock on stock.id = stockid
inner join filterproducts on filterproducts.id = stock.itemid
where bill.billdate>=@previousyearstart and bill.billdate<@todate and bill.billtype != 9 and bill.isactive = true
group by EXTRACT(quarter from billdate)
)

select * from salesprices;";

        string pricerangequery = @"with filterproducts 
as
(
 select items.id,{1} as name,products.id groupid  from items
 inner join products on products.id = productid
 {0}
),
salesprices
as
(
select filterproducts.name,filterproducts.groupid,floor((billitems.rate+billitems.itemtax)::numeric),
sum(billitems.qty) qty from bill
inner join billitems on billitems.billid = bill.id
inner join stock on stock.id = stockid
inner join filterproducts on filterproducts.id = stock.itemid
where bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 and bill.isactive = true
group by filterproducts.name,filterproducts.groupid,floor((billitems.rate+billitems.itemtax)::numeric)
)
select * from salesprices";

        string supplierquery = @"with filterproducts 
as
(
 select items.id,{1} as name,products.id groupid  from items
 inner join products on products.id = productid
 {0}
),
currentstock as
(
select stock.supplierid,case when EXTRACT(day from @current_date - stock.stockdate)<30 then 30
	when EXTRACT(day from @current_date - stock.stockdate)<61 then 60 
	when EXTRACT(day from @current_date - stock.stockdate)<91 then 90
	when EXTRACT(day from @current_date - stock.stockdate)<121 then 120
    when EXTRACT(day from @current_date - stock.stockdate)<151 then 150
    when EXTRACT(day from @current_date - stock.stockdate)<181 then 180
	else 999 end as 
	days,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as cqty, sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) cstock from salablegoods
inner join stock on stock.id = stockid
inner join filterproducts on filterproducts.id = stock.itemid
where salablegoods.hasstock = true and salablegoods.isactive = true
group by stock.supplierid,case when EXTRACT(day from @current_date - stock.stockdate)<30 then 30
	when EXTRACT(day from @current_date - stock.stockdate)<61 then 60 
	when EXTRACT(day from @current_date - stock.stockdate)<91 then 90
	when EXTRACT(day from @current_date - stock.stockdate)<121 then 120
    when EXTRACT(day from @current_date - stock.stockdate)<151 then 150
    when EXTRACT(day from @current_date - stock.stockdate)<181 then 180
	else 999 end
),
currentsales as
(
select stock.supplierid,case when EXTRACT(day from bill.billdate - stock.stockdate)<31 then 30
	when EXTRACT(day from bill.billdate - stock.stockdate)<61 then 60 
	when EXTRACT(day from bill.billdate - stock.stockdate)<91 then 90
	when EXTRACT(day from bill.billdate - stock.stockdate)<121 then 120
    when EXTRACT(day from bill.billdate - stock.stockdate)<151 then 150
    when EXTRACT(day from bill.billdate - stock.stockdate)<181 then 180
	else 999 end as 
	days, sum((billitems.qty)*stock.ibuyingprice::numeric) costval,sum(billitems.qty) qty,sum(billitems.receivable) salesvalue from bill
inner join billitems on billitems.billid = bill.id
inner join stock on stock.id = stockid
inner join filterproducts on filterproducts.id = stock.itemid
where bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 and bill.isactive = true
group by stock.supplierid,case when EXTRACT(day from bill.billdate - stock.stockdate)<31 then 30
	when EXTRACT(day from bill.billdate - stock.stockdate)<61 then 60 
	when EXTRACT(day from bill.billdate - stock.stockdate)<91 then 90
	when EXTRACT(day from bill.billdate - stock.stockdate)<121 then 120
    when EXTRACT(day from bill.billdate - stock.stockdate)<151 then 150
    when EXTRACT(day from bill.billdate - stock.stockdate)<181 then 180
	else 999 end
)

select COALESCE(stocksupplier.name,salessupplier.name,'UNKNOWN') supplier,COALESCE(currentstock.days,currentsales.days) days,
COALESCE(currentstock.cqty,0) stockqty,COALESCE(currentstock.cstock,0) stockvalue,
COALESCE(currentsales.costval,0) soldcost,COALESCE(currentsales.qty,0) saleqty,
COALESCE(currentsales.salesvalue,0)
from currentstock
full outer join currentsales on currentsales.supplierid = currentstock.supplierid and currentsales.days = currentstock.days
left outer join supplier stocksupplier  on stocksupplier.id = currentstock.supplierid
left outer join supplier salessupplier  on salessupplier.id = currentsales.supplierid";

        string brandquery = @"with filterproducts 
as
(
 select items.id,brand.name as name,brand.id groupid  from items
 inner join products on products.id = productid
 left outer join brand on brand.id = items.brandid
 {0}
),
currentstock as
(
select filterproducts.groupid,filterproducts.name,case when EXTRACT(day from @current_date - stock.stockdate)<30 then 30
	when EXTRACT(day from @current_date - stock.stockdate)<61 then 60 
	when EXTRACT(day from @current_date - stock.stockdate)<91 then 90
	when EXTRACT(day from @current_date - stock.stockdate)<121 then 120
    when EXTRACT(day from @current_date - stock.stockdate)<151 then 150
    when EXTRACT(day from @current_date - stock.stockdate)<181 then 180
	else 999 end as 
	days,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as cqty, sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) cstock from salablegoods
inner join stock on stock.id = stockid
inner join filterproducts on filterproducts.id = stock.itemid
where salablegoods.hasstock = true and salablegoods.isactive = true
group by filterproducts.groupid,filterproducts.name,case when EXTRACT(day from @current_date - stock.stockdate)<30 then 30
	when EXTRACT(day from @current_date - stock.stockdate)<61 then 60 
	when EXTRACT(day from @current_date - stock.stockdate)<91 then 90
	when EXTRACT(day from @current_date - stock.stockdate)<121 then 120
    when EXTRACT(day from @current_date - stock.stockdate)<151 then 150
    when EXTRACT(day from @current_date - stock.stockdate)<181 then 180
	else 999 end
),
currentsales as
(
select filterproducts.groupid,filterproducts.name,case when EXTRACT(day from bill.billdate - stock.stockdate)<31 then 30
	when EXTRACT(day from bill.billdate - stock.stockdate)<61 then 60 
	when EXTRACT(day from bill.billdate - stock.stockdate)<91 then 90
	when EXTRACT(day from bill.billdate - stock.stockdate)<121 then 120
    when EXTRACT(day from bill.billdate - stock.stockdate)<151 then 150
    when EXTRACT(day from bill.billdate - stock.stockdate)<181 then 180
	else 999 end as 
	days, sum((billitems.qty)*stock.ibuyingprice::numeric) costval,sum(billitems.qty) qty,sum(billitems.receivable) salesvalue from bill
inner join billitems on billitems.billid = bill.id
inner join stock on stock.id = stockid
inner join filterproducts on filterproducts.id = stock.itemid
where bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 and bill.isactive = true
group by filterproducts.groupid,filterproducts.name,case when EXTRACT(day from bill.billdate - stock.stockdate)<31 then 30
	when EXTRACT(day from bill.billdate - stock.stockdate)<61 then 60 
	when EXTRACT(day from bill.billdate - stock.stockdate)<91 then 90
	when EXTRACT(day from bill.billdate - stock.stockdate)<121 then 120
    when EXTRACT(day from bill.billdate - stock.stockdate)<151 then 150
    when EXTRACT(day from bill.billdate - stock.stockdate)<181 then 180
	else 999 end
)

select COALESCE(currentstock.name,currentsales.name,'UNKNOWN') brand,COALESCE(currentstock.days,currentsales.days) days,
COALESCE(currentstock.cqty,0) stockqty,COALESCE(currentstock.cstock,0) stockvalue,
COALESCE(currentsales.costval,0) soldcost,COALESCE(currentsales.qty,0) saleqty,
COALESCE(currentsales.salesvalue,0)
from currentstock
full outer join currentsales on currentsales.groupid = currentstock.groupid and currentsales.days = currentstock.days";

        string productquery = @"with filterproducts 
as
(
 select items.id,{1} as name,{2} groupid  from items
 inner join products on products.id = productid
 {3}
),
currentstock as
(
select filterproducts.name,filterproducts.groupid,sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as cqty, sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) cstock from salablegoods
inner join stock on stock.id = stockid
inner join filterproducts on filterproducts.id = stock.itemid
where salablegoods.hasstock = true and salablegoods.isactive = true
group by filterproducts.name,filterproducts.groupid
),
newpurchase as
(
select filterproducts.name,filterproducts.groupid, sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as qty,sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) costvalue from salablegoods
inner join stock on stock.id = stockid
inner join filterproducts on filterproducts.id = stock.itemid
where stock.stockdate>=@fromdate and stock.stockdate<@todate
group by filterproducts.name,filterproducts.groupid
),
afterpurchase as
(
select filterproducts.name,filterproducts.groupid, sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as qty,sum((salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))*stock.ibuyingprice::numeric) costvalue from salablegoods
inner join stock on stock.id = stockid
inner join filterproducts on filterproducts.id = stock.itemid
where stock.stockdate>=@todate
group by filterproducts.name,filterproducts.groupid
),
salesvalues
as
(
select filterproducts.name,filterproducts.groupid, sum((billitems.qty)*stock.ibuyingprice::numeric) costval,sum(billitems.qty) qty,sum(billitems.receivable) salesvalue from bill
inner join billitems on billitems.billid = bill.id
inner join stock on stock.id = stockid
inner join filterproducts on filterproducts.id = stock.itemid
where bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 and bill.isactive = true
group by filterproducts.name,filterproducts.groupid
),
newsalesvalues
as
(
select filterproducts.name,filterproducts.groupid, sum((billitems.qty)*stock.ibuyingprice::numeric) costval,sum(billitems.qty) qty,sum(billitems.receivable) salesvalue from bill
inner join billitems on billitems.billid = bill.id
inner join stock on stock.id = stockid
inner join filterproducts on filterproducts.id = stock.itemid
where bill.billdate>=@fromdate and bill.billdate<@todate and bill.billtype != 9 and bill.isactive = true and stock.stockdate>=@fromdate
group by filterproducts.name,filterproducts.groupid
),grouplist
as
(
select distinct groupid,name from filterproducts
)

select grouplist.groupid,grouplist.name,currentstock.cqty currentstockqty,currentstock.cstock currentstockvalue,
newpurchase.qty newpurchaseqty,newpurchase.costvalue newpurchasevalue,
afterpurchase.qty as afterpurchaseqty,afterpurchase.costvalue afterpurchasevalue,
salesvalues.costval salescostvalue,salesvalues.qty salesqty,salesvalues.salesvalue,newsalesvalues.costval newsalecostvalue,
newsalesvalues.qty newsalesqty,newsalesvalues.salesvalue newsalesvalue from grouplist
left outer join currentstock on currentstock.groupid = grouplist.groupid
left outer join newpurchase on newpurchase.groupid = grouplist.groupid
left outer join afterpurchase on afterpurchase.groupid = grouplist.groupid
left outer join salesvalues on salesvalues.groupid = grouplist.groupid
left outer join newsalesvalues on newsalesvalues.groupid = grouplist.groupid;";


    }
}