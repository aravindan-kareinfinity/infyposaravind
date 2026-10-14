using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using WebAPI.Data;

namespace InfyPOS.Data
{
    public class BillitemsDataService : SAASData
    {
        const string SQL_Search = @"select billitems.id, billitems.isactive, billitems.createdby, billitems.modifiedby, billitems.version,billitems.locationid,billitems.sellingmode,
                                    billitems.createdon, billitems.modifiedon, barcode, stockid,salablegoodsid, printingname, employee.employeecode as salesmancode,billitems.productid,
                                    price, discount,additionaldiscount, receivable, salesmanid, tax, taxid, billitems.companyid, billid, qty, rate,employee.name as salesmanname,attributes,isexchange,
                                    piece,journalqty,itemtax,salerate,saleamount,taxpercentage,discountpercentage,additionaldiscountpercentage,hsncode,schemediscount,schemereward,
                                    schemediscountcoupen,schemeid,billitems.etprice,billitems.etdiscount,returnbillitemid,rtnsrcitemid
                                    from billitems 
                                    left outer join employee on employee.id = salesmanid
                                    where billitems.id=@id
                                    order by billitems.orderid";

        const string SQL_ItemSearch = @"select referencelist.name as location,billitems.barcode,customer.name customer,
                                    billitems.printingname, employee.employeecode as salesmancode,items.brandid,stock.designid,
                                    billitems.price, billitems.discount,billitems.additionaldiscount, stock.imageid as stockimageid,
                                    billitems.receivable, billitems.tax, billitems.qty, rate,employee.name as salesmanname,
                                    piece,billitems.journalqty,billitems.hsncode,billitems.id,billitems.imageid as returnimageid,billitems.isexchange,
                                    bill.billno,bill.billdate,bill.receivable+bill.rounding billvalue,billitems.productid,
                                    billitems.sellingmode,billitems.etprice,billitems.etdiscount,returnbillitemid,rtnsrcitemid
                                    from billitems 
                                    inner join bill on bill.id = billitems.billid
                                    inner join referencelist on referencelist.id = bill.locationid
                                    inner join salablegoods on salablegoods.id = billitems.salablegoodsid
				                    inner join stock on stock.id = salablegoods.stockid
				                    left outer join items on items.id = stock.itemid
                                    left outer join employee on employee.id = salesmanid
                                    left outer join customer on customer.id = bill.customerid";

        const string SQL_SearchAll = @"select billitems.id, billitems.isactive, billitems.createdby, billitems.modifiedby, billitems.version,billitems.locationid,billitems.sellingmode,
                                    billitems.createdon, billitems.modifiedon, barcode, stockid,salablegoodsid, printingname, employee.employeecode as salesmancode,billitems.productid,
                                    price, discount,additionaldiscount, receivable, salesmanid, tax, taxid, billitems.companyid, billid, qty, rate,employee.name as salesmanname,attributes,isexchange,
                                    piece,journalqty,itemtax,salerate,saleamount,taxpercentage,discountpercentage,additionaldiscountpercentage,hsncode,schemediscount,schemereward,
                                    schemediscountcoupen,schemeid,billitems.etprice,billitems.etdiscount,returnbillitemid,rtnsrcitemid
                                    from billitems 
                                    left outer join employee on employee.id = salesmanid
                                    where billid=@billid and billitems.isactive = true
                                    order by billitems.orderid";

        const string SQL_SearchAllWithTax = @"select billitems.id, billitems.isactive, billitems.createdby, billitems.modifiedby, billitems.version,billitems.locationid, 
                                    billitems.createdon, billitems.modifiedon, barcode, stockid,salablegoodsid, printingname, employee.employeecode as salesmancode,
                                    price, discount,additionaldiscount, receivable, salesmanid, tax, taxid, billitems.companyid, attributes,isexchange,
                                    billid, qty, rate,employee.name as salesmanname,taxsplit,tax.name as taxname,billitems.productid,billitems.sellingmode,
                                    piece, journalqty,itemtax,salerate,saleamount,billitems.taxpercentage,hsncode,schemediscount,schemereward,schemediscountcoupen,schemeid,
                                    discountpercentage,additionaldiscountpercentage,billitems.etprice,billitems.etdiscount,returnbillitemid,rtnsrcitemid
                                    from billitems 
                                    left outer join tax on tax.id = taxid
                                    left outer join employee on employee.id = salesmanid
                                    where billitems.billid=@billid and billitems.isactive = true
                                    order by billitems.orderid";


        const string SQL_Insert = @"INSERT INTO public.billitems(isactive, createdby, modifiedby, version, createdon, modifiedon, barcode, stockid,salablegoodsid, printingname, price, discount,additionaldiscount,
receivable, salesmanid, tax, taxid, companyid, billid, qty, rate,locationid,orderid,piece,journalqty,itemtax,salerate,saleamount,taxpercentage,discountpercentage,additionaldiscountpercentage,hsncode,
schemediscount,schemereward,schemediscountcoupen,schemeid,incentive,attributes,productid,sellingmode,isexchange,etprice,etdiscount,returnbillitemid,rtnsrcitemid) 
VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @barcode, @stockid,@salablegoodsid, @printingname, @price,
@discount,@additionaldiscount, @receivable, @salesmanid, @tax, @taxid, @companyid, @billid, @qty, @rate,@locationid,@orderid,@piece,@journalqty,@itemtax,@salerate,@saleamount,@taxpercentage,@discountpercentage,
@additionaldiscountpercentage,@hsncode,@schemediscount,@schemereward,@schemediscountcoupen,@schemeid,@incentive,@attributes,@productid,
@sellingmode,@isexchange,@etprice,@etdiscount,@returnbillitemid,@rtnsrcitemid) RETURNING id";

        const string SQL_UpdateStock = @"update salablegoods set oqty=oqty+@qty where id=@id and stockid=@stockid RETURNING iqty,oqty,dqty,jqty,olqty";

        const string SQL_Update = @"UPDATE public.billitems SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, barcode=@barcode,locationid=@locationid, stockid=@stockid,salablegoodsid=@salablegoodsid,
printingname=@printingname, price=@price, discount=@discount,additionaldiscount=@additionaldiscount,taxpercentage=@taxpercentage, receivable=@receivable, salesmanid=@salesmanid, tax=@tax, taxid=@taxid,hsncode=@hsncode,
companyid=@companyid, billid=@billid, qty=@qty, piece=@piece, rate=@rate,schemediscount=@schemediscount,schemeid=@schemeid,productid=@productid,sellingmode=@sellingmode,salerate=@salerate,saleamount=@saleamount,attributes=@attributes,
etprice=@etprice,etdiscount=@etdiscount,itemtax=@itemtax,additionaldiscountpercentage=@additionaldiscountpercentage,discountpercentage=@discountpercentage,schemereward=@schemereward,schemediscountcoupen=@schemediscountcoupen
where id=@id and version=@version RETURNING version,qty,piece";

        const string SQL_Delete = @"update public.billitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";
        const string SQL_DeleteAll = @"update public.billitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon,version=version+1 where billid=@billid";
        public List<Billitems> Search(BillitemsCreteria _billitems)
        {
            List<Billitems> lst = new List<Billitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_billitems.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("billitems.isactive", true, "=");
                    if (_billitems.ids != null)
                    {
                        qb.AddInParamer("Billitems.id", string.Join<long>(",", _billitems.ids));
                    }
                    if (_billitems.billids != null)
                    {
                        qb.AddInParamer("Billitems.billid", string.Join<long>(",", _billitems.billids));
                    }
                    if (_billitems.salablegoodsids != null)
                    {
                        qb.AddInParamer("salablegoodsid", string.Join<long>(",", _billitems.salablegoodsids));
                    }
                    //if (!string.IsNullOrEmpty(_billitems.Code))
                    //    qb.AddParameter("code", _billitems.Code, "=");

                    command = qb.GetCommand(conn);
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billitems.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Billitems obillitems = new Billitems();
                        obillitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obillitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        obillitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obillitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obillitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obillitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        obillitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obillitems.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"].ToString());
                        obillitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obillitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        obillitems.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                        obillitems.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        obillitems.attributes_json = reader["attributes"] == DBNull.Value ? "" : reader["attributes"].ToString();
                        obillitems.salesmanname = reader["salesmanname"] == DBNull.Value ? "" : reader["salesmanname"].ToString();
                        obillitems.salesmancode = reader["salesmancode"] == DBNull.Value ? "" : reader["salesmancode"].ToString();
                        obillitems.salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"].ToString());
                        obillitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        obillitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        obillitems.piece = reader["piece"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["piece"]);
                        obillitems.journalqty = reader["journalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["journalqty"]);
                        obillitems.itemtax = reader["itemtax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itemtax"]);
                        obillitems.salerate = reader["salerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salerate"]);
                        obillitems.saleamount = reader["saleamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleamount"]);
                        obillitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        obillitems.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"]);
                        obillitems.additionaldiscountpercentage = reader["additionaldiscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscountpercentage"]);
                        obillitems.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"].ToString());

                        obillitems.schemediscountcoupen = reader["schemediscountcoupen"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemediscountcoupen"]);
                        obillitems.schemereward = reader["schemereward"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemereward"]);
                        obillitems.schemediscount = reader["schemediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemediscount"]);
                        obillitems.schemeid = reader["schemeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["schemeid"]);
                        obillitems.isexchange = reader["isexchange"] == DBNull.Value ? false : Convert.ToBoolean(reader["isexchange"]);
                        obillitems.etprice = reader["etprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["etprice"]);
                        obillitems.etdiscount = reader["etdiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["etdiscount"]);

                        obillitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        obillitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        obillitems.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"].ToString());
                        obillitems.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        obillitems.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"].ToString());
                        obillitems.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"].ToString());
                        obillitems.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        obillitems.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        obillitems.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"]);
                        obillitems.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);

                        if (obillitems.attributes != null &&
                            obillitems.attributes.colourname != null &&
                            obillitems.attributes.designid != null)
                        {
                            obillitems.attributes.groupname = obillitems.attributes.designid + "/" + obillitems.attributes.colourname;
                            obillitems.attributes.groupkey = obillitems.attributes.designid + "/" + obillitems.attributes.colourname;
                        }
                        obillitems.orderid = lst.Count + 1;
                        lst.Add(obillitems);
                    }
                }
                conn.Close();


            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }

        public void UpdateProductUOM(List<PurchaseorderitemsDetail> lst)
        {
            string query = @"select id, uomid from products 
                            where products.id in (" + string.Join(",", lst.ConvertAll<long>(e => e.productid)) + ")";
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    long prodcutid = reader.GetInt64(0);
                    int sellingid = reader.IsDBNull(1) ? 1 : reader.GetInt32(1);
                    lst.ForEach(e =>
                    {
                        if (e.productid == prodcutid)
                            e.uomid = sellingid;
                    });
                }
            }
        }

        public void UpdateSellingMode(List<Billitems> lst)
        {
            string query = @"select stock.id,sellingmode from stock
                    left outer join products on products.id = stock.productid
                    where stock.id in (" + string.Join(",", lst.ConvertAll<long>(e => e.stockid)) + ")";
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    long stockid = reader.GetInt64(0);
                    int sellingid = reader.IsDBNull(1) ? 1 : reader.GetInt32(1);
                    lst.ForEach(e =>
                    {
                        if (e.stockid == stockid)
                            e.sellingmode = sellingid;
                    });
                }
            }
        }
        public List<Billitems> SearchAll(long _billid, bool includetaxinformation, bool isinterstate)
        {
            List<Billitems> lst = new List<Billitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                lst = SearchAll(_billid, includetaxinformation, isinterstate, conn, null);
                conn.Close();
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return lst;
        }

        public List<Billitems> SearchAll(long _billid, bool includetaxinformation, bool isinterstate, Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction transaction)
        {
            List<Billitems> lst = new List<Billitems>();

            NpgsqlCommand command = GetCommand(includetaxinformation ? SQL_SearchAllWithTax : SQL_SearchAll, conn);
            if (transaction != null) command.Transaction = transaction;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Billitems obillitems = new Billitems();
                    obillitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    obillitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                    obillitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                    obillitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                    obillitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                    obillitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                    obillitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                    obillitems.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"].ToString());
                    obillitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                    obillitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                    obillitems.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                    obillitems.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                    obillitems.attributes_json = reader["attributes"] == DBNull.Value ? "" : reader["attributes"].ToString();
                    obillitems.salesmanname = reader["salesmanname"] == DBNull.Value ? "" : reader["salesmanname"].ToString();
                    obillitems.salesmancode = reader["salesmancode"] == DBNull.Value ? "" : reader["salesmancode"].ToString();
                    obillitems.salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"].ToString());
                    obillitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                    obillitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                    obillitems.piece = reader["piece"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["piece"]);
                    obillitems.journalqty = reader["journalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["journalqty"]);
                    obillitems.itemtax = reader["itemtax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itemtax"]);
                    obillitems.salerate = reader["salerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salerate"]);
                    obillitems.saleamount = reader["saleamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleamount"]);
                    obillitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                    obillitems.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"]);
                    obillitems.additionaldiscountpercentage = reader["additionaldiscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscountpercentage"]);
                    obillitems.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"].ToString());

                    obillitems.schemediscountcoupen = reader["schemediscountcoupen"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemediscountcoupen"]);
                    obillitems.schemereward = reader["schemereward"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemereward"]);
                    obillitems.schemediscount = reader["schemediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemediscount"]);
                    obillitems.schemeid = reader["schemeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["schemeid"]);
                    obillitems.isexchange = reader["isexchange"] == DBNull.Value ? false : Convert.ToBoolean(reader["isexchange"]);
                    obillitems.etprice = reader["etprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["etprice"]);
                    obillitems.etdiscount = reader["etdiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["etdiscount"]);

                    obillitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    obillitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                    obillitems.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"].ToString());
                    obillitems.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                    obillitems.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"].ToString());
                    obillitems.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"].ToString());
                    obillitems.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                    obillitems.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                    obillitems.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"]);
                    obillitems.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                    obillitems.returnbillitemid = reader["returnbillitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["returnbillitemid"].ToString());
                    obillitems.rtnsrcitemid = reader["rtnsrcitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["rtnsrcitemid"].ToString());


                    if (obillitems.attributes != null &&
                        obillitems.attributes.colourname != null &&
                        obillitems.attributes.designid != null)
                    {
                        obillitems.attributes.groupname = obillitems.attributes.designid + "/" + obillitems.attributes.colourname;
                        obillitems.attributes.groupkey = obillitems.attributes.designid + "/" + obillitems.attributes.colourname;
                    }


                    obillitems.taxsplit = new TaxSplit();
                    if (includetaxinformation)
                        obillitems.taxsplit.name = reader["taxname"] == DBNull.Value ? "" : reader["taxname"].ToString();
                    if (includetaxinformation && reader["taxsplit"] != DBNull.Value)
                    {
                        var taxsplit = Newtonsoft.Json.JsonConvert.DeserializeObject<Tax.TaxSplit>(reader["taxsplit"].ToString());

                        if (isinterstate)
                        {
                            obillitems.taxsplit.secondtax = "IGST";
                        }
                        else
                        {
                            obillitems.taxsplit.secondtax = "SGST";
                        }

                        if (taxsplit != null)
                        {
                            //rajasilks_v5
                            var taxconfig = taxsplit.Find(obillitems.price - obillitems.additionaldiscount);
                            if (taxconfig != null)
                            {
                                obillitems.taxsplit.sgsttaxpercentage = isinterstate ? taxconfig.igst : taxconfig.sgst;
                                obillitems.taxsplit.cgsttaxpercentage = isinterstate ? 0 : taxconfig.cgst;
                                if (obillitems.taxsplit.cgsttaxpercentage + obillitems.taxsplit.sgsttaxpercentage > 0)
                                {
                                    if (!isinterstate)
                                    {
                                        obillitems.taxsplit.sgsttaxamount = Math.Round(obillitems.tax / (obillitems.taxsplit.cgsttaxpercentage + obillitems.taxsplit.sgsttaxpercentage) * obillitems.taxsplit.sgsttaxpercentage, 4);
                                        obillitems.taxsplit.cgsttaxamount = obillitems.taxsplit.sgsttaxamount;
                                    }
                                    else
                                    {
                                        obillitems.taxsplit.sgsttaxamount = 0;
                                        obillitems.taxsplit.cgsttaxamount = Math.Round(obillitems.tax, 4);
                                    }
                                }
                            }
                            else
                            {
                                obillitems.taxsplit.sgsttaxpercentage = isinterstate ? obillitems.taxpercentage : obillitems.taxpercentage / 2;
                                obillitems.taxsplit.cgsttaxpercentage = isinterstate ? 0 : obillitems.taxpercentage / 2;
                                if (!isinterstate)
                                {
                                    obillitems.taxsplit.sgsttaxamount = Math.Round(obillitems.tax / (obillitems.taxsplit.cgsttaxpercentage + obillitems.taxsplit.sgsttaxpercentage) * obillitems.taxsplit.sgsttaxpercentage, 4);
                                    obillitems.taxsplit.cgsttaxamount = obillitems.taxsplit.sgsttaxamount;
                                }
                                else
                                {
                                    obillitems.taxsplit.sgsttaxamount = 0;
                                    obillitems.taxsplit.cgsttaxamount = Math.Round(obillitems.tax, 4);
                                }
                            }
                        }
                        else if (obillitems.taxpercentage > 0 && Math.Abs(obillitems.tax) > 0)
                        {
                            obillitems.taxsplit.sgsttaxpercentage = isinterstate ? obillitems.taxpercentage : obillitems.taxpercentage / 2;
                            obillitems.taxsplit.cgsttaxpercentage = isinterstate ? 0 : obillitems.taxpercentage / 2;
                            if (!isinterstate)
                            {
                                obillitems.taxsplit.sgsttaxamount = Math.Round(obillitems.tax / (obillitems.taxsplit.cgsttaxpercentage + obillitems.taxsplit.sgsttaxpercentage) * obillitems.taxsplit.sgsttaxpercentage, 4);
                                obillitems.taxsplit.cgsttaxamount = obillitems.taxsplit.sgsttaxamount;
                            }
                            else
                            {
                                obillitems.taxsplit.sgsttaxamount = 0;
                                obillitems.taxsplit.cgsttaxamount = Math.Round(obillitems.tax, 4);
                            }
                        }
                    }
                    obillitems.orderid = lst.Count + 1;
                    lst.Add(obillitems);
                }
            }

            return lst;
        }


        public List<BillitemsB2B> SearchB2BAll(ItemSearchCreteria creteria)
        {
            List<BillitemsB2B> lst = new List<BillitemsB2B>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_ItemSearch;
                qb.AddParameter("bill.isactive", true, "=");
                qb.AddParameter("bill.billtype", 9, "!=");

                if (!string.IsNullOrEmpty(creteria.barcode))
                    qb.AddParameter("billitems.barcode", creteria.barcode, "=");

                if (creteria.barcodes != null && creteria.barcodes.Count > 0)
                    qb.AddInParamer("billitems.barcode", string.Join(",", creteria.barcodes.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")));

                if (!string.IsNullOrEmpty(creteria.designid))
                    qb.AddParameter("stock.designid", "%" + WebAPI.Data.Utility.SafeSQL(creteria.barcode) + "%", " like ");

                if (creteria.locationid > 0)
                    qb.AddParameter("bill.locationid", creteria.locationid, "=");
                if (creteria.companyid > 0)
                    qb.AddParameter("bill.companyid", creteria.companyid, "=");
                if (creteria.customerid > 0)
                    qb.AddParameter("bill.customerid", creteria.customerid, "=");
                if (creteria.patternid > 0)
                    qb.AddParameter("items.patternid", creteria.patternid, "=");
                if (creteria.brandid > 0)
                    qb.AddParameter("items.brandid", creteria.brandid, "=");
                if (creteria.productid > 0)
                    qb.AddParameter("items.productid", creteria.productid, "=");
                if (creteria.supplierid > 0)
                    qb.AddParameter("stock.supplierid", creteria.supplierid, "=");
                if (creteria.materialid > 0)
                    qb.AddParameter("items.materialid", creteria.materialid, "=");
                if (creteria.styleid > 0)
                    qb.AddParameter("items.styleid", creteria.styleid, "=");
                if (creteria.typeid > 0)
                    qb.AddParameter("items.typeid", creteria.typeid, "=");
                if (creteria.colourid > 0)
                    qb.AddParameter("items.colourid", creteria.colourid, "=");
                if (creteria.patternid > 0)
                    qb.AddParameter("items.patternid", creteria.patternid, "=");
                if (!string.IsNullOrEmpty(creteria.mobileno))
                    qb.AddParameter("customer.mobileno", creteria.mobileno, "=");

                if (creteria.pricefrom > 0)
                {
                    qb.AddParameter("billitems.price", "pricefrom", creteria.pricefrom, ">=");
                }
                if (creteria.priceto < 0)
                {
                    qb.AddParameter("billitems.price", "priceto", creteria.priceto, "<");
                }

                if (creteria.fromdate != DateTime.MinValue)
                {
                    qb.AddParameter("bill.billdate", "billdatefrom", creteria.fromdate.Date, ">=");
                }
                if (creteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("bill.billdate", "billdateto", creteria.todate, "<");
                }
                if (creteria.billamtfrom > 0)
                {
                    qb.CurrentQuery += " and bill.receivable>=" + creteria.billamtfrom + "";
                    //qb.AddParameter("billitems.receivable", "receivablefrom", creteria.billamtfrom, ">=");
                }
                if (creteria.billamtto > 0)
                {
                    qb.CurrentQuery += " and bill.receivable<=" + creteria.billamtto + "";
                    //qb.AddParameter("billitems.receivable", "receivableto", creteria.billamtto, "<=");
                }
                qb.SortField = "billitems.id desc";
                qb.Limit = 1000;
                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BillitemsB2B obillitems = new BillitemsB2B();
                        obillitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        obillitems.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"].ToString());
                        obillitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obillitems.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        obillitems.location = reader["location"] == DBNull.Value ? "" : reader["location"].ToString();
                        obillitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        obillitems.returnimageid = reader["returnimageid"] == DBNull.Value ? "" : reader["returnimageid"].ToString();
                        obillitems.stockimageid = reader["stockimageid"] == DBNull.Value ? "" : reader["stockimageid"].ToString();
                        obillitems.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        obillitems.customer = reader["customer"] == DBNull.Value ? "" : reader["customer"].ToString();
                        obillitems.salesmanname = reader["salesmanname"] == DBNull.Value ? "" : reader["salesmanname"].ToString();
                        obillitems.salesmancode = reader["salesmancode"] == DBNull.Value ? "" : reader["salesmancode"].ToString();
                        obillitems.designid = reader["designid"] == DBNull.Value ? "" : reader["designid"].ToString();
                        obillitems.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"]);
                        obillitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        obillitems.piece = reader["piece"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["piece"]);
                        obillitems.journalqty = reader["journalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["journalqty"]);
                        obillitems.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        obillitems.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"]);
                        obillitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        obillitems.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"].ToString());
                        obillitems.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        obillitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        obillitems.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        obillitems.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"]);
                        obillitems.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        obillitems.orderid = lst.Count + 1;
                        lst.Add(obillitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State != System.Data.ConnectionState.Closed)
                    conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return lst;
        }
        public List<Billitems> SearchAll(ItemSearchCreteria creteria)
        {
            if (string.IsNullOrEmpty(creteria.barcode) &&
                (creteria.barcodes == null || creteria.barcodes.Count == 0) &&
                string.IsNullOrEmpty(creteria.mobileno))
                throw new Exception("Insufficient search criteria. either Barcode/Barcodes condition is required");

            List<Billitems> lst = new List<Billitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_ItemSearch;
                qb.AddParameter("bill.isactive", true, "=");
                qb.AddParameter("bill.billtype", 9, "!=");


                if (!string.IsNullOrEmpty(creteria.barcode))
                    qb.AddParameter("billitems.barcode", creteria.barcode, "=");
                if (creteria.barcodes != null && creteria.barcodes.Count > 0)
                    qb.AddInParamer("billitems.barcode", string.Join(",", creteria.barcodes.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")));


                if (creteria.locationid > 0)
                    qb.AddParameter("bill.locationid", creteria.locationid, "=");
                if (creteria.companyid > 0)
                    qb.AddParameter("bill.companyid", creteria.companyid, "=");
                if (creteria.customerid > 0)
                    qb.AddParameter("bill.customerid", creteria.customerid, "=");
                if (creteria.patternid > 0)
                    qb.AddParameter("items.patternid", creteria.patternid, "=");
                if (creteria.brandid > 0)
                    qb.AddParameter("items.brandid", creteria.brandid, "=");
                if (creteria.productid > 0)
                    qb.AddParameter("items.productid", creteria.productid, "=");
                if (creteria.supplierid > 0)
                    qb.AddParameter("stock.supplierid", creteria.supplierid, "=");
                if (creteria.materialid > 0)
                    qb.AddParameter("items.materialid", creteria.materialid, "=");
                if (creteria.styleid > 0)
                    qb.AddParameter("items.styleid", creteria.styleid, "=");
                if (creteria.typeid > 0)
                    qb.AddParameter("items.typeid", creteria.typeid, "=");
                if (creteria.colourid > 0)
                    qb.AddParameter("items.colourid", creteria.colourid, "=");
                if (creteria.patternid > 0)
                    qb.AddParameter("items.patternid", creteria.patternid, "=");
                if (!string.IsNullOrEmpty(creteria.mobileno))
                    qb.AddParameter("customer.mobileno", creteria.mobileno, "=");

                if (creteria.fromdate != DateTime.MinValue)
                {
                    qb.AddParameter("bill.billdate", "billdatefrom", creteria.fromdate.Date, ">=");
                }
                if (creteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("bill.billdate", "billdateto", creteria.todate, "<");
                }
                if (creteria.billamtfrom > 0)
                {
                    qb.CurrentQuery += " and bill.receivable>=" + creteria.billamtfrom + "";
                    //qb.AddParameter("billitems.receivable", "receivablefrom", creteria.billamtfrom, ">=");
                }
                if (creteria.billamtto > 0)
                {
                    qb.CurrentQuery += " and bill.receivable<=" + creteria.billamtto + "";
                    //qb.AddParameter("billitems.receivable", "receivableto", creteria.billamtto, "<=");
                }
                qb.SortField = "billitems.id desc";
                qb.Limit = 10000;
                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Billitemdetails obillitems = new Billitemdetails();
                        obillitems.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        obillitems.sellingmode = reader["sellingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sellingmode"].ToString());
                        obillitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obillitems.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        obillitems.customer = reader["customer"] == DBNull.Value ? "" : reader["customer"].ToString();
                        obillitems.location = reader["location"] == DBNull.Value ? "" : reader["location"].ToString();
                        obillitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        obillitems.returnimageid = reader["returnimageid"] == DBNull.Value ? "" : reader["returnimageid"].ToString();
                        obillitems.stockimageid = reader["stockimageid"] == DBNull.Value ? "" : reader["stockimageid"].ToString();
                        obillitems.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        obillitems.salesmanname = reader["salesmanname"] == DBNull.Value ? "" : reader["salesmanname"].ToString();
                        obillitems.salesmancode = reader["salesmancode"] == DBNull.Value ? "" : reader["salesmancode"].ToString();
                        obillitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        obillitems.piece = reader["piece"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["piece"]);
                        obillitems.journalqty = reader["journalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["journalqty"]);
                        obillitems.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        obillitems.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"]);
                        obillitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        obillitems.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"].ToString());
                        obillitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        obillitems.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        obillitems.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        obillitems.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"]);
                        obillitems.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        obillitems.orderid = lst.Count + 1;
                        obillitems.returnbillitemid = reader["returnbillitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["returnbillitemid"].ToString());
                        obillitems.rtnsrcitemid = reader["rtnsrcitemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["rtnsrcitemid"].ToString());

                        lst.Add(obillitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State != System.Data.ConnectionState.Closed)
                    conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return lst;
        }

        public Billitems Insert(Billitems oBillitems, Bill bill, long locationid)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();


                    Insert(transaction, conn, bill, oBillitems, locationid);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

            return oBillitems;
        }

        public class ConflictUAN
        {
            public string barcode { get; set; }
            public decimal qty { get; set; }
            public long stockid { get; set; }
            public long salablegoodsid { get; set; }
        }

        public Billitems InsertInternal(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill bill, Billitems oBillitems, long locid)
        {


            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillitems.isactive;
            command.Parameters.Add("isexchange", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillitems.isexchange;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.productid;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillitems.version;
            command.Parameters.Add("sellingmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillitems.sellingmode;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillitems.barcode == null ? "" : oBillitems.barcode;
            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillitems.attributes == null ? "null" : oBillitems.attributes_json;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillitems.hsncode == null ? "" : oBillitems.hsncode;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.stockid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.locationid;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillitems.printingname == null ? "" : oBillitems.printingname;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.price;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.discount;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.additionaldiscount;

            command.Parameters.Add("etprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.etprice;
            command.Parameters.Add("etdiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.etdiscount;

            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.receivable;
            command.Parameters.Add("salesmanid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salesmanid;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.tax;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.taxid;
            command.Parameters.Add("orderid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillitems.orderid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.companyid;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.billid;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salablegoodsid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.qty;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.rate;
            command.Parameters.Add("schemediscountcoupen", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.schemediscountcoupen;
            command.Parameters.Add("schemereward", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.schemereward;
            command.Parameters.Add("schemediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.schemediscount;
            command.Parameters.Add("schemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.schemeid;
            command.Parameters.Add("piece", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.piece;
            command.Parameters.Add("journalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.journalqty;
            command.Parameters.Add("itemtax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.itemtax;
            command.Parameters.Add("salerate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.salerate;
            command.Parameters.Add("saleamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.saleamount;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.taxpercentage;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.discountpercentage;
            command.Parameters.Add("additionaldiscountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.additionaldiscountpercentage;
            command.Parameters.Add("incentive", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.incentive;
            command.Parameters.Add("returnbillitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.returnbillitemid;
            command.Parameters.Add("rtnsrcitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.rtnsrcitemid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            return oBillitems;
        }

        public void UpdateSOItem(List<Billitems> billitems)
        {

            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                foreach (var oBillitems in billitems)
                {
                    //var command = GetCommand("Update Salesorderitems set bqty=bqty+@bqty,pqty=(case when pqty>=@bqty then pqty-@bqty else 0 end) where id=@id and oqty>bqty+@bqty RETURNING BQTY,OQTY,UID", conn, transaction);
                    //command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    //command.Parameters.Add("bqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    //foreach (var soitem in oBillitems.attributes.so.GroupBy(e => e.id))
                    //{
                    //    command.Parameters["id"].Value = soitem.Key;
                    //    command.Parameters["bqty"].Value = Math.Abs(soitem.Sum(e => e.qty));
                    //    using (var reader = command.ExecuteReader())
                    //    {
                    //        if (reader.Read())
                    //        {
                    //            var BQTY = reader["bqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bqty"]);
                    //            var OQTY = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                    //            string UID = reader["UID"] == DBNull.Value ? "" : Convert.ToString(reader["UID"]);
                    //            if (OQTY < BQTY)
                    //            {
                    //                conn.Close();
                    //                return;
                    //            }
                    //        }
                    //    }
                    //}
                    var command = GetCommand("Update billitems set attributes=@attributes where id=@id", conn, transaction);
                    command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillitems.attributes == null ? "null" : oBillitems.attributes_json;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.id;
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
                conn.Close();
            }
        }
        public Billitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill bill, Billitems oBillitems, long locid)
        {


            if (oBillitems.isactive == true &&
                !bill.issynchronize && Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany && oBillitems.companyid <= 0)
                throw new Exception("Please select the company");

            if (oBillitems.schemeid == 0 && oBillitems.schemediscount > 0)
                oBillitems.schemediscount = 0;


            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication &&
                Math.Abs(oBillitems.qty) != Math.Abs(oBillitems.piece) && oBillitems.sellingmode != 3)
                throw new Exception("Qty / Piece mismatching " + oBillitems.ToJSON());

            if (oBillitems.sellingmode == 2 && oBillitems.qty != oBillitems.piece)
                oBillitems.piece = oBillitems.qty;

            if (oBillitems.receivable < 0 && oBillitems.piece > 0)
                oBillitems.piece = oBillitems.piece * -1;

            if (oBillitems.receivable > 0 && oBillitems.piece < 0)
                oBillitems.piece = oBillitems.piece * -1;

            if (oBillitems.taxid == 0 && !this.IsMigration)
                throw new Exception("Tax information missing for " + oBillitems.barcode + "(" + oBillitems.printingname + ")");


            if (!IsMigration && oBillitems.taxpercentage > 0 &&
                oBillitems.receivable != 0 &&
                oBillitems.tax == 0)
                throw new Exception("Tax computation missing for " + oBillitems.barcode + "(" + oBillitems.printingname + ")");

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5 &&
                oBillitems.taxpercentage > 0)
            {
                var projtax = oBillitems.receivable / (100 + oBillitems.taxpercentage) * oBillitems.taxpercentage;
                var projitax = projtax / oBillitems.qty;
                if (Math.Abs(Math.Abs(projtax) - Math.Abs(oBillitems.tax)) > (decimal)0.2)
                {
                    throw new Exception(oBillitems.barcode + "(" + oBillitems.attributes.serialno + ") contains some tax missmatches");
                }
            }

            if (oBillitems.isactive == true &&
                oBillitems.attributes != null &&
                oBillitems.attributes.so != null &&
                oBillitems.attributes.so.Count > 0 &&
                oBillitems.attributes.so.Sum(x => x.qty) < Math.Abs(oBillitems.qty))
            {
                var available = oBillitems.attributes.so.ToList();
                oBillitems.attributes.so.Clear();
                decimal allocated = 0;
                foreach (var soitem in available)
                {
                    if (soitem.qty < (Math.Abs(oBillitems.qty) - allocated))
                    {
                        oBillitems.attributes.so.Add(soitem);
                    }
                    else
                    {
                        soitem.qty = Math.Abs(oBillitems.qty) - allocated;
                        oBillitems.attributes.so.Add(soitem);
                        break;
                    }
                }
            }

            if (oBillitems.etprice == 0)
            {
                oBillitems.etprice = Math.Round((oBillitems.receivable - oBillitems.tax) / oBillitems.qty, 6);
                if (oBillitems.qty < 0)
                    oBillitems.etprice = Math.Abs(oBillitems.etprice);
            }


            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5 &&
                bill.istransfer != true &&
                string.IsNullOrEmpty(bill.ecomorderno) &&
                !bill.DontChangeStock)
            {
                var hasorderid = (bill.billattributes != null &&
                                  !string.IsNullOrEmpty(bill.billattributes.orderrefid));
                if (hasorderid && bill.billtype != Bill.BillTypes.ReturnBill)
                {
                    if (oBillitems.attributes == null ||
                        oBillitems.attributes.so == null ||
                        oBillitems.attributes.so.Sum(x => Math.Abs(x.qty)) != Math.Abs(oBillitems.qty))
                    {
                        throw new Exception("Matching sales order missing for " + oBillitems.attributes.serialno);
                    }
                }
            }
            InsertInternal(transaction, conn, bill, oBillitems, locid);
            NpgsqlCommand command = null;
            if (!IsMigration && oBillitems.isactive == true && oBillitems.qty != 0)
            {
                /*with allbilledqty as
                (
                select cast(schemelist->>'id' as bigint) salesorderitemid,
                sum(cast(schemelist->>'qty' as numeric)) billedqty from billitems
                inner join bill on bill.id = billid  and bill.isactive=true 
                LEFT JOIN LATERAL  json_array_elements (billitems.attributes->'so') schemelist ON true
                where billitems.isactive=true and billitems.qty<0
                group by cast(schemelist->>'id' as bigint)
                )

                update salesorderitems set bqty=billedqty from allbilledqty
                where allbilledqty.salesorderitemid =salesorderitems.id*/


                if (bill.billattributes != null && string.IsNullOrEmpty(bill.billattributes.orderrefid) &&
                    (bill.isreturn || bill.billtype == Bill.BillTypes.ReturnBill) &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5)
                {
                    oBillitems.attributes.so = new List<BillItemAttributes.itemqty>();
                }
                var billqtycanexceedso = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode != 5;
                if (oBillitems.attributes != null &&
                    oBillitems.attributes.so != null &&
                    oBillitems.attributes.so.Count > 0
                    )
                {
                    if (bill.billtype != Bill.BillTypes.ReturnBill)
                    {
                        command = GetCommand("Update Salesorderitems set bqty=bqty+@bqty,pqty=(case when pqty>=@bqty then pqty-@bqty else 0 end) where id=@id RETURNING BQTY,OQTY,UID", conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                        command.Parameters.Add("bqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                        foreach (var soitem in oBillitems.attributes.so.GroupBy(e => e.id))
                        {
                            command.Parameters["id"].Value = soitem.Key;
                            command.Parameters["bqty"].Value = soitem.Sum(e => e.qty);
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    var BQTY = reader["bqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bqty"]);
                                    var OQTY = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                                    string UID = reader["UID"] == DBNull.Value ? "" : Convert.ToString(reader["UID"]);
                                    if (!billqtycanexceedso &&
                                        OQTY < BQTY)
                                    {
                                        throw new Exception(UID + " Can't exceed the Order of " + OQTY);
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        command = GetCommand("Update Salesorderitems set bqty=bqty-@bqty where id=@id RETURNING BQTY,OQTY,UID", conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                        command.Parameters.Add("bqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                        foreach (var soitem in oBillitems.attributes.so.GroupBy(e => e.id))
                        {
                            command.Parameters["id"].Value = soitem.Key;
                            command.Parameters["bqty"].Value = Math.Abs(soitem.Sum(e => e.qty))*-1;
                            using (var reader = command.ExecuteReader())
                            {
                                if (reader.Read())
                                {
                                    var BQTY = reader["bqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["bqty"]);
                                    var OQTY = reader["oqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oqty"]);
                                    string UID = reader["UID"] == DBNull.Value ? "" : Convert.ToString(reader["UID"]);
                                    if (BQTY<0)
                                    {
                                        throw new Exception(UID + " Can't reduce the Billed of " + BQTY);
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (!IsMigration && oBillitems.isactive == true && (!bill.issynchronize || bill.updatestock))
            {
                if (oBillitems.id > 0)
                {
                    if (oBillitems.runtimeprice)
                    {
                        BuybackDataService buybackDataService = new BuybackDataService() { Context = this.Context };
                        buybackDataService.Sold(bill, oBillitems, conn, transaction);
                    }
                    if (!oBillitems.isexchange)
                    {
                        command = GetCommand(SQL_UpdateStock, conn, transaction);
                        command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.stockid;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salablegoodsid;
                        command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.journalqty > 0 && oBillitems.lastbitqty > 0 ?
                            (oBillitems.qty > 0 ? oBillitems.journalqty : oBillitems.journalqty * -1) : oBillitems.qty;
                        decimal shortagestock = 0;
                        bool nostockfound = false;
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var iqty = reader["iqty"] == DBNull.Value ? 0 : (decimal)reader["iqty"];
                                var oqty = reader["oqty"] == DBNull.Value ? 0 : (decimal)reader["oqty"];
                                var dqty = reader["dqty"] == DBNull.Value ? 0 : (decimal)reader["dqty"];
                                var jqty = reader["jqty"] == DBNull.Value ? 0 : (decimal)reader["jqty"];
                                var olqty = reader["olqty"] == DBNull.Value ? 0 : (decimal)reader["olqty"];
                                if (oBillitems.qty > 0 && oqty + dqty + jqty > iqty && locid > 0 &&
                                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locid) &&
                                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locid].sale_negativestock)
                                {
                                    shortagestock = oqty + dqty + jqty - iqty;
                                }
                                if (oBillitems.attributes == null) oBillitems.attributes = new BillItemAttributes();
                                oBillitems.attributes.newstock = (iqty - (oqty + dqty + jqty));
                                oBillitems.attributes.newolstock = olqty;
                                oBillitems.attributes.isolstock = olqty > 0;
                            }
                            else {
                                nostockfound = true;
                            }
                        }

                        if (nostockfound || shortagestock > 0)
                        {
                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                                !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan &&
                                oBillitems.attributes != null &&
                                !string.IsNullOrEmpty(oBillitems.attributes.serialno))
                            {
                                if (!nostockfound)
                                {
                                    command = GetCommand(SQL_UpdateStock, conn, transaction);
                                    command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.stockid;
                                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salablegoodsid;
                                    command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.qty * -1;
                                    command.ExecuteNonQuery();
                                }
                                string querystockuan = "select stock.id,salablegoods.id,salablegoods.barcode,iqty-(oqty+jqty+dqty) from stock " +
                                                       "inner join salablegoods on salablegoods.stockid = stock.id " +
                                                       "where stock.serialno = @serialno and iqty>(oqty+jqty+dqty) " +
                                                       "and salablegoods.locationid = @locationid " +
                                                       "order by iqty-(oqty+jqty+dqty) desc";
                                List<ConflictUAN> listofstock = new List<ConflictUAN>();
                                SortedList<string, object> param = new SortedList<string, object>();
                                param.Add("@serialno", oBillitems.attributes.serialno);
                                param.Add("@locationid", bill.locationid);
                                using (var reader = ExecuteReader(querystockuan, param, conn, transaction))
                                {
                                    while (reader.Read())
                                    {
                                        listofstock.Add(new ConflictUAN()
                                        {
                                            stockid = reader.GetInt64(0),
                                            salablegoodsid = reader.GetInt64(1),
                                            barcode = reader.GetString(2),
                                            qty = reader.GetDecimal(3)
                                        });

                                        if (listofstock.Sum(e => e.qty) >= shortagestock)
                                            break;
                                    }
                                }


                                if (listofstock.Sum(e => e.qty) < shortagestock &&
                                    !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportuannegative)
                                {
                                    throw new Exception(oBillitems.attributes.serialno + " item don't have required stock (" + (shortagestock).ToString() + ")");
                                }


                                bill.Billitems.Remove(oBillitems);
                                command = GetCommand("delete from billitems where id=@id", conn, transaction);
                                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.id;
                                command.ExecuteNonQuery();


                                if (listofstock.Sum(e => e.qty) < shortagestock &&
                                   InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportuannegative)
                                {
                                    listofstock.Add(new ConflictUAN()
                                    {
                                        barcode = oBillitems.barcode,
                                        qty = shortagestock - listofstock.Sum(e => e.qty),
                                        salablegoodsid = oBillitems.salablegoodsid,
                                        stockid = oBillitems.stockid
                                    });
                                }


                                var availableqty = oBillitems.qty;
                                var availablestocklist = listofstock.ToList();
                                var existingso = oBillitems.attributes.so;
                                oBillitems.attributes.so = new List<BillItemAttributes.itemqty>();
                                for (var itemindex = 0; availableqty > 0 && itemindex < availablestocklist.Count; itemindex++)
                                {
                                    Billitems newbillitem = Newtonsoft.Json.JsonConvert.DeserializeObject<Billitems>(Newtonsoft.Json.JsonConvert.SerializeObject(oBillitems));
                                    var newqty = availablestocklist[itemindex].qty > availableqty ? availableqty : availablestocklist[itemindex].qty;
                                    newbillitem.salablegoodsid = availablestocklist[itemindex].salablegoodsid;
                                    newbillitem.stockid = availablestocklist[itemindex].stockid;
                                    newbillitem.receivable = newbillitem.receivable / newbillitem.qty * newqty;
                                    //if (newbillitem.additionaldiscount > 0)
                                    //    newbillitem.additionaldiscount = newbillitem.additionaldiscount / newbillitem.qty * newqty;
                                    if (newbillitem.saleamount > 0)
                                        newbillitem.saleamount = newbillitem.saleamount / newbillitem.qty * newqty;
                                    if (newbillitem.schemediscount > 0)
                                        newbillitem.schemediscount = newbillitem.schemediscount / newbillitem.qty * newqty;
                                    if (newbillitem.tax > 0)
                                        newbillitem.tax = newbillitem.tax / newbillitem.qty * newqty;
                                    if (itemindex != 0)
                                    {
                                        newbillitem.schemediscountcoupen = 0;
                                        newbillitem.schemereward = 0;
                                    }

                                    newbillitem.qty = newqty;
                                    if (newbillitem.sellingmode == 2 && newbillitem.qty != newbillitem.piece)
                                        newbillitem.piece = newbillitem.qty;

                                    if (existingso != null && existingso.Count > 0)
                                    {
                                        newbillitem.attributes.so = GetSOItems(existingso, newqty);
                                    }
                                    InsertUANConflict(transaction, conn, newbillitem);
                                    availableqty -= newqty;
                                    bill.Billitems.Add(newbillitem);
                                }
                                if (availableqty > 0 && !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportuannegative)
                                {
                                    if (oBillitems.attributes != null &&
                                        !string.IsNullOrEmpty(oBillitems.attributes.serialno))
                                    {
                                        throw new Exception(
                                            (!string.IsNullOrEmpty(oBillitems.attributes.groupkey) ? oBillitems.attributes.groupkey + "-" : "") +
                                            oBillitems.attributes.serialno + " required additional (" + (availableqty).ToString() + " stock)");
                                    }
                                    else
                                    {
                                        throw new Exception(oBillitems.barcode + " required additional (" + (availableqty).ToString() + " stock)");
                                    }
                                }

                            }
                            else
                            {
                                if (oBillitems.attributes != null &&
                                        !string.IsNullOrEmpty(oBillitems.attributes.serialno))
                                {
                                    throw new Exception(oBillitems.attributes.serialno + " item don't have required stock (" + (shortagestock).ToString() + ")");
                                }
                                else
                                {
                                    throw new Exception(oBillitems.barcode + " item don't have required stock (" + (shortagestock).ToString() + ")");
                                }
                            }
                        }

                        if (bill != null && bill.billtype != Bill.BillTypes.Cancel && oBillitems.qty < 0)
                        {
                            command = GetCommand("Update salablegoods set salereturnqty=COALESCE(salereturnqty,0)+@salereturnqty where id=@id and stockid=@stockid", conn, transaction);
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salablegoodsid;
                            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.stockid;
                            command.Parameters.Add("salereturnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = Math.Abs(oBillitems.qty);
                            command.ExecuteNonQuery();
                        }
                        if (bill != null && bill.billtype == Bill.BillTypes.Cancel && oBillitems.qty > 0)
                        {
                            command = GetCommand("Update salablegoods set salereturnqty=COALESCE(salereturnqty,0)-@salereturnqty where id=@id and stockid=@stockid and COALESCE(salereturnqty,0)>0", conn, transaction);
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salablegoodsid;
                            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.stockid;
                            command.Parameters.Add("salereturnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = Math.Abs(oBillitems.qty);
                            command.ExecuteNonQuery();
                        }
                    }
                }
            }
            return oBillitems;
        }

        private List<BillItemAttributes.itemqty> GetSOItems(List<BillItemAttributes.itemqty> existingso, decimal newqty)
        {
            List<BillItemAttributes.itemqty> rlist = new List<BillItemAttributes.itemqty>();
            for (var i = 0; i < existingso.Count && newqty > 0; i++)
            {
                if (existingso[i].qty > 0)
                {
                    var aqty = existingso[i].qty > newqty ? newqty : existingso[i].qty;
                    rlist.Add(new BillItemAttributes.itemqty()
                    {
                        id = existingso[i].id,
                        qty = aqty
                    });
                    existingso[i].qty -= aqty;
                    newqty -= aqty;
                }
            }
            return rlist;
        }

        public void InsertUANConflict(NpgsqlTransaction transaction, NpgsqlConnection conn, Billitems oBillitems)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillitems.isactive;
            command.Parameters.Add("isexchange", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillitems.isexchange;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.productid;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillitems.version;
            command.Parameters.Add("sellingmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillitems.sellingmode;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillitems.barcode == null ? "" : oBillitems.barcode;
            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillitems.attributes == null ? "null" : oBillitems.attributes_json;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillitems.hsncode == null ? "" : oBillitems.hsncode;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.stockid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.locationid;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillitems.printingname == null ? "" : oBillitems.printingname;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.price;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.discount;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.additionaldiscount;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.receivable;
            command.Parameters.Add("salesmanid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salesmanid;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.tax;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.taxid;
            command.Parameters.Add("orderid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillitems.orderid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.companyid;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.billid;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salablegoodsid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.qty;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.rate;
            command.Parameters.Add("schemediscountcoupen", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.schemediscountcoupen;
            command.Parameters.Add("schemereward", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.schemereward;
            command.Parameters.Add("schemediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.schemediscount;
            command.Parameters.Add("schemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.schemeid;
            command.Parameters.Add("piece", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.piece;
            command.Parameters.Add("journalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.journalqty;
            command.Parameters.Add("itemtax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.itemtax;
            command.Parameters.Add("salerate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.salerate;
            command.Parameters.Add("saleamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.saleamount;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.taxpercentage;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.discountpercentage;
            command.Parameters.Add("additionaldiscountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.additionaldiscountpercentage;
            command.Parameters.Add("incentive", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.incentive;
            command.Parameters.Add("etprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.etprice;
            command.Parameters.Add("etdiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.etdiscount;
            command.Parameters.Add("returnbillitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.returnbillitemid;
            command.Parameters.Add("rtnsrcitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.rtnsrcitemid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            command = GetCommand(SQL_UpdateStock, conn, transaction);
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.stockid;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salablegoodsid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.qty;
            decimal shortagestock = 0;
            using (var reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    var iqty = reader["iqty"] == DBNull.Value ? 0 : (decimal)reader["iqty"];
                    var oqty = reader["oqty"] == DBNull.Value ? 0 : (decimal)reader["oqty"];
                    var dqty = reader["dqty"] == DBNull.Value ? 0 : (decimal)reader["dqty"];
                    var jqty = reader["jqty"] == DBNull.Value ? 0 : (decimal)reader["jqty"];
                    var olqty = reader["olqty"] == DBNull.Value ? 0 : (decimal)reader["olqty"];
                    if (oqty + dqty + jqty > iqty)
                    {
                        shortagestock = oqty + dqty + jqty - iqty;
                    }
                    oBillitems.attributes.newstock = (iqty - (oqty + dqty + jqty));
                    oBillitems.attributes.newolstock = olqty;
                    oBillitems.attributes.isolstock = olqty > 0;
                }
            }
            if (shortagestock > 0 && !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportuannegative)
            {
                throw new Exception(oBillitems.attributes.serialno + " item don't have required stock (" + (shortagestock).ToString() + ")");
            }
        }

        /*with updateattribe as
        (
        select billitems.id,
        (jsonb_set(attributes::jsonb, '{size}'::text[], to_jsonb(referencelist.name::text), false))::json as nattributes from billitems
        inner join stock on stock.id=stockid
        inner join items on items.id = itemid
        inner join referencelist on referencelist.id = sizeid
        where billid in (
        select id from bill where billno='SG-31325')
        and attributes->>'size' != referencelist.name
        )
        update billitems set attributes=nattributes from updateattribe
        where billitems.id = updateattribe.id
        */

        public bool IsMigration = false;

        public Billitems Update(Bill bill, Billitems oBillitems, Billitems oExistingBillitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, bill, oBillitems, oExistingBillitems);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

            return oBillitems;
        }

        public void UpdateMRP(List<Billitems> lst)
        {
            string query = @"select stock.id,displaymrp,designid,
                            style.name stylename,pattern.name patternname,size.name sizename, 
                            colour.name colourname,type.name typename,material.name materialname,
                            brand.name brandname 
                                from stock 
                                inner join items on items.id = stock.itemid
                                left outer join referencelist on referencelist.id = items.sizeid
                                    left outer join products on products.id = items.productid
                                    left outer join brand brand on brand.id = items.brandid
                                    left outer join referencelist colour on colour.id = colourid
                                    left outer join referencelist size on size.id = sizeid
                                    left outer join referencelist style on style.id = styleid
                                    left outer join referencelist pattern on pattern.id = patternid
                                    left outer join referencelist material on material.id = materialid
                                    left outer join referencelist type on type.id = typeid
                    where stock.id in (" + string.Join(",", lst.ConvertAll<long>(e => e.stockid)) + ")";
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    long stockid = reader.GetInt64(0);
                    decimal displaymrp = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                    string designno = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    string stylename = reader.IsDBNull(3) ? "" : reader.GetString(3);
                    string patternname = reader.IsDBNull(4) ? "" : reader.GetString(4);
                    string sizename = reader.IsDBNull(5) ? "" : reader.GetString(5);
                    string colourname = reader.IsDBNull(6) ? "" : reader.GetString(6);
                    string typename = reader.IsDBNull(7) ? "" : reader.GetString(7);
                    string materialname = reader.IsDBNull(8) ? "" : reader.GetString(8);
                    string brand = reader.IsDBNull(9) ? "" : reader.GetString(9);
                    lst.ForEach(e =>
                    {
                        if (e.stockid == stockid)
                            e.mrp = displaymrp;
                        if (e.attributes != null && e.stockid == stockid && string.IsNullOrEmpty(e.attributes.designid))
                            e.attributes.designid = designno;
                        if (e.attributes != null && e.stockid == stockid && string.IsNullOrEmpty(e.attributes.size))
                            e.attributes.size = sizename;
                        if (e.attributes != null && e.stockid == stockid && string.IsNullOrEmpty(e.attributes.brand))
                            e.attributes.brand = brand;
                        if (e.attributes != null && e.stockid == stockid && string.IsNullOrEmpty(e.attributes.typename))
                            e.attributes.typename = typename;
                        if (e.attributes != null && e.stockid == stockid && string.IsNullOrEmpty(e.attributes.materialname))
                            e.attributes.materialname = materialname;
                        if (e.attributes != null && e.stockid == stockid && string.IsNullOrEmpty(e.attributes.stylename))
                            e.attributes.stylename = stylename;
                        if (e.attributes != null && e.stockid == stockid && string.IsNullOrEmpty(e.attributes.colourname))
                            e.attributes.colourname = colourname;
                        if (e.attributes != null && e.stockid == stockid && string.IsNullOrEmpty(e.attributes.patternname))
                            e.attributes.patternname = patternname;
                    });
                }
            }
        }

        public void DirectUpdate(Billitems oBillitems)
        {
            NpgsqlTransaction transaction = null;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand("Update billitems set attributes=@attributes where id=@id", conn, transaction);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.id;
                command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillitems.attributes == null ? "null" : oBillitems.attributes_json;
                command.ExecuteNonQuery();
                conn.Close();
            }
        }
        public void DirectUpdate(List<Billitems> oSalesorderitems)
        {
            NpgsqlTransaction transaction = null;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                transaction = conn.BeginTransaction();

                foreach (var oBillitems in oSalesorderitems)
                {
                    if (oBillitems.attributes != null &&
                oBillitems.attributes.so != null &&
                oBillitems.attributes.so.Count > 0 &&
                oBillitems.attributes.so.Sum(x => x.qty) > oBillitems.qty)
                    {
                        var available = oBillitems.attributes.so.ToList();
                        oBillitems.attributes.so.Clear();
                        decimal allocated = 0;
                        foreach (var soitem in available.OrderByDescending(x => x.qty))
                        {
                            if (soitem.qty < (oBillitems.qty - allocated))
                            {
                                oBillitems.attributes.so.Add(soitem);
                            }
                            else
                            {
                                soitem.qty = oBillitems.qty - allocated;
                                oBillitems.attributes.so.Add(soitem);
                                break;
                            }
                        }
                    }
                    NpgsqlCommand command = GetCommand("Update billitems set attributes=@attributes where id=@id", conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.id;
                    command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillitems.attributes == null ? "null" : oBillitems.attributes_json;
                    command.ExecuteNonQuery();
                }
                transaction.Commit();
                conn.Close();
            }
        }
        public Billitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn,
            Bill bill,
            Billitems oBillitems,
            Billitems oExistingBillitems)
        {
            if (!IsMigration)
            {
                if (oBillitems.schemeid == 0 && oBillitems.schemediscount > 0)
                    oBillitems.schemediscount = 0;

                if (oBillitems.sellingmode == 2 && oBillitems.qty != oBillitems.piece)
                    oBillitems.piece = oBillitems.qty;

                if (oBillitems.receivable < 0 && oBillitems.piece > 0)
                    oBillitems.piece = oBillitems.piece * -1;

                if (oBillitems.receivable > 0 && oBillitems.piece < 0)
                    oBillitems.piece = oBillitems.piece * -1;

                if (oBillitems.taxid == 0)
                    throw new Exception("Tax information missing for " + oBillitems.barcode + "(" + oBillitems.printingname + ")");

                if (oBillitems.taxpercentage > 0 &&
                    oBillitems.receivable != 0 &&
                    oBillitems.tax == 0)
                    throw new Exception("Tax computation missing for " + oBillitems.barcode + "(" + oBillitems.printingname + ")");

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5 &&
                    oBillitems.taxpercentage > 0)
                {
                    var projtax = oBillitems.receivable / (100 + oBillitems.taxpercentage) * oBillitems.taxpercentage;
                    var projitax = projtax / oBillitems.qty;
                    if (Math.Abs(Math.Abs(projtax) - Math.Abs(oBillitems.tax)) > (decimal)0.2)
                    {
                        throw new Exception(oBillitems.barcode + "(" + oBillitems.attributes.serialno + ") contains some tax missmatches");
                    }
                }

                if (oBillitems.attributes != null &&
                    oBillitems.attributes.so != null &&
                    oBillitems.attributes.so.Count > 0 &&
                    oBillitems.attributes.so.Sum(x => x.qty) < oBillitems.qty)
                {
                    var available = oBillitems.attributes.so.ToList();
                    oBillitems.attributes.so.Clear();
                    decimal allocated = 0;
                    foreach (var soitem in available)
                    {
                        if (soitem.qty < (oBillitems.qty - allocated))
                        {
                            oBillitems.attributes.so.Add(soitem);
                        }
                        else
                        {
                            soitem.qty = oBillitems.qty - allocated;
                            oBillitems.attributes.so.Add(soitem);
                            break;
                        }
                    }
                }
            }

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillitems.createdon;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.productid;
            command.Parameters.Add("sellingmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillitems.sellingmode;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillitems.hsncode == null ? "" : oBillitems.hsncode;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillitems.barcode == null ? "" : oBillitems.barcode;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.stockid;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillitems.printingname == null ? "" : oBillitems.printingname;
            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillitems.attributes == null ? "null" : oBillitems.attributes_json;
            command.Parameters.Add("price", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.price;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.discount;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.additionaldiscount;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.receivable;
            command.Parameters.Add("salesmanid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salesmanid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.locationid;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.tax;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.taxid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.companyid;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.billid;
            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salablegoodsid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.qty;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.rate;
            command.Parameters.Add("piece", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.piece;
            command.Parameters.Add("journalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.journalqty;
            command.Parameters.Add("itemtax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.itemtax;
            command.Parameters.Add("salerate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.salerate;
            command.Parameters.Add("saleamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.saleamount;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.taxpercentage;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.discountpercentage;
            command.Parameters.Add("additionaldiscountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.additionaldiscountpercentage;
            command.Parameters.Add("schemediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.schemediscount;
            command.Parameters.Add("schemediscountcoupen", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.schemediscountcoupen;
            command.Parameters.Add("schemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.schemeid;
            command.Parameters.Add("schemereward", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.schemereward;
            command.Parameters.Add("etprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.etprice;
            command.Parameters.Add("etdiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.etdiscount;

            bool dataupdated = false;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillitems.version = Convert.ToInt32(reader[0]);
                    dataupdated = true;
                }
            }

            if (!IsMigration && dataupdated && oExistingBillitems != null && (oBillitems.qty - oExistingBillitems.qty) != 0)
            {
                //command = GetCommand(SQL_UpdateStock, conn, transaction);
                //command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.stockid;
                //command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salablegoodsid;
                //command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.qty - oExistingBillitems.qty;
                //command.ExecuteNonQuery();

                if (oExistingBillitems.attributes != null &&
                    oExistingBillitems.attributes.so != null &&
                    oExistingBillitems.attributes.so.Count > 0)
                {
                    command = GetCommand("Update Salesorderitems set bqty=bqty-@bqty,pqty=pqty+@bqty where id=@id", conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    command.Parameters.Add("bqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                    foreach (var soitem in oExistingBillitems.attributes.so.GroupBy(e => e.id))
                    {
                        command.Parameters["id"].Value = soitem.Key;
                        command.Parameters["bqty"].Value = soitem.Sum(e => e.qty);
                        command.ExecuteNonQuery();
                    }
                }

                if (!IsMigration && oBillitems.qty > 0)
                {
                    if (oBillitems.attributes != null &&
                        oBillitems.attributes.so != null &&
                        oBillitems.attributes.so.Count > 0)
                    {
                        command = GetCommand("Update Salesorderitems set bqty=bqty+@bqty,pqty=(case when pqty>=@bqty then pqty-@bqty else 0 end) where id=@id", conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                        command.Parameters.Add("bqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = 0;
                        foreach (var soitem in oBillitems.attributes.so.GroupBy(e => e.id))
                        {
                            command.Parameters["id"].Value = soitem.Key;
                            command.Parameters["bqty"].Value = Math.Abs(soitem.Sum(e => e.qty));
                            command.ExecuteNonQuery();
                        }
                    }
                }


                if (oBillitems.id > 0 && !IsMigration)
                {
                    if (!oBillitems.isexchange)
                    {
                        command = GetCommand(SQL_UpdateStock, conn, transaction);
                        command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oExistingBillitems.stockid;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oExistingBillitems.salablegoodsid;
                        command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = (oExistingBillitems.journalqty > 0 && oExistingBillitems.lastbitqty > 0 ?
                            (oExistingBillitems.qty > 0 ? oExistingBillitems.journalqty : oExistingBillitems.journalqty * -1) : oExistingBillitems.qty) * -1;
                        command.ExecuteNonQuery();

                        command = GetCommand(SQL_UpdateStock, conn, transaction);
                        command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.stockid;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salablegoodsid;
                        command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.journalqty > 0 && oBillitems.lastbitqty > 0 ?
                            (oBillitems.qty > 0 ? oBillitems.journalqty : oBillitems.journalqty * -1) : oBillitems.qty;
                        decimal shortagestock = 0;
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var iqty = reader["iqty"] == DBNull.Value ? 0 : (decimal)reader["iqty"];
                                var oqty = reader["oqty"] == DBNull.Value ? 0 : (decimal)reader["oqty"];
                                var dqty = reader["dqty"] == DBNull.Value ? 0 : (decimal)reader["dqty"];
                                var jqty = reader["jqty"] == DBNull.Value ? 0 : (decimal)reader["jqty"];
                                if (oBillitems.qty > 0 && oqty + dqty + jqty > iqty && bill.locationid > 0 &&
                                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(bill.locationid) &&
                                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[bill.locationid].sale_negativestock)
                                {
                                    shortagestock = oqty + dqty + jqty - iqty;
                                }
                            }
                        }

                        if (shortagestock > 0)
                        {
                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                                !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan &&
                                oBillitems.attributes != null &&
                                !string.IsNullOrEmpty(oBillitems.attributes.serialno))
                            {
                                shortagestock = oBillitems.qty;
                                command = GetCommand(SQL_UpdateStock, conn, transaction);
                                command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.stockid;
                                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salablegoodsid;
                                command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.qty * -1;
                                command.ExecuteNonQuery();

                                string querystockuan = "select stock.id,salablegoods.id,salablegoods.barcode,iqty-(oqty+jqty+dqty) from stock " +
                                                       "inner join salablegoods on salablegoods.stockid = stock.id " +
                                                       "where stock.serialno = @serialno and iqty>(oqty+jqty+dqty) " +
                                                       "and salablegoods.locationid = @locationid " +
                                                       "order by iqty-(oqty+jqty+dqty) desc";
                                List<ConflictUAN> listofstock = new List<ConflictUAN>();
                                SortedList<string, object> param = new SortedList<string, object>();
                                param.Add("@serialno", oBillitems.attributes.serialno);
                                param.Add("@locationid", bill.locationid);
                                using (var reader = ExecuteReader(querystockuan, param, conn, transaction))
                                {
                                    while (reader.Read())
                                    {
                                        listofstock.Add(new ConflictUAN()
                                        {
                                            stockid = reader.GetInt64(0),
                                            salablegoodsid = reader.GetInt64(1),
                                            barcode = reader.GetString(2),
                                            qty = reader.GetDecimal(3)
                                        });

                                        if (listofstock.Sum(e => e.qty) >= shortagestock)
                                            break;
                                    }
                                }


                                if (listofstock.Sum(e => e.qty) < shortagestock &&
                                    !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportuannegative)
                                {
                                    throw new Exception(oBillitems.attributes.serialno + " item don't have required stock (" + (shortagestock).ToString() + ")");
                                }


                                bill.Billitems.Remove(oBillitems);
                                command = GetCommand("delete from billitems where id=@id", conn, transaction);
                                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.id;
                                command.ExecuteNonQuery();


                                if (listofstock.Sum(e => e.qty) < shortagestock &&
                                   InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportuannegative)
                                {
                                    listofstock.Add(new ConflictUAN()
                                    {
                                        barcode = oBillitems.barcode,
                                        qty = shortagestock - listofstock.Sum(e => e.qty),
                                        salablegoodsid = oBillitems.salablegoodsid,
                                        stockid = oBillitems.stockid
                                    });
                                }


                                var availableqty = oBillitems.qty;
                                var availablestocklist = listofstock.ToList();
                                var existingso = oBillitems.attributes.so;
                                oBillitems.attributes.so = new List<BillItemAttributes.itemqty>();
                                for (var itemindex = 0; availableqty > 0 && itemindex < availablestocklist.Count; itemindex++)
                                {
                                    Billitems newbillitem = Newtonsoft.Json.JsonConvert.DeserializeObject<Billitems>(Newtonsoft.Json.JsonConvert.SerializeObject(oBillitems));
                                    var newqty = availablestocklist[itemindex].qty > availableqty ? availableqty : availablestocklist[itemindex].qty;
                                    newbillitem.salablegoodsid = availablestocklist[itemindex].salablegoodsid;
                                    newbillitem.stockid = availablestocklist[itemindex].stockid;
                                    newbillitem.receivable = newbillitem.receivable / newbillitem.qty * newqty;
                                    if (newbillitem.additionaldiscount > 0)
                                        newbillitem.additionaldiscount = newbillitem.additionaldiscount / newbillitem.qty * newqty;
                                    if (newbillitem.saleamount > 0)
                                        newbillitem.saleamount = newbillitem.saleamount / newbillitem.qty * newqty;
                                    if (newbillitem.schemediscount > 0)
                                        newbillitem.schemediscount = newbillitem.schemediscount / newbillitem.qty * newqty;
                                    if (newbillitem.tax > 0)
                                        newbillitem.tax = newbillitem.tax / newbillitem.qty * newqty;
                                    if (itemindex != 0)
                                    {
                                        newbillitem.schemediscountcoupen = 0;
                                        newbillitem.schemereward = 0;
                                    }
                                    newbillitem.qty = newqty;
                                    if (existingso != null && existingso.Count > 0)
                                    {
                                        newbillitem.attributes.so = GetSOItems(existingso, newqty);
                                    }
                                    InsertUANConflict(transaction, conn, newbillitem);
                                    availableqty -= newqty;
                                    bill.Billitems.Add(newbillitem);
                                }

                            }
                            else
                            {
                                throw new Exception(oBillitems.barcode + " item don't have required stock (" + (shortagestock).ToString() + ")");
                            }
                        }
                    }
                }

            }
            return oBillitems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill bill, List<Billitems> list, long id, long locationid)
        {

            var existingItems = SearchAll(id, false, false);
            if (existingItems != null && existingItems.Count > 0)
            {
                var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                foreach (var item in deletedItems)
                    Delete(transaction, conn, item);
            }

            foreach (var item in list.ToArray())
            {
                if (item.id > 0 && existingItems.Exists(x => x.id == item.id))
                {
                    Update(transaction, conn, bill, item, existingItems.Find(e => e.id == item.id));
                }
                else
                {
                    item.billid = id;
                    Insert(transaction, conn, bill, item, locationid);
                }
            }
            StocktransactionDataService stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
            stocktransactionDataService.UpdateBillItems(bill, transaction, conn);
            return true;
        }

        public bool Delete(Billitems oBillitems)
        {

            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {

                using (NpgsqlConnection conn = CreateConnection())
                {

                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Delete(transaction, conn, oBillitems);


                    transaction.Commit();
                    conn.Close();

                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return successCount > 0;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Billitems oBillitems)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillitems.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillitems.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.id;
            int successCount = command.ExecuteNonQuery();
            if (successCount > 0)
            {
                command = GetCommand(SQL_UpdateStock, conn, transaction);
                command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.stockid;
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salablegoodsid;
                command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.qty * -1;
                command.ExecuteNonQuery();
            }
            return successCount > 0;
        }
        public bool OffSetSaleQty(NpgsqlTransaction transaction, NpgsqlConnection conn, Billitems oBillitems, Bill.BillTypes otype)
        {
            NpgsqlCommand command = GetCommand(SQL_UpdateStock, conn, transaction);
            command = GetCommand(SQL_UpdateStock, conn, transaction);
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.stockid;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salablegoodsid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = (otype == Bill.BillTypes.ReturnBill || oBillitems.qty > 0) ? oBillitems.qty * -1 : 0;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }
        public bool DeleteAll(long billid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            int successCount = 0;
            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = billid;
            successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

    }

}
