using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class ProductrequestformDataService : SAASData
    {
        const string SQL_Search = @"select id, requestedby, requestedon, prfno, reasonid, priorityid, companyid, isactive, createdby, modifiedby, version, createdon, modifiedon, locationid from productrequestform where id=@id";

        const string SQL_SearchPO = @"select sum(qty-(transfered+journalqty+damagedqty)) as warehouse from stock where itemid=@itemid;
                                        select sum(salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) as showroom from salablegoods where itemid=@itemid;
                                        select sum(qty-(rqty+cqty)) as ordered from productrequestformitems where itemid=@itemid;
                                        select sum(qty-(rqty+cqty)) as today from productrequestformitems
                                        inner join productrequestform on productrequestform.id = productrequestformid
                                        where itemid=@itemid and requestedon>=@requestedonfrom and requestedon<@requestedonto;
                                        select sum(buyingqty-(sellableqty+damagedqty)) as invoice from lrinvoiceitems where itemid=@itemid and lrinvoiceitems.isactive=true";

        const string SQL_Insert = @"INSERT INTO public.productrequestform(requestedby, requestedon, prfno, reasonid, priorityid, companyid, isactive, createdby, modifiedby, version, createdon, modifiedon, locationid) VALUES(@requestedby, @requestedon, @prfno, @reasonid, @priorityid, @companyid, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @locationid) RETURNING id";

        const string SQL_Update = @"UPDATE public.productrequestform SET requestedby=@requestedby, requestedon=@requestedon, prfno=@prfno, reasonid=@reasonid, priorityid=@priorityid, companyid=@companyid, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, locationid=@locationid where id=@id and version=@version RETURNING version";

        const string SQL_SearchOrders = @"select productrequestformitems.*, items.sizeid,
        requestedon,requestedby,products.name as productname,items.printingname,brandname,colourname,patternname,sizename,stylename,materialname,typename from productrequestformitems
        inner join productrequestform on productrequestform.id = productrequestformitems.productrequestformid
        left outer join items on items.id = productrequestformitems.itemid
        inner join products on products.id = productrequestformitems.productid
        and productrequestform.isactive=true and productrequestformitems.isactive=true ";

        internal List<ProductrequestformitemsDetail> SearchOrders(PRFRequest item)
        {
            List<ProductrequestformitemsDetail> lst = new List<ProductrequestformitemsDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchOrders;
                if (item.id > 0)
                {
                    qb.AddParameter("productrequestform.id", item.id, "=");
                }
                else
                {
                    if (item.supplierid > 0)
                    {
                        qb.AddParameter("productrequestformitems.supplierid", item.supplierid, "=");
                    }
                    if (item.requestedby > 0)
                    {
                        qb.AddParameter("productrequestform.requestedby", item.requestedby, "=");
                    }
                    if (item.priorityid > 0)
                    {
                        qb.AddParameter("productrequestform.priorityid", item.priorityid, "=");
                    }
                    if (item.locationid > 0)
                    {
                        qb.AddParameter("productrequestform.locationid", item.locationid, "=");
                    }
                    if (item.reasonid > 0)
                    {
                        qb.AddParameter("productrequestform.reasonid", item.reasonid, "=");
                    }
                    if (item.fromdate != DateTime.MinValue && item.todate != DateTime.MinValue)
                    {
                        qb.AddParameter("productrequestform.requestedon", "requestedfrom", item.fromdate.Date, ">=");
                        qb.AddParameter("productrequestform.requestedon", "requestedto", item.todate.Date.AddDays(1), "<");
                    }

                    if (item.brandid > 0)
                    {
                        qb.AddParameter("items.brandid", item.brandid, "=");
                    }
                    if (item.productid > 0)
                    {
                        qb.AddParameter("productrequestformitems.productid", item.productid, "=");
                    }
                    if (item.materialid > 0)
                    {
                        qb.AddParameter("items.materialid", item.materialid, "=");
                    }
                    if (item.patternid > 0)
                    {
                        qb.AddParameter("items.patternid", item.patternid, "=");
                    }
                    if (item.sizeid > 0)
                    {
                        qb.AddParameter("items.sizeid", item.sizeid, "=");
                    }
                    if (item.styleid > 0)
                    {
                        qb.AddParameter("items.styleid", item.styleid, "=");
                    }
                    if (item.colourid > 0)
                    {
                        qb.AddParameter("items.colourid", item.colourid, "=");
                    }
                    if (item.cancelled)
                    {

                        qb.CurrentQuery += " and productrequestformitems.cqty > 0";
                    }
                    else
                    {
                        qb.CurrentQuery += " and productrequestformitems.qty > (productrequestformitems.cqty + productrequestformitems.rqty)";
                    }
                }
                qb.SortField = "productrequestform.createdon desc";
                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ProductrequestformitemsDetail oproductrequestformitems = new ProductrequestformitemsDetail();
                        oproductrequestformitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oproductrequestformitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        oproductrequestformitems.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        oproductrequestformitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        oproductrequestformitems.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cqty"]);
                        oproductrequestformitems.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rqty"]);
                        oproductrequestformitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oproductrequestformitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oproductrequestformitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oproductrequestformitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oproductrequestformitems.productrequestformid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oproductrequestformitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oproductrequestformitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oproductrequestformitems.requestedon = reader["requestedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["requestedon"]);
                        oproductrequestformitems.requestedby = reader["requestedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["requestedby"]);
                        oproductrequestformitems.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        oproductrequestformitems.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        oproductrequestformitems.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        oproductrequestformitems.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        oproductrequestformitems.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        oproductrequestformitems.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        oproductrequestformitems.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        oproductrequestformitems.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        oproductrequestformitems.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        oproductrequestformitems.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        oproductrequestformitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        oproductrequestformitems.pricerange = reader["pricerange"] == DBNull.Value ? "" : reader["pricerange"].ToString();
                        oproductrequestformitems.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        oproductrequestformitems.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        lst.Add(oproductrequestformitems);
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

        const string SQL_Delete = @"update public.productrequestform set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Productrequestform> Search(ProductrequestformCreteria _productrequestform)
        {
            List<Productrequestform> lst = new List<Productrequestform>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_productrequestform.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _productrequestform.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_productrequestform.Code))
                    //    qb.AddParameter("code", _productrequestform.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Productrequestform oproductrequestform = new Productrequestform();
                        oproductrequestform.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oproductrequestform.requestedby = reader["requestedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["requestedby"]);
                        oproductrequestform.requestedon = reader["requestedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["requestedon"]);
                        oproductrequestform.prfno = reader["prfno"] == DBNull.Value ? "" : reader["prfno"].ToString();
                        oproductrequestform.reasonid = reader["reasonid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["reasonid"]);
                        oproductrequestform.priorityid = reader["priorityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["priorityid"]);
                        oproductrequestform.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oproductrequestform.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oproductrequestform.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oproductrequestform.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oproductrequestform.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oproductrequestform.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oproductrequestform.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oproductrequestform.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(oproductrequestform);
                    }
                }
                if (_productrequestform.id > 0 && lst.Count == 1)
                {
                    ProductrequestformitemsDataService subProductrequestformitems = new ProductrequestformitemsDataService() { Context = this.Context };
                    subProductrequestformitems.Context = this.Context;
                    lst[0].Productrequestformitems = subProductrequestformitems.SearchAll(_productrequestform.id);
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _productrequestform);
            }
            return lst;
        }

        public POResult SearchPO(POSearch _productrequestform)
        {
            List<long> ids = new List<long>();

            ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
            var items = itemsDataService.Search(new ItemsCreteria()
            {
                productid = _productrequestform.productid,
                colourid = _productrequestform.colourid,
                sizeid = _productrequestform.sizeid,
                styleid = _productrequestform.styleid,
                patternid = _productrequestform.patternid,
                materialid = _productrequestform.materialid,
                brandid = _productrequestform.brandid,
                glndesignid = _productrequestform.glndesignid,
                typeid = _productrequestform.typeid,
                productmap = false
            });
            ids = items.ConvertAll(x => x.id);

            

            POResult result = new POResult()
            {
                itemid = _productrequestform.id
            };
            if (ids.Count == 0) return result;


            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                _productrequestform.id = (ids.Count == 1) ? ids.First() : 0;
                NpgsqlCommand command = null;
                command = GetCommand(SQL_SearchPO, conn);
                if (_productrequestform.id > 0)
                {
                    command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _productrequestform.id;
                }
                else
                {
                    command = GetCommand(SQL_SearchPO.Replace(@"itemid=@itemid", "itemid in (" + String.Join(",", ids) + ")"), conn);
                }
                command.Parameters.Add("requestedonfrom", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date;
                command.Parameters.Add("requestedonto", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTimeNow.Date.AddDays(1);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        result.warehouse = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                    if (reader.NextResult() && reader.Read())
                    {
                        result.showroom = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                    if (reader.NextResult() && reader.Read())
                    {
                        result.ordered = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                    if (reader.NextResult() && reader.Read())
                    {
                        result.today = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                    if (reader.NextResult() && reader.Read())
                    {
                        result.invoice = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _productrequestform);
            }
            return result;
        }

        public Productrequestform Insert(Productrequestform oProductrequestform)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("productrequestform.prfno", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    oProductrequestform.prfno = sequence.GetNextNumber(Context, conn, transaction);

                    Insert(transaction, conn, oProductrequestform);
                    ProductrequestformitemsDataService subProductrequestformitems = new ProductrequestformitemsDataService() { Context = this.Context };
                    subProductrequestformitems.Context = this.Context;
                    foreach (var item in oProductrequestform.Productrequestformitems)
                    {
                        item.productrequestformid = oProductrequestform.id;
                        if (item.itemid == 0 && item.detail != null && item.detail.jumplist != null && item.detail.jumplist.Count >0)
                        {
                            Data.ItemsDataService itemdata = new ItemsDataService() { Context = this.Context };

                            foreach (var sitem in item.detail.jumplist)
                            {
                                var fitem = itemdata.Search(new ItemsCreteria()
                                {
                                    productid = item.productid,
                                    brandid = item.detail.brandid,
                                    materialid = item.detail.materialid,
                                    styleid = item.detail.styleid,
                                    patternid = item.detail.patternid,
                                    sizeid = sitem.id,
                                    typeid = item.detail.typeid,
                                    colourid = item.detail.colourid,
                                    productmap = true
                                });
                                if (fitem == null || fitem.Count == 0)
                                {
                                    var nitem = new Items()
                                    {
                                        productid = item.productid,
                                        brandid = item.detail.brandid,
                                        materialid = item.detail.materialid,
                                        styleid = item.detail.styleid,
                                        patternid = item.detail.patternid,
                                        typeid = item.detail.typeid,
                                        sizeid = sitem.id,
                                        colourid = item.detail.colourid
                                    };
                                    itemdata.Insert(nitem);
                                    item.itemid = nitem.id;
                                }
                                else
                                {
                                    item.itemid = fitem[0].id;
                                }
                                if (item.itemid > 0)
                                {
                                    item.qty = sitem.qty;
                                    subProductrequestformitems.Insert(transaction, conn, item);
                                }
                            }
                        }
                        else
                        {
                            if(item.itemid == 0)
                            {
                                ItemsDataService itemsDataService = new ItemsDataService() { Context = this.Context };
                                var items = itemsDataService.Search(new ItemsCreteria()
                                {
                                    productid = item.productid,
                                    colourid = item.detail.colourid,
                                    sizeid = item.detail.sizeid,
                                    styleid = item.detail.styleid,
                                    patternid = item.detail.patternid,
                                    materialid = item.detail.materialid,
                                    brandid = item.detail.brandid,
                                    glndesignid = item.designid,
                                    typeid = item.detail.typeid,
                                    productmap = true
                                });
                                if (items.Count == 0)
                                    items.Add(itemsDataService.Insert(new Items()
                                    {
                                        productid = item.productid,
                                        colourid = item.detail.colourid,
                                        sizeid = item.detail.sizeid,
                                        styleid = item.detail.styleid,
                                        patternid = item.detail.patternid,
                                        materialid = item.detail.materialid,
                                        brandid = item.detail.brandid,
                                        glndesignid = item.designid,
                                        typeid = item.detail.typeid,
                                        isactive =true
                                    }));
                                item.itemid = items[0].id;
                            }
                            subProductrequestformitems.Insert(transaction, conn, item);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oProductrequestform);


            }

            return oProductrequestform;
        }

        public Productrequestform Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Productrequestform oProductrequestform)
        {
            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("requestedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestform.requestedby;
            command.Parameters.Add("requestedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oProductrequestform.requestedon;
            command.Parameters.Add("prfno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductrequestform.prfno == null ? "" : oProductrequestform.prfno;
            command.Parameters.Add("reasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestform.reasonid;
            command.Parameters.Add("priorityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestform.priorityid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestform.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestform.locationid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductrequestform.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProductrequestform.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oProductrequestform.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oProductrequestform;
        }

        public Productrequestform Update(Productrequestform oProductrequestform)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oProductrequestform);
                    ProductrequestformitemsDataService subProductrequestformitems = new ProductrequestformitemsDataService() { Context = this.Context };
                    subProductrequestformitems.Context = this.Context;
                    subProductrequestformitems.Update(transaction, conn, oProductrequestform.Productrequestformitems, oProductrequestform.id);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oProductrequestform);


            }

            return oProductrequestform;
        }

        public Productrequestform Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Productrequestform oProductrequestform)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestform.id;
            command.Parameters.Add("requestedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestform.requestedby;
            command.Parameters.Add("requestedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oProductrequestform.requestedon;
            command.Parameters.Add("prfno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oProductrequestform.prfno == null ? "" : oProductrequestform.prfno;
            command.Parameters.Add("reasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestform.reasonid;
            command.Parameters.Add("priorityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestform.priorityid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestform.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestform.locationid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductrequestform.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestform.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProductrequestform.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oProductrequestform.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oProductrequestform.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oProductrequestform;
        }


        public bool Delete(Productrequestform oProductrequestform)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oProductrequestform);
                    ProductrequestformitemsDataService subProductrequestformitems = new ProductrequestformitemsDataService() { Context = this.Context };
                    subProductrequestformitems.Context = this.Context;
                    subProductrequestformitems.DeleteAll(oProductrequestform.id, oProductrequestform.modifiedby, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oProductrequestform);



            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Productrequestform oProductrequestform)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oProductrequestform.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oProductrequestform.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oProductrequestform.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
