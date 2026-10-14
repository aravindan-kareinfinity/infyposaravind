using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class SalesorderitemsDataService : SAASData
    {
        /*
         *
         *

with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
	case when position('/' in schemelist->>'code') = 0 then '' else
	substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
	end colourcode
	from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
	where isactive=true
),changescript as
(select salesorder.sodate,salesorderitems.description,referencelist.name, salesorderitems.version,salesorderitems.id,salesorder.sono,alluid.colourid,alluid.uid,
alluid.colourcode,designitemcode,salluid.uid nuid  from salesorderitems
	inner join salesorder on salesorder.id = salesorderitems.salesorderid
	inner join alluid on alluid.uid = salesorderitems.uid
 	inner join alluid salluid on salluid.dname = salesorderitems.designitemcode and salluid.sizeid = salesorderitems.sizeid
	inner join referencelist on referencelist.id = salesorderitems.colourid
	where salesorderitems.isactive=true and 
 (alluid.code != salesorderitems.designid or 
cast(alluid.dname as varchar) != salesorderitems.designitemcode or 
alluid.colourcode != salesorderitems.colourcode or 
alluid.colourid != salesorderitems.colourid or 
alluid.sizeid != salesorderitems.sizeid)
) 
select * from changescript
--update salesorderitems set uid=nuid from changescript
--where changescript.id = salesorderitems.id




with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
	case when position('/' in schemelist->>'code') = 0 then '' else
	substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
	end colourcode
	from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
	where isactive=true
),changescript as
(
select distinct salesorder.sono, salesorderitems.description,acolour.name mfgcolour,salesorderitems.designid, salluid.code ,
	salluid.dname,salesorderitems.colourcode,salesorderitems.id,salluid.uid nuid,salluid.colourcode ncolourcode from salesorderitems
	inner join salesorder on salesorder.id = salesorderitems.salesorderid
	inner join alluid on  alluid.uid = salesorderitems.uid
	inner join alluid salluid on salluid.code = alluid.code 
	inner join referencelist acolour on acolour.id = salluid.colourid
	where salesorderitems.version = -111 and 
	position(acolour.name in salesorderitems.description)>0
)

--select * from changescript
--update salesorderitems set version=-111 from changescript
--where changescript.id = salesorderitems.id

update salesorderitems set designitemcode=dname,colourcode=ncolourcode,uid=nuid,version=-9 from changescript
where changescript.id = salesorderitems.id

with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
	case when position('/' in schemelist->>'code') = 0 then '' else
	substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
	end colourcode
	from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
	where isactive=true
),changescript as
(
select salesorderitems.description,acolour.name mfgcolour,salesorderitems.designid,
	alluid.dname,salesorderitems.id,alluid.uid nuid,alluid.colourcode ncolourcode from salesorderitemsold
	inner join salesorderitems on salesorderitems.id = salesorderitemsold.id
	inner join salesorder on salesorder.id = salesorderitems.salesorderid
	inner join alluid on  alluid.code = salesorderitems.DESIGNID and alluid.colourid = salesorderitemsold.colourid and alluid.sizeid = salesorderitems.sizeid 
 	and alluid.colourid = salesorderitemsold.colourid and alluid.dname ilike salesorderitems.designid||'/'||salesorderitemsold.colourcode
	inner join referencelist acolour on acolour.id = alluid.colourid
	inner join referencelist scolour on scolour.id = salesorderitems.colourid
	where salesorderitems.uid != alluid.uid
	and position(acolour.name in salesorderitems.description)>0 
)
update salesorderitems set designitemcode=dname,colourcode=ncolourcode,uid=nuid,version=-9 from changescript
where changescript.id = salesorderitems.id


with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
	case when position('/' in schemelist->>'code') = 0 then '' else
	substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
	end colourcode
	from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
	where isactive=true
),changescript as
(
select salesorderitems.description,acolour.name mfgcolour,salesorderitems.designid,
	alluid.dname,salesorderitems.id,alluid.uid nuid,alluid.colourcode ncolourcode from salesorderitems
	inner join salesorder on salesorder.id = salesorderitems.salesorderid
	inner join alluid on  alluid.code = salesorderitems.DESIGNID and alluid.colourid = salesorderitems.colourid and alluid.sizeid = salesorderitems.sizeid 
 	and alluid.colourid = salesorderitems.colourid and alluid.dname ilike salesorderitems.designid||'/'||salesorderitems.colourcode
	inner join referencelist acolour on acolour.id = alluid.colourid
	where salesorderitems.version != -9 and salesorderitems.uid != alluid.uid
	and position(acolour.name in salesorderitems.description)>0 
)

update salesorderitems set designitemcode=dname,colourcode=ncolourcode,uid=nuid,version=-99 from changescript
where changescript.id = salesorderitems.id




with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
	case when position('/' in schemelist->>'code') = 0 then '' else
	substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
	end colourcode
	from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
	where isactive=true
)

select salesorderitems.id,salesorderitems.uid,salluid.uid,salluid.colourcode,salluid.dname from salesorderitems
inner join alluid on alluid.uid = salesorderitems.uid
inner join referencelist socolour on socolour.id = alluid.colourid
inner join salesorder on salesorder.id = salesorderitems.salesorderid
left outer join alluid salluid on salluid.code = salesorderitems.designid and salluid.sizeid = salesorderitems.sizeid 
 and salluid.colourid = salesorderitems.colourid 
where salesorderitems.description != '' and salesorderitems.isactive=true and salesorder.sono ='SO-501' AND DESIGNID='QCH-112'
and position(socolour.name in salesorderitems.description)=0
order by salesorderitems.id desc limit 10

with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
	case when position('/' in schemelist->>'code') = 0 then '' else
	substring(schemelist->>'code',position('/' in schemelist->>'code')+1)
	end colourcode
	from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
	where isactive=true
),wrongsoitem as
(
select salesorderitemsold.*,alluid.dname,alluid.colourcode ncolourcode,alluid.colourid ncolourid,code from salesorderitemsold
inner join salesorderitems on salesorderitems.id = salesorderitemsold.id
inner join alluid on alluid.uid = salesorderitemsold.uid
where salesorderitems.uid != salesorderitemsold.uid
)

update salesorderitems set designitemcode = wrongsoitem.dname,colourcode=wrongsoitem.ncolourcode,colourid=ncolourid from wrongsoitem
where wrongsoitem.id = salesorderitems.id



with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,modifiedon from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
	where mfgdesign.isactive=true
),wrongso as
(
select Salesorderitems.id, alluid.uid,designitemcode,dname,colourcode,Salesorderitems.createdon,
Salesorderitems.sizeid,Salesorderitems.colourid,alluid.modifiedon from Salesorderitems
inner join salesorder on salesorder.id = Salesorderitems.salesorderid
inner join alluid on alluid.dname = Salesorderitems.designitemcode  and 
alluid.sizeid = Salesorderitems.sizeid and Salesorderitems.designid = alluid.code --and alluid.uid = Salesorderitems.uid
where Salesorderitems.isactive=true and salesorder.isactive=true 
)
update Salesorderitems set uid = wrongso.uid from wrongso
where wrongso.id = Salesorderitems.id

            with allsoitems
as
(
select cast(schemelist->>'id' as bigint) id,sum(cast(schemelist->>'qty' as decimal)) qty
									from billitems
									LEFT JOIN LATERAL json_array_elements (billitems.attributes->'so') schemelist ON true
									inner join bill on bill.id = billitems.billid
                                    where billitems.attributes is not null and billitems.attributes->>'so' != '[null]' and
	
					bill.isactive = true
	group by cast(schemelist->>'id' as bigint) 
),changes
as
(
select salesorderitems.id,salesorderitems.bqty,allsoitems.qty from salesorderitems
inner join allsoitems on allsoitems.id = salesorderitems.id
where salesorderitems.bqty != allsoitems.qty
	)
    
	
	update salesorderitems  set bqty=changes.qty from changes where salesorderitems.id = changes.id
*/

        const string SQL_Search = @"select Salesorderitems.id, Salesorderitems.productid, itemid, oqty, rqty, cqty, Salesorderitems.isactive, Salesorderitems.price,Salesorderitems.designuid,
                                    Salesorderitems.priceupto,Salesorderitems.designid,Salesorderitems.rowtype,Salesorderitems.size,Salesorderitems.colourcode,
                                    COLOURSHADE,Salesorderitems.pricerange,description,Salesorderitems.bqty,Salesorderitems.tax,Salesorderitems.taxpercentage,Salesorderitems.taxid,
                                    Salesorderitems.imageid, Salesorder.customerid, Salesorder.sono,Salesorderitems.sizegroup,Salesorderitems.sizegroupid,Salesorderitems.colourid,Salesorderitems.pqty,
                                    Salesorderitems.createdby, Salesorderitems.modifiedby, Salesorderitems.version, salesorderid, Salesorderitems.createdon, Salesorderitems.sizeid,
                                    Salesorderitems.modifiedon,products.name productname,products.name printingname,brand.name brandname,mfgsampleordercode,
									colours.name colourname,patternname,sizes.name sizename,stylename,materialname,typename,Salesorderitems.brandid,
									Salesorderitems.uid,additional.name addnlchargename,Salesorderitems.designitemcode,box.name boxname,mfgsampleorderid,
                                    Salesorderitems.addnlchargeid,Salesorderitems.addnlcharge,Salesorderitems.mrp,fgboxid,soboxid,mfgdesign.prdhsncode
                                    from Salesorder 
                                    inner join Salesorderitems on Salesorderitems.salesorderid = Salesorder.id and Salesorderitems.isactive = true
                                    left outer join mfgdesign on mfgdesign.code = Salesorderitems.designid and mfgdesign.isactive=true
                                    left outer join items on items.id = Salesorderitems.itemid 
									left outer join products on products.id = Salesorderitems.productid
                                    left outer join brand on brand.id = Salesorderitems.brandid
									left outer join referencelist colours on colours.id= Salesorderitems.colourid
									left outer join referencelist sizes on sizes.id= Salesorderitems.sizeid
                                    left outer join referencelist additional on additional.id= Salesorderitems.addnlchargeid
                                    left outer join referencelist box on box.id= Salesorderitems.soboxid
                                    where Salesorderitems.id=@id";

        const string SQL_SearchAll = @"select id, productid, itemid, oqty, rqty, cqty, isactive, createdby, modifiedby, version, salesorderid, createdon,imageid, designitemcode,fgboxid,soboxid,
                                    modifiedon,price,priceupto,designid,rowtype,size,colourcode,COLOURSHADE,pricerange,sizeid,description,designuid,colourid,sizegroupid,sizegroup,bqty,pqty,uid,addnlchargeid,addnlcharge,mrp,
                                    Salesorderitems.tax,Salesorderitems.taxpercentage,Salesorderitems.taxid,mfgsampleorderid,mfgsampleordercode from Salesorderitems 
                                    where salesorderid=@salesorderid and isactive=true";

        const string SQL_Insert = @"INSERT INTO public.Salesorderitems(productid, itemid, oqty, rqty, cqty, isactive, createdby, modifiedby, version,
        salesorderid, createdon, modifiedon,price,designid,priceupto,rowtype,size,colourcode,COLOURSHADE,pricerange,sizeid,description,imageid,designuid,colourid,sizegroupid,
        sizegroup,bqty,uid,addnlchargeid,addnlcharge,mrp,brandid,designitemcode,fgboxid,soboxid,tax,taxpercentage,taxid,mfgsampleorderid,mfgsampleordercode) 
        VALUES(@productid, @itemid, @oqty, @rqty, @cqty, @isactive, @createdby, @modifiedby, @version, @salesorderid, @createdon,
        @modifiedon,@price,@designid,@priceupto,@rowtype,@size,@colourcode,@COLOURSHADE,@pricerange,@sizeid,@description,@imageid,@designuid,@colourid,@sizegroupid,@sizegroup,
        @bqty,@uid,@addnlchargeid,@addnlcharge,@mrp,@brandid,@designitemcode,@fgboxid,@soboxid,@tax,@taxpercentage,@taxid,@mfgsampleorderid,@mfgsampleordercode) RETURNING id";


        const string SQL_Update = @"UPDATE public.Salesorderitems SET productid=@productid, itemid=@itemid, oqty=@oqty, rqty=@rqty, cqty=@cqty,colourcode=@colourcode,COLOURSHADE=@COLOURSHADE,
        fgboxid=@fgboxid,soboxid=@soboxid,price=@price,designid=@designid, modifiedby=@modifiedby, version=@version+1, salesorderid=@salesorderid, modifiedon=@modifiedon,imageid=@imageid,
        designuid=designuid,colourid=colourid,sizegroupid=@sizegroupid,sizegroup=@sizegroup,priceupto=@priceupto,rowtype=@rowtype,size=@size,pricerange=@pricerange,sizeid=@sizeid,
        description=@description,bqty=@bqty,pqty=@pqty,tax=@tax,taxpercentage=@taxpercentage,taxid=@taxid,mfgsampleorderid=@mfgsampleorderid,mfgsampleordercode=@mfgsampleordercode
        where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.Salesorderitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.Salesorderitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where salesorderid=@salesorderid";

        public List<SalesorderitemsDetail> Search(SalesorderitemsCreteria _Salesorderitems)
        {
            List<SalesorderitemsDetail> lst = new List<SalesorderitemsDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_Salesorderitems.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _Salesorderitems.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("Salesorderitems.isactive", true, "=");
                    qb.AddParameter("Salesorder.isactive", true, "=");

                    if (_Salesorderitems.sizeid > 0)
                        qb.AddParameter("Salesorderitems.sizeid", _Salesorderitems.sizeid, "=");

                    if (_Salesorderitems.salesorderid > 0)
                        qb.AddParameter("salesorderid", _Salesorderitems.salesorderid, "=");

                    if (!string.IsNullOrEmpty(_Salesorderitems.uid))
                        qb.AddParameter("uid", _Salesorderitems.uid, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesorderitemsDetail oSalesorderitems = new SalesorderitemsDetail();
                        oSalesorderitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oSalesorderitems.sono = reader["sono"] == DBNull.Value ? "" : reader["sono"].ToString();
                        oSalesorderitems.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        oSalesorderitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oSalesorderitems.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        oSalesorderitems.uid = reader["uid"] == DBNull.Value ? "" : reader["uid"].ToString();
                        oSalesorderitems.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oSalesorderitems.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        oSalesorderitems.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        oSalesorderitems.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        oSalesorderitems.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        oSalesorderitems.sizename = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        oSalesorderitems.colourcode = reader["colourcode"] == DBNull.Value ? "" : reader["colourcode"].ToString();
                        oSalesorderitems.colourshade = reader["colourshade"] == DBNull.Value ? "" : reader["colourshade"].ToString();
                        oSalesorderitems.designitemcode = reader["designitemcode"] == DBNull.Value ? "" : reader["designitemcode"].ToString();
                        oSalesorderitems.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        oSalesorderitems.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        oSalesorderitems.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        oSalesorderitems.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        oSalesorderitems.hsncode = reader["prdhsncode"] == DBNull.Value ? "" : reader["prdhsncode"].ToString();
                        oSalesorderitems.addnlchargename = reader["addnlchargename"] == DBNull.Value ? "" : reader["addnlchargename"].ToString();
                        oSalesorderitems.boxname = reader["boxname"] == DBNull.Value ? "" : reader["boxname"].ToString();

                        oSalesorderitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        oSalesorderitems.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        oSalesorderitems.priceupto = reader["priceupto"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["priceupto"]);
                        oSalesorderitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();

                        oSalesorderitems.addnlcharge = reader["addnlcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["addnlcharge"]);
                        oSalesorderitems.addnlchargeid = reader["addnlchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["addnlchargeid"]);
                        oSalesorderitems.mfgsampleorderid = reader["mfgsampleorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgsampleorderid"]);
                        oSalesorderitems.mfgsampleordercode = reader["mfgsampleordercode"] == DBNull.Value ? "" : reader["mfgsampleordercode"].ToString();

                        oSalesorderitems.fgboxid = reader["fgboxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fgboxid"]);
                        oSalesorderitems.soboxid = reader["soboxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["soboxid"]);
                        oSalesorderitems.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);

                        oSalesorderitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        oSalesorderitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        oSalesorderitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);

                        oSalesorderitems.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        oSalesorderitems.sizegroupid = reader["sizegroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizegroupid"]);
                        oSalesorderitems.designuid = reader["designuid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designuid"]);
                        oSalesorderitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        oSalesorderitems.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);

                        oSalesorderitems.pricerange = reader["pricerange"] == DBNull.Value ? "" : reader["pricerange"].ToString();
                        oSalesorderitems.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        oSalesorderitems.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        oSalesorderitems.rowtype = reader["rowtype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rowtype"]);

                        oSalesorderitems.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        oSalesorderitems.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        oSalesorderitems.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                        oSalesorderitems.bqty = reader["bqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bqty"]);
                        oSalesorderitems.pqty = reader["pqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pqty"]);

                        oSalesorderitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oSalesorderitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oSalesorderitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oSalesorderitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oSalesorderitems.salesorderid = reader["salesorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesorderid"]);
                        oSalesorderitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oSalesorderitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        if (string.IsNullOrEmpty(oSalesorderitems.description))
                        {
                            oSalesorderitems.description = oSalesorderitems.productname + "/" + oSalesorderitems.brandname + "/" + oSalesorderitems.colourname;
                        }
                        lst.Add(oSalesorderitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _Salesorderitems);
            }
            return lst;
        }

        public List<SalesorderitemsDetail> SearchAll(long _salesorderid)
        {
            List<SalesorderitemsDetail> lst = new List<SalesorderitemsDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);
                command.Parameters.Add("salesorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _salesorderid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesorderitemsDetail oSalesorderitems = new SalesorderitemsDetail();
                        oSalesorderitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oSalesorderitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oSalesorderitems.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        oSalesorderitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oSalesorderitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oSalesorderitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oSalesorderitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oSalesorderitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        oSalesorderitems.salesorderid = reader["salesorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesorderid"]);
                        oSalesorderitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oSalesorderitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oSalesorderitems.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        oSalesorderitems.priceupto = reader["priceupto"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["priceupto"]);
                        oSalesorderitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        oSalesorderitems.pricerange = reader["pricerange"] == DBNull.Value ? "" : reader["pricerange"].ToString();
                        oSalesorderitems.priceranges = oSalesorderitems.pricerange;
                        oSalesorderitems.mfgsampleorderid = reader["mfgsampleorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgsampleorderid"]);
                        oSalesorderitems.mfgsampleordercode = reader["mfgsampleordercode"] == DBNull.Value ? "" : reader["mfgsampleordercode"].ToString();
                        oSalesorderitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        oSalesorderitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        oSalesorderitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        oSalesorderitems.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        oSalesorderitems.sizegroupid = reader["sizegroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizegroupid"]);
                        oSalesorderitems.designuid = reader["designuid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designuid"]);
                        oSalesorderitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        oSalesorderitems.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        oSalesorderitems.sizename = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        oSalesorderitems.colourcode = reader["colourcode"] == DBNull.Value ? "" : reader["colourcode"].ToString();
                        oSalesorderitems.colourshade = reader["colourshade"] == DBNull.Value ? "" : reader["colourshade"].ToString();
                        oSalesorderitems.designitemcode = reader["designitemcode"] == DBNull.Value ? "" : reader["designitemcode"].ToString();
                        oSalesorderitems.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        oSalesorderitems.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        oSalesorderitems.rowtype = reader["rowtype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rowtype"]);
                        oSalesorderitems.bqty = reader["bqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bqty"]);
                        oSalesorderitems.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        oSalesorderitems.pqty = reader["pqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pqty"]);
                        oSalesorderitems.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        oSalesorderitems.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                        oSalesorderitems.uid = reader["uid"] == DBNull.Value ? "" : reader["uid"].ToString();
                        oSalesorderitems.addnlcharge = reader["addnlcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["addnlcharge"]);
                        oSalesorderitems.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        oSalesorderitems.addnlchargeid = reader["addnlchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["addnlchargeid"]);
                        oSalesorderitems.fgboxid = reader["fgboxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fgboxid"]);
                        oSalesorderitems.soboxid = reader["soboxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["soboxid"]);

                        lst.Add(oSalesorderitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _salesorderid);
            }
            return lst;
        }

        public Salesorderitems Insert(SalesorderitemsDetail oSalesorderitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oSalesorderitems);

                    
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalesorderitems);


            }

            return oSalesorderitems;
        }

        public Salesorderitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, SalesorderitemsDetail oSalesorderitems)
        {
            if (string.IsNullOrEmpty(oSalesorderitems.uid))
            {
                throw new Exception("Empty UAN can't add to SO - " + oSalesorderitems.designid + "/" + oSalesorderitems.colourcode + "/" + oSalesorderitems.size);
            }
            if (string.IsNullOrEmpty(oSalesorderitems.designitemcode))
            {
                throw new Exception("Empty Design/Code can't add to SO - " + oSalesorderitems.designid + "/" + oSalesorderitems.colourcode + "/" + oSalesorderitems.size);
            }
            if(oSalesorderitems.price == 0)
            {
                throw new Exception(oSalesorderitems.uid+" - Price missing\r\n"+"Design/Code can't add to SO - " + oSalesorderitems.designid + "/" + oSalesorderitems.colourcode + "/" + oSalesorderitems.size);
            }
            if (oSalesorderitems.itemid == 0)
            {
                ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
                var items = itemsDataService.Search(new ItemsCreteria()
                {
                    productmap = true,
                    sizeid = oSalesorderitems.sizeid,
                    styleid = oSalesorderitems.styleid,
                    materialid = oSalesorderitems.materialid,
                    brandid = oSalesorderitems.brandid,
                    typeid = oSalesorderitems.typeid,
                    patternid = oSalesorderitems.patternid,
                    colourid = oSalesorderitems.colourid,
                    productid = oSalesorderitems.productid,
                    gln = oSalesorderitems.uid
                });
                if (items.Count > 0)
                    oSalesorderitems.itemid = items[0].id;
                else
                {
                    var item = itemsDataService.Insert(transaction, conn, new Items()
                    {
                        printingname = oSalesorderitems.printingname,
                        productname = oSalesorderitems.printingname,
                        materialname = oSalesorderitems.materialname,
                        stylename = oSalesorderitems.stylename,
                        sizename = oSalesorderitems.sizename,
                        typename = oSalesorderitems.typename,
                        patternname = oSalesorderitems.patternname,
                        colourname = oSalesorderitems.colourname,
                        sizeid = oSalesorderitems.sizeid,
                        styleid = oSalesorderitems.styleid,
                        materialid = oSalesorderitems.materialid,
                        brandid = oSalesorderitems.brandid,
                        brandname = oSalesorderitems.brandname,
                        productid = oSalesorderitems.productid,
                        typeid = oSalesorderitems.typeid,
                        patternid = oSalesorderitems.patternid,
                        colourid = oSalesorderitems.colourid,
                        isactive = true,
                        gln = oSalesorderitems.uid,
                        version = 1,
                        createdby = Context.UserID,
                        modifiedby = Context.UserID,
                        createdon = DateTimeNow,
                        modifiedon = DateTimeNow
                    });
                    oSalesorderitems.itemid = item.id;
                }
            }
            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.productid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.itemid;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.brandid;
            command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.oqty;
            command.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.rqty;
            command.Parameters.Add("cqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.cqty;
            command.Parameters.Add("bqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.bqty;

            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.taxid;
            command.Parameters.Add("mfgsampleorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.mfgsampleorderid;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.tax;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.taxpercentage;
            command.Parameters.Add("mfgsampleordercode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.mfgsampleordercode == null ? "" : oSalesorderitems.mfgsampleordercode;

            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.price;
            command.Parameters.Add("priceupto", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.priceupto;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.designid == null ? "" : oSalesorderitems.designid;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.description == null ? "" : oSalesorderitems.description;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.imageid == null ? "" : oSalesorderitems.imageid;

            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalesorderitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalesorderitems.version;
            command.Parameters.Add("salesorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.salesorderid;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;

            command.Parameters.Add("uid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.uid == null ? "" : oSalesorderitems.uid;
            command.Parameters.Add("mrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.mrp;
            command.Parameters.Add("addnlcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.addnlcharge;
            command.Parameters.Add("addnlchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.addnlchargeid;

            command.Parameters.Add("fgboxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.fgboxid;
            command.Parameters.Add("soboxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.soboxid;


            if (!string.IsNullOrEmpty(oSalesorderitems.size) && oSalesorderitems.size.Length > 40)
            {
                if (oSalesorderitems.size.IndexOf('-') > 0)
                    oSalesorderitems.size = oSalesorderitems.size.Substring(0, oSalesorderitems.size.IndexOf('-'));
                else
                    oSalesorderitems.size = oSalesorderitems.size.Substring(0,40);
            }

            if (!string.IsNullOrEmpty(oSalesorderitems.sizegroup) && oSalesorderitems.sizegroup.Length > 40)
            {
                if (oSalesorderitems.sizegroup.IndexOf('-') > 0)
                    oSalesorderitems.sizegroup = oSalesorderitems.sizegroup.Substring(0, oSalesorderitems.sizegroup.IndexOf('-'));
                else
                    oSalesorderitems.sizegroup = oSalesorderitems.sizegroup.Substring(0, 40);

            }

            //if(oSalesorderitems.colourcode != null &&
            //    oSalesorderitems.colourcode.IndexOf('/')>=0)
            //{
            //    oSalesorderitems.colourcode = oSalesorderitems.colourcode.Substring(oSalesorderitems.colourcode.IndexOf('/') + 1);
            //}

            command.Parameters.Add("rowtype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalesorderitems.rowtype;
            command.Parameters.Add("pricerange", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.pricerange == null ? "" : oSalesorderitems.pricerange;
            command.Parameters.Add("size", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.size == null ? "" : oSalesorderitems.size;
            command.Parameters.Add("colourcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.colourcode == null ? "" : oSalesorderitems.colourcode;
            command.Parameters.Add("colourshade", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.colourshade == null ? "" : oSalesorderitems.colourshade;
            command.Parameters.Add("designitemcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.designitemcode == null ? "" : oSalesorderitems.designitemcode;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.sizeid;

            command.Parameters.Add("sizegroup", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.sizegroup == null ? "" : oSalesorderitems.sizegroup;
            command.Parameters.Add("sizegroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.sizegroupid;
            command.Parameters.Add("designuid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.designuid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.colourid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSalesorderitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oSalesorderitems;
        }


        public Salesorderitems Update(SalesorderitemsDetail oSalesorderitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var items = Search(new SalesorderitemsCreteria { id = oSalesorderitems.id });
                    if (items.Count == 1)
                        Update(transaction, conn, oSalesorderitems, items[0]);
                    else
                        Update(transaction, conn, oSalesorderitems, null);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalesorderitems);


            }

            return oSalesorderitems;
        }

        public Salesorderitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn,
            Salesorderitems oSalesorderitems, Salesorderitems oldItem)
        {

            if (oSalesorderitems.price == 0)
            {
                throw new Exception(oSalesorderitems.uid + " - Price missing\r\n" + "Design/Code can't add to SO - " + oSalesorderitems.designid + "/" + oSalesorderitems.colourcode + "/" + oSalesorderitems.size);
            }

            if (!string.IsNullOrEmpty(oSalesorderitems.size) && oSalesorderitems.size.Length > 40)
            {
                if (oSalesorderitems.size.IndexOf('-') > 0)
                    oSalesorderitems.size = oSalesorderitems.size.Substring(0, oSalesorderitems.size.IndexOf('-'));
                else
                    oSalesorderitems.size = oSalesorderitems.size.Substring(0, 40);
            }

            if (!string.IsNullOrEmpty(oSalesorderitems.sizegroup) && oSalesorderitems.sizegroup.Length > 40)
            {
                if (oSalesorderitems.sizegroup.IndexOf('-') > 0)
                    oSalesorderitems.sizegroup = oSalesorderitems.sizegroup.Substring(0, oSalesorderitems.sizegroup.IndexOf('-'));
                else
                    oSalesorderitems.sizegroup = oSalesorderitems.sizegroup.Substring(0, 40);
            }


    //        if (oSalesorderitems.colourcode != null &&
    //oSalesorderitems.colourcode.IndexOf('/') >= 0)
    //        {
    //            oSalesorderitems.colourcode = oSalesorderitems.colourcode.Substring(oSalesorderitems.colourcode.IndexOf('/') + 1);
    //        }
            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.id;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.productid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.itemid;
            command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.oqty;
            command.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.rqty;
            command.Parameters.Add("cqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.cqty;
            command.Parameters.Add("bqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.bqty;
            command.Parameters.Add("pqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.pqty;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.price;
            command.Parameters.Add("priceupto", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.priceupto;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.designid == null ? "" : oSalesorderitems.designid;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.description == null ? "" : oSalesorderitems.description;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalesorderitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalesorderitems.version;
            command.Parameters.Add("salesorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.salesorderid;
            command.Parameters.Add("mfgsampleorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.mfgsampleorderid;
            command.Parameters.Add("mfgsampleordercode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.mfgsampleordercode == null ? "" : oSalesorderitems.mfgsampleordercode;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oSalesorderitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("rowtype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalesorderitems.rowtype;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.taxid;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.tax;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oSalesorderitems.taxpercentage;
            command.Parameters.Add("pricerange", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.pricerange == null ? "" : oSalesorderitems.pricerange;
            command.Parameters.Add("size", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.size == null ? "" : oSalesorderitems.size;
            command.Parameters.Add("colourcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.colourcode == null ? "" : oSalesorderitems.colourcode;
            command.Parameters.Add("colourshade", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.colourshade == null ? "" : oSalesorderitems.colourshade;
            command.Parameters.Add("designitemcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.designitemcode == null ? "" : oSalesorderitems.designitemcode;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.sizeid;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.imageid == null ? "" : oSalesorderitems.imageid;
            command.Parameters.Add("sizegroup", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oSalesorderitems.sizegroup == null ? "" : oSalesorderitems.sizegroup;
            command.Parameters.Add("sizegroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.sizegroupid;
            command.Parameters.Add("designuid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.designuid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.colourid;
            command.Parameters.Add("fgboxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.fgboxid;
            command.Parameters.Add("soboxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.soboxid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oSalesorderitems.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oSalesorderitems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<SalesorderitemsDetail> list, long id)
        {
            var existingItems = SearchAll(id);
            if (existingItems != null && existingItems.Count > 0)
            {
                var grouplist = list.FindAll(e => e.id < 0);
                var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                foreach (var item in deletedItems)
                {
                    if (!grouplist.Exists(e => e.jumpqty.Exists(x => x.id == item.id)))
                        Delete(transaction, conn, item);
                }
            }

            foreach (var gitem in list.GroupBy(x => new { x.designitemcode,x.price, x.mrp, x.productid, x.brandid,x.fgboxid,x.soboxid }))
            {
                var existList = gitem.Where(x => x.id > 0).ToList();
                foreach (var eitem in existList)
                {
                    Update(transaction, conn, eitem, existingItems.Find(e => e.id == eitem.id));
                }
                var newList = gitem.Where(x => x.id <= 0 && (x.jumpqty == null || x.jumpqty.Count == 0)).ToList();
                foreach (var eitem in newList)
                {
                    if (!string.IsNullOrEmpty(eitem.sizename) && string.IsNullOrEmpty(eitem.size))
                        eitem.size = eitem.sizename;
                    Insert(transaction, conn, eitem);
                }
                newList = gitem.Where(x => x.id <= 0 && x.jumpqty != null && x.jumpqty.Count > 0).ToList();
                SortedDictionary<long, SalesorderitemsDetail.SizeQty> listOfQty = new SortedDictionary<long, SalesorderitemsDetail.SizeQty>();
                foreach (var eqty in newList)
                {
                    if (eqty.jumpqty.Exists(x => x.id > 0))
                        UpdateAdditionalInfo(transaction, conn, eqty);

                    foreach (var eitem in eqty.jumpqty)
                    {

                        if (listOfQty.ContainsKey(eitem.sizeid))
                        {
                            if (eitem.id == 0)
                            {
                                listOfQty[eitem.sizeid].oqty += eitem.oqty;
                            }
                            else if (listOfQty[eitem.sizeid].id == 0)
                            {
                                listOfQty[eitem.sizeid].id = eitem.id;
                                listOfQty[eitem.sizeid].oqty += eitem.oqty;
                            }
                            else
                            {
                                listOfQty[eitem.sizeid].oqty += eitem.oqty;
                                if (existingItems.Exists(x => x.id == eitem.id))
                                    Delete(transaction, conn, existingItems.Find(x => x.id == eitem.id));
                            }
                        }
                        else
                        {
                            eitem.parent = eqty;
                            listOfQty.Add(eitem.sizeid, eitem);
                        }
                    }
                }


                SalesorderitemsDetail item = Newtonsoft.Json.JsonConvert.DeserializeObject<SalesorderitemsDetail>(Newtonsoft.Json.JsonConvert.SerializeObject(gitem.First()));
                foreach (var qitem in listOfQty)
                {

                    var jitem = qitem.Value;
                    if (jitem.id > 0)
                    {
                        UpdateOQty(transaction, conn, jitem);
                    }
                    else
                    {
                        item.id = 0;
                        item.salesorderid = id;
                        item.oqty = jitem.oqty;
                        item.sizeid = jitem.sizeid;
                        item.size = jitem.name;
                        item.sizename = jitem.name;
                        item.rowtype = 2;
                        item.isactive = true;
                        item.price = jitem.parent.price;
                        item.pricerange = jitem.parent.pricerange;
                        item.priceupto = jitem.parent.priceupto;
                        item.addnlchargeid = jitem.parent.addnlchargeid;
                        item.mrp = jitem.parent.mrp;
                        item.fgboxid = jitem.parent.fgboxid;
                        item.soboxid = jitem.parent.soboxid;
                        item.uid = jitem.uid;
                        Insert(transaction, conn, item);
                        jitem.id = item.id;
                    }
                }
            }

            return true;
        }

        private void UpdateOQty(NpgsqlTransaction transaction,
        NpgsqlConnection conn, SalesorderitemsDetail.SizeQty item)
        {
            string query = @"Update salesorderitems set oqty=@oqty where id = @id";
            NpgsqlCommand command = GetCommand(query, conn, transaction);
            command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.oqty;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
            command.ExecuteNonQuery();
        }

        private void UpdateAdditionalInfo(NpgsqlTransaction transaction,
            NpgsqlConnection conn, SalesorderitemsDetail item)
        {
            string query = @"Update salesorderitems set mrp=@mrp,price=@price,priceupto=@priceupto,fgboxid=@fgboxid,
                            pricerange=@pricerange,addnlchargeid=@addnlchargeid,addnlcharge=@addnlcharge,soboxid=@soboxid,
                            modifiedby=@modifiedby,modifiedon=@modifiedon where id in ({0})";

            decimal price;
            if (!string.IsNullOrEmpty(item.pricerange) && item.price == 0 && item.priceupto==0)
            {
                var priceparts = item.pricerange.Split('-');
                if (decimal.TryParse(priceparts[0], out price) && price>0)
                    item.price = price;
                if (priceparts.Length == 2 && decimal.TryParse(priceparts[1], out price) && price>0)
                    item.priceupto = price;
                else
                    item.priceupto = item.price;
            }
            //else if(decimal.TryParse(item.pricerange, out price) && price > 0)
            //{
            //    item.price = 0;
            //    item.priceupto = 0;
            //}
            if (item.price == 0)
            {
                throw new Exception(item.uid + " - Price missing\r\n" + "Design/Code can't add to SO - " + item.designid + "/" + item.colourcode + "/" + item.size);
            }
            NpgsqlCommand command = GetCommand(string.Format(query, string.Join(",", item.jumpqty.ConvertAll(x => x.id))), conn, transaction);
            command.Parameters.Add("mrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.mrp;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.price;
            command.Parameters.Add("addnlcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.addnlcharge;
            command.Parameters.Add("addnlchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.addnlchargeid;
            command.Parameters.Add("soboxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.soboxid;
            command.Parameters.Add("fgboxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.fgboxid;
            command.Parameters.Add("priceupto", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.priceupto;
            command.Parameters.Add("pricerange", NpgsqlTypes.NpgsqlDbType.Varchar).Value = item.pricerange;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.ExecuteNonQuery();
        }

        public bool Delete(Salesorderitems oSalesorderitems)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oSalesorderitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oSalesorderitems);



            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Salesorderitems oSalesorderitems)
        {
            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oSalesorderitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oSalesorderitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oSalesorderitems.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long salesorderid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            int successCount = 0;
            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("salesorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = salesorderid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal string FindSOBookNo(List<long> soitemids)
        {
            string query = string.Format(@"select sobookno from salesorderitems
            inner join  salesorder on salesorder.id = salesorderitems.salesorderid
            where salesorderitems.id in ({0}) limit 1", string.Join(",", soitemids));
            using (var reader = ExecuteReader(query, new SortedList<string, object>()))
            {
                if (reader.Read() && !reader.IsDBNull(0))
                    return reader.GetString(0);
            }
            return "";
        }

        internal long FindAdditionalCharge(List<long> soitemids)
        {
            string query = string.Format(@"select addnlchargeid from salesorderitems
            inner join  salesorder on salesorder.id = salesorderitems.salesorderid
            where salesorderitems.id in ({0}) and addnlchargeid>0  limit 1", string.Join(",", soitemids));
            using (var reader = ExecuteReader(query, new SortedList<string, object>()))
            {
                if (reader.Read())
                    return reader.GetInt64(0);
            }
            return 0;
        }

        internal long FindAgent(List<long> soitemids)
        {
            string query = string.Format(@"select agentid from salesorderitems
            inner join  salesorder on salesorder.id = salesorderitems.salesorderid
            where salesorderitems.id in ({0}) and agentid>0  limit 1", string.Join(",", soitemids));
            using (var reader = ExecuteReader(query, new SortedList<string, object>()))
            {
                if (reader.Read())
                    return reader.GetInt64(0);
            }
            return 0;
        }

        internal long FindSalesmen(List<long> soitemids)
        {
            string query = string.Format(@"select salesmenid from salesorderitems
            inner join  salesorder on salesorder.id = salesorderitems.salesorderid
            where salesorderitems.id in ({0}) and salesmenid>0  limit 1", string.Join(",", soitemids));
            using (var reader = ExecuteReader(query, new SortedList<string, object>()))
            {
                if (reader.Read())
                    return reader.GetInt64(0);
            }
            return 0;
        }


        internal List<SalesorderitemsDetail> UpdatePrice(SalesorderitemsCreteria item)
        {
            if (item.id <= 0) return new List<SalesorderitemsDetail>();
            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
            var allList = Search(new SalesorderitemsCreteria() { salesorderid = item.id });
            foreach (var design in allList.GroupBy(x => x.designid))
            {
                var designinfo = mfgdesignDataService.Search(new MfgdesignCreteria() { code = design.Key, includedetail = true }).First();
                foreach (var ditem in design)
                {
                    var findex = designinfo.sizegroup.items.FindIndex(x => x.uid == ditem.uid);
                    if (findex >= 0)
                    {
                        switch (item.pricesource)
                        {
                            case 0:
                                ditem.price = designinfo.sizegroup.items[findex].salerate;  
                                break;
                            case 1:
                                ditem.price = designinfo.sizegroup.items[findex].wsrate;
                                break;
                            case 2:
                                ditem.price = designinfo.sizegroup.items[findex].dealerprice;
                                break;
                        }
                        ditem.priceupto = ditem.price;
                        ditem.pricerange = ditem.price.ToString();
                    }
                }
            }
            return allList;
        }
    }

}
