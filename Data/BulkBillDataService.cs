using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Data;

namespace InfyPOS.Data
{
    public class BulkBillDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, billmasterid, billno, billdate, companyid, customerid, tax, discount, receivable, billtype,
discountstatus,additionaldiscount,isinterstatesale,qty,locationid,rounding,counterid,returnreasonid,returnbillno,approvedby,billattributes from bulkbill where id=@id";


        const string SQL_Insert = @"INSERT INTO public.bulkbill(isactive, createdby, modifiedby, version, createdon, modifiedon, billmasterid, billno, billdate, companyid,customerid, tax, discount, receivable,billtype,discountstatus,additionaldiscount,isinterstatesale,qty,locationid,settled,rounding,counterid,returnreasonid,returnbillno,approvedby,billattributes) 
                                    VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @billmasterid, @billno, @billdate, @companyid, @customerid , @tax, @discount, @receivable,@billtype,@discountstatus,@additionaldiscount,@isinterstatesale,@qty,@locationid,false,@rounding,@counterid,@returnreasonid,@returnbillno,@approvedby,@billattributes) RETURNING id";


        const string SQL_Update = @"UPDATE public.bulkbill SET modifiedby=@modifiedby, customerid=@customerid, version=@version+1, modifiedon=@modifiedon, billmasterid=@billmasterid,rounding=@rounding, billno=@billno, billdate=@billdate, companyid=@companyid, tax=@tax, discount=@discount, 
                                        isinterstatesale=@isinterstatesale,receivable=@receivable, billtype=@billtype,discountstatus=@discountstatus,additionaldiscount=@additionaldiscount,qty=@qty,locationid=@locationid,
                                        counterid=@counterid,returnreasonid=@returnreasonid,returnbillno=@returnbillno,approvedby=@approvedby,billattributes=@billattributes where id=@id and version=@version  RETURNING version";

        const string SQL_Cancel = @"UPDATE public.bulkbill SET modifiedby=@modifiedby, customerid=@customerid, version=@version+1, modifiedon=@modifiedon, billmasterid=@billmasterid,rounding=@rounding, billno=@billno, billdate=@billdate, companyid=@companyid, tax=@tax, discount=@discount, 
                                        isinterstatesale=@isinterstatesale,receivable=@receivable, billtype=@billtype,discountstatus=@discountstatus,additionaldiscount=@additionaldiscount,qty=@qty,locationid=@locationid,
                                        counterid=@counterid,returnreasonid=@returnreasonid,returnbillno=@returnbillno,approvedby=@approvedby,billattributes=@billattributes where id=@id and version=@version and billtype != 9 RETURNING version";

        const string SQL_Delete = @"update public.bulkbill set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        const string SQL_SearchSalesReport = @"select bulkbill.id,bulkbill.billno ,bulkbill.billdate ,bulkbill.companyid ,bulkbill.tax ,bulkbill.discount,
                                               bulkbill.receivable,  bulkbill.additionaldiscount,bulkbill.counterid,
                                               bulkbillitems.barcode ,bulkbillitems.printingname ,bulkbillitems.price ,bulkbillitems.discount,
                                               bulkbillitems.receivable,bulkbillitems.companyid,bulkbillitems.rate,
                                               items.brandid,items.patternid, isinterstatesale,
                                               items.styleid,items.materialid,items.typeid,items.productid,
                                               stock.price,stock.buyingprice buyingprice,stock.qty,stock.supplierid
                                               from bulkbill 
                                               inner join bulkbillitems on bulkbillitems.billid = bulkbill.id
                                               INNER JOIN stock on stock.id = bulkbillitems.stockid
                                               left outer join items on items.id = stock.itemid";

        const string SQL_SearchItems = @"select bulkbill.id,bulkbill.billno,bulkbill.billdate,bulkbill.tax,bulkbill.discount,bulkbill.receivable,bulkbill.companyid,
bulkbillitems.barcode,bulkbillitems.printingname,bulkbillitems.price,bulkbillitems.discount,bulkbillitems.tax,bulkbillitems.qty,bulkbillitems.rate,stock.itemid,stock.supplierid,stock.productid,
                                    items.brandid,items.patternid,items.productid,items.colourid,isinterstatesale, 
                                    items.styleid,items.sizeid,items.materialid,bulkbill.counterid,items.typeid
                                    from bulkbill 
                                    inner join bulkbillitems on bulkbillitems.billid = bulkbill.id
                                    inner join stock on bulkbillitems.stockid = stock.id
                                    left outer join items on stock.itemid = items.id";



        public List<SalesReport> SearchSalesReport(SalesReportCreteria _Sales)
        {
            List<SalesReport> lst = new List<SalesReport>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchSalesReport;

                if (_Sales.companyid > 0)
                    qb.AddParameter("companyid", _Sales.companyid, "=");
                if (_Sales.patternid > 0)
                    qb.AddParameter("patternid", _Sales.patternid, "=");
                if (_Sales.brandid > 0)
                    qb.AddParameter("brandid", _Sales.brandid, "=");
                if (_Sales.productid > 0)
                    qb.AddParameter("productid", _Sales.productid, "=");
                if (_Sales.supplierid > 0)
                    qb.AddParameter("supplierid", _Sales.supplierid, "=");
                if (_Sales.materialid > 0)
                    qb.AddParameter("materialid", _Sales.materialid, "=");
                if (_Sales.styleid > 0)
                    qb.AddParameter("styleid", _Sales.styleid, "=");

                var command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesReport obill = new SalesReport();
                        obill.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        obill.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        obill.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        obill.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                        obill.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        obill.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        obill.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"].ToString());
                        obill.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"].ToString());
                        //ostock.colorid = reader["colorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colorid"].ToString());
                        //ostock.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"].ToString());
                        obill.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                        //ostock.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"].ToString());
                        //ostock.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"].ToString());
                        obill.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"].ToString());
                        lst.Add(obill);
                    }
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return lst;
        }


        public List<SalesReport> ItemSearch(SalesReportCreteria _Sales)
        {
            List<SalesReport> lst = new List<SalesReport>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchItems;


                if (_Sales.companyid > 0)
                    qb.AddParameter("companyid", _Sales.companyid, "=");
                if (_Sales.patternid > 0)
                    qb.AddParameter("item.patternid", _Sales.patternid, "=");
                if (_Sales.brandid > 0)
                    qb.AddParameter("item.brandid", _Sales.brandid, "=");
                if (_Sales.productid > 0)
                    qb.AddParameter("stock.productid", _Sales.productid, "=");
                if (_Sales.supplierid > 0)
                    qb.AddParameter("supplierid", _Sales.supplierid, "=");
                if (_Sales.materialid > 0)
                    qb.AddParameter("materialid", _Sales.materialid, "=");
                if (_Sales.styleid > 0)
                    qb.AddParameter("item.styleid", _Sales.styleid, "=");
                if (!string.IsNullOrEmpty(_Sales.barcode))
                    qb.AddParameter("bulkbillitems.barcode", _Sales.barcode, "=");

                qb.Limit = 250;
                var command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SalesReport obill = new SalesReport();
                        obill.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        obill.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        //obill.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"].ToString());
                        obill.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"].ToString());
                        obill.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"].ToString());
                        obill.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"].ToString());
                        obill.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        obill.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        obill.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        obill.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        obill.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"].ToString());
                        obill.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"].ToString());
                        //ostock.colorid = reader["colorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colorid"].ToString());
                        //ostock.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"].ToString());
                        obill.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                        //ostock.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"].ToString());
                        //ostock.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"].ToString());
                        obill.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"].ToString());
                        lst.Add(obill);
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

        internal List<Bill> Cancellation(ActionRequest<BillCancel> request)
        {
            if (request.Item.locationid == 0) return null;
            if (request.Item.ids == null || request.Item.ids.Count == 0)
                throw new Exception("Please select atleast one Bill for Cancellation");

            if (request.Item.ids.Count > 100)
                throw new Exception("Can't Cancel more than 100 Bills at once.");

            var billlist = Search(new BillCreteria()
            {
                ids = request.Item.ids
            });
            foreach (var item in billlist)
            {
                item.ChangeStatus(BaseEntity.DataStatus.Update);
                if (item.Billitems != null)
                    item.Billitems.ForEach(e => e.ChangeStatus(BaseEntity.DataStatus.Update));

                if (item.billtype == Bill.BillTypes.Estimate)
                    item.billtype = Bill.BillTypes.Cancel;
                else
                    item.billtype = Bill.BillTypes.BulkCancel;
                Update(item);
            }
            return billlist;
        }

        public List<StockReport> SearchStock(StockReportCreteria _Stock)
        {
            List<StockReport> lst = new List<StockReport>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchItems;

                //if (_Stock.companyid > 0)
                //    qb.AddParameter("companyid", _Stock.companyid, "=");
                if (_Stock.patternid > 0)
                    qb.AddParameter("patternid", _Stock.patternid, "=");
                if (_Stock.brandid > 0)
                    qb.AddParameter("brandid", _Stock.brandid, "=");
                if (_Stock.productid > 0)
                    qb.AddParameter("productid", _Stock.productid, "=");
                if (_Stock.supplierid > 0)
                    qb.AddParameter("supplierid", _Stock.supplierid, "=");
                if (_Stock.materialid > 0)
                    qb.AddParameter("materialid", _Stock.materialid, "=");
                if (_Stock.styleid > 0)
                    qb.AddParameter("styleid", _Stock.styleid, "=");
                if (_Stock.sizeid > 0)
                    qb.AddParameter("sizeid", _Stock.sizeid, "=");

                var command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        StockReport ostock = new StockReport();
                        ostock.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        ostock.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"].ToString());
                        ostock.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"].ToString());
                        ostock.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        ostock.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"].ToString());
                        ostock.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"].ToString());
                        ostock.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        ostock.patternid = reader["patternid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["patternid"].ToString());
                        ostock.styleid = reader["styleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["styleid"].ToString());
                        //ostock.colorid = reader["colorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colorid"].ToString());
                        //ostock.materialid = reader["materialid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["materialid"].ToString());
                        ostock.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                        //ostock.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"].ToString());
                        //ostock.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["typeid"].ToString());
                        ostock.brandid = reader["brandid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["brandid"].ToString());


                        lst.Add(ostock);
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

        public long GetBillIDFromBarcode(string barcode)
        {
            string query = @"select billid from billitems
                            where barcode=@barcode
                            order by id desc
                            limit 1";

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(query, conn);
                command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = barcode;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        return reader.GetInt64(0);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
            return 0;
        }


        public List<Bill> Search(BillCreteria _bill)
        {
            List<Bill> lst = new List<Bill>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_bill.id == 0 && !string.IsNullOrEmpty(_bill.barcode))
                {
                    _bill.id = GetBillIDFromBarcode(_bill.barcode);
                }
                if (_bill.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (!string.IsNullOrEmpty(_bill.billno))
                        qb.AddParameter("billno", _bill.billno, "=");

                    if (_bill.billmasterid != 0)
                        qb.AddParameter("billmasterid", _bill.billmasterid, "=");

                    if (_bill.locationid != 0)
                        qb.AddParameter("locationid", _bill.locationid, "=");


                    if (_bill.forsettlement)
                    {
                        qb.AddParameter("settled", false, "=");
                        qb.AddParameter("billtype", 9, "!=");
                        qb.AddParameter("billtype", 16, "!=");
                    }
                    else if (_bill.forsalesreturn)
                    {
                        qb.AddParameter("settled", true, "=");
                        qb.AddParameter("billtype", 9, "!=");
                        qb.AddParameter("billtype", 16, "!=");
                    }
                    else if (_bill.billnos != null && _bill.billnos.Count > 0)
                    {
                        qb.AddInParamer("bulkbill.billno", string.Join<string>(",", _bill.billnos.ConvertAll(e => "'" + e + "'")));
                    }
                    else
                    {

                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportestimate)
                            qb.AddInParamer("billtype", "10,20");
                        else
                            qb.AddParameter("billtype", 10, "=");
                    }

                    if (_bill.forsalesreturn || _bill.forsettlement || !string.IsNullOrEmpty(_bill.billno))
                    {
                        qb.AddParameter("locationid", _bill.locationid, "=");
                        qb.SortField = "bulkbill.id desc";
                        qb.Limit = 1;
                    }
                    else
                    {
                        if (_bill.ids != null)
                        {
                            qb.AddInParamer("bulkbill.id", string.Join<long>(",", _bill.ids));
                        }
                        else
                        {
                            qb.Limit = 100;
                        }
                    }
                    command = qb.GetCommand(conn);
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _bill.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Bill obill = new Bill();

                        obill.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obill.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        obill.counterid = reader["counterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["counterid"]);
                        obill.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obill.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obill.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obill.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obill.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obill.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obill.billmasterid = reader["billmasterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billmasterid"]);
                        obill.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        obill.returnreasonid = reader["returnreasonid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["returnreasonid"]);
                        obill.returnbillno = reader["returnbillno"] == DBNull.Value ? "" : reader["returnbillno"].ToString();
                        obill.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        obill.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        obill.billtype = reader["billtype"] == DBNull.Value ? Bill.BillTypes.Cash : (Bill.BillTypes)Convert.ToInt32(reader["billtype"]);
                        obill.discountstatus = reader["discountstatus"] == DBNull.Value ? Discountqueue.DiscountStatus.NoDiscount : (Discountqueue.DiscountStatus)Convert.ToInt32(reader["discountstatus"]);
                        obill.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        obill.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        obill.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"]);
                        obill.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        obill.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        obill.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        obill.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        obill.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                        obill.billattributes_json = reader["billattributes"] == DBNull.Value ? "" : reader["billattributes"].ToString();

                        if (obill.billattributes == null) obill.billattributes = new BillAttribute();
                        obill.billattributes.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        lst.Add(obill);
                    }
                }
                if ((_bill.id > 0 || !string.IsNullOrEmpty(_bill.billno) || _bill.ids != null || (_bill.billnos != null && _bill.billnos.Count>0)))
                {
                    foreach (var billitem in lst)
                    {
                        BulkBillItemsDataService subBillitems = new BulkBillItemsDataService() { Context = this.Context };
                        subBillitems.Context = this.Context;
                        billitem.AddItems(subBillitems.SearchAll(billitem.id,
                            _bill.enableprintdocument || _bill.forsalesreturn,
                            billitem.isinterstatesale,
                            lst[0].billtype == Bill.BillTypes.Estimate ? false : _bill.isbulkreturn), false);
                        // Added to avoid bulkbill billedqty offset operations  "lst[0].billtype == Bill.BillTypes.Estimate ? false : "

                        if (!string.IsNullOrEmpty(_bill.billno))
                        {
                            if (billitem.discount > 0 && billitem.schemediscount == 0)
                            {
                                billitem.schemediscount = billitem.Billitems.Sum(e => e.schemediscount);
                                billitem.discount -= billitem.schemediscount;
                            }
                        }

                        if (_bill.isbulkreturn)
                        {
                            billitem.Billitems.RemoveAll(e => e.qty == 0);
                            billitem.qty = billitem.Billitems.Sum(e => e.qty);
                            billitem.tax = billitem.Billitems.Sum(e => e.tax);
                            billitem.receivable = billitem.Billitems.Sum(e => e.receivable);
                            billitem.rounding = Math.Round(billitem.receivable, MidpointRounding.AwayFromZero) - billitem.receivable;
                        }

                        if (billitem.customerid != 0)
                        {
                            CustomerDataService customerData = new CustomerDataService() { Context = this.Context };
                            customerData.Context = this.Context;
                            var custid = new CustomerCreteria();
                            custid.id = billitem.customerid;
                            var customer = customerData.Search(custid);
                            if (customer != null && customer.Count >= 1)
                            {
                                billitem.customername = customer[0].name;
                                billitem.phoneno = customer[0].mobileno;
                                billitem.customergst = customer[0].gstno;
                                billitem.customer = customer[0];
                            }
                        }
                        billitem.Billitems = billitem.Billitems.OrderBy(e => e.orderid).ToList();

                        //billitem.totalqty = billitem.Billitems.Sum(e => e.qty);
                        //billitem.totalpiece = billitem.Billitems.Sum(e => e.piece);

                        if (billitem.Billitems.Any(e => e.qty < 0) && billitem.Billitems.Any(e => e.qty > 0))
                        {
                            billitem.totalqty = billitem.qty = billitem.Billitems.Sum(e => e.qty > 0 ? e.qty : 0);
                            billitem.totalpiece = billitem.Billitems.Sum(e => e.piece > 0 ? e.piece : 0);
                        }
                        else
                        {
                            billitem.totalqty = billitem.qty = billitem.Billitems.Sum(e => e.qty);
                            billitem.totalpiece = billitem.Billitems.Sum(e => e.piece);
                        }

                        //billitem.total = billitem.receivable + billitem.rounding;
                        billitem.taxsplit = new TaxSplit();
                        billitem.taxsplit.totalamount = billitem.Billitems.Sum(e => e.rate * e.qty);
                        billitem.taxsplit.cgsttaxamount = Math.Round(billitem.tax / 2, 5);
                        billitem.taxsplit.sgsttaxamount = Math.Round(billitem.tax / 2, 5);
                        billitem.taxsplit.secondtax = billitem.isinterstatesale ? "IGST" : "SGST";
                        billitem.BillTax.ForEach(e => e.secondtax = billitem.taxsplit.secondtax);
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

        public List<Customerinvoice> SearchForReminder(CustomerinvoiceCreteria _bill)
        {
            const string SQL_Reminder = @"Select customer.id, customer.mobileno, customer.name customername, sum(receivable) receivable, sum(qty) qty
from bulkbill inner join customer on customer.id = bulkbill.customerid 
Where bulkbill.isactive=True And billdate<@fromdate 
And bulkbill.billtype=10
group by customer.id, customer.mobileno, customer.name";

            List<Customerinvoice> lst = new List<Customerinvoice>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_Reminder, conn);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Date).Value = _bill.fromdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customerinvoice obill = new Customerinvoice();
                        obill.customerid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obill.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        obill.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                        obill.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        obill.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        lst.Add(obill);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _bill);
            }
            return lst;
        }

        public Bill Insert(Bill oBill)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                SequenceNo sequence = null;
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_floorwisebulkbillno)
                {
                    sequence = SequenceManager.Instance(Context.OrganizationID).Get("bulkbill.billno", oBill.locationid, oBill.companyid,
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.system_floorwisebulkbillno ? oBill.floorid : 0);
                }
                else
                    sequence = SequenceManager.Instance(Context.OrganizationID).Get("bulkbill.billno", oBill.locationid, oBill.companyid);

                if (!sequence.Validate(Context, "Bulk Bill No", false))
                {
                    InfyPOS.Data.ReferencelistDataService reflist = new ReferencelistDataService() { Context = this.Context };
                    var location = reflist.Search(new ReferencelistCreteria()
                    {
                        id = oBill.locationid
                    });
                    if (location.Count == 1)
                        sequence.Validate(Context, "Bulk Bill No for " + location[0].name, true);
                }
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    oBill.billno = sequence.GetNextNumber(Context, conn, transaction);

                    Insert(transaction, conn, oBill);
                    //BulkBillItemsDataService subBillitems = new BulkBillItemsDataService() { Context = this.Context };
                    //subBillitems.Context = this.Context;
                    //int orderid = 1;
                    //foreach (var item in oBill.Billitems)
                    //{
                    //    item.billid = oBill.id;
                    //    item.orderid = orderid++;
                    //    subBillitems.Insert(transaction, conn, item);
                    //}

                    if (oBill.billtype == Bill.BillTypes.Cancel && !string.IsNullOrEmpty(oBill.returnbillno))
                    {
                        NpgsqlCommand command = GetCommand("Update bulkbill set billtype=9 where billno='" + WebAPI.Data.Utility.SafeSQL(oBill.returnbillno) + "'", conn, transaction);
                        command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

            return oBill;
        }
        public Bill Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill oBill)
        {
            return Insert(transaction, conn, oBill, false);
        }
        public Bill Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill oBill, bool doubleadjustment)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.isactive;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.isinterstatesale;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.locationid;
            command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.counterid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBill.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("billmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.billmasterid;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBill.billno == null ? "" : oBill.billno;
            command.Parameters.Add("returnreasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.returnreasonid;
            command.Parameters.Add("returnbillno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBill.returnbillno == null ? "" : oBill.returnbillno;
            command.Parameters.Add("billdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBill.billdate;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.companyid;
            command.Parameters.Add("billtype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBill.billtype;
            command.Parameters.Add("discountstatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBill.discountstatus;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.additionaldiscount;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.tax;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.rounding;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.discount;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.qty;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.receivable;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.customerid;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.billattributes == null ? 0 : oBill.billattributes.approvedby;
            command.Parameters.Add("billattributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oBill.billattributes == null ? "null" : oBill.billattributes_json;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBill.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            bool updatestock = false;
            if (oBill.customerid > 0 && oBill.billtype != Bill.BillTypes.Estimate)
            {
                CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
                customerDataService.Context = this.Context;
                var customers = customerDataService.Search(new CustomerCreteria()
                {
                    id = oBill.customerid
                });
                updatestock = customers.Count == 1 && customers[0].customertype == 5;
            }

            BulkBillItemsDataService subBillitems = new BulkBillItemsDataService() { Context = this.Context };
            subBillitems.Context = this.Context;
            foreach (var item in oBill.Billitems)
            {
                item.billid = oBill.id;
                if (doubleadjustment)
                {
                    subBillitems.Insert(transaction, conn, item, updatestock, doubleadjustment);
                    //subBillitems.Insert(transaction, conn, item, updatestock, oBill.billtype == Bill.BillTypes.Estimate);
                }
                else
                    subBillitems.Insert(transaction, conn, item, updatestock, (item.receivable == 0 || oBill.billtype == Bill.BillTypes.Estimate) ? true : false);
            }
            return oBill;
        }

        public Bill Update(Bill oBill)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Update(transaction, conn, oBill);

                    if (oBill.billtype == Bill.BillTypes.BulkCancel && (oBill.Billitems == null || oBill.Billitems.Count == 0))
                        throw new Exception("Can't do Cancellation!. Please do Bulk Return");

                    if (oBill.billtype == Bill.BillTypes.BulkCancel)
                    {
                        foreach (Billitems billitem in oBill.Billitems)
                        {
                            OffSetApprovedQty(transaction, conn, billitem);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }

            return oBill;
        }

        public Bill Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill oBill)
        {
            NpgsqlCommand command = GetCommand(oBill.billtype == Bill.BillTypes.Cancel ? SQL_Cancel : SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.isactive;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.isinterstatesale;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.locationid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBill.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBill.createdon;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.rounding;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBill.billno == null ? "" : oBill.billno;
            command.Parameters.Add("returnreasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.returnreasonid;
            command.Parameters.Add("returnbillno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBill.returnbillno == null ? "" : oBill.returnbillno;
            command.Parameters.Add("returnreasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.returnreasonid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("billmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.billmasterid;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBill.billno == null ? "" : oBill.billno;
            command.Parameters.Add("billdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBill.billdate;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.companyid;
            command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.counterid;
            command.Parameters.Add("billtype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBill.billtype;
            command.Parameters.Add("discountstatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBill.discountstatus;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.additionaldiscount;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.tax;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.discount;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.receivable;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.customerid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.qty;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.billattributes == null ? 0 : oBill.billattributes.approvedby;
            command.Parameters.Add("billattributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oBill.billattributes == null ? "null" : oBill.billattributes_json;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBill.version = Convert.ToInt32(reader[0]);
                }
            }

            return oBill;
        }
        public bool OffSetApprovedQty(NpgsqlTransaction transaction, NpgsqlConnection conn, Billitems oBillitems)
        {
            const string SQL_UpdateStock = @"update salablegoods set aqty=aqty-@qty where id=@id";

            NpgsqlCommand command = GetCommand(SQL_UpdateStock, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillitems.salablegoodsid;
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillitems.qty;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }


        public bool Delete(Bill oBill)
        {

            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Delete(transaction, conn, oBill);
                    BulkBillItemsDataService subBillitems = new BulkBillItemsDataService() { Context = this.Context };
                    subBillitems.Context = this.Context;
                    subBillitems.DeleteAll(oBill.id, oBill.modifiedby, conn, transaction);
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

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill oBill)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBill.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal Bill InsertReturn(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill bill)
        {
            var sourcebill = Search(new BillCreteria()
            {
                billno = bill.returnbillno,
                locationid = bill.locationid
            }).First();

            BulkBillItemsDataService bulkBillItemsDataService = new BulkBillItemsDataService() { Context = this.Context };
            if (!bulkBillItemsDataService.SyncJournalBillQty(transaction, conn, sourcebill, bill))
                throw new Exception("Some of the goods already received");

            bill.billtype = Bill.BillTypes.BulkSaleReturn;
            return Insert(transaction, conn, bill, true);
        }

        internal DataTable ExportBillDetail(long billid)
        {
            string query = @"Select 
BulkBillItems.id as ""Quanto Row ID"",
bill.billno as ""Bill No"",bill.billdate::date ""Bill Date"", 
location.name as Location,
company.name ""Supplier Company"",
company.tinanddate ""Supplier GST NO"",
supplier.name ""Source Supplier"",
customer.name ""Customer Name"",
customer.gstno ""Customer GST NO"",
billitems.printingname ""Detail"",
billitems.hsncode ""HSN"",
billitems.barcode,stock.serialno ""Article ID"",
items.brandname ""Brand"",
items.patternname ""Pattern"",
items.stylename ""Style"",  items.productname ""Product"", 
items.sizename ""Size"",items.materialname ""Material"",
items.colourname ""Colour"",
items.typename ""Item Type"",
billitems.price ""Price"", 
billitems.rate,lrinvoiceitems.mrp ""MRP Price"",billitems.taxpercentage ""Tax Percentage"",
case when billitems.discount > 0 then billitems.discount::numeric / billitems.qty::numeric else 0::numeric end ""Discount Per Item"",
billitems.additionaldiscount as ""Additional discount per item"",billitems.itemtax ""Tax Per Item"",stock.designid,
billitems.tax, billitems.discount,billitems.additionaldiscount::numeric* billitems.qty::numeric ""Additional discount"", 
billitems.qty,(billitems.discount::numeric+(billitems.additionaldiscount::numeric* billitems.qty::numeric)) ""Total disount"",
billitems.receivable ""Item Value""
from bulkbill bill
inner join BulkBillItems billitems on billitems.billid = bill.id
left outer join company on company.id = bill.companyid 
left outer join customer on customer.id = bill.customerid
inner join referencelist location on location.id = bill.locationid
inner join salablegoods on salablegoods.id = billitems.salablegoodsid
inner join stock on stock.id = salablegoods.stockid
left outer join supplier on supplier.id = stock.supplierid
left outer join lrinvoiceitems on lrinvoiceitems.id=stock.invoiceitemid
left outer join items on items.id = stock.itemid
where bill.id = @id";
            List<Bill> lst = new List<Bill>();
            System.Data.DataSet dset = null;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                var param = new SortedList<string, object>();
                param.Add("id", billid);
                conn.Open();
                dset = ExecuteDataSet(query, param);
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                throw exp;
            }
            return dset != null ? dset.Tables[0] : null;
        }

        internal void UpdateReturnBillNo(NpgsqlConnection conn, NpgsqlTransaction transaction,
            List<Bill> bills)
        {
            List<string> lst = new List<string>();
            bills.ForEach(x =>
            {
                lst.AddRange(x.billattributes.ebillno.ConvertAll(y => y.billno));
            });
            var builkbills = Search(new BillCreteria()
            {
                billnos = lst
            });

            foreach (var bill in bills)
            {
                var bubills = builkbills.FindAll(x => x.companyid == bill.companyid);
                foreach (var bub in bubills)
                {
                    string query = @"update bulkbill set billtype=21,returnbillno=@returnbillno where billno=@billno and billtype!=21";
                    NpgsqlCommand command = GetCommand(query, conn, transaction);
                    command.Parameters.Add("returnbillno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = bill.billno;
                    command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = bub.billno;
                    int affected = command.ExecuteNonQuery();
                    if (affected == 0)
                    {
                        string createdbillno = "";
                        var param = new SortedList<string, object>();
                        param.Add("billno", bub.billno);
                        using (var reader = ExecuteReader("select returnbillno from bulkbill where billno=@billno", param))
                        {
                            if (reader.Read())
                            {
                                createdbillno = reader[0] == DBNull.Value ? "" : Convert.ToString(reader[0]);
                            }
                        }
                        throw new Exception("This estimation " + bub.billno + " already settled on " + createdbillno);
                    }
                    else
                    {
                        foreach (var newbilleditem in bub.Billitems.Where(e => e.qty > 0).GroupBy(x => x.salablegoodsid))
                        {
                            var billedqty = bill.Billitems.FindAll(x => x.salablegoodsid == newbilleditem.Key).Sum(y => y.qty);
                            if (newbilleditem.Count() == 1)
                            {
                                var bulkstatuscommand = GetCommand("update bulkbillitems set billedqty=@billedqty where id=@id;", conn, transaction);
                                bulkstatuscommand.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = newbilleditem.First().id;
                                bulkstatuscommand.Parameters.Add("billedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = billedqty;
                                bulkstatuscommand.ExecuteNonQuery();
                            }
                            else
                            {
                                var bulkbillitems = newbilleditem.ToList();
                                var billeditems = bill.Billitems.FindAll(x => x.salablegoodsid == newbilleditem.Key);
                                bulkbillitems.ForEach(z =>
                                {
                                    if (billeditems.Exists(y => y.qty == z.qty))
                                    {
                                        var bulkstatuscommand = GetCommand("update bulkbillitems set billedqty=@billedqty where id=@id;", conn, transaction);
                                        bulkstatuscommand.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = z.id;
                                        bulkstatuscommand.Parameters.Add("billedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = z.qty;
                                        bulkstatuscommand.ExecuteNonQuery();
                                        billeditems.Remove(billeditems.Find(y => y.qty == z.qty));
                                    }
                                    else if (billeditems.Count > 0)
                                    {
                                        var bulkstatuscommand = GetCommand("update bulkbillitems set billedqty=@billedqty where id=@id;", conn, transaction);
                                        bulkstatuscommand.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = z.id;
                                        bulkstatuscommand.Parameters.Add("billedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = billeditems[0].qty;
                                        bulkstatuscommand.ExecuteNonQuery();
                                        billeditems.Remove(billeditems[0]);
                                    }
                                });
                            }
                        }
                    }
                }
            }
        }
    }

}
