using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class DiscountschemeDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, schemedate, schemeappliedon, schemeremovedon, schemeappliedby, reason, companyid, filter, discountmode, discountvalue, rounding, status, locationid from discountscheme where id=@id";

        const string SQL_SearchStock = @"select stock.id,stock.price,stock.discount,stock.printingname,stock.barcode,
                                        stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty) stock from stock 
                                        left outer join items on items.id = stock.itemid
                                        where (stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty)>0)
                                        and stock.isactive=true";

        const string SQL_SearchSalableStock = @"
                                        select salablegoods.id,salablegoods.price,salablegoods.discount,stock.printingname,salablegoods.barcode,
                                        salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) stock,salablegoods.stockid from salablegoods
                                        inner join stock on stock.id=salablegoods.stockid
                                        left outer join items on items.id = stock.itemid
                                        where (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)>0)
                                        and salablegoods.isactive=true";

        const string SQL_SearchItems = @"select stock.id,salablegoods.price,salablegoods.discount,salablegoods.additionaldiscount, stock.printingname,salablegoods.id salalblegoodsid,
                                        salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) salablestock,stock.barcode,
                                        stock.qty-(stock.transfered+stock.damagedqty+stock.journalqty+stock.returnqty) stockqty,
                                        (schemelist ->> 'amt') :: money as applieddiscount
                                        from discountscheme LEFT JOIN LATERAL
                                        json_array_elements (discountscheme.itemlist :: json) schemelist ON true
                                        inner join salablegoods ON (schemelist ->> 'salablegoodsid') :: BIGINT = salablegoods.id
                                        left outer join stock on salablegoods.stockid = stock.id
                                        left outer join items on items.id = stock.itemid
                                        where 
                                        discountscheme.id = @discountschemeid and
                                        (salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty)>0 or 
                                        stock.qty-(stock.transfered+stock.damagedqty+journalqty+stock.returnqty)>0)
                                        and stock.isactive=true";
        

        const string SQL_Insert = @"INSERT INTO public.discountscheme(isactive, createdby, modifiedby, version, createdon, modifiedon, schemedate, schemeappliedon, schemeremovedon, schemeappliedby, reason, companyid, filter, discountmode, discountvalue, rounding, status, itemlist, locationid) VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @schemedate, @schemeappliedon, @schemeremovedon, @schemeappliedby, @reason, @companyid, @filter, @discountmode, @discountvalue, @rounding, @status, @itemlist, @locationid) RETURNING id";

        const string SQL_UpdateStock = @"update stock set discount=@discount,version=version+1 where id=@id";
        const string SQL_UpdateSalablegoods = @"update salablegoods set discount=@discount,version=version+1 where id=@id";

        const string SQL_Update = @"UPDATE public.discountscheme SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, schemedate=@schemedate, schemeappliedon=@schemeappliedon, schemeremovedon=@schemeremovedon, schemeappliedby=@schemeappliedby, reason=@reason, companyid=@companyid, filter=@filter, discountmode=@discountmode, discountvalue=@discountvalue, rounding=@rounding, status=@status, itemlist=@itemlist, locationid=@locationid where id=@id and version=@version RETURNING version";

        const string SQL_UpdateItems = @"UPDATE public.discountscheme SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon,itemlist=@itemlist,status=@status where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.discountscheme set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Discountscheme> Search(DiscountschemeCreteria _discountscheme)
        {
            List<Discountscheme> lst = new List<Discountscheme>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_discountscheme.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _discountscheme.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_discountscheme.companyid > 0)
                        qb.AddParameter("companyid", _discountscheme.companyid, "=");

                    if (_discountscheme.fromdate > DateTime.MinValue)
                        qb.AddParameter("schemedate", "fromdate", _discountscheme.fromdate.Date, ">");

                    if (_discountscheme.todate > DateTime.MinValue)
                        qb.AddParameter("schemedate", "fromdate", _discountscheme.todate.AddDays(1).Date, "<");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Discountscheme odiscountscheme = new Discountscheme();
                        odiscountscheme.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        odiscountscheme.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        odiscountscheme.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        odiscountscheme.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        odiscountscheme.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        odiscountscheme.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        odiscountscheme.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        odiscountscheme.schemedate = reader["schemedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["schemedate"]);
                        odiscountscheme.schemeappliedon = reader["schemeappliedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["schemeappliedon"]);
                        odiscountscheme.schemeremovedon = reader["schemeremovedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["schemeremovedon"]);
                        odiscountscheme.schemeappliedby = reader["schemeappliedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["schemeappliedby"]);
                        odiscountscheme.reason = reader["reason"] == DBNull.Value ? "" : reader["reason"].ToString();
                        odiscountscheme.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        odiscountscheme.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        odiscountscheme.filter_json = reader["filter"] == DBNull.Value ? "null" : reader["filter"].ToString();
                        odiscountscheme.discountmode = reader["discountmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["discountmode"]);
                        odiscountscheme.discountvalue = reader["discountvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountvalue"]);
                        odiscountscheme.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        odiscountscheme.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        lst.Add(odiscountscheme);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _discountscheme);
            }
            return lst;
        }

        public List<Discountscheme.DiscountItem> SearchItems(long id)
        {
            List<Discountscheme.DiscountItem> lst = new List<Discountscheme.DiscountItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchItems, conn);
                command.Parameters.Add("discountschemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Discountscheme.DiscountItem odiscountscheme = new Discountscheme.DiscountItem();
                        odiscountscheme.stockid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        odiscountscheme.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        odiscountscheme.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        odiscountscheme.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"]);
                        odiscountscheme.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        odiscountscheme.salalblegoodsid = reader["salalblegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salalblegoodsid"]);
                        odiscountscheme.salablestock = reader["salablestock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salablestock"]);
                        odiscountscheme.stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        odiscountscheme.applieddiscount = reader["applieddiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["applieddiscount"]);
                        odiscountscheme.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        odiscountscheme.status = 1;
                        lst.Add(odiscountscheme);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, id);
            }
            return lst;
        }

        public List<Discountscheme.DiscountItem> SearchItems(Discountscheme.FilterCreteria request)
        {
            if (request.discountschemeid > 0)
            {
                return SearchItems(request.discountschemeid);
            }
            var stocklist = SearchStockItems(request);
            var sellableist = SearchSellableItems(request);
            List<Discountscheme.DiscountItem> retlist = new List<Discountscheme.DiscountItem>();
            retlist.AddRange(stocklist);
            foreach(var item in sellableist)
            {
                if(retlist.Exists(e=>e.stockid == item.stockid))
                {
                    var ditem = retlist.Find(e => e.stockid == item.stockid);
                    ditem.salablestock = item.salablestock;
                }
                else
                {
                    retlist.Add(item);
                }
            }
            return retlist;
        }
        public List<Discountscheme.DiscountItem> SearchStockItems(Discountscheme.FilterCreteria request)
        {
            List<Discountscheme.DiscountItem> lst = new List<Discountscheme.DiscountItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchStock;

                if (!string.IsNullOrEmpty(request.barcode))
                    qb.AddParameter("stock.barcode", "barcode", request.barcode, "=");

                if (request.productid > 0)
                    qb.AddParameter("items.productid", "productid", request.productid, "=");
                if (request.brandid > 0)
                    qb.AddParameter("items.brandid", "brandid", request.brandid, "=");
                if (request.colourid > 0)
                    qb.AddParameter("items.colourid", "colourid", request.colourid, "=");
                if (request.materialid > 0)
                    qb.AddParameter("items.materialid", "materialid", request.materialid, "=");
                if (request.patternid > 0)
                    qb.AddParameter("items.patternid", "patternid", request.patternid, "=");
                if (request.typeid > 0)
                    qb.AddParameter("items.typeid", "typeid", request.typeid, "=");
                if (request.sizeid > 0)
                    qb.AddParameter("items.sizeid", "sizeid", request.sizeid, "=");
                if (request.styleid > 0)
                    qb.AddParameter("items.styleid", "styleid", request.styleid, "=");
                if (request.supplierid > 0)
                    qb.AddParameter("stock.supplierid", "supplierid", request.supplierid, "=");
                qb.Limit = 500;
                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //salablegoods.id,salablegoods.price,salablegoods.discount,stock.printingname,salablegoods.barcode,salablegoods.iqty - (salablegoods.oqty + salablegoods.dqty) stock
                        Discountscheme.DiscountItem odiscountscheme = new Discountscheme.DiscountItem();
                        odiscountscheme.stockid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        odiscountscheme.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        odiscountscheme.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        odiscountscheme.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        odiscountscheme.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        odiscountscheme.stockqty = reader["stock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stock"]);
                        odiscountscheme.status = 0;
                        lst.Add(odiscountscheme);
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

        public List<Discountscheme.DiscountItem> SearchSellableItems(Discountscheme.FilterCreteria request)
        {
            List<Discountscheme.DiscountItem> lst = new List<Discountscheme.DiscountItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchSalableStock;

                if (!string.IsNullOrEmpty(request.barcode))
                    qb.AddParameter("stock.barcode", "barcode", request.barcode, "=");
                if(request.locationid>0)
                    qb.AddParameter("salablegoods.locationid", "locationid", request.locationid, "=");
                if (request.productid > 0)
                    qb.AddParameter("items.productid", "productid", request.productid, "=");
                if (request.brandid > 0)
                    qb.AddParameter("items.brandid", "brandid", request.brandid, "=");
                if (request.colourid > 0)
                    qb.AddParameter("items.colourid", "colourid", request.colourid, "=");
                if (request.materialid > 0)
                    qb.AddParameter("items.materialid", "materialid", request.materialid, "=");
                if (request.patternid > 0)
                    qb.AddParameter("items.patternid", "patternid", request.patternid, "=");
                if (request.typeid > 0)
                    qb.AddParameter("items.typeid", "typeid", request.typeid, "=");
                if (request.sizeid > 0)
                    qb.AddParameter("items.sizeid", "sizeid", request.sizeid, "=");
                if (request.styleid > 0)
                    qb.AddParameter("items.styleid", "styleid", request.styleid, "=");
                if (request.supplierid > 0)
                    qb.AddParameter("stock.supplierid", "supplierid", request.supplierid, "=");
                qb.Limit = 500;
                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                       
                        Discountscheme.DiscountItem odiscountscheme = new Discountscheme.DiscountItem();
                        odiscountscheme.salalblegoodsid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        odiscountscheme.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        odiscountscheme.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        odiscountscheme.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        odiscountscheme.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        odiscountscheme.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        odiscountscheme.salablestock = reader["stock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stock"]);
                        lst.Add(odiscountscheme);
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

        public DiscountschemeCover Insert(DiscountschemeCover oDiscountscheme)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    if (oDiscountscheme.Items != null && oDiscountscheme.Items.Count > 0)
                    {
                        List<Discountscheme.DiscountValue> itemlist = new List<Discountscheme.DiscountValue>();
                        foreach (var item in oDiscountscheme.Items)
                        {
                            Discountscheme.DiscountValue dv = new Discountscheme.DiscountValue();
                            dv.id = item.stockid;
                            dv.salablegoodsid = item.salalblegoodsid;
                            itemlist.Add(dv);
                        }
                        oDiscountscheme.Scheme.itemlist_json = Newtonsoft.Json.JsonConvert.SerializeObject(itemlist);
                    }
                    Insert(transaction, conn, oDiscountscheme.Scheme);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDiscountscheme);


            }
            oDiscountscheme.Items = null;
            oDiscountscheme.Scheme.itemlist_json = null;
            return oDiscountscheme;
        }

        public Discountscheme Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Discountscheme oDiscountscheme)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDiscountscheme.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDiscountscheme.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("schemedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDiscountscheme.schemedate;
            command.Parameters.Add("schemeappliedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDiscountscheme.schemeappliedon;
            command.Parameters.Add("schemeremovedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDiscountscheme.schemeremovedon;
            command.Parameters.Add("schemeappliedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountscheme.schemeappliedby;
            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDiscountscheme.reason == null ? "" : oDiscountscheme.reason;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountscheme.companyid;
            command.Parameters.Add("filter", NpgsqlTypes.NpgsqlDbType.Json).Value = oDiscountscheme.filter_json == null ? "null" : oDiscountscheme.filter_json;
            command.Parameters.Add("discountmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDiscountscheme.discountmode;
            command.Parameters.Add("discountvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDiscountscheme.discountvalue;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDiscountscheme.rounding;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDiscountscheme.status;
            command.Parameters.Add("itemlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oDiscountscheme.itemlist_json == null ? "null" : oDiscountscheme.itemlist_json;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountscheme.locationid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oDiscountscheme.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oDiscountscheme;
        }

        public DiscountschemeCover Update(DiscountschemeCover oDiscountscheme)
        {
            var items = Search(new DiscountschemeCreteria() { id = oDiscountscheme.Scheme.id });
            if (items != null && items.Count > 0 && items[0].status == 1)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("Edit not allowed once the discount applied.", 
                    WebAPI.Common.Logger.LogType.Error, oDiscountscheme);
                return oDiscountscheme;
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    if (oDiscountscheme.Items != null && oDiscountscheme.Items.Count > 0)
                    {
                        List<Discountscheme.DiscountValue> itemlist = new List<Discountscheme.DiscountValue>();
                        foreach (var item in oDiscountscheme.Items)
                        {
                            Discountscheme.DiscountValue dv = new Discountscheme.DiscountValue();
                            dv.id = item.stockid;
                            dv.salablegoodsid = item.salalblegoodsid;
                            itemlist.Add(dv);
                        }
                        oDiscountscheme.Scheme.itemlist_json = Newtonsoft.Json.JsonConvert.SerializeObject(itemlist);
                    }

                    Update(transaction, conn, oDiscountscheme.Scheme);
                    //if (oDiscountscheme.Items != null && oDiscountscheme.Items.Count > 0)
                    //{
                    //    List<Discountscheme.DiscountItem> removeditems = new List<Discountscheme.DiscountItem>();
                    //    List<Discountscheme.DiscountItem> addeditems = new List<Discountscheme.DiscountItem>();

                    //    var items = SearchItems(oDiscountscheme.Scheme.id);
                    //    foreach (var item in oDiscountscheme.Items)
                    //    {
                    //        if (item.status == 2 && items.Exists(e => e.stockid == item.stockid))
                    //        {
                    //            var ritem = items.Find(e => e.stockid == item.stockid);
                    //            ritem.discount -= ritem.applieddiscount;
                    //            addeditems.Add(ritem);
                    //            items.RemoveAll(e => e.stockid == item.stockid);
                    //        }
                    //        else
                    //        {
                    //            if (oDiscountscheme.Scheme.status == 1)
                    //            {
                    //                decimal discount;
                    //                if (oDiscountscheme.Scheme.discountmode == 1)
                    //                {
                    //                    discount = oDiscountscheme.Scheme.discountvalue;
                    //                }
                    //                else if (oDiscountscheme.Scheme.discountmode == 2)
                    //                {
                    //                    discount = Round(item.price * oDiscountscheme.Scheme.discountvalue / 100, oDiscountscheme.Scheme.rounding);
                    //                }
                    //                else
                    //                {
                    //                    discount = Round(item.price * oDiscountscheme.Scheme.discountvalue / 100, oDiscountscheme.Scheme.rounding);
                    //                    if (item.discount < discount)
                    //                        discount -= item.discount;
                    //                }
                    //                item.applieddiscount = discount;
                    //                item.discount += discount;
                    //            }
                    //            addeditems.Add(item);
                    //            items.Add(item);
                    //        }
                    //    }

                    //    if (addeditems.Count > 0)
                    //    {
                    //        NpgsqlCommand command = GetCommand(SQL_UpdateStock, conn, transaction);
                    //        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    //        command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    //        foreach (var item in addeditems)
                    //        {
                    //            command.Parameters["id"].Value = item.stockid;
                    //            command.Parameters["discount"].Value = item.discount;
                    //            command.ExecuteNonQuery();
                    //        }
                    //    }

                    //    List<Discountscheme.DiscountValue> itemlist = new List<Discountscheme.DiscountValue>();
                    //    foreach (var item in items)
                    //    {
                    //        Discountscheme.DiscountValue dv = new Discountscheme.DiscountValue();
                    //        dv.id = item.stockid;
                    //        dv.amt = item.applieddiscount;
                    //        itemlist.Add(dv);
                    //    }
                    //    oDiscountscheme.Scheme.itemlist_json = Newtonsoft.Json.JsonConvert.SerializeObject(itemlist);
                    //    UpdateItems(transaction, conn, oDiscountscheme.Scheme);
                    //    oDiscountscheme.Scheme.itemlist_json = "";
                    //}

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDiscountscheme);


            }
            return oDiscountscheme;
        }

        private decimal Round(decimal value, decimal roundingnear)
        {
            return value;
        }

        public Discountscheme UpdateStatus(Discountscheme oDiscountscheme)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                var list = Search(new DiscountschemeCreteria()
                {
                    id = oDiscountscheme.id
                });
                if (list.Count == 1)
                {
                    list[0].status = oDiscountscheme.status;
                    oDiscountscheme = list[0];
                    var items = SearchItems(oDiscountscheme.id);
                    if (items.Count > 0 && oDiscountscheme.discountvalue > 0)
                    {
                        List<Discountscheme.DiscountValue> discountitemlist = new List<Discountscheme.DiscountValue>();
                        List<Discountscheme.DiscountItem> priceChangeList = new List<Discountscheme.DiscountItem>();
                        if (oDiscountscheme.status == 1)
                        {
                            foreach (var item in items)
                            {
                                if (!discountitemlist.Exists(e => e.id == item.stockid))
                                {
                                    priceChangeList.Add(item);
                                    decimal discount = 0;
                                    if (oDiscountscheme.discountmode == 1)
                                    {
                                        discount = oDiscountscheme.discountvalue;
                                    }
                                    else if (oDiscountscheme.discountmode == 2)
                                    {
                                        discount = Round(item.price * oDiscountscheme.discountvalue / 100, oDiscountscheme.rounding);
                                    }
                                    else if (oDiscountscheme.discountmode == 3)
                                    {
                                        discount = Round(item.price * oDiscountscheme.discountvalue / 100, oDiscountscheme.rounding);
                                        if (item.discount < discount)
                                            discount -= item.discount;
                                    }
                                    item.discount += discount;
                                    discountitemlist.Add(new Discountscheme.DiscountValue()
                                    {
                                        id = item.stockid,
                                        salablegoodsid = item.salalblegoodsid,
                                        amt = discount
                                    });
                                }
                            }
                        }
                        else
                        {
                            foreach (var item in items)
                            {
                                if (!discountitemlist.Exists(e => e.id == item.stockid))
                                {
                                    priceChangeList.Add(item);
                                    item.discount -= item.applieddiscount;
                                    discountitemlist.Add(new Discountscheme.DiscountValue()
                                    {
                                        id = item.stockid,
                                        salablegoodsid = item.salalblegoodsid,
                                        amt = 0
                                    });
                                }
                            }
                        }

                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();
                            //NpgsqlCommand command = GetCommand(SQL_UpdateStock, conn, transaction);
                            //command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                            //command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;

                            NpgsqlCommand commandsalablegoods = GetCommand(SQL_UpdateSalablegoods, conn, transaction);
                            commandsalablegoods.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                            commandsalablegoods.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;

                            foreach (var item in priceChangeList)
                            {
                                //command.Parameters["id"].Value = item.stockid;
                                //command.Parameters["discount"].Value = item.discount;
                                //command.ExecuteNonQuery();

                                commandsalablegoods.Parameters["id"].Value = item.salalblegoodsid;
                                commandsalablegoods.Parameters["discount"].Value = item.discount;
                                commandsalablegoods.ExecuteNonQuery();
                            }
                            oDiscountscheme.itemlist_json = Newtonsoft.Json.JsonConvert.SerializeObject(discountitemlist);
                            UpdateItems(transaction, conn, oDiscountscheme);
                            transaction.Commit();
                            conn.Close();
                            oDiscountscheme.itemlist_json = "";
                        }

                    }
                }
                return oDiscountscheme;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDiscountscheme);


            }
            return oDiscountscheme;
        }

        public Discountscheme UpdateItems(NpgsqlTransaction transaction, NpgsqlConnection conn, Discountscheme oDiscountscheme)
        {
            NpgsqlCommand command = GetCommand(SQL_UpdateItems, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountscheme.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDiscountscheme.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDiscountscheme.version;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("itemlist", NpgsqlTypes.NpgsqlDbType.Jsonb).Value = oDiscountscheme.itemlist_json;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDiscountscheme.status;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oDiscountscheme.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oDiscountscheme;
        }

        public Discountscheme Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Discountscheme oDiscountscheme)
        {
            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountscheme.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDiscountscheme.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountscheme.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDiscountscheme.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDiscountscheme.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("schemedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDiscountscheme.schemedate;
            command.Parameters.Add("schemeappliedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDiscountscheme.schemeappliedon;
            command.Parameters.Add("schemeremovedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oDiscountscheme.schemeremovedon;
            command.Parameters.Add("schemeappliedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountscheme.schemeappliedby;
            command.Parameters.Add("reason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oDiscountscheme.reason == null ? "" : oDiscountscheme.reason;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountscheme.companyid;
            command.Parameters.Add("filter", NpgsqlTypes.NpgsqlDbType.Json).Value = oDiscountscheme.filter_json == null ? "null" : oDiscountscheme.filter_json;
            command.Parameters.Add("discountmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDiscountscheme.discountmode;
            command.Parameters.Add("discountvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDiscountscheme.discountvalue;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oDiscountscheme.rounding;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDiscountscheme.status;
            command.Parameters.Add("itemlist", NpgsqlTypes.NpgsqlDbType.Json).Value = oDiscountscheme.itemlist_json == null ? "null" : oDiscountscheme.itemlist_json;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountscheme.locationid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oDiscountscheme.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oDiscountscheme;
        }


        public bool Delete(Discountscheme oDiscountscheme)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oDiscountscheme);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oDiscountscheme);



            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Discountscheme oDiscountscheme)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oDiscountscheme.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oDiscountscheme.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oDiscountscheme.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
