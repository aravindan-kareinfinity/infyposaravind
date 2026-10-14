using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class BillhistoryDataService : BaseData
    {
        const string SQL_Search = @"select billhistory.id, billhistory.companyid, billhistory.locationid, billhistory.fromdate, 
        billhistory.todate, billhistory.isactive, billhistory.createdby, billhistory.modifiedby,
        billhistory.version, billhistory.createdon, billhistory.modifiedon, billhistory.creteria, 
        billhistory.tax, billhistory.receivable,employee.name editedby from billhistory
        left outer join users on users.id = billhistory.createdby
        left outer join employee on employee.id = users.employeeid
        where billhistory.id=@id";


        const string SQL_Insert = @"INSERT INTO public.billhistory(companyid, locationid, fromdate, todate, isactive, createdby, modifiedby, version, createdon, modifiedon, creteria, tax, receivable) VALUES(@companyid, @locationid, @fromdate, @todate, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @creteria, @tax, @receivable) RETURNING id";


        const string SQL_Update = @"UPDATE public.billhistory SET companyid=@companyid, locationid=@locationid, fromdate=@fromdate, todate=@todate, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, creteria=@creteria, tax=@tax, receivable=@receivable where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.billhistory set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Billhistory> Search(BillhistoryCreteria _billhistory)
        {
            List<Billhistory> lst = new List<Billhistory>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_billhistory.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billhistory.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("billhistory.isactive", true, "=");

                    if (_billhistory.companyid > 0)
                        qb.AddParameter("billhistory.companyid", _billhistory.companyid, "=");
                    if (_billhistory.locationid > 0)
                        qb.AddParameter("billhistory.locationid", _billhistory.locationid, "=");
                    if (_billhistory.fromdate.Year > 1900)
                        qb.AddParameter("billhistory.fromdate", "fromdate", _billhistory.fromdate.Date, ">=");
                    if (_billhistory.todate.Year > 1900)
                        qb.AddParameter("billhistory.todate", "todate", _billhistory.todate.Date.AddDays(1), "<");
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Billhistory obillhistory = new Billhistory();
                        obillhistory.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obillhistory.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        obillhistory.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        obillhistory.fromdate = reader["fromdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["fromdate"]);
                        obillhistory.todate = reader["todate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["todate"]);
                        obillhistory.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obillhistory.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obillhistory.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obillhistory.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obillhistory.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obillhistory.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obillhistory.creteria_json = reader["creteria"] == DBNull.Value ? null : reader["creteria"].ToString();
                        obillhistory.editedby = reader["editedby"] == DBNull.Value ? null : reader["editedby"].ToString();
                        obillhistory.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        obillhistory.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        lst.Add(obillhistory);
                    }
                }
                if (_billhistory.id > 0 && lst.Count == 1)
                {
                    BillhistoryitemsDataService subBillhistoryitems = new BillhistoryitemsDataService() { Context = this.Context };
                    lst[0].Billhistoryitems = subBillhistoryitems.SearchAll(_billhistory.id);
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _billhistory);
            }
            return lst;
        }

        internal bool RollBack(Billhistory item)
        {
            var items = Search(new BillhistoryCreteria()
            {
                id = item.id
            }).First();
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();


                    BillDataService billDataService = new BillDataService() { Context = this.Context };
                    BillsettlementmasterDataService billsettlementdata = new BillsettlementmasterDataService() { Context = this.Context };
                    BillsettlementDataService subBillsettlement = new BillsettlementDataService() { Context = this.Context };

                    string undeletebillquery = @"update bill set isactive = true where id=@billid;
                                                 update billitems set isactive = true where billid=@billid;
                                                 ";

                    string undeletesettlementquery = @"update billsettlementmaster set isactive = true where id=@billsettlementmasterid;
                                             update billsettlement set isactive = true where billsettlementmasterid=@billsettlementmasterid;";


                    foreach (var historyitem in items.Billhistoryitems)
                    {
                        if(historyitem.deleted)
                        {
                            NpgsqlCommand command = new NpgsqlCommand(historyitem.billsettlementmasterid <= 0 ? undeletebillquery : undeletebillquery+ undeletesettlementquery, conn, transaction);
                            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
                            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = historyitem.billid;
                            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            command.Parameters.Add("billsettlementmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = historyitem.billsettlementmasterid;
                            command.ExecuteNonQuery();
                        }
                        else
                        {
                            historyitem.oldbilldata.DontChangeStock = true;
                            billDataService.Update(transaction, conn, historyitem.oldbilldata,true);
                            if (historyitem.oldsettlementdata != null)
                                billsettlementdata.Update(transaction, conn, historyitem.oldsettlementdata);
                        }
                    }

                    
                    Delete(transaction, conn, item);
                    BillhistoryitemsDataService subBillhistoryitems = new BillhistoryitemsDataService() { Context = this.Context };
                    subBillhistoryitems.DeleteAll(item.id, item.modifiedby, conn, transaction);

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

        public Billhistory Insert(Billhistory oBillhistory)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    var items = oBillhistory.creteria.items;
                    oBillhistory.creteria.items = null;
                    Insert(transaction, conn, oBillhistory);
                    oBillhistory.creteria.items = items;

                    BillDataService billDataService = new BillDataService() { Context = this.Context };
                    BillsettlementmasterDataService billsettlementdata = new BillsettlementmasterDataService() { Context = this.Context };
                    BillsettlementDataService subBillsettlement = new BillsettlementDataService() { Context = this.Context };
                    var deletedbills = oBillhistory.Billhistoryitems.FindAll(e => e.deleted);
                    foreach (var deletedbill in deletedbills)
                    {
                        billDataService.Delete(transaction, conn, deletedbill.oldbilldata);
                        if (deletedbill.oldsettlementdata != null)
                            billsettlementdata.Delete(transaction, conn, deletedbill.oldsettlementdata);
                    }

                    var modifiedbills = oBillhistory.Billhistoryitems.FindAll(e => !e.deleted);
                    foreach (var modifiedbill in modifiedbills)
                    {
                        modifiedbill.newbilldata.DontChangeStock = true;
                        billDataService.Update(transaction, conn, modifiedbill.newbilldata,true);
                        if (modifiedbill.newsettlementdata != null &&
                            !modifiedbill.multiplesettlement)
                            billsettlementdata.Update(transaction, conn, modifiedbill.newsettlementdata);
                    }
                    foreach (var multisettlement in modifiedbills.FindAll(e => e.multiplesettlement).GroupBy(e => e.billsettlementmasterid))
                    {
                        if(multisettlement.First().newsettlementdata != null)
                            billsettlementdata.Update(transaction, conn, multisettlement.First().newsettlementdata);
                    }

                    BillhistoryitemsDataService subBillhistoryitems = new BillhistoryitemsDataService()
                    {
                        Context = this.Context
                    };
                    foreach (var item in oBillhistory.Billhistoryitems)
                    {
                        item.billhistoryid = oBillhistory.id;
                        item.newbilldata.taxsplit = null;
                        item.newbilldata.Billitems.ForEach(e => e.taxsplit = null);
                        item.oldbilldata.taxsplit = null;
                        item.oldbilldata.Billitems.ForEach(e => e.taxsplit = null);
                        subBillhistoryitems.Insert(transaction, conn, item);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillhistory);
            }

            return oBillhistory;
        }

        public Billhistory Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Billhistory oBillhistory)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistory.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistory.locationid;
            command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillhistory.fromdate;
            command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillhistory.todate;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillhistory.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistory.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistory.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillhistory.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillhistory.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillhistory.modifiedon;
            command.Parameters.Add("creteria", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillhistory.creteria_json == null ? "null" : oBillhistory.creteria_json;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillhistory.tax;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillhistory.receivable;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillhistory.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oBillhistory;
        }

        public Billhistory Update(Billhistory oBillhistory)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBillhistory);
                    BillhistoryitemsDataService subBillhistoryitems = new BillhistoryitemsDataService() { Context = this.Context };
                    subBillhistoryitems.Update(transaction, conn, oBillhistory.Billhistoryitems, oBillhistory.id);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillhistory);
            }

            return oBillhistory;
        }


        public List<SaleEditItem> SearchBill4Edit(SaleEdit _Sales)
        {
            List<SaleEditItem> lst = new List<SaleEditItem>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var CurrentQuery = @"select billitems.id,billitems.receivable,billitems.schemediscount,billitems.productid,billitems.printingname,billitems.price,billitems.taxid,
                billitems.discount,billitems.tax,billitems.rate,billitems.salerate,billitems.saleamount,billitems.taxpercentage,billitems.qty,lrinvoiceitems.purchasetaxpercentage,
                billitems.additionaldiscount,bill.billdate,bill.billno,billitems.barcode,bill.receivable billvalue,bill.id billid,billitems.printingname,lrinvoiceitems.buyingprice,
                billitems.schemeid
                from bill
                inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                inner join stock on stock.id = billitems.stockid
                inner join products on products.id = stock.productid
                inner join items on items.id = stock.itemid
                left outer join customer on customer.id = bill.customerid
                left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid ";

                //if (!_Sales.includecash || !_Sales.includecard)
                {
                    CurrentQuery += @" left outer join billsettlement on billsettlement.billid = bill.id
                                          left outer join billsettlementmaster on billsettlementmaster.id= billsettlementmasterid ";
                }

                CurrentQuery += @" where bill.isactive = true and bill.billtype != 9 and billitems.isactive = true and billitems.qty>0 ";

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = CurrentQuery;
                if (_Sales.companyid > 0)
                    qb.AddParameter("bill.companyid", _Sales.companyid, "=");
                if (_Sales.locationid > 0)
                    qb.AddParameter("bill.locationid", _Sales.locationid, "=");
                if (_Sales.brandid > 0)
                    qb.AddParameter("items.brandid", _Sales.brandid, "=");
                if (_Sales.sectionid > 0)
                    qb.AddParameter("products.salescounterid", _Sales.sectionid, "=");
                if (_Sales.productid > 0)
                    qb.AddParameter("billitems.productid", _Sales.productid, "=");
                if (_Sales.billdatefrom.Year > 2000)
                    qb.AddParameter("bill.billdate", "frombilldate", _Sales.billdatefrom.Date, ">=");
                if (_Sales.billdateto.Year > 2000)
                    qb.AddParameter("bill.billdate", "tobilldate", _Sales.billdateto.Date.AddDays(1), "<");
                if (_Sales.billvaluefrom > 0)
                    qb.AddParameter("bill.receivable", _Sales.billvaluefrom, ">=");
                if (_Sales.billvalueto > 0)
                    qb.AddParameter("bill.receivable", _Sales.billvalueto, "<=");
                if (_Sales.pricefrom > 0)
                    qb.AddParameter("billitems.price", _Sales.pricefrom, ">=");
                if (_Sales.priceto > 0)
                    qb.AddParameter("billitems.price", _Sales.priceto, "<=");
                if (_Sales.purchasetaxpercentage > 0)
                    qb.AddParameter("lrinvoiceitems.purchasetaxpercentage", _Sales.purchasetaxpercentage, "=");
                if (_Sales.salestaxpercentage > 0)
                    qb.AddParameter("billitems.taxpercentage", _Sales.salestaxpercentage, "<=");
                if (!string.IsNullOrEmpty(_Sales.barcode))
                    qb.AddParameter("billitems.barcode", _Sales.barcode, "=");
                if (!string.IsNullOrEmpty(_Sales.billno))
                    qb.AddParameter("bill.billno", _Sales.billno, "=");
                if (!_Sales.includeaditionaldiscount)
                    qb.AddParameter("billitems.additionaldiscount", 0, "=");
                if (!_Sales.includediscount)
                    qb.AddParameter("billitems.discount", 0, "=");
                if (!_Sales.includescheme)
                    qb.AddParameter("billitems.schemediscount", 0, "=");
                if (!_Sales.includegst)
                    qb.AddParameter("customer.gstno", "", "=");
                if (!_Sales.includecredit)
                    qb.AddParameter("bill.billtype", 2, "!=");
                else
                    qb.AddParameter("bill.billtype", 2, "=");

                if (_Sales.includecash && _Sales.includecard)
                {
                    qb.Add2Query(" (billsettlementmaster.paid_card+coalesce(billsettlementmaster.paid_upi,0) > 0 or billsettlementmaster.paid_cash <= 0 or billsettlementmaster.id is null) ");
                }
                else
                {
                    if (!_Sales.includecash)
                        qb.Add2Query(" (billsettlementmaster.paid_cash <= 0 or billsettlementmaster.id is null) ");
                    else
                        qb.Add2Query(" (billsettlementmaster.paid_cash > 0 or billsettlementmaster.id is null) ");
                    if (!_Sales.includecard)
                        qb.Add2Query(" (billsettlementmaster.paid_card+coalesce(billsettlementmaster.paid_upi,0) <= 0 or billsettlementmaster.id is null) ");
                    else
                        qb.Add2Query(" (billsettlementmaster.paid_card+coalesce(billsettlementmaster.paid_upi,0) > 0 or billsettlementmaster.id is null) ");
                }

                qb.AddParameter("bill.isb2b", _Sales.includeb2b, "=");

                var command = qb.GetCommand(conn);
                command.CommandTimeout = 10000;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SaleEditItem obill = new SaleEditItem();
                        obill.bno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        obill.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        obill.product = reader["printingname"] == DBNull.Value ? "" : reader["printingname"].ToString();
                        obill.pid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        obill.bid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        obill.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obill.schemeid = reader["schemeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["schemeid"]);
                        obill.tid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        obill.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"].ToString());
                        obill.price = reader["price"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["price"]);
                        obill.cost = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                        obill.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        obill.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"]);
                        obill.schemediscount = reader["schemediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemediscount"]);
                        obill.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        obill.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        obill.salerate = reader["salerate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salerate"]);
                        obill.saleamount = reader["saleamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["saleamount"]);
                        obill.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        obill.ptaxpercentage = reader["purchasetaxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasetaxpercentage"]);
                        obill.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"]);
                        obill.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        obill.bdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
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


        public Billhistory Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Billhistory oBillhistory)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistory.id;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistory.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistory.locationid;
            command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillhistory.fromdate;
            command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillhistory.todate;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillhistory.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistory.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistory.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillhistory.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillhistory.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillhistory.modifiedon;
            command.Parameters.Add("creteria", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillhistory.creteria_json == null ? "null" : oBillhistory.creteria_json;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillhistory.tax;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillhistory.receivable;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillhistory.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBillhistory;
        }

        internal BillHistoryInfo Bill4Update(SaleEdit item)
        {
            var info = new BillHistoryInfo();
            List<BillEdit> billeditList = new List<BillEdit>();
            foreach (var bitem in item.items.GroupBy(e => e.bid))
            {
                BillEdit be = new BillEdit()
                {
                    billid = bitem.Key,
                    items = bitem.ToList()
                };
                billeditList.Add(be);
            }

            BillDataService billDataService = new BillDataService()
            {
                Context = this.Context
            };

            var OLDBillList = billDataService.Search(new BillCreteria()
            {
                ids = billeditList.ConvertAll(e => e.billid),
                isimport = true,
                removecancelled = true
            });

            BillsettlementmasterDataService billsettlementDataService = new BillsettlementmasterDataService()
            {
                Context = this.Context
            };
            var OLDSettlementList = billsettlementDataService.Search(new BillsettlementmasterCreteria()
            {
                billids = billeditList.ConvertAll(e => e.billid)
            });



            foreach (var obill in OLDBillList)
            {
                var bill = billeditList.Find(e => e.billid == obill.id);
                bill.OLDBill = obill;
                if (OLDSettlementList.Exists(e => e.Billsettlement.Exists(x => x.billid == obill.id)))
                {
                    bill.OLDSettlement = OLDSettlementList.Find(e => e.Billsettlement.Exists(x => x.billid == obill.id));
                    bill.NewSettlement = Newtonsoft.Json.JsonConvert.DeserializeObject<Billsettlementmaster>(Newtonsoft.Json.JsonConvert.SerializeObject(obill));
                }
                bill.NewBill = Newtonsoft.Json.JsonConvert.DeserializeObject<Bill>(Newtonsoft.Json.JsonConvert.SerializeObject(obill));
            }

            foreach (var bitem in billeditList)
            {
                var bill = bitem.NewBill;
                var settlement = bitem.NewSettlement;
                if (bill.Billitems.Count == bitem.items.Count && bitem.items.All(e => e.deleted))
                {
                    bitem.delete = true;
                }
                else
                {
                    foreach (var mbillitem in bitem.items)
                    {
                        var nbillitem = bill.Billitems.Find(e => e.id == mbillitem.id);
                        nbillitem.price = mbillitem.price;
                        nbillitem.additionaldiscount = mbillitem.additionaldiscount;
                        nbillitem.schemediscount = mbillitem.schemediscount;
                        nbillitem.schemeid = mbillitem.schemeid;
                        nbillitem.tax = mbillitem.tax;
                        nbillitem.itemtax = mbillitem.tax / mbillitem.qty;
                        nbillitem.receivable = mbillitem.receivable;
                        nbillitem.rate = (mbillitem.receivable - mbillitem.tax) / mbillitem.qty;
                        nbillitem.taxpercentage = mbillitem.taxpercentage;
                        nbillitem.discount = mbillitem.discount;
                        nbillitem.isactive = !mbillitem.deleted;
                    }
                    bill.totalqty = bill.qty = bill.Billitems.Sum(e => !e.isactive ? 0 : e.qty);
                    bill.totalpiece = bill.Billitems.Sum(e => !e.isactive ? 0 : e.piece);
                    bill.additionaldiscount = bill.Billitems.Sum(e => !e.isactive ? 0 : (e.additionaldiscount * e.qty));
                    bill.discount = bill.Billitems.Sum(e => !e.isactive ? 0 : e.discount);
                    bill.schemediscount = bill.Billitems.Sum(e => !e.isactive ? 0 : e.schemediscount);
                    bill.tax = bill.Billitems.Sum(e => !e.isactive ? 0 : e.tax);
                    bill.receivable = bill.Billitems.Sum(e => !e.isactive ? 0 : e.receivable);
                    bill.rounding = Math.Round(bill.receivable, MidpointRounding.AwayFromZero) - bill.receivable;
                    bill.RecalculateTax();

                    if (settlement != null)
                    {
                        var reducedamount = bitem.OLDBill.receivable - bitem.NewBill.receivable;
                        var reducetax = bitem.OLDBill.tax - bitem.NewBill.tax;
                        if (settlement.Billsettlement.Count == 1)
                        {
                            settlement.paidamount -= reducedamount;
                            settlement.receivable -= reducedamount;
                            settlement.tax -= reducetax;
                            settlement.Billsettlement[0].billamount -= reducedamount;
                            settlement.Billsettlement[0].paid -= reducedamount;
                            settlement.Billsettlement[0].receivable -= reducedamount;

                            bool altered = false;
                            if (settlement.paymentinfo.HasCash && !settlement.paymentinfo.HasCard)
                            {
                                if (settlement.paymentinfo.cash_paid - reducedamount >= 0)
                                {
                                    settlement.paymentinfo.cash_paid -= reducedamount;
                                    altered = true;
                                }
                            }
                            else if (!settlement.paymentinfo.HasCash && settlement.paymentinfo.HasCard)
                            {
                                if (settlement.paymentinfo.card_paid - reducedamount >= 0)
                                {
                                    settlement.paymentinfo.card_paid -= reducedamount;
                                    altered = true;
                                }
                            }
                            if (!altered)
                            {
                                bitem.rejected = true;
                            }
                        }
                        else
                        {
                            bitem.multiplesettlement = true;
                            bitem.settlementmasterid = settlement.id;
                        }
                    }
                }
            }

            var multiplesettlementlist = billeditList.FindAll(e => e.multiplesettlement).ToList();
            if (multiplesettlementlist.Count > 0)
            {
                foreach (var multibill in multiplesettlementlist.GroupBy(e => e.settlementmasterid))
                {
                    var reducedamount = multibill.Sum(e => e.OLDBill.receivable - e.NewBill.receivable);
                    var reducetax = multibill.Sum(e => e.OLDBill.tax - e.NewBill.tax);
                    var settlement = multibill.First().NewSettlement;

                    settlement.paidamount -= reducedamount;
                    settlement.receivable -= reducedamount;
                    settlement.tax -= reducetax;

                    bool altered = false;
                    if (settlement.paymentinfo.HasCash && !settlement.paymentinfo.HasCard)
                    {
                        if (settlement.paymentinfo.cash_paid - reducedamount >= 0)
                        {
                            settlement.paymentinfo.cash_paid -= reducedamount;
                            altered = true;
                        }
                    }
                    else if (!settlement.paymentinfo.HasCash && settlement.paymentinfo.HasCard)
                    {
                        if (settlement.paymentinfo.card_paid - reducedamount >= 0)
                        {
                            settlement.paymentinfo.card_paid -= reducedamount;
                            altered = true;
                        }
                    }
                    foreach (var sbill in multibill)
                    {
                        if (!altered)
                        {
                            sbill.rejected = true;
                        }
                        else
                        {
                            var billsettlement = sbill.NewSettlement.Billsettlement.Find(e => e.billid == sbill.billid);
                            var breduced = sbill.OLDBill.receivable - sbill.NewBill.receivable;
                            var treduced = sbill.OLDBill.tax - sbill.NewBill.tax;
                            billsettlement.billamount -= breduced;
                            billsettlement.paid -= breduced;
                            billsettlement.receivable -= breduced;
                        }
                    }
                }
            }

            var billhistory = new Billhistory()
            {
                companyid = item.companyid,
                locationid = item.locationid,
                creteria = item,
                fromdate = item.billdatefrom,
                todate = item.billdateto,
                createdby = Context.UserID,
                createdon = DateTimeNow,
                isactive = true,
                version = 1,
                modifiedby = Context.UserID,
                modifiedon = DateTimeNow,
                receivable = billeditList.Sum(e => e.delete ? e.OLDBill.receivable : (e.OLDBill.receivable - e.NewBill.receivable)),
                tax = billeditList.Sum(e => e.delete ? e.OLDBill.tax : (e.OLDBill.tax - e.NewBill.tax))
            };

            billhistory.Billhistoryitems = billeditList.Where(e => !e.rejected).ToList().ConvertAll(e => new Billhistoryitems()
            {
                createdby = Context.UserID,
                createdon = DateTimeNow,
                isactive = true,
                version = 1,
                modifiedby = Context.UserID,
                modifiedon = DateTimeNow,
                oldbilldata = e.OLDBill,
                newbilldata = e.NewBill,
                oldsettlementdata = e.OLDSettlement,
                newsettlementdata = e.NewSettlement,
                billid = e.OLDBill.id,
                billsettlementmasterid = e.OLDSettlement != null ? e.OLDSettlement.id : 0,
                multiplesettlement = e.multiplesettlement,
                deleted = e.delete
            });
            Insert(billhistory);
            info.deletedbillcount = billeditList.Count(e => e.delete);
            info.deletedbillitemcount = billeditList.Sum(e => e.delete ? e.OLDBill.Billitems.Count : (e.OLDBill.Billitems.Count - e.NewBill.Billitems.Count));
            info.reducedamount = billhistory.receivable;
            info.reducedtax = billhistory.tax;
            info.rejectedbillcount = billeditList.Where(e => !e.rejected).Count();
            info.rejectedbillitemcount = billeditList.Sum(e => e.rejected ? e.OLDBill.Billitems.Count : 0);
            info.rejectedbillamount = billeditList.Sum(e => e.rejected ? e.OLDBill.receivable : 0);
            return info;
        }

        public bool Delete(Billhistory oBillhistory)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBillhistory);
                    BillhistoryitemsDataService subBillhistoryitems = new BillhistoryitemsDataService() { Context = this.Context };
                    subBillhistoryitems.DeleteAll(oBillhistory.id, oBillhistory.modifiedby, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillhistory);

            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Billhistory oBillhistory)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillhistory.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistory.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillhistory.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillhistory.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillhistory.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
