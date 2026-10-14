using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Threading.Tasks;
using System.Data.Common;
using InfyPOS.Controllers;
using System.Data;
using InfyPOS.Services;
using WebAPI.Data;


namespace InfyPOS.Data

{
    public class MfgdesignDataService : SAASData
    {
        public bool ismigration { get; set; }
        /*
         * 
        with allsizelist
as
(
select serialno,sizeid,referencelist.name from stock
inner join items on items.id = stock.itemid
inner join referencelist on referencelist.id = sizeid
group by serialno,sizeid,referencelist.name
),wrongsize as
(
select serialno from allsizelist
group by serialno
having count(*)>1
)
select barcode,serialno,sizeid,referencelist.name,itemid from stock
inner join items on items.id = stock.itemid
inner join referencelist on referencelist.id = sizeid
where serialno in (select serialno from wrongsize)
         * 
         * 
         * 
         * 
         * brand conflict
with alluid
as
(
select mfgdesign.brandid, mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true

),conflict as
(
select items.id,items.brandid,alluid.brandid abranid from stock
inner join items on items.id = itemid
inner join alluid on alluid.uid = stock.serialno
where items.brandid != alluid.brandid
	),singleconflict as
	(
	
	select id,count(*) from conflict
	group by id
	having count(*)=1)
	
	update items set brandid = abranid from singleconflict
	inner join conflict on conflict.id = singleconflict.id
	where items.id = singleconflict.id*/
        const string SQL_SizeItemQuery = @"
with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,prdhsncode,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,cast(schemelist->>'cost' as numeric) dcost,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,cast(schemelist->>'sleeveid' as bigint) sleeveid,
mfgdesign.productid,mfgdesign.brandid,cast(schemelist->>'itemid' as bigint) itemid,mfgdesign.boxid,
mfgdesign.fitid,mfgdesign.styleid,mfgdesign.materialid,mfgdesign.patternid,mfgdesign.typeid,mfgdesign.id,
schemelist->>'designitemcode' designitemcode,schemelist->>'name' pname,schemelist->>'prdhsn' prdhsn,
case
when colourlist->>'imageid' is not null then colourlist->>'imageid'
when (imagelist is not null and imagelist->>0 != '') then imagelist->>0
else '' end
imageid
from mfgdesign
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
LEFT JOIN LATERAL json_array_elements (cast(mfgdesign.sizegroup->>'colouritems' as json)) colourlist ON colourlist->>'colourid' = schemelist->>'colourid'
where mfgdesign.isactive = true and mfgdesign.code = @code
)

select alluid.* from alluid";

        const string SQL_SizeItemByCodeQuery = @"
with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,cast(schemelist->>'cost' as numeric) dcost,
schemelist->>'code' dname,cast(schemelist->>'colourid' as bigint) colourid,
cast(schemelist->>'sleeveid' as bigint) sleeveid from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where mfgdesign.isactive = true and mfgdesign.id =@id
)

select alluid.* from alluid";

        const string SQL_SyncPrice = @"with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
schemelist->>'wsrate' wsrate,schemelist->>'dealerprice' dealerprice,schemelist->>'salerate' salerate,
cast(schemelist->>'mrp' as numeric) mrp,
schemelist->>'code' dname,schemelist->>'code' dcode,cast(schemelist->>'colourid' as bigint) colourid,
cast(schemelist->>'sleeveid' as bigint) sleeveid from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where mfgdesign.isactive = true 
),conflit as 
(
select alluid.*,salablegoods.id,salablegoods.price,salablegoods.discount
from alluid 
inner join salablegoods on salablegoods.barcode  = alluid.uid
where salablegoods.price != alluid.mrp
)
update salablegoods set price = conflit.mrp from conflit
where conflit.id = salablegoods.id";

        const string SQL_Uid = @"
with alluid
as
(
select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,
schemelist->>'wsrate' wsrate,schemelist->>'dealerprice' dealerprice,schemelist->>'salerate' salerate,schemelist->>'mrp' mrp,
schemelist->>'code' dname,schemelist->>'code' dcode,cast(schemelist->>'colourid' as bigint) colourid,
cast(schemelist->>'sleeveid' as bigint) sleeveid from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where mfgdesign.isactive = true 
)
select alluid.*,c.name,s.name,salesorderitems.id,alluid.uid,salesorderitems.uid,salesorderitems.price from alluid 
INNER JOIN REFERENCELIST C ON C.ID = alluid.colourid
INNER JOIN REFERENCELIST s ON s.ID = alluid.sizeid
inner join salesorderitems on salesorderitems.designitemcode=alluid.dcode
and salesorderitems.colourid = alluid.colourid and salesorderitems.sizeid = alluid.sizeid
where alluid.uid != salesorderitems.uid";


        const string SQL_Detail = @"select mfgdesign.code,schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,schemelist->>'sizeid' sizeid,
schemelist->>'wsrate' wsrate,schemelist->>'dealerprice' dealerprice,schemelist->>'salerate' salerate,schemelist->>'mrp' mrp from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true";

        const string SQL_Search = @"select code, name, productid, brandid, sleeveid, typeid, styleid, pack,cutratio, mfgmonth, mfgyear, patternid, materialid, racknumber,prdhsncode,prddivisionid,boxid,hascombo,imagelist,
        fabricid, sizemode,displayname, classid,iscoredesign, isactive, createdon, modifiedon, version, createdby, modifiedby, id,autonumber,colourid,supplierid ,seasonid ,fitid,taxid {0} from mfgdesign where id=@id";

        const string SQL_SearchCombo = @"select code, name, productid, brandid, sleeveid, typeid, styleid, pack,cutratio, mfgmonth, mfgyear, patternid, materialid, racknumber,prdhsncode,prddivisionid,boxid,imagelist,
        fabricid, sizemode,displayname, classid,iscoredesign, isactive, createdon, modifiedon, version, createdby, modifiedby, id,autonumber,colourid,supplierid ,seasonid ,fitid,taxid,sizegroup,comboconfig from mfgdesign 
        where hascombo=true and isactive=true";

        const string SQL_SearchList = @"select id,code, name, sizegroup,fabricid,productid from mfgdesign where id=@id";

        const string SQL_Insert = @"INSERT INTO public.mfgdesign(code, name, productid, brandid, sleeveid, typeid, styleid, pack,cutratio, mfgmonth, mfgyear, patternid, materialid, fabricid, sizegroup, isactive, createdon, modifiedon, version, createdby, modifiedby,autonumber,colourid,supplierid ,seasonid ,fitid,taxid,sizemode,displayname,classid,iscoredesign,racknumber,prdhsncode,prddivisionid,activestatus,boxid,imagelist {0})
                            VALUES(@code, @name, @productid, @brandid, @sleeveid, @typeid, @styleid, @pack,@cutratio, @mfgmonth, @mfgyear, @patternid, @materialid, @fabricid, @sizegroup, @isactive, @createdon, @modifiedon, @version, @createdby, @modifiedby,@autonumber,@colourid,@supplierid ,@seasonid ,@fitid,@taxid,@sizemode,@displayname,@classid,@iscoredesign,@racknumber,@prdhsncode,@prddivisionid,@activestatus,@boxid,@imagelist {1}) RETURNING id";

        const string SQL_Update = @"UPDATE public.mfgdesign SET code=@code, name=@name, productid=@productid, brandid=@brandid, sleeveid=@sleeveid, typeid=@typeid, styleid=@styleid,cutratio=@cutratio, 
        pack=@pack, mfgmonth=@mfgmonth, mfgyear=@mfgyear, patternid=@patternid, materialid=@materialid, boxid =@boxid,imagelist=@imagelist,prdhsncode=@prdhsncode,prddivisionid=@prddivisionid,
        fabricid=@fabricid,autonumber=@autonumber,colourid=@colourid, sizegroup=@sizegroup, modifiedon=@modifiedon, version=@version+1, modifiedby=@modifiedby,racknumber=@racknumber,
        supplierid =@supplierid,seasonid=@seasonid ,fitid=@fitid,taxid=@taxid,sizemode=@sizemode,displayname=@displayname,classid=@classid,iscoredesign=@iscoredesign {0} where id=@id and version=@version RETURNING version";

        public long GetDesignID(string code, out string newuid)
        {
            newuid = null;
            var param = new SortedList<string, object>();
            param.Add("designcode", code);

            string query = @"select id 
from mfgdesign
where code = @designcode
and isactive = true";
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                    return reader.GetInt64(0);
            }


            query = @"select id 
from mfgdesign
LEFT JOIN LATERAL
json_array_elements(cast(mfgdesign.sizegroup->> 'items' as json)) schemelist ON true
where cast(schemelist ->> 'code' as varchar)= @designcode
and mfgdesign.isactive = true";
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                    return reader.GetInt64(0);
            }

            query = @"select id,mfgdesign.code, cast(schemelist ->> 'code' as varchar) dcode,
                cast(schemelist ->> 'sizeid' as bigint) sizeid
                from mfgdesign
                LEFT JOIN LATERAL
                json_array_elements(cast(mfgdesign.sizegroup->> 'items' as json)) schemelist ON true
                where cast(schemelist ->> 'uid' as varchar) = @designcode
                and mfgdesign.isactive = true";

            long id = 0;
            long sizeid = 0;
            string oldcode = "";
            string designcode = "";
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    id = reader.GetInt64(0);
                    oldcode = reader.GetString(1);
                    designcode = reader.GetString(2);
                    sizeid = reader.GetInt64(3);
                }
            }

            if (oldcode == InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_oldskudesign)
            {
                query = @"select id, cast(schemelist ->> 'uid' as varchar)
                from mfgdesign
                LEFT JOIN LATERAL
                json_array_elements(cast(mfgdesign.sizegroup->> 'items' as json)) schemelist ON true
                where cast(schemelist ->> 'code' as varchar) = @dcode
                and cast(schemelist ->> 'sizeid' as bigint) = @sizeid
                and mfgdesign.isactive = true";
                id = 0;
                param.Add("sizeid", sizeid);
                param.Add("dcode", designcode);
                using (var reader = ExecuteReader(query, param))
                {
                    if (reader.Read())
                    {
                        id = reader.GetInt64(0);
                        newuid = reader.GetString(1);
                    }
                }
            }
            return id;
        }
        public Mfgdesign.Sizegroup.Item SearchUANByName(string design, string designcode, string sizename)
        {
            string newuid = "";
            var designid = GetDesignID(design, out newuid);
            var param = new SortedList<string, object>();
            param.Add("dname", designcode);
            param.Add("sizename", sizename);
            param.Add("id", designid);
            using (var reader = ExecuteReader(SQL_SizeItemByCodeQuery + @" inner join referencelist on referencelist.id = sizeid WHERE ALLUID.dname = @dname and referencelist.name=@sizename", param))
            {
                if (reader.Read())
                {
                    Mfgdesign.Sizegroup.Item item = new Mfgdesign.Sizegroup.Item();
                    item.code = reader["code"] != DBNull.Value ? Convert.ToString(reader["code"]) : "";
                    item.uid = reader["uid"] != DBNull.Value ? Convert.ToString(reader["uid"]) : "";
                    item.name = reader["dname"] != DBNull.Value ? Convert.ToString(reader["dname"]) : "";
                    item.cost = reader["dcost"] != DBNull.Value ? Convert.ToDecimal(reader["dcost"]) : 0;
                    item.sizegroup = sizename;
                    item.colourid = reader["colourid"] != DBNull.Value ? Convert.ToInt64(reader["colourid"]) : 0;
                    item.sizeid = reader["sizeid"] != DBNull.Value ? Convert.ToInt64(reader["sizeid"]) : 0;
                    item.sleeveid = reader["sleeveid"] != DBNull.Value ? Convert.ToInt64(reader["sleeveid"]) : 0;
                    item.salerate = reader["salerate"] != DBNull.Value ? Convert.ToInt64(reader["salerate"]) : 0;
                    item.wsrate = reader["wsrate"] != DBNull.Value ? Convert.ToInt64(reader["wsrate"]) : 0;
                    item.dealerprice = reader["dealerprice"] != DBNull.Value ? Convert.ToInt64(reader["dealerprice"]) : 0;
                    item.mrp = reader["mrp"] != DBNull.Value ? Convert.ToInt64(reader["mrp"]) : 0;
                    return item;

                }
                return null;
            }
        }

        public List<Mfgdesign.Sizegroup.SearchItem> SearchUANItem(Mfgdesign.Sizegroup.SearchItem request)
        {
            var result = new List<Mfgdesign.Sizegroup.SearchItem>();
            string query = SQL_SizeItemQuery;
            var param = new SortedList<string, object>();
            param.Add("code", string.IsNullOrEmpty(request.code) ? "" : request.code);
            param.Add("uid", string.IsNullOrEmpty(request.uid) ? "" : request.uid);
            param.Add("designitemcode", string.IsNullOrEmpty(request.designitemcode) ? "" : request.designitemcode);
            param.Add("colourid", request.colourid);
            param.Add("sizeid", request.sizeid);
            param.Add("sleeveid", request.sleeveid);

            List<string> conditions = new List<string>();
            if (!string.IsNullOrEmpty(request.uid))
            {
                query = query.Replace(" and mfgdesign.code = @code", "");
                conditions.Add("uid= @uid");
            }
            else
            {
                if (string.IsNullOrEmpty(request.code))
                    query = query.Replace(" and mfgdesign.code = @code", "");
                if (request.colourid > 0)
                    conditions.Add("colourid = @colourid");
                if (request.sizeid > 0)
                    conditions.Add("sizeid = @sizeid");
                if (request.sleeveid > 0)
                    conditions.Add("sleeveid = @sleeveid");
            }

            if (!string.IsNullOrEmpty(request.designitemcode))
            {
                if (request.designitemcode.IndexOf(',') > 0)
                {
                    conditions.Add("dname in (" + String.Join(",", request.designitemcode.Split(',').ToList().ConvertAll(x => "'" + x + "'")) + ")");
                }
                else
                {
                    conditions.Add("dname = @designitemcode");
                }
            }
            if (request.uids != null && request.uids.Count > 0)
                conditions.Add("uid in (" + String.Join(",", request.uids.ConvertAll(x => "'" + x + "'")) + ")");
            if (conditions.Count > 0)
            {
                query += " WHERE " + string.Join(" AND ", conditions);
            }
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    Mfgdesign.Sizegroup.SearchItem item = new Mfgdesign.Sizegroup.SearchItem();
                    item.code = reader["code"] != DBNull.Value ? Convert.ToString(reader["code"]) : "";
                    item.uid = reader["uid"] != DBNull.Value ? Convert.ToString(reader["uid"]) : "";
                    item.name = reader["dname"] != DBNull.Value ? Convert.ToString(reader["dname"]) : "";
                    item.cost = reader["dcost"] != DBNull.Value ? Convert.ToDecimal(reader["dcost"]) : 0;

                    item.colourid = reader["colourid"] != DBNull.Value ? Convert.ToInt64(reader["colourid"]) : 0;
                    item.sizeid = reader["sizeid"] != DBNull.Value ? Convert.ToInt64(reader["sizeid"]) : 0;
                    item.sleeveid = reader["sleeveid"] != DBNull.Value ? Convert.ToInt64(reader["sleeveid"]) : 0;

                    item.fitid = reader["fitid"] != DBNull.Value ? Convert.ToInt64(reader["fitid"]) : 0;
                    item.styleid = reader["styleid"] != DBNull.Value ? Convert.ToInt64(reader["styleid"]) : 0;
                    item.materialid = reader["materialid"] != DBNull.Value ? Convert.ToInt64(reader["materialid"]) : 0;
                    item.patternid = reader["patternid"] != DBNull.Value ? Convert.ToInt64(reader["patternid"]) : 0;
                    item.typeid = reader["typeid"] != DBNull.Value ? Convert.ToInt64(reader["typeid"]) : 0;

                    item.productid = reader["productid"] != DBNull.Value ? Convert.ToInt64(reader["productid"]) : 0;
                    item.brandid = reader["brandid"] != DBNull.Value ? Convert.ToInt64(reader["brandid"]) : 0;
                    item.itemid = reader["itemid"] != DBNull.Value ? Convert.ToInt64(reader["itemid"]) : 0;

                    item.salerate = reader["salerate"] != DBNull.Value ? Convert.ToInt64(reader["salerate"]) : 0;
                    item.wsrate = reader["wsrate"] != DBNull.Value ? Convert.ToInt64(reader["wsrate"]) : 0;
                    item.dealerprice = reader["dealerprice"] != DBNull.Value ? Convert.ToInt64(reader["dealerprice"]) : 0;
                    item.mrp = reader["mrp"] != DBNull.Value ? Convert.ToInt64(reader["mrp"]) : 0;
                    result.Add(item);
                }
            }
            return result;
        }

        public Mfgdesign.Sizegroup.SearchItem SearchUANItem(string uan, string designcode)
        {
            string query = SQL_SizeItemQuery;
            var param = new SortedList<string, object>();
            param.Add("uan", uan);
            param.Add("code", string.IsNullOrEmpty(designcode) ? "" : designcode);
            if (string.IsNullOrEmpty(designcode))
                query = SQL_SizeItemQuery.Replace("and mfgdesign.code = @code", "");
            using (var reader = ExecuteReader(query + "   WHERE ALLUID.uid = @uan", param))
            {
                if (reader.Read())
                {
                    Mfgdesign.Sizegroup.SearchItem item = new Mfgdesign.Sizegroup.SearchItem();
                    item.code = reader["code"] != DBNull.Value ? Convert.ToString(reader["code"]) : "";
                    item.uid = reader["uid"] != DBNull.Value ? Convert.ToString(reader["uid"]) : "";
                    item.name = reader["dname"] != DBNull.Value ? Convert.ToString(reader["dname"]) : "";

                    item.cost = reader["dcost"] != DBNull.Value ? Convert.ToDecimal(reader["dcost"]) : 0;

                    item.boxid = reader["boxid"] != DBNull.Value ? Convert.ToInt64(reader["boxid"]) : 0;

                    item.colourid = reader["colourid"] != DBNull.Value ? Convert.ToInt64(reader["colourid"]) : 0;
                    item.sizeid = reader["sizeid"] != DBNull.Value ? Convert.ToInt64(reader["sizeid"]) : 0;
                    item.sleeveid = reader["sleeveid"] != DBNull.Value ? Convert.ToInt64(reader["sleeveid"]) : 0;
                    item.productid = reader["productid"] != DBNull.Value ? Convert.ToInt64(reader["productid"]) : 0;
                    item.brandid = reader["brandid"] != DBNull.Value ? Convert.ToInt64(reader["brandid"]) : 0;
                    item.itemid = reader["itemid"] != DBNull.Value ? Convert.ToInt64(reader["itemid"]) : 0;

                    item.salerate = reader["salerate"] != DBNull.Value ? Convert.ToInt64(reader["salerate"]) : 0;
                    item.wsrate = reader["wsrate"] != DBNull.Value ? Convert.ToInt64(reader["wsrate"]) : 0;
                    item.dealerprice = reader["dealerprice"] != DBNull.Value ? Convert.ToInt64(reader["dealerprice"]) : 0;
                    item.mrp = reader["mrp"] != DBNull.Value ? Convert.ToInt64(reader["mrp"]) : 0;
                    return item;

                }
                return null;
            }
        }

        public SortedDictionary<string, string> SearchRackNumber(List<string> uan)
        {
            SortedDictionary<string, string> results = new SortedDictionary<string, string>();
            List<Mfgdesign.Sizegroup.SearchItem> items = new List<Mfgdesign.Sizegroup.SearchItem>();

            string query = @"
            select mfgdesign.racknumber,schemelist->>'uid' uid
            from mfgdesign
            LEFT JOIN LATERAL
            json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
            where mfgdesign.isactive = true and schemelist->>'uid' in (" + string.Join(",",uan.ConvertAll(x=>"'"+x+"'")) +")";

            var param = new SortedList<string, object>();
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    var uid = reader["uid"] != DBNull.Value ? Convert.ToString(reader["uid"]) : "";
                    var racknumber = reader["racknumber"] != DBNull.Value ? Convert.ToString(reader["racknumber"]) : "";
                    if (!results.ContainsKey(uid))
                        results.Add(uid, racknumber);
                }
                return results;
            }
        }

        public List<Mfgdesign.Sizegroup.SearchItem> SearchUANItem(List<string> uan)
        {
            List<Mfgdesign.Sizegroup.SearchItem> items = new List<Mfgdesign.Sizegroup.SearchItem>();
            string query = SQL_SizeItemQuery;
            var param = new SortedList<string, object>();
            query = SQL_SizeItemQuery.Replace("and mfgdesign.code = @code", "");
            using (var reader = ExecuteReader(query + "   WHERE ALLUID.uid in (" + String.Join(",", uan.ConvertAll(x => "'" + x + "'")) + ")", param))
            {
                while (reader.Read())
                {
                    Mfgdesign.Sizegroup.SearchItem item = new Mfgdesign.Sizegroup.SearchItem();
                    item.designitemcode = reader["designitemcode"] != DBNull.Value ? Convert.ToString(reader["designitemcode"]) : "";
                    item.code = reader["code"] != DBNull.Value ? Convert.ToString(reader["code"]) : "";
                    item.uid = reader["uid"] != DBNull.Value ? Convert.ToString(reader["uid"]) : "";
                    item.name = reader["dname"] != DBNull.Value ? Convert.ToString(reader["dname"]) : "";
                    item.pname = reader["pname"] != DBNull.Value ? Convert.ToString(reader["pname"]) : "";
                    item.hsn = reader["prdhsncode"] != DBNull.Value ? Convert.ToString(reader["prdhsncode"]) : "";
                    if (item.name.IndexOf('/') > 0)
                    {
                        item.colourcode = item.name.Split('/')[1];
                    }
                    if (reader["prdhsn"] != DBNull.Value && reader["prdhsn"].ToString() != "")
                        item.hsn = reader["prdhsn"].ToString();
                    item.img = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                    item.cost = reader["dcost"] != DBNull.Value ? Convert.ToDecimal(reader["dcost"]) : 0;
                    item.colourid = reader["colourid"] != DBNull.Value ? Convert.ToInt64(reader["colourid"]) : 0;
                    item.sizeid = reader["sizeid"] != DBNull.Value ? Convert.ToInt64(reader["sizeid"]) : 0;
                    item.sleeveid = reader["sleeveid"] != DBNull.Value ? Convert.ToInt64(reader["sleeveid"]) : 0;
                    item.productid = reader["productid"] != DBNull.Value ? Convert.ToInt64(reader["productid"]) : 0;
                    item.brandid = reader["brandid"] != DBNull.Value ? Convert.ToInt64(reader["brandid"]) : 0;
                    item.itemid = reader["itemid"] != DBNull.Value ? Convert.ToInt64(reader["itemid"]) : 0;
                    item.id = reader["id"] != DBNull.Value ? Convert.ToInt64(reader["id"]) : 0;
                    item.salerate = reader["salerate"] != DBNull.Value ? Convert.ToDecimal(reader["salerate"]) : 0;
                    item.boxid = reader["boxid"] != DBNull.Value ? Convert.ToInt64(reader["boxid"]) : 0;
                    item.wsrate = reader["wsrate"] != DBNull.Value ? Convert.ToDecimal(reader["wsrate"]) : 0;
                    item.dealerprice = reader["dealerprice"] != DBNull.Value ? Convert.ToDecimal(reader["dealerprice"]) : 0;
                    item.mrp = reader["mrp"] != DBNull.Value ? Convert.ToDecimal(reader["mrp"]) : 0;
                    items.Add(item);
                }
                return items;
            }


        }

        internal List<ComboConfig.ComboConfiguration> GetAllComboDesigns()
        {
            string query = @"select schemelist->>'productid' productid,
schemelist->>'designlist' DesignList,
mfgdesign.name,mfgdesign.id from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.comboconfig->>'configlist' as json)) schemelist ON true
where hascombo=true and mfgdesign.isactive=true";

            List<ComboConfig.ComboConfiguration> lst = new List<ComboConfig.ComboConfiguration>();
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    ComboConfig.ComboConfiguration item = new ComboConfig.ComboConfiguration();
                    item.DesignList = reader["DesignList"] != DBNull.Value ? Convert.ToString(reader["DesignList"]) : "";
                    item.mfgdesignname = reader["name"] != DBNull.Value ? Convert.ToString(reader["name"]) : "";
                    item.Productid = reader["productid"] != DBNull.Value ? Convert.ToInt64(reader["productid"]) : 0;
                    item.mfgdesignid = reader["id"] != DBNull.Value ? Convert.ToInt64(reader["id"]) : 0;
                    foreach (var itm in item.DesignList.Split(','))
                    {
                        lst.Add(new ComboConfig.ComboConfiguration()
                        {
                            DesignList = itm,
                            mfgdesignid = item.mfgdesignid,
                            Productid = item.Productid,
                            mfgdesignname = item.mfgdesignname,
                        });
                    }
                }
            }
            return lst;
        }

        public List<Mfgdesign.Sizegroup.Item> SearchUANItems()
        {
            string query = SQL_SizeItemQuery.Replace("and mfgdesign.code = @code", "");

            List<Mfgdesign.Sizegroup.Item> lst = new List<Mfgdesign.Sizegroup.Item>();
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    Mfgdesign.Sizegroup.Item item = new Mfgdesign.Sizegroup.Item();
                    item.code = reader["code"] != DBNull.Value ? Convert.ToString(reader["code"]) : "";
                    item.uid = reader["uid"] != DBNull.Value ? Convert.ToString(reader["uid"]) : "";
                    item.name = reader["dname"] != DBNull.Value ? Convert.ToString(reader["dname"]) : "";
                    item.hsn = reader["prdhsncode"] != DBNull.Value ? Convert.ToString(reader["prdhsncode"]) : "";
                    if (reader["prdhsn"] != DBNull.Value && reader["prdhsn"].ToString() != "")
                        item.hsn = reader["prdhsn"].ToString();

                    item.pname = reader["pname"] != DBNull.Value ? Convert.ToString(reader["pname"]) : "";
                    item.cost = reader["dcost"] != DBNull.Value ? Convert.ToDecimal(reader["dcost"]) : 0;
                    item.colourid = reader["colourid"] != DBNull.Value ? Convert.ToInt64(reader["colourid"]) : 0;
                    item.sizeid = reader["sizeid"] != DBNull.Value ? Convert.ToInt64(reader["sizeid"]) : 0;
                    item.itemid = reader["itemid"] != DBNull.Value ? Convert.ToInt64(reader["itemid"]) : 0;
                    item.productid = reader["productid"] != DBNull.Value ? Convert.ToInt64(reader["productid"]) : 0;
                    item.sleeveid = reader["sleeveid"] != DBNull.Value ? Convert.ToInt64(reader["sleeveid"]) : 0;
                    item.salerate = reader["salerate"] != DBNull.Value ? Convert.ToInt64(reader["salerate"]) : 0;
                    item.wsrate = reader["wsrate"] != DBNull.Value ? Convert.ToInt64(reader["wsrate"]) : 0;
                    item.dealerprice = reader["dealerprice"] != DBNull.Value ? Convert.ToInt64(reader["dealerprice"]) : 0;
                    item.mrp = reader["mrp"] != DBNull.Value ? Convert.ToInt64(reader["mrp"]) : 0;
                    lst.Add(item);
                }
            }
            return lst;
        }
        internal MFGDesinInfo SearchInfo(string name, bool showall, List<long> stocklocations)
        {
            string newuid = "";
            long designid = GetDesignID(name, out newuid);

            if (designid == 0)
            {
                return null;
            }
            if (!string.IsNullOrEmpty(newuid))
                name = newuid;

            List<Mfgdesign> lst = Search(new MfgdesignCreteria()
            {
                id = designid
            });

            if (lst.Count == 0)
            {
                return null;
            }
            if (!showall)
            {
                if (lst.First().name != name &&
                    lst.First().sizegroup != null &&
                    lst.First().sizegroup.items != null &&
                    lst.First().sizegroup.items.Exists(x => x.uid == name))
                {
                    var sizegroupitem = lst.First().sizegroup.items.Find(x => x.uid == name);
                    if (sizegroupitem.colourid > 0)
                    {
                        lst.First().sizegroup.items.RemoveAll(x =>
                        x.colourid != sizegroupitem.colourid || x.salerate != sizegroupitem.salerate);
                    }
                }

                if (lst.First().name != name &&
        lst.First().sizegroup != null &&
        lst.First().sizegroup.items != null &&
        lst.First().sizegroup.items.Exists(x => x.code == name))
                {
                    var sizegroupitem = lst.First().sizegroup.items.Find(x => x.code.Trim().ToUpper() == name.Trim().ToUpper());
                    if (sizegroupitem.colourid > 0)
                    {
                        lst.First().sizegroup.items.RemoveAll(x =>
                        x.colourid != sizegroupitem.colourid || x.code.Trim().ToUpper() != name.Trim().ToUpper());
                    }
                }
            }
            var design = lst.OrderByDescending(e => e.id).ToList().First();

            //if (design.sizegroup != null && design.sizegroup.items != null &&
            //    design.sizegroup.items.Exists(x => !x.alloworder))
            //{
            //    design.sizegroup.items.RemoveAll(x => !x.alloworder);
            //}

            MFGDesinInfo mfgdesigninfo = new MFGDesinInfo();
            mfgdesigninfo.iscoredesign = lst.First().iscoredesign;
            mfgdesigninfo.design = design.name;
            mfgdesigninfo.designid = design.id;
            mfgdesigninfo.fabricid = design.fabricid;
            using (var reader = ExecuteReader("Select name from products where id=" + design.productid, null))
            {
                if (reader.Read())
                    mfgdesigninfo.product = reader.GetString(0);
            }
            using (var reader = ExecuteReader("Select name from brand where id=" + design.brandid, null))
            {
                if (reader.Read())
                    mfgdesigninfo.brand = reader.GetString(0);
            }

            mfgdesigninfo.productid = design.productid;
            mfgdesigninfo.brandid = design.brandid;
            mfgdesigninfo.materialid = design.materialid;
            mfgdesigninfo.styleid = design.styleid;
            mfgdesigninfo.patternid = design.patternid;
            mfgdesigninfo.typeid = design.typeid;
            mfgdesigninfo.boxid = design.boxid;
            mfgdesigninfo.sleeveid = design.sleeveid;
            mfgdesigninfo.fitid = design.fitid;
            mfgdesigninfo.sizelist = new List<Tuple<string, List<string>>>();
            mfgdesigninfo.sizegroup = new List<MFGDesinInfo.SizeGroup>();
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };

            if (design.sizegroup != null && design.sizegroup.items != null && design.sizegroup.items.Count > 0)
            {
                var sizelist = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    ids = design.sizegroup.items.ConvertAll(e => e.sizeid).Distinct().ToList()
                });

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist != null &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist != "")
                {
                    var sizecolumns = new List<string>(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist.Split(','));
                    sizelist = sizelist.OrderBy(x => sizecolumns.IndexOf(x.name)).ToList();
                }

                string grp = string.Join("-", sizelist.ConvertAll(x => x.name));
                design.sizegroup.items.ForEach(x => x.sizegroup = grp);
                mfgdesigninfo.rate = design.sizegroup.items.First().dealerprice;
                mfgdesigninfo.salerate = design.sizegroup.items.First().salerate;
                mfgdesigninfo.mrp = design.sizegroup.items.First().mrp;
                mfgdesigninfo.wsrate = design.sizegroup.items.First().wsrate;
                mfgdesigninfo.sizegroupid = design.sizegroup.items.First().sizegroup;
                //List<Mfgdesign.Sizegroup.Item> dlist = new List<Mfgdesign.Sizegroup.Item>();

                //foreach (var duplicate in dlist)
                //    design.sizegroup.items.Remove(duplicate);

                var colourlist = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    ids = design.sizegroup.items.ConvertAll(e => e.colourid).Distinct().ToList()
                });
                mfgdesigninfo.colourlist = new List<MFGDesinInfo.ColourInfo>();
                if (!showall)
                {
                    if (design.sizegroup.items.Exists(x => x.code == name))
                    {
                        design.sizegroup.items.RemoveAll(x => x.code != name);
                    }
                }
                foreach (var item in design.sizegroup.items.GroupBy(x => x.code))
                {
                    var code = item.Key;
                    if (item.Key.IndexOf('/') > 0)
                        code = code.Substring(item.Key.IndexOf('/') + 1);
                    mfgdesigninfo.colourlist.Add(new MFGDesinInfo.ColourInfo()
                    {
                        id = item.First().colourid,
                        designcode = item.Key,
                        code = code,
                        name = colourlist.Exists(e => e.id == item.First().colourid) ?
                        colourlist.Find(e => e.id == item.First().colourid).name : ""
                    });
                }
                //if (mfgdesigninfo.fabricid > 0 && mfgdesigninfo.colourlist != null &&
                //    mfgdesigninfo.colourlist.Count > 0)
                //{

                //    //BOMMasterDataService bOMMasterDataService = new BOMMasterDataService() { Context = this.Context };
                //    //var fabrics = bOMMasterDataService.Search(new BOMMasterCreteria() { id = mfgdesigninfo.fabricid });
                //    //if (fabrics.Count > 0)
                //    //{
                //    //    if (fabrics[0].colourlist != null && fabrics[0].colourlist.Count > 0)
                //    //    {
                //    //        foreach (var colour in fabrics[0].colourlist)
                //    //        {
                //    //            if (mfgdesigninfo.colourlist.Exists(x => x.id == colour.id))
                //    //            {
                //    //                if (colour.name.IndexOf('/') > 0)
                //    //                {
                //    //                    var tcolour = colour.name.Split('/')[1];
                //    //                    if (tcolour.IndexOf('(') > 0)
                //    //                    {
                //    //                        tcolour = tcolour.Substring(0, tcolour.IndexOf('('));
                //    //                    }
                //    //                    mfgdesigninfo.colourlist.Find(x => x.id == colour.id).code = tcolour;
                //    //                }
                //    //                else
                //    //                {
                //    //                    var tcolour = colour.name;
                //    //                    if (tcolour.IndexOf('(') > 0)
                //    //                    {
                //    //                        tcolour = tcolour.Substring(0, tcolour.IndexOf('('));
                //    //                    }
                //    //                    mfgdesigninfo.colourlist.Find(x => x.id == colour.id).code = tcolour;
                //    //                }
                //    //            }
                //    //        }
                //    //    }
                //    //}
                //}

                if (mfgdesigninfo.colourlist != null && mfgdesigninfo.colourlist.Count == 1)
                    mfgdesigninfo.colourid = mfgdesigninfo.colourlist[0].id;



                bool hassaleratediff = false;
                bool hascolourdiff = design.sizegroup.items.ConvertAll(x => x.colourid).Distinct().Count() > 1;
                foreach (var sizegroup in design.sizegroup.items.GroupBy(e =>
                new { e.code, e.sizegroup, e.colourid }))
                {
                    var sizegroupname = sizegroup.Key.code + '-' + sizegroup.Key.sizegroup;
                    if (string.IsNullOrEmpty(sizegroupname))
                    {
                        sizegroupname = string.Join("-", sizegroup.ToList().ConvertAll(e =>
                           {
                               return sizelist.Find(x => x.id == e.sizeid).name;
                           }).ToArray());
                        if (hascolourdiff)
                            sizegroupname += "(" + mfgdesigninfo.colourlist.Find(e => e.id == sizegroup.Key.colourid).name + ")";
                    };
                    var item = new Tuple<string, List<string>>(sizegroupname, new List<string>());
                    var sitem = new List<MFGDesinInfo.SizeItem>();

                    foreach (var size in sizegroup)
                    {
                        if (sizelist.Exists(e => e.id == size.sizeid))
                        {
                            if (!sitem.Exists(e => e.id == size.sizeid))
                            {
                                var sizename = sizelist.Find(e => e.id == size.sizeid).name;
                                if (!item.Item2.Contains(sizename))
                                    item.Item2.Add(sizename);
                                sitem.Add(new MFGDesinInfo.SizeItem()
                                {
                                    id = size.sizeid,
                                    uid = size.uid,
                                    name = sizename,
                                    colourid = size.colourid,
                                    rate = size.dealerprice,
                                    wsrate = size.wsrate,
                                    mrp = size.mrp,
                                    salerate = size.salerate,
                                });
                            }
                        }
                    }
                    if (item.Item2.Count > 0)
                    {
                        if (mfgdesigninfo.sizegroup.Exists(x => x.name == sizegroupname))
                            hassaleratediff = true;

                        var colourshade = sizegroup.First().code.Replace(mfgdesigninfo.design + "/", "");
                        colourshade = colourshade.Replace(mfgdesigninfo.design, "");

                        mfgdesigninfo.sizegroup.Add(new MFGDesinInfo.SizeGroup()
                        {
                            id = sizegroupname,
                            name = sizegroupname,
                            designcode = sizegroup.First().code,
                            colourshade = colourshade,
                            colourcode = sizegroup.First().code.IndexOf('/') > 0 ?
                            sizegroup.First().code.Substring(sizegroup.First().code.IndexOf('/') + 1) : "",
                            rate = sizegroup.First().dealerprice,
                            wsrate = sizegroup.First().wsrate,
                            mrp = sizegroup.First().mrp,
                            colourid = sizegroup.First().colourid,
                            salerate = sizegroup.First().salerate,
                            list = sitem,
                            diffprice = sitem.GroupBy(x => new { x.rate, x.salerate, x.wsrate, x.mrp }).Count() > 1
                        });
                        mfgdesigninfo.sizelist.Add(item);
                    }
                }

                if (hassaleratediff)
                {
                    mfgdesigninfo.sizegroup.ForEach(e =>
                    {
                        e.name += "-" + e.rate.ToString("f0");
                    });
                }
                mfgdesigninfo.sizegroupid = mfgdesigninfo.sizegroup.First().name;

                if (design.code != name)
                {
                    Mfgdesign.Sizegroup.Item item = null;
                    if (design.sizegroup.items.Exists(e => e.code == name))
                    {
                        item = design.sizegroup.items.Find(e => e.code == name);
                        mfgdesigninfo.sizegroupid = item.sizegroup;
                        mfgdesigninfo.colourid = item.colourid;
                        mfgdesigninfo.designname = item.code;
                    }
                    else if (design.sizegroup.items.Exists(e => e.uid == name))
                    {
                        item = design.sizegroup.items.Find(e => e.uid == name);
                        mfgdesigninfo.sizeid = item.sizeid;
                    }
                }

            }

            if (string.IsNullOrEmpty(mfgdesigninfo.designname))
            {
                List<string> duplicatenames = new List<string>();
                foreach (var duplicate in design.sizegroup.items.GroupBy(e => e.colourid))
                {
                    if (duplicate.GroupBy(x => x.code).Count() > 1)
                    {
                        duplicatenames.AddRange(duplicate.GroupBy(x => x.code).ToList().ConvertAll(x => x.First().name));
                    }
                }
                if (duplicatenames.Contains(name))
                {
                    string availablenames = string.Join(",", duplicatenames);
                    throw new Exception("Multiple option in same colour - " + availablenames);
                }
            }

            if (stocklocations != null)
            {
                UpdateStock(0, mfgdesigninfo.sizegroup, stocklocations, false,
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(stocklocations[0]) &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[stocklocations[0]].mfg_so_fs);
            }
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist != "")
            {
                var sizecolumns = new List<string>(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_possizelist.Split(','));
                foreach (var sg in mfgdesigninfo.sizegroup)
                {
                    sg.list = sg.list.OrderBy(x => sizecolumns.IndexOf(x.name)).ToList();
                }
            }
            mfgdesigninfo.colourlist.ForEach(x =>
            {
                x.displaycode = x.designcode.Replace(mfgdesigninfo.design + "/", "");
                x.displaycode = x.displaycode.Replace(mfgdesigninfo.design, "");
                //if (x.displaycode.IndexOf("/") >= 0 && !x.displaycode.EndsWith("/"))
                //{
                //    x.displaycode = x.displaycode.Substring(x.displaycode.IndexOf("/") + 1);
                //}
            });


            return mfgdesigninfo;
        }

        internal bool UpdateStatus(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgdesign mfgdesign)
        {
            NpgsqlCommand command = GetCommand("update mfgdesign set isactive=@isactive,activestatus=@activestatus,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id", conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = mfgdesign.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("activestatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = mfgdesign.activestatus;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = mfgdesign.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        internal void UpdateImageList(MfgdesignCode mfgdesignCode)
        {
            const string SQL_UpdateImage = @"update public.mfgdesign set imagelist=@imagelist where id=@id";

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_UpdateImage, conn);
                    command.Parameters.Add("imagelist", NpgsqlTypes.NpgsqlDbType.Json).Value = mfgdesignCode.imagelist_json;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = mfgdesignCode.id;
                    int successCount = command.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,
                    mfgdesignCode);
                throw exp;
            }
        }

        public void UpdateStock(long soid, List<MFGDesinInfo.SizeGroup> sizegroup, List<long> stocklocation, bool includewip, bool includeallstocklocation)
        {
            if (sizegroup == null) return;
            if (stocklocation == null)
            {
                stocklocation = new List<long>();
                stocklocation.Add(0);
            }

            if (includeallstocklocation)
            {
                foreach (var lc in InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ToList())
                {
                    if (lc.Value.mfg_so_retail)
                    {
                        stocklocation.Add(lc.Key);
                    }
                }
            }

            var uids = new List<string>();
            sizegroup.ForEach(e =>
            {
                uids.AddRange(e.list.ConvertAll(x => "'" + x.uid + "'"));
            });
            string query = UpdateQueryForHyper(string.Format(@"select sum(iqty-(oqty+dqty+jqty)) stock,stock.serialno serialno from salablegoods
            inner join stock on stock.id = salablegoods.stockid
            where stock.serialno in ({0}) and salablegoods.locationid in ({1}) and salablegoods.isactive=true and stock.isactive=true and iqty-(oqty+dqty+jqty)>0
            group by stock.serialno", string.Join(",", uids), string.Join(",", stocklocation)));

            //query = query.Replace("and salablegoods.locationid in (0)", "");
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    var stock = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    var uid = reader.IsDBNull(0) ? "" : reader.GetString(1);
                    var sg = sizegroup.Find(x => x.list.Exists(y => y.uid == uid));
                    sg.list.Find(y => y.uid == uid).stock += stock;
                }
            }

            if (InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.mfg_whstock)
            {
                query = string.Format(@"select sum(stock.qty - (stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)) stock,stock.serialno serialno from stock
                where stock.serialno in ({0}) and stock.isactive=true and stock.transfered+stock.returnqty+stock.damagedqty+stock.journalqty)>0
                group by stock.serialno", string.Join(",", uids));
                using (var reader = ExecuteReader(query, null))
                {
                    while (reader.Read())
                    {
                        var stock = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                        var uid = reader.IsDBNull(0) ? "" : reader.GetString(1);
                        var sg = sizegroup.Find(x => x.list.Exists(y => y.uid == uid));
                        sg.list.Find(y => y.uid == uid).stock += stock;
                    }
                }
            }

            query = string.Format(@"select sum(case when qty-delivered>0 then qty-delivered else 0 end) wip,uid from Mfgworkorderitemsizes 
            inner join mfgworkorderitems on mfgworkorderitems.id = Mfgworkorderitemsizes.mfgworkorderitemsid
            inner join mfgworkorder on mfgworkorder.id = mfgworkorderitems.mfgworkorderid
            where Mfgworkorderitemsizes.isactive = true and mfgworkorderitems.isactive = true
            and mfgworkorder.isactive = true and mfgworkorder.statusid != 9
            and uid in ({0}) 
            group by uid", string.Join(",", uids));
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    var wip = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    var uid = reader.IsDBNull(0) ? "" : reader.GetString(1);
                    var sg = sizegroup.Find(x => x.list.Exists(y => y.uid == uid));
                    sg.list.Find(y => y.uid == uid).wip += wip;
                }
            }

            query = string.Format(@"select sum(oqty-(bqty+rqty+cqty)) poqty,uid from salesorderitems
            inner join salesorder on salesorder.id = salesorderitems.salesorderid 
            where 
            salesorderitems.hascompleted=false and salesorderitems.isactive = true and salesorder.isactive=true
            and salesorder.status<2 and salesorder.id != {2}
            and oqty-(bqty+rqty+cqty)>0 and uid in ({0}) 
            and salesorder.deliverylocationid in ({1}) 
            group by uid", string.Join(",", uids), string.Join(",", stocklocation), soid);
            query = query.Replace("and salesorder.deliverylocationid in (0)", "");
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    var po = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    var uid = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    var sg = sizegroup.Find(x => x.list.Exists(y => y.uid == uid));
                    sg.list.Find(y => y.uid == uid).order += po;
                }
            }

            if (includewip)
            {
                query = string.Format(@" with alljobissued as
            (
            select Mfgjoborderitems.mfgworkorderid,designitemname,mfgjoborder.id,
                sum(oqty) issuedqty,uid  from Mfgjoborderitems 
                inner join mfgworkorder on  mfgworkorder.id =Mfgjoborderitems.mfgworkorderid
	            inner join mfgworkorderitemsizes on mfgworkorderitemsizes.id = mfgworkorderitemsizesid
                left outer join Mfgworkorderitems on Mfgworkorderitems.id = Mfgjoborderitems.mfgworkorderitemsid
                left outer join mfgjoborder on mfgjoborder.id = mfgjoborderid
                where inwardjob = false and mfgjoborder.inwardgoods=true and Mfgjoborderitems.isactive=true 
                and mfgworkorder.statusid != 9
                and uid in ({0})
                group by Mfgjoborderitems.mfgworkorderid,designitemname,uid,mfgjoborder.id
            ),alljobreceived as
            (
            select Mfgjoborderitems.mfgworkorderid,designitemname,mfgjoborder.id,
               sum(oqty) receivedqty,uid  from Mfgjoborderitems 
               inner join mfgworkorder on  mfgworkorder.id =Mfgjoborderitems.mfgworkorderid
	           inner join mfgworkorderitemsizes on mfgworkorderitemsizes.id = mfgworkorderitemsizesid
               left outer join Mfgworkorderitems on Mfgworkorderitems.id = Mfgjoborderitems.mfgworkorderitemsid
               left outer join mfgjoborder on mfgjoborder.id = mfgjoborderid
               where inwardjob = true  and mfgjoborder.inwardgoods=true and Mfgjoborderitems.isactive=true
               and mfgworkorder.statusid != 9
               and uid in ({0})
               group by Mfgjoborderitems.mfgworkorderid,designitemname,uid,mfgjoborder.id
            )

            select sum(issuedqty-coalesce(alljobreceived.receivedqty,0)),alljobissued.uid
            from alljobissued
            inner join mfgworkorder on  mfgworkorder.id =alljobissued.mfgworkorderid
            inner join mfgjoborder on  mfgjoborder.id =alljobissued.id
            left outer join alljobreceived on alljobreceived.mfgworkorderid = alljobissued.mfgworkorderid
            and alljobreceived.id = alljobissued.id and alljobreceived.uid = alljobissued.uid
            where mfgjoborder.isactive=true 
            and coalesce(alljobreceived.receivedqty,0)<issuedqty
			group by alljobissued.uid ", string.Join(",", uids));
                using (var reader = ExecuteReader(query, null))
                {
                    while (reader.Read())
                    {
                        var wip = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                        var uid = reader.IsDBNull(0) ? "" : reader.GetString(1);
                        var sg = sizegroup.Find(x => x.list.Exists(y => y.uid == uid));
                        sg.list.Find(y => y.uid == uid).wip += wip;
                    }
                }
            }
        }

        public string NextAutoNumber()
        {

            string result = "";
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get("mfgdesign.autonumber", 0);
                    result = sequence.GetNextNumber(Context, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "NextAutoNumber");
                throw exp;
            }
            return result;
        }

        internal List<MfgdesignCode> SearchList(MfgdesignCreteria mfgdesignCreteria)
        {
            List<MfgdesignCode> lst = new List<MfgdesignCode>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = "select id,code,name from mfgdesign";
                qb.AddParameter("isactive", true, "=");
                if (mfgdesignCreteria.fabricid > 0)
                    qb.AddParameter("fabricid", mfgdesignCreteria.fabricid, "=");

                if (mfgdesignCreteria.ids != null && mfgdesignCreteria.ids.Count > 0)
                    qb.AddInParamer("id", string.Join(",", mfgdesignCreteria.ids));

                if (mfgdesignCreteria.codes != null && mfgdesignCreteria.codes.Count > 0)
                    qb.AddInParamer("upper(mfgdesign.code)", string.Join(",", mfgdesignCreteria.codes.ConvertAll(x => "'" + WebAPI.Data.Utility.SafeSQL(x.ToUpper()) + "'")));

                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_mfgdesign)
                {
                    qb.AddParameter("activestatus", 2, "=");
                }

                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MfgdesignCode omfgdesign = new MfgdesignCode();
                        omfgdesign.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        omfgdesign.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        omfgdesign.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        lst.Add(omfgdesign);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, mfgdesignCreteria);
            }

            return lst;
        }

        const string SQL_Delete = @"update public.mfgdesign set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<MfgdesignCode> SearchDesignList(MfgdesignCreteria _mfgdesign)
        {
            List<MfgdesignCode> lst = new List<MfgdesignCode>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgdesign.id != 0)
                {
                    command = new NpgsqlCommand(SQL_SearchList, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgdesign.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_SearchList.Substring(0, SQL_SearchList.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_mfgdesign)
                    {
                        qb.AddParameter("activestatus", 2, "=");
                    }
                    if (_mfgdesign.fabricid > 0)
                        qb.AddParameter("fabricid", _mfgdesign.fabricid, "=");

                    if (_mfgdesign.productid > 0)
                        qb.AddParameter("productid", _mfgdesign.productid, "=");

                    if (_mfgdesign.codes != null && _mfgdesign.codes.Count > 0)
                        qb.AddInParamer("mfgdesign.code", string.Join(",", _mfgdesign.codes));

                    if (!string.IsNullOrEmpty(_mfgdesign.code))
                    {
                        if (_mfgdesign.code.IndexOf('%') >= 0)
                            qb.AddParameter("code", _mfgdesign.code, " ilike ");
                        else
                            qb.AddParameter("code", _mfgdesign.code, "=");
                    }

                    if (!string.IsNullOrEmpty(_mfgdesign.name))
                    {
                        if (_mfgdesign.name.IndexOf('%') >= 0)
                            qb.AddParameter("name", _mfgdesign.name, " ilike ");
                        else
                            qb.AddParameter("name", _mfgdesign.name, "=");
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MfgdesignCode omfgdesign = new MfgdesignCode();
                        omfgdesign.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        omfgdesign.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        omfgdesign.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        if (!_mfgdesign.onlydesign)
                        {
                            omfgdesign.sizegroup_json = reader["sizegroup"] == DBNull.Value ? null : reader["sizegroup"].ToString();
                        }
                        else
                        {
                            omfgdesign.fabricid = reader["fabricid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fabricid"]);
                            omfgdesign.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        }
                        lst.Add(omfgdesign);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgdesign);
            }
            return lst;
        }

        public List<Mfgdesign> SearchCombo(MfgdesignCreteria _mfgdesign)
        {
            List<Mfgdesign> lst = new List<Mfgdesign>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchCombo;

                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_mfgdesign)
                {
                    qb.AddParameter("activestatus", 2, "=");
                }

                if (!string.IsNullOrEmpty(_mfgdesign.code))
                {
                    if (_mfgdesign.code.IndexOf('%') >= 0)
                        qb.AddParameter("code", _mfgdesign.code, " ilike ");
                    else
                        qb.AddParameter("code", _mfgdesign.code, "=");
                }

                if (!string.IsNullOrEmpty(_mfgdesign.name))
                {
                    if (_mfgdesign.name.IndexOf('%') >= 0)
                        qb.AddParameter("name", _mfgdesign.name, " ilike ");
                    else
                        qb.AddParameter("name", _mfgdesign.name, "=");
                }


                if (_mfgdesign.codes != null && _mfgdesign.codes.Count > 0)
                    qb.AddInParamer("code", string.Join(",", _mfgdesign.codes.ConvertAll(x => "'" + x + "'")));
                if (_mfgdesign.fabricid > 0)
                    qb.AddParameter("fabricid", _mfgdesign.fabricid, "=");

                if (_mfgdesign.productid > 0)
                    qb.AddParameter("productid", _mfgdesign.productid, "=");
                if (_mfgdesign.supplierid > 0)
                    qb.AddParameter("supplierid", _mfgdesign.supplierid, "=");

                if (_mfgdesign.fromdate.Year > 1900)
                    qb.AddParameter("mfgdesign.createdon", "fromdate", _mfgdesign.fromdate.Date, ">=");
                if (_mfgdesign.todate.Year > 1900)
                    qb.AddParameter("mfgdesign.createdon", "todate", _mfgdesign.todate.Date.AddDays(1), "<");

                if (!string.IsNullOrEmpty(_mfgdesign.uancode))
                {
                    var _addCondition = @"mfgdesign.id in (SELECT mfgdesign.id FROM mfgdesign LEFT JOIN LATERAL 
                        json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                        WHERE mfgdesign.isactive=true and schemelist->>'uid' in ({0}))";

                    _mfgdesign.codes = new List<string>();
                    _mfgdesign.codes.AddRange(_mfgdesign.uancode.Split(','));

                    qb.Add2Query(string.Format(_addCondition, string.Join(",", _mfgdesign.codes.ConvertAll(x => "'" + x + "'"))));
                }
                command = qb.GetCommand(conn);


                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var colourlist = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "COLOUR"
                });

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgdesign omfgdesign = new Mfgdesign();
                        omfgdesign.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        omfgdesign.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        omfgdesign.displayname = reader["displayname"] == DBNull.Value ? "" : reader["displayname"].ToString();
                        omfgdesign.racknumber = reader["racknumber"] == DBNull.Value ? "" : reader["racknumber"].ToString();
                        omfgdesign.prdhsncode = reader["prdhsncode"] == DBNull.Value ? "" : reader["prdhsncode"].ToString();
                        omfgdesign.prddivisionid = reader["prddivisionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prddivisionid"]);

                        omfgdesign.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        omfgdesign.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        omfgdesign.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);

                        omfgdesign.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        omfgdesign.seasonid = reader["seasonid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["seasonid"]);
                        omfgdesign.fitid = reader["fitid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fitid"]);
                        omfgdesign.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        omfgdesign.classid = reader["classid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["classid"]);
                        omfgdesign.iscoredesign = reader["iscoredesign"] == DBNull.Value ? false : Convert.ToBoolean(reader["iscoredesign"]);
                        omfgdesign.sizemode = reader["sizemode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sizemode"]);

                        omfgdesign.autonumber = reader["autonumber"] == DBNull.Value ? 0 : Convert.ToInt32(reader["autonumber"]);
                        omfgdesign.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        omfgdesign.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                        omfgdesign.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                        omfgdesign.pack = reader["pack"] == DBNull.Value ? 0 : Convert.ToInt32(reader["pack"]);

                        omfgdesign.cutratio = reader["cutratio"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cutratio"]);
                        omfgdesign.mfgmonth = reader["mfgmonth"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mfgmonth"]);
                        omfgdesign.mfgyear = reader["mfgyear"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mfgyear"]);
                        omfgdesign.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                        omfgdesign.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"]);
                        omfgdesign.fabricid = reader["fabricid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fabricid"]);
                        omfgdesign.boxid = reader["boxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["boxid"]);
                        omfgdesign.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgdesign.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgdesign.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgdesign.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgdesign.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgdesign.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgdesign.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);

                        omfgdesign.sizegroup_json = reader["sizegroup"] == DBNull.Value ? null : reader["sizegroup"].ToString();
                        var clist = omfgdesign.sizegroup.items.GroupBy(e => e.colourid).ToList().ConvertAll(e =>
                        {
                            if (colourlist.Exists(ex => ex.id == e.Key))
                                return colourlist.Find(ex => ex.id == e.Key).name;
                            else
                                return "";
                        });
                        omfgdesign.colour = string.Join(",", clist);
                        clist = omfgdesign.sizegroup.items.GroupBy(e => e.sizegroup).ToList().ConvertAll(e => e.Key);
                        omfgdesign.sizegroupname = string.Join(",", clist);
                        omfgdesign.sizegroup.items.ForEach(x =>
                        {
                            if (x.mrp <= 0) x.mrp = x.salerate;
                        });
                        if (reader["comboconfig"] != DBNull.Value)
                            omfgdesign.comboconfig = Newtonsoft.Json.JsonConvert.DeserializeObject<ComboConfig>(reader["comboconfig"].ToString());
                        lst.Add(omfgdesign);
                    }
                }
                conn.Close();

                foreach (var combodesign in lst.ToArray())
                {
                    var designcodes = new List<string>();
                    foreach (var comboitem in combodesign.comboconfig.Configlist)
                    {
                        if (!string.IsNullOrEmpty(comboitem.DesignList))
                        {
                            designcodes.AddRange(comboitem.DesignList.Split(','));
                        }
                    }
                    designcodes.RemoveAll(x => string.IsNullOrEmpty(x));
                    lst.AddRange(Search(new MfgdesignCreteria()
                    {
                        includedetail = true,
                        codes = designcodes
                    }));
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgdesign);
            }
            return lst;
        }
        public List<Mfgdesign> Search(MfgdesignCreteria _mfgdesign)
        {
            List<Mfgdesign> lst = new List<Mfgdesign>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgdesign.id != 0)
                {
                    if (ConfigManager.Instance(Context.OrganizationID).Config.system_customfields &&
                        CustomFieldsManager.Instance(Context.OrganizationID).Config != null &&
                        CustomFieldsManager.Instance(Context.OrganizationID).Config.Count > 0)
                    {
                        command = GetCommand(string.Format(SQL_Search, "," + string.Join(",", CustomFieldsManager.Instance(Context.OrganizationID).Config.ConvertAll(x => "mfg_" + x.cfname))), conn);
                    }
                    else
                    {
                        command = GetCommand(string.Format(SQL_Search, ""), conn);
                    }
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgdesign.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    if (ConfigManager.Instance(Context.OrganizationID).Config.system_customfields &&
                        CustomFieldsManager.Instance(Context.OrganizationID).Config != null &&
                        CustomFieldsManager.Instance(Context.OrganizationID).Config.Count > 0)
                    {
                        qb.CurrentQuery = string.Format(SQL_Search.Substring(0, SQL_Search.IndexOf("where")), "," + string.Join(",", CustomFieldsManager.Instance(Context.OrganizationID).Config.ConvertAll(x => "mfg_" + x.cfname)));
                    }
                    else
                    {
                        qb.CurrentQuery = string.Format(SQL_Search.Substring(0, SQL_Search.IndexOf("where")), "");
                    }


                    qb.AddParameter("isactive", true, "=");
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_mfgdesign)
                    {
                        qb.AddParameter("activestatus", 2, "=");
                    }

                    if (!string.IsNullOrEmpty(_mfgdesign.code) &&
                        _mfgdesign.code.Contains(","))
                    {
                        _mfgdesign.codes = _mfgdesign.code.Split(',').ToList();
                        _mfgdesign.code = "";
                    }

                    if (!string.IsNullOrEmpty(_mfgdesign.code))
                    {
                        if (_mfgdesign.code.IndexOf('%') >= 0)
                            qb.AddParameter("code", _mfgdesign.code, " ilike ");
                        else
                            qb.AddParameter("code", _mfgdesign.code, "=");
                    }

                    if (!string.IsNullOrEmpty(_mfgdesign.name))
                    {
                        if (_mfgdesign.name.IndexOf('%') >= 0)
                            qb.AddParameter("name", _mfgdesign.name, " ilike ");
                        else
                            qb.AddParameter("name", _mfgdesign.name, "=");
                    }

                    if (_mfgdesign.ids != null && _mfgdesign.ids.Count > 0)
                        qb.AddInParamer("mfgdesign.id", string.Join(",", _mfgdesign.ids.ConvertAll(x => x)));

                    if (_mfgdesign.codes != null && _mfgdesign.codes.Count > 0)
                        qb.AddInParamer("code", string.Join(",", _mfgdesign.codes.ConvertAll(x => "'" + x + "'")));
                    if (_mfgdesign.fabricid > 0)
                        qb.AddParameter("fabricid", _mfgdesign.fabricid, "=");

                    if (_mfgdesign.brandid > 0)
                        qb.AddParameter("brandid", _mfgdesign.brandid, "=");
                    if (_mfgdesign.productid > 0)
                        qb.AddParameter("productid", _mfgdesign.productid, "=");
                    else if (_mfgdesign.productid == -2)
                        qb.AddParameter("hascombo", true, "=");
                    if (_mfgdesign.supplierid > 0)
                        qb.AddParameter("supplierid", _mfgdesign.supplierid, "=");

                    if (_mfgdesign.coredesigntype == 1)
                        qb.AddParameter("iscoredesign", true, "=");
                    if (_mfgdesign.coredesigntype == 2)
                        qb.AddParameter("iscoredesign", false, "=");

                    if (_mfgdesign.imagestatus > 0)
                    {
                        if (_mfgdesign.imagestatus == 1)
                        {
                            var _addCondition = @"mfgdesign.id in (SELECT mfgdesign.id FROM mfgdesign 
                            LEFT JOIN LATERAL json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                            LEFT JOIN LATERAL json_array_elements (cast(mfgdesign.sizegroup->>'colouritems' as json)) colourlist ON colourlist->>'colourid' = schemelist->>'colourid'
                            where mfgdesign.isactive=true and 
						    ((colourlist->>'imageid' is not null and colourlist->>'imageid'!='')
						    or (imagelist is not null and imagelist->>0 != '')))";
                            qb.Add2Query(_addCondition);
                        }
                        else
                        {
                            var _addCondition = @"mfgdesign.id in (SELECT mfgdesign.id FROM mfgdesign 
                            LEFT JOIN LATERAL json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                            LEFT JOIN LATERAL json_array_elements (cast(mfgdesign.sizegroup->>'colouritems' as json)) colourlist ON colourlist->>'colourid' = schemelist->>'colourid'
                            where mfgdesign.isactive=true and 
						   	(colourlist->>'imageid' is null or colourlist->>'imageid' ='')
						    and (imagelist is null or imagelist->>0 = ''))";
                            qb.Add2Query(_addCondition);
                        }
                    }
                    if (_mfgdesign.onlycombo)
                    {
                        qb.AddParameter("hascombo", true, "=");
                    }
                    if (_mfgdesign.fromdate.Year > 1900)
                        qb.AddParameter("mfgdesign.createdon", "fromdate", _mfgdesign.fromdate.Date, ">=");
                    if (_mfgdesign.todate.Year > 1900)
                        qb.AddParameter("mfgdesign.createdon", "todate", _mfgdesign.todate.Date.AddDays(1), "<");

                    if (!string.IsNullOrEmpty(_mfgdesign.uancode))
                    {
                        var _addCondition = @"mfgdesign.id in (SELECT mfgdesign.id FROM mfgdesign LEFT JOIN LATERAL 
                        json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                        WHERE isactive=true and schemelist->>'uid' in ({0}))";

                        _mfgdesign.codes = new List<string>();
                        _mfgdesign.codes.AddRange(_mfgdesign.uancode.Split(','));

                        qb.Add2Query(string.Format(_addCondition, string.Join(",", _mfgdesign.codes.ConvertAll(x => "'" + x + "'"))));
                    }
                    if (!string.IsNullOrEmpty(_mfgdesign.skucode))
                    {
                        var _addCondition = @"mfgdesign.id in (SELECT mfgdesign.id FROM mfgdesign LEFT JOIN LATERAL 
                        json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                        WHERE isactive=true and schemelist->>'prdsku' in ({0}))";

                        _mfgdesign.codes = new List<string>();
                        _mfgdesign.codes.AddRange(_mfgdesign.skucode.Split(','));

                        qb.Add2Query(string.Format(_addCondition, string.Join(",", _mfgdesign.codes.ConvertAll(x => "'" + x + "'"))));
                    }
                    if (_mfgdesign.sectionid > 0)
                    {
                        var _addCondition = @"mfgdesign.id in (select mfgdesign.id from mfgdesign inner join products on products.id=productid and products.salescounterid=" + _mfgdesign.sectionid + ")";
                        qb.Add2Query(_addCondition);
                    }
                    command = qb.GetCommand(conn);
                }



                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgdesign omfgdesign = new Mfgdesign();
                        omfgdesign.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        omfgdesign.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        omfgdesign.racknumber = reader["racknumber"] == DBNull.Value ? "" : reader["racknumber"].ToString();
                        omfgdesign.prdhsncode = reader["prdhsncode"] == DBNull.Value ? "" : reader["prdhsncode"].ToString();
                        omfgdesign.prddivisionid = reader["prddivisionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prddivisionid"]);

                        omfgdesign.imagelist_json = reader["imagelist"] == DBNull.Value ? "" : reader["imagelist"].ToString();

                        omfgdesign.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        omfgdesign.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        omfgdesign.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgdesign.boxid = reader["boxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["boxid"]);
                        omfgdesign.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        omfgdesign.seasonid = reader["seasonid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["seasonid"]);
                        omfgdesign.fitid = reader["fitid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fitid"]);
                        omfgdesign.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        omfgdesign.classid = reader["classid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["classid"]);
                        omfgdesign.iscoredesign = reader["iscoredesign"] == DBNull.Value ? false : Convert.ToBoolean(reader["iscoredesign"]);
                        omfgdesign.sizemode = reader["sizemode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sizemode"]);
                        omfgdesign.displayname = reader["displayname"] == DBNull.Value ? "" : reader["displayname"].ToString();
                        omfgdesign.autonumber = reader["autonumber"] == DBNull.Value ? 0 : Convert.ToInt32(reader["autonumber"]);
                        omfgdesign.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        omfgdesign.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                        omfgdesign.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                        omfgdesign.pack = reader["pack"] == DBNull.Value ? 0 : Convert.ToInt32(reader["pack"]);

                        omfgdesign.cutratio = reader["cutratio"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cutratio"]);
                        omfgdesign.mfgmonth = reader["mfgmonth"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mfgmonth"]);
                        omfgdesign.mfgyear = reader["mfgyear"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mfgyear"]);
                        omfgdesign.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                        omfgdesign.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"]);
                        omfgdesign.fabricid = reader["fabricid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fabricid"]);
                        omfgdesign.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgdesign.hascombo = reader["hascombo"] == DBNull.Value ? false : Convert.ToBoolean(reader["hascombo"]);

                        omfgdesign.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgdesign.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgdesign.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgdesign.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgdesign.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgdesign.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);

                        //omfgdesign.sizegroup_json = reader["sizegroup"] == DBNull.Value ? null : reader["sizegroup"].ToString();
                        //var clist = omfgdesign.sizegroup.items.GroupBy(e => e.colourid).ToList().ConvertAll(e =>
                        //{
                        //    if (colourlist.Exists(ex => ex.id == e.Key))
                        //        return colourlist.Find(ex => ex.id == e.Key).name;
                        //    else
                        //        return "";
                        //});
                        //omfgdesign.colour = string.Join(",", clist);
                        //clist = omfgdesign.sizegroup.items.GroupBy(e => e.sizegroup).ToList().ConvertAll(e => e.Key);
                        //omfgdesign.sizegroupname = string.Join(",", clist);

                        if (ConfigManager.Instance(Context.OrganizationID).Config.system_customfields &&
                            CustomFieldsManager.Instance(Context.OrganizationID).Config != null &&
                            CustomFieldsManager.Instance(Context.OrganizationID).Config.Count > 0)
                        {
                            ReadCustomFields(omfgdesign, reader);
                        }
                        lst.Add(omfgdesign);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgdesign);
            }
            if (_mfgdesign.id > 0 || _mfgdesign.includedetail)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var colourlist = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "COLOUR"
                });
                UpdateSizeGroup(colourlist, lst);
            }
            else
            {
                if (_mfgdesign.productid > 0 && _mfgdesign.includecolours)
                {
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                    var colourlist = referencelistDataService.SearchList(new ReferencelistCreteria()
                    {
                        type = "COLOUR"
                    });
                    UpdateColourGroup(colourlist, lst);
                }
            }
            return lst;
        }

        public DataTable ExportAddOnFields(List<string> ids)
        {
            string query = @"select schemelist->>'prdsku' ProductSKU,
                            prdhsncode HSN,
                            schemelist->>'prdhsn' PRDHSN,
                            schemelist->>'code' ProductColour, 
                            schemelist->>'designitemcode' DesignItemCode,
                            schemelist->>'name' printingname,
                            products.name Product,
                            brand.name Brand,
                            refsize.name ""Size"",
                            refsize.code ""Size Code"",
                            refsize.additionalinfo->>'formula' ""Size Age"",
                            refcolour.name Colour,
                            refcolour.code ""Colour Code"",
                            refsleeve.name Sleeve,
                            reffit.name Fit,
                            refstyle.name ""Style"",
                            refmaterial.name Material,
                            refpattern.name Pattern,
                            reftype.name ""Type"",
                            refseason.name Season,
                            refcalss.name ""Class"",
                            refdivision.name Division,
                            schemelist->>'uid' UAN
                            {0}
                            from mfgdesign
                            inner join products on products.id = mfgdesign.productid
                            left outer join brand on brand.id = mfgdesign.brandid
                            LEFT JOIN LATERAL
                            json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                            left outer join referencelist refsize on refsize.id = cast(schemelist->>'sizeid' as bigint)
                            left outer join referencelist refcolour on refcolour.id = cast(schemelist->>'colourid' as bigint)
                            left outer join referencelist refsleeve on refsleeve.id = mfgdesign.sleeveid
                            left outer join referencelist reffit on reffit.id = mfgdesign.fitid
                            left outer join referencelist refstyle on refstyle.id =mfgdesign.styleid
                            left outer join referencelist refmaterial on refmaterial.id = mfgdesign.materialid
                            left outer join referencelist refpattern on refpattern.id = mfgdesign.patternid
                            left outer join referencelist reftype on reftype.id = mfgdesign.typeid
                            left outer join referencelist refseason on refseason.id =mfgdesign.seasonid
                            left outer join referencelist refcalss on refcalss.id = mfgdesign.classid
                            left outer join referencelist refdivision on refdivision.id =mfgdesign.prddivisionid
                            {1}
                            where mfgdesign.isactive = true
                            and schemelist->>'uid' in (" + string.Join(",",ids.ConvertAll(x=>"'"+ Utility.SafeSQL(x) +"'")) +")";

            if (CustomFieldsManager.Instance(0).Config.Count == 0)
            {
                query = string.Format(query, "", "");
            }
            else
            {
                List<string> fields = new List<string>();
                List<string> refjoins = new List<string>();
                foreach (var field in CustomFieldsManager.Instance(0).Config)
                {
                    if (field.datatype <= 3)
                    {
                        fields.Add("mfg_" + field.cfname + @" as """ + field.name + @"""");
                    }
                    else
                    {
                        fields.Add("ref" + field.cfname + @".name as """ + field.name + @"""");
                        refjoins.Add(String.Format("left outer join referencelist ref{0} on ref{0}.id = mfgdesign.mfg_{0}", field.cfname));
                    }
                }
                query = string.Format(query, "," + string.Join(",", fields), string.Join("\r\n", refjoins));
            }
            return ExecuteDataSet(query, null).Tables[0];
        }

        public DataTable Export(List<long> ids)
        {
            string query = @"select mfgdesign.code,schemelist->>'uid' UAN,
                            cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
                            cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
                            cast(schemelist->>'cost' as numeric) ""Cost"",

                            schemelist->>'prdsku' ProductSKU,prdhsncode HSN,
                            schemelist->>'prdhsn' PRDHSN,
                            schemelist->>'code' ProductColour, 
                            schemelist->>'designitemcode' DesignItemCode,
                            schemelist->>'name' printingname,
                            products.name Product,
                            brand.name Brand,
                            refsize.name ""Size"",
                            refsize.code ""Size Code"",
                            refsize.additionalinfo->>'formula' ""Size Age"",
                            refcolour.name Colour,
                            refcolour.code ""Colour Code"",
                            refsleeve.name Sleeve,
                            reffit.name Fit,
                            refstyle.name ""Style"",
                            refmaterial.name Material,
                            refpattern.name Pattern,
                            reftype.name ""Type"",
                            refseason.name Season,
                            refcalss.name ""Class"",
                            refdivision.name Division
                            {0}
                            from mfgdesign
                            inner join products on products.id = mfgdesign.productid
                            left outer join brand on brand.id = mfgdesign.brandid
                            LEFT JOIN LATERAL
                            json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                            left outer join referencelist refsize on refsize.id = cast(schemelist->>'sizeid' as bigint)
                            left outer join referencelist refcolour on refcolour.id = cast(schemelist->>'colourid' as bigint)
                            left outer join referencelist refsleeve on refsleeve.id = mfgdesign.sleeveid
                            left outer join referencelist reffit on reffit.id = mfgdesign.fitid
                            left outer join referencelist refstyle on refstyle.id =mfgdesign.styleid
                            left outer join referencelist refmaterial on refmaterial.id = mfgdesign.materialid
                            left outer join referencelist refpattern on refpattern.id = mfgdesign.patternid
                            left outer join referencelist reftype on reftype.id = mfgdesign.typeid
                            left outer join referencelist refseason on refseason.id =mfgdesign.seasonid
                            left outer join referencelist refcalss on refcalss.id = mfgdesign.classid
                            left outer join referencelist refdivision on refdivision.id =mfgdesign.prddivisionid
                            {1}
                            where mfgdesign.isactive = true";

            if (CustomFieldsManager.Instance(0).Config.Count == 0)
            {
                query = string.Format(query, "", "");
            }
            else
            {
                List<string> fields = new List<string>();
                List<string> refjoins = new List<string>();
                foreach (var field in CustomFieldsManager.Instance(0).Config)
                {
                    if (field.datatype <= 3)
                    {
                        fields.Add("mfg_" + field.cfname + @" as """ + field.name + @"""");
                    }
                    else
                    {
                        fields.Add("ref" + field.cfname + @".name as """ + field.name + @"""");
                        refjoins.Add(String.Format("left outer join referencelist ref{0} on ref{0}.id = mfgdesign.mfg_{0}", field.cfname));
                    }
                }
                query = string.Format(query, "," + string.Join(",", fields), string.Join("\r\n", refjoins));
                if (ids != null && ids.Count > 0)
                    query += " and mfgdesign.id in (" + String.Join(",", ids) + ")";
            }
            var table = ExecuteDataSet(query, null).Tables[0];
            var rows = table.Select("wsrate>0");
            if (rows.Length == 0) table.Columns.Remove("wsrate");
            rows = table.Select("dealerprice>0");
            if (rows.Length == 0) table.Columns.Remove("dealerprice");
            rows = table.Select("salerate>0");
            if (rows.Length == 0) table.Columns.Remove("salerate");
            foreach (var row in table.AsEnumerable())
            {
                if (row["PRDHSN"] != DBNull.Value &&
                   row["PRDHSN"].ToString() != "")
                {
                    row["HSN"] = row["PRDHSN"];
                }
            }
            table.Columns.Remove("PRDHSN");

            if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_skufields))
            {
                var desiredOrder = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_skufields.Split(',').ToList();
                desiredOrder.RemoveAll(x => string.IsNullOrEmpty(x));
                int position = 0;
                int uposition = desiredOrder.Count;
                List<string> missingfields = new List<string>();
                foreach (string colName in desiredOrder)
                {
                    if (table.Columns.Contains(colName))
                    {
                        table.Columns[colName].SetOrdinal(position++);
                    }
                    else
                    {
                        missingfields.Add(colName);
                    }
                }
                if (missingfields.Count > 0)
                {
                    var support = new List<string>();
                    foreach (DataColumn item in table.Columns)
                    {
                        support.Add(item.ColumnName);
                    }
                    throw new Exception("Can't find the sku fields list is missing " + string.Join(",", missingfields) + "\r\n" +
                        " Expected colunms are " + string.Join(",", support));
                }
            }
            return table;
        }

        private void ReadCustomFields(Mfgdesign mfgdesign,
            IDataReader reader)
        {
            mfgdesign.CustomFields = new List<Mfgdesign.CustomFieldValue>();
            foreach (var field in CustomFieldsManager.Instance(Context.OrganizationID).Config)
            {
                if (reader["mfg_" + field.cfname] == DBNull.Value)
                {
                    mfgdesign.CustomFields.Add(new Mfgdesign.CustomFieldValue()
                    {
                        key = field.cfname,
                        value = null
                    });
                }
                else if (field.datatype == 1)
                {
                    mfgdesign.CustomFields.Add(new Mfgdesign.CustomFieldValue()
                    {
                        key = field.cfname,
                        value = reader["mfg_" + field.cfname].ToString()
                    });
                }
                else if (field.datatype == 2)
                {
                    mfgdesign.CustomFields.Add(new Mfgdesign.CustomFieldValue()
                    {
                        key = field.cfname,
                        value = Convert.ToDecimal(reader["mfg_" + field.cfname]).ToString()
                    });
                }
                else if (field.datatype == 3)
                {
                    mfgdesign.CustomFields.Add(new Mfgdesign.CustomFieldValue()
                    {
                        key = field.cfname,
                        value = Convert.ToDateTime(reader["mfg_" + field.cfname]).ToString()
                    });
                }
                else
                {
                    mfgdesign.CustomFields.Add(new Mfgdesign.CustomFieldValue()
                    {
                        key = field.cfname,
                        value = Convert.ToInt64(reader["mfg_" + field.cfname]).ToString()
                    });
                }
            }
        }



        public void UpdateColourGroup(List<ReferenceItem> colourlist, List<Mfgdesign> omfgdesigns)
        {

            string query = "Select sizegroup,id from Mfgdesign where id in (" + string.Join(",", omfgdesigns.ConvertAll(x => x.id)) + ")";
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    long id = reader.GetInt64(1);
                    var omfgdesign = omfgdesigns.Find(x => x.id == id);
                    omfgdesign.sizegroup_json = reader["sizegroup"] == DBNull.Value ? null : reader["sizegroup"].ToString();
                    omfgdesign.sizegroup.colouritems = omfgdesign.sizegroup.items.GroupBy(e => e.colourid).ToList().ConvertAll(e =>
                    {
                        Mfgdesign.Sizegroup.ColourItem ci = new Mfgdesign.Sizegroup.ColourItem() { colourid = e.Key };
                        if (colourlist.Exists(ex => ex.id == e.Key))
                            ci.name = colourlist.Find(ex => ex.id == e.Key).name;
                        else
                            ci.name = "";
                        return ci;
                    });
                }
            }
        }


        public void UpdateSizeGroup(List<ReferenceItem> colourlist, List<Mfgdesign> omfgdesigns)
        {
            if (omfgdesigns.Count == 0) return;
            string query = "Select sizegroup,id from Mfgdesign where id in (" + string.Join(",", omfgdesigns.ConvertAll(x => x.id)) + ")";
            var param = new SortedList<string, object>();
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    long id = reader.GetInt64(1);
                    var omfgdesign = omfgdesigns.Find(x => x.id == id);
                    omfgdesign.sizegroup_json = reader["sizegroup"] == DBNull.Value ? null : reader["sizegroup"].ToString();
                    var clist = omfgdesign.sizegroup.items.GroupBy(e => e.colourid).ToList().ConvertAll(e =>
                    {
                        if (colourlist.Exists(ex => ex.id == e.Key))
                            return colourlist.Find(ex => ex.id == e.Key).name;
                        else
                            return "";
                    });
                    omfgdesign.colour = string.Join(",", clist);
                    clist = omfgdesign.sizegroup.items.GroupBy(e => e.sizegroup).ToList().ConvertAll(e => e.Key);
                    omfgdesign.sizegroupname = string.Join(",", clist);
                    omfgdesign.sizegroup.items.ForEach(x =>
                    {
                        if (x.mrp <= 0) x.mrp = x.salerate;
                        x.combo = false;
                    });
                }
            }

            if (omfgdesigns.Exists(x => x.hascombo))
            {
                foreach (var omfgdesign in omfgdesigns.FindAll(x => x.hascombo))
                {
                    var config = GetComboConfig(new ComboConfig()
                    {
                        Id = omfgdesign.id
                    });
                    if (config != null)
                    {
                        foreach (var gitem in config.UANList.GroupBy(x => x.uan))
                        {
                            if (gitem.Count() == config.Qty)
                            {
                                if (omfgdesign.sizegroup.items.Exists(x => x.uid == gitem.Key))
                                {
                                    var barcodes = new List<string>();
                                    gitem.ToList().ForEach(x =>
                                    {
                                        barcodes.AddRange(x.uans);
                                    });
                                    omfgdesign.sizegroup.items.Find(x => x.uid == gitem.Key).combo = barcodes.Count == config.Qty;
                                    omfgdesign.sizegroup.items.Find(x => x.uid == gitem.Key).barcodes = barcodes;
                                }
                            }
                        }
                    }
                }
            }
        }

        public Mfgdesign SearchDetail(long id)
        {
            Mfgdesign omfgdesign = Search(new MfgdesignCreteria()
            {
                id = id
            }).First();
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var colourlist = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                type = "COLOUR"
            });
            UpdateSizeGroup(colourlist, new List<Mfgdesign>(new Mfgdesign[] { omfgdesign }));
            return omfgdesign;
        }

        public Mfgdesign Insert(Mfgdesign oMfgdesign)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    HasDuplicate(oMfgdesign);
                    Insert(transaction, conn, oMfgdesign);
                    if (!ismigration && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_mfgdesign)
                    {
                        Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID,
                            Services.IntrayManager.EntryType.MfgDesign,
                        oMfgdesign.id, string.Format("New Design {0} registered.", oMfgdesign.name),
                        Context.UserID, 0, transaction, this.Context);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgdesign);
                throw exp;
            }

            return oMfgdesign;
        }

        public void HasDuplicate(Mfgdesign mfgdesign)
        {

            if (mfgdesign.sizegroup != null &&
                mfgdesign.sizegroup.items != null)
            {
                var duplicateuids = mfgdesign.sizegroup.items.GroupBy(x => x.uid).ToList().FindAll(x => x.Count() > 1);
                if (duplicateuids.Count > 0)
                {
                    if (ismigration)
                    {
                        foreach (var dupitem in duplicateuids)
                        {
                            var allitem = mfgdesign.sizegroup.items.FindAll(x => x.uid == dupitem.Key);
                            for (var i = allitem.Count - 1; i > 0; i--)
                            {
                                allitem[0].barcodes.AddRange(allitem[i].barcodes);
                                allitem[0].stock += allitem[i].stock;
                                mfgdesign.sizegroup.items.Remove(allitem[i]);
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Duplicate UID - " + string.Join(",", duplicateuids.ConvertAll(x => x.Key)));
                    }
                }
            }

            if (ismigration) return;

            if (mfgdesign.id > 0)
            {
                var oldmfgdesign = Search(new MfgdesignCreteria() { id = mfgdesign.id }).First();
                if (oldmfgdesign.productid != mfgdesign.productid ||
                    oldmfgdesign.brandid != mfgdesign.brandid ||
                    oldmfgdesign.materialid != mfgdesign.materialid ||
                    oldmfgdesign.fitid != mfgdesign.fitid ||
                    oldmfgdesign.styleid != mfgdesign.styleid ||
                    oldmfgdesign.materialid != mfgdesign.materialid ||
                    oldmfgdesign.typeid != mfgdesign.typeid)
                {
                    var uids = oldmfgdesign.sizegroup.items.ConvertAll(x => "'" + x.uid + "'");
                    string dquery = string.Format(@"Select sono,uid from salesorder 
                inner join salesorderitems on salesorderitems.salesorderid=salesorder.id
                where salesorderitems.isactive = true and salesorderitems.isactive=true and
                uid in ({0})", string.Join(",", uids));
                    using (var reader = ExecuteReader(dquery, new SortedList<string, object>()))
                    {
                        if (reader.Read())
                            throw new Exception("Can't change attribute,already SO available for the UAN " + reader.GetString(1) + "(" + reader.GetString(0) + ") can't alter the master");
                    }
                    dquery = string.Format(@"Select sum(qty),stock.serialno serialno from stock
                where stock.isactive = true and stock.serialno in ({0}) group by stock.serialno", string.Join(",", uids));
                    using (var reader = ExecuteReader(UpdateQueryForHyper(dquery), new SortedList<string, object>()))
                    {
                        if (reader.Read())
                            throw new Exception("Can't change attribute,already stock available for the UAN " + reader.GetString(1));
                    }

                    dquery = string.Format(@"Select uid from mfgworkorderitemsizes
                where mfgworkorderitemsizes.isactive = true and uid in ({0}) group by uid;
                select distinct workorderno from mfgworkorder
                inner join mfgworkorderitems on mfgworkorderitems.mfgworkorderid = mfgworkorder.id
                inner join mfgworkorderitemsizes on mfgworkorderitemsizes.mfgworkorderitemsid = mfgworkorderitems.id
                where mfgworkorder.isactive=true and mfgworkorderitems.isactive = true and mfgworkorderitemsizes.isactive = true and uid in ({0})", string.Join(",", uids));
                    using (var reader = ExecuteReader(dquery, new SortedList<string, object>()))
                    {
                        List<string> uanlist = new List<string>();
                        while (reader.Read())
                        {
                            uanlist.Add(reader.GetString(0));
                        }
                        if (uanlist.Count > 0 && reader.NextResult())
                        {
                            List<string> wonos = new List<string>();
                            while (reader.Read())
                            {
                                wonos.Add(reader.GetString(0));
                            }
                            throw new Exception("Can't change attribute,already Work order " + string.Join(",", wonos) + " available for the UAN " + string.Join(",", uanlist));
                        }

                    }
                }

            }
            if (mfgdesign.sizegroup != null &&
                mfgdesign.sizegroup.items != null &&
                mfgdesign.sizegroup.ditems != null &&
                mfgdesign.sizegroup.ditems.Count > 0)
            {
                var uids = mfgdesign.sizegroup.ditems.ConvertAll(x => "'" + x.uid + "'");
                string dquery = string.Format(@"Select sono,uid from salesorder 
                inner join salesorderitems on salesorderitems.salesorderid=salesorder.id
                where salesorderitems.isactive = true and salesorderitems.isactive=true and
                uid in ({0})", string.Join(",", uids));
                using (var reader = ExecuteReader(dquery, new SortedList<string, object>()))
                {
                    if (reader.Read())
                        throw new Exception("Can't delete,already on SO available for the UAN " + reader.GetString(1) + "(" + reader.GetString(0) + ")");
                }


                dquery = string.Format(@"Select sum(qty),stock.serialno serialno from stock
                where stock.isactive = true and stock.serialno in ({0}) group by stock.serialno", string.Join(",", uids));
                using (var reader = ExecuteReader(UpdateQueryForHyper(dquery), new SortedList<string, object>()))
                {
                    if (reader.Read())
                        throw new Exception("Can't delete,already on stock available for the UAN " + reader.GetString(1));
                }

                dquery = string.Format(@"Select sum(qty),uid from mfgworkorderitemsizes
                where mfgworkorderitemsizes.isactive = true and uid in ({0}) group by uid", string.Join(",", uids));
                using (var reader = ExecuteReader(dquery, new SortedList<string, object>()))
                {
                    if (reader.Read())
                        throw new Exception("Can't delete,already on Work order available for the UAN " + reader.GetString(1));
                }
            }

            string query = @"select schemelist->>'uid' from mfgdesign 
            LEFT JOIN LATERAL
            json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
            where schemelist->>'uid' in ({0}) and id!=@id and
            isactive=true";
            var param = new SortedList<string, object>();
            param.Add("id", mfgdesign.id);
            param.Add("code", mfgdesign.code);
            if (mfgdesign.sizegroup != null &&
                mfgdesign.sizegroup.items != null &&
                mfgdesign.sizegroup.items.Count > 0)
            {
                query = string.Format(query, string.Join(",", mfgdesign.sizegroup.items.ConvertAll(x => "'" + x.uid + "'")));
                using (var reader = ExecuteReader(query, param))
                {
                    if (reader.Read())
                    {
                        if (!reader.IsDBNull(0))
                        {
                            throw new Exception("Can't support duplicate UAN," + reader.GetString(0) + " UAN already exist");
                        }
                    }
                }
            }

            query = "select mfgdesign.code from mfgdesign where code=@code and id!=@id and isactive=true";
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                    {
                        throw new Exception(reader.GetString(0) + " Design already exist");
                    }
                }
            }
        }
        private List<Uniquesequance> UpdateUID(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgdesign oMfgdesign)
        {
            var list = oMfgdesign.sizegroup.items.FindAll(e => string.IsNullOrEmpty(e.uid));
            List<Uniquesequance> sequencelist = null;
            UniquesequanceDataService uniquesequanceDataService = new UniquesequanceDataService() { Context = this.Context };
            if (list.Count > 0)
            {
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_eansequence)
                {
                    sequencelist = uniquesequanceDataService.Get(list.Count, transaction, conn);
                    if (sequencelist.Count != list.Count)
                        throw new Exception("EAN Sequnce number shortage, please add more sequence");
                    for (int index = 0; index < list.Count; index++)
                    {
                        sequencelist[index].used = true;
                        sequencelist[index].usedreferencename = list[index].code;
                        list[index].uid = sequencelist[index].sequenceno;
                        uniquesequanceDataService.UpdateUsed(transaction, conn, sequencelist[index]);
                    }
                }
                else
                {
                    var sequence = SequenceManager.Instance(Context.OrganizationID).Get("mfgdesign.uid", 0);
                    for (int index = 0; index < list.Count; index++)
                    {
                        list[index].uid = sequence.GetNextNumber(Context, conn, transaction);
                    }
                }
            }

            if (list.GroupBy(e => e.uid).ToList().Exists(ex => ex.Count() > 1))
            {
                var duplicatedList = list.GroupBy(e => e.uid).ToList().FindAll(ex => ex.Count() > 1);

                for (int index = 0; index < duplicatedList.Count; index++)
                {
                    var sl = sequencelist.Find(e => e.sequenceno == duplicatedList[index].Key);
                    sl.used = true;
                    sl.usedon = DateTimeNow;
                    uniquesequanceDataService.UpdateUsed(transaction, conn, sl);
                }

                return UpdateUID(transaction, conn, oMfgdesign);
            }

            if (list.Count > 0)
            {
                string query = string.Format(@"select Mfgdesign.code, cast(schemelist->>'uid' as varchar) from Mfgdesign
LEFT JOIN LATERAL
json_array_elements (Mfgdesign.sizegroup ->'items') schemelist ON true
where isactive = true and Mfgdesign.id != @id and cast(schemelist->>'uid' as varchar) in ({0})",
    string.Join(",", list.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e.uid) + "'")));
                var parameter = new SortedList<string, object>();
                parameter.Add("id", oMfgdesign.id);
                string duplicatedesigncode = "";
                List<string> duplicateuan = new List<string>();
                using (var reader = ExecuteReader(query, parameter, conn, transaction))
                {
                    while (reader.Read())
                    {
                        duplicatedesigncode = reader.GetString(0);
                        duplicateuan.Add(reader.GetString(1));
                    }
                    reader.Close();
                }


                if (!string.IsNullOrEmpty(duplicatedesigncode))
                {
                    foreach (var slkey in duplicateuan)
                    {
                        var sl = sequencelist.Find(e => e.sequenceno == slkey);
                        if (!sl.used)
                        {
                            sl.used = true;
                            sl.usedon = DateTimeNow;
                            uniquesequanceDataService.UpdateUsed(transaction, conn, sl);
                        }
                    }
                    return UpdateUID(transaction, conn, oMfgdesign);
                }
            }

            list = oMfgdesign.sizegroup.items.FindAll(e => string.IsNullOrEmpty(e.code));
            if (list.Count > 0)
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("mfgdesign.designitemcode", 0);
                for (int index = 0; index < list.Count; index++)
                {
                    list[index].code = sequence.GetNextNumber(Context, conn, transaction);
                }
            }
            return sequencelist;
        }
        public Mfgdesign Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgdesign oMfgdesign)
        {
            if (oMfgdesign.productid == 0) throw new Exception("Please select the product");
            List<Uniquesequance> sequencelist = UpdateUID(transaction, conn, oMfgdesign);
            if (string.IsNullOrEmpty(oMfgdesign.code))
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("mfgdesign.designcode", 0);
                oMfgdesign.code = sequence.GetNextNumber(Context, conn, transaction);
            }
            else
            {
                var param = new SortedList<string, object>();
                param.Add("designid", oMfgdesign.code);
                using (var reader = ExecuteReader("select serialno,stockdate from stock where designid=@designid limit 10", param, conn, transaction))
                {
                    if (reader.Read())
                    {
                        var serialno = reader["serialno"].ToString();
                        var stockdate = Convert.ToDateTime(reader["stockdate"]);
                        throw new Exception("Given design code already has stock and generated " + serialno + " on " + stockdate.ToString("dd-MM-yyyy"));
                    }
                }
            }
            var itemlist = oMfgdesign.sizegroup.items.FindAll(e => e.itemid == 0);
            if (itemlist.Count > 0)
            {
                ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
                ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
                var productname = productsDataService.SearchList(new ProductsCreteria()
                {
                    id = oMfgdesign.productid
                }).First().name;

                var searchitemalllist = itemsDataService.Search(new ItemsCreteria()
                {
                    glns = itemlist.ConvertAll(x => x.uid)
                });
                foreach (var item in itemlist)
                {
                    var itemmaster = new ItemsCreteria()
                    {
                        productid = oMfgdesign.productid,
                        brandid = oMfgdesign.brandid,
                        typeid = oMfgdesign.typeid,
                        styleid = oMfgdesign.styleid,
                        patternid = oMfgdesign.patternid,
                        fitid = oMfgdesign.fitid,
                        seasonid = oMfgdesign.seasonid,
                        materialid = oMfgdesign.materialid,
                        sleeveid = item.sleeveid,
                        colourid = item.colourid,
                        sizeid = item.sizeid,
                        classid = oMfgdesign.classid,
                        mrp = item.mrp,
                        gln = item.uid,
                        glndesignid = oMfgdesign.code,
                        support_gln = true,
                        productmap = true
                    };
                    var searchitemlist = searchitemalllist.FindAll(x => x.gln == item.uid);
                    if (searchitemlist.Count == 0)
                    {
                        itemmaster.productname = productname;
                        itemmaster.printingname = item.name;
                        itemmaster.sellingname = item.name;
                        itemmaster.seasonid = oMfgdesign.seasonid;
                        itemmaster.classid = oMfgdesign.classid;
                        itemmaster.prddivisionid = oMfgdesign.prddivisionid;
                        itemmaster.gln = item.uid;
                        itemmaster.mrp = item.mrp;
                        itemmaster.support_gln = true;
                        itemmaster.CustomFields = new List<Items.CustomFieldValue>();
                        if (oMfgdesign.CustomFields != null &&
                            oMfgdesign.CustomFields.Count > 0)
                        {
                            foreach (var cfield in oMfgdesign.CustomFields)
                            {
                                itemmaster.CustomFields.Add(new Items.CustomFieldValue()
                                {
                                    key = cfield.key,
                                    value = cfield.value,
                                });
                            }
                        }
                        item.itemid = itemsDataService.Insert(transaction, conn, itemmaster).id;
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(searchitemlist.First().gln))
                        {
                            searchitemlist.First().gln = item.uid;
                            searchitemlist.First().printingname = item.name;
                            searchitemlist.First().sellingname = item.name;
                            searchitemlist.First().CustomFields = new List<Items.CustomFieldValue>();
                            if (oMfgdesign.CustomFields != null &&
                            oMfgdesign.CustomFields.Count > 0)
                            {
                                foreach (var cfield in oMfgdesign.CustomFields)
                                {
                                    searchitemlist.First().CustomFields.Add(new Items.CustomFieldValue()
                                    {
                                        key = cfield.key,
                                        value = cfield.value,
                                    });
                                }
                            }
                            itemsDataService.Update(searchitemlist.First(), false);
                        }
                        else
                        {
                            item.uid = searchitemlist.First().gln;
                        }
                        item.itemid = searchitemlist.First().id;
                    }
                }
            }

            NpgsqlCommand command = null;

            if (ConfigManager.Instance(Context.OrganizationID).Config.system_customfields &&
                CustomFieldsManager.Instance(Context.OrganizationID).Config != null &&
                CustomFieldsManager.Instance(Context.OrganizationID).Config.Count > 0)
            {
                var flist = CustomFieldsManager.Instance(Context.OrganizationID).Config.ConvertAll(x => "mfg_" + x.cfname);
                var vlist = CustomFieldsManager.Instance(Context.OrganizationID).Config.ConvertAll(x => "@" + "mfg_" + x.cfname);
                var query = string.Format(SQL_Insert, "," + string.Join(",", flist), "," + string.Join(",", vlist));
                command = GetCommand(query, conn, transaction);
                AddCustomFields(oMfgdesign, command);
            }
            else
            {
                command = new NpgsqlCommand(string.Format(SQL_Insert, "", ""), conn, transaction);
            }

            if ((Services.ConfigManager.Instance(Context.OrganizationID).Config == null ||
                Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_mfgdesign == false))
            {
                oMfgdesign.isactive = true;
                oMfgdesign.Activestatus = BaseEntity.ActiveStatus.Approved;
            }
            else
            {
                oMfgdesign.isactive = false;
                oMfgdesign.Activestatus = BaseEntity.ActiveStatus.UnApproved;
            }
            if (oMfgdesign.sizegroup != null && oMfgdesign.sizegroup.items != null && oMfgdesign.sizegroup.items.Count > 0)
            {
                oMfgdesign.sizegroup.items.ForEach(x => x.barcodes = null);
            }
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgdesign.isactive;
            command.Parameters.Add("activestatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.activestatus;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesign.code == null ? "" : oMfgdesign.code;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesign.name == null ? "" : oMfgdesign.name;
            command.Parameters.Add("displayname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesign.displayname == null ? "" : oMfgdesign.displayname;
            command.Parameters.Add("racknumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesign.racknumber == null ? "" : oMfgdesign.racknumber;
            command.Parameters.Add("prdhsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesign.prdhsncode == null ? "" : oMfgdesign.prdhsncode;
            command.Parameters.Add("prddivisionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.prddivisionid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.productid;
            command.Parameters.Add("autonumber", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.autonumber;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.brandid;
            command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.sleeveid;

            command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.typeid;
            command.Parameters.Add("styleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.styleid;
            command.Parameters.Add("pack", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.pack;
            command.Parameters.Add("cutratio", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgdesign.cutratio;
            command.Parameters.Add("mfgmonth", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.mfgmonth;
            command.Parameters.Add("mfgyear", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.mfgyear;
            command.Parameters.Add("patternid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.patternid;
            command.Parameters.Add("materialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.materialid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.colourid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.supplierid;
            command.Parameters.Add("fitid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.fitid;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.taxid;
            command.Parameters.Add("classid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.classid;
            command.Parameters.Add("iscoredesign", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgdesign.iscoredesign;
            command.Parameters.Add("sizemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.sizemode;
            command.Parameters.Add("seasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.seasonid;
            command.Parameters.Add("boxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.boxid;
            command.Parameters.Add("fabricid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.fabricid;
            command.Parameters.Add("sizegroup", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgdesign.sizegroup_json == null ? "null" : oMfgdesign.sizegroup_json;
            command.Parameters.Add("imagelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgdesign.imagelist_json == null ? "null" : oMfgdesign.imagelist_json;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgdesign.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgdesign.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.version;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.modifiedby;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgdesign.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            //if (sequencelist != null)
            //{
            //    UniquesequanceDataService uniquesequanceDataService = new UniquesequanceDataService() { Context = this.Context };
            //    for (int index = 0; index < sequencelist.Count; index++)
            //    {
            //        sequencelist[index].usedreferenceid = oMfgdesign.id;
            //        uniquesequanceDataService.UpdateUsed(transaction, conn, sequencelist[index]);
            //    }
            //}
            return oMfgdesign;
        }

        private void AddCustomFields(Mfgdesign oLrinvoiceitems,
        NpgsqlCommand command)
        {
            foreach (var field in CustomFieldsManager.Instance(Context.OrganizationID).Config)
            {
                var hasvalue = (oLrinvoiceitems.CustomFields != null && oLrinvoiceitems.CustomFields.Exists(x => x.key == field.cfname));
                Mfgdesign.CustomFieldValue cfvalue = null;
                if (hasvalue)
                {
                    cfvalue = oLrinvoiceitems.CustomFields.Find(x => x.key == field.cfname);
                }
                if (field.datatype == 1)
                {
                    command.Parameters.Add("@" + "mfg_" + field.cfname,
                        NpgsqlTypes.NpgsqlDbType.Varchar).Value =
                        !hasvalue ? "" : (string.IsNullOrEmpty(cfvalue.value) ? "" : cfvalue.value);
                }
                else if (field.datatype == 2)
                {
                    decimal value = 0;
                    if (hasvalue)
                        decimal.TryParse(cfvalue.value, out value);
                    command.Parameters.Add("@" + "mfg_" + field.cfname,
                        NpgsqlTypes.NpgsqlDbType.Numeric).Value = !hasvalue ? 0 : value;
                }
                else if (field.datatype == 3)
                {
                    DateTime value = DateTime.MinValue;
                    if (hasvalue)
                        DateTime.TryParse(cfvalue.value, out value);
                    command.Parameters.Add("@" + "mfg_" + field.cfname,
                        NpgsqlTypes.NpgsqlDbType.Timestamp).Value = value;
                }
                else
                {
                    long value = 0;
                    if (hasvalue)
                        long.TryParse(cfvalue.value, out value);
                    command.Parameters.Add("@" + "mfg_" + field.cfname,
                        NpgsqlTypes.NpgsqlDbType.Bigint).Value = value;
                }
            }
        }

        public bool UpdateImages(Mfgdesign oMfgdesign)
        {
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
                command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesign.code == null ? "" : oMfgdesign.code;
                command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesign.name == null ? "" : oMfgdesign.name;
                command.Parameters.Add("racknumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesign.racknumber == null ? "" : oMfgdesign.racknumber;
                command.Parameters.Add("displayname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesign.displayname == null ? "" : oMfgdesign.displayname;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.productid;
                command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.brandid;
                command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.sleeveid;
                command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.typeid;
                command.Parameters.Add("styleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.styleid;
                command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.colourid;
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.supplierid;
                command.Parameters.Add("fitid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.fitid;
                command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.taxid;
                command.Parameters.Add("sizemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.sizemode;
                command.Parameters.Add("classid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.classid;
                command.Parameters.Add("iscoredesign", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgdesign.iscoredesign;
                command.Parameters.Add("seasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.seasonid;
                command.Parameters.Add("boxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.boxid;
                command.Parameters.Add("autonumber", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.autonumber;
                command.Parameters.Add("pack", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.pack;
                command.Parameters.Add("cutratio", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgdesign.cutratio;
                command.Parameters.Add("mfgmonth", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.mfgmonth;
                command.Parameters.Add("mfgyear", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.mfgyear;
                command.Parameters.Add("patternid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.patternid;
                command.Parameters.Add("materialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.materialid;
                command.Parameters.Add("fabricid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.fabricid;
                command.Parameters.Add("sizegroup", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgdesign.sizegroup_json == null ? "null" : oMfgdesign.sizegroup_json;
                command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgdesign.isactive;
                command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgdesign.createdon;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgdesign.modifiedon;
                command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.version;
                command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.createdby;
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.modifiedby;
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.id;
                command.Parameters.Add("imagelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgdesign.imagelist_json == null ? "null" : oMfgdesign.imagelist_json;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        oMfgdesign.version = Convert.ToInt32(reader[0].ToString());
                    }
                }
                transaction.Commit();
                conn.Close();
            }
            return true;
        }

        public Mfgdesign Update(Mfgdesign oMfgdesign, ChangeAttributes changeAttributes)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (changeAttributes == null)
                        HasDuplicate(oMfgdesign);
                    Update(transaction, conn, oMfgdesign, changeAttributes);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgdesign);
                throw exp;
            }

            return oMfgdesign;
        }

        public ImageController.ExcelDocument ConvertToUID(ImageController.ExcelDocument source)
        {
            var designindex = source.columns.FindIndex(x => x.ToLower().StartsWith("design"));
            var designcodes = source.rows.ConvertAll(x =>
            {
                return x.values[designindex].IndexOf('/') > 0 ? x.values[designindex].Substring(0, x.values[designindex].IndexOf('/')) : x.values[designindex];
            }).Distinct();
            var designs = Search(new MfgdesignCreteria()
            {
                codes = designcodes.ToList(),
                includedetail = true
            });
            ImageController.ExcelDocument excelDocument = new ImageController.ExcelDocument();
            excelDocument.columns = new List<string>();
            excelDocument.columns.Add("barcode");
            excelDocument.columns.Add("qty");
            excelDocument.rows = new List<ImageController.ExcelDocument.Row>();
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var sizeReference = referencelistDataService.SearchList(new ReferencelistCreteria() { type = "SIZE" });
            SortedList<int, Tuple<long, string>> sizeList = new SortedList<int, Tuple<long, string>>();
            for (var i = 0; i < source.columns.Count; i++)
            {
                if (sizeReference.Exists(x => x.name.ToLower().Trim() == source.columns[i].ToLower().Trim()))
                {
                    var sr = sizeReference.Find(x => x.name.ToLower().Trim() == source.columns[i].ToLower().Trim());
                    sizeList.Add(i, new Tuple<long, string>(sr.id, sr.name));
                }
            }
            int rowindex = 0;
            List<string> missingdesigncodes = new List<string>();
            foreach (var row in source.rows)
            {
                var designcode = row.values[designindex].IndexOf('/') > 0 ? row.values[designindex].Substring(0, row.values[designindex].IndexOf('/')) : row.values[designindex];
                if (designs.Exists(x => x.code == designcode))
                {
                    var design = designs.Find(x => x.code == designcode);

                    var alluan = design.sizegroup.items.FindAll(x => x.code.Trim() == row.values[designindex].Trim());
                    var allpossibleuan = design.sizegroup.items.FindAll(x => x.code.Trim().Contains(row.values[designindex].Trim()));
                    if (alluan.Count == 0)
                    {
                        var t1 = row.values[designindex].ToUpper().Replace(" ", "");
                        alluan = design.sizegroup.items.FindAll(x => x.name.ToUpper().Replace(" ", "") == t1);
                        if (alluan.Count == 0)
                        {
                            t1 = row.values[designindex].Split('(')[0];
                            alluan = design.sizegroup.items.FindAll(x => x.name.ToUpper().Replace(" ", "") == t1);
                        }
                    }

                    if (alluan.Count == 0)
                    {
                        missingdesigncodes.Add(row.values[designindex]);
                        continue;
                    }
                    foreach (var uansizeid in sizeList)
                    {
                        if (!string.IsNullOrEmpty(row.values[uansizeid.Key]))
                        {
                            int qty;
                            if (int.TryParse(row.values[uansizeid.Key], out qty))
                            {
                                if (alluan.Exists(x => x.sizeid == uansizeid.Value.Item1))
                                {
                                    var newrow = new ImageController.ExcelDocument.Row() { values = new List<string>() };
                                    newrow.values.Add(alluan.Find(x => x.sizeid == uansizeid.Value.Item1).uid);
                                    newrow.values.Add(qty.ToString());
                                    excelDocument.rows.Add(newrow);
                                }
                                else if (allpossibleuan.Count(x => x.sizeid == uansizeid.Value.Item1) == 1)
                                {
                                    var newrow = new ImageController.ExcelDocument.Row() { values = new List<string>() };
                                    newrow.values.Add(allpossibleuan.Find(x => x.sizeid == uansizeid.Value.Item1).uid);
                                    newrow.values.Add(qty.ToString());
                                    excelDocument.rows.Add(newrow);
                                }
                                else
                                {
                                    missingdesigncodes.Add(row.values[designindex].Trim() + " " + uansizeid.Value.Item2);
                                }
                            }
                        }
                    }
                }
                else if (rowindex > 0)
                {
                    missingdesigncodes.Add(designcode);
                }
                rowindex++;
            }
            if (missingdesigncodes.Count > 0)
                throw new Exception("Can't find the design code - " + string.Join("<br>", missingdesigncodes));

            var alluids = excelDocument.rows.ConvertAll(x => @"{""uid"":""" + x.values[0] + @"""}").Distinct();

            string query = @"
with alluid
as
(
select schemelist->>'uid' uid,cast(schemelist->>'sizeid' as bigint) sizeid,mfgdesign.code,mfgdesign.productid,
cast(schemelist->>'itemid' as bigint) itemid,cast(schemelist->>'colourid' as bigint) colourid,
cast(schemelist->>'sleeveid' as bigint) sleeveid,
cast(schemelist->>'wsrate' as numeric) wsrate,mfgdesign.brandid from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where mfgdesign.isactive = true
),uniquestock as
(
select distinct stock.serialno serialno from stock
)
select alluid.* from json_array_elements('[" + string.Join(",", alluids) + @"]') alllist
inner join alluid on alluid.uid = alllist->>'uid'
left outer join uniquestock on uniquestock.serialno = alluid.uid 
where uniquestock.serialno is null";

            var dset = this.ExecuteDataSet(UpdateQueryForHyper(query), new SortedList<string, object>());
            var result = from d in dset.Tables[0].AsEnumerable()
                         select new
                         {
                             sizeid = d.Field<long>("sizeid"),
                             productid = d.Field<long>("productid"),
                             itemid = d.Field<long>("itemid"),
                             brandid = d.Field<long>("brandid"),
                             wsrate = d.Field<decimal>("wsrate"),
                             uid = d.Field<string>("uid"),
                             code = d.Field<string>("code")
                         };

            StockDataService stockDataService = new StockDataService() { Context = this.Context };
            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            var companylist = companyDataService.Search(new CompanyCreteria());

            foreach (var item in result)
            {
                var productlist = productsDataService.SearchList(new ProductsCreteria() { id = item.productid });
                var stock = stockDataService.Insert(new Stock()
                {
                    barcode = item.uid.Replace("-", "") + "-" + "AUDIT",
                    serialno = item.uid,
                    designid = item.code,
                    itemid = item.itemid,
                    productid = item.productid,
                    brandid = item.brandid,
                    buyingprice = item.wsrate,
                    ibuyingprice = item.wsrate,
                    buyingqty = 0,
                    companyid = companylist.First().id,
                    isactive = true,
                    printingname = productlist.First().name,
                    price = item.wsrate,
                    version = 1,
                    eprice = item.wsrate,
                    transfered = 0
                });

            }

            int q = 0;
            excelDocument.rows.ForEach(x =>
            {
                if (int.TryParse(x.values[1], out q))
                    x.values[1] = q.ToString();
            });

            excelDocument.rows = excelDocument.rows.ConvertAll(x => new { uid = x.values[0], qty = x.values[1] }).GroupBy(x => x.uid).ToList().ConvertAll(x =>
            {
                var r = new ImageController.ExcelDocument.Row();
                r.values.Add(x.Key);
                r.values.Add(x.Sum(y => int.Parse(y.qty)).ToString());
                return r;
            });

            return excelDocument;
        }

        public Mfgdesign Update(NpgsqlTransaction transaction, NpgsqlConnection conn,
            Mfgdesign oMfgdesign, ChangeAttributes changeAttributes)
        {
            if (oMfgdesign.productid == 0) throw new Exception("Please select the product");
            UpdateUID(transaction, conn, oMfgdesign);

            var param = new SortedList<string, object>();
            string lastCode = "";
            string hsnCode = "";
            param.Add("id", oMfgdesign.id);
            param.Add("designid", oMfgdesign.code);
            using (var reader = ExecuteReader("select code,prdhsncode from mfgdesign where id=@id", param))
            {
                if (reader.Read())
                {
                    lastCode = reader["code"].ToString();
                    hsnCode = reader["prdhsncode"] == DBNull.Value ? "" : reader["prdhsncode"].ToString();
                }
            }
            if (lastCode != oMfgdesign.code)
            {
                using (var reader = ExecuteReader("select serialno,stockdate from stock where designid=@designid limit 10", param))
                {
                    if (reader.Read())
                    {
                        var serialno = reader["serialno"].ToString();
                        var stockdate = Convert.ToDateTime(reader["stockdate"]);
                        throw new Exception("Can't change the design code, this design already has stock and generated " + serialno + " on " + stockdate.ToString("dd-MM-yyyy"));
                    }
                }
            }

            //if(hsnCode!= oMfgdesign.prdhsncode)
            //{
            //    int cnt = ExecuteQuery("Update stock hsncode=@hsncode where designid=@designid and itemid in (select id from items where glndesignid=@designid)",
            //        param);

            //}
            if (string.IsNullOrEmpty(oMfgdesign.code))
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("mfgdesign.designcode", 0);
                oMfgdesign.code = sequence.GetNextNumber(Context, conn, transaction);
            }

            var list = oMfgdesign.sizegroup.items.FindAll(e => string.IsNullOrEmpty(e.code));
            if (list.Count > 0)
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("mfgdesign.designitemcode", 0);
                for (int index = 0; index < list.Count; index++)
                {
                    list[index].code = sequence.GetNextNumber(Context, conn, transaction);
                }
            }

            var itemlist = oMfgdesign.sizegroup.items.FindAll(e => e.itemid == 0);
            if (itemlist.Count > 0)
            {
                ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
                ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
                var productname = productsDataService.SearchList(new ProductsCreteria()
                {
                    id = oMfgdesign.productid
                }).First().name;

                foreach (var item in itemlist)
                {
                    var itemmaster = new ItemsCreteria()
                    {
                        productid = oMfgdesign.productid,
                        brandid = oMfgdesign.brandid,
                        typeid = oMfgdesign.typeid,
                        styleid = oMfgdesign.styleid,
                        patternid = oMfgdesign.patternid,
                        fitid = oMfgdesign.fitid,
                        seasonid = oMfgdesign.seasonid,
                        materialid = oMfgdesign.materialid,
                        sleeveid = item.sleeveid,
                        colourid = item.colourid,
                        sizeid = item.sizeid,
                        gln = item.uid,
                        mrp = item.mrp,
                        glndesignid = oMfgdesign.code,
                        support_gln = true,
                        productmap = true
                    };
                    var searchitemlist = itemsDataService.Search(itemmaster);
                    if (searchitemlist.Count == 0)
                    {
                        if (string.IsNullOrEmpty(item.name))
                        {
                            itemmaster.productname = productname;
                            itemmaster.printingname = productname;
                            itemmaster.sellingname = productname;
                        }
                        else
                        {
                            itemmaster.productname = item.name;
                            itemmaster.printingname = item.name;
                            itemmaster.sellingname = item.name;
                        }
                        item.itemid = itemsDataService.Insert(transaction, conn, itemmaster).id;
                    }
                    else
                    {
                        item.itemid = searchitemlist.First().id;
                    }
                }
            }
            if (oMfgdesign.sizegroup != null && oMfgdesign.sizegroup.items != null && oMfgdesign.sizegroup.items.Count > 0)
            {
                oMfgdesign.sizegroup.items.ForEach(x => x.barcodes = null);
            }
            NpgsqlCommand command = null;
            if (ConfigManager.Instance(Context.OrganizationID).Config.system_customfields &&
                CustomFieldsManager.Instance(Context.OrganizationID).Config != null &&
                CustomFieldsManager.Instance(Context.OrganizationID).Config.Count > 0)
            {
                var flist = CustomFieldsManager.Instance(Context.OrganizationID).Config.ConvertAll(x => "mfg_" + x.cfname + "=@mfg_" + x.cfname);
                var query = string.Format(SQL_Update, "," + string.Join(",", flist));
                command = GetCommand(query, conn, transaction);
                AddCustomFields(oMfgdesign, command);
            }
            else
            {
                command = GetCommand(string.Format(SQL_Update, ""), conn, transaction);
            }
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesign.code == null ? "" : oMfgdesign.code;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesign.name == null ? "" : oMfgdesign.name;
            command.Parameters.Add("racknumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesign.racknumber == null ? "" : oMfgdesign.racknumber;
            command.Parameters.Add("prdhsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesign.prdhsncode == null ? "" : oMfgdesign.prdhsncode;
            command.Parameters.Add("prddivisionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.prddivisionid;
            command.Parameters.Add("displayname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgdesign.displayname == null ? "" : oMfgdesign.displayname;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.productid;
            command.Parameters.Add("brandid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.brandid;
            command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.sleeveid;
            command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.typeid;
            command.Parameters.Add("styleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.styleid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.colourid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.supplierid;
            command.Parameters.Add("fitid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.fitid;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.taxid;
            command.Parameters.Add("sizemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.sizemode;
            command.Parameters.Add("classid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.classid;
            command.Parameters.Add("iscoredesign", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgdesign.iscoredesign;
            command.Parameters.Add("seasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.seasonid;
            command.Parameters.Add("boxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.boxid;
            command.Parameters.Add("autonumber", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.autonumber;
            command.Parameters.Add("pack", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.pack;
            command.Parameters.Add("cutratio", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgdesign.cutratio;
            command.Parameters.Add("mfgmonth", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.mfgmonth;
            command.Parameters.Add("mfgyear", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.mfgyear;
            command.Parameters.Add("patternid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.patternid;
            command.Parameters.Add("materialid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.materialid;
            command.Parameters.Add("fabricid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.fabricid;
            command.Parameters.Add("sizegroup", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgdesign.sizegroup_json == null ? "null" : oMfgdesign.sizegroup_json;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgdesign.isactive;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgdesign.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgdesign.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.version;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.modifiedby;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.id;
            command.Parameters.Add("imagelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgdesign.imagelist_json == null ? "null" : oMfgdesign.imagelist_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgdesign.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            if (changeAttributes != null)
            {
                if (oMfgdesign.sizegroup.items != null &&
                    oMfgdesign.sizegroup.items.Exists(x => x.modified))
                {
                    var lst = oMfgdesign.sizegroup.items.FindAll(x => x.modified);
                    List<string> mfgjoborderitemslist = new List<string>();
                    List<string> mfgworkordergrnitemsizeslist = new List<string>();

                    if (changeAttributes.criteria.colourChanged)
                        mfgjoborderitemslist.Add(" colourid=@colourid ");
                    if (changeAttributes.criteria.productChanged)
                        mfgjoborderitemslist.Add(" productid=@productid ");
                    if (changeAttributes.criteria.sizeChanged)
                        mfgjoborderitemslist.Add(" sizeid=@sizeid ");
                    string mfgjoborderitemscondition = string.Join(",", mfgjoborderitemslist);
                    if (changeAttributes.criteria.sleeveChanged)
                        mfgjoborderitemslist.Add(" sleeveid=@sleeveid ");

                    mfgjoborderitemslist.RemoveAll(x => x == " productid=@productid ");
                    string mfgworkordergrnitemsizescondition = string.Join(",", mfgjoborderitemslist);

                    string query = @"
update mfgjoborderitems set {0}
where mfgworkorderitemsizesid in (select id from mfgworkorderitemsizes where uid=@uid);";

                    query = string.Format(
                        (mfgjoborderitemscondition.Length > 0 ? query : "") +
                        @"update mfgworkordergrnitemsizes set itemid=@itemid{1} where uid=@uid;
update mfgworkorderitemsizes set itemid=@itemid where uid=@uid;
update stock set itemid=@itemid where stock.serialno = @uid", mfgjoborderitemscondition,
(mfgworkordergrnitemsizescondition.Length > 0 ? "," : "") + mfgworkordergrnitemsizescondition);

                    foreach (var litem in lst)
                    {
                        if (oMfgdesign.sizegroup.items.Exists(x => x.uid == litem.uid))
                        {
                            param = new SortedList<string, object>();
                            param.Add("uid", litem.uid);
                            param.Add("itemid", oMfgdesign.sizegroup.items.Find(x => x.uid == litem.uid).itemid);
                            param.Add("productid", changeAttributes.criteria.productid);
                            param.Add("sizeid", changeAttributes.criteria.sizeid);
                            param.Add("brandid", changeAttributes.criteria.brandid);
                            param.Add("colourid", changeAttributes.criteria.colourid);
                            param.Add("sleeveid", changeAttributes.criteria.sleeveid);
                            command = new NpgsqlCommand(UpdateQueryForHyper(query), conn, transaction);
                            UpdateParameters(command, param);
                            Task<DbDataReader> readerTask = command.ExecuteReaderAsync();
                            DateTime dateTime = DateTime.Now;
                            while (!readerTask.IsCompleted && DateTime.Now.Subtract(dateTime).TotalMinutes < 5)
                            {
                                readerTask.Wait(new TimeSpan(0, 0, 15));
                            }
                            if (DateTime.Now.Subtract(dateTime).TotalMinutes > 5)
                            {
                                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Design attributes change delayed"
                                    , WebAPI.Common.Logger.LogType.Error, GetSQL(command));
                                throw new Exception("Design attributes change delayed");
                            }
                            else
                            {
                                using (readerTask.Result)
                                {
                                    if (readerTask.Result.Read())
                                    {
                                        param.Clear();
                                    }
                                }
                            }
                        }
                        else
                        {
                            throw new Exception("Can't delete the UID while updating the attributes - " + litem.uid);
                        }
                    }
                }
            }
            return oMfgdesign;
        }


        public bool Delete(Mfgdesign oMfgdesign)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfgdesign);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgdesign);

                throw exp;
            }
            return hasSuccessCount;

        }

        public List<MfgDesignItem> SearchDesignList(List<string> uanlist)
        {
            if (uanlist == null || uanlist.Count == 0) return new List<MfgDesignItem>();

            string query = @"select mfgdesign.id,mfgdesign.code,
schemelist->>'sizegroup' sizegroup,schemelist->>'uid' uid,schemelist->>'sizeid' sizeid,
schemelist->>'wsrate' wsrate,schemelist->>'dealerprice' dealerprice,schemelist->>'salerate' salerate,
schemelist->>'mrp' mrp,schemelist->>'code' designitemcode,schemelist->>'name' printingname,
schemelist->>'colourid' colourid,schemelist->>'sleeveid' sleeveid,
cast(schemelist->>'alloworder' as boolean) alloworder,mfgdesign.productid,
size.name size,sleeve.name sleeve,colour.name colour,prdhsncode,schemelist->>'prdhsn' prdhsn
from mfgdesign 
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
left outer join referencelist colour on colour.id = cast(schemelist->>'colourid' as bigint)
left outer join referencelist size on size.id = cast(schemelist->>'sizeid' as bigint)
left outer join referencelist sleeve on sleeve.id = cast(schemelist->>'sleeveid' as bigint)
where mfgdesign.isactive=true and cast(schemelist->>'uid' as varchar) in (" + string.Join(",", uanlist.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")) + ")";

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_mfgdesign)
            {
                query += " and mfgdesign.isactive=true and mfgdesign.activestatus=2";
            }
            List<MfgDesignItem> lst = new List<MfgDesignItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = command = new NpgsqlCommand(query, conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MfgDesignItem omfgdesign = new MfgDesignItem();
                        omfgdesign.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        omfgdesign.uid = reader["uid"] == DBNull.Value ? "" : reader["uid"].ToString();
                        omfgdesign.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        omfgdesign.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        omfgdesign.sleeve = reader["sleeve"] == DBNull.Value ? "" : reader["sleeve"].ToString();
                        omfgdesign.colour = reader["colour"] == DBNull.Value ? "" : reader["colour"].ToString();
                        omfgdesign.hsn = reader["prdhsncode"] == DBNull.Value ? "" : reader["prdhsncode"].ToString();

                        if (reader["prdhsn"] != DBNull.Value &&
                            reader["prdhsn"].ToString() != "")
                            omfgdesign.hsn = reader["prdhsn"].ToString();

                        omfgdesign.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        omfgdesign.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);

                        omfgdesign.designitemcode = reader["designitemcode"] == DBNull.Value ? "" : reader["designitemcode"].ToString();
                        if (omfgdesign.designitemcode.IndexOf('/') > 0)
                            omfgdesign.colourcode = omfgdesign.designitemcode.Substring(omfgdesign.designitemcode.IndexOf('/') + 1);
                        omfgdesign.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgdesign.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgdesign.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        omfgdesign.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        omfgdesign.alloworder = reader["alloworder"] == DBNull.Value ? true : Convert.ToBoolean(reader["alloworder"]);
                        omfgdesign.wsrate = reader["wsrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wsrate"]);
                        omfgdesign.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        omfgdesign.dealerprice = reader["dealerprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dealerprice"]);
                        omfgdesign.salerate = reader["salerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salerate"]);
                        lst.Add(omfgdesign);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, uanlist);
            }

            return lst;
        }
        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgdesign oMfgdesign)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgdesign.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgdesign.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgdesign.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgdesign.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }


        internal bool ChangeStatus(ChangeAttributes item)
        {

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();

                    NpgsqlCommand command = GetCommand("update mfgdesign set activestatus=@activestatus,modifiedby=@modifiedby,modifiedon=@modifiedon where id in (" + string.Join(",", item.ids) + ")", conn);
                    command.Parameters.Add("activestatus", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.statusid;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    int successCount = command.ExecuteNonQuery();
                    conn.Close();
                    return successCount > 0;
                }
            }
            catch (Exception exp)
            {
                return false;
            }
        }
        internal bool Change2Core(ChangeAttributes item)
        {
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand("update mfgdesign set iscoredesign=@iscoredesign,modifiedby=@modifiedby,modifiedon=@modifiedon where id in (" + string.Join(",", item.ids) + ")", conn);
                command.Parameters.Add("iscoredesign", NpgsqlTypes.NpgsqlDbType.Boolean).Value = item.iscoredesign;
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                int successCount = command.ExecuteNonQuery();
                conn.Close();
                return successCount > 0;
            }
            catch (Exception exp)
            {
                return false;
            }
        }
        internal bool ChangeClassid(ChangeAttributes item)
        {
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand("update mfgdesign set classid=@classid,modifiedby=@modifiedby,modifiedon=@modifiedon where id in (" + string.Join(",", item.ids) + ")", conn);
                command.Parameters.Add("classid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.classid;
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                int successCount = command.ExecuteNonQuery();
                conn.Close();
                return successCount > 0;
            }
            catch (Exception exp)
            {
                return false;
            }
        }
        internal bool ChangeAtributes(ChangeAttributes item)
        {
            var coresku = Search(new MfgdesignCreteria()
            {
                id = item.sku.id
            }).First();
            var uids = item.items.ConvertAll(x => x.uid);
            var changeditems = coresku.sizegroup.items.FindAll(x => uids.Contains(x.uid));
            changeditems.ForEach(x =>
            {
                x.itemid = 0;
                x.modified = true;
                if (item.criteria.colourChanged)
                    x.colourid = item.criteria.colourid;
                if (item.criteria.sleeveChanged)
                    x.sleeveid = item.criteria.sleeveid;
                if (item.criteria.sizeChanged)
                    x.sizeid = item.criteria.sizeid;
            });
            if (item.criteria.brandChanged)
                coresku.brandid = item.criteria.brandid;
            if (item.criteria.colourChanged)
                coresku.colourid = item.criteria.colourid;
            if (item.criteria.fitChanged)
                coresku.fitid = item.criteria.fitid;
            if (item.criteria.materialChanged)
                coresku.materialid = item.criteria.materialid;
            if (item.criteria.patternChanged)
                coresku.patternid = item.criteria.patternid;
            if (item.criteria.productChanged)
                coresku.productid = item.criteria.productid;
            if (item.criteria.sleeveChanged)
                coresku.sleeveid = item.criteria.sleeveid;
            if (item.criteria.styleChanged)
                coresku.styleid = item.criteria.styleid;
            if (item.criteria.typeChanged)
                coresku.typeid = item.criteria.typeid;

            Update(coresku, item);
            return true;
        }

        internal bool SetComboConfig(ComboConfig item)
        {
            const string SQL_UpdateImage = @"update public.mfgdesign set comboconfig=@comboconfig,hascombo=@hascombo,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id";
            int successCount = 0;
            try
            {
                List<string> confictlist = new List<string>();
                var designconfig = Search(new MfgdesignCreteria()
                {
                    id = item.Id,
                    includedetail = true
                }).First();
                List<string> designcodes = new List<string>();
                item.Configlist.ForEach(x =>
                {
                    if (x.Limitdesign && x.DesignList != null && x.DesignList != "")
                    {
                        designcodes.AddRange(x.DesignList.Split(','));
                    }
                });
                designcodes.RemoveAll(x => string.IsNullOrWhiteSpace(x));
                var comboitemconfig = Search(new MfgdesignCreteria()
                {
                    codes = designcodes,
                    includedetail = true
                });

                item.UANList = new List<ComboConfig.ComboUANConfig>();
                var setcolours = designconfig.sizegroup.items.ConvertAll(x => x.colourid).Distinct().ToList();
                foreach (var pitem in item.Configlist)
                {
                    if (pitem.Limitdesign && pitem.DesignList != null && pitem.DesignList != "")
                    {
                        var dlist = pitem.DesignList.Split(',');

                        if (comboitemconfig.Any(x => x.productid != pitem.Productid && dlist.Contains(x.code)))
                        {
                            throw new Exception(comboitemconfig.Find(x => x.productid != pitem.Productid).code + " design not belong to the product your have selected");
                        }
                    }

                    if (pitem.PrimaryColour && comboitemconfig.Exists(x => x.name == pitem.DesignList))
                    {
                        var sitemcolours = comboitemconfig.Find(x => x.name == pitem.DesignList).sizegroup.items.ConvertAll(x => x.colourid).Distinct().ToList();
                        if (setcolours.Exists(x =>
                        {
                            return !sitemcolours.Contains(x);
                        }))
                        {
                            var extracolour = setcolours.FindAll(x =>
                             {
                                 return !sitemcolours.Contains(x);
                             });

                            var extracolouruans = designconfig.sizegroup.items.FindAll(x => extracolour.Contains(x.colourid)).ConvertAll(x => x.name).Distinct().ToList();
                            confictlist.Add("Extra colour designs - " + String.Join(",", extracolouruans));
                        }
                    }
                }

                ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
                var products = productsDataService.SearchList(new ProductsCreteria()
                {
                    ids = item.Configlist.ToList().ConvertAll(x => x.Productid)
                });

                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var allsizes = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    type = "SIZE"
                });

                var misingsetsize = new List<string>();
                foreach (var uanitem in designconfig.sizegroup.items)
                {
                    var sizeList = new List<long>();
                    var sizeInfo = allsizes.Find(x => x.id == uanitem.sizeid);
                    if (sizeInfo != null && sizeInfo.additionalinfo != null &&
                        sizeInfo.additionalinfo.billdiscountinclude && sizeInfo.additionalinfo.sizegroups != null)
                    {
                        sizeList = sizeInfo.additionalinfo.sizegroups;
                        if (sizeList == null) sizeList = new List<long>();
                    }
                    if (sizeList.Count <= 1 && item.Configlist.Exists(x => x.SecondarySize))
                    {
                        misingsetsize.Add(sizeInfo.name);
                        continue;
                        throw new Exception("Size Group doesn't configured for " + sizeInfo.name);
                    }

                    foreach (var pitem in item.Configlist)
                    {
                        ComboConfig.ComboUANConfig uconfig = new ComboConfig.ComboUANConfig() { uan = uanitem.uid, productid = pitem.Productid, qty = pitem.Productqty, uans = new List<string>() };
                        item.UANList.Add(uconfig);
                        if (pitem.Limitdesign && pitem.DesignList != null && pitem.DesignList != "")
                        {
                            var dlist = pitem.DesignList.Split(',');
                            foreach (var ditem in dlist)
                            {

                                var sizegroupitems = comboitemconfig.Find(x => x.code == ditem).sizegroup.items;
                                var colouritems = sizegroupitems.FindAll(x => x.colourid == uanitem.colourid);
                                var sizeitems = sizegroupitems.FindAll(x => x.sizeid == uanitem.sizeid);
                                var coloursizeitems = sizegroupitems.FindAll(x => x.colourid == uanitem.colourid && x.sizeid == uanitem.sizeid);

                                if (sizeList.Count > 1)
                                {
                                    var primarysizeitems = sizegroupitems.FindAll(x => x.sizeid == sizeList[0]);
                                    var secondarysizeitems = sizegroupitems.FindAll(x => x.sizeid == sizeList[1]);
                                    var pscoloursizeitems = sizegroupitems.FindAll(x => x.colourid == uanitem.colourid && x.sizeid == sizeList[0]);
                                    var sccoloursizeitems = sizegroupitems.FindAll(x => x.colourid == uanitem.colourid && x.sizeid == sizeList[1]);
                                }

                                var alluans = sizegroupitems.FindAll(x =>
                                {
                                    if (pitem.PrimaryColour && uanitem.colourid != x.colourid) return false;
                                    if (pitem.PrimarySleeve && uanitem.sleeveid != x.sleeveid) return false;
                                    if (pitem.PrimarySize && uanitem.sizeid != x.sizeid && (sizeList.Count == 0 || (sizeList.Count > 0 && sizeList[0] != x.sizeid))) return false;
                                    if (pitem.SecondarySize && (sizeList.Count < 2 || (sizeList.Count > 1 && sizeList[1] != x.sizeid))) return false;
                                    if (pitem.ThirdSize && sizeList.Count > 2 && sizeList[2] != x.sizeid) return false;
                                    return true;
                                });
                                uconfig.uans = alluans.ConvertAll(x => x.uid);
                            }
                        }
                    }
                }

                foreach (var pitem in item.Configlist)
                {
                    if (pitem.Limitdesign && pitem.DesignList != null && pitem.DesignList != "")
                    {
                        var sizegroupitems = comboitemconfig.Find(x => x.code == pitem.DesignList).sizegroup.items;
                        var missinguid = sizegroupitems.FindAll(x =>
                        {
                            return !item.UANList.Exists(y => y.uans.Contains(x.uid));
                        });
                        if (missinguid.Count > 0)
                        {
                            var productid = comboitemconfig.Find(x => x.code == pitem.DesignList).productid;
                            var zerouan = item.UANList.FindAll(x => x.productid == productid);
                            zerouan.ForEach(x =>
                                {
                                    x.uans.AddRange(missinguid.ConvertAll(y => y.uid));
                                    x.optional = true;
                                });
                        }
                    }
                }


                if (misingsetsize.Count > 0)
                    confictlist.Add("Set size config missing - " + String.Join(",", misingsetsize));

                List<string> mismatchuans = new List<string>();

                var availableuans = item.UANList.FindAll(X => X.uans.Count > 0);

                foreach (var conflits in item.UANList.FindAll(X => X.uans.Count == 0))
                {
                    var prod = products.Find(x => x.id == conflits.productid);
                    mismatchuans.Add(conflits.uan);
                    //confictlist.Add(conflits.uan + " " + (prod != null ? (prod.name + "Size or Colour missing") : ("Product,Size and Colour missing")));
                }

                var multipleuan = item.UANList.FindAll(X => X.uans.Count == 0).GroupBy(x => x.uan).ToList().ConvertAll(x => x.Key);

                item.UANList.RemoveAll(x => mismatchuans.Contains(x.uan));

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_UpdateImage, conn);
                    command.Parameters.Add("comboconfig", NpgsqlTypes.NpgsqlDbType.Json).Value = item == null ? "null" : Newtonsoft.Json.JsonConvert.SerializeObject(item);
                    command.Parameters.Add("hascombo", NpgsqlTypes.NpgsqlDbType.Boolean).Value = item == null || item.Qty == 0 ? false : true;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.Id;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTime.Now;
                    successCount = command.ExecuteNonQuery();
                    conn.Close();
                }

                if (confictlist.Count > 0)
                {
                    throw new Exception(string.Join("\r\n", confictlist));
                }
                return true;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,
                    item);
                throw exp;
            }
            return successCount > 0;
        }


        internal ComboConfig GetComboConfig(ComboConfig item)
        {
            const string SQL_UpdateImage = @"select comboconfig from public.mfgdesign where id=@id and hascombo=true";
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_UpdateImage, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.Id;
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read() && !reader.IsDBNull(0))
                        {
                            item = Newtonsoft.Json.JsonConvert.DeserializeObject<ComboConfig>(reader.GetString(0));
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,
                    item);
                throw exp;
            }
            return item;
        }

        internal List<Mfgdesign.Sizegroup.SearchItem> SearchUANList(MfgdesignCreteria item)
        {
            if (string.IsNullOrEmpty(item.name) &&
                string.IsNullOrEmpty(item.code) &&
                string.IsNullOrEmpty(item.uancode))
                return new List<Mfgdesign.Sizegroup.SearchItem>();
            var uanitems = SearchUANItem(new Mfgdesign.Sizegroup.SearchItem()
            {
                code = item.code,
                name = item.name,
                uid = item.uancode,
                sleeveid = item.sleeveid,
                colourid = item.colourid
            });
            return uanitems;
        }
        internal List<Mfgdesign.Sizegroup.SearchItem> SearchUAN(MfgdesignCreteria item)
        {
            List<Mfgdesign.Sizegroup.SearchItem> response = new List<Mfgdesign.Sizegroup.SearchItem>();
            if (string.IsNullOrEmpty(item.name) &&
                string.IsNullOrEmpty(item.code) &&
                string.IsNullOrEmpty(item.uancode))
                return response;


            var uanitems = SearchUANItem(new Mfgdesign.Sizegroup.SearchItem()
            {
                code = item.code,
                name = item.name,
                uid = item.uancode,
                sleeveid = item.sleeveid,
                colourid = item.colourid,
                sizeid = item.sizeid
            });
            if (uanitems.Count > 0)
                return uanitems;


            var designinfo = Search(new MfgdesignCreteria()
            {
                name = item.name,
                code = item.code,
                includedetail = true,
                uancode = item.uancode
            });
            if (designinfo.Count > 0 &&
                designinfo[0].sizegroup.items != null)
            {
                if (designinfo[0].sizegroup.items.Count == 1)
                {
                    response.Add(designinfo[0].sizegroup.Create(designinfo[0].sizegroup.items[0], designinfo[0]));
                    return response;
                }

                if (!string.IsNullOrEmpty(item.uancode) &&
                    designinfo[0].sizegroup.items.Exists(x => x.uid == item.uancode))
                {
                    response.Add(designinfo[0].sizegroup.Create(designinfo[0].sizegroup.items.Find(x => x.uid == item.uancode), designinfo[0]));
                    return response;
                }

                if (designinfo[0].sizegroup.items.Exists(x => x.colourid == item.colourid))
                {
                    var colouritem = designinfo[0].sizegroup.items.FindAll(x => x.colourid == item.colourid);
                    if (item.sizeid > 0)
                        colouritem.RemoveAll(x => x.sizeid != item.sizeid);

                    if (item.sleeveid > 0 && colouritem.Count > 1)
                        colouritem.RemoveAll(x => x.sleeveid != item.sleeveid);

                    if (colouritem.Count == 1)
                    {
                        response.Add(designinfo[0].sizegroup.Create(colouritem[0], designinfo[0]));
                        return response;
                    }

                    if (colouritem.Count == 0)
                    {
                        throw new Exception("Can't find the relevent UAN of the design - " + item.name);
                    }
                    else if (colouritem.Count > 1)
                    {
                        throw new Exception("The given design - " + item.name + " contains more possible finished goods");
                    }
                }
            }

            designinfo = Search(new MfgdesignCreteria() { code = item.uancode, includedetail = true });
            if (designinfo.Count == 0)
                throw new Exception("The given design - " + item.name + " not available ");

            foreach (var sizeitem in designinfo[0].sizegroup.items)
                response.Add(designinfo[0].sizegroup.Create(sizeitem, designinfo[0]));
            return response;
        }

        public string UpdateQueryForHyper(string query)
        {
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan)
                return query.Replace("stock.serialno", "stock.barcode");
            return query;
        }


        internal string CreateZeroStock(long id)
        {
            var mfgdesign = Search(new MfgdesignCreteria()
            {
                id = id,
            }).First();
            var barcodeList = mfgdesign.sizegroup.items.ConvertAll(x => x.uid);
            var totalbarcodes = barcodeList.Count;
            var existbarcodes = 0;
            var newlycreated = 0;
            var barcodestring = String.Join(",", barcodeList.ConvertAll(x => "'" + x + "'"));
            List<string> onlyretailbarcodes = new List<string>();
            using (var reader = ExecuteReader("Select serialno from stock where serialno in (" + barcodestring + ")", null))
            {
                while (reader.Read())
                {
                    string barcode = reader.GetString(0);
                    barcodeList.Remove(barcode);
                    onlyretailbarcodes.Add(barcode);
                }
            }
            if (totalbarcodes != barcodeList.Count)
            {
                existbarcodes = totalbarcodes - barcodeList.Count;
            }
            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            var companylist = companyDataService.Search(new CompanyCreteria());
            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var allretail = referencelistDataService.SearchMyRetailLocation(new ReferencelistCreteria());
            if (barcodeList.Count > 0)
            {
                StockDataService stockDataService = new StockDataService() { Context = this.Context };
                SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
                foreach (var barcode in barcodeList)
                {
                    newlycreated++;
                    var newbarcode = barcode + "-1";
                    int barcodeid = 0;
                    while (true)
                    {
                        barcodeid++;
                        var param = new SortedList<string, object>();
                        newbarcode = barcode + "-" + barcodeid;
                        param.Add("barcode", newbarcode);
                        using (var reader = ExecuteReader("Select barcode from stock where barcode=@barcode", param))
                        {
                            if (reader.Read())
                                continue;
                        }
                        break;
                    }
                    var item = mfgdesign.sizegroup.items.Find(x => x.uid == barcode);
                    var stock = stockDataService.Insert(new Stock()
                    {
                        serialno = item.uid,
                        itemid = item.itemid,
                        price = item.wsrate,
                        barcode = newbarcode,
                        productid = mfgdesign.productid,
                        buyingprice = item.cost,
                        ibuyingprice = item.cost,
                        hsncode = mfgdesign.prdhsncode,
                        isactive = true,
                        companyid = companylist.First().id,
                        createdby = Context.UserID,
                        modifiedby = Context.UserID,
                        createdon = DateTimeNow,
                        displaymrp = item.mrp,
                        modifiedon = DateTimeNow,
                        qty = 0,
                        transfered = 0
                    });
                    foreach (var retail in allretail)
                    {
                        salablegoodsDataService.Insert(new Salablegoods()
                        {
                            stockid = stock.id,
                            locationid = retail.id,
                            barcode = newbarcode,
                            iqty = 0,
                            price = item.wsrate,
                            isactive = true,
                            createdby = Context.UserID,
                            modifiedby = Context.UserID,
                            createdon = DateTimeNow,
                            modifiedon = DateTimeNow,
                            companyid = retail.additionalinfo.parentid > 0 ? retail.additionalinfo.parentid : companylist.First().id
                        });
                    }
                }
            }

            if (onlyretailbarcodes.Count > 0)
            {
                StockDataService stockDataService = new StockDataService() { Context = this.Context };
                SalablegoodsDataService salablegoodsDataService = new SalablegoodsDataService() { Context = this.Context };
                foreach (var barcode in onlyretailbarcodes)
                {
                    var retaillocations = allretail.ConvertAll(x => x.id).ToList();
                    var param = new SortedList<string, object>();
                    param.Add("barcode", barcode);
                    using (var reader = ExecuteReader(@"
                        Select distinct salablegoods.locationid from stock 
                        inner join salablegoods on salablegoods.stockid = stock.id and salablegoods.isactive=true
                        where serialno=@barcode and stock.isactive=true", param))
                    {
                        while (reader.Read())
                        {
                            var retailid = reader.GetInt64(0);
                            if (retaillocations.Contains(retailid))
                            {
                                retaillocations.Remove(retailid);
                            }
                        }
                    }
                    Stock stock = null;
                    using (var reader = ExecuteReader(@"
                        Select stock.id from stock 
                        where serialno=@barcode and stock.isactive=true
                        limit 1", param))
                    {
                        if (reader.Read())
                        {
                            var retailid = reader.GetInt64(0);
                            stock =  stockDataService.Search(new StockCreteria() { id = retailid }).First();
                        }
                    }

                    var item = mfgdesign.sizegroup.items.Find(x => x.uid == barcode);
                    foreach (var retailid in retaillocations)
                    {
                        var price = item.salerate;
                        if(price == 0)
                            price = item.wsrate;
                        if (price == 0)
                            price = item.mrp;
                        var retail = allretail.Find(x => x.id == retailid);
                        salablegoodsDataService.Insert(new Salablegoods()
                        {
                            stockid = stock.id,
                            locationid = retail.id,
                            barcode = stock.barcode,
                            iqty = 0,
                            price = price,
                            isactive = true,
                            createdby = Context.UserID,
                            modifiedby = Context.UserID,
                            createdon = DateTimeNow,
                            modifiedon = DateTimeNow,
                            companyid = retail.additionalinfo.parentid > 0 ? retail.additionalinfo.parentid : companylist.First().id
                        });
                    }
                }
            }

            return "Total Barcodes :" + totalbarcodes + " Exist :" + existbarcodes + " Created : " + newlycreated;
        }

        public void UpdateSizeGroup(List<Mfgdesign> currentdesigns)
        {
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                currentdesigns.ForEach(x =>
                {
                    NpgsqlCommand command = new NpgsqlCommand("UPDATE public.mfgdesign SET sizegroup=@sizegroup,imagelist=@imagelist,modifiedon=@modifiedon,modifiedby=@modifiedby where id=@id", conn, transaction);
                    command.Parameters.Add("imagelist", NpgsqlTypes.NpgsqlDbType.Json).Value = x.imagelist_json;
                    command.Parameters.Add("sizegroup", NpgsqlTypes.NpgsqlDbType.Json).Value = x.sizegroup_json == null ? "null" : x.sizegroup_json;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTime.Now;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = x.id;
                    int successCount = command.ExecuteNonQuery();

                });
                transaction.Commit();
                conn.Close();
            }

        }
        internal List<MfgdesignProcess> ImportSKU(List<MfgdesignProcess> items)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                var existingids = items.FindAll(x => x.id > 0).ConvertAll(x => x.id);
                List<Mfgdesign> currentdesigns = null;
                if (existingids.Count > 0)
                {
                    currentdesigns = Search(new MfgdesignCreteria()
                    {
                        ids = existingids,
                        includedetail = true
                    });
                }
                if (items.Exists(x => x.sizegroup == null || x.sizegroup.items == null))
                {
                    ImageData imageData = new ImageData() { Context = this.Context };
                    List<string> exceptions = new List<string>();
                    foreach (var itm in items.GroupBy(x => x.id))
                    {
                        var design = currentdesigns.Find(x => x.id == itm.Key);
                        if (design.sizegroup.colouritems == null)
                        {
                            design.sizegroup.colouritems = new List<Mfgdesign.Sizegroup.ColourItem>();
                        }
                        foreach (var img in itm)
                        {
                            string file = img.imagelist[0];
                            if (file.StartsWith("http") || file.StartsWith("www"))
                            {
                                using (System.Net.Http.HttpClient client = new System.Net.Http.HttpClient())
                                {
                                    try
                                    {
                                        Console.WriteLine("Downloading image...");
                                        byte[] imageBytes = client.GetByteArrayAsync(file).Result;

                                        var filedata = new ImageData.File()
                                        {
                                            isactive = true,
                                            createdby = Context.UserID,
                                            createdon = DateTime.Now,
                                            type = "skuimage",
                                            filename = file,
                                            ext = file.EndsWith("jpg") ? "JPG" : "PNG"
                                        };

                                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream(imageBytes))
                                        {
                                            System.Drawing.Image imgstr = System.Drawing.Image.FromStream(ms);
                                            var thumbimg = WebAPI.Common.ImageManager.ScaleBySize(imgstr, 64);
                                            filedata.thumbnail = WebAPI.Common.ImageManager.ToByteArray(thumbimg, 90);
                                            filedata.data = WebAPI.Common.ImageManager.ToByteArray(imgstr, 60);
                                        }
                                        var imgfile = imageData.Insert(filedata);
                                        design.sizegroup.colouritems.Add(new Mfgdesign.Sizegroup.ColourItem()
                                        {
                                            colourid = img.colourid,
                                            imageid = imgfile.id.ToBase62()
                                        });
                                    }
                                    catch (Exception ex)
                                    {
                                        Console.WriteLine($"Error: {ex.Message}");
                                        exceptions.Add(img.code + " " + img.name + "/" + img.colour + " Failed to load image from " + file);
                                    }
                                }
                            }
                            else
                            {
                                try
                                {
                                    if (System.IO.File.Exists(file))
                                    {

                                        byte[] imageBytes = System.IO.File.ReadAllBytes(file);
                                        var filedata = new ImageData.File()
                                        {
                                            isactive = true,
                                            createdby = Context.UserID,
                                            createdon = DateTime.Now,
                                            type = "skuimage",
                                            filename = file,
                                            ext = file.EndsWith("jpg") ? "JPG" : "PNG"
                                        };

                                        using (System.IO.MemoryStream ms = new System.IO.MemoryStream(imageBytes))
                                        {
                                            System.Drawing.Image imgstr = System.Drawing.Image.FromStream(ms);
                                            var thumbimg = WebAPI.Common.ImageManager.ScaleBySize(imgstr, 64);
                                            filedata.thumbnail = WebAPI.Common.ImageManager.ToByteArray(thumbimg, 90);
                                            filedata.data = WebAPI.Common.ImageManager.ToByteArray(imgstr, 60);
                                        }
                                        var imgfile = imageData.Insert(filedata);
                                        design.sizegroup.colouritems.Add(new Mfgdesign.Sizegroup.ColourItem()
                                        {
                                            colourid = img.colourid,
                                            imageid = imgfile.id.ToBase62()
                                        });
                                    }
                                }
                                catch (Exception exp)
                                {
                                    Console.WriteLine($"Error: {exp.Message}");
                                    exceptions.Add(img.code + " " + img.name + "/" + img.colour + " Failed to load image from " + file);
                                }
                            }
                        }
                        if (design.imagelist == null &&
                            design.sizegroup.colouritems != null &&
                            design.sizegroup.colouritems.Count > 0)
                        {
                            design.imagelist = new List<string>() { design.sizegroup.colouritems[0].imageid };
                        }
                    }
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        currentdesigns.ForEach(x =>
                        {
                            NpgsqlCommand command = new NpgsqlCommand("UPDATE public.mfgdesign SET sizegroup=@sizegroup,imagelist=@imagelist,modifiedon=@modifiedon,modifiedby=@modifiedby where id=@id", conn, transaction);
                            command.Parameters.Add("imagelist", NpgsqlTypes.NpgsqlDbType.Json).Value = x.imagelist_json;
                            command.Parameters.Add("sizegroup", NpgsqlTypes.NpgsqlDbType.Json).Value = x.sizegroup_json == null ? "null" : x.sizegroup_json;
                            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTime.Now;
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = x.id;
                            int successCount = command.ExecuteNonQuery();

                        });
                        transaction.Commit();
                        conn.Close();
                    }
                }
                else
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        var updatinglist = items.FindAll(x => x.id > 0);
                        var newlist = items.FindAll(x => x.id == 0);
                        var duplicates = newlist.GroupBy(x => x.code).Where(x => x.Count() > 1);
                        foreach (var newmfgsku in newlist)
                        {
                            Insert(transaction, conn, newmfgsku);
                            if (!ismigration && Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_mfgdesign)
                            {
                                Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID,
                                    Services.IntrayManager.EntryType.MfgDesign,
                                newmfgsku.id, string.Format("New Design {0} registered.", newmfgsku.name),
                                Context.UserID, 0, transaction, this.Context);
                            }
                        }

                        if (currentdesigns != null &&
                            currentdesigns.Count > 0)
                        {
                            foreach (var newmfgsku in updatinglist)
                            {
                                var mfgdesign = currentdesigns.Find(x => x.id == newmfgsku.id);
                                if (mfgdesign.sizegroup == null)
                                    mfgdesign.sizegroup = new Mfgdesign.Sizegroup() { items = new List<Mfgdesign.Sizegroup.Item>() };
                                if (!mfgdesign.sizegroup.items.Exists(x => newmfgsku.sizegroup.items.Exists(y => y.uid == x.uid)))
                                {
                                    mfgdesign.sizegroup.items.AddRange(newmfgsku.sizegroup.items);
                                    var newuanitems = mfgdesign.sizegroup.items.FindAll(x => x.itemid == 0);
                                    if (newuanitems.Count > 0)
                                    {
                                        ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
                                        ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
                                        var productname = productsDataService.SearchList(new ProductsCreteria()
                                        {
                                            id = mfgdesign.productid
                                        }).First().name;

                                        foreach (var item in newuanitems)
                                        {
                                            var itemmaster = new ItemsCreteria()
                                            {
                                                productid = mfgdesign.productid,
                                                brandid = mfgdesign.brandid,
                                                typeid = mfgdesign.typeid,
                                                styleid = mfgdesign.styleid,
                                                patternid = mfgdesign.patternid,
                                                fitid = mfgdesign.fitid,
                                                seasonid = mfgdesign.seasonid,
                                                materialid = mfgdesign.materialid,
                                                sleeveid = item.sleeveid,
                                                colourid = item.colourid,
                                                sizeid = item.sizeid,
                                                gln = item.uid,
                                                glndesignid = mfgdesign.code,
                                                support_gln = true,
                                                productmap = true,
                                                mrp = item.mrp,
                                            };
                                            var searchitemlist = itemsDataService.Search(itemmaster);
                                            if (searchitemlist.Count == 0)
                                            {
                                                if (string.IsNullOrEmpty(item.name))
                                                {
                                                    itemmaster.productname = productname;
                                                    itemmaster.printingname = productname;
                                                    itemmaster.sellingname = productname;
                                                }
                                                else
                                                {
                                                    itemmaster.productname = item.name;
                                                    itemmaster.printingname = item.name;
                                                    itemmaster.sellingname = item.name;
                                                }
                                                item.itemid = itemsDataService.Insert(transaction, conn, itemmaster).id;
                                            }
                                            else
                                            {
                                                item.itemid = searchitemlist.First().id;
                                            }
                                        }
                                    }
                                    if (mfgdesign.sizegroup != null && mfgdesign.sizegroup.items != null && mfgdesign.sizegroup.items.Count > 0)
                                    {
                                        mfgdesign.sizegroup.items.ForEach(x => x.barcodes = null);
                                    }

                                    var command = GetCommand("UPDATE public.mfgdesign SET sizegroup=@sizegroup where id=@id", conn, transaction);
                                    command.Parameters.Add("sizegroup", NpgsqlTypes.NpgsqlDbType.Json).Value = mfgdesign.sizegroup_json == null ? "null" : mfgdesign.sizegroup_json;
                                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = mfgdesign.id;
                                    command.ExecuteNonQuery();
                                }
                                else
                                {
                                    transaction.Rollback();
                                    conn.Close();
                                    throw new Exception(newmfgsku.code + " Design UAN, already contains same UAN,please retry");
                                }
                            }
                        }
                        transaction.Commit();
                        conn.Close();
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
            }
            return items;
        }

        internal int ChangePrice(ChangeAttributes item)
        {

            if (item.mode == "Cost")
            {
                string query = string.Format(@"

with alluid
as
(
select mfgdesign.code,schemelist->>'uid' uid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
cast(schemelist->>'cost' as numeric) dcost
from mfgdesign
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where mfgdesign.isactive = true and mfgdesign.id in ({0})
)

update stock set buyingprice=dcost,ibuyingprice=dcost from alluid
where alluid.uid = stock.{1}", string.Join(",", item.ids),
(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan) ? "barcode" : "serialno");

                return ExecuteQuery(query, null);
            }
            else if (item.mode == "MRP")
            {
                string query = string.Format(@"
                with alluid
                as
                (
                select mfgdesign.code,schemelist->>'uid' uid,
                cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
                cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
                cast(schemelist->>'cost' as numeric) dcost
                from mfgdesign
                LEFT JOIN LATERAL
                json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                where mfgdesign.isactive = true and mfgdesign.id in ({0})
                )

                update stock set displaymrp=alluid.mrp from alluid
                where alluid.uid = stock.{1}", string.Join(",", item.ids),
                (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan) ? "barcode" : "serialno");
                ExecuteQuery(query, null);

                query = string.Format(@"
                with alluid
                as
                (
                select mfgdesign.code,schemelist->>'uid' uid,
                cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
                cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
                cast(schemelist->>'cost' as numeric) dcost
                from mfgdesign
                LEFT JOIN LATERAL
                json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
                where mfgdesign.isactive = true and mfgdesign.id in ({0})
                )

                update items set mrp=alluid.mrp from alluid
                where alluid.uid = items.gln", string.Join(",", item.ids),
                (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan) ? "barcode" : "serialno");
                return ExecuteQuery(query, null);
            }
            else if (item.mode == "Warehouse")
            {
                string query = string.Format(@"

with alluid
as
(
select mfgdesign.code,schemelist->>'uid' uid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
cast(schemelist->>'cost' as numeric) dcost
from mfgdesign
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where mfgdesign.isactive = true and mfgdesign.id in ({0})
)

update stock set price=alluid.salerate from alluid
where alluid.uid = stock.{1}", string.Join(",", item.ids),
(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan) ? "barcode" : "serialno");

                return ExecuteQuery(query, null);
            }
            else if (item.mode == "MrpAsWarehouse")
            {
                string query = string.Format(@"

with alluid
as
(
select mfgdesign.code,schemelist->>'uid' uid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
cast(schemelist->>'cost' as numeric) dcost
from mfgdesign
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where mfgdesign.isactive = true and mfgdesign.id in ({0})
)

update stock set price=alluid.mrp from alluid
where alluid.uid = stock.{1}", string.Join(",", item.ids),
(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan) ? "barcode" : "serialno");
                return ExecuteQuery(query, null);
            }
            else if (item.mode == "Wholesale")
            {
                string query = string.Format(@"

with alluid
as
(
select mfgdesign.code,schemelist->>'uid' uid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
cast(schemelist->>'cost' as numeric) dcost
from mfgdesign
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where mfgdesign.isactive = true and mfgdesign.id in ({0})
)

update salablegoods set price=wsrate from stock
inner join alluid on alluid.uid = stock.{1}
where  stock.id = salablegoods.stockid and salablegoods.locationid={2}", string.Join(",", item.ids),
(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan) ? "barcode" : "serialno",
item.locationid);
                return ExecuteQuery(query, null);
            }
            else if (item.mode == "Retail")
            {
                string query = string.Format(@"

with alluid
as
(
select mfgdesign.code,schemelist->>'uid' uid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
cast(schemelist->>'cost' as numeric) dcost
from mfgdesign
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where mfgdesign.isactive = true and mfgdesign.id in ({0})
)

update salablegoods set price=salerate from stock
inner join alluid on alluid.uid = stock.{1}
where  stock.id = salablegoods.stockid and salablegoods.locationid={2}", string.Join(",", item.ids),
(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan) ? "barcode" : "serialno",
item.locationid);
                var records = ExecuteQuery(query, null);
            }
            else if (item.mode == "MrpAsRetail")
            {
                string query = string.Format(@"

with alluid
as
(
select mfgdesign.code,schemelist->>'uid' uid,
cast(schemelist->>'wsrate' as numeric) wsrate,cast(schemelist->>'dealerprice' as numeric) dealerprice,
cast(schemelist->>'salerate' as numeric) salerate,cast(schemelist->>'mrp' as numeric) mrp,
cast(schemelist->>'cost' as numeric) dcost
from mfgdesign
LEFT JOIN LATERAL
json_array_elements (cast(mfgdesign.sizegroup->>'items' as json)) schemelist ON true
where mfgdesign.isactive = true and mfgdesign.id in ({0})
)

update salablegoods set price=mrp from stock
inner join alluid on alluid.uid = stock.{1}
where  stock.id = salablegoods.stockid and salablegoods.locationid={2}", string.Join(",", item.ids),
(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan) ? "barcode" : "serialno",
item.locationid);
                return ExecuteQuery(query, null);
            }
            return 0;
        }
    }

}
