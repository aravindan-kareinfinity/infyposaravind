using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class StockmarkerDataService : SAASData
    {
        const string SQL_SearchWHStock = @"select stockmarker.isactive, stockmarker.createdby, stockmarker.modifiedby, stockmarker.version, stockmarker.id, stockmarker.createdon, 
                                    stockmarker.modifiedon, stockid, markerid, stockmarker.qty,stockmarker.locationid, salablegoodsid,stockmarker.barcode as barcode,
                                    stock.productid, stock.buyingprice buyingprice, stockmarker.isdamaged,stockqty,addedon,glncode,
					                stock.price, stock.discount, stock.qty pqty, stock.qty-(stock.transfered+stock.journalqty+stock.damagedqty+stock.returnqty) stock, 
					                brand.name brandname, products.id productid, products.salestaxid, products.name productname, stock.designid,0::numeric olqty,
					                stock.hsncode,supplier.name suppliername, style.name stylename,pattern.name patternname,lrinvoiceitems.purchasetaxpercentage,
                                    size.name sizename, colour.name colourname,type.name typename,material.name materialname,sleeve.name sleevename,fit.name fitname
					                from stockmarker 
					                inner join stock on stock.id=stockmarker.stockid
                                    left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true 
                                    left outer join lrinvoice on lrinvoice.id=lrinvoiceitems.lrinvoiceid and lrinvoice.isactive=true
                                    left outer join lrentry on lrentry.id = lrinvoice.lrid
                                    left outer join items on items.id = stock.itemid
                                    left outer join products on products.id = stock.productid
                                    left outer join supplier on supplier.id = lrentry.supplierid
                                    left outer join brand brand on brand.id = items.brandid
                                    left outer join referencelist colour on colour.id = colourid
                                    left outer join referencelist size on size.id = sizeid
                                    left outer join referencelist style on style.id = styleid
                                    left outer join referencelist pattern on pattern.id = patternid
                                    left outer join referencelist material on material.id = materialid
                                    left outer join referencelist sleeve on sleeve.id = sleeveid
                                    left outer join referencelist fit on fit.id = fitid
                                    left outer join referencelist type on type.id = typeid
                                    where id=@id";


        const string SQL_SearchRetailStock = @"select stockmarker.isactive, stockmarker.createdby, stockmarker.modifiedby, stockmarker.version, stockmarker.id, stockmarker.createdon, 
                                    stockmarker.modifiedon, stockmarker.stockid, stockmarker.markerid, stockmarker.qty, stockmarker.locationid, salablegoodsid,stockmarker.barcode as barcode,
                                    stock.productid, stock.buyingprice buyingprice,stockmarker.isdamaged,stockqty,addedon,glncode,
					                salablegoods.price, salablegoods.discount, stock.qty pqty, salablegoods.iqty-(salablegoods.oqty+salablegoods.jqty+salablegoods.dqty) stock, 
					                brand.name brandname, products.id productid, products.salestaxid, products.name productname, stock.designid, salablegoods.olqty,
					                stock.hsncode,supplier.name suppliername, style.name stylename,pattern.name patternname,lrinvoiceitems.purchasetaxpercentage,
                                    size.name sizename, colour.name colourname,type.name typename,material.name materialname,sleeve.name sleevename,fit.name fitname
                                    from stockmarker inner join salablegoods on salablegoods.id = stockmarker.salablegoodsid
                                    inner join stock on stock.id = salablegoods.stockid 
                                    left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid and lrinvoiceitems.isactive = true 
                                    left outer join lrinvoice on lrinvoice.id=lrinvoiceitems.lrinvoiceid and lrinvoice.isactive=true
                                    left outer join lrentry on lrentry.id = lrinvoice.lrid
                                    left outer join items on items.id = stock.itemid
                                    left outer join products on products.id = stock.productid
                                    left outer join supplier on supplier.id = lrentry.supplierid
                                    left outer join brand brand on brand.id = items.brandid
                                    left outer join referencelist colour on colour.id = colourid
                                    left outer join referencelist size on size.id = sizeid
                                    left outer join referencelist style on style.id = styleid
                                    left outer join referencelist pattern on pattern.id = patternid
                                    left outer join referencelist material on material.id = materialid
                                    left outer join referencelist sleeve on sleeve.id = sleeveid
                                    left outer join referencelist fit on fit.id = fitid
                                    left outer join referencelist type on type.id = typeid
                                    where salablegoods.isactive=true";

        const string SQL_Insert = @"INSERT INTO public.stockmarker(isactive, createdby, modifiedby, version, createdon, modifiedon, stockid, markerid, qty, locationid, salablegoodsid,barcode,isdamaged,stockqty,addedon,glncode)
                                    VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @stockid, @markerid, @qty, @locationid, @salablegoodsid,@barcode,@isdamaged,@stockqty,@addedon,@glncode)"; /*WHERE NOT EXISTS (SELECT 1 FROM stockmarker WHERE barcode=@barcode and locationid=@locationid and salablegoodsid=@salablegoodsid and isactive=true)*/


        const string SQL_DeleteAll = @"update public.stockmarker set isactive=false,version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon  where markerid=@markerid";

        public List<Stockmarkerdetail> Search(StockmarkerCreteria _stockmarker)
        {
            List<Stockmarkerdetail> lst = new List<Stockmarkerdetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_stockmarker.id != 0)
                {
                    command = GetCommand(SQL_SearchWHStock, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _stockmarker.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    if (_stockmarker.mode == 1)
                        qb.CurrentQuery = SQL_SearchWHStock.Substring(0, SQL_SearchWHStock.IndexOf("where"));
                    else
                        qb.CurrentQuery = SQL_SearchRetailStock.Substring(0, SQL_SearchRetailStock.IndexOf("where"));
                    qb.AddParameter("stockmarker.isactive", true, "=");

                    if (string.IsNullOrEmpty(_stockmarker.barcode))
                    {
                        qb.AddParameter("stockmarker.markerid", _stockmarker.markerid, "=");
                    }
                    else
                    {
                        qb.AddParameter("stockmarker.barcode", _stockmarker.barcode, "=");
                        if (_stockmarker.markerids != null && _stockmarker.markerids.Count > 0)
                            qb.AddInParamer("stockmarker.markerid", string.Join(",", _stockmarker.markerids));
                    }
                    if (_stockmarker.mode == 1)
                        qb.AddParameter("stockmarker.locationid", 0, "=");
                    else if ((_stockmarker.mode == 2 || _stockmarker.mode == 3) && _stockmarker.locationid > 0)
                        qb.AddParameter("stockmarker.locationid", _stockmarker.locationid, "=");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Stockmarkerdetail ostockmarker = new Stockmarkerdetail();
                        ostockmarker.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ostockmarker.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ostockmarker.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ostockmarker.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ostockmarker.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ostockmarker.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ostockmarker.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ostockmarker.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                        ostockmarker.markerid = reader["markerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["markerid"]);
                        ostockmarker.stockqty = reader["stock"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stock"]);
                        ostockmarker.olqty = reader["olqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["olqty"]);
                        ostockmarker.stockqtyatmark = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]);
                        ostockmarker.addedon = reader["addedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["addedon"]);
                        ostockmarker.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        ostockmarker.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ostockmarker.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"]);
                        ostockmarker.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ostockmarker.glncode = reader["glncode"] == DBNull.Value ? "" : reader["glncode"].ToString();
                        ostockmarker.productname = reader["productname"] == DBNull.Value ? "" : reader["productname"].ToString();
                        ostockmarker.brandname = reader["brandname"] == DBNull.Value ? "" : reader["brandname"].ToString();
                        ostockmarker.patternname = reader["patternname"] == DBNull.Value ? "" : reader["patternname"].ToString();
                        ostockmarker.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                        ostockmarker.stylename = reader["stylename"] == DBNull.Value ? "" : reader["stylename"].ToString();
                        ostockmarker.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        ostockmarker.materialname = reader["materialname"] == DBNull.Value ? "" : reader["materialname"].ToString();
                        ostockmarker.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        ostockmarker.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();
                        ostockmarker.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        ostockmarker.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        ostockmarker.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        ostockmarker.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        ostockmarker.salestaxid = reader["salestaxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salestaxid"]);
                        ostockmarker.taxpercentage = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetaxpercentage"]);
                        lst.Add(ostockmarker);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _stockmarker);
            }
            return lst;
        }

        public List<Stockmarker> Save(MarkerContainer markercontainer)
        {
            var lstStockmarker = markercontainer.items;
            if (markercontainer.markerid == 0)
                throw new Exception("Some of the items doesn't have Marker ID");

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    foreach (Stockmarker oStockmarker in lstStockmarker)
                    {
                        if (markercontainer.isexclusive)
                        {
                            NpgsqlCommand xcommand = GetCommand("Update stockmarker set isactive=false,modifiedon=@modifiedon,modifiedby=@modifiedby where stockid=@stockid and locationid=@locationid and salablegoodsid=@salablegoodsid", conn, transaction);
                            xcommand.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            xcommand.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            xcommand.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockmarker.stockid;
                            xcommand.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockmarker.locationid;
                            xcommand.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockmarker.salablegoodsid;
                            xcommand.ExecuteNonQuery();
                        }
                        NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                        oStockmarker.isactive = true;
                        oStockmarker.createdby = this.Context.UserID;
                        oStockmarker.modifiedby = this.Context.UserID;
                        oStockmarker.modifiedon = DateTimeNow;
                        oStockmarker.createdon = DateTimeNow;
                        command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStockmarker.isactive;
                        command.Parameters.Add("isdamaged", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oStockmarker.isdamaged;
                        command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oStockmarker.version;
                        command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("addedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockmarker.stockid;
                        command.Parameters.Add("markerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = markercontainer.markerid;
                        command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockmarker.qty;
                        command.Parameters.Add("stockqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockmarker.stockqty;
                        command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockmarker.locationid;
                        command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockmarker.salablegoodsid;
                        command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStockmarker.barcode == null ? "" : oStockmarker.barcode;
                        command.Parameters.Add("glncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oStockmarker.glncode == null ? "" : oStockmarker.glncode;
                        command.ExecuteNonQuery();
                    }

                    if (markercontainer.isdamaged)
                    {
                        foreach (Stockmarker oStockmarker in lstStockmarker)
                        {
                            NpgsqlCommand command = GetCommand("Update stock set damaged=damaged+@damaged where id=@stockid", conn, transaction);
                            if (oStockmarker.salablegoodsid > 0)
                                command = GetCommand("Update salablegoods set dqty=dqty+@damaged where id=@salablegoodsid", conn, transaction);

                            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockmarker.stockid;
                            command.Parameters.Add("damaged", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockmarker.qty;
                            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockmarker.salablegoodsid;
                            command.ExecuteNonQuery();
                        }
                    }

                    //if (markercontainer.isonline)
                    //{
                    //    foreach (Stockmarker oStockmarker in lstStockmarker.FindAll(x => x.salablegoodsid > 0))
                    //    {
                    //        NpgsqlCommand command = GetCommand("Update salablegoods set olqty=@olqty where id=@salablegoodsid", conn, transaction);
                    //        command.Parameters.Add("olqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockmarker.qty;
                    //        command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockmarker.salablegoodsid;
                    //        command.ExecuteNonQuery();
                    //    }
                    //}

                    if (markercontainer.isonline)
                    {
                        List<string> stocknotfound = new List<string>();
                        List<OnlineStock.OnlineBarcodes> salablegoodsids = new List<OnlineStock.OnlineBarcodes>();
                        foreach (Stockmarker oStockmarker in lstStockmarker.FindAll(x => x.salablegoodsid > 0))
                        {
                            NpgsqlCommand command = GetCommand("Update salablegoods set olqty=olqty+@olqty where id=@salablegoodsid and (iqty-(oqty+dqty+jqty))>=(olqty+@olqty) RETURNING iqty,oqty,dqty,jqty,olqty;", conn, transaction);
                            command.Parameters.Add("olqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oStockmarker.qty;
                            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oStockmarker.salablegoodsid;
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    var iqty = reader["iqty"] == DBNull.Value ? 0 : (decimal)reader["iqty"];
                                    var oqty = reader["oqty"] == DBNull.Value ? 0 : (decimal)reader["oqty"];
                                    var dqty = reader["dqty"] == DBNull.Value ? 0 : (decimal)reader["dqty"];
                                    var jqty = reader["jqty"] == DBNull.Value ? 0 : (decimal)reader["jqty"];
                                    var olqty = reader["olqty"] == DBNull.Value ? 0 : (decimal)reader["olqty"];
                                    OnlineStock.OnlineBarcodes onlineBarcodes = new OnlineStock.OnlineBarcodes();
                                    onlineBarcodes.barcode = oStockmarker.barcode;
                                    onlineBarcodes.salablegoodsid = oStockmarker.salablegoodsid;
                                    onlineBarcodes.olqty = olqty;
                                    onlineBarcodes.stockqty = iqty - (oqty + dqty + jqty);
                                    salablegoodsids.Add(onlineBarcodes);
                                }
                                else
                                {
                                    stocknotfound.Add(oStockmarker.barcode + " - " + oStockmarker.qty);
                                }
                            }
                        }

                        if (stocknotfound.Count > 0)
                        {
                            throw new Exception("Physical stock not found for moving stock to Online stock - " +
                                string.Join(",", stocknotfound));
                        }

                        string query = string.Format(@"
                                         select serialno,salablegoods.id from salablegoods
                                         inner join stock on stock.id = salablegoods.stockid
                                         where stock.serialno in (
                                         Select serialno from salablegoods 
                                         inner join stock on stock.id = salablegoods.stockid
                                         where salablegoods.id in ({0}) and serialno is not null and serialno != '')
                                         and salablegoods.hasstock=true and iqty-(oqty+dqty+jqty)>0 and salablegoods.isactive=true",
                                         string.Join(",", salablegoodsids.ConvertAll(e => e.salablegoodsid)));

                        SortedList<long, string> GLNStocks = new SortedList<long, string>();
                        using (var reader = ExecuteReader(query, null, conn, transaction))
                        {
                            while (reader.Read())
                            {
                                GLNStocks.Add(reader.GetInt64(1), reader.GetString(0));
                            }
                        }

                        var serialnos = GLNStocks.ToList().ConvertAll(x => x.Value.Replace("'", "")).Distinct().ToList();
                        OnlineStock onlinestock = new OnlineStock()
                        {
                            items = new List<OnlineStock.OnlineBarcodes>(),
                            locationid = markercontainer.locationid
                        };
                        if (serialnos.Count > 0)
                        {
                            query = string.Format(@"select serialno,sum(iqty-(oqty+dqty+jqty)) stockqty,sum(olqty) oldqty from salablegoods
                                         inner join stock on stock.id = salablegoods.stockid
                                         where stock.serialno in ({0})
                                         and
                                         salablegoods.hasstock=true and iqty-(oqty+dqty+jqty)>0 
										 and salablegoods.isactive=true
										 group by serialno",
                                             string.Join(",", serialnos.ConvertAll(x => "'" + x + "'")));


                            using (var reader = ExecuteReader(query, null, conn, transaction))
                            {
                                while (reader.Read())
                                {
                                    OnlineStock.OnlineBarcodes onlineBarcodes = new OnlineStock.OnlineBarcodes();
                                    onlineBarcodes.barcode = reader.GetString(0);
                                    onlineBarcodes.glno = onlineBarcodes.barcode;
                                    onlineBarcodes.stockqty = reader[1] == DBNull.Value ? 0 : reader.GetDecimal(1);
                                    onlineBarcodes.olqty = reader[2] == DBNull.Value ? 0 : reader.GetDecimal(2);
                                    onlinestock.items.Add(onlineBarcodes);
                                }
                            }
                        }
                        foreach (var stockitem in markercontainer.items)
                        {
                            if (!GLNStocks.ContainsKey(stockitem.salablegoodsid) &&
                                salablegoodsids.Exists(x => x.salablegoodsid == stockitem.salablegoodsid))
                            {
                                OnlineStock.OnlineBarcodes onlineBarcodes = new OnlineStock.OnlineBarcodes();
                                onlineBarcodes.barcode = stockitem.barcode;
                                onlineBarcodes.glno = onlineBarcodes.barcode;
                                onlineBarcodes.stockqty = salablegoodsids.Find(x => x.salablegoodsid == stockitem.salablegoodsid).stockqty;
                                onlineBarcodes.olqty = salablegoodsids.Find(x => x.salablegoodsid == stockitem.salablegoodsid).olqty;
                                onlinestock.items.Add(onlineBarcodes);
                            }
                        }

                        Processors.Schedule_Actions.Instance.OnChange(this.Context.OrganizationID, onlinestock,
                            Processors.Schedule_Actions.Actions.ECommerce);
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, lstStockmarker);
                throw exp;
            }

            return lstStockmarker;

        }


        public List<Stockmarker> Delete(MarkerContainer markercontainer)
        {
            var lstStockmarker = markercontainer.items;
            if (markercontainer.markerid == 0)
                throw new Exception("Some of the items doesn't have Marker ID");

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    if (markercontainer.deleteall)
                    {
                        NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
                        if (markercontainer.isdamaged)
                        {
                            string query = @"update salablegoods set dqty=dqty-stockmarker.qty from stockmarker
                            where stockmarker.salablegoodsid = salablegoods.id and stockmarker.markerid = @markerid;
                            update public.stockmarker set isactive=false where markerid=@markerid;";
                            command = GetCommand(query, conn, transaction);
                        }
                        command.Parameters.Add("markerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = markercontainer.markerid;
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.ExecuteNonQuery();
                    }
                    else
                    {
                        NpgsqlCommand command = null;
                        string query = "";
                        string ids = string.Join(",", markercontainer.items.ConvertAll(e => "" + e.id + ""));
                        if (markercontainer.isdamaged)
                        {
                            query = @"update salablegoods set dqty=dqty-stockmarker.qty from stockmarker
                                where stockmarker.markerid = @markerid and stockmarker.salablegoodsid = salablegoods.id and stockmarker.id in ({0});";

                            command = GetCommand(string.Format(query, ids), conn, transaction);
                            command.Parameters.Add("markerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = markercontainer.markerid;
                            command.ExecuteNonQuery();

                            query = @"update stock set damagedqty=damagedqty-stockmarker.qty from stockmarker
                                where stockmarker.markerid = @markerid and stockmarker.stockid = stock.id and stockmarker.salablegoodsid = 0 and stockmarker.id in ({0});";

                            command = GetCommand(string.Format(query, ids), conn, transaction);
                            command.Parameters.Add("markerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = markercontainer.markerid;
                            command.ExecuteNonQuery();
                        }

                        query = @"update stockmarker set isactive = false, version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon 
                                where stockmarker.markerid = @markerid and stockmarker.id in ({0});";

                        command = GetCommand(string.Format(query, ids), conn, transaction);
                        command.Parameters.Add("markerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = markercontainer.markerid;
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.ExecuteNonQuery();

                        if (markercontainer.isonline)
                        {
                            query = @"update salablegoods set olqty=0 from stockmarker 
                                where stockmarker.markerid = @markerid and stockmarker.id in ({0})
                                and salablegoods.id = stockmarker.salablegoodsid;";

                            command = GetCommand(string.Format(query, ids), conn, transaction);
                            command.Parameters.Add("markerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = markercontainer.markerid;
                            command.ExecuteNonQuery();
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, markercontainer);
                throw exp;
            }

            return lstStockmarker;
        }

        internal List<Stockmarkerdetail> SearchStock(StockmarkerCreteria item)
        {
            List<Stockmarkerdetail> lst = new List<Stockmarkerdetail>();
            foreach (var itm in item.StockList)
            {
                var stocklist = Search(new StockmarkerCreteria()
                {
                    markerid = itm.markerid,
                    mode = 2,
                    locationid = item.locationid
                }).FindAll(x => x.stockqty > 0);
                while (stocklist.Count > 0 && itm.qty > 0)
                {
                    var sf = stocklist.First();
                    if (sf.stockqty >= itm.qty)
                    {
                        sf.stockqty = itm.qty;
                        lst.Add(sf);
                        stocklist.Remove(sf);
                    }
                    else
                    {
                        lst.Add(sf);
                        stocklist.Remove(sf);
                    }
                    itm.qty -= sf.stockqty;
                }
            }
            return lst;
        }

        internal void Associate(List<SplitBarcode> obarcodereport)
        {
            MarkerContainer markerContainer = new MarkerContainer();
            markerContainer.items = new List<Stockmarker>();
            markerContainer.markerid = obarcodereport.First().stockmarkerid;
            var barcodeList = new List<string>();
            foreach (var item in obarcodereport)
            {
                var list = new List<string>(item.newbarcodelist.Split(','));
                if (item.splitqty == 0) item.splitqty = item.splitpiece;
                list.RemoveAll(e => e == item.barcode);
                barcodeList.AddRange(list);
            }

            if (barcodeList.Count > 0)
            {
                string query = @"select id,stockid,barcode from salablegoods where barcode in (" + string.Join(",", barcodeList.ConvertAll(e => "'" + e + "'")) + ") and locationid=" + obarcodereport.First().locationid;
                using (var reader = ExecuteReader(query, new SortedList<string, object>()))
                {
                    while (reader.Read())
                    {
                        long salablegoodsid = reader.GetInt64(0);
                        long stockid = reader.GetInt64(1);
                        string barcode = reader.GetString(2);
                        markerContainer.items.Add(new Stockmarker()
                        {
                            markerid = obarcodereport.First().stockmarkerid,
                            isactive = true,
                            isdamaged = false,
                            stockid = stockid,
                            qty = obarcodereport.First().splitqty,
                            locationid = obarcodereport.First().locationid,
                            salablegoodsid = salablegoodsid,
                            barcode = barcode
                        });
                    }
                }

                Save(markerContainer);
            }
        }

        internal string SendOLQTY()
        {
            OnlineStock onlinestock = new OnlineStock()
            {
                items = new List<OnlineStock.OnlineBarcodes>(),
                fullsync = true
            };
            var query = @"select serialno,stock.barcode,sum(iqty-(oqty+dqty+jqty)) stockqty,sum(olqty) oldqty from salablegoods
                                         inner join stock on stock.id = salablegoods.stockid
                                         where 
                                         salablegoods.hasstock=true and olqty>0 
										 and salablegoods.isactive=true
										 group by serialno,stock.barcode";

            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    OnlineStock.OnlineBarcodes onlineBarcodes = new OnlineStock.OnlineBarcodes();
                    onlineBarcodes.glno = reader[0] == DBNull.Value ? "" : reader.GetString(0);
                    onlineBarcodes.barcode = reader[1] == DBNull.Value ? "" : reader.GetString(1);
                    onlineBarcodes.stockqty = reader[2] == DBNull.Value ? 0 : reader.GetDecimal(2);
                    onlineBarcodes.olqty = reader[3] == DBNull.Value ? 0 : reader.GetDecimal(3);
                    onlinestock.items.Add(onlineBarcodes);
                }
            }
            Processors.Schedule_Actions.Instance.OnChange(this.Context.OrganizationID, onlinestock,
                        Processors.Schedule_Actions.Actions.ECommerce);
            return "No Of barcodes "+ onlinestock.items.Count+" No of Qty "+
                onlinestock.items.Sum(x=>x.olqty);
        }
        internal bool SaveOLQTY(List<StockMarkerOLQty> items)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                OnlineStock onlinestock = new OnlineStock()
                {
                    items = new List<OnlineStock.OnlineBarcodes>()
                };
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    string query = "update salablegoods set olqty=@olqty where id=@id";
                    foreach (var item in items)
                    {
                        var command = GetCommand(query, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.salablegoodsid;
                        command.Parameters.Add("olqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = item.olqty;
                        command.ExecuteNonQuery();
                    }

                    //and salablegoods.hasstock=true and iqty-(oqty+dqty+jqty)>0 
                    query = string.Format(@"select serialno,stock.barcode,sum(iqty-(oqty+dqty+jqty)) stockqty,sum(olqty) oldqty from salablegoods
                                         inner join stock on stock.id = salablegoods.stockid
                                         where salablegoods.id in ({0})
										 and salablegoods.isactive=true
										 group by serialno,stock.barcode",
                    string.Join(",", items.ConvertAll(x => x.salablegoodsid)));

                    using (var reader = ExecuteReader(query, null, conn, transaction))
                    {
                        while (reader.Read())
                        {
                            OnlineStock.OnlineBarcodes onlineBarcodes = new OnlineStock.OnlineBarcodes();
                            onlineBarcodes.barcode = reader.GetString(1);
                            onlineBarcodes.glno = reader.IsDBNull(0) ? "": reader.GetString(0);
                            onlineBarcodes.stockqty = reader[2] == DBNull.Value ? 0 : reader.GetDecimal(2);
                            onlineBarcodes.olqty = reader[3] == DBNull.Value ? 0 : reader.GetDecimal(3);
                            onlinestock.items.Add(onlineBarcodes);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }

           
                if (onlinestock.items != null &&
                    onlinestock.items.Count>0)
                Processors.Schedule_Actions.Instance.OnChange(this.Context.OrganizationID, onlinestock,
                            Processors.Schedule_Actions.Actions.ECommerce);
                return true;
            }
            catch (Exception exp)
            {

            }
            return false;
        }

        internal List<Stockmarker> UpdateBarcodes(MarkerContainer item)
        {
            string query = @"select salablegoods.id,stockid,salablegoods.barcode,iqty from salablegoods
                            inner join stock on stock.id = stockid
                            where salablegoods.locationid = @locationid and 
                            stock.invoiceitemid in (" + string.Join(",",item.invoiceitemids) +")";

            var param = new SortedList<string, object>();
            param.Add("locationid", item.locationid);
            List<Stockmarker> stockmarkers = new List<Stockmarker>();
            using (var reader = ExecuteReader(query, param))
            {
                while(reader.Read())
                {
                    stockmarkers.Add(new Stockmarker()
                    {
                        salablegoodsid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]),
                        stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]),
                        barcode = reader["barcode"] == DBNull.Value ? "" : Convert.ToString(reader["barcode"]),
                        stockqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]),
                        qty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]),
                        stockqtyatmark = reader["iqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["iqty"]),
                        markerid = item.markerid,
                        isdamaged = item.isdamaged,
                        locationid = item.locationid,
                        isactive = true
                    });
                }
            }
            return stockmarkers;
        }
    }
}