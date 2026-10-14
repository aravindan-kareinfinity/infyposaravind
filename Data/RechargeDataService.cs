using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class RechargeDataService : SAASData
    {
        const string SQL_Search = @"select isactive, createdby, modifiedby, version, createdon, modifiedon, id, supplierid, productid, itemid, schemeid, rate, cashback, receivedemployeeid, 
        approvedemployeeid, rechargedate, consumedbillid, consumedbillno, consumeduserid, rechargekey, rechargeno, customerid, customername,
        customerphoneno, operatorname, schemename, locationid, companyid, customeraddress, amount, fromlocationid,billvalue,invoiceno,taxid,lrinvoiceid, 
        tolocationid, tranmode, rechargecode, description,paid_cash,upiproviderid,paid_upi,paid_card,swipmachineid,swipbankid,discount,isinterstatesale from recharge where id=@id";

        const string SQL_Insert = @"INSERT INTO public.recharge(isactive, createdby, modifiedby, version, createdon, modifiedon, supplierid, productid, itemid, schemeid, rate, cashback, receivedemployeeid, approvedemployeeid, rechargedate, consumedbillid, consumedbillno, consumeduserid, rechargekey, rechargeno, customerid, customername, customerphoneno, operatorname, schemename, locationid, companyid, customeraddress, amount, fromlocationid, tolocationid, tranmode, rechargecode, description,paid_cash,upiproviderid,paid_upi,paid_card,swipmachineid,swipbankid,discount,billvalue,invoiceno,taxid,lrinvoiceid,isinterstatesale) 
        VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @supplierid, @productid, @itemid, @schemeid, @rate, @cashback, @receivedemployeeid, @approvedemployeeid, @rechargedate, @consumedbillid, @consumedbillno, @consumeduserid, @rechargekey, @rechargeno, @customerid, @customername, @customerphoneno, @operatorname, @schemename, @locationid, @companyid, @customeraddress, @amount, @fromlocationid, @tolocationid, @tranmode, @rechargecode, @description,@paid_cash,@upiproviderid,@paid_upi,@paid_card,@swipmachineid,@swipbankid,@discount,@billvalue,@invoiceno,@taxid,@lrinvoiceid,@isinterstatesale) RETURNING id";

        const string SQL_Update = @"UPDATE public.recharge SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, supplierid=@supplierid, productid=@productid, itemid=@itemid, schemeid=@schemeid, rate=@rate, cashback=@cashback, receivedemployeeid=@receivedemployeeid, approvedemployeeid=@approvedemployeeid, discount = @discount,
        rechargedate=@rechargedate, consumedbillid=@consumedbillid, consumedbillno=@consumedbillno, consumeduserid=@consumeduserid, rechargekey=@rechargekey, rechargeno=@rechargeno, customerid=@customerid, customername=@customername, customerphoneno=@customerphoneno, operatorname=@operatorname, schemename=@schemename, locationid=@locationid,
        companyid=@companyid, customeraddress=@customeraddress, amount=@amount, fromlocationid=@fromlocationid, tolocationid=@tolocationid, tranmode=@tranmode, rechargecode=@rechargecode, description=@description,paid_cash=@paid_cash,upiproviderid=@upiproviderid,paid_upi=@paid_upi,paid_card=@paid_card,swipmachineid=@swipmachineid,swipbankid=@swipbankid,
        billvalue=@billvalue,invoiceno=@invoiceno,taxid=@taxid,lrinvoiceid=@lrinvoiceid,isinterstatesale=@isinterstatesale
        where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.recharge set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Recharge> Search(RechargeCreteria _recharge)
        {
            List<Recharge> lst = new List<Recharge>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_recharge.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _recharge.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_recharge.tranmode > 0)
                        qb.AddParameter("tranmode", _recharge.tranmode, "=");

                    if (_recharge.schemeid > 0)
                        qb.AddParameter("schemeid", _recharge.schemeid, "=");

                    if (_recharge.tranmode == 1 && _recharge.locationid > 0)
                        qb.AddParameter("locationid", _recharge.locationid, "=");
                    else if (_recharge.tranmode == 2 && _recharge.locationid > 0)
                        qb.AddParameter("tolocationid", _recharge.locationid, "=");
                    else if (_recharge.tranmode == 3 && _recharge.locationid > 0)
                        qb.AddParameter("fromlocationid", _recharge.locationid, "=");
                    else if(_recharge.locationid>0)
                        qb.AddParameter("locationid", _recharge.locationid, "=");

                    if (_recharge.fromlocationid > 0)
                        qb.AddParameter("fromlocationid", _recharge.fromlocationid, "=");

                    if (_recharge.tolocationid > 0)
                        qb.AddParameter("tolocationid", _recharge.tolocationid, "=");

                    if(_recharge.fromdate>DateTime.MinValue)
                        qb.AddParameter("rechargedate", _recharge.fromdate.Date, ">=");

                    if(_recharge.todate>DateTime.MinValue)
                        qb.AddParameter("rechargedate", _recharge.todate.Date.AddDays(1), "<");

                    if(_recharge.rechargedate>DateTime.MinValue)
                    {
                        qb.AddParameter("rechargedate", _recharge.rechargedate.Date, ">=");
                        qb.AddParameter("rechargedate", _recharge.rechargedate.Date.AddDays(1), "<");
                    }

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Recharge orecharge = new Recharge();
                        orecharge.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        orecharge.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        orecharge.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        orecharge.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        orecharge.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        orecharge.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        orecharge.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        orecharge.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        orecharge.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);

                        orecharge.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"]);
                        orecharge.taxid = reader["taxid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["taxid"]);
                        orecharge.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"]);
                        orecharge.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        orecharge.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);

                        orecharge.itemid = reader["itemid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["itemid"]);
                        orecharge.schemeid = reader["schemeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["schemeid"]);
                        orecharge.rate = reader["rate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rate"]);
                        orecharge.cashback = reader["cashback"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["cashback"]);
                        orecharge.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        orecharge.receivedemployeeid = reader["receivedemployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedemployeeid"]);
                        orecharge.approvedemployeeid = reader["approvedemployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedemployeeid"]);
                        orecharge.rechargedate = reader["rechargedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["rechargedate"]);
                        orecharge.consumedbillid = reader["consumedbillid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["consumedbillid"]);
                        orecharge.consumedbillno = reader["consumedbillno"] == DBNull.Value ? "" : reader["consumedbillno"].ToString();
                        orecharge.consumeduserid = reader["consumeduserid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["consumeduserid"]);
                        orecharge.rechargekey = reader["rechargekey"] == DBNull.Value ? "" : reader["rechargekey"].ToString();
                        orecharge.rechargeno = reader["rechargeno"] == DBNull.Value ? "" : reader["rechargeno"].ToString();
                        orecharge.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        orecharge.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        orecharge.customerphoneno = reader["customerphoneno"] == DBNull.Value ? "" : reader["customerphoneno"].ToString();
                        orecharge.operatorname = reader["operatorname"] == DBNull.Value ? "" : reader["operatorname"].ToString();
                        orecharge.schemename = reader["schemename"] == DBNull.Value ? "" : reader["schemename"].ToString();
                        orecharge.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        orecharge.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        orecharge.customeraddress = reader["customeraddress"] == DBNull.Value ? "" : reader["customeraddress"].ToString();
                        orecharge.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);

                        orecharge.paid_card = reader["paid_card"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid_card"]);
                        orecharge.paid_cash = reader["paid_cash"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid_cash"]);
                        orecharge.swipbankid = reader["swipbankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["swipbankid"]);
                        orecharge.swipmachineid = reader["swipmachineid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["swipmachineid"]);


                        orecharge.paid_upi = reader["paid_cash"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid_upi"]);
                        orecharge.upiproviderid = reader["upiproviderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["upiproviderid"]);

                        orecharge.fromlocationid = reader["fromlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromlocationid"]);
                        orecharge.tolocationid = reader["tolocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tolocationid"]);
                        orecharge.tranmode = reader["tranmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["tranmode"]);
                        orecharge.rechargecode = reader["rechargecode"] == DBNull.Value ? "" : reader["rechargecode"].ToString();
                        orecharge.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        lst.Add(orecharge);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _recharge);
            }
            return lst;
        }

        public Recharge Insert(RechargeCreteria oRecharge)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    
                    switch(oRecharge.mode)
                    {
                        case Recharge.TranModes.Recharge:
                            oRecharge.description = "Recharge to " + (string.IsNullOrEmpty(oRecharge.rechargecode) ? oRecharge.customerphoneno : oRecharge.rechargecode);
                            if(oRecharge.customerid == 0)
                            {
                                if(!string.IsNullOrEmpty(oRecharge.customerphoneno) && !string.IsNullOrEmpty(oRecharge.customername))
                                {
                                    CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
                                    var customers = customerDataService.Search(new CustomerCreteria()
                                    {
                                        mobileno = oRecharge.customerphoneno
                                    });
                                    if(customers.Count == 0)
                                    {
                                        var customer = customerDataService.Insert(transaction, conn, new Customer()
                                        {
                                            isactive = true,
                                            createdby = Context.UserID,
                                            modifiedby = Context.UserID,
                                            createdon = DateTimeNow,
                                            modifiedon = DateTimeNow,
                                            version = 1,
                                            mobileno = oRecharge.customerphoneno,
                                            name = oRecharge.customername,
                                            address = oRecharge.customeraddress
                                        });
                                        oRecharge.customerid = customer.id;
                                    }
                                    else
                                    {
                                        oRecharge.customerid = customers[0].id;
                                    }
                                }
                            }
                            break;
                        case Recharge.TranModes.Topup:
                            oRecharge.description = "Main balance topup";
                            oRecharge.fromlocation = 0;
                            oRecharge.tolocationid = 0;
                            CreateLRInvoice(transaction, conn, oRecharge);
                            break;
                        case Recharge.TranModes.Transfer:
                            oRecharge.description = "Transfer from " + oRecharge.fromlocation + " - to " + oRecharge.tolocation;
                            break;
                    }

                    Insert(transaction, conn, oRecharge);
                    RechargebalanceDataService rechargebalanceDataService = new RechargebalanceDataService() { Context = this.Context };
                    var balance = rechargebalanceDataService.UpdateBalance(transaction, conn, oRecharge);
                    if (balance < 0)
                    {
                        oRecharge.id = 0;
                        oRecharge.errormessage = "Negative Balance - " + balance.ToString("F2");
                        transaction.Rollback();
                        conn.Close();
                    }
                    else
                    {
                        transaction.Commit();
                        conn.Close();
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oRecharge);
                oRecharge.errormessage = exp.Message;
            }

            return oRecharge;
        }

        public Recharge CreateLRInvoice(NpgsqlTransaction transaction, NpgsqlConnection conn, RechargeCreteria oRecharge)
        {
            TaxDataService taxDataService = new TaxDataService() { Context = this.Context };
            var searchresult = taxDataService.Search(new TaxCreteria()
            {
                id = oRecharge.taxid
            });
            SuppliercompanyDataService suppliercompanyDataService = new SuppliercompanyDataService() { Context = this.Context };
            var suppliercompany = suppliercompanyDataService.Search(new SuppliercompanyCreteria()
            {
                supplierid = oRecharge.supplierid
            });
            var taxpercentage = searchresult.First().taxpercentage;
            if(taxpercentage==0)
            {
                taxpercentage = searchresult.First().taxsplit.items.Sum(e => e.taxpercent);
            }
            LrinvoiceDataService lrinvoiceDataService = new LrinvoiceDataService() { Context = this.Context };
            var item = new Lrinvoice();
            item.supplierid = oRecharge.supplierid;
            item.isinterstatesale = oRecharge.isinterstatesale;
            item.invoicedate = oRecharge.rechargedate;
            item.invoiceno = oRecharge.invoiceno;
            item.suppliercompanyid = suppliercompany.Count > 0 ? suppliercompany.First().id : 0;
            item.isactive = true;
            item.lrsupplierid = oRecharge.supplierid;
            item.totalamount = oRecharge.billvalue;
            item.companyid = Context.CompanyID;
            item.createdby = Context.UserID;
            item.modifiedby = Context.UserID;
            item.createdon = DateTimeNow;
            item.modifiedon = DateTimeNow;
            item.taxamount = (oRecharge.billvalue/ (100+taxpercentage))* taxpercentage;
            item.Lrinvoicetaxitems = new List<Lrinvoicetax>();
            item.Lrinvoicetaxitems.Add(new Lrinvoicetax()
            {
                taxid = oRecharge.taxid,
                taxamount = item.taxamount,
                percentage = taxpercentage,
                isinterstatesale = item.isinterstatesale,
                netamount = item.totalamount,
                amounton = item.totalamount - item.taxamount,
                isactive = true,
                type = 0,
            });
            lrinvoiceDataService.Insert(transaction, conn, item);
            LrinvoicetaxDataService subLrinvoicetaxs = new LrinvoicetaxDataService() { Context = this.Context };
            subLrinvoicetaxs.Context = this.Context;
            foreach (var titem in item.Lrinvoicetaxitems)
            {
                titem.invoiceid = item.id;
                titem.createdon = item.createdon;
                titem.modifiedon = DateTimeNow;
                titem.createdby = Context.UserID;
                subLrinvoicetaxs.Insert(transaction, conn, titem);
            }
            oRecharge.lrinvoiceid = item.id;
            return oRecharge;
        }
        public Recharge Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, RechargeCreteria oRecharge)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oRecharge.isactive;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oRecharge.isinterstatesale;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRecharge.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRecharge.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRecharge.modifiedon;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.supplierid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.productid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.itemid;
            command.Parameters.Add("schemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.schemeid;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRecharge.rate;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRecharge.discount;
            command.Parameters.Add("cashback", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRecharge.cashback;
            command.Parameters.Add("receivedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.receivedemployeeid;
            command.Parameters.Add("approvedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.approvedemployeeid;
            command.Parameters.Add("rechargedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRecharge.rechargedate;
            command.Parameters.Add("consumedbillid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.consumedbillid;
            command.Parameters.Add("consumedbillno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.consumedbillno == null ? "" : oRecharge.consumedbillno;
            command.Parameters.Add("consumeduserid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.consumeduserid;
            command.Parameters.Add("rechargekey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.rechargekey == null ? "" : oRecharge.rechargekey;
            command.Parameters.Add("rechargeno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.rechargeno == null ? "" : oRecharge.rechargeno;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.customerid;
            command.Parameters.Add("customername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.customername == null ? "" : oRecharge.customername;
            command.Parameters.Add("customerphoneno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.customerphoneno == null ? "" : oRecharge.customerphoneno;
            command.Parameters.Add("operatorname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.operatorname == null ? "" : oRecharge.operatorname;
            command.Parameters.Add("schemename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.schemename == null ? "" : oRecharge.schemename;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.companyid;
            command.Parameters.Add("customeraddress", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.customeraddress == null ? "" : oRecharge.customeraddress;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRecharge.amount;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.fromlocationid;
            command.Parameters.Add("tolocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.tolocationid;

            command.Parameters.Add("paid_cash", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRecharge.paid_cash;
            command.Parameters.Add("paid_card", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRecharge.paid_card;
            command.Parameters.Add("swipbankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.swipbankid;
            command.Parameters.Add("swipmachineid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.swipmachineid;

            command.Parameters.Add("paid_upi", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRecharge.paid_upi;
            command.Parameters.Add("upiproviderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.upiproviderid;

            command.Parameters.Add("billvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRecharge.billvalue;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.taxid;
            command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.lrinvoiceid;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.invoiceno == null ? "" : oRecharge.invoiceno;

            command.Parameters.Add("tranmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRecharge.tranmode;
            command.Parameters.Add("rechargecode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.rechargecode == null ? "" : oRecharge.rechargecode;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.description == null ? "" : oRecharge.description;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oRecharge.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oRecharge;
        }

        public Recharge Update(Recharge oRecharge)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oRecharge);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oRecharge);
                throw exp;
            }

            return oRecharge;
        }

        public Recharge Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Recharge oRecharge)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oRecharge.isinterstatesale;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oRecharge.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRecharge.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRecharge.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRecharge.modifiedon;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.id;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.supplierid;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.productid;
            command.Parameters.Add("itemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.itemid;
            command.Parameters.Add("schemeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.schemeid;
            command.Parameters.Add("rate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRecharge.rate;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRecharge.discount;
            command.Parameters.Add("cashback", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRecharge.cashback;
            command.Parameters.Add("receivedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.receivedemployeeid;
            command.Parameters.Add("approvedemployeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.approvedemployeeid;
            command.Parameters.Add("rechargedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRecharge.rechargedate;
            command.Parameters.Add("consumedbillid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.consumedbillid;
            command.Parameters.Add("consumedbillno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.consumedbillno == null ? "" : oRecharge.consumedbillno;
            command.Parameters.Add("consumeduserid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.consumeduserid;
            command.Parameters.Add("rechargekey", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.rechargekey == null ? "" : oRecharge.rechargekey;
            command.Parameters.Add("rechargeno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.rechargeno == null ? "" : oRecharge.rechargeno;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.customerid;
            command.Parameters.Add("customername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.customername == null ? "" : oRecharge.customername;
            command.Parameters.Add("customerphoneno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.customerphoneno == null ? "" : oRecharge.customerphoneno;
            command.Parameters.Add("operatorname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.operatorname == null ? "" : oRecharge.operatorname;
            command.Parameters.Add("schemename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.schemename == null ? "" : oRecharge.schemename;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.companyid;
            command.Parameters.Add("customeraddress", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.customeraddress == null ? "" : oRecharge.customeraddress;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRecharge.amount;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.fromlocationid;
            command.Parameters.Add("tolocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.tolocationid;
            command.Parameters.Add("tranmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRecharge.tranmode;
            command.Parameters.Add("paid_cash", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRecharge.paid_cash;
            command.Parameters.Add("paid_card", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRecharge.paid_card;
            command.Parameters.Add("swipbankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.swipbankid;
            command.Parameters.Add("swipmachineid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.swipmachineid;

            command.Parameters.Add("paid_upi", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRecharge.paid_upi;
            command.Parameters.Add("upiproviderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.upiproviderid;


            command.Parameters.Add("rechargecode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.rechargecode == null ? "" : oRecharge.rechargecode;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.description == null ? "" : oRecharge.description;

            command.Parameters.Add("billvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oRecharge.billvalue;
            command.Parameters.Add("taxid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.taxid;
            command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.lrinvoiceid;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oRecharge.invoiceno == null ? "" : oRecharge.invoiceno;


            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oRecharge.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oRecharge;
        }


        public bool Delete(RechargeCreteria oRecharge)
        {
            var sourcelist = Search(new RechargeCreteria()
            {
                id = oRecharge.id
            });
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    
                    hasSuccessCount = Delete(transaction, conn, oRecharge);
                    if(sourcelist.Count == 1 && oRecharge.tranmode != 1)
                    {
                        RechargebalanceDataService rechargebalanceDataService = new RechargebalanceDataService() { Context = this.Context };
                        sourcelist[0].amount = sourcelist[0].amount * -1;
                        sourcelist[0].rate = (sourcelist[0].rate- sourcelist[0].discount) * -1;
                        sourcelist[0].cashback = sourcelist[0].cashback * -1;
                        sourcelist[0].discount = sourcelist[0].discount * -1;
                        var balance = rechargebalanceDataService.UpdateBalance(transaction, conn, sourcelist[0]);
                        if (balance < 0)
                        {
                            oRecharge.id = 0;
                            oRecharge.errormessage = "Negative Balance - " + balance.ToString("F2");
                            transaction.Rollback();
                            conn.Close();
                        }
                        else
                        {
                            transaction.Commit();
                            conn.Close();
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oRecharge);
                oRecharge.errormessage = exp.Message;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Recharge oRecharge)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oRecharge.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oRecharge.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oRecharge.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oRecharge.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
