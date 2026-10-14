using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class PurchaseorderitemsDataService : SAASData
    {
        const string SQL_Search = @"select purchaseorderitems.id, purchaseorderitems.productid, itemid, oqty, rqty, cqty, purchaseorderitems.isactive, purchaseorderitems.price,purchaseorderitems.designuid,
                                    purchaseorderitems.priceupto,purchaseorderitems.designid,purchaseorderitems.rowtype,purchaseorderitems.utolerance,purchaseorderitems.size,purchaseorderitems.pricerange,description,
                                    purchaseorderitems.imageid, purchaseorder.supplierid, purchaseorder.pono,purchaseorderitems.sizegroup,purchaseorderitems.sizegroupid,purchaseorderitems.colourid,
                                    purchaseorderitems.createdby, purchaseorderitems.modifiedby, purchaseorderitems.version, purchaseorderid, purchaseorderitems.createdon, purchaseorderitems.sizeid,
                                    purchaseorderitems.modifiedon,productrequestformids,products.name productname,products.name printingname,brand.name brandname,purchaseorderitems.gln,purchaseorderitems.itbuscode,
                                    style.name stylename,pattern.name patternname,size.name sizename, colour.name colourname,type.name typename,material.name materialname,tradeagreementid,
                                    designperqty,designqty,deliverymode,groupname,groupcode, sleeve.name sleevename, fit.name fitname,purchaseorderitems.mrp,purchaseorderitems.sellingprice,
                                    purchaseorderitems.discount,purchaseorderitems.discountpercentage,purchaseorderitems.taxid,purchaseorderitems.tax,purchaseorderitems.cessvalue,
                                    purchaseorderitems.cesspercentage,purchaseorderitems.taxpercentage,purchaseorderitems.cessid,purchaseorderitems.hsncode
                                    from purchaseorder inner join purchaseorderitems on purchaseorderitems.purchaseorderid = purchaseorder.id
                                    left outer join items on items.id = purchaseorderitems.itemid 
                                    left outer join products products on products.id = purchaseorderitems.productid
                                    left outer join brand brand on brand.id = items.brandid
                                    left outer join referencelist colour on colour.id = items.colourid
                                    left outer join referencelist size on size.id = items.sizeid
                                    left outer join referencelist style on style.id = items.styleid
                                    left outer join referencelist pattern on pattern.id = items.patternid
                                    left outer join referencelist material on material.id = items.materialid
                                    left outer join referencelist type on type.id = items.typeid 
                                    left outer join referencelist sleeve on sleeve.id = items.sleeveid
                                    left outer join referencelist fit on fit.id = items.fitid 
                                    where purchaseorderitems.id=@id";

        const string SQL_ItemDetails = @"select purchaseorderitems.id,products.name productname,
case when brand.id is null then items.brandname else brand.name end brandname,
case when size.id is null then items.sizename else size.name end sizename,
case when colour.id is null then items.colourname else colour.name end colourname,
case when pattern.id is null then items.patternname else pattern.name end patternname,
case when material.id is null then items.materialname else material.name end materialname,
case when style.id is null then items.stylename else style.name end stylename,
case when type.id is null then items.typename else type.name end typename,
sleeve.name sleevename, products.hsncode, tax.name taxname,
fit.name fitname
from purchaseorderitems 
left outer join items on items.id = purchaseorderitems.itemid 
left outer join products on products.id = items.productid 
left outer join brand on brand.id = items.brandid
left outer join tax on tax.id = purchaseorderitems.taxid
left outer join referencelist pattern on pattern.id = items.patternid
left outer join referencelist colour on colour.id = items.colourid
left outer join referencelist style on style.id = items.styleid
left outer join referencelist size on size.id = items.sizeid
left outer join referencelist material on material.id = items.materialid
left outer join referencelist type on type.id = items.typeid
left outer join referencelist sleeve on sleeve.id = items.sleeveid
left outer join referencelist fit on fit.id = items.fitid
where purchaseorderitems.purchaseorderid = @purchaseorderid";

        const string SQL_SearchAll = @"select id, productid, itemid, oqty, rqty, cqty, isactive, createdby, modifiedby, version, purchaseorderid, createdon,imageid,designperqty,designqty,deliverymode,groupname,groupcode, 
modifiedon,productrequestformids,price,priceupto,designid,rowtype,utolerance,size,pricerange,sizegroup,sizegroupid,designuid,sizeid,colourid,description,mrp,sellingprice,purchaseorderitems.hsncode,
purchaseorderitems.discount,purchaseorderitems.discountpercentage,gln,purchaseorderitems.itbuscode,tradeagreementid,tax,taxid,cessvalue,cessid,cesspercentage,taxpercentage 
from purchaseorderitems where purchaseorderid=@purchaseorderid and isactive=true";

        const string SQL_Insert = @"INSERT INTO public.purchaseorderitems(productid, itemid, oqty, rqty, cqty, isactive, createdby, modifiedby, version,
purchaseorderid, createdon, modifiedon,productrequestformids,price,designid,priceupto,rowtype,utolerance,size,pricerange,sizeid,description,imageid,designuid,
colourid,sizegroupid,sizegroup,designperqty,designqty,deliverymode,groupname,groupcode,mrp,sellingprice,gln,itbuscode,tradeagreementid,discount,
discountpercentage,tax,taxid,cessvalue,cesspercentage,taxpercentage,cessid,hsncode)
VALUES(@productid, @itemid, @oqty, @rqty, @cqty, @isactive, @createdby, @modifiedby, @version, @purchaseorderid, @createdon,
@modifiedon,@productrequestformids,@price,@designid,@priceupto,@rowtype,@utolerance,@size,@pricerange,@sizeid,@description,@imageid,@designuid,
@colourid,@sizegroupid,@sizegroup,@designperqty,@designqty,@deliverymode,@groupname,@groupcode,@mrp,@sellingprice,@gln,@itbuscode,
@tradeagreementid,@discount,@discountpercentage,@tax,@taxid,@cessvalue,@cesspercentage,@taxpercentage,@cessid,@hsncode) RETURNING id";

        const string SQL_Update = @"UPDATE public.purchaseorderitems SET productid=@productid, itemid=@itemid, oqty=@oqty, rqty=@rqty, cqty=@cqty,mrp=@mrp,
sellingprice=@sellingprice,gln=@gln,itbuscode=@itbuscode,tradeagreementid=@tradeagreementid,discount=@discount,discountpercentage=@discountpercentage,tax=@tax,taxid=@taxid,
price=@price,designid=@designid, modifiedby=@modifiedby, version=@version+1, purchaseorderid=@purchaseorderid, modifiedon=@modifiedon,imageid=@imageid,groupcode=@groupcode,
designuid=designuid,colourid=colourid,sizegroupid=@sizegroupid,sizegroup=@sizegroup,productrequestformids=@productrequestformids,priceupto=@priceupto,groupname=@groupname,
rowtype=@rowtype,utolerance=@utolerance,size=@size,pricerange=@pricerange,sizeid=@sizeid,description=@description,designperqty=@designperqty,designqty=@designqty,deliverymode=@deliverymode,
cessvalue = @cessvalue,cesspercentage=@cesspercentage,taxpercentage=@taxpercentage,cessid=@cessid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.purchaseorderitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.purchaseorderitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where purchaseorderid=@purchaseorderid";

        const string SQL_UpdatePRF = @"update public.productrequestformitems set rqty=rqty+@rqty where id=@id";

        const string SQL_SearchWithInvoice = @"with polist as
(
select lrinvoice.invoiceno, lrinvoice.invoicedate, lrinvoiceitems.sno, lrinvoiceitems.id,cast(schemelist ->> 'purchaseorderitemsid' as bigint) purchaseorderitemsid,
cast(schemelist ->> 'aqty' as numeric) aqty from lrinvoice inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id
LEFT OUTER JOIN LATERAL json_array_elements (case when lrinvoiceitems.poinfo is null 
or lrinvoiceitems.poinfo::text ='null' then '[]'::json else lrinvoiceitems.poinfo::json end) schemelist ON true
where  cast(schemelist ->> 'purchaseorderitemsid' as bigint) is not null
)

select polist.invoiceno,polist.invoicedate,polist.sno,polist.aqty,purchaseorderitems.id, purchaseorderitems.productid, itemid, oqty, rqty, cqty, purchaseorderitems.isactive, 
purchaseorderitems.price,purchaseorderitems.designuid,purchaseorderitems.designperqty,purchaseorderitems.designqty,purchaseorderitems.deliverymode,
purchaseorderitems.priceupto,purchaseorderitems.designid,purchaseorderitems.rowtype,purchaseorderitems.utolerance,purchaseorderitems.size,purchaseorderitems.pricerange,description,
purchaseorderitems.imageid, purchaseorder.supplierid, purchaseorder.pono,purchaseorderitems.sizegroup,purchaseorderitems.sizegroupid,purchaseorderitems.colourid,
purchaseorderitems.createdby, purchaseorderitems.modifiedby, purchaseorderitems.version, purchaseorderid, purchaseorderitems.createdon, purchaseorderitems.sizeid,
purchaseorderitems.modifiedon,productrequestformids, products.name productname,products.name printingname,brand.name brandname,groupname,groupcode,
style.name stylename,pattern.name patternname,size.name sizename, colour.name colourname,type.name typename,sleeve.name sleevename,fit.name fitname,
material.name materialname,purchaseorderitems.discount,purchaseorderitems.discountpercentage,purchaseorderitems.mrp,purchaseorderitems.tax,
purchaseorderitems.taxid,purchaseorderitems.sellingprice,purchaseorderitems.gln,purchaseorderitems.itbuscode,purchaseorderitems.tradeagreementid,
purchaseorderitems.cessvalue,purchaseorderitems.cesspercentage,purchaseorderitems.taxpercentage,purchaseorderitems.cessid
from purchaseorder 
inner join purchaseorderitems on purchaseorderitems.purchaseorderid = purchaseorder.id
left outer join items on items.id = purchaseorderitems.itemid 
left outer join polist on polist.purchaseorderitemsid = purchaseorderitems.id
left outer join products products on products.id = purchaseorderitems.productid
left outer join brand brand on brand.id = items.brandid
left outer join referencelist colour on colour.id = items.colourid
left outer join referencelist size on size.id = items.sizeid
left outer join referencelist style on style.id = items.styleid
left outer join referencelist pattern on pattern.id = items.patternid
left outer join referencelist material on material.id = items.materialid
left outer join referencelist sleeve on sleeve.id = items.sleeveid
left outer join referencelist fit on fit.id = items.fitid
left outer join referencelist type on type.id = items.typeid ";

        public List<PurchaseorderitemsDetail> UpdateItemDetails(List<PurchaseorderitemsDetail> lst)
        {
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = GetCommand(SQL_ItemDetails, conn);
                command.Parameters.Add("purchaseorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = lst.First().purchaseorderid;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        if (!lst.Exists(e => e.id == id)) continue;

                        PurchaseorderitemsDetail opurchaseorderitems = lst.Find(e => e.id == id);
                        opurchaseorderitems.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        opurchaseorderitems.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        opurchaseorderitems.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        opurchaseorderitems.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        opurchaseorderitems.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        if (string.IsNullOrEmpty(opurchaseorderitems.sizename))
                            opurchaseorderitems.sizename = opurchaseorderitems.size;
                        opurchaseorderitems.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        opurchaseorderitems.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        opurchaseorderitems.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        opurchaseorderitems.sleevename = reader["sleevename"] == DBNull.Value ? "" : reader["sleevename"].ToString();
                        opurchaseorderitems.fitname = reader["fitname"] == DBNull.Value ? "" : reader["fitname"].ToString();
                        if(string.IsNullOrEmpty(opurchaseorderitems.hsncode))
                            opurchaseorderitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        opurchaseorderitems.taxname = reader["taxname"] == DBNull.Value ? "" : reader["taxname"].ToString();
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

        public List<PurchaseorderitemsDetail> Search(PurchaseorderitemsCreteria _purchaseorderitems)
        {
            List<PurchaseorderitemsDetail> lst = new List<PurchaseorderitemsDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                int i = 1;
                NpgsqlCommand command = null;
                if (_purchaseorderitems.id != 0)
                {
                    if (_purchaseorderitems.withinvoice)
                        command = GetCommand(string.Concat(SQL_SearchWithInvoice, " where purchaseorderitems.id=@id"), conn);
                    else
                        command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _purchaseorderitems.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    if (_purchaseorderitems.withinvoice)
                        qb.CurrentQuery = SQL_SearchWithInvoice;
                    else
                        qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                    qb.AddParameter("purchaseorderitems.isactive", true, "=");
                    qb.AddParameter("purchaseorder.isactive", true, "=");

                    if (_purchaseorderitems.purchaseorderid > 0)
                        qb.AddParameter("purchaseorderid", _purchaseorderitems.purchaseorderid, "=");

                    if (!string.IsNullOrEmpty(_purchaseorderitems.pono))
                        qb.AddParameter("purchaseorder.pono", _purchaseorderitems.pono, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PurchaseorderitemsDetail opurchaseorderitems = new PurchaseorderitemsDetail();
                        opurchaseorderitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opurchaseorderitems.pono = reader["pono"] == DBNull.Value ? "" : reader["pono"].ToString();
                        opurchaseorderitems.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        opurchaseorderitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        opurchaseorderitems.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);

                        opurchaseorderitems.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        opurchaseorderitems.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        opurchaseorderitems.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        opurchaseorderitems.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        opurchaseorderitems.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        opurchaseorderitems.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        opurchaseorderitems.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        opurchaseorderitems.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        opurchaseorderitems.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        opurchaseorderitems.sleevename = reader["sleevename"] == DBNull.Value ? "" : reader["sleevename"].ToString();
                        opurchaseorderitems.fitname = reader["fitname"] == DBNull.Value ? "" : reader["fitname"].ToString();
                        opurchaseorderitems.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        opurchaseorderitems.description = string.Join("/", opurchaseorderitems.description.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));

                        opurchaseorderitems.groupname = reader["groupname"] == DBNull.Value ? "" : reader["groupname"].ToString();
                        opurchaseorderitems.groupcode = reader["groupcode"] == DBNull.Value ? "" : reader["groupcode"].ToString();

                        opurchaseorderitems.deliverymode = reader["deliverymode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["deliverymode"]);
                        opurchaseorderitems.designperqty = reader["designperqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["designperqty"]);
                        opurchaseorderitems.designqty = reader["designqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["designqty"]);

                        opurchaseorderitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        opurchaseorderitems.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        opurchaseorderitems.priceupto = reader["priceupto"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["priceupto"]);
                        opurchaseorderitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();

                        opurchaseorderitems.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        opurchaseorderitems.sizegroupid = reader["sizegroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizegroupid"]);
                        opurchaseorderitems.designuid = reader["designuid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designuid"]);
                        opurchaseorderitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        opurchaseorderitems.pricerange = reader["pricerange"] == DBNull.Value ? "" : reader["pricerange"].ToString();
                        opurchaseorderitems.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        opurchaseorderitems.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        opurchaseorderitems.rowtype = reader["rowtype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rowtype"]);

                        opurchaseorderitems.utolerance = reader["utolerance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["utolerance"]); 
                        opurchaseorderitems.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        opurchaseorderitems.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"]);

                        opurchaseorderitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        opurchaseorderitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);

                        opurchaseorderitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        opurchaseorderitems.cessvalue = reader["cessvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cessvalue"]);
                        opurchaseorderitems.cesspercentage = reader["cesspercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cesspercentage"]);
                        opurchaseorderitems.cessid = reader["cessid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cessid"]);

                        opurchaseorderitems.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        opurchaseorderitems.sellingprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);
                        opurchaseorderitems.gln = reader["gln"] == DBNull.Value ? "" : reader["gln"].ToString();
                        opurchaseorderitems.itbuscode = reader["itbuscode"] == DBNull.Value ? "" : reader["itbuscode"].ToString();
                        opurchaseorderitems.tradeagreementid = reader["tradeagreementid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tradeagreementid"]);
                        opurchaseorderitems.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        opurchaseorderitems.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        opurchaseorderitems.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                        opurchaseorderitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        opurchaseorderitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        opurchaseorderitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        opurchaseorderitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        opurchaseorderitems.purchaseorderid = reader["purchaseorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["purchaseorderid"]);
                        opurchaseorderitems.productrequestformids_json = reader["productrequestformids"] == DBNull.Value ? "null" : reader["productrequestformids"].ToString();
                        opurchaseorderitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        opurchaseorderitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        opurchaseorderitems.bqty = opurchaseorderitems.oqty - (opurchaseorderitems.rqty + opurchaseorderitems.cqty);

                        if (_purchaseorderitems.withinvoice)
                        {
                            opurchaseorderitems.id = i++;
                            opurchaseorderitems.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                            opurchaseorderitems.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                            opurchaseorderitems.sno = reader["sno"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sno"]);
                            opurchaseorderitems.aqty = reader["aqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["aqty"]);
                        }
                        lst.Add(opurchaseorderitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _purchaseorderitems);
            }

            if(lst.Any(e=>string.IsNullOrEmpty(e.groupcode)))
            {
                foreach(var group in lst.GroupBy(e=>new { e.productid,e.rowtype, e.description, e.designid, e.price }))
                {
                    var groupname = "";
                    var groupcode = Guid.NewGuid().ToString();
                    if (group.Key.rowtype == 2)
                    {
                        groupname = string.Format("{0}-{1}", group.OrderBy(e => e.id).First().size,
                             group.OrderBy(e => e.id).Last().size);
                    }
                    else
                    {
                        groupname = group.First().sizename;
                    }
                    group.ToList().ForEach(e =>
                    {
                        e.groupcode = groupcode;
                        e.groupname = groupname;
                    });
                }
            }
            return lst;
        }

        public List<PurchaseorderitemsDetail> SearchAll(long _purchaseorderid)
        {
            List<PurchaseorderitemsDetail> lst = new List<PurchaseorderitemsDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchAll, conn);

                command.Parameters.Add("purchaseorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _purchaseorderid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PurchaseorderitemsDetail opurchaseorderitems = new PurchaseorderitemsDetail();
                        opurchaseorderitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opurchaseorderitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        opurchaseorderitems.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        opurchaseorderitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        opurchaseorderitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        opurchaseorderitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        opurchaseorderitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        opurchaseorderitems.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                        opurchaseorderitems.sellingprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);
                        opurchaseorderitems.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        opurchaseorderitems.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"]);
                        opurchaseorderitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        opurchaseorderitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        opurchaseorderitems.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        opurchaseorderitems.purchaseorderid = reader["purchaseorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["purchaseorderid"]);
                        opurchaseorderitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        opurchaseorderitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        opurchaseorderitems.productrequestformids_json = reader["productrequestformids"] == DBNull.Value ? "null" : reader["productrequestformids"].ToString();
                        opurchaseorderitems.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        opurchaseorderitems.priceupto = reader["priceupto"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["priceupto"]);
                        opurchaseorderitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        opurchaseorderitems.pricerange = reader["pricerange"] == DBNull.Value ? "" : reader["pricerange"].ToString();
                        opurchaseorderitems.priceranges = opurchaseorderitems.pricerange;
                        opurchaseorderitems.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        opurchaseorderitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        opurchaseorderitems.sizegroupid = reader["sizegroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizegroupid"]);
                        opurchaseorderitems.designuid = reader["designuid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designuid"]);
                        opurchaseorderitems.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        opurchaseorderitems.tradeagreementid = reader["tradeagreementid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tradeagreementid"]);
                        opurchaseorderitems.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        opurchaseorderitems.gln = reader["gln"] == DBNull.Value ? "" : reader["gln"].ToString();
                        opurchaseorderitems.itbuscode = reader["itbuscode"] == DBNull.Value ? "" : reader["itbuscode"].ToString();
                        opurchaseorderitems.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        opurchaseorderitems.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        opurchaseorderitems.rowtype = reader["rowtype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rowtype"]);
                        opurchaseorderitems.description = string.Join("/", opurchaseorderitems.description.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries));
                        opurchaseorderitems.oqty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        opurchaseorderitems.utolerance = reader["utolerance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["utolerance"]);
                        opurchaseorderitems.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        opurchaseorderitems.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                        opurchaseorderitems.deliverymode = reader["deliverymode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["deliverymode"]);
                        opurchaseorderitems.designperqty = reader["designperqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["designperqty"]);
                        opurchaseorderitems.designqty = reader["designqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["designqty"]);
                        opurchaseorderitems.groupname = reader["groupname"] == DBNull.Value ? "" : reader["groupname"].ToString();
                        opurchaseorderitems.groupcode = reader["groupcode"] == DBNull.Value ? "" : reader["groupcode"].ToString();
                        opurchaseorderitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        opurchaseorderitems.cessvalue = reader["cessvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cessvalue"]);
                        opurchaseorderitems.cesspercentage = reader["cesspercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cesspercentage"]);
                        opurchaseorderitems.cessid = reader["cessid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cessid"]);
                        lst.Add(opurchaseorderitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _purchaseorderid);
            }
            return lst;
        }

        public Purchaseorderitems Insert(PurchaseorderitemsDetail oPurchaseorderitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oPurchaseorderitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPurchaseorderitems);


            }

            return oPurchaseorderitems;
        }

        public Purchaseorderitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, PurchaseorderitemsDetail oPurchaseorderitems)
        {
            if (oPurchaseorderitems.itemid == 0)
            {
                ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
                var items = itemsDataService.Search(new ItemsCreteria()
                {
                    productmap = true,
                    sizeid = oPurchaseorderitems.sizeid,
                    styleid = oPurchaseorderitems.styleid,
                    materialid = oPurchaseorderitems.materialid,
                    brandid = oPurchaseorderitems.brandid,
                    typeid = oPurchaseorderitems.typeid,
                    sleeveid = oPurchaseorderitems.sleeveid,
                    fitid = oPurchaseorderitems.fitid,
                    patternid = oPurchaseorderitems.patternid,
                    colourid = oPurchaseorderitems.colourid,
                    productid = oPurchaseorderitems.productid
                });
                if (items.Count > 0)
                    oPurchaseorderitems.itemid = items[0].id;
                else
                {
                    var item = itemsDataService.Insert(transaction, conn, new Items()
                    {
                        printingname = oPurchaseorderitems.printingname,
                        productname = oPurchaseorderitems.printingname,
                        materialname = oPurchaseorderitems.materialname,
                        stylename = oPurchaseorderitems.stylename,
                        sizename = oPurchaseorderitems.sizename,
                        typename = oPurchaseorderitems.typename,
                        sleevename = oPurchaseorderitems.sleevename,
                        fitname = oPurchaseorderitems.fitname,
                        patternname = oPurchaseorderitems.patternname,
                        colourname = oPurchaseorderitems.colourname,
                        sizeid = oPurchaseorderitems.sizeid,
                        styleid = oPurchaseorderitems.styleid,
                        materialid = oPurchaseorderitems.materialid,
                        brandid = oPurchaseorderitems.brandid,
                        brandname = oPurchaseorderitems.brandname,
                        productid = oPurchaseorderitems.productid,
                        typeid = oPurchaseorderitems.typeid,
                        sleeveid = oPurchaseorderitems.sleeveid,
                        fitid = oPurchaseorderitems.fitid,
                        patternid = oPurchaseorderitems.patternid,
                        colourid = oPurchaseorderitems.colourid,
                        isactive = true,
                        version = 1,
                        createdby = Context.UserID,
                        modifiedby = Context.UserID,
                        createdon = DateTimeNow,
                        modifiedon = DateTimeNow,
                        agmtcode = oPurchaseorderitems.itbuscode,
                        gln = oPurchaseorderitems.gln
                    });
                    oPurchaseorderitems.itemid = item.id;
                }
            }
            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.productid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.itemid;
            command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.oqty;
            command.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.rqty;
            command.Parameters.Add("cqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.cqty;

            command.Parameters.Add("tradeagreementid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.tradeagreementid;
            command.Parameters.Add("mrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.mrp;
            command.Parameters.Add("sellingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.sellingprice;

            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.discount;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.discountpercentage;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.taxid;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.tax;

            command.Parameters.Add("cessid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.cessid;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.taxpercentage;
            command.Parameters.Add("cesspercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.cesspercentage;
            command.Parameters.Add("cessvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.cessvalue;

            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.hsncode == null ? "" : oPurchaseorderitems.hsncode;
            command.Parameters.Add("itbuscode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.itbuscode == null ? "" : oPurchaseorderitems.itbuscode;
            command.Parameters.Add("gln", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.gln == null ? "" : oPurchaseorderitems.gln;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.price;
            command.Parameters.Add("utolerance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.utolerance;
            

            command.Parameters.Add("priceupto", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.priceupto;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.designid == null ? "" : oPurchaseorderitems.designid;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.description == null ? "" : oPurchaseorderitems.description;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.imageid == null ? "" : oPurchaseorderitems.imageid;

            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPurchaseorderitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorderitems.version;
            command.Parameters.Add("purchaseorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.purchaseorderid;
            command.Parameters.Add("productrequestformids", NpgsqlTypes.NpgsqlDbType.Json).Value = oPurchaseorderitems.productrequestformids == null ? "null" : oPurchaseorderitems.productrequestformids_json;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;

            command.Parameters.Add("rowtype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorderitems.rowtype;

            command.Parameters.Add("designperqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.designperqty;
            command.Parameters.Add("designqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.designqty;
            command.Parameters.Add("deliverymode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorderitems.deliverymode;

            command.Parameters.Add("pricerange", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.pricerange == null ? "" : oPurchaseorderitems.pricerange;
            command.Parameters.Add("size", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.size == null ? "" : oPurchaseorderitems.size;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.sizeid;

            command.Parameters.Add("groupname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.groupname == null ? "" : oPurchaseorderitems.groupname;
            command.Parameters.Add("groupcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.groupcode == null ? "" : oPurchaseorderitems.groupcode;

            command.Parameters.Add("sizegroup", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.sizegroup == null ? "" : oPurchaseorderitems.sizegroup;
            command.Parameters.Add("sizegroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.sizegroupid;
            command.Parameters.Add("designuid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.designuid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.colourid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPurchaseorderitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            if (oPurchaseorderitems.productrequestformids != null)
            {
                foreach (var item in oPurchaseorderitems.productrequestformids)
                {
                    command = GetCommand(SQL_UpdatePRF, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.qty;
                    command.ExecuteNonQuery();
                }
            }
            return oPurchaseorderitems;
        }

        public Purchaseorderitems Update(PurchaseorderitemsDetail oPurchaseorderitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    var items = Search(new PurchaseorderitemsCreteria { id = oPurchaseorderitems.id });
                    if (items.Count == 1)
                        Update(transaction, conn, oPurchaseorderitems, items[0]);
                    else
                        Update(transaction, conn, oPurchaseorderitems, null);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPurchaseorderitems);
            }

            return oPurchaseorderitems;
        }

        public Purchaseorderitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn,
            Purchaseorderitems oPurchaseorderitems, Purchaseorderitems oldItem)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.id;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.productid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.itemid;
            command.Parameters.Add("oqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.oqty;
            command.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.rqty;
            command.Parameters.Add("cqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.cqty;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.price;
            command.Parameters.Add("priceupto", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.priceupto;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.designid == null ? "" : oPurchaseorderitems.designid;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.description == null ? "" : oPurchaseorderitems.description;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPurchaseorderitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorderitems.version;
            command.Parameters.Add("gln", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.gln == null ? "" : oPurchaseorderitems.gln;
            command.Parameters.Add("itbuscode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.itbuscode == null ? "" : oPurchaseorderitems.itbuscode;
            command.Parameters.Add("tradeagreementid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.tradeagreementid;
            command.Parameters.Add("mrp", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.mrp;
            command.Parameters.Add("sellingprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.sellingprice;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.discount;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.discountpercentage;
            command.Parameters.Add("productrequestformids", NpgsqlTypes.NpgsqlDbType.Json).Value = oPurchaseorderitems.productrequestformids == null ? "null" : oPurchaseorderitems.productrequestformids_json;
            command.Parameters.Add("purchaseorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.purchaseorderid;
            command.Parameters.Add("utolerance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.utolerance;

            command.Parameters.Add("cessid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.cessid;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.taxid;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.tax;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.taxpercentage;
            command.Parameters.Add("cesspercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.cesspercentage;
            command.Parameters.Add("cessvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.cessvalue;

            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchaseorderitems.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("rowtype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorderitems.rowtype;
            command.Parameters.Add("pricerange", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.pricerange == null ? "" : oPurchaseorderitems.pricerange;
            command.Parameters.Add("size", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.size == null ? "" : oPurchaseorderitems.size;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.sizeid;
            command.Parameters.Add("imageid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.imageid == null ? "" : oPurchaseorderitems.imageid;
            command.Parameters.Add("sizegroup", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.sizegroup == null ? "" : oPurchaseorderitems.sizegroup;
            command.Parameters.Add("sizegroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.sizegroupid;
            command.Parameters.Add("designuid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.designuid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.colourid;
            command.Parameters.Add("designperqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.designperqty;
            command.Parameters.Add("designqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorderitems.designqty;
            command.Parameters.Add("deliverymode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorderitems.deliverymode;
            command.Parameters.Add("groupname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.groupname == null ? "" : oPurchaseorderitems.groupname;
            command.Parameters.Add("groupcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorderitems.groupcode == null ? "" : oPurchaseorderitems.groupcode;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPurchaseorderitems.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            if (oPurchaseorderitems != null && oPurchaseorderitems.productrequestformids != null)
            {
                foreach (var item in oPurchaseorderitems.productrequestformids)
                {
                    if (oldItem != null && oldItem.productrequestformids.Exists(e => e.id == item.id))
                    {
                        command = GetCommand(SQL_UpdatePRF, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                        command.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oldItem.productrequestformids.Find(e => e.id == item.id).qty * -1;
                        command.ExecuteNonQuery();
                    }
                    command = GetCommand(SQL_UpdatePRF, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.qty;
                    command.ExecuteNonQuery();
                }
            }
            return oPurchaseorderitems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<PurchaseorderitemsDetail> list, long id)
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
                    Update(transaction, conn, item, existingItems.Find(e => e.id == item.id));
                }
                else
                {
                    item.purchaseorderid = id;
                    if (item.jumpqty != null && item.jumpqty.Count > 0)
                    {
                        foreach (var jitem in item.jumpqty)
                        {
                            item.id = 0;
                            item.oqty = jitem.qty;
                            item.sizeid = jitem.id;
                            item.size = jitem.name;
                            item.sizename = jitem.name;
                            item.rowtype = 2;
                            Insert(transaction, conn, item);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(item.sizename) && string.IsNullOrEmpty(item.size))
                            item.size = item.sizename;
                        Insert(transaction, conn, item);
                    }
                }
            }
            return true;
        }
      
        public bool Delete(Purchaseorderitems oPurchaseorderitems)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oPurchaseorderitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPurchaseorderitems);



            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Purchaseorderitems oPurchaseorderitems)
        {
            var items = Search(new PurchaseorderitemsCreteria
            {
                id = oPurchaseorderitems.id
            });

            if (items.Count == 1)
            {
                if (oPurchaseorderitems.productrequestformids != null)
                {
                    foreach (var item in oPurchaseorderitems.productrequestformids)
                    {
                        var dcommand = GetCommand(SQL_UpdatePRF, conn, transaction);
                        dcommand.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                        dcommand.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.qty * -1;
                        dcommand.ExecuteNonQuery();
                    }
                }
            }

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPurchaseorderitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorderitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorderitems.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long purchaseorderid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            var items = SearchAll(purchaseorderid);

            foreach (var oPurchaseorderitems in items)
            {
                if (oPurchaseorderitems.productrequestformids != null)
                {
                    foreach (var item in oPurchaseorderitems.productrequestformids)
                    {
                        var dcommand = GetCommand(SQL_UpdatePRF, conn, transaction);
                        dcommand.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                        dcommand.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.qty * -1;
                        dcommand.ExecuteNonQuery();
                    }
                }
            }

            int successCount = 0;
            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("purchaseorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = purchaseorderid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
