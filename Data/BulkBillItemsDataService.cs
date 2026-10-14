using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class BulkBillItemsDataService : SAASData
    {
        const string SQL_Search = @"select bulkbillitems.id, bulkbillitems.isactive, bulkbillitems.createdby, bulkbillitems.modifiedby, bulkbillitems.version,bulkbillitems.locationid, 
                                    bulkbillitems.createdon, bulkbillitems.modifiedon, barcode, stockid,salablegoodsid, printingname, employee.employeecode as salesmancode,
                                    price, discount,additionaldiscount, receivable, salesmanid, tax, taxid, bulkbillitems.companyid, billid, qty, rate,employee.name as salesmanname,
                                    piece,journalqty,itemtax,salerate,saleamount,taxpercentage,discountpercentage,additionaldiscountpercentage,hsncode,bulkbillitems.orderid,etprice,etdiscount,bulkbillitems.attributes,schemediscount,schemeid
                                    from bulkbillitems 
                                    left outer join employee on employee.id = salesmanid
                                    where bulkbillitems.id=@id
                                    order by bulkbillitems.orderid";

        const string SQL_ItemSearch = @"select referencelist.name as location,bulkbillitems.barcode,customer.name customer,
                                    bulkbillitems.printingname, employee.employeecode as salesmancode,items.brandid,stock.designid,
                                    bulkbillitems.price, bulkbillitems.discount,bulkbillitems.additionaldiscount, stock.imageid as stockimageid,
                                    bulkbillitems.receivable, bulkbillitems.tax, bulkbillitems.qty, rate,employee.name as salesmanname,
                                    piece,bulkbillitems.journalqty,bulkbillitems.hsncode,bulkbillitems.id,
                                    bulkbill.billno,bulkbill.billdate,bulkbill.receivable+bulkbill.rounding billvalue,
                                    bulkbillitems.etprice,bulkbillitems.etdiscount
                                    from bulkbillitems 
                                    inner join bulkbill on bulkbill.id = bulkbillitems.billid
                                    inner join referencelist on referencelist.id = bulkbill.locationid
                                    inner join salablegoods on salablegoods.id = bulkbillitems.salablegoodsid
				                    inner join stock on stock.id = salablegoods.stockid
				                    left outer join items on items.id = stock.itemid
                                    left outer join employee on employee.id = salesmanid
                                    left outer join customer on customer.id = bulkbill.customerid";

        const string SQL_SearchAll = @"select bulkbillitems.id, bulkbillitems.isactive, bulkbillitems.createdby, bulkbillitems.modifiedby, bulkbillitems.version,bulkbillitems.locationid,
                                    bulkbillitems.createdon, bulkbillitems.modifiedon, barcode, stockid,salablegoodsid, printingname, employee.employeecode as salesmancode,etprice,etdiscount,
                                    price, discount,additionaldiscount, receivable, salesmanid, tax, taxid, bulkbillitems.companyid, billid, qty, rate,employee.name as salesmanname,
                                    piece,journalqty,itemtax,salerate,saleamount,taxpercentage,discountpercentage,additionaldiscountpercentage,hsncode,bulkbillitems.orderid,billedqty,journalqty,attributes,schemediscount,schemeid
                                    from bulkbillitems 
                                    left outer join employee on employee.id = salesmanid
                                    where billid=@billid
                                    order by bulkbillitems.orderid";

        const string SQL_SearchAllWithTax = @"select bulkbillitems.id, bulkbillitems.isactive, bulkbillitems.createdby, bulkbillitems.modifiedby, bulkbillitems.version,bulkbillitems.locationid, 
                                    bulkbillitems.createdon, bulkbillitems.modifiedon, barcode, stockid,salablegoodsid, printingname, employee.employeecode as salesmancode,
                                    price, discount,additionaldiscount, receivable, salesmanid, tax, taxid, bulkbillitems.companyid, 
                                    billid, qty, rate,employee.employeecode as salesmanname,taxsplit,tax.name as taxname,
                                    piece, journalqty,itemtax,salerate,saleamount,bulkbillitems.taxpercentage,hsncode,etprice,etdiscount,
                                    discountpercentage,additionaldiscountpercentage, bulkbillitems.orderid,billedqty,journalqty,bulkbillitems.attributes,schemediscount,schemeid
                                    from bulkbillitems 
                                    left outer join tax on tax.id = taxid
                                    left outer join employee on employee.id = salesmanid
                                    where bulkbillitems.billid=@billid
                                    order by bulkbillitems.orderid";


        const string SQL_Insert = @"
        update salablegoods set aqty=aqty+@qty where id=@salablegoodsid;
INSERT INTO public.bulkbillitems(isactive, createdby, modifiedby, version, createdon, modifiedon, barcode, stockid,salablegoodsid, printingname, price, discount,additionaldiscount,
receivable, salesmanid, tax, taxid, companyid, billid, qty, rate,locationid,orderid,piece,journalqty,itemtax,salerate,saleamount,taxpercentage,discountpercentage,additionaldiscountpercentage,hsncode,billedqty,etprice,etdiscount,attributes,schemediscount, schemeid) 
VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @barcode, @stockid,@salablegoodsid, @printingname, @price,
@discount,@additionaldiscount, @receivable, @salesmanid, @tax, @taxid, @companyid, @billid, @qty, @rate,@locationid,@orderid,@piece,@journalqty,@itemtax,@salerate,@saleamount,@taxpercentage,@discountpercentage,
@additionaldiscountpercentage,@hsncode,0,@etprice,@etdiscount,@attributes,@schemediscount,@schemeid) RETURNING id";

        const string SQL_UpdateBilledQty = @"
        update salablegoods set aqty=aqty-(@qty+@journalqty) where id=@salablegoodsid;
        update bulkbillitems set billedqty=billedqty+@qty,journalqty=journalqty+@journalqty,
        modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and salablegoodsid=@salablegoodsid RETURNING billedqty,qty,journalqty";

        const string SQL_UpdateReturnQty = @"
        update salablegoods set aqty=aqty-@journalqty where id=@salablegoodsid;
        update bulkbillitems set journalqty=journalqty+@journalqty,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and salablegoodsid=@salablegoodsid RETURNING billedqty,qty,journalqty";

        const string SQL_UpdateBillStatus = @"update bulkbill set billtype=11,returnbillno=@returnbillno,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id;";

        const string SQL_Update = @"UPDATE public.bulkbillitems SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, barcode=@barcode,locationid=@locationid, stockid=@stockid,salablegoodsid=@salablegoodsid,
        printingname=@printingname, price=@price, discount=@discount,additionaldiscount=@additionaldiscount, receivable=@receivable, salesmanid=@salesmanid, tax=@tax, taxid=@taxid,hsncode=@hsncode,
        companyid=@companyid, billid=@billid, qty=@qty, rate=@rate,etprice=@etprice,etdiscount=@etdiscount,attributes=@attributes,schemediscount=@schemediscount,schemeid=@schemeid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"
        update salablegoods set aqty=aqty-@journalqty from bulkbillitems
        where id=bulkbillitems.salablegoodsid and bulkbillitems.id=@id;
        update public.bulkbillitems set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        const string SQL_DeleteAll = @"
        update salablegoods set aqty=aqty-@journalqty from bulkbillitems
        where id=bulkbillitems.salablegoodsid and bulkbillitems.billid=@billid;
        update public.bulkbillitems set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where billid=@billid";
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
                    qb.AddParameter("isactive", true, "=");

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
                        obillitems.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obillitems.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obillitems.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obillitems.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        obillitems.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obillitems.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obillitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        obillitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        obillitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        obillitems.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        obillitems.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        obillitems.stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"].ToString());
                        obillitems.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"].ToString());
                        obillitems.printingname = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        obillitems.salesmancode = reader["salesmancode"] == DBNull.Value ? "" : reader["salesmancode"].ToString();
                        obillitems.salesmanname = reader["salesmanname"] == DBNull.Value ? "" : reader["salesmanname"].ToString();
                        obillitems.salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"].ToString());
                        obillitems.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"].ToString());
                        obillitems.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        obillitems.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"].ToString());
                        obillitems.salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"].ToString());
                        //piece,discountrate,additionaldiscountrate,taxrate,journalqty,itemtax,salerate,saleamount,billitems.taxpercentage,discountpercentage,additionaldiscountpercentage
                        obillitems.piece = reader["piece"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["piece"]);
                        obillitems.journalqty = reader["journalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["journalqty"]);
                        obillitems.itemtax = reader["itemtax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itemtax"]);
                        obillitems.salerate = reader["salerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salerate"]);
                        obillitems.saleamount = reader["saleamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleamount"]);
                        obillitems.etdiscount = reader["etdiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["etdiscount"]);
                        obillitems.etprice= reader["etprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["etprice"]);
                        obillitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        obillitems.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"]);
                        obillitems.additionaldiscountpercentage = reader["additionaldiscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscountpercentage"]);
                        obillitems.orderid = reader["orderid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["orderid"].ToString());
                        obillitems.attributes_json = reader["attributes"] == DBNull.Value ? "" : reader["attributes"].ToString();
                        obillitems.schemediscount = reader["schemediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemediscount"]);
                        obillitems.schemeid = reader["schemeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["schemeid"]);

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

        public List<Billitems> SearchAll(long _billid, 
            bool includetaxinformation,
            bool isinterstate,
            bool removecompleted)
        {
            List<Billitems> lst = new List<Billitems>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(includetaxinformation ? SQL_SearchAllWithTax : SQL_SearchAll, conn);
                command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Billitems obillitems = new Billitems();
                        obillitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
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
                        obillitems.salesmanname = reader["salesmanname"] == DBNull.Value ? "" : reader["salesmanname"].ToString();
                        obillitems.salesmancode = reader["salesmancode"] == DBNull.Value ? "" : reader["salesmancode"].ToString();
                        obillitems.salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"].ToString());
                        obillitems.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        obillitems.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        obillitems.piece = reader["piece"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["piece"]);
                        obillitems.journalqty = reader["journalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["journalqty"]);
                        obillitems.billedqty = reader["billedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billedqty"]);
                        obillitems.itemtax = reader["itemtax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["itemtax"]);
                        obillitems.salerate = reader["salerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salerate"]);
                        obillitems.saleamount = reader["saleamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleamount"]);
                        obillitems.etdiscount = reader["etdiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["etdiscount"]);
                        obillitems.etprice = reader["etprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["etprice"]);
                        obillitems.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        obillitems.discountpercentage = reader["discountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountpercentage"]);
                        obillitems.additionaldiscountpercentage = reader["additionaldiscountpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscountpercentage"]);
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
                        obillitems.orderid = reader["orderid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["orderid"].ToString());
                        obillitems.attributes_json = reader["attributes"] == DBNull.Value ? "" : reader["attributes"].ToString();
                        obillitems.schemediscount = reader["schemediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemediscount"]);
                        obillitems.schemeid = reader["schemeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["schemeid"]);

                        if (removecompleted)
                        {
                            var journal = reader["journalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["journalqty"]);
                            var billed = reader["billedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billedqty"]);
                            var newqty = obillitems.qty -(journal + billed);
                            if (newqty == 0)
                            {
                                obillitems.receivable = 0;
                                obillitems.tax = 0;
                                obillitems.qty = 0;
                                obillitems.piece = 0;
                            }
                            else if (newqty > 0)    // Added this condition to avoid this process for Estimate bill
                            {
                                obillitems.receivable = obillitems.receivable / obillitems.qty * newqty;
                                obillitems.tax = obillitems.tax / obillitems.qty * newqty;
                                obillitems.qty = newqty;
                                if (journal > 0 || billed > 0)
                                    obillitems.piece = newqty;
                                // Removed 
                                obillitems.journalqty = 0;
                            }
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
                                var taxconfig = taxsplit.Find(obillitems.price - obillitems.additionaldiscount);
                                if (taxconfig != null)
                                {
                                    obillitems.taxsplit.sgsttaxpercentage = isinterstate ? taxconfig.igst : taxconfig.sgst;
                                    obillitems.taxsplit.cgsttaxpercentage = taxconfig.cgst;
                                    if (obillitems.taxsplit.cgsttaxpercentage + obillitems.taxsplit.sgsttaxpercentage > 0)
                                    {
                                        if (isinterstate)
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
                                    if (isinterstate)
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
                        }
                        lst.Add(obillitems);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
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
                qb.AddParameter("bulkbill.isactive", true, "=");
                qb.AddParameter("bulkbill.billtype", 9, "!=");


                if (!string.IsNullOrEmpty(creteria.barcode))
                    qb.AddParameter("bulkbillitems.barcode", creteria.barcode, "=");
                if (creteria.barcodes != null && creteria.barcodes.Count > 0)
                    qb.AddInParamer("bulkbillitems.barcode", string.Join(",", creteria.barcodes.ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e) + "'")));


                if (creteria.locationid > 0)
                    qb.AddParameter("bulkbill.locationid", creteria.locationid, "=");
                if (creteria.companyid > 0)
                    qb.AddParameter("bulkbill.companyid", creteria.companyid, "=");
                if (creteria.customerid > 0)
                    qb.AddParameter("bulkbill.customerid", creteria.customerid, "=");
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
                    qb.AddParameter("bulkbill.billdate", "billdatefrom", creteria.fromdate.Date, ">=");
                }
                if (creteria.todate != DateTime.MinValue)
                {
                    qb.AddParameter("bulkbill.billdate", "billdateto", creteria.todate, "<");
                }
                if (creteria.billamtfrom > 0)
                {
                    qb.CurrentQuery += " and bulkbill.receivable>=" + creteria.billamtfrom + "";
                    //qb.AddParameter("billitems.receivable", "receivablefrom", creteria.billamtfrom, ">=");
                }
                if (creteria.billamtto > 0)
                {
                    qb.CurrentQuery += " and bulkbill.receivable<=" + creteria.billamtto + "";
                    //qb.AddParameter("billitems.receivable", "receivableto", creteria.billamtto, "<=");
                }
                qb.SortField = "bulkbillitems.id desc";
                qb.Limit = 10000;
                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Billitemdetails obillitems = new Billitemdetails();
                        obillitems.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obillitems.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        obillitems.customer = reader["customer"] == DBNull.Value ? "" : reader["customer"].ToString();
                        obillitems.location = reader["location"] == DBNull.Value ? "" : reader["location"].ToString();
                        obillitems.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
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

        public bool SyncBillQty(Bill sourceBill, 
            Bill newbill,
            NpgsqlTransaction transaction,
            NpgsqlConnection conn)
        {
            List<Billitems> billitems = new List<Billitems>();
            List<Billitems> items = newbill.Billitems;
            bool isvalid = true;
            Billitems billeditem = null;
            foreach (var item in sourceBill.Billitems)
            {
                decimal journalqty = item.qty;
                if (!newbill.isbulkreturngoods)
                    journalqty = 0;
                if (items.Exists(e => e.salablegoodsid == item.salablegoodsid))
                {
                    billeditem = items.Find(e => e.salablegoodsid == item.salablegoodsid);
                    
                    if (newbill.isbulkreturngoods)
                        journalqty = item.qty - billeditem.qty;

                    NpgsqlCommand command = GetCommand(SQL_UpdateBilledQty, conn, transaction);
                    command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.salablegoodsid;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = billeditem.qty;
                    command.Parameters.Add("journalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = journalqty;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            var billedqty = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                            var qty = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                            var journal = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                            if (qty - (billedqty + journal) < 0)
                            {
                                isvalid = false;
                            }
                        }
                    }
                }

                if (journalqty > 0 && !InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.system_disablebulkreturnonsale)
                {
                    var newbillitem = Newtonsoft.Json.JsonConvert.DeserializeObject<Billitems>(Newtonsoft.Json.JsonConvert.SerializeObject(item));
                    newbillitem.qty = journalqty * -1;
                    newbillitem.saleamount = newbillitem.saleamount * -1;
                    newbillitem.piece = item.qty == item.piece ? (newbillitem.qty*-1) : item.piece;
                    newbillitem.tax = newbillitem.tax / (newbillitem.qty * -1) * -1;
                    newbillitem.receivable = newbillitem.receivable / (newbillitem.qty * -1) * -1;
                    billitems.Add(newbillitem);
                }

                if(billeditem != null && billeditem.qty>0)
                {
                    var newbillitem = Newtonsoft.Json.JsonConvert.DeserializeObject<Billitems>(Newtonsoft.Json.JsonConvert.SerializeObject(item));
                    newbillitem.qty = billeditem.qty;
                    newbillitem.saleamount = 0;
                    newbillitem.piece = item.qty == item.piece ? (newbillitem.qty) : item.piece;
                    newbillitem.tax = 0;
                    newbillitem.receivable = 0;
                    newbillitem.price = 0;
                    newbillitem.taxpercentage = 0;
                    newbillitem.rate = 0;
                    billitems.Add(newbillitem);
                }
                billeditem = null;
            }

            if (isvalid)
            {
                var newbulkbill = Newtonsoft.Json.JsonConvert.DeserializeObject<Bill>(Newtonsoft.Json.JsonConvert.SerializeObject(sourceBill));
                if (billitems != null && billitems.Count > 0)
                {

                    newbulkbill.billtype = Bill.BillTypes.BulkSaleReturn;
                    newbulkbill.id = 0;
                    newbulkbill.billno = '~'+newbill.billno;
                    newbulkbill.billmasterid = newbill.billmasterid;
                    newbulkbill.Billitems.Clear();
                    newbulkbill.Billitems.AddRange(billitems);
                    newbulkbill.RecalculateTax();
                    newbulkbill.receivable = newbulkbill.Billitems.Sum(e => e.receivable);
                    newbulkbill.tax = newbulkbill.Billitems.Sum(e => e.tax);
                    newbulkbill.qty = newbulkbill.Billitems.Sum(e => e.qty>0 ? 0 : e.qty);
                    if(newbulkbill.qty==0)
                        newbulkbill.billtype = Bill.BillTypes.BulkSaleBilled;

                    newbulkbill.returnbillno = sourceBill.billno;
                    newbulkbill.createdby = Context.UserID;
                    newbulkbill.modifiedby = Context.UserID;
                    BulkBillDataService bulkBillDataService = new BulkBillDataService() { Context = this.Context };
                    bulkBillDataService.Insert(transaction, conn, newbulkbill);
                    foreach(var newbilleditem in newbulkbill.Billitems.Where(e=>e.qty>0))
                    {
                        var bulkstatuscommand = GetCommand("update bulkbillitems set qty=0,billedqty=@billedqty,journalqty=0 where id=@id;", conn, transaction);
                        bulkstatuscommand.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = newbilleditem.id;
                        bulkstatuscommand.Parameters.Add("billedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = newbilleditem.qty;
                        bulkstatuscommand.ExecuteNonQuery();
                    }
                }

                if (newbill.isbulkreturngoods)
                {
                    if (!InfyPOS.Services.ConfigManager.Instance(this.Context.OrganizationID).Config.system_disablebulkreturnonsale)
                    {
                        var bulkstatuscommand = GetCommand(SQL_UpdateBillStatus +
            "update bulkbillitems set journalqty=journalqty+(qty-(billedqty+journalqty)) where billid=@id;", conn, transaction);
                        bulkstatuscommand.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = sourceBill.id;
                        bulkstatuscommand.Parameters.Add("returnbillno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "~" + newbill.billno;
                        bulkstatuscommand.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        bulkstatuscommand.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        bulkstatuscommand.ExecuteNonQuery();
                    }
                }
                else
                {
                    int remaining = 0;
                    using (var reader = ExecuteReader(@"select sum(qty-(billedqty+journalqty)) from bulkbillitems
                    where billid=" + sourceBill.id, new SortedList<string, object>(), conn, transaction))
                    {
                        if (reader.Read())
                            remaining = Convert.ToInt32(reader[0]);
                    }

                    if (isvalid && remaining == 0)
                    {
                        var bulkstatuscommand = GetCommand(SQL_UpdateBillStatus, conn, transaction);
                        bulkstatuscommand.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = sourceBill.id;
                        bulkstatuscommand.Parameters.Add("returnbillno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "~"+newbill.billno;
                        bulkstatuscommand.ExecuteNonQuery();
                    }
                }
            }
            return isvalid;
        }

        public Billitems Insert(Billitems oBillitems, bool updatestock)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oBillitems, updatestock);
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
        public Billitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Billitems oBillitems, bool updatestock)
        {
            return Insert(transaction, conn, oBillitems, updatestock, false);
        }
        public Billitems Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Billitems oBillitems, bool updatestock, bool doubleadjustment)
        {
            // added to avoid last bit qty as journal qty.
            // Excluded for Estimate bills
            if (!doubleadjustment && oBillitems.journalqty > 0 && oBillitems.lastbitqty > 0)
                oBillitems.journalqty = 0;

            if (oBillitems.schemeid == 0 && oBillitems.schemediscount > 0)
                oBillitems.schemediscount = 0;

            NpgsqlCommand command = GetCommand((doubleadjustment ? SQL_Insert.Replace("update salablegoods set aqty=aqty+@qty where id=@salablegoodsid;", "") : SQL_Insert), conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillitems.barcode == null ? "" : oBillitems.barcode;
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
            command.Parameters.Add("etprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.etprice;
            command.Parameters.Add("etdiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.etdiscount;
            command.Parameters.Add("piece", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.piece;
            command.Parameters.Add("journalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.journalqty;
            command.Parameters.Add("itemtax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.itemtax;
            command.Parameters.Add("salerate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.salerate;
            command.Parameters.Add("saleamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.saleamount;
            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.taxpercentage;
            command.Parameters.Add("discountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.discountpercentage;
            command.Parameters.Add("additionaldiscountpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.additionaldiscountpercentage;
            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillitems.attributes == null ? "null" : oBillitems.attributes_json;
            command.Parameters.Add("schemediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.schemediscount;
            command.Parameters.Add("schemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.schemeid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillitems.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            //if (!isMigration)
            //{
            if (oBillitems.id > 0 && updatestock)
            {
                const string SQL_UpdateStock = @"update salablegoods set oqty=oqty+@qty where id=@id and stockid=@stockid";
                command = GetCommand(SQL_UpdateStock, conn, transaction);
                command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.stockid;
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salablegoodsid;
                command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.qty;
                command.ExecuteNonQuery();
            }
            //}
            return oBillitems;
        }

        public bool isMigration = false;

        public Billitems Update(Billitems oBillitems, Billitems oExistingBillitems)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBillitems, oExistingBillitems);
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

        public Billitems Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Billitems oBillitems, Billitems oExistingBillitems)
        {

            if (oBillitems.schemeid == 0 && oBillitems.schemediscount > 0)
                oBillitems.schemediscount = 0;

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillitems.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillitems.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillitems.createdon;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillitems.hsncode == null ? "" : oBillitems.hsncode;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillitems.barcode == null ? "" : oBillitems.barcode;
            command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.stockid;
            command.Parameters.Add("printingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillitems.printingname == null ? "" : oBillitems.printingname;
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
            command.Parameters.Add("etprice", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.etprice;
            command.Parameters.Add("etdiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.etdiscount;
            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillitems.attributes == null ? "null" : oBillitems.attributes_json;
            command.Parameters.Add("schemediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.schemediscount;
            command.Parameters.Add("schemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.schemeid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillitems.version = Convert.ToInt32(reader[0]);
                }
            }


            //command = GetCommand(SQL_UpdateStock, conn, transaction);
            //command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.stockid;
            //command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salablegoodsid;
            //command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.qty - oExistingBillitems.qty;
            //command.ExecuteNonQuery();

            return oBillitems;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, List<Billitems> list, long id)
        {
            var existingItems = SearchAll(id, false, false,false);
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
                    item.billid = id;
                    Insert(transaction, conn, item, false);
                }
            }
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
            //if (successCount > 0)
            //{
            //    command = GetCommand(SQL_UpdateStock, conn, transaction);
            //    command.Parameters.Add("stockid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.stockid;
            //    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salablegoodsid;
            //    command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.qty*-1;
            //    command.ExecuteNonQuery();
            //}
            return successCount > 0;
        }

        internal bool SyncJournalBillQty(NpgsqlTransaction transaction, NpgsqlConnection conn,
            Bill sourceBill, Bill bill)
        {
            bool isvalid = true;

            foreach (var item in bill.Billitems)
            {
                if (sourceBill.Billitems.Exists(e => e.salablegoodsid == item.salablegoodsid))
                {
                    var existitem = sourceBill.Billitems.Find(e => e.salablegoodsid == item.salablegoodsid);

                    if (sourceBill.Billitems.FindAll(e => e.salablegoodsid == item.salablegoodsid).Count > 1)
                    {
                        //existitem = bill.Billitems.Find(e => e.salablegoodsid == item.salablegoodsid && Math.Abs(e.qty) == item.qty);
                        existitem = sourceBill.Billitems.Find(e => e.salablegoodsid == item.salablegoodsid 
                        && Math.Abs(e.salesmanid) == item.salesmanid && e.qty - (e.journalqty + e.billedqty) > 0);
                    }


                    if (existitem != null)
                    {
                        NpgsqlCommand command = GetCommand(SQL_UpdateReturnQty, conn, transaction);
                        command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = existitem.salablegoodsid;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = existitem.id;
                        command.Parameters.Add("journalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = Math.Abs(item.qty);
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;

                        using (System.Data.IDataReader reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var billedqty = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
                                var qty = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
                                var journal = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
                                existitem.journalqty = journal;
                                if (qty - (billedqty + journal) < 0)
                                {
                                    isvalid = false;
                                }
                            }
                        }
                    }
                }
            }

            //foreach (var item in sourceBill.Billitems)
            //{
            //    if (bill.Billitems.Exists(e => e.salablegoodsid == item.salablegoodsid))
            //    {
            //        var existitem = bill.Billitems.Find(e => e.salablegoodsid == item.salablegoodsid);

            //        if (bill.Billitems.FindAll(e => e.salablegoodsid == item.salablegoodsid).Count > 1)
            //        {
            //            //existitem = bill.Billitems.Find(e => e.salablegoodsid == item.salablegoodsid && Math.Abs(e.qty) == item.qty);
            //            existitem = bill.Billitems.Find(e => e.salablegoodsid == item.salablegoodsid && Math.Abs(e.salesmanid) == item.salesmanid);
            //        }


            //        if (existitem != null)
            //        {
            //            NpgsqlCommand command = GetCommand(SQL_UpdateReturnQty, conn, transaction);
            //            command.Parameters.Add("salablegoodsid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.salablegoodsid;
            //            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
            //            command.Parameters.Add("journalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = Math.Abs(existitem.qty);
            //            using (System.Data.IDataReader reader = command.ExecuteReader())
            //            {
            //                if (reader.Read())
            //                {
            //                    var billedqty = reader.IsDBNull(0) ? 0 : reader.GetDecimal(0);
            //                    var qty = reader.IsDBNull(1) ? 0 : reader.GetDecimal(1);
            //                    var journal = reader.IsDBNull(2) ? 0 : reader.GetDecimal(2);
            //                    if (qty - (billedqty + journal) < 0)
            //                    {
            //                        isvalid = false;
            //                    }
            //                }
            //            }
            //        }
            //    }
            //}

            int remaining = 0;
            using (var reader = ExecuteReader(@"select sum(qty-(billedqty+journalqty)) from bulkbillitems
                    where billid=" + sourceBill.id, new SortedList<string, object>(), conn, transaction))
            {
                if (reader.Read())
                    remaining = Convert.ToInt32(reader[0]);
            }

            if (isvalid && remaining == 0)
            {
                var bulkstatuscommand = GetCommand(SQL_UpdateBillStatus, conn, transaction);
                bulkstatuscommand.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = sourceBill.id;
                bulkstatuscommand.Parameters.Add("returnbillno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = sourceBill.returnbillno;
                bulkstatuscommand.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                bulkstatuscommand.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                bulkstatuscommand.ExecuteNonQuery();
            }
            return isvalid;
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
