using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Windows.Input;


namespace InfyPOS.Data
{
    public class PurchaseorderDataService : SAASData
    {
        const string SQL_Search = @"select purchaseorder.id, podate, expectedate,expirydate, pono, purchaseorder.potype, agreementpoid, 
purchaseorder.supplierid, termsandcondition,purchaseorder.isinterstate, purchaseorder.isactive, 
purchaseorder.createdby, purchaseorder.modifiedby,  purchaseorder.version, tolerance, suppliercompany.id suppliercompanyid,
purchaseorder.createdon, purchaseorder.modifiedon, purchaseorder.locationid, purchaseorder.status, purchaseorder.additionalcharges,
purchaseorder.deliverylocationid, requestlocationid, purchaseorder.mode, purchaseorder.agentid, purchaseorder.freight,
actionon,actionby,purchaseorder.companyid,sizegroup,transportid,purchaseorder.remarks,purchaseorder.pouniquenumber,shopcontactemployeeid,shopcontactnumber,discountallowed,
referencelist.name suppliercity, potypereflist.name potypename, supplier.name suppliername,purchaseorder.inclusivetax,
employee.name employeeincharge
from purchaseorder 
left outer join supplier on supplier.id=purchaseorder.supplierid
left outer join suppliercompany on suppliercompany.supplierid = supplier.id
left outer join referencelist on referencelist.id = suppliercompany.cityid
left outer join referencelist potypereflist on potypereflist.id = purchaseorder.potype
left outer join employee on employee.id = shopcontactemployeeid
where purchaseorder.id=@id";

        const string SQL_Insert = @"INSERT INTO public.purchaseorder(podate, expectedate,expirydate, pono, potype, supplierid, termsandcondition,isinterstate, isactive, createdby, modifiedby, version, createdon, modifiedon, 
        locationid, status, deliverylocationid,mode,tolerance,agentid,freight,additionalcharges,companyid,sizegroup,transportid,remarks,pouniquenumber,shopcontactemployeeid,shopcontactnumber,discountallowed,requestlocationid,agreementpoid,inclusivetax) 
        VALUES(@podate, @expectedate,@expirydate, @pono, @potype, @supplierid, @termsandcondition,@isinterstate, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @locationid, @status, @deliverylocationid,
        @mode,@tolerance,@agentid,@freight,@additionalcharges,@companyid,@sizegroup,@transportid,@remarks,@pouniquenumber,@shopcontactemployeeid,@shopcontactnumber,@discountallowed,@requestlocationid,@agreementpoid,@inclusivetax) RETURNING id";

        const string SQL_SearchMobilePOSummary = @"select purchaseorderitems.id, purchaseorderitems.purchaseorderid poid,purchaseorderitems.sizegroup,
purchaseorderitems.productid,brandid,products.name product,brandname brand,coalesce(size.name,sizename) size,designid,supplier.name supplier,supplierid,
podate,pono,designid,oqty,price,purchaseorderitems.imageid,transportid,transport.name transportname,
purchaseorder.deliverylocationid,location.name deliverylocation
from purchaseorderitems
inner join purchaseorder on purchaseorder.id = purchaseorderitems.purchaseorderid
inner join supplier on supplier.id = supplierid
left outer join products on products.id = productid
left outer join items on items.id = itemid
left outer join transport on transport.id = purchaseorder.transportid
left outer join referencelist size on size.id = purchaseorderitems.sizeid
left outer join referencelist location on location.id = purchaseorder.deliverylocationid";


        const string SQL_SearchPOSummary = @"with itemsummary as
(
	select purchaseorderid, sum(oqty) oqty, sum(rqty) rqty, sum(cqty) cqty from purchaseorderitems 
	where isactive = true group by purchaseorderid
)

select purchaseorder.id, podate, expectedate,expirydate, purchaseorder.pono, potype, purchaseorder.supplierid, termsandcondition,
purchaseorder.isinterstate, purchaseorder.isactive, purchaseorder.createdby, purchaseorder.modifiedby, purchaseorder.version,tolerance, 
purchaseorder.createdon, purchaseorder.modifiedon, purchaseorder.locationid,status, itemsummary.oqty, itemsummary.rqty, itemsummary.cqty,
purchaseorder.remarks,purchaseorder.pouniquenumber, purchaseorder.deliverylocationid,requestlocationid,mode,actionby,actionon,
aemployee.name actionemployee,purchaseorder.agentid,freight,employee.name as employeename,purchaseorder.expirydate,purchaseorder.internalstatus,
shopcontactemployeeid,shopcontactnumber,shopemployee.name contactemployee,agreementpoid,additionalcharges,
supplier.name suppliername,referencelist.name suppliercity,purchaseorder.companyid,lrinvoice.invoiceno,lrinvoice.invoicedate 
from purchaseorder 
inner join itemsummary on itemsummary.purchaseorderid = purchaseorder.id
inner join users on users.id=purchaseorder.createdby 
left outer join lrinvoice on lrinvoice.poid = purchaseorder.id and lrinvoice.isactive=true
left outer join users auser on auser.id = actionby
left outer join employee aemployee on aemployee.id = auser.employeeid
left outer join supplier on supplier.id = purchaseorder.supplierid
left outer join suppliercompany on suppliercompany.supplierid = supplier.id and suppliercompany.isactive=true
left outer join referencelist on referencelist.id = suppliercompany.cityid
left outer join employee on employee.id=users.employeeid
left outer join employee shopemployee on shopemployee.id=shopcontactemployeeid";

        internal List<PurchaseorderMobile> UpdateMobilePO(MobilePurchaseOrder item)
        {
            var po = Search(new PurchaseorderCreteria()
            {
                id = item.id
            }).First();

            po.deliverylocationid = item.locationid;
            po.transportid = item.transportid;
            if (item.items != null &&
               item.items.Count > 0)
            {
                foreach (var itm in item.items)
                {
                    foreach (var poitm in itm.items)
                    {
                        if (po.Purchaseorderitems.Exists(x => x.id == poitm.id))
                        {
                            po.Purchaseorderitems.Find(x => x.id == poitm.id).oqty = poitm.qty;
                        }
                    }
                }
            }
            if (item.clone)
            {
                po.id = 0;
                po.createdby = Context.UserID;
                po.modifiedby = Context.UserID;
                po.createdon = DateTime.Now;
                po.modifiedon = DateTime.Now;
                po.version = 1;
                po.isactive = true;
                Insert(po);
            }
            else
            {
                po.modifiedby = Context.UserID;
                po.modifiedon = DateTime.Now;
                Update(po);
            }
            List<PurchaseorderMobile> rst = new List<PurchaseorderMobile>();
            foreach (var pitem in SearchMobilePO(0, 0, 1).GroupBy(e => e.poid))
            {
                rst.Add(new PurchaseorderMobile(pitem.ToList()));
            }
            return rst;
        }

        internal Purchaseorderitems ChangeMobilePO(PurchaseOrderItemSimple item)
        {
            PurchaseorderitemsDataService purchaseorderitemsDataService = new PurchaseorderitemsDataService() { Context = this.Context };
            var orderitem = purchaseorderitemsDataService.Search(new PurchaseorderitemsCreteria()
            {
                id = item.id == 0 ? item.cloneid : item.id
            });
            orderitem.First().oqty = item.qty;
            if (item.id == 0)
            {
                orderitem.First().id = 0;
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var list = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "SIZE",
                    name = item.size
                });
                if (list.Count > 0)
                {
                    orderitem.First().sizeid = list.First().id;
                    orderitem.First().size = list.First().name;
                    orderitem.First().sizename = list.First().name;
                }
                return purchaseorderitemsDataService.Insert(orderitem.First());
            }
            return purchaseorderitemsDataService.Update(orderitem.First());
        }

        internal string DeleteMobilePO(long purchaseorderitemid)
        {
            PurchaseorderitemsDataService purchaseorderitemsDataService = new PurchaseorderitemsDataService() { Context = this.Context };
            var item = purchaseorderitemsDataService.Search(new PurchaseorderitemsCreteria()
            {
                id = purchaseorderitemid
            }).First();
            return purchaseorderitemsDataService.Delete(item).ToString();
        }

        internal Purchaseorder AddMobilePO(PurchaseOrderItemSimple item)
        {
            long currentpoid = 0;
            long currenttransportid = 0;
            var param = new SortedList<string, object>();
            param.Add("supplierid", item.supplierid);
            param.Add("fromdate", DateTimeNow.Date);
            param.Add("todate", DateTimeNow.Date.AddDays(1));
            param.Add("createdby", Context.UserID);

            string query = "select id from purchaseorder where ismobilepo=true and sizegroup=@sizegroup and podate>=@fromdate and podate<@todate and supplierid=@supplierid and createdby=@createdby order by id desc limit 1";
            if (string.IsNullOrEmpty(item.sizegroup))
                query = "select id from purchaseorder where ismobilepo=true and podate>=@fromdate and podate<@todate and supplierid=@supplierid and createdby=@createdby order by id desc limit 1";
            else
                param.Add("sizegroup", item.sizegroup);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    currentpoid = reader.GetInt64(0);
                }
            }

            if (currentpoid == 0)
            {
                using (var reader = ExecuteReader("select preferredtransportid from suppliercompany where supplierid=@supplierid", param))
                {
                    if (reader.Read())
                    {
                        currenttransportid = reader.IsDBNull(0) ? 0 : reader.GetInt64(0);
                    }
                }
            }

            ProductsDataService productsDataService = new ProductsDataService() { Context = this.Context };
            item.product = productsDataService.Search(new ProductsCreteria()
            {
                id = item.productid
            }).First().name;
            if (item.brandid > 0)
            {
                BrandDataService brandData = new BrandDataService() { Context = this.Context };
                item.brand = brandData.Search(new BrandCreteria()
                {
                    id = item.brandid
                }).First().name;
            }


            if (item.sizelist != null && item.sizelist != null && item.sizelist.Count > 0)
            {
                item.sizelist.RemoveAll(e => e.qty == 0);
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var list = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    type = "SIZE"
                });
                foreach (var itm in item.sizelist)
                {
                    if (list.Exists(e => e.name.ToLower() == itm.name.ToLower()))
                        itm.id = list.Find(e => e.name.ToLower() == itm.name.ToLower()).id;
                    else
                    {
                        itm.id = referencelistDataService.Insert(new Referencelist()
                        {
                            type = "SIZE",
                            name = itm.name,
                            isactive = true,
                            createdby = Context.UserID,
                            modifiedby = Context.UserID,
                            createdon = DateTimeNow,
                            modifiedon = DateTimeNow
                        }).id;
                    }
                }
            }
            if (currentpoid == 0)
            {


                var po = new Purchaseorder();
                po.podate = DateTimeNow.Date;
                po.supplierid = item.supplierid;
                po.companyid = item.companyid;
                po.createdby = Context.UserID;
                po.modifiedby = Context.UserID;
                po.transportid = currenttransportid;
                po.createdon = DateTimeNow;
                po.modifiedon = DateTimeNow;
                po.version = 1;
                po.sizegroup = item.sizegroup;
                po.isactive = true;
                po.shopcontactemployeeid = Context.UserID;
                po.mode = !string.IsNullOrEmpty(item.sizegroup) && item.sizegroup.IndexOf('-') > 0 ? 0 : 2;
                po.Purchaseorderitems = new List<PurchaseorderitemsDetail>();
                if (item.sizelist != null && item.sizelist.Count > 0)
                {
                    item.sizelist.ForEach(e =>
                    {
                        po.Purchaseorderitems.Add(new PurchaseorderitemsDetail()
                        {
                            productid = item.productid,
                            brandid = item.brandid,
                            brandname = item.brand,
                            designid = item.designid,
                            oqty = e.qty,
                            price = item.price,
                            printingname = item.product,
                            description = item.brandid > 0 ? (item.product + "/" + item.brand) : (item.product),
                            size = e.name,
                            sizename = e.name,
                            sizeid = e.id,
                            imageid = item.imageid,
                            isactive = true,
                            version = 1,
                            createdby = Context.UserID,
                            modifiedby = Context.UserID,
                            createdon = DateTimeNow,
                            modifiedon = DateTimeNow
                        });
                    });
                }
                else
                {
                    po.Purchaseorderitems.Add(new PurchaseorderitemsDetail()
                    {
                        productid = item.productid,
                        brandid = item.brandid,
                        brandname = item.brand,
                        designid = item.designid,
                        oqty = item.qty,
                        price = item.price,
                        printingname = item.product,
                        description = item.brandid > 0 ? (item.product + "/" + item.brand) : (item.product),
                        sizeid = item.sizeid,
                        size = item.size,
                        sizename = item.size,
                        imageid = item.imageid,
                        isactive = true,
                        version = 1,
                        createdby = Context.UserID,
                        modifiedby = Context.UserID,
                        createdon = DateTimeNow,
                        modifiedon = DateTimeNow
                    });
                }
                Insert(po);
                ExecuteQuery("update purchaseorder set ismobilepo=true where id=" + po.id.ToString(), null);
                return po;
            }
            else
            {
                var po = Search(new PurchaseorderCreteria()
                {
                    id = currentpoid
                }).First();

                if (item.sizeid > 0)
                {
                    po.Purchaseorderitems.RemoveAll(e => e.productid == item.productid &&
                    e.brandid == item.brandid && e.sizeid == item.sizeid && e.designid == item.designid);
                }
                else
                {
                    po.Purchaseorderitems.RemoveAll(e => e.productid == item.productid &&
                    e.brandid == item.brandid && e.price == item.price && e.designid == item.designid);
                }
                if (item.sizelist != null && item.sizelist.Count > 0)
                {
                    item.sizelist.ForEach(e =>
                    {
                        po.Purchaseorderitems.Add(new PurchaseorderitemsDetail()
                        {
                            productid = item.productid,
                            brandid = item.brandid,
                            brandname = item.brand,
                            designid = item.designid,
                            oqty = e.qty,
                            price = item.price,
                            printingname = item.product,
                            description = item.brandid > 0 ? (item.product + "/" + item.brand) : (item.product),
                            size = e.name,
                            sizename = e.name,
                            sizeid = e.id,
                            imageid = item.imageid,
                            isactive = true,
                            version = 1,
                            createdby = Context.UserID,
                            modifiedby = Context.UserID,
                            createdon = DateTimeNow,
                            modifiedon = DateTimeNow
                        });
                    });
                }
                else
                {
                    po.Purchaseorderitems.Add(new PurchaseorderitemsDetail()
                    {
                        productid = item.productid,
                        brandid = item.brandid,
                        brandname = item.brand,
                        designid = item.designid,
                        oqty = item.qty,
                        price = item.price,
                        printingname = item.product,
                        description = item.brandid > 0 ? (item.product + "/" + item.brand) : (item.product),
                        sizeid = item.sizeid,
                        size = item.size,
                        sizename = item.size,
                        imageid = item.imageid,
                        isactive = true,
                        version = 1,
                        createdby = Context.UserID,
                        modifiedby = Context.UserID,
                        createdon = DateTimeNow,
                        modifiedon = DateTimeNow
                    });
                }

                Update(po);

                return po;
            }
        }

        internal List<PurchaseOrderItemSimple> SearchMobilePO(long supplierid, long poid, long limit)
        {
            List<PurchaseOrderItemSimple> lst = new List<PurchaseOrderItemSimple>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                if (limit == 1 && poid == 0)
                {
                    var orders = Search(new PurchaseorderCreteria()
                    {
                        supplierid = supplierid,
                        limit = 1
                    });
                    if (orders.Count == 1)
                    {
                        poid = orders.First().id;
                    }
                    else
                    {
                        return new List<PurchaseOrderItemSimple>();
                    }
                }
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchMobilePOSummary;

                if (poid > 0)
                {
                    qb.AddParameter("purchaseorder.id", "id", poid, "=");
                }
                else
                {
                    qb.AddParameter("purchaseorder.createdby", "createdby", Context.UserID, "=");
                    qb.AddParameter("purchaseorder.ismobilepo", "ismobilepo", true, "=");
                    if (supplierid > 0)
                        qb.AddParameter("purchaseorder.supplierid", "purchaseorder.supplierid", supplierid, "=");

                    qb.AddParameter("purchaseorder.isactive", "purchaseorder.isactive", true, "=");
                    qb.AddParameter("purchaseorderitems.isactive", "purchaseorderitems.isactive", true, "=");

                    qb.AddParameter("purchaseorder.podate", "fromdate", DateTimeNow.AddDays(-7), ">=");
                    qb.AddParameter("purchaseorder.podate", "todate", DateTimeNow.Date.AddDays(1), "<");
                    qb.SortField = "purchaseorder.modifiedon desc";
                }
                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PurchaseOrderItemSimple opurchaseorder = new PurchaseOrderItemSimple();
                        opurchaseorder.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opurchaseorder.podate = reader["podate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["podate"]);
                        opurchaseorder.pono = reader["pono"] == DBNull.Value ? "" : reader["pono"].ToString();
                        opurchaseorder.transportname = reader["transportname"] == DBNull.Value ? "" : reader["transportname"].ToString();
                        opurchaseorder.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        opurchaseorder.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"]);
                        opurchaseorder.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        opurchaseorder.poid = reader["poid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["poid"]);
                        opurchaseorder.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        opurchaseorder.qty = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        opurchaseorder.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        opurchaseorder.brand = reader["brand"] == DBNull.Value ? "" : reader["brand"].ToString();
                        opurchaseorder.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        opurchaseorder.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        opurchaseorder.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                        opurchaseorder.size = reader["size"] == DBNull.Value ? "" : reader["size"].ToString();
                        opurchaseorder.supplier = reader["supplier"] == DBNull.Value ? "" : reader["supplier"].ToString();
                        opurchaseorder.location = reader["deliverylocation"] == DBNull.Value ? "" : reader["deliverylocation"].ToString();
                        opurchaseorder.locationid = reader["deliverylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deliverylocationid"]);
                        lst.Add(opurchaseorder);
                    }
                }
                //if (lst.Count == 1)
                //{
                //    PurchaseorderitemsDataService subPurchaseorderitems = new PurchaseorderitemsDataService() { Context = this.Context };
                //    subPurchaseorderitems.Context = this.Context;
                //    lst[0].Purchaseorderitems = subPurchaseorderitems.SearchAll(_reportcreteria.id);
                //}
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "SearchMobilePO");
            }
            return lst;
        }

        const string SQL_SearchPendingPO = @"select purchaseorder.id, podate, expectedate,expirydate, pono, potype, agreementpoid, supplierid, termsandcondition, tolerance, 
purchaseorder.deliverylocationid,requestlocationid,mode,agentid,freight, purchaseorder.locationid,status,purchaseorder.isinterstate,
purchaseorderitems.productid, itemid, oqty, rqty, cqty, purchaseorderitems.price,purchaseorder.inclusivetax,
purchaseorderitems.priceupto,purchaseorderitems.designid,purchaseorderitems.rowtype,purchaseorderitems.size,
purchaseorderitems.pricerange,description,purchaseorderitems.imageid, purchaseorderitems.sizeid,
productrequestformids,products.name productname,products.name printingname,brand.name brandname,
style.name stylename,pattern.name patternname,size.name sizename, colour.name colourname,type.name typename,
sleeve.name sleevename,fit.name fitname,material.name materialname,purchaseorder.agreementpoid
from purchaseorder
inner join purchaseorderitems on purchaseorderitems.purchaseorderid = purchaseorder.id
left outer join items on items.id = purchaseorderitems.itemid
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

        const string SQL_Update = @"UPDATE public.purchaseorder SET podate=@podate, potype=@potype, expectedate=@expectedate,expirydate=@expirydate, pono=@pono, supplierid=@supplierid,isinterstate=@isinterstate, termsandcondition=@termsandcondition, modifiedby=@modifiedby,tolerance=@tolerance, shopcontactemployeeid=@shopcontactemployeeid,shopcontactnumber=@shopcontactnumber,inclusivetax=@inclusivetax,
                            version=@version+1,mode=@mode, modifiedon=@modifiedon, locationid=@locationid, status=@status, deliverylocationid=@deliverylocationid,agentid=@agentid,freight=@freight,additionalcharges=@additionalcharges,companyid=@companyid,transportid=@transportid,
                            pouniquenumber=@pouniquenumber,remarks=@remarks,discountallowed=@discountallowed,requestlocationid=@requestlocationid,agreementpoid=@agreementpoid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.purchaseorder set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_UpdateStatus = @"update public.purchaseorder set status=@status,internalstatus=@internalstatus,version=@version+1,actionby=@modifiedby,actionon=@modifiedon where id=@id and version=@version RETURNING version";

        public List<Purchaseorder> Search(PurchaseorderCreteria _purchaseorder)
        {
            if (_purchaseorder.isagreement)
            {
                ReferencelistDataService _referenceData = new ReferencelistDataService();
                _purchaseorder.potype = _referenceData.GetAgreementPOType();
                if (_purchaseorder.potype == 0) return new List<Purchaseorder>();
            }

            List<Purchaseorder> lst = new List<Purchaseorder>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_purchaseorder.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _purchaseorder.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("purchaseorder.isactive", true, "=");

                    if (!string.IsNullOrEmpty(_purchaseorder.pono))
                        qb.AddParameter("pono", _purchaseorder.pono, "=");
                    if (_purchaseorder.potype > 0)
                        qb.AddParameter("purchaseorder.potype", _purchaseorder.potype, "=");

                    if (_purchaseorder.locationid > 0)
                        qb.AddParameter("purchaseorder.locationid", _purchaseorder.locationid, "=");
                    if (_purchaseorder.companyid > 0)
                        qb.AddParameter("purchaseorder.companyid", _purchaseorder.companyid, "=");

                    if (_purchaseorder.supplierid > 0)
                        qb.AddParameter("purchaseorder.supplierid", _purchaseorder.supplierid, "=");

                    if (_purchaseorder.agentid > 0)
                        qb.AddParameter("purchaseorder.agentid", _purchaseorder.agentid, "=");

                    if (_purchaseorder.suppliercityid > 0)
                        qb.AddParameter("suppliercompany.cityid", _purchaseorder.suppliercityid, "=");

                    if (_purchaseorder.shopcontactemployeeid > 0)
                        qb.AddParameter("purchaseorder.shopcontactemployeeid",
                            "shopcontactemployeeid", _purchaseorder.shopcontactemployeeid, "=");

                    if (_purchaseorder.statusid >= 0)
                        qb.AddParameter("purchaseorder.status", _purchaseorder.statusid, "=");

                    if (_purchaseorder.podate != DateTime.MinValue)
                    {
                        qb.AddParameter("podate", "podatefrom", _purchaseorder.podate.Date, ">=");
                        qb.AddParameter("podate", "podateto", _purchaseorder.podate.Date.AddDays(1), "<");
                    }
                    if (_purchaseorder.expectedate != DateTime.MinValue)
                    {
                        qb.AddParameter("expectedate", "expectedatefrom", _purchaseorder.expectedate.Date, ">=");
                        qb.AddParameter("expectedate", "expectedateto", _purchaseorder.expectedate.Date.AddDays(1), "<");
                    }
                    if (_purchaseorder.expectedate != DateTime.MinValue)
                    {
                        qb.AddParameter("expirydate", "expirydatefrom", _purchaseorder.expirydate.Date, ">=");
                        qb.AddParameter("expirydate", "expirydateto", _purchaseorder.expirydate.Date.AddDays(1), "<");
                    }


                    qb.SortField = "purchaseorder.id desc";
                    qb.Limit = _purchaseorder.limit;
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Purchaseorder opurchaseorder = new Purchaseorder();
                        opurchaseorder.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opurchaseorder.tolerance = reader["tolerance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tolerance"]);
                        opurchaseorder.discountallowed = reader["discountallowed"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountallowed"]);
                        opurchaseorder.potype = reader["potype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["potype"]);
                        opurchaseorder.agreementpoid = reader["agreementpoid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agreementpoid"]);
                        opurchaseorder.potypename = reader["potypename"] == DBNull.Value ? "" : reader["potypename"].ToString();

                        opurchaseorder.podate = reader["podate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["podate"]);
                        opurchaseorder.expectedate = reader["expectedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedate"]);
                        opurchaseorder.expirydate = reader["expirydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expirydate"]);

                        opurchaseorder.pono = reader["pono"] == DBNull.Value ? "" : reader["pono"].ToString();
                        opurchaseorder.sizegroup = reader["sizegroup"] == DBNull.Value ? "" : reader["sizegroup"].ToString();
                        opurchaseorder.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        opurchaseorder.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        opurchaseorder.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        opurchaseorder.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);

                        opurchaseorder.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                        opurchaseorder.shopcontactnumber = reader["shopcontactnumber"] == DBNull.Value ? "" : reader["shopcontactnumber"].ToString();
                        opurchaseorder.shopcontactemployeeid = reader["shopcontactemployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["shopcontactemployeeid"]);
                        opurchaseorder.suppliercity = reader["suppliercity"] == DBNull.Value ? "" : reader["suppliercity"].ToString();

                        opurchaseorder.deliverylocationid = reader["deliverylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deliverylocationid"]);
                        opurchaseorder.requestlocationid = reader["requestlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["requestlocationid"]);
                        opurchaseorder.termsandcondition_json = reader["termsandcondition"] == DBNull.Value ? "null" : reader["termsandcondition"].ToString();

                        opurchaseorder.additionalcharges_json = reader["additionalcharges"] == DBNull.Value ? "null" : reader["additionalcharges"].ToString();

                        opurchaseorder.inclusivetax = reader["inclusivetax"] == DBNull.Value ? false : Convert.ToBoolean(reader["inclusivetax"]);

                        opurchaseorder.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        opurchaseorder.isinterstate = reader["isinterstate"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstate"]);

                        opurchaseorder.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        opurchaseorder.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        opurchaseorder.actionby = reader["actionby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["actionby"]);
                        opurchaseorder.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        opurchaseorder.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"]);
                        opurchaseorder.status = reader["status"] == DBNull.Value ? Purchaseorder.POStatus.PENDING : (Purchaseorder.POStatus)Convert.ToInt32(reader["status"]);
                        opurchaseorder.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        opurchaseorder.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        opurchaseorder.actionon = reader["actionon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["actionon"]);
                        opurchaseorder.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        opurchaseorder.freight = reader["freight"] == DBNull.Value ? 0 : Convert.ToInt32(reader["freight"]);
                        opurchaseorder.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"]);
                        opurchaseorder.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        opurchaseorder.pouniquenumber = reader["pouniquenumber"] == DBNull.Value ? "" : reader["pouniquenumber"].ToString();
                        
                        opurchaseorder.employeeincharge = reader["employeeincharge"] == DBNull.Value ? "" : reader["employeeincharge"].ToString();

                        if ((opurchaseorder.status == Purchaseorder.POStatus.APPROVED || opurchaseorder.status == Purchaseorder.POStatus.PARTIALLYCOMPLETED)
                            && opurchaseorder.expectedate > DateTime.MinValue
                            && opurchaseorder.expectedate.Date < DateTimeNow.Date)
                            opurchaseorder.overduedays = (DateTimeNow - opurchaseorder.expectedate).Days;
                        lst.Add(opurchaseorder);
                    }
                }
                if (((_purchaseorder.includeitems || _purchaseorder.id > 0) && lst.Count == 1))
                {
                    PurchaseorderitemsDataService subPurchaseorderitems = new PurchaseorderitemsDataService() { Context = this.Context };
                    subPurchaseorderitems.Context = this.Context;
                    lst[0].Purchaseorderitems = subPurchaseorderitems.SearchAll(lst.First().id);
                }

                if (lst.Count > 0 && lst.Exists(e => e.agreementpoid > 0))
                {
                    List<Purchaseorder> agreementpolist = GetAllAgreementPO();
                    foreach (var item in lst.FindAll(e => e.agreementpoid > 0))
                    {
                        if (agreementpolist.Exists(e => e.id == item.agreementpoid))
                        {
                            item.agreementpono = agreementpolist.Find(e => e.id == item.agreementpoid).pono;
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _purchaseorder);
            }
            return lst;
        }

        private List<Purchaseorder> GetAllAgreementPO()
        {
            const string SQL_Search = @"select purchaseorder.id, podate, pono
                                        from purchaseorder where purchaseorder.isactive=true and potype={0}";

            ReferencelistDataService _referenceData = new ReferencelistDataService();
            long potype = _referenceData.GetAgreementPOType();

            List<Purchaseorder> lst = new List<Purchaseorder>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                command = GetCommand(string.Format(SQL_Search, potype), conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Purchaseorder opurchaseorder = new Purchaseorder();
                        opurchaseorder.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opurchaseorder.podate = reader["podate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["podate"]);
                        opurchaseorder.pono = reader["pono"] == DBNull.Value ? "" : reader["pono"].ToString();
                        lst.Add(opurchaseorder);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return lst;

        }

        public List<PurchaseOrderHistory> SearchMyPendingPO(PurchaseorderCreteria _purchaseorder)
        {
            const string SQL_Search = @"select purchaseorder.id, purchaseorder.pono, purchaseorder.podate,
sum((oqty-(rqty+cqty))*purchaseorderitems.price) ordervalue,sum((oqty-(rqty+cqty))) orderqty
from purchaseorder 
inner join purchaseorderitems on purchaseorderitems.purchaseorderid = purchaseorder.id
where purchaseorder.isactive=true and purchaseorderitems.isactive=true
and purchaseorder.status = 1 and oqty-(rqty+cqty)>0 and potype!=@potypeid
and purchaseorder.supplierid = @supplierid
group by purchaseorder.id, purchaseorder.pono, purchaseorder.podate";

            const string SQL_TASearch = @"select purchaseorder.id, purchaseorder.pono, purchaseorder.podate,
sum((oqty-(rqty+cqty))*purchaseorderitems.price) ordervalue,purchaseorderitems.tradeagreementid,
suppliertradeagreement.itemcode,sum((oqty-(rqty+cqty))) orderqty
from purchaseorder 
inner join purchaseorderitems on purchaseorderitems.purchaseorderid = purchaseorder.id
inner join suppliertradeagreement on suppliertradeagreement.id = purchaseorderitems.tradeagreementid
where purchaseorder.isactive=true and purchaseorderitems.isactive=true
and purchaseorder.status = 1 and oqty-(rqty+cqty)>0
and purchaseorder.supplierid = @supplierid
group by purchaseorder.id, purchaseorder.pono, purchaseorder.podate,purchaseorderitems.tradeagreementid,suppliertradeagreement.itemcode";

            const string SQL_TASearchWithPO = @"with currentpoitems
as
(
select cast(schemelist->>'purchaseorderitemsid' as bigint) id,
sum(cast(schemelist->>'aqty' as numeric)) qty
from lrinvoiceitems
LEFT JOIN LATERAL
json_array_elements (poinfo :: json) schemelist ON true
where lrinvoiceid=@lrinvoiceid and isactive=true
group by cast(schemelist->>'purchaseorderitemsid' as bigint)
)

select purchaseorder.id, purchaseorder.pono, purchaseorder.podate,
sum((oqty-(rqty+cqty-(coalesce(currentpoitems.qty,0))))*purchaseorderitems.price) ordervalue,purchaseorderitems.tradeagreementid,
suppliertradeagreement.itemcode,sum((oqty-(rqty+cqty-(coalesce(currentpoitems.qty,0))))) orderqty
from purchaseorder 
inner join purchaseorderitems on purchaseorderitems.purchaseorderid = purchaseorder.id
left outer join currentpoitems on currentpoitems.id = purchaseorderitems.id
inner join suppliertradeagreement on suppliertradeagreement.id = purchaseorderitems.tradeagreementid
where purchaseorder.isactive=true and purchaseorderitems.isactive=true
and purchaseorder.status = 1 and oqty-(rqty+cqty-(coalesce(currentpoitems.qty,0)))>0
and purchaseorder.supplierid = @supplierid
group by purchaseorder.id, purchaseorder.pono, purchaseorder.podate,purchaseorderitems.tradeagreementid,suppliertradeagreement.itemcode";


            ReferencelistDataService _referenceData = new ReferencelistDataService();
            long potype = _referenceData.GetAgreementPOType();

            List<PurchaseOrderHistory> lst = new List<PurchaseOrderHistory>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                command = GetCommand(_purchaseorder.istradeagreement ? SQL_TASearch : SQL_Search, conn);

                if (_purchaseorder.istradeagreement && _purchaseorder.lrinvoiceid > 0)
                {
                    command = GetCommand(SQL_TASearchWithPO, conn);
                }

                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _purchaseorder.supplierid;
                command.Parameters.Add("potypeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = potype;
                command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _purchaseorder.lrinvoiceid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PurchaseOrderHistory opurchaseorder = new PurchaseOrderHistory();
                        opurchaseorder.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opurchaseorder.podate = reader["podate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["podate"]);
                        opurchaseorder.pono = reader["pono"] == DBNull.Value ? "" : reader["pono"].ToString();
                        opurchaseorder.amount = reader["ordervalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ordervalue"]);
                        opurchaseorder.qty = reader["orderqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["orderqty"]);
                        if (_purchaseorder.istradeagreement)
                        {
                            opurchaseorder.tradeagreementid = reader["tradeagreementid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tradeagreementid"]);
                            opurchaseorder.tradeagreementcode = reader["itemcode"] == DBNull.Value ? "" : reader["itemcode"].ToString();
                        }
                        lst.Add(opurchaseorder);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return lst;

        }

        public Purchaseorder Insert(Purchaseorder oPurchaseorder)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                SupplierDataService supplierdata = new SupplierDataService() { Context = this.Context };
                var supplier = supplierdata.Search(new SupplierCreteria() { id = oPurchaseorder.supplierid });

                if (supplier != null && supplier.Count > 0 && supplier[0].agentid > 0 && oPurchaseorder.agentid == 0)
                    oPurchaseorder.agentid = supplier[0].agentid;

                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("purchaseorder.pono",
    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportlocationwisesopackingno ? oPurchaseorder.locationid : 0,
    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ? oPurchaseorder.companyid : 0);

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    oPurchaseorder.pono = sequence.GetNextNumber(Context, conn, transaction);

                    Insert(transaction, conn, oPurchaseorder);
                    PurchaseorderitemsDataService subPurchaseorderitems = new PurchaseorderitemsDataService() { Context = this.Context };
                    subPurchaseorderitems.Context = this.Context;
                    foreach (var item in oPurchaseorder.Purchaseorderitems)
                    {
                        if (!string.IsNullOrEmpty(item.pricerange))
                            item.priceranges = item.pricerange;
                        item.groupcode = Guid.NewGuid().ToString();
                        item.purchaseorderid = oPurchaseorder.id;
                        if (item.jumpqty != null && item.jumpqty.Count > 0)
                        {
                            //item.rowtype = 1;
                            //subPurchaseorderitems.Insert(transaction, conn, item);
                            item.groupname = string.Format("{0}-{1}", item.jumpqty.First().name, item.jumpqty.Last().name);
                            foreach (var jitem in item.jumpqty)
                            {
                                item.id = 0;
                                item.itemid = 0;
                                item.oqty = jitem.qty;
                                item.sizeid = jitem.id;
                                item.size = jitem.name;
                                item.sizename = jitem.name;
                                item.rowtype = 2;
                                subPurchaseorderitems.Insert(transaction, conn, item);
                            }
                        }
                        else
                        {
                            item.groupname = item.sizename;
                            if (!string.IsNullOrEmpty(item.sizename) && string.IsNullOrEmpty(item.size))
                                item.size = item.sizename;
                            subPurchaseorderitems.Insert(transaction, conn, item);
                        }
                    }

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_notification_purchaseorder)
                    {
                        string inchargename = "";
                        if (oPurchaseorder.shopcontactemployeeid > 0)
                        {
                            EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
                            employeeDataService.Context = this.Context;
                            inchargename = " (" + employeeDataService.SearchHeader(new EmployeeCreteria()
                            {
                                id = oPurchaseorder.shopcontactemployeeid
                            }).First().name + ")";
                        }
                        if (supplier != null && supplier.Count > 0)
                        {
                            Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.PurchaseOrder,
                                oPurchaseorder.id, string.Format("PURCHASE ORDER - NO {0} FOR THE SUPPLIER {1} WITH {2} QTY {3}.", oPurchaseorder.pono, supplier[0].name,
                                oPurchaseorder.Purchaseorderitems.Sum(x => x.oqty),
                                inchargename), Context.UserID, oPurchaseorder.locationid,
                                oPurchaseorder.companyid, transaction, Context);
                        }
                    }
                    transaction.Commit();
                    conn.Close();

                    oPurchaseorder.Purchaseorderitems = subPurchaseorderitems.SearchAll(oPurchaseorder.id);
                    Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, oPurchaseorder, Processors.Schedule_Actions.Actions.New);
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPurchaseorder);
            }
            return oPurchaseorder;
        }

        public List<PurchaseOrderItemDetail> SearchPO(PurchaseorderCreteria purchaseorderCreteria)
        {
            string SQL_SearchPO = @"select purchaseorderitems.id,purchaseorder.podate,purchaseorder.id,purchaseorder.pono,oqty,
                            oqty-(rqty+cqty) balance,designid,price,purchaseorderitems.imageid,purchaseorderitems.description,referencelist.name sizename,
                            tolerance,purchaseorderitems.productid,items.sizeid,potypelist.name as potypename,purchaseorderitems.tradeagreementid from purchaseorder
                            inner join purchaseorderitems on purchaseorderitems.purchaseorderid = purchaseorder.id
                            left outer join items on items.id = itemid
							left outer join referencelist on referencelist.id = items.sizeid
                            left outer join referencelist potypelist on potypelist.id = purchaseorder.potype
                            where purchaseorder.supplierid = @supplierid and purchaseorderitems.productid = @productid 
                            and purchaseorder.isactive = true and purchaseorderitems.isactive = true and purchaseorder.status = 1 
                            and purchaseorder.expectedate>=@expectedate 
                            and purchaseorder.podate>=@fromdate and oqty-(rqty+cqty)>0";

            List<Lrinvoiceitems> items = null;
            if (purchaseorderCreteria.tradeagreementid > 0)
            {
                SQL_SearchPO = @"select purchaseorderitems.id,purchaseorder.podate,purchaseorder.id,purchaseorder.pono,oqty,
                            purchaseorderitems.gln,purchaseorderitems.itemid,items.brandid,items.styleid,items.patternid,            
                            oqty-(rqty+cqty) balance,designid,price,purchaseorderitems.imageid,purchaseorderitems.description,
                            referencelist.name sizename,items.materialid,items.fitid,items.sleeveid,items.typeid,purchaseorderitems.mrp,
                            tolerance,purchaseorderitems.productid,items.sizeid,potypelist.name as potypename,
                            purchaseorderitems.tradeagreementid,purchaseorderitems.sellingprice from purchaseorder
                            inner join purchaseorderitems on purchaseorderitems.purchaseorderid = purchaseorder.id
                            left outer join items on items.id = itemid
							left outer join referencelist on referencelist.id = items.sizeid
                            left outer join referencelist potypelist on potypelist.id = purchaseorder.potype
                            where purchaseorderitems.tradeagreementid = @tradeagreementid 
                            and purchaseorder.isactive = true and purchaseorderitems.isactive = true and purchaseorder.status = 1 
                            and oqty-(rqty+cqty)>0";

                if (purchaseorderCreteria.lrinvoiceid > 0)
                    SQL_SearchPO = @"with currentpoitems
                                            as
                                            (
                                            select cast(schemelist->>'purchaseorderitemsid' as bigint) id,
                                            sum(cast(schemelist->>'aqty' as numeric)) qty
                                            from lrinvoiceitems
                                            LEFT JOIN LATERAL
                                            json_array_elements (poinfo :: json) schemelist ON true
                                            where lrinvoiceid=@lrinvoiceid and isactive=true
                                            group by cast(schemelist->>'purchaseorderitemsid' as bigint)
                                            )

                                            select purchaseorderitems.id,purchaseorder.podate,purchaseorder.id,purchaseorder.pono,oqty,
                                            purchaseorderitems.gln,purchaseorderitems.itemid,items.brandid,items.styleid,items.patternid,            
                                            oqty-(rqty+cqty-(coalesce(currentpoitems.qty,0))) balance,designid,price,purchaseorderitems.imageid,purchaseorderitems.description,
                                            referencelist.name sizename,items.materialid,items.fitid,items.sleeveid,items.typeid,purchaseorderitems.mrp,
                                            tolerance,purchaseorderitems.productid,items.sizeid,potypelist.name as potypename,
                                            purchaseorderitems.tradeagreementid,purchaseorderitems.sellingprice from purchaseorder
                                            inner join purchaseorderitems on purchaseorderitems.purchaseorderid = purchaseorder.id
                                            left outer join currentpoitems on currentpoitems.id = purchaseorderitems.id
                                            left outer join items on items.id = itemid
                                            left outer join referencelist on referencelist.id = items.sizeid
                                            left outer join referencelist potypelist on potypelist.id = purchaseorder.potype
                                            where purchaseorder.id = @poid and purchaseorderitems.tradeagreementid = @tradeagreementid 
                                            and purchaseorder.isactive = true and purchaseorderitems.isactive = true and purchaseorder.status = 1 
                                            and oqty-(rqty+cqty-(coalesce(currentpoitems.qty,0)))>0";

            }
            else if (purchaseorderCreteria.lrinvoiceid > 0)
            {
                LrinvoiceDataService lrinvoiceDataService = new LrinvoiceDataService() { Context = this.Context };
                List<Lrinvoice> lrinvoice = lrinvoiceDataService.Search(new LrinvoiceCreteria()
                {
                    id = purchaseorderCreteria.lrinvoiceid,
                    showitems = false
                });

                LrinvoiceitemsDataService lrinvoiceitemsDataService = new LrinvoiceitemsDataService() { Context = this.Context };
                items = lrinvoiceitemsDataService.SearchAll(purchaseorderCreteria.lrinvoiceid);
                var productlist = string.Join(",", items.ConvertAll(e => e.productid));
                var purchaseorderitemids = new List<long>();
                foreach (var item in items)
                {
                    if (item.poinfo != null && item.poinfo.Count > 0)
                        purchaseorderitemids.AddRange(item.poinfo.ConvertAll(e => e.purchaseorderitemsid));
                }

                SQL_SearchPO = @"select purchaseorderitems.id,purchaseorder.podate,purchaseorder.id,purchaseorder.pono,oqty,
                            oqty-(rqty+cqty) balance,designid,price,purchaseorderitems.imageid,purchaseorderitems.description,referencelist.name sizename,
                            tolerance,purchaseorderitems.productid,items.sizeid,potypelist.name as potypename, purchaseorderitems.tradeagreementid from purchaseorder
                            inner join purchaseorderitems on purchaseorderitems.purchaseorderid = purchaseorder.id
                            left outer join items on items.id = itemid
							left outer join referencelist on referencelist.id = items.sizeid
                            left outer join referencelist potypelist on potypelist.id = purchaseorder.potype
                            where purchaseorder.supplierid = @supplierid 
                            and purchaseorder.isactive = true and purchaseorderitems.isactive = true and purchaseorder.status = 1 ";

                if (lrinvoice.Count > 0 && lrinvoice[0].purchaseorderid > 0)
                    SQL_SearchPO += " and purchaseorder.id=" + lrinvoice[0].purchaseorderid;
                if (lrinvoice.Count > 0 && lrinvoice[0].ptype > 0)
                    SQL_SearchPO += " and purchaseorder.potype=" + lrinvoice[0].ptype;
                if (lrinvoice.Count > 0 && lrinvoice[0].retaillocationid > 0)
                    SQL_SearchPO += " and purchaseorder.requestlocationid=" + lrinvoice[0].retaillocationid;

                // and purchaseorder.expectedate>=@expectedate 
                if (productlist.Length > 0 || purchaseorderitemids.Count > 0)
                {
                    SQL_SearchPO += " and (";
                    if (productlist.Length > 0)
                        SQL_SearchPO += " purchaseorderitems.productid in (" + productlist + ") or ";

                    if (purchaseorderitemids.Count > 0)
                        SQL_SearchPO += " purchaseorderitems.id in (" + string.Join(",", purchaseorderitemids) + ") or ";

                    SQL_SearchPO += "(purchaseorder.podate>=@fromdate and oqty-(rqty+cqty)>0))";
                }
                else
                {
                    SQL_SearchPO += " and (purchaseorder.podate>=@fromdate and oqty-(rqty+cqty)>0)";
                }
            }

            List<PurchaseOrderItemDetail> purchaseOrderItemDetails = new List<PurchaseOrderItemDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var command = GetCommand(SQL_SearchPO, conn);
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = purchaseorderCreteria.supplierid;
                command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = purchaseorderCreteria.productid;
                command.Parameters.Add("tradeagreementid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = purchaseorderCreteria.tradeagreementid;
                command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = purchaseorderCreteria.lrinvoiceid;
                command.Parameters.Add("poid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = purchaseorderCreteria.id;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.AddMonths(-6);
                command.Parameters.Add("expectedate", NpgsqlTypes.NpgsqlDbType.Date).Value = purchaseorderCreteria.expectedate;
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PurchaseOrderItemDetail purchaseOrderItemDetail = new PurchaseOrderItemDetail();
                        purchaseOrderItemDetail.purchaseorderitemsid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        purchaseOrderItemDetail.podate = reader["podate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["podate"]);
                        purchaseOrderItemDetail.pono = reader["pono"] == DBNull.Value ? "" : reader["pono"].ToString();
                        purchaseOrderItemDetail.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        purchaseOrderItemDetail.potypename = reader["potypename"] == DBNull.Value ? "" : reader["potypename"].ToString();
                        purchaseOrderItemDetail.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        purchaseOrderItemDetail.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        if (reader["sizename"] != DBNull.Value)
                            purchaseOrderItemDetail.description += "/" + reader["sizename"].ToString();
                        purchaseOrderItemDetail.imageid = reader["imageid"] == DBNull.Value ? "" : reader["imageid"].ToString();
                        purchaseOrderItemDetail.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        purchaseOrderItemDetail.balance = reader["balance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["balance"]);
                        purchaseOrderItemDetail.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        purchaseOrderItemDetail.order = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        purchaseOrderItemDetail.tolerance = reader["tolerance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tolerance"]);
                        purchaseOrderItemDetail.tradeagreementid = reader["tradeagreementid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tradeagreementid"]);
                        if (items != null && items.Exists(e => e.poinfo != null && e.poinfo.Exists(x => x.purchaseorderitemsid == purchaseOrderItemDetail.purchaseorderitemsid)))
                        {
                            purchaseOrderItemDetail.balance += items.Sum(e => e.poinfo == null ? 0 : (e.poinfo.Sum(x => x.purchaseorderitemsid == purchaseOrderItemDetail.purchaseorderitemsid ? x.aqty : 0)));
                        }
                        if (purchaseOrderItemDetail.tolerance > 0)
                        {
                            purchaseOrderItemDetail.maxqty = (purchaseOrderItemDetail.order + Math.Round(purchaseOrderItemDetail.order * purchaseOrderItemDetail.tolerance / 100, 0));
                        }
                        else
                        {
                            purchaseOrderItemDetail.maxqty = purchaseOrderItemDetail.order;
                        }
                        if (purchaseorderCreteria.tradeagreementid > 0)
                        {
                            purchaseOrderItemDetail.sprice = reader["sellingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sellingprice"]);
                            purchaseOrderItemDetail.mrp = reader["mrp"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["mrp"]);
                            purchaseOrderItemDetail.gln = reader["gln"] == DBNull.Value ? "" : reader["gln"].ToString();
                            purchaseOrderItemDetail.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                            purchaseOrderItemDetail.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                            purchaseOrderItemDetail.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"]);
                            purchaseOrderItemDetail.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"]);
                            purchaseOrderItemDetail.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"]);
                            purchaseOrderItemDetail.fitid = reader["fitid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fitid"]);
                            purchaseOrderItemDetail.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                            purchaseOrderItemDetail.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"]);
                        }
                        purchaseOrderItemDetails.Add(purchaseOrderItemDetail);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, purchaseorderCreteria);
            }
            return purchaseOrderItemDetails.OrderBy(e => e.podate).ToList();
        }

        public List<Purchaseorder> SearchAgreementPOSummary(PurchaseorderCreteria _reportcreteria)
        {
            ReferencelistDataService _referenceData = new ReferencelistDataService();
            long potypeid = _referenceData.GetAgreementPOType();
            if (potypeid == 0) return new List<Purchaseorder>();

            _reportcreteria.potype = potypeid;
            List<Purchaseorder> lstAgreement = SearchPOSummary(_reportcreteria);
            _reportcreteria.potype = 0;
            _reportcreteria.isagreement = true;
            List<Purchaseorder> lstpo = SearchPOSummary(_reportcreteria);

            foreach (Purchaseorder item in lstAgreement)
            {
                if (lstpo.Exists(e => e.agreementpoid == item.id))
                {
                    item.agreed = item.ordered;
                    item.ordered = lstpo.Sum(e => e.agreementpoid == item.id ? e.ordered : 0);
                    item.received = lstpo.Sum(e => e.agreementpoid == item.id ? e.received : 0);
                    item.cancelled = lstpo.Sum(e => e.agreementpoid == item.id ? e.cancelled : 0);
                    item.balance = item.ordered - (item.received + item.cancelled);
                }
            }
            return lstAgreement;
        }
        public List<Purchaseorder> SearchPOSummary(PurchaseorderCreteria _reportcreteria)
        {
            List<Purchaseorder> lst = new List<Purchaseorder>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchPOSummary;
                qb.AddParameter("purchaseorder.isactive", true, "=");

                if (_reportcreteria.ids != null &&
                   _reportcreteria.ids.Count > 0)
                {
                    qb.AddInParamer("purchaseorder.id", String.Join(",", _reportcreteria.ids));
                }
                if (_reportcreteria.isagreement)
                    qb.AddParameter("purchaseorder.agreementpoid", 0, ">");

                if (_reportcreteria.supplierid > 0)
                    qb.AddParameter("purchaseorder.supplierid", "supplierid", _reportcreteria.supplierid, "=");

                if (_reportcreteria.shopcontactemployeeid > 0)
                    qb.AddParameter("purchaseorder.shopcontactemployeeid",
                        "shopcontactemployeeid", _reportcreteria.shopcontactemployeeid, "=");

                if (_reportcreteria.ismobilepo)
                {
                    qb.AddParameter("purchaseorder.createdby", "createdby", Context.UserID, "=");
                    qb.AddParameter("purchaseorder.ismobilepo", "ismobilepo", true, "=");
                }
                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("purchaseorder.podate", "fromdate", _reportcreteria.fromdate.Date, ">=");
                    qb.AddParameter("purchaseorder.podate", "todate", _reportcreteria.todate.Date.AddDays(1), "<");
                }
                if (_reportcreteria.agentid > 0)
                    qb.AddParameter("purchaseorder.agentid", _reportcreteria.agentid, "=");

                if (_reportcreteria.internalstatus != Purchaseorder.InternalStatus.ALL)
                {
                    if (_reportcreteria.internalstatus == Purchaseorder.InternalStatus.UNAPPROVED)
                        qb.AddParameter("purchaseorder.internalstatus", 0, "=");
                    else if (_reportcreteria.internalstatus == Purchaseorder.InternalStatus.PENDING)
                        qb.Add2Query("internalstatus!=9 and status != 2 and status != 4");
                    else if (_reportcreteria.internalstatus == Purchaseorder.InternalStatus.COMPLETED || _reportcreteria.internalstatus == Purchaseorder.InternalStatus.FULLCOMPLETE)
                        qb.Add2Query("(oqty-(rqty+cqty)<=0 OR internalstatus=9 OR internalstatus=12 OR status = 4)");
                    else
                        qb.AddParameter("purchaseorder.internalstatus", (int)_reportcreteria.internalstatus, "=");
                }
                if (_reportcreteria.potype != 0)
                {
                    qb.AddParameter("purchaseorder.potype", _reportcreteria.potype, "=");
                }
                else
                {
                    ReferencelistDataService _referenceData = new ReferencelistDataService();
                    long potypeid = _referenceData.GetAgreementPOType();
                    if (potypeid > 0)
                        qb.AddParameter("purchaseorder.potype", potypeid, "!=");
                }

                if (_reportcreteria.companyid > 0)
                    qb.AddParameter("purchaseorder.companyid", _reportcreteria.companyid, "=");

                if (!string.IsNullOrEmpty(_reportcreteria.pono))
                    qb.AddParameter("purchaseorder.pono", _reportcreteria.pono, "=");

                qb.SortField = "purchaseorder.id desc";
                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Purchaseorder opurchaseorder = new Purchaseorder();
                        opurchaseorder.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opurchaseorder.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        opurchaseorder.actionby = reader["actionby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["actionby"]);
                        opurchaseorder.actionon = reader["actionon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["actionon"]);
                        opurchaseorder.tolerance = reader["tolerance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tolerance"]);
                        opurchaseorder.podate = reader["podate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["podate"]);
                        opurchaseorder.expectedate = reader["expectedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedate"]);
                        opurchaseorder.expirydate = reader["expirydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expirydate"]);
                        opurchaseorder.pono = reader["pono"] == DBNull.Value ? "" : reader["pono"].ToString();
                        opurchaseorder.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        opurchaseorder.pouniquenumber = reader["pouniquenumber"] == DBNull.Value ? "" : reader["pouniquenumber"].ToString();

                        opurchaseorder.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                        opurchaseorder.suppliercity = reader["suppliercity"] == DBNull.Value ? "" : reader["suppliercity"].ToString();
                        opurchaseorder.actionemployee = reader["actionemployee"] == DBNull.Value ? "" : reader["actionemployee"].ToString();
                        opurchaseorder.createdemployee = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();

                        opurchaseorder.potype = reader["potype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["potype"]);
                        opurchaseorder.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        opurchaseorder.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        opurchaseorder.deliverylocationid = reader["deliverylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deliverylocationid"]);
                        opurchaseorder.requestlocationid = reader["requestlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["requestlocationid"]);
                        opurchaseorder.termsandcondition_json = reader["termsandcondition"] == DBNull.Value ? "null" : reader["termsandcondition"].ToString();
                        opurchaseorder.isinterstate = reader["isinterstate"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstate"]);
                        opurchaseorder.additionalcharges_json = reader["additionalcharges"] == DBNull.Value ? "null" : reader["additionalcharges"].ToString();
                        opurchaseorder.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        opurchaseorder.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        opurchaseorder.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        opurchaseorder.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        opurchaseorder.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"]);
                        if ((reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"])) == 0)
                        {
                            opurchaseorder.status = Purchaseorder.POStatus.UNAPPROVED;
                        }
                        else
                        {
                            opurchaseorder.status = (Purchaseorder.POStatus)Convert.ToInt32(reader["status"]);
                        }
                        if ((reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"])) == 0)
                        {
                            opurchaseorder.internalstatus = Purchaseorder.InternalStatus.UNAPPROVED;
                        }
                        else
                        {
                            opurchaseorder.internalstatus = reader["internalstatus"] == DBNull.Value ? Purchaseorder.InternalStatus.UNAPPROVED : (Purchaseorder.InternalStatus)Convert.ToInt32(reader["internalstatus"]);
                        }
                        opurchaseorder.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        opurchaseorder.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);

                        opurchaseorder.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        opurchaseorder.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        opurchaseorder.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        opurchaseorder.freight = reader["freight"] == DBNull.Value ? 0 : Convert.ToInt32(reader["freight"]);
                        opurchaseorder.ordered = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        opurchaseorder.received = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        opurchaseorder.cancelled = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                        opurchaseorder.agreementpoid = reader["agreementpoid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agreementpoid"]);
                        opurchaseorder.balance = opurchaseorder.ordered - (opurchaseorder.received + opurchaseorder.cancelled);
                        if (opurchaseorder.balance < 0)
                            opurchaseorder.balance = 0;

                        if ((int)opurchaseorder.internalstatus < 2 &&
                            !string.IsNullOrEmpty(opurchaseorder.invoiceno))
                            opurchaseorder.internalstatus = Purchaseorder.InternalStatus.GRN;

                        if (!lst.Exists(x => x.id == opurchaseorder.id))
                            lst.Add(opurchaseorder);
                    }
                }
                //if (lst.Count == 1)
                //{
                //    PurchaseorderitemsDataService subPurchaseorderitems = new PurchaseorderitemsDataService() { Context = this.Context };
                //    subPurchaseorderitems.Context = this.Context;
                //    lst[0].Purchaseorderitems = subPurchaseorderitems.SearchAll(_reportcreteria.id);
                //}
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }


            return lst;
        }

        public List<PurchaseorderDetails> SearchPendingPO(PurchaseorderCreteria _reportcreteria)
        {
            List<PurchaseorderDetails> lst = new List<PurchaseorderDetails>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchPendingPO;

                qb.AddParameter("purchaseorder.isactive", true, "=");
                qb.AddParameter("purchaseorderitems.isactive", true, "=");

                if (!_reportcreteria.showdetail)
                    qb.Add2Query("oqty-(rqty + cqty) > 0");
                if (_reportcreteria.shopcontactemployeeid > 0)
                    qb.AddParameter("purchaseorder.shopcontactemployeeid",
                        "shopcontactemployeeid", _reportcreteria.shopcontactemployeeid, "=");
                if (_reportcreteria.supplierid > 0)
                    qb.AddParameter("purchaseorder.supplierid", "purchaseorder.supplierid", _reportcreteria.supplierid, "=");
                if (_reportcreteria.agentid > 0)
                    qb.AddParameter("purchaseorder.agentid", "purchaseorder.agentid", _reportcreteria.agentid, "=");

                if (_reportcreteria.potype != 0)
                {
                    qb.AddParameter("purchaseorder.potype", "purchaseorder.potype", _reportcreteria.potype, "=");
                }
                else
                {
                    ReferencelistDataService _referenceData = new ReferencelistDataService();
                    long potypeid = _referenceData.GetAgreementPOType();
                    if (potypeid > 0)
                        qb.AddParameter("purchaseorder.potype", potypeid, "!=");
                }

                if (_reportcreteria.companyid > 0)
                    qb.AddParameter("purchaseorder.companyid", "purchaseorder.companyid", _reportcreteria.companyid, "=");
                if (_reportcreteria.productid > 0)
                    qb.AddParameter("purchaseorderitems.productid", "purchaseorderitems.productid", _reportcreteria.productid, "=");
                if (_reportcreteria.brandid > 0)
                    qb.AddParameter("items.brandid", "items.brandid", _reportcreteria.brandid, "=");
                if (!string.IsNullOrEmpty(_reportcreteria.designid))
                    qb.AddParameter("purchaseorderitems.designid", "purchaseorderitems.designid", _reportcreteria.designid, "=");

                if (_reportcreteria.fromdate != DateTime.MinValue && _reportcreteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("purchaseorder.podate", "datefrom", _reportcreteria.fromdate.Date, ">=");
                    qb.AddParameter("purchaseorder.podate", "dateto", _reportcreteria.todate.Date.AddDays(1), "<");
                }
                command = qb.GetCommand(conn);

                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PurchaseorderDetails opurchaseorder = new PurchaseorderDetails();
                        opurchaseorder.tolerance = reader["tolerance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tolerance"]);
                        opurchaseorder.podate = reader["podate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["podate"]);
                        opurchaseorder.expectedate = reader["expectedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedate"]);
                        opurchaseorder.expirydate = reader["expirydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expirydate"]);
                        opurchaseorder.pono = reader["pono"] == DBNull.Value ? "" : reader["pono"].ToString();
                        opurchaseorder.potype = reader["potype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["potype"]);
                        opurchaseorder.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        opurchaseorder.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        opurchaseorder.agreementpoid = reader["agreementpoid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agreementpoid"]);
                        opurchaseorder.deliverylocationid = reader["deliverylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["deliverylocationid"]);
                        opurchaseorder.requestlocationid = reader["requestlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["requestlocationid"]);
                        opurchaseorder.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"]);
                        opurchaseorder.status = reader["status"] == DBNull.Value ? PurchaseorderDetails.POStatus.PENDING : (PurchaseorderDetails.POStatus)Convert.ToInt32(reader["status"]);
                        opurchaseorder.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        opurchaseorder.freight = reader["freight"] == DBNull.Value ? 0 : Convert.ToInt32(reader["freight"]);
                        opurchaseorder.ordered = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                        opurchaseorder.received = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        opurchaseorder.cancelled = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                        opurchaseorder.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        opurchaseorder.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        opurchaseorder.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        opurchaseorder.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        opurchaseorder.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        opurchaseorder.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        opurchaseorder.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        opurchaseorder.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        opurchaseorder.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        opurchaseorder.sleevename = reader["sleevename"] == DBNull.Value ? "" : reader["sleevename"].ToString();
                        opurchaseorder.fitname = reader["fitname"] == DBNull.Value ? "" : reader["fitname"].ToString();
                        opurchaseorder.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        opurchaseorder.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        opurchaseorder.priceupto = reader["priceupto"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["priceupto"]);
                        opurchaseorder.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        opurchaseorder.pricerange = reader["pricerange"] == DBNull.Value ? "" : reader["pricerange"].ToString();
                        opurchaseorder.balance = opurchaseorder.ordered - opurchaseorder.received - opurchaseorder.cancelled;

                        lst.Add(opurchaseorder);

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



        public Purchaseorder Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Purchaseorder oPurchaseorder)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("podate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchaseorder.podate;
            command.Parameters.Add("expectedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchaseorder.expectedate;
            command.Parameters.Add("expirydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchaseorder.expirydate;
            command.Parameters.Add("pono", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorder.pono == null ? "" : oPurchaseorder.pono;
            command.Parameters.Add("sizegroup", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorder.sizegroup == null ? "" : oPurchaseorder.sizegroup;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.supplierid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.companyid;
            command.Parameters.Add("deliverylocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.deliverylocationid;
            command.Parameters.Add("requestlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.requestlocationid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorder.status;
            command.Parameters.Add("tolerance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorder.tolerance;
            command.Parameters.Add("termsandcondition", NpgsqlTypes.NpgsqlDbType.Json).Value = oPurchaseorder.termsandcondition_json == null ? "null" : oPurchaseorder.termsandcondition_json;
            command.Parameters.Add("additionalcharges", NpgsqlTypes.NpgsqlDbType.Json).Value = oPurchaseorder.additionalcharges_json == null ? "null" : oPurchaseorder.additionalcharges_json;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPurchaseorder.isactive;
            command.Parameters.Add("isinterstate", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPurchaseorder.isinterstate;
            command.Parameters.Add("inclusivetax", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPurchaseorder.inclusivetax;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("potype", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorder.potype;

            command.Parameters.Add("shopcontactemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.shopcontactemployeeid;
            command.Parameters.Add("shopcontactnumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorder.shopcontactnumber == null ? "" : oPurchaseorder.shopcontactnumber;
            command.Parameters.Add("discountallowed", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorder.discountallowed;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorder.version;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorder.mode;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.agentid;
            command.Parameters.Add("freight", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorder.freight;
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.transportid;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorder.remarks == null ? "" : oPurchaseorder.remarks;
            command.Parameters.Add("pouniquenumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorder.pouniquenumber == null ? "" : oPurchaseorder.pouniquenumber;

            command.Parameters.Add("agreementpoid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.agreementpoid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPurchaseorder.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_purchaseorder)
            {
                SupplierDataService supplierDataService = new SupplierDataService() { Context = this.Context };
                var supplier = supplierDataService.SearchHeaders(new SupplierCreteria()
                {
                    id = oPurchaseorder.supplierid,
                    rowtype = -1
                }).First();

                Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.PurchaseOrder,
                oPurchaseorder.id, string.Format("PO CREATION REQUEST - {0} - {1}", supplier.name, oPurchaseorder.pono + " on " + oPurchaseorder.podate.ToString("dd-MM-yyyy")),
                        Context.UserID, 0, transaction, Context);
            }

            //InboxManager.Instance(true, oLrentry, transaction, conn);
            return oPurchaseorder;
        }

        public Purchaseorder Update(Purchaseorder oPurchaseorder)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    oPurchaseorder.status = Purchaseorder.POStatus.PENDING;
                    Update(transaction, conn, oPurchaseorder);

                    PurchaseorderitemsDataService subPurchaseorderitems = new PurchaseorderitemsDataService() { Context = this.Context };
                    subPurchaseorderitems.Context = this.Context;
                    subPurchaseorderitems.Update(transaction, conn, oPurchaseorder.Purchaseorderitems, oPurchaseorder.id);

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_notification_purchaseorder)
                    {
                        SupplierDataService supplierdata = new SupplierDataService() { Context = this.Context };
                        var supplier = supplierdata.Search(new SupplierCreteria() { id = oPurchaseorder.supplierid });
                        if (supplier != null && supplier.Count > 0)
                        {
                            Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.PurchaseOrder,
                                oPurchaseorder.id, string.Format("PURCHASE ORDER NO {0} MODIFIED - FOR THE SUPPLIER {1} WITH {2} QTY.", oPurchaseorder.pono, supplier[0].name,
                                oPurchaseorder.Purchaseorderitems.Sum(x => x.oqty)), Context.UserID,
                                oPurchaseorder.locationid, transaction, Context);
                        }
                    }

                    transaction.Commit();
                    conn.Close();

                    oPurchaseorder.Purchaseorderitems = subPurchaseorderitems.SearchAll(oPurchaseorder.id);
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPurchaseorder);


            }

            return oPurchaseorder;
        }

        public Purchaseorder Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Purchaseorder oPurchaseorder)
        {

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_purchaseorder)
            {
                oPurchaseorder.status = Purchaseorder.POStatus.PENDING;
            }
            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.id;
            command.Parameters.Add("podate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchaseorder.podate;
            command.Parameters.Add("expectedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchaseorder.expectedate;
            command.Parameters.Add("expirydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchaseorder.expirydate;
            command.Parameters.Add("pono", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorder.pono == null ? "" : oPurchaseorder.pono;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.supplierid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.companyid;
            command.Parameters.Add("deliverylocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.deliverylocationid;
            command.Parameters.Add("requestlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.requestlocationid;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorder.status;
            command.Parameters.Add("tolerance", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorder.tolerance;
            command.Parameters.Add("termsandcondition", NpgsqlTypes.NpgsqlDbType.Json).Value = oPurchaseorder.termsandcondition_json == null ? "null" : oPurchaseorder.termsandcondition_json;
            command.Parameters.Add("additionalcharges", NpgsqlTypes.NpgsqlDbType.Json).Value = oPurchaseorder.additionalcharges_json == null ? "null" : oPurchaseorder.additionalcharges_json;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPurchaseorder.isactive;
            command.Parameters.Add("isinterstate", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPurchaseorder.isinterstate;
            command.Parameters.Add("inclusivetax", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPurchaseorder.inclusivetax;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorder.mode;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorder.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchaseorder.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.agentid;
            command.Parameters.Add("freight", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorder.freight;
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.transportid;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorder.remarks == null ? "" : oPurchaseorder.remarks;
            command.Parameters.Add("pouniquenumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorder.pouniquenumber == null ? "" : oPurchaseorder.pouniquenumber;
            command.Parameters.Add("shopcontactemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.shopcontactemployeeid;
            command.Parameters.Add("shopcontactnumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchaseorder.shopcontactnumber == null ? "" : oPurchaseorder.shopcontactnumber;
            command.Parameters.Add("discountallowed", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorder.discountallowed;
            command.Parameters.Add("potype", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchaseorder.potype;
            command.Parameters.Add("agreementpoid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.agreementpoid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPurchaseorder.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_purchaseorder)
            {
                SupplierDataService supplierDataService = new SupplierDataService() { Context = this.Context };
                var supplier = supplierDataService.SearchHeaders(new SupplierCreteria()
                {
                    id = oPurchaseorder.supplierid,
                    rowtype = -1
                }).First();

                Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.PurchaseOrder,
                oPurchaseorder.id, string.Format("PO UPDATED REQUEST - {0} - {1}", supplier.name, oPurchaseorder.pono + " on " + oPurchaseorder.podate.ToString("dd-MM-yyyy")),
                        Context.UserID, 0, transaction, Context);
            }

            return oPurchaseorder;
        }

        internal bool UpdateDesingImage(long purchaseorderitemid, string imageid)
        {
            var param = new SortedList<string, object>();
            param.Add("id", purchaseorderitemid);
            param.Add("imageid", imageid);
            string query = "update purchaseorderitems set imageid=@imageid where id=@id";
            return ExecuteQuery(query, param) == 1;
        }

        public bool Delete(Purchaseorder oPurchaseorder)
        {
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oPurchaseorder);
                    PurchaseorderitemsDataService subPurchaseorderitems = new PurchaseorderitemsDataService() { Context = this.Context };
                    subPurchaseorderitems.Context = this.Context;
                    subPurchaseorderitems.DeleteAll(oPurchaseorder.id, oPurchaseorder.modifiedby, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPurchaseorder);
            }
            return hasSuccessCount;

        }

        public bool IsNonTextilePO(long poid)
        {
            string _SQL = @"select count(products.producttype) consumables from products inner join purchaseorderitems on purchaseorderitems.productid = products.id
                            where purchaseorderid=@poid and purchaseorderitems.isactive = true and products.producttype = 9";
            int _count = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlCommand command = GetCommand(_SQL, conn);
                    command.Parameters.Add("poid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = poid;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            _count = reader["consumables"] == DBNull.Value ? 0 : Convert.ToInt32(reader["consumables"]);
                        }
                    }
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, poid);
            }
            return _count > 0;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Purchaseorder oPurchaseorder)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPurchaseorder.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorder.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }
        public Purchaseorder UpdateStatus(NpgsqlTransaction transaction, NpgsqlConnection conn, Purchaseorder oPurchaseorder)
        {
            if ((Purchaseorder.POStatus)oPurchaseorder.status == Purchaseorder.POStatus.APPROVED ||
                (Purchaseorder.POStatus)oPurchaseorder.status == Purchaseorder.POStatus.REJECTED)
            {
                NpgsqlCommand command = GetCommand(SQL_UpdateStatus, conn, transaction);
                command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = (Purchaseorder.POStatus)oPurchaseorder.status;
                command.Parameters.Add("internalstatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = (Purchaseorder.InternalStatus)oPurchaseorder.internalstatus;
                //internalstatus
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorder.version;
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        oPurchaseorder.version = Convert.ToInt32(reader[0].ToString());
                    }
                }
                return oPurchaseorder;
            }
            else
            {
                NpgsqlCommand command = GetCommand(SQL_UpdateStatus.Replace(",actionby=@modifiedby", ""), conn, transaction);
                command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = (Purchaseorder.POStatus)oPurchaseorder.status;
                command.Parameters.Add("internalstatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = (Purchaseorder.InternalStatus)oPurchaseorder.internalstatus;
                //internalstatus
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchaseorder.version;
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchaseorder.id;
                int successCount = command.ExecuteNonQuery();
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        oPurchaseorder.version = Convert.ToInt32(reader[0].ToString());
                    }
                }
                return oPurchaseorder;
            }
        }
        public Purchaseorder UpdateStatus(Purchaseorder oPurchaseorder)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    UpdateStatus(transaction, conn, oPurchaseorder);
                    transaction.Commit();
                    conn.Close();
                }

                EmployeeDataService usersDataService = new EmployeeDataService() { Context = this.Context };
                var header = usersDataService.SearchHeader(new EmployeeCreteria() { id = Context.EmployeeID });
                oPurchaseorder.actionby = Context.UserID;
                oPurchaseorder.actionon = DateTime.Now;
                oPurchaseorder.actionemployee = header.Count > 0 ? header[0].name : "";
                if (oPurchaseorder.status == Purchaseorder.POStatus.APPROVED &&
                    Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_purchaseorder)
                {
                    //PurchaseorderPrint print = Print(new Purchaseorder() { id = oPurchaseorder.id });
                    //Publish(Context.OrganizationID, print);
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPurchaseorder);
            }

            return oPurchaseorder;
        }
        public PurchaseorderPrint Print(Purchaseorder _purchaseorder)
        {
            var purchaseorder = Search(new PurchaseorderCreteria()
            {
                id = _purchaseorder.id,
                pono = _purchaseorder.pono
            });

            PurchaseorderPrint ret = new PurchaseorderPrint();
            ret.id = purchaseorder[0].id;
            ret.tolerance = purchaseorder[0].tolerance;
            ret.companyid = purchaseorder[0].companyid;
            ret.locationid = purchaseorder[0].locationid;
            ret.deliverylocationid = purchaseorder[0].deliverylocationid;
            ret.requestlocationid = purchaseorder[0].requestlocationid;
            ret.podate = purchaseorder[0].podate;
            ret.pono = purchaseorder[0].pono;
            ret.potype = purchaseorder[0].potype;
            ret.potypename = purchaseorder[0].potypename;
            ret.expectedate = purchaseorder[0].expectedate;
            ret.expirydate = purchaseorder[0].expirydate;
            ret.supplierid = purchaseorder[0].supplierid;
            ret.termsandcondition = purchaseorder[0].termsandcondition;
            ret.isinterstate = purchaseorder[0].isinterstate;
            ret.mode = purchaseorder[0].mode;
            ret.status = purchaseorder[0].status;
            ret.Purchaseorderitems = purchaseorder[0].Purchaseorderitems;
            ret.freight = purchaseorder[0].freight;
            ret.agentid = purchaseorder[0].agentid;
            ret.shopcontactemployeeid = purchaseorder[0].shopcontactemployeeid;
            ret.shopcontactnumber = purchaseorder[0].shopcontactnumber;
            ret.createdby = purchaseorder[0].createdby;
            ret.createdon = purchaseorder[0].createdon;
            ret.modifiedby = purchaseorder[0].modifiedby;
            ret.modifiedon = purchaseorder[0].modifiedon;
            ret.actionby = purchaseorder[0].actionby;
            ret.actionon = purchaseorder[0].actionon;
            ret.transportid = purchaseorder[0].transportid;
            ret.remarks = purchaseorder[0].remarks;
            ret.pouniquenumber = purchaseorder[0].pouniquenumber;
            ret.discountallowed = purchaseorder[0].discountallowed;
            ret.Gross = ret.Purchaseorderitems.Sum(x => x.Gross);
            ret.tax = ret.Purchaseorderitems.Sum(x => x.totaltax);
            ret.Net = ret.Purchaseorderitems.Sum(x => x.Net);
            ret.singletaxpercentage = "";
            if (ret.Purchaseorderitems.ConvertAll(x => x.taxpercentage).Distinct().ToList().Count == 1)
            {
                ret.singletaxpercentage = ret.Purchaseorderitems[0].taxpercentage.ToString("F0") + "%";
            }
            PurchaseorderitemsDataService purchaseorderitemsDataService = new PurchaseorderitemsDataService() { Context = this.Context };
            purchaseorderitemsDataService.UpdateItemDetails(ret.Purchaseorderitems);

            if (ret.isinterstate)
                ret.Purchaseorderitems.ForEach(x => x.isinterstate = true);

            Data.CompanyDataService companyservice = new CompanyDataService() { Context = this.Context };
            companyservice.Context = this.Context;
            var comnanylist = companyservice.Search(new CompanyCreteria()
            {
                id = ret.companyid
            });

            ret.company = comnanylist[0];
            ret.company.parseaddress();
            Data.SupplierDataService _supplierdata = new SupplierDataService() { Context = this.Context };
            var supplier = _supplierdata.Search(new SupplierCreteria()
            {
                id = ret.supplierid
            });
            if (supplier != null && supplier.Count > 0)
            {
                ret.supplier = supplier[0];
                ret.suppliercompany = supplier[0].companylist != null && supplier[0].companylist.Count > 0 ? supplier[0].companylist[0] : null;
            }

            Data.ReferencelistDataService _refdata = new ReferencelistDataService() { Context = this.Context };
            var _deliverylocation = _refdata.Search(new ReferencelistCreteria()
            {
                id = ret.deliverylocationid
            });

            if (_deliverylocation != null && _deliverylocation.Count > 0 && _deliverylocation[0].additionalinfo != null)
            {
                ret.deliverylocation = new CompanyLocation(_deliverylocation[0]);
                if (ret.deliverylocation.stateid > 0)
                {
                    var states = _refdata.SearchList(new ReferencelistCreteria()
                    {
                        id = ret.deliverylocation.stateid
                    });
                    if (states != null && states.Count > 0)
                    {
                        ret.deliverylocation.statename = states[0].name;
                        ret.deliverylocation.statecode = states[0].code;
                    }
                }
                if (ret.deliverylocation.cityid > 0)
                {
                    var states = _refdata.SearchList(new ReferencelistCreteria()
                    {
                        id = ret.deliverylocation.cityid
                    });
                    if (states != null && states.Count > 0)
                    {
                        ret.deliverylocation.cityname = states[0].name;
                    }
                }
            }

            if (ret.requestlocationid > 0)
            {
                var _requestlocation = _refdata.Search(new ReferencelistCreteria()
                {
                    id = ret.requestlocationid
                });
                if (_requestlocation != null && _requestlocation.Count > 0 && _requestlocation[0].additionalinfo != null)
                {
                    ret.requestlocation = new CompanyLocation(_deliverylocation[0]);
                    if (ret.requestlocation.stateid > 0)
                    {
                        var states = _refdata.SearchList(new ReferencelistCreteria()
                        {
                            id = ret.requestlocation.stateid
                        });
                        if (states != null && states.Count > 0)
                        {
                            ret.requestlocation.statename = states[0].name;
                            ret.requestlocation.statecode = states[0].code;
                        }
                    }
                    if (ret.requestlocation.cityid > 0)
                    {
                        var states = _refdata.SearchList(new ReferencelistCreteria()
                        {
                            id = ret.requestlocation.cityid
                        });
                        if (states != null && states.Count > 0)
                        {
                            ret.requestlocation.cityname = states[0].name;
                        }
                    }
                }
            }

            if (ret.transportid > 0)
            {
                Data.TransportDataService transdata = new TransportDataService() { Context = this.Context };
                var translist = transdata.Search(new TransportCreteria()
                {
                    id = ret.transportid
                });
                if (translist.Count > 0)
                {
                    ret.transport = translist[0];
                }
            }

            if (ret.agentid > 0)
            {
                AgentDataService _agentdata = new AgentDataService() { Context = this.Context };
                var agent = _agentdata.Search(new AgentCreteria()
                {
                    id = ret.agentid
                });
                if (agent.Count > 0)
                    ret.agent = agent[0];
            }
            else
            {
                ret.agent = new Agent() { name = "DIRECT" };
            }


            List<long> userids = new List<long>();
            userids.Add(ret.createdby);
            userids.Add(ret.actionby);
            Data.EmployeeDataService _empdata = new EmployeeDataService() { Context = this.Context };
            var emplist = _empdata.GetUserName(userids);
            if (emplist != null && emplist.Count > 0)
            {
                if (emplist.Exists(e => e.id == ret.createdby)) ret.whocreated = emplist.Find(e => e.id == ret.createdby).name;
                if (emplist.Exists(e => e.id == ret.actionby)) ret.whoapproved = emplist.Find(e => e.id == ret.actionby).name;
            }

            if (ret.shopcontactemployeeid > 0)
            {
                var shopcontactperson = _empdata.SearchHeader(new EmployeeCreteria() { id = ret.shopcontactemployeeid });
                if (shopcontactperson.Count == 1)
                    ret.shopcontactemployee = shopcontactperson.First().name;
            }

            ret.isnontextilepo = IsNonTextilePO(ret.id);
            ret.HSNList = new List<PurchaseorderPrint.HSNItem>();

            if (ret.Purchaseorderitems != null)
            {
                foreach (var grp in ret.Purchaseorderitems.GroupBy(e => new { e.isinterstate, e.hsncode, e.taxpercentage }))
                {
                    ret.HSNList.Add(new PurchaseorderPrint.HSNItem()
                    {
                        code = grp.Key.hsncode,
                        isinterstate = grp.Key.isinterstate,
                        taxable = grp.Sum(e => e.Gross),
                        tax = grp.Sum(e => e.totaltax),
                        total = grp.Sum(e => e.Net),
                        qty = grp.Sum(e => e.oqty),
                        peice = grp.Sum(e => e.oqty),
                        cgst_percentage = (grp.Key.isinterstate || grp.Key.taxpercentage == 0) ? 0 : grp.Key.taxpercentage / 2,
                        igst_percentage = (!grp.Key.isinterstate || grp.Key.taxpercentage == 0) ? 0 : grp.Key.taxpercentage,
                        sgst_percentage = (grp.Key.isinterstate || grp.Key.taxpercentage == 0) ? 0 : grp.Key.taxpercentage / 2,
                        sgst_value = grp.Sum(e => e.sgst),
                        cgst_value = grp.Sum(e => e.cgst),
                        igst_value = grp.Sum(e => e.igst)
                    });
                }

                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5 &&
                    ret.Purchaseorderitems.Exists(x => !string.IsNullOrEmpty(x.gln)))
                {
                    MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
                    var uanlist = mfgdesignDataService.SearchUANItem(ret.Purchaseorderitems.ConvertAll(x => x.gln).Distinct().ToList());
                    ret.Purchaseorderitems.ForEach(x =>
                    {
                        if (uanlist.Exists(y => y.uid == x.gln))
                        {
                            if(x.mrp == 0)
                                x.mrp = uanlist.Find(y => y.uid == x.gln).mrp;
                            x.imageid = uanlist.Find(y => y.uid == x.gln).img;
                        }
                    });
                }
            }

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var sizelist = referencelistDataService.Search(new ReferencelistCreteria()
            {
                ids = ret.Purchaseorderitems.ConvertAll(x => x.sizeid).Distinct().ToList()
            });
            sizelist.ForEach(x =>
            {
                ret.Purchaseorderitems.FindAll(y => y.sizeid == x.id).ForEach(z => z.sizeindex = x.sortingindex);
            });
            ret.Purchaseorderitems = ret.Purchaseorderitems.OrderBy(x => x.designid).ThenBy(y => y.sizeindex).ToList();
            return ret;

        }
        public bool Publish(long organizationid, PurchaseorderPrint print)
        {
            if (!string.IsNullOrEmpty(print.supplier.contactnumber) && print.supplier.contactnumber.Length >= 10 &&
                !string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(organizationid).Config.hostedserver))
            {
                string licensekey = LicenseManager.License.Key.Replace("-", "");
                InfyPOS.Controllers.ImageController.HashDetail hashDetail = new InfyPOS.Controllers.ImageController.HashDetail(organizationid, InfyPOS.Controllers.ImageController.HashDetail.GetLongHash(licensekey), print.id);
                string smscontent = print.company.name + " PO (Order No " + print.pono + "/" + print.podate.ToString("dd-MM-yyyy") + ")\r\n";
                smscontent += InfyPOS.Services.ConfigManager.Instance(organizationid).Config.hostedserver + (InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.hostedserver.EndsWith("/") ? "" : "/") + "File/PO/" + LicenseManager.License.Key.Replace("-", "") + "/" + System.Web.HttpUtility.UrlEncode(hashDetail.Hash);

                Processors.Schedule_Actions.Instance.OnChange((organizationid), print, Processors.Schedule_Actions.Actions.Publish);

                return true;
                //InfyPOS.Data.SmshistoryDataService smshistoryDataService = new Data.SmshistoryDataService() { Context = this.Context };
                //return smshistoryDataService.SendWithLog(organizationid, print.supplier.contactnumber, smscontent, "Purchase Order");
                //return InfyPOS.Services.SMSManager.Instance.SendSMS((organizationid), print.supplier.contactnumber, smscontent);
            }
            else
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("Supplier Mobile No or Host not configured."), WebAPI.Common.Logger.LogType.Error, null);
            }
            return false;
        }

        internal List<PurchaseOrderHistory> SaveUANPO(POUANUploaded pouploaditem)
        {
            List<PurchaseOrderHistory> history = new List<PurchaseOrderHistory>();
            NpgsqlTransaction transaction = null;
            try
            {
                SupplierDataService supplierdata = new SupplierDataService() { Context = this.Context };
                PurchaseorderitemsDataService subPurchaseorderitems = new PurchaseorderitemsDataService() { Context = this.Context };
                CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
                var allcompany = companyDataService.Search(new CompanyCreteria()
                {
                });
                subPurchaseorderitems.Context = this.Context;
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var orderitem in pouploaditem.orders)
                    {
                        var sequence = SequenceManager.Instance(Context.OrganizationID).Get("purchaseorder.pono",
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportlocationwisesopackingno ? orderitem.enntrylocationid : 0,
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ? pouploaditem.companyid : 0);

                        var company = allcompany.Find(x => x.id == orderitem.companyid);

                        Purchaseorder oPurchaseorder = new Purchaseorder();
                        oPurchaseorder.companyid = orderitem.companyid;
                        oPurchaseorder.deliverylocationid = orderitem.locationid;
                        oPurchaseorder.supplierid = orderitem.supplierid;
                        oPurchaseorder.expectedate = orderitem.DueDate;
                        oPurchaseorder.expirydate = orderitem.ExpiryDate;
                        oPurchaseorder.podate = orderitem.PODate;
                        oPurchaseorder.remarks = orderitem.Remark;
                        oPurchaseorder.pouniquenumber = orderitem.OrderSplit;
                        oPurchaseorder.potype = -5;
                        oPurchaseorder.mode = 5;
                        oPurchaseorder.version = 1;
                        oPurchaseorder.isactive = true;
                        oPurchaseorder.locationid = orderitem.locationid;
                        oPurchaseorder.requestlocationid = orderitem.enntrylocationid;
                        oPurchaseorder.Purchaseorderitems = new List<PurchaseorderitemsDetail>();
                        var supplier = supplierdata.Search(new SupplierCreteria() { id = orderitem.supplierid });
                        oPurchaseorder.agentid = supplier[0].agentid;
                        if (supplier.First().gst != "" && company.gstno != "")
                        {
                            oPurchaseorder.isinterstate = supplier.First().gst.Substring(0, 2) != company.gstno.Substring(0, 2);
                        }
                        foreach (var oitem in orderitem.items)
                        {
                            PurchaseorderitemsDetail oPurchaseorderitemsDetail = new PurchaseorderitemsDetail();
                            oPurchaseorderitemsDetail.gln = oitem.UAN;
                            oPurchaseorderitemsDetail.sno = oPurchaseorder.Purchaseorderitems.Count() + 1;
                            oPurchaseorderitemsDetail.productname = oitem.Product;
                            oPurchaseorderitemsDetail.printingname = oitem.Product;
                            oPurchaseorderitemsDetail.size = oitem.Size;
                            oPurchaseorderitemsDetail.sizename = oitem.Size;
                            oPurchaseorderitemsDetail.utolerance = oitem.Tolerance;
                            oPurchaseorderitemsDetail.sizeid = oitem.SizeID;
                            oPurchaseorderitemsDetail.colourid = oitem.ColourID;
                            oPurchaseorderitemsDetail.mrp = oitem.MRP;
                            oPurchaseorderitemsDetail.price = oitem.Cost;
                            oPurchaseorderitemsDetail.productid = oitem.ProductID;
                            oPurchaseorderitemsDetail.itemid = oitem.ItemID;
                            oPurchaseorderitemsDetail.designid = oitem.Design;
                            oPurchaseorderitemsDetail.designname = oitem.Design;
                            oPurchaseorderitemsDetail.hsncode = oitem.HSN;
                            oPurchaseorderitemsDetail.oqty = oitem.Qty;
                            oPurchaseorderitemsDetail.isactive = true;
                            oPurchaseorderitemsDetail.tax = oitem.Tax / oitem.Qty;
                            oPurchaseorderitemsDetail.taxpercentage = oitem.Percentage;
                            oPurchaseorderitemsDetail.sellingprice = oitem.MRP;
                            oPurchaseorderitemsDetail.taxid = oitem.TaxID;
                            oPurchaseorder.Purchaseorderitems.Add(oPurchaseorderitemsDetail);
                        }

                        MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
                        var uanlist = mfgdesignDataService.SearchUANItem(oPurchaseorder.Purchaseorderitems.ConvertAll(x => x.gln).Distinct().ToList());
  
                        oPurchaseorder.pono = sequence.GetNextNumber(Context, conn, transaction);
                        Insert(transaction, conn, oPurchaseorder);
                        foreach (var item in oPurchaseorder.Purchaseorderitems)
                        {
                            if (!string.IsNullOrEmpty(item.pricerange))
                                item.priceranges = item.pricerange;
                            item.groupcode = Guid.NewGuid().ToString();
                            item.purchaseorderid = oPurchaseorder.id;

                            if (uanlist.Exists(y => y.uid == item.gln))
                            {
                                if (item.mrp == 0)
                                    item.mrp = uanlist.Find(y => y.uid == item.gln).mrp;
                                item.imageid = uanlist.Find(y => y.uid == item.gln).img;
                            }

                            if (item.jumpqty != null && item.jumpqty.Count > 0)
                            {
                                //item.rowtype = 1;
                                //subPurchaseorderitems.Insert(transaction, conn, item);
                                item.groupname = string.Format("{0}-{1}", item.jumpqty.First().name, item.jumpqty.Last().name);
                                foreach (var jitem in item.jumpqty)
                                {
                                    item.id = 0;
                                    item.itemid = 0;
                                    item.oqty = jitem.qty;
                                    item.sizeid = jitem.id;
                                    item.size = jitem.name;
                                    item.sizename = jitem.name;
                                    item.rowtype = 2;
                                    subPurchaseorderitems.Insert(transaction, conn, item);
                                }
                            }
                            else
                            {
                                item.groupname = item.sizename;
                                if (!string.IsNullOrEmpty(item.sizename) && string.IsNullOrEmpty(item.size))
                                    item.size = item.sizename;
                                subPurchaseorderitems.Insert(transaction, conn, item);
                            }
                        }
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_notification_purchaseorder)
                        {
                            string inchargename = "";
                            if (oPurchaseorder.shopcontactemployeeid > 0)
                            {
                                EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
                                employeeDataService.Context = this.Context;
                                inchargename = " (" + employeeDataService.SearchHeader(new EmployeeCreteria()
                                {
                                    id = oPurchaseorder.shopcontactemployeeid
                                }).First().name + ")";
                            }
                            if (supplier != null && supplier.Count > 0)
                            {
                                Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID, Services.IntrayManager.EntryType.PurchaseOrder,
                                    oPurchaseorder.id, string.Format("PURCHASE ORDER - NO {0} FOR THE SUPPLIER {1} WITH {2} QTY {3}.", oPurchaseorder.pono, supplier[0].name,
                                    oPurchaseorder.Purchaseorderitems.Sum(x => x.oqty),
                                    inchargename), Context.UserID, oPurchaseorder.locationid,
                                    oPurchaseorder.companyid, transaction, Context);
                            }
                        }
                        oPurchaseorder.Purchaseorderitems = subPurchaseorderitems.SearchAll(oPurchaseorder.id);
                        Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, oPurchaseorder, Processors.Schedule_Actions.Actions.New);
                        history.Add(new PurchaseOrderHistory()
                        {
                            id = oPurchaseorder.id,
                            pono = oPurchaseorder.pono,
                            remark = oPurchaseorder.remarks,
                            pouniquenumber = oPurchaseorder.pouniquenumber,
                            amount = oPurchaseorder.Purchaseorderitems.Sum(x => x.amount),
                            supplier = orderitem.Supplier,
                            qty = oPurchaseorder.Purchaseorderitems.Sum(x => x.oqty)
                        });
                    }
                    transaction.Commit();
                    conn.Close();
                }

                var order = SearchPOSummary(new PurchaseorderCreteria()
                {
                    ids = history.ConvertAll(x => x.id)
                });
                history.ForEach(e =>
                {
                    e.order = order.Find(x => x.id == e.id);
                });
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, pouploaditem);
            }
            return history;
        }

        internal bool RemovePOGRN(Lrentry item)
        {
            var oPurchaseorder = Search(new PurchaseorderCreteria()
            {
                id = item.importreferenceid
            }).First();
            LrinvoiceDataService lrinvoiceDataService = new LrinvoiceDataService() { Context = this.Context };
            LrinvoiceitemsDataService lrinvoiceitemsDataService = new LrinvoiceitemsDataService() { Context = this.Context };
            var lrinvoices = lrinvoiceDataService.Search(new LrinvoiceCreteria()
            {
                importreferenceid = item.importreferenceid,
                importreferencetype = 3
            });
            lrinvoiceitemsDataService.DeleteBarcode(lrinvoices[0].Lrinvoiceitems.ConvertAll(x => x.id));
            lrinvoiceDataService.Delete(new Lrinvoice()
            { 
                id = lrinvoices[0].id,
                lrid = lrinvoices[0].lrid,
            });
            return true;
        }
        internal Purchaseorder SavePOGRN(Lrentry item)
        {
            NpgsqlTransaction transaction = null;
            Purchaseorder oPurchaseorder = null;
            try
            {
                oPurchaseorder = Search(new PurchaseorderCreteria()
                {
                    id = item.importreferenceid
                }).First();
                LrentryDataService lrentryDataService = new LrentryDataService() { Context = this.Context };
                LrinvoiceDataService lrinvoiceDataService = new LrinvoiceDataService() { Context = this.Context };
                LrinvoicetaxDataService subLrinvoicetaxs = new LrinvoicetaxDataService() { Context = this.Context };

                TaxDataService taxDataService = new TaxDataService() { Context = Context };
                var taxlist = taxDataService.Search(new TaxCreteria() { ids = oPurchaseorder.Purchaseorderitems.ConvertAll(x => x.taxid).Distinct().ToList() });

                if(oPurchaseorder.Purchaseorderitems.Exists(x => x.taxid == 0))
                {
                    throw new Exception("Some of the product don't have taxes , Sample UAN - "+
                     oPurchaseorder.Purchaseorderitems.Find(x => x.taxid == 0).gln);
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (item.id == 0)
                    {
                        item.lrcompanyid = oPurchaseorder.companyid;
                        item.entrylocationid = oPurchaseorder.requestlocationid;
                        item.retaillocationid = oPurchaseorder.deliverylocationid;
                        lrentryDataService.Insert(item);

                        if (item.id == 0)
                            throw new Exception(item.ErrorMessage);

                        ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
                        var glninfo = itemsDataService.Search(new ItemsCreteria()
                        {
                            glns = oPurchaseorder.Purchaseorderitems.ConvertAll(x => x.gln).Distinct().ToList()
                        });

                        var invoice = new Lrinvoice()
                        {
                            isactive = true,
                            invoicedate = item.invoicedate,
                            invoiceno = item.invoiceno,
                            companyid = item.lrcompanyid,
                            entrydate = DateTimeNow,
                            isinterstatesale = oPurchaseorder.isinterstate,
                            lrid = item.id,
                            noofbundle = item.noofbundels,
                            noofpieces = item.noofpieces,
                            pono = oPurchaseorder.pono,
                            statusid = 6,
                            purchaseorderid = oPurchaseorder.id,
                            ptype = (int)oPurchaseorder.potype,
                            lrsupplierid = oPurchaseorder.supplierid,
                            supplierid = oPurchaseorder.supplierid,
                            supportpo = true,
                            suppliercompanyid = oPurchaseorder.suppliercompanyid,
                            retaillocationid = oPurchaseorder.deliverylocationid,
                            entrylocationid = oPurchaseorder.locationid,
                            Lrinvoiceitems = new List<LrinvoiceItemDetail>()
                        };

                        oPurchaseorder.Purchaseorderitems.ForEach(x =>
                        {
                            var invoiceitems = new LrinvoiceItemDetail()
                            {
                                uan = x.gln,
                                sno = (invoice.Lrinvoiceitems.Count + 1),
                                itemid = x.itemid,
                                hsncode = x.hsncode,
                                productid = x.productid,
                                buyingprice = x.cost,
                                buyingqty = x.oqty,
                                ptaxid = x.taxid,
                                taxid = x.taxid,
                                basebuyingprice = x.cost,
                                designid = x.designid,
                                mrp = x.mrp,
                                version = 1,
                                imgid = x.imageid,
                                sellingprice = x.sellingprice,
                                purchasetaxpercentage = x.taxpercentage,
                                purchasetax = Math.Round((x.cost * x.oqty) * x.taxpercentage / 100, 2),
                                amount = (x.cost * x.oqty),
                                isactive = true,
                                CustomFields = new List<LrinvoiceItemDetail.CustomFieldValue>(),
                                poinfo = new List<POItemInfo>(),
                                Details = new Items()
                                {
                                    sizeid = x.sizeid,
                                    brandid = x.brandid,
                                    productid = x.productid,
                                    serialno = x.gln
                                }
                            };

                            var taxinfo = taxlist.Find(y => y.id == x.taxid);
                            var taxpercentage = taxinfo.taxsplit.Find(x.mrp).taxpercent;
                            var tax = Math.Round((x.mrp / (100 + taxpercentage) * taxpercentage), 2);
                            invoiceitems.taxid = x.taxid;
                            invoiceitems.salestax = tax;
                            invoiceitems.sellingprice = x.mrp;

                            var glnitem = glninfo.Find(y => y.gln == x.gln);
                            if (glnitem.CustomFields != null &&
                            glnitem.CustomFields.Count > 0)
                            {
                                invoiceitems.CustomFields.AddRange(glnitem.CustomFields.ConvertAll(z => new LrinvoiceItemDetail.CustomFieldValue()
                                {
                                    key = z.key,
                                    value = z.value
                                }));
                            }

                            decimal tqty = x.utolerance > 0 ? Math.Round((x.oqty * x.utolerance / 100), MidpointRounding.AwayFromZero) : 0;
                            invoiceitems.poinfo.Add(new POItemInfo()
                            {
                                designid = x.designid,
                                oqty = x.oqty,
                                aqty = x.aqty,
                                price = x.price,
                                sizeid = x.sizeid,
                                pono = oPurchaseorder.pono,
                                purchaseorderitemsid = x.id,
                                tqty = x.oqty + tqty
                            });
                            invoice.Lrinvoiceitems.Add(invoiceitems);
                        });

                        invoice.Lrinvoicetaxitems = new List<Lrinvoicetax>();
                        foreach (var taxgrp in invoice.Lrinvoiceitems.GroupBy(x => new { x.ptaxid, x.purchasetaxpercentage }))
                        {
                            invoice.Lrinvoicetaxitems.Add(new Lrinvoicetax()
                            {
                                isactive = true,
                                amounton = taxgrp.Sum(x => x.amount),
                                taxid = taxgrp.Key.ptaxid,
                                percentage = taxgrp.Key.purchasetaxpercentage,
                                taxamount = taxgrp.Sum(x => x.purchasetax),
                                netamount = taxgrp.Sum(x => x.amount + x.purchasetax)
                            });
                        }
                        lrinvoiceDataService.Insert(transaction, conn, invoice);
                        lrinvoiceDataService.ImportInvoiceItems(invoice, transaction, conn);
                        invoice.Lrinvoicetaxitems.ForEach(e => e.isinterstatesale = invoice.isinterstatesale);
                        foreach (var taxitem in invoice.Lrinvoicetaxitems)
                        {
                            taxitem.invoiceid = invoice.id;
                            taxitem.createdon = invoice.createdon;
                            taxitem.modifiedon = DateTimeNow;
                            taxitem.createdby = Context.UserID;
                            subLrinvoicetaxs.Insert(transaction, conn, taxitem);
                        }
                    }
                    else
                    {
                        lrentryDataService.Update(transaction, conn, item);
                        var invoices = lrinvoiceDataService.Search(new LrinvoiceCreteria()
                        {
                            purchaseorderid = oPurchaseorder.id
                        });

                        if (invoices.Count == 0)
                        {
                            ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
                            var glninfo = itemsDataService.Search(new ItemsCreteria()
                            {
                                glns = oPurchaseorder.Purchaseorderitems.ConvertAll(x => x.gln).Distinct().ToList()
                            });

                            var invoice = new Lrinvoice()
                            {
                                isactive = true,
                                invoicedate = item.invoicedate,
                                invoiceno = item.invoiceno,
                                companyid = item.lrcompanyid,
                                entrydate = DateTimeNow,
                                isinterstatesale = oPurchaseorder.isinterstate,
                                lrid = item.id,
                                noofbundle = item.noofbundels,
                                noofpieces = item.noofpieces,
                                pono = oPurchaseorder.pono,
                                statusid = 6,
                                purchaseorderid = oPurchaseorder.id,
                                ptype = (int)oPurchaseorder.potype,
                                lrsupplierid = oPurchaseorder.supplierid,
                                supplierid = oPurchaseorder.supplierid,
                                supportpo = true,
                                suppliercompanyid = oPurchaseorder.suppliercompanyid,
                                retaillocationid = oPurchaseorder.deliverylocationid,
                                entrylocationid = oPurchaseorder.locationid,
                                Lrinvoiceitems = new List<LrinvoiceItemDetail>()
                            };

                            oPurchaseorder.Purchaseorderitems.ForEach(x =>
                            {
                                var invoiceitems = new LrinvoiceItemDetail()
                                {
                                    uan = x.gln,
                                    sno = (invoice.Lrinvoiceitems.Count + 1),
                                    itemid = x.itemid,
                                    hsncode = x.hsncode,
                                    productid = x.productid,
                                    buyingprice = x.cost,
                                    buyingqty = x.oqty,
                                    ptaxid = x.taxid,
                                    taxid = x.taxid,
                                    basebuyingprice = x.cost,
                                    designid = x.designid,
                                    mrp = x.mrp,
                                    version = 1,
                                    sellingprice = x.sellingprice,
                                    purchasetaxpercentage = x.taxpercentage,
                                    purchasetax = Math.Round((x.cost * x.oqty) * x.taxpercentage / 100, 2),
                                    amount = (x.cost * x.oqty),
                                    isactive = true,
                                    CustomFields = new List<LrinvoiceItemDetail.CustomFieldValue>(),
                                    poinfo = new List<POItemInfo>(),
                                    Details = new Items()
                                    {
                                        sizeid = x.sizeid,
                                        brandid = x.brandid,
                                        productid = x.productid,
                                        serialno = x.gln
                                    }
                                };

                                var taxinfo = taxlist.Find(y => y.id == x.taxid);
                                var taxpercentage = taxinfo.taxsplit.Find(x.mrp).taxpercent;
                                var tax = Math.Round((x.mrp / (100 + taxpercentage) * taxpercentage), 2);
                                invoiceitems.taxid = x.taxid;
                                invoiceitems.salestax = tax;
                                invoiceitems.sellingprice = x.mrp;

                                var glnitem = glninfo.Find(y => y.gln == x.gln);
                                if (glnitem.CustomFields != null &&
                                glnitem.CustomFields.Count > 0)
                                {
                                    invoiceitems.CustomFields.AddRange(glnitem.CustomFields.ConvertAll(z => new LrinvoiceItemDetail.CustomFieldValue()
                                    {
                                        key = z.key,
                                        value = z.value
                                    }));
                                }

                                decimal tqty = x.utolerance > 0 ? Math.Round((x.oqty * x.utolerance / 100), MidpointRounding.AwayFromZero) : 0;
                                invoiceitems.poinfo.Add(new POItemInfo()
                                {
                                    designid = x.designid,
                                    oqty = x.oqty,
                                    aqty = x.aqty,
                                    price = x.price,
                                    sizeid = x.sizeid,
                                    pono = oPurchaseorder.pono,
                                    purchaseorderitemsid = x.id,
                                    tqty = x.oqty + tqty
                                });
                                invoice.Lrinvoiceitems.Add(invoiceitems);
                            });

                            invoice.Lrinvoicetaxitems = new List<Lrinvoicetax>();
                            foreach (var taxgrp in invoice.Lrinvoiceitems.GroupBy(x => new { x.ptaxid, x.purchasetaxpercentage }))
                            {
                                invoice.Lrinvoicetaxitems.Add(new Lrinvoicetax()
                                {
                                    isactive = true,
                                    amounton = taxgrp.Sum(x => x.amount),
                                    taxid = taxgrp.Key.ptaxid,
                                    percentage = taxgrp.Key.purchasetaxpercentage,
                                    taxamount = taxgrp.Sum(x => x.purchasetax),
                                    netamount = taxgrp.Sum(x => x.amount + x.purchasetax)
                                });
                            }
                            lrinvoiceDataService.Insert(transaction, conn, invoice);
                            lrinvoiceDataService.ImportInvoiceItems(invoice, transaction, conn);
                            invoice.Lrinvoicetaxitems.ForEach(e => e.isinterstatesale = invoice.isinterstatesale);
                            foreach (var taxitem in invoice.Lrinvoicetaxitems)
                            {
                                taxitem.invoiceid = invoice.id;
                                taxitem.createdon = invoice.createdon;
                                taxitem.modifiedon = DateTimeNow;
                                taxitem.createdby = Context.UserID;
                                subLrinvoicetaxs.Insert(transaction, conn, taxitem);
                            }
                        }
                        else
                        {
                            var invoice = invoices[0];
                            invoice.invoicedate = item.invoicedate;
                            invoice.invoiceno = item.invoiceno;
                            invoice.noofbundle = item.noofbundels;
                            invoice.noofpieces = item.noofpieces;
                            lrinvoiceDataService.Update(transaction, conn, invoice);
                        }
                    }

                    if (oPurchaseorder.internalstatus < Purchaseorder.InternalStatus.GRN)
                    {
                        oPurchaseorder.internalstatus = Purchaseorder.InternalStatus.GRN;
                        oPurchaseorder.status = Purchaseorder.POStatus.PARTIALLYCOMPLETED;
                        UpdateStatus(transaction, conn, oPurchaseorder);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return oPurchaseorder;
        }

        internal bool ClonePO(Purchaseorder item)
        {
            var existingorders = Search(new PurchaseorderCreteria()
            {
                id = item.id
            });
            if (existingorders.Count != 1)
                throw new Exception("Can't find the purchase order in the system");

            NpgsqlTransaction transaction = null;
            try
            {
                var neworder = Newtonsoft.Json.JsonConvert.DeserializeObject<Purchaseorder>(Newtonsoft.Json.JsonConvert.SerializeObject(existingorders[0]));
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    NpgsqlCommand command = GetCommand("update purchaseorder set status=1,internalstatus=1,modifiedby=@modifiedby,modifiedon=@modifiedon,version=version+1000 where id=@id and isactive=true", conn, transaction);
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.ExecuteNonQuery();

                    foreach (var itm in neworder.Purchaseorderitems)
                    {
                        command = GetCommand("update Purchaseorderitems set cqty=cqty+@qty,modifiedby=@modifiedby,modifiedon=@modifiedon,version=version+1000 where id=@id and isactive=true", conn, transaction);
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = itm.id;
                        command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = itm.oqty - (itm.rqty + itm.bqty + itm.cqty);
                        command.ExecuteNonQuery();
                        itm.oqty = itm.oqty - (itm.rqty + itm.bqty + itm.cqty);
                        itm.rqty = 0;
                        itm.bqty = 0;
                        itm.cqty = 0;
                        itm.id = 0;
                        itm.purchaseorderid = 0;
                    }
                    neworder.Purchaseorderitems.RemoveAll(x => x.isactive == false || x.oqty <= 0);
                    neworder.id = 0;

                    string query = "select count(*) from purchaseorder where pouniquenumber=@pouniquenumber";
                    var param = new SortedList<string, object>();
                    param.Add("pouniquenumber", neworder.pouniquenumber);
                    int nooforders = 0;
                    using (var reader = ExecuteReader(query, param))
                    {
                        if (reader.Read())
                            nooforders = reader.GetInt32(0);
                    }
                    if (nooforders == 1)
                    {
                        neworder.pono = neworder.pono + "/1";
                    }
                    else
                    {
                        if (neworder.pono.LastIndexOf('/') >= 0)
                        {
                            neworder.pono = neworder.pono.Substring(0, neworder.pono.LastIndexOf('/')) + "/" + (nooforders + 1);
                        }
                        else
                        {
                            neworder.pono = neworder.pono + "/1";
                        }
                    }
                    Insert(transaction, conn, neworder);

                    command = GetCommand(@"update Purchaseorder set status=1,internalstatus=1,modifiedby=@modifiedby,
                    modifiedon=@modifiedon,version=1,actionon=@actionon,actionby=@actionby where id=@id and isactive=true", conn, transaction);
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("actionon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = neworder.actionon;
                    command.Parameters.Add("actionby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = neworder.actionby;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = neworder.id;
                    command.ExecuteNonQuery();

                    PurchaseorderitemsDataService subPurchaseorderitems = new PurchaseorderitemsDataService() { Context = this.Context };
                    subPurchaseorderitems.Context = this.Context;
                    foreach (var poitem in neworder.Purchaseorderitems)
                    {
                        poitem.purchaseorderid = neworder.id;
                        subPurchaseorderitems.Insert(transaction, conn, poitem);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                return false;
            }
            return true;
        }
    }

}
