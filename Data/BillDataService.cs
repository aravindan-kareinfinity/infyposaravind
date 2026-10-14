using System;
using System.Collections.Generic;
using System.Linq;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using System.Net;
using InfyPOS.Processors;
using System.Text;
using InfyPOS.Services;
using Newtonsoft.Json;
using WebAPI.Data;
using System.Data;
using System.Threading.Tasks;

namespace InfyPOS.Data
{
    public class BillDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, billmasterid, billno, billdate, companyid, customerid, tax, discount, receivable, billtype,iscreditbill,tzoffset,
discountstatus,additionaldiscount,schemediscount,isinterstatesale,qty,locationid,rounding,counterid,returnreasonid,returnbillno,settled,HasPromotionApplied,billattributes,offline,isinterstatetransfer,hasemi,emidetail,floorid,additionalcharges,
isb2b,isreturn,iscancel,iscreditapproved,isexport,istransfer,customerstateid from bill where id=@id";

        internal string GetFBURL(string billno)
        {
            string mobileno = "";
            string name = "";
            string location = "";
            if (!string.IsNullOrEmpty(billno))
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = @"select billno,customer.mobileno,customer.name,referencelist.code from bill 
left outer join customer on customer.id = bill.customerid 
inner join referencelist on referencelist.id = bill.locationid ";

                    qb.AddParameter("bill.isactive", true, "=");
                    qb.AddParameter("bill.billno", billno, "=");
                    var command = qb.GetCommand(conn);

                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            billno = reader.GetString(0);
                            mobileno = reader.IsDBNull(1) ? "" : reader.GetString(1);
                            name = reader.IsDBNull(2) ? "" : reader.GetString(2);
                            location = reader.IsDBNull(3) ? "" : reader.GetString(3);
                        }
                    }
                    conn.Close();
                }
            }
            string server = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.hostedserver;
            // fburl = "HTTP://49.207.185.145/QUANTO/fb/q/0?r={BillNo}&n={Name}&m={Mobile}";

            string fburl = "";
            if (!string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.sms_fburl))
                fburl = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.sms_fburl;

            if (string.IsNullOrEmpty(fburl))
            {
                return string.Format("{0}/fb/q/{1}?r={2}&b={3}&n={4}&m={5}",
                    server, Context.OrganizationID,
                    WebUtility.UrlEncode(billno), WebUtility.UrlEncode(location),
                    WebUtility.UrlEncode(mobileno), WebUtility.UrlEncode(name));
            }
            else
            {
                string url = fburl.Replace("{Mobile}", WebUtility.UrlEncode(mobileno)).Replace("{Name}", WebUtility.UrlEncode(name)).Replace("{BillNo}", WebUtility.UrlEncode(billno));
                return url;
            }
        }

        const string SQL_SearchByMasterCode = @"select bill.id, bill.isactive, bill.createdby, bill.modifiedby, bill.version, bill.createdon, bill.modifiedon,bill.iscreditbill,
                                            billmasterid, bill.billno, bill.billdate,bill.isinterstatetransfer,iscreditapproved,
                                            bill.companyid, bill.customerid, bill.tax, bill.discount, bill.receivable, bill.billtype,
                                            discountstatus,additionaldiscount,schemediscount,isinterstatesale,qty,bill.locationid,rounding,counterid,returnreasonid,returnbillno,
                                            settled,HasPromotionApplied,billattributes,offline,emidetail,hasemi,floorid,additionalcharges,isb2b,bill.tzoffset,isexport,istransfer,customerstateid from billmaster
                                            inner join bill on bill.billmasterid = billmaster.id";

        const string SQL_Insert = @"INSERT INTO public.bill(isactive, createdby, modifiedby, version, createdon, modifiedon, billmasterid, billno, billdate, companyid,customerid, tax, discount, receivable,billtype,discountstatus,additionaldiscount,schemediscount,isinterstatesale,qty,locationid,settled,rounding,counterid,returnreasonid,returnbillno,HasPromotionApplied,salereasonid,areaid,seasonid,billattributes,offline,isinterstatetransfer,unsettledcancelled,hasemi,emidetail,floorid,additionalcharges,isb2b,isreturn,iscreditbill,iscancel,iscreditapproved,tzoffset,unlimitedcouponid,customerstateid,ecomorderno,istransfer) 
                                    VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @billmasterid, @billno, @billdate, @companyid, @customerid , @tax, @discount, @receivable,@billtype,@discountstatus,@additionaldiscount,@schemediscount,@isinterstatesale,@qty,@locationid,false,@rounding,@counterid,@returnreasonid,@returnbillno,@HasPromotionApplied,@salereasonid,@areaid,@seasonid,@billattributes,@offline,@isinterstatetransfer,@unsettledcancelled,@hasemi,@emidetail,@floorid,@additionalcharges,@isb2b,@isreturn,@iscreditbill,@iscancel,@iscreditapproved,@tzoffset,@unlimitedcouponid,@customerstateid,@ecomorderno,@istransfer) RETURNING id";



        const string SQL_Update = @"UPDATE public.bill SET modifiedby=@modifiedby, customerid=@customerid, version=@version+1,isinterstatetransfer=@isinterstatetransfer, modifiedon=@modifiedon, billmasterid=@billmasterid,rounding=@rounding,billno=@billno,
        billdate=@billdate, companyid=@companyid, tax=@tax, discount=@discount, additionalcharges=@additionalcharges,isinterstatesale=@isinterstatesale,receivable=@receivable, billtype=@billtype,discountstatus=@discountstatus,
        additionaldiscount=@additionaldiscount,qty=@qty,schemediscount=@schemediscount,
        locationid=@locationid,counterid=@counterid,returnreasonid=@returnreasonid,returnbillno=@returnbillno,billattributes=@billattributes,offline=@offline,HasPromotionApplied=@HasPromotionApplied,
        areaid=@areaid,emidetail=@emidetail,floorid=@floorid,hasemi=@hasemi,isb2b=@isb2b,iscancel=@iscancel,iscreditapproved=@iscreditapproved,iscreditbill=@iscreditbill,isreturn=@isreturn,
        salereasonid=@salereasonid,seasonid=@seasonid where id=@id and version=@version  RETURNING version";

        const string SQL_Delete = @"update public.bill set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id RETURNING version";

        const string SQL_Settled = @"update public.bill set version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon,settled=true where id=@id and settled=false";

        const string SQL_Cancel = @"update public.bill set version=version+1,modifiedby=@modifiedby,modifiedon=@modifiedon,billtype=9,unsettledcancelled=true where id=@id and settled=false and billtype!=9";

        const string SQL_SearchSalesReport = @"select bill.id,bill.billno ,bill.billdate ,bill.companyid ,bill.tax ,bill.discount,
                                               bill.receivable,  bill.additionaldiscount,bill.counterid,bill.schemediscount,
                                               billitems.barcode ,billitems.printingname ,billitems.price ,billitems.discount,
                                               billitems.receivable,billitems.companyid,billitems.rate,
                                               items.brandid,items.patternid, isinterstatesale,
                                               items.styleid,items.materialid,items.typeid,items.productid,
                                               stock.price,stock.buyingprice buyingprice,stock.qty,stock.supplierid
                                               from bill inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                                               INNER JOIN stock on stock.id = billitems.stockid
                                               left outer join items on items.id = stock.itemid";

        const string SQL_SearchItems = @"select bill.id,bill.billno,bill.billdate,bill.tax,bill.discount,bill.receivable,bill.companyid,
billitems.barcode,billitems.printingname,billitems.price,billitems.discount,billitems.tax,billitems.qty,billitems.rate,stock.itemid,stock.supplierid,stock.productid,
                                    items.brandid,items.patternid,items.productid,items.colourid,isinterstatesale, 
                                    items.styleid,items.sizeid,items.materialid,bill.counterid,items.typeid
                                    from bill 
                                    inner join billitems on billitems.id = billitems.billid and billitems.isactive=true
                                    inner join stock on billitems.stockid = stock.id
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
        internal List<Bill> UpdateCustomer(CustomerChange item)
        {
            NpgsqlTransaction transaction = null;

            if (item == null || item.ids == null || item.customerid == 0 || item.ids.Count == 0)
            {
                throw new Exception("Invalid input data for customer change");
            }
            List<Bill> lstbill = Search(new BillCreteria() { ids = item.ids, isimport = true, removecancelled = true });
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                foreach (long billid in item.ids)
                {
                    if (lstbill.Exists(e => e.id == billid))
                    {
                        string SQL = "Update bill set customerid=@customerid, version=version+1, modifiedby=@modifiedby, modifiedon=@modifiedon where id=@id and isactive=true and version=@version; ";
                        var oldbill = lstbill.Find(e => e.id == billid);
                        if ((oldbill.isb2b || oldbill.billtype == Bill.BillTypes.Credit) && oldbill.customerid > 0)
                            SQL += "Update customerinvoice set customerid=@customerid, version=version+1, modifiedby=@modifiedby, modifiedon=@modifiedon where billid=@id and paid=0;";

                        var command = GetCommand(SQL, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = billid;
                        command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.customerid;
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oldbill.version;
                        int successCount = command.ExecuteNonQuery();
                        if (successCount > 2) throw new Exception("Unknown error occured.");
                    }
                }

                transaction.Commit();
                conn.Close();

            }

            return new List<Bill>();
        }
        internal List<Bill> UpdateIGST(CustomerChange item)
        {
            NpgsqlTransaction transaction = null;

            if (item == null || item.ids == null || item.ids.Count == 0)
            {
                throw new Exception("Invalid bill information. Please select bill");
            }
            List<Bill> lstbill = Search(new BillCreteria() { ids = item.ids, isimport = true, removecancelled = true });
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                foreach (long billid in item.ids)
                {
                    if (lstbill.Exists(e => e.id == billid))
                    {
                        var oldbill = lstbill.Find(e => e.id == billid);
                        string SQL = "Update bill set isinterstatesale=@isinterstatesale, version=version+1, modifiedby=@modifiedby, modifiedon=@modifiedon where id=@id and isactive=true and version=@version; ";

                        var command = GetCommand(SQL, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = billid;
                        command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = (oldbill.isinterstatesale ? false : true);
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oldbill.version;
                        int successCount = command.ExecuteNonQuery();
                        if (successCount > 2) throw new Exception("Unknown error occured.");
                    }
                }

                transaction.Commit();
                conn.Close();
            }
            return new List<Bill>();
        }
        internal List<Bill> UpdateHSN(HSNChange item)
        {
            NpgsqlTransaction transaction = null;

            if (item == null || item.barcodes == null || item.barcodes.Count == 0)
            {
                throw new Exception("Please select barcode to be updated.");
            }
            if (string.IsNullOrEmpty(item.hsncode))
                throw new Exception("Please enter HSN code to be updated.");

            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                foreach (string barcode in item.barcodes)
                {
                    if (!string.IsNullOrEmpty(barcode))
                    {
                        string SQL = "Update billitems set hsncode=@hsncode, version=version+1, modifiedby=@modifiedby, modifiedon=@modifiedon where barcode=@barcode; ";

                        var command = GetCommand(SQL, conn, transaction);
                        command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = barcode;
                        command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = item.hsncode;
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        int successCount = command.ExecuteNonQuery();
                    }
                }

                transaction.Commit();
                conn.Close();
            }
            return new List<Bill>();
        }

        internal List<Bill> UpdateBillHSN(CustomerChange item)
        {
            NpgsqlTransaction transaction = null;

            if (item == null || item.ids == null || item.ids.Count == 0)
            {
                throw new Exception("Invalid bill information. Please select bill");
            }
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                foreach (long billid in item.ids)
                {
                    string SQL = "update billitems set hsncode = products.hsncode, version=billitems.version+1, modifiedby = @modifiedby, modifiedon = @modifiedon from products where products.id = billitems.productid and billitems.billid = @billid and billitems.isactive=true;";

                    var command = GetCommand(SQL, conn, transaction);
                    command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = billid;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    int successCount = command.ExecuteNonQuery();
                }

                transaction.Commit();
                conn.Close();
            }
            return new List<Bill>();
        }

        internal Bill UpdateBillasCredit(Bill obill)
        {
            if (obill.id == 0)
                throw new Exception("Invalid bill id.");

            List<Bill> lstbill = Search(new BillCreteria() { id = obill.id });
            if (lstbill == null || lstbill.Count == 0)
                throw new Exception("Invalid bill id.");
            if (lstbill[0].billtype != Bill.BillTypes.Cash || lstbill[0].settled)
                throw new Exception("Bill is not a cash bill or either settled");
            if (lstbill[0].customerid == 0 && obill.customerid == 0 && string.IsNullOrEmpty(obill.phoneno))
                throw new Exception("Bill is not associated with Customer.");

            if (obill.customerid == 0 && !string.IsNullOrEmpty(obill.phoneno))
            {
                CustomerDataService customerData = new CustomerDataService() { Context = this.Context };
                var cust = new Models.Customer();
                cust.name = string.IsNullOrEmpty(obill.customername) ? "New Customer" : obill.customername;
                cust.mobileno = obill.phoneno;
                cust.isactive = true;
                cust.version = 1;
                cust.createdon = DateTimeNow;
                cust.customertype = 4;

                cust = customerData.Insert(cust);
                obill.customerid = cust.id;
            }

            if (lstbill[0].billtype == Bill.BillTypes.Cash && !lstbill[0].settled)
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlTransaction transaction = conn.BeginTransaction();
                    string SQL = "Update bill set billtype=1, iscreditbill=true, customerid=@customerid, version=version+1, modifiedby=@modifiedby, modifiedon=@modifiedon where id=@id and isactive=true and version=@version; ";

                    var command = GetCommand(SQL, conn, transaction);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obill.id;
                    command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = obill.customerid;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = lstbill[0].version;
                    int successCount = command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                }
            }
            return obill;
        }
        internal List<Bill> UpdateBillDate(CustomerChange item)
        {
            NpgsqlTransaction transaction = null;

            if (item == null || item.ids == null || item.billdate == DateTime.MinValue || item.ids.Count == 0)
            {
                throw new Exception("Invalid input data for bill date change");
            }
            List<Bill> lstbill = Search(new BillCreteria() { ids = item.ids, isimport = true, removecancelled = true });
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                foreach (long billid in item.ids)
                {
                    if (lstbill.Exists(e => e.id == billid))
                    {
                        string SQL = "Update bill set billdate=@billdate, version=version+1, modifiedby=@modifiedby, modifiedon=@modifiedon where id=@id and isactive=true and version=@version; ";
                        var oldbill = lstbill.Find(e => e.id == billid);
                        if ((oldbill.isb2b || oldbill.billtype == Bill.BillTypes.Credit) && oldbill.customerid > 0)
                            SQL += "Update customerinvoice set transactiondate=@billdate, version=version+1, modifiedby=@modifiedby, modifiedon=@modifiedon where billid=@id and paid=0;";
                        if (oldbill.settled)
                            SQL += @"Update billsettlementmaster set settlementon=@billdate, version=billsettlementmaster.version+1, modifiedby=@modifiedby, modifiedon=@modifiedon  from billsettlement
                                    where billsettlement.billsettlementmasterid = billsettlementmaster.id and billsettlement.billid=@id";

                        var command = GetCommand(SQL, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = billid;
                        command.Parameters.Add("billdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = item.billdate;
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oldbill.version;
                        int successCount = command.ExecuteNonQuery();
                        if (successCount > 3) throw new Exception("Unknown error occured.");
                    }
                }

                transaction.Commit();
                conn.Close();

            }

            return new List<Bill>();
        }
        internal List<Bill> UpdateSalesman(CustomerChange item)
        {
            NpgsqlTransaction transaction = null;

            if (item == null || item.ids == null || item.ids.Count == 0 || item.customerid == 0)
            {
                throw new Exception("Invalid input data for bill date change");
            }

            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                foreach (long billitemid in item.ids)
                {
                    string SQL = "Update billitems set salesmanid=@salesmanid, version=version+1, modifiedby=@modifiedby, modifiedon=@modifiedon where id=@billitemid and isactive=true; ";

                    var command = GetCommand(SQL, conn, transaction);
                    command.Parameters.Add("salesmanid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.customerid;
                    command.Parameters.Add("billitemid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = billitemid;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    int successCount = command.ExecuteNonQuery();
                }

                transaction.Commit();
                conn.Close();
            }
            return new List<Bill>();
        }

        internal List<Bill> Cancellation(ActionRequest<BillCancel> request)
        {
            if (request.Item.locationid == 0) return null;
            if (request.Item.ids == null || request.Item.ids.Count == 0)
                throw new Exception("Please select atleast one Bill for Cancellation");

            if (request.Item.ids.Count > 200)
                throw new Exception("Can't Cancel more than 200 Bills at once.");

            NpgsqlTransaction transaction = null;
            List<Bill> retvalue = null;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                transaction = conn.BeginTransaction();

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(request.Item.locationid) &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[request.Item.locationid].sale_disableoffsetbill)
                {
                    retvalue = CancelOnly(request, transaction, conn, true);
                }
                else
                {
                    retvalue = Cancellation(request, transaction, conn, null);
                }
                transaction.Commit();
                conn.Close();
            }
            return retvalue;
        }

        internal List<Bill> CancelOnly(ActionRequest<BillCancel> request, Npgsql.NpgsqlTransaction transaction,
            Npgsql.NpgsqlConnection connection, bool unsettledcancel)
        {
            var billlist = CancelOnly(request.Item, transaction, connection, unsettledcancel);
            return billlist;
        }

        public List<Bill> CancelOnly(BillCancel billcancelitem, NpgsqlTransaction transaction, NpgsqlConnection conn, bool unsettledcancel)
        {
            billcancelitem.ids.RemoveAll(x => x == 0);
            if (billcancelitem.ids.Count == 0) new List<Bill>();

            var billlist = Search(new BillCreteria()
            {
                ids = billcancelitem.ids,
                isb2b = true
            });
            if (billlist == null || billlist.Count == 0) return billlist;

            BillitemsDataService billitemdata = new BillitemsDataService() { Context = this.Context };
            billitemdata.Context = this.Context;
            //if (billlist.Exists(e => e.isb2b == true))
            //{
            //    throw new Exception("B2B Invoice can't be cancelled. Please use Invoice return.");
            //}
            if (billlist.Exists(e => e.billtype != Bill.BillTypes.Cancel && e.settled == true))
            {
                throw new Exception("Please cancel the settlement before cancel bill.");
            }
            foreach (var item in billlist)
            {
                if (item.billtype != Bill.BillTypes.Cancel && item.settled == false)
                {
                    if (item.billtype != Bill.BillTypes.Cancel &&
                        !billcancelitem.disablestockreverse)
                    {
                        if (item.Billitems.Exists(e => e.attributes != null &&
                e.attributes.so != null &&
                e.attributes.so.Count > 0))
                        {
                            string updatesoquery = string.Format(@"with soorderitems
                        as
                        (
                        select cast(schemelist->>'id' as bigint) id,cast(schemelist->>'qty' as decimal) qty from billitems 
                        LEFT JOIN LATERAL
                        json_array_elements (cast(billitems.attributes->>'so' as json)) schemelist ON true
                        where billid = {0} and isactive=true
                        )

                        update salesorderitems set bqty=bqty-soorderitems.qty,pqty=pqty+soorderitems.qty from soorderitems
                        where soorderitems.id = salesorderitems.id and bqty>0", item.id);

                            ExecuteQuery(updatesoquery, null, conn, transaction);
                        }
                    }

                    var cancelquery = @"Update bill set billtype=9,iscancel=true,unsettledcancelled=true,modifiedby=@modifiedby,modifiedon=@modifiedon where billtype != 9 and settled = false and isactive=true and id=" + item.id;
                    if (item.billtype == Bill.BillTypes.ReturnBill && unsettledcancel)
                        cancelquery = @"Update bill set billtype=9,iscancel=true,unsettledcancelled=true,modifiedby=@modifiedby,modifiedon=@modifiedon  where billtype != 9 and settled = false and isactive=true and id = " + item.id;
                    else if (item.billtype == Bill.BillTypes.ReturnBill && !unsettledcancel)
                        cancelquery = @"Update bill set billtype=9,iscancel=true,unsettledcancelled=false,modifiedby=@modifiedby,modifiedon=@modifiedon  where billtype != 9 and settled = false and isactive=true and id = " + item.id;

                    NpgsqlCommand command = GetCommand(cancelquery, conn, transaction);
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    if (command.ExecuteNonQuery() == 1)
                    {
                        if (!billcancelitem.disablestockreverse)
                        {
                            foreach (Billitems billitem in item.Billitems)
                            {
                                billitemdata.OffSetSaleQty(transaction, conn, billitem, item.billtype);
                            }
                        }
                        else
                        {
                            item.billattributes.removed = true;
                            UpdateAttributes(transaction, conn, item);
                            int version = 1;
                            var param = new SortedList<string, object>();
                            param.Add("billno", item.billno);
                            while (true)
                            {
                                var newbillno = item.billno + "_" + version.ToString("F0");
                                param["billno"] = newbillno;
                                using (var reader = ExecuteReader("Select id,isactive,billtype from bill where billno=@billno", param))
                                {
                                    if (!reader.Read())
                                    {
                                        break;
                                    }
                                }
                            }
                            command = GetCommand("Update bill set billno=@billno where id=" + item.id + " and isactive=true",
                                conn, transaction);
                            command.Parameters.AddWithValue("billno", param["billno"]);
                            command.ExecuteNonQuery();
                        }

                        if (item.customerid > 0 && item.billattributes != null && item.billattributes.spentreward > 0)
                        {
                            CustomerrewardsDataService customerrewardsDataService = new CustomerrewardsDataService() { Context = this.Context };
                            customerrewardsDataService.Context = this.Context;
                            customerrewardsDataService.ReverseReward(transaction, conn, item.customerid, item.billattributes.spentreward, item.locationid);
                        }
                        if (item.customerid > 0 && item.billattributes != null && !string.IsNullOrEmpty(item.billattributes.couponcode))
                        {
                            command = GetCommand("update vouchers set billid=0, billmasterid=0 where code=@vouchercode and unlimited=false;", conn, transaction);
                            command.Parameters.AddWithValue("vouchercode", item.billattributes.couponcode);
                            command.ExecuteNonQuery();
                        }
                        if (!string.IsNullOrEmpty(item.returnbillno))
                        {
                            var param = new SortedList<string, object>();
                            param.Add("billid", item.id);
                            string _ReturnBills = @"select distinct billid from billitems 
                                inner join bill on bill.id = billitems.billid
                                where bill.billtype=9 and bill.isreturn=true and billitems.isactive=true and billitems.id in (
                                select returnbillitemid from billitems where billid = @billid and isactive=true) ";

                            List<long> ids = new List<long>();
                            using (var reader = ExecuteReader(_ReturnBills, param))
                            {
                                while (reader.Read())
                                {
                                    ids.Add(reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]));
                                }
                            }
                            if (ids.Count > 0)
                            {
                                // date(billdate)=date(@modifiedon) avoids re-opening of other day return bill 
                                // source bill being created with return bill and again this bill used to again return
                                // then using this return bill to adjust with sales bills

                                command = GetCommand("update bill set billtype=8, modifiedby=@modifiedby,modifiedon=@modifiedon " +
                                    "where date(billdate)=date(@modifiedon) and billtype=9 and isreturn=true and settled=false and id in (" + string.Join<long>(",", ids) + ")  and isactive=true;", conn, transaction);
                                //command.Parameters.AddWithValue("retbillno", item.returnbillno);
                                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                                int successCount = command.ExecuteNonQuery();
                                //if (successCount == 0)
                                //    throw new Exception("Other day return bills are trying to enabled. " + string.Join<long>(",", ids));
                            }
                        }
                    }
                    else
                    {
                        throw new Exception("Some of the return bill trying to settled twice - " + item.billno);
                    }
                }
                if (item.isb2b)
                {
                    NpgsqlCommand command = GetCommand("update customerinvoice set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where billid=" + item.id, conn, transaction);
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.ExecuteNonQuery();

                    if (!string.IsNullOrEmpty(item.billattributes.packslipid))
                    {
                        command = GetCommand("update PACKINGSLIP set billid=0,billno='' where PACKINGNO=@PACKINGNO", conn, transaction);
                        command.Parameters.Add("PACKINGNO", NpgsqlTypes.NpgsqlDbType.Varchar).Value = item.billattributes.packslipid;
                        command.ExecuteNonQuery();
                    }
                }

                var stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
                stocktransactionDataService.DeleteBillItems(item, transaction, conn, true);

                if (unsettledcancel && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_notification_bill)
                {
                    Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, item, Processors.Schedule_Actions.Actions.Cancelled);
                }

            }
            return billlist;
        }

        internal List<Bill> Cancellation(ActionRequest<BillCancel> request,
            Npgsql.NpgsqlTransaction transaction, Npgsql.NpgsqlConnection connection,
            SequenceNo sequence)
        {
            var billlist = Search(new BillCreteria()
            {
                ids = request.Item.ids,
                isb2b = true
            });
            if (billlist == null || billlist.Count == 0) return billlist;

            if (billlist.Exists(e => e.isb2b == true))
            {
                throw new Exception("B2B Invoice can't be cancelled. Please use Invoice return.");
            }

            if (billlist.Exists(e => e.billtype != Bill.BillTypes.Cancel && e.settled == true))
            {
                throw new Exception("Please cancel the settlement before cancel bill.");
            }

            foreach (var item in billlist)
            {
                if (item.billtype != Bill.BillTypes.Cancel && item.settled == false)
                {
                    if (item.billtype != Bill.BillTypes.Cancel &&
                        !request.Item.disablestockreverse)
                    {
                        if (item.Billitems.Exists(e => e.attributes != null &&
                                e.attributes.so != null &&
                                e.attributes.so.Count > 0))
                        {
                            string updatesoquery = string.Format(@"with soorderitems
                        as
                        (
                        select cast(schemelist->>'id' as bigint) id,cast(schemelist->>'qty' as decimal) qty from billitems 
                        LEFT JOIN LATERAL
                        json_array_elements (cast(billitems.attributes->>'so' as json)) schemelist ON true
                        where billid = {0} and isactive=true
                        )

                        update salesorderitems set bqty=bqty-soorderitems.qty,pqty=pqty+soorderitems.qty from soorderitems
                        where soorderitems.id = salesorderitems.id and bqty>0", item.id);

                            ExecuteQuery(updatesoquery, null, connection, transaction);
                        }
                    }
                    NpgsqlCommand command = GetCommand("Update bill set billtype=9,iscancel=true,unsettledcancelled=true,modifiedby=@modifiedby,modifiedon=@modifiedon  where billtype != 9 and settled = false and id=" + item.id + "  and isactive=true", connection, transaction);
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    int successCount = command.ExecuteNonQuery();

                    if (item.customerid > 0 && item.billattributes != null && item.billattributes.spentreward > 0)
                    {
                        CustomerrewardsDataService customerrewardsDataService = new CustomerrewardsDataService() { Context = this.Context };
                        customerrewardsDataService.Context = this.Context;
                        customerrewardsDataService.ReverseReward(transaction, connection, item.customerid, item.billattributes.spentreward, item.locationid);
                    }


                }
            }

            if (sequence == null)
            {
                string billsequence = "bill.billno";
                bool locationwisebillno = true;

                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_srbillno)
                {
                    locationwisebillno = Services.ConfigManager.Instance(Context.OrganizationID).Config.system_srbillnobylocation;
                    billsequence = "billreturn.billno";
                }

                sequence = SequenceManager.Instance(Context.OrganizationID).Get(billsequence, locationwisebillno ? billlist.First().locationid : 0, billlist.First().companyid,
                    Services.ConfigManager.Instance(Context.OrganizationID).Config.system_salefloorwisebillno ? billlist.First().floorid : 0);

                if (billsequence == "billreturn.billno")
                {
                    sequence = SequenceManager.Instance(Context.OrganizationID).Get(billsequence, locationwisebillno ? billlist.First().locationid : 0, billlist.First().companyid,
                    Services.ConfigManager.Instance(Context.OrganizationID).Config.system_srfloorwisebillno ? billlist.First().floorid : 0);
                }
            }

            var newbilllist = Billmaster.CreateCancelledBills(billlist, DateTimeNow);
            foreach (var item in newbilllist)
            {
                item.DontChangeStock = request.Item.disablestockreverse;
                item.ChangeStatus(BaseEntity.DataStatus.Create);
                if (item.Billitems != null)
                    item.Billitems.ForEach(e => e.ChangeStatus(BaseEntity.DataStatus.Create));

                if (item.billattributes != null && item.billattributes.rewardamount != 0)
                {
                    item.billattributes.rewardamount = 0;
                    item.billattributes.spentreward = 0;
                }

                if (transaction == null)
                {
                    Insert(item, new Billmaster()
                    {
                        id = item.billmasterid,
                        total = item.total
                    });
                }
                else
                {
                    if (sequence != null)
                        item.billno = sequence.GetNextNumber(this.Context, DateTimeNow, connection, transaction);
                    Insert(transaction, connection, item, new Billmaster()
                    {
                        id = item.billmasterid,
                        total = item.total
                    });
                }

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_notification_bill)
                {
                    Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, item, Processors.Schedule_Actions.Actions.Cancelled);
                }
            }
            return billlist;
        }

        internal void UpdateCreditApproval(NpgsqlConnection conn, NpgsqlTransaction transaction, BillinboxStatus entity)
        {
            var command = GetCommand("update bill set iscreditapproved=true,billattributes=@billattributes where id=@id  and isactive=true", conn, transaction);
            var bill = Search(new BillCreteria() { id = entity.billid }, conn, transaction).First();
            if (bill.billattributes == null) bill.billattributes = new BillAttribute();
            bill.billattributes.printapprovedby = Context.UserID;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = bill.id;
            command.Parameters.Add("billattributes", NpgsqlTypes.NpgsqlDbType.Json).Value = bill.billattributes == null ? "null" : bill.billattributes_json;
            command.ExecuteNonQuery();
        }

        internal Bill FindOffers(Bill item)
        {
            try
            {
                return InfyPOS.Services.PromotionManager.Instance(Context.OrganizationID).Process(item);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace, Context.UserID, item.locationid);
                return item;
            }
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
                    qb.AddParameter("billitems.barcode", _Sales.barcode, "=");

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

        internal void UpdateAttributes(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill oBill)
        {
            string query = "update bill set billattributes = @billattributes where id =@id  and isactive=true";
            NpgsqlCommand command = GetCommand(query, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.id;
            command.Parameters.Add("billattributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oBill.billattributes == null ? "null" : oBill.billattributes_json;
            command.ExecuteNonQuery();
        }

        internal void UpdateEMI(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill oBill)
        {
            string query = "update bill set emidetail = @emidetail where id =@id and isactive=true";
            NpgsqlCommand command = GetCommand(query, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.id;
            command.Parameters.Add("emidetail", NpgsqlTypes.NpgsqlDbType.Json).Value = oBill.emidetail == null ? "null" : oBill.emidetail_json;
            command.ExecuteNonQuery();
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

        public System.Data.DataTable ExportBillDetail(long billid)
        {
            string query = @"Select 
billitems.id as ""Quanto Row ID"",
bill.billno as ""Bill No"",bill.billdate::date ""Bill Date"", 
location.name as Location,
company.name ""Supplier Company"",
company.tinanddate ""Supplier GST NO"",
supplier.name ""Source Supplier"",
suppliercompany.tinanddate ""Source Supplier GST NO"",
city.name ""Source Supplier PLACE"",
customer.name ""Customer Name"",
customer.gstno ""Customer GST NO"",
billitems.printingname ""Detail"",
billitems.hsncode ""HSN"",
billitems.barcode ""Barcode"",
stock.serialno ""Article ID"",
case when cast(billitems.attributes->>'obarcode' as character varying(100)) is null then billitems.barcode else cast(billitems.attributes->>'obarcode' as character varying(100)) end ""OtherNo"",
brand.name ""Brand"",
pattern.name  ""Pattern"",
style.name ""Style"",  
products.name ""Product"", 
size.name ""Size"",
material.name ""Material"",
colour.name ""Colour"",
type.name    ""Item Type"",
sleeve.name  ""Sleeve"",
fit.name  ""Fit"",
pricetag.name  ""PriceTag"",
billitems.price ""Price"",billitems.taxpercentage ""Tax Percentage"",
billitems.rate ""Rate"",
stock.displaymrp ""MRP Price"",
case when billitems.discount > 0 then billitems.discount::numeric / billitems.qty::numeric else 0::numeric end ""Discount Per Item"",
billitems.additionaldiscount as ""Additional discount per item"",billitems.itemtax ""Tax Per Item"",stock.designid ""DesignId"",
billitems.tax ""Tax"", billitems.discount  ""Discount"",billitems.additionaldiscount::numeric* billitems.qty::numeric ""Additional discount"", 
billitems.qty ""Qty"",(billitems.discount::numeric+(billitems.additionaldiscount::numeric* billitems.qty::numeric)) ""Total disount"",
billitems.receivable ""Item Value""
from bill
inner join billitems on billitems.billid = bill.id and billitems.isactive=true
inner join referencelist location on location.id = bill.locationid
inner join salablegoods on salablegoods.id = billitems.salablegoodsid
inner join stock on stock.id = salablegoods.stockid
left outer join company on company.id = bill.companyid 
left outer join customer on customer.id = bill.customerid
left outer join supplier on supplier.id = stock.supplierid
left outer join suppliercompany on suppliercompany.supplierid = stock.supplierid
left outer join referencelist city on city.id = suppliercompany.cityid
left outer join lrinvoiceitems on lrinvoiceitems.id=stock.invoiceitemid
left outer join items on items.id = stock.itemid
left outer join products on products.id = stock.productid
left outer join brand brand on brand.id = items.brandid
left outer join referencelist colour on colour.id = colourid
left outer join referencelist size on size.id = sizeid
left outer join referencelist style on style.id = styleid
left outer join referencelist pattern on pattern.id = patternid
left outer join referencelist material on material.id = materialid
left outer join referencelist type on type.id = typeid
left outer join referencelist sleeve on sleeve.id = sleeveid
left outer join referencelist fit on fit.id = fitid
left outer join referencelist pricetag on pricetag.id = stock.pricetagid
where bill.id=@id";
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
        public List<Bill> Search(BillCreteria _bill)
        {
            List<Bill> lst = new List<Bill>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                lst = Search(_bill, conn, null);
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

        public List<Bill> Search(BillCreteria _bill, Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction transaction)
        {
            List<Bill> lst = new List<Bill>();


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

                if (_bill.isMasterBill)
                {
                    qb.CurrentQuery = SQL_SearchByMasterCode;
                    qb.AddParameter("billmaster.code", _bill.billno, "=");
                    qb.AddParameter("bill.billtype", 9, "!=");
                    _bill.billno = "";
                }

                qb.AddParameter("bill.isactive", true, "=");

                if (!_bill.isb2b && !_bill.isimport)
                    qb.AddParameter("bill.isb2b", false, "=");

                if (!string.IsNullOrEmpty(_bill.billno))
                    qb.AddParameter("bill.billno", _bill.billno, "=");

                if (_bill.billmasterid != 0)
                    qb.AddParameter("bill.billmasterid", _bill.billmasterid, "=");

                if (_bill.locationid != 0)
                    qb.AddParameter("bill.locationid", _bill.locationid, "=");

                if (_bill.customerid != 0)
                    qb.AddParameter("bill.customerid", _bill.customerid, "=");

                if (_bill.forsettlement)
                {
                    qb.AddParameter("bill.settled", false, "=");
                    qb.AddParameter("bill.billtype", 9, "!=");
                    qb.AddParameter("bill.isb2b", false, "=");
                }

                if (_bill.forsalesreturn)
                {
                    qb.AddParameter("bill.settled", true, "=");
                    qb.AddParameter("bill.billtype", 9, "!=");
                }

                if (_bill.removecancelled && !_bill.forsettlement && !_bill.forsalesreturn)
                    qb.AddParameter("bill.billtype", 9, "!=");


                if (_bill.billnos != null && _bill.billnos.Count > 0)
                {
                    qb.AddInParamer("bill.billno", string.Join<string>(",", _bill.billnos.ConvertAll(e => "'" + e + "'")));
                }

                if (_bill.isimport)
                {
                    if (_bill.ids != null && _bill.ids.Count > 0)
                    {
                        qb.AddInParamer("bill.id", string.Join<long>(",", _bill.ids));
                    }
                    qb.SortField = "bill.id desc";
                    qb.Limit = _bill.ids == null || _bill.ids.Count == 1 ? 1 : 0;
                }
                else if (_bill.forsalesreturn || _bill.forsettlement || !string.IsNullOrEmpty(_bill.billno))
                {
                    qb.AddParameter("bill.locationid", _bill.locationid, "=");
                    qb.SortField = "bill.id desc";
                    if (!_bill.isMasterBill)
                        qb.Limit = 1;
                }
                else
                {
                    if (_bill.ids != null)
                    {
                        qb.AddInParamer("bill.id", string.Join<long>(",", _bill.ids));
                    }
                    else
                    {
                        qb.Limit = 100;
                    }
                }
                command = transaction != null ? qb.GetCommand(conn, transaction) : qb.GetCommand(conn);
            }
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _bill.id;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Bill obill = new Bill();
                    obill.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    obill.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                    obill.floorid = reader["floorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["floorid"]);
                    obill.customerstateid = reader["customerstateid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerstateid"]);
                    obill.additionalcharges = reader["additionalcharges"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionalcharges"]);
                    obill.isb2b = reader["isb2b"] == DBNull.Value ? false : Convert.ToBoolean(reader["isb2b"]);
                    obill.counterid = reader["counterid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["counterid"]);
                    obill.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                    obill.isinterstatetransfer = reader["isinterstatetransfer"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatetransfer"]);
                    obill.offline = reader["offline"] == DBNull.Value ? false : Convert.ToBoolean(reader["offline"]);
                    obill.HasPromotionApplied = reader["HasPromotionApplied"] == DBNull.Value ? false : Convert.ToBoolean(reader["HasPromotionApplied"]);
                    obill.billattributes_json = reader["billattributes"] == DBNull.Value ? "" : reader["billattributes"].ToString();
                    obill.emidetail_json = reader["emidetail"] == DBNull.Value ? "" : reader["emidetail"].ToString();
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
                    obill.schemediscount = reader["schemediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["schemediscount"]);
                    obill.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                    obill.discount -= obill.schemediscount;
                    obill.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                    obill.receivable = (reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"])) - obill.additionalcharges;
                    //obill.receivable = (reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]));
                    obill.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                    obill.hasemi = reader["hasemi"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasemi"]);
                    obill.isinterstatesale = reader["isinterstatesale"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstatesale"]);
                    obill.settled = reader["settled"] == DBNull.Value ? false : Convert.ToBoolean(reader["settled"]);
                    obill.isb2b = reader["isb2b"] == DBNull.Value ? false : Convert.ToBoolean(reader["isb2b"]);
                    obill.iscreditapproved = reader["iscreditapproved"] == DBNull.Value ? false : Convert.ToBoolean(reader["iscreditapproved"]);
                    obill.iscreditbill = reader["iscreditbill"] == DBNull.Value ? false : Convert.ToBoolean(reader["iscreditbill"]);
                    obill.isexport = reader["isexport"] == DBNull.Value ? false : Convert.ToBoolean(reader["isexport"]);
                    obill.istransfer = reader["istransfer"] == DBNull.Value ? false : Convert.ToBoolean(reader["istransfer"]);
                    obill.tzoffset = reader["tzoffset"] == DBNull.Value ? 0 : Convert.ToInt32(reader["tzoffset"]);
                    lst.Add(obill);
                }
            }
            if ((_bill.id > 0 || !string.IsNullOrEmpty(_bill.billno) || _bill.ids != null || _bill.isMasterBill)
                && (lst.Count == 1 || _bill.isMasterBill || _bill.ids != null))
            {

                foreach (var billitem in lst)
                {

                    BilladditionalchargesDataService billadditionalchargesDataService = new BilladditionalchargesDataService() { Context = this.Context };
                    billadditionalchargesDataService.Context = this.Context;
                    billitem.additionalchargeslist = billadditionalchargesDataService.SearchAll(billitem.id);

                    BillitemsDataService subBillitems = new BillitemsDataService() { Context = this.Context };
                    subBillitems.Context = this.Context;
                    billitem.AddItems(subBillitems.SearchAll(billitem.id,
                        _bill.enableprintdocument || _bill.forsalesreturn,
                        billitem.isinterstatesale, conn, transaction), false);

                    if (_bill.isMasterBill || !string.IsNullOrEmpty(_bill.billno))
                    {
                        if (billitem.discount > 0 && billitem.schemediscount == 0)
                        {
                            billitem.schemediscount = billitem.Billitems.Sum(e => e.schemediscount);
                            billitem.discount -= billitem.schemediscount;
                        }
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
                            billitem.customer = customer[0];
                            billitem.customername = customer[0].name;
                            billitem.phoneno = customer[0].mobileno;
                            billitem.customergst = customer[0].gstno;
                            billitem.rewardunit = customer[0].rewardunit;
                            billitem.customerdiscount = customer[0].discount;
                            billitem.customer = customer[0];
                        }
                    }
                    billitem.Billitems = billitem.Billitems.OrderBy(e => e.orderid).ToList();

                    billitem.totalqty = _bill.billtype == Bill.BillTypes.ReturnBill || billitem.Billitems.All(e => e.qty > 0) || billitem.Billitems.All(e => e.qty < 0)
                                            ? billitem.Billitems.Sum(e => e.qty) : billitem.Billitems.Sum(e => e.qty > 0 ? e.qty : 0);

                    billitem.totalpiece = _bill.billtype == Bill.BillTypes.ReturnBill || billitem.Billitems.All(e => e.piece > 0) || billitem.Billitems.All(e => e.piece < 0)
                                            ? billitem.Billitems.Sum(e => e.piece) : billitem.Billitems.Sum(e => e.piece > 0 ? e.piece : 0);

                    //billitem.total = billitem.receivable + billitem.rounding;
                    billitem.taxsplit = new TaxSplit();
                    billitem.taxsplit.totalamount = billitem.Billitems.Sum(e => e.rate * e.qty);
                    if (billitem.isinterstatesale)
                    {
                        billitem.taxsplit.cgsttaxamount = 0;
                        billitem.taxsplit.sgsttaxamount = billitem.tax;
                        billitem.taxsplit.secondtax = "IGST";
                    }
                    else
                    {
                        billitem.taxsplit.cgsttaxamount = Math.Round(billitem.tax / 2, 5);
                        billitem.taxsplit.sgsttaxamount = Math.Round(billitem.tax / 2, 5);
                        billitem.taxsplit.secondtax = "SGST";
                    }
                    billitem.BillTax.ForEach(e =>
                    {
                        e.secondtax = billitem.taxsplit.secondtax;
                    });
                }

                if (_bill.frompos && lst.Count == 1 &&
                    Context.HasPermission(PermissionSet.salebarcodeprice, InfyPOS.Common.Action.View))
                {
                    RemoveSimulation(lst[0]);
                }
            }

            return lst;
        }

        public void RemoveSimulation(Bill sourcebill)
        {
            if (PromotionManager.Instance(Context.OrganizationID).Schems == null)
            {
                PromotionManager.Instance(Context.OrganizationID).LoadSchemes();
            }
            var discountlist = PromotionManager.Instance(Context.OrganizationID).SimulationPromotions;
            if (discountlist.Count > 0)
            {
                var affectedbillitems = sourcebill.Billitems.FindAll(x => discountlist.Exists(y => y.id == x.schemeid));
                if (affectedbillitems.Count > 0)
                {
                    string query = @"select billitems.id,salablegoods.price,salablegoods.discount,
                                    tax.type,taxid
                                    from billitems 
                                    inner join salablegoods  on salablegoods.id = salablegoodsid
                                    inner join tax on tax.id = billitems.taxid
                                    where  billid =" + sourcebill.id+" and billitems.isactive=true";
                    List<Billitemsimulation> simprice = new List<Billitemsimulation>();
                    using (var reader = ExecuteReader(query, null))
                    {
                        while (reader.Read())
                        {
                            Billitemsimulation billitemsimulation = new Billitemsimulation();
                            billitemsimulation.id = Convert.ToInt64(reader["id"]);
                            billitemsimulation.taxid = Convert.ToInt64(reader["taxid"]);
                            billitemsimulation.price = Convert.ToInt64(reader["price"]);
                            billitemsimulation.discount = Convert.ToInt64(reader["discount"]);
                            billitemsimulation.taxtype = Convert.ToInt32(reader["type"]);
                            simprice.Add(billitemsimulation);
                        }
                    }


                    TaxDataService taxDataService = new TaxDataService() { Context = this.Context };
                    var taxsplits = taxDataService.RangedTaxProducts(sourcebill.id.ToString());
                    foreach (var item in affectedbillitems)
                    {
                        var itemprice = simprice.Find(x => x.id == item.id);

                        if (taxsplits.ContainsKey(item.barcode))
                        {
                            var newtax = taxsplits[item.barcode].Find(itemprice.price - itemprice.discount);
                            if (newtax.taxpercent != item.taxpercentage)
                            {
                                item.taxpercentage = newtax.taxpercent;
                            }
                        }

                        if (item.schemediscount > 0)
                        {
                            item.discount = itemprice.discount * item.qty;
                            item.price = itemprice.price;
                            item.receivable = (item.price - item.discount) * (item.qty - item.journalqty);
                            var qty = (item.qty - item.journalqty);
                            if (qty > 0 && item.additionaldiscount > 0)
                            {
                                item.receivable -= item.additionaldiscount * qty;
                            }
                            item.tax = Math.Round((item.receivable / (100 + item.taxpercentage) * item.taxpercentage), 4);
                            item.itemtax = Math.Round(item.tax > 0 ? (item.tax / item.qty) : 0, 4);
                            item.rate = Math.Round((item.receivable / (100 + item.taxpercentage) * 100), 4);
                            item.saleamount = Math.Round((item.rate) * item.qty, 4);
                            item.schemediscount = 0;
                            item.schemeid = 0;
                        }
                    }
                    sourcebill.schemediscount = 0;
                    sourcebill.additionaldiscount = sourcebill.Billitems.Sum(e => e.additionaldiscount * (e.qty - e.journalqty));
                    sourcebill.discount = sourcebill.Billitems.Sum(e => e.discount);
                    sourcebill.receivable = sourcebill.Billitems.Sum(e => e.receivable);
                    sourcebill.tax = sourcebill.Billitems.Sum(e => e.tax);
                    sourcebill.rounding = Math.Round(sourcebill.receivable, MidpointRounding.AwayFromZero) - sourcebill.receivable;
                    sourcebill.RecalculateTax();
                }
            }
        }

        public void UpdateBillNo(Bill oBill, SequenceNo sequence, Npgsql.NpgsqlConnection conn, Npgsql.NpgsqlTransaction transaction)
        {
            SortedList<string, object> parameters = new SortedList<string, object>();
            parameters.Add("billid", oBill.id);
            if (oBill.billattributes != null)
            {
                parameters.Add("billinboxid", oBill.billattributes.billinboxid);
                parameters.Add("customerinvoiceid", oBill.billattributes.customerinvoiceid);
            }
            parameters.Add("billnoguid", oBill.billno);

            oBill.billno = sequence.GetNextNumber(Context, conn, transaction);
            if (Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBill.locationid) &&
                Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].system_salebillnodailyreset
                && oBill.billdate.Date != sequence.modifiedon.Date)
            {
                if (oBill.backwardserialno)
                {
                    var dateserialno = sequence.GetFormatedValue("~", oBill.billdate).Split('~')[0];
                    var param = new SortedList<string, object>();
                    param.Add("billdate", oBill.billdate.Date);
                    using (var reader = ExecuteReader("select billno from bill where billdate::date=@billdate order by id desc limit 1", param))
                    {
                        if (reader.Read())
                        {
                            var lastbillno = reader.GetString(0).Substring(dateserialno.Length);
                            int billidentity = 0;
                            if (int.TryParse(lastbillno, out billidentity))
                            {
                                billidentity++;
                                oBill.billno = dateserialno + billidentity.ToString("F0");
                            }
                        }
                    }
                }
                else
                {
                    sequence.ResetNumber(conn, transaction, oBill.billdate.Date);
                    oBill.billno = sequence.GetNextNumber(Context, Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].system_salebillnodailyreset, oBill.billdate.Date, conn, transaction);
                }
            }
            while (true)
            {
                var billno = oBill.billno;
                if (LicenseManager.Instance(Context.OrganizationID).GetLicense().isoffline &&
                    !string.IsNullOrEmpty(Services.ConfigManager.Instance(Context.OrganizationID).Config.system_offlineprefix))
                    billno = Services.ConfigManager.Instance(Context.OrganizationID).Config.system_offlineprefix + oBill.billno;
                else if (!LicenseManager.Instance(Context.OrganizationID).GetLicense().isoffline && !string.IsNullOrEmpty(Services.ConfigManager.Instance(Context.OrganizationID).Config.system_onlineprefix))
                    billno = Services.ConfigManager.Instance(Context.OrganizationID).Config.system_onlineprefix + oBill.billno;

                var param = new SortedList<string, object>();
                param.Add("billno", billno);
                using (var reader = ExecuteReader("Select id from bill where billno=@billno", param))
                {
                    if (!reader.Read())
                    {
                        break;
                    }
                    if (oBill.backwardserialno)
                    {
                        var dateserialno = sequence.GetFormatedValue("~", oBill.billdate).Split('~')[0];

                        param.Add("billdate", oBill.billdate.Date);
                        using (var readers = ExecuteReader("select billno from bill where billdate::date=@billdate order by id desc limit 1", param))
                        {
                            if (readers.Read())
                            {
                                var lastbillno = readers.GetString(0).Substring(dateserialno.Length);
                                int billidentity = 0;
                                if (int.TryParse(lastbillno, out billidentity))
                                {
                                    billidentity++;
                                    oBill.billno = dateserialno + billidentity.ToString("F0");
                                }
                            }
                        }
                    }
                    else
                    {
                        oBill.billno = sequence.GetNextNumber(Context, Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].system_salebillnodailyreset, oBill.billdate.Date, conn, transaction);
                    }
                }
            }

            if (oBill.id > 0)
            {
                parameters.Add("billno", oBill.billno);
                parameters.Add("billsettlementid", long.Parse("0"));
                ExecuteQuery(@"update bill set billno=@billno,billnoguid=@billnoguid where id=@billid  and isactive=true", parameters, conn, transaction);
                ExecuteQuery(@"update billinbox set billno=@billno where id=@billinboxid", parameters, conn, transaction);
                ExecuteQuery(@"update Customerinvoice set billno=@billno where id=@customerinvoiceid", parameters, conn, transaction);
                if (!string.IsNullOrEmpty(oBill.billattributes.packslipid))
                {
                    PackingslipDataService packingslipDataService = new PackingslipDataService();
                    packingslipDataService.Context = this.Context;
                    packingslipDataService.UpdateBill(oBill, conn, transaction);
                }

                if (oBill.billattributes != null &&
                    oBill.billattributes.settlement != null &&
                    oBill.billattributes.settlement.Billsettlement != null &&
                    oBill.billattributes.settlement.Billsettlement.Count > 0)
                {
                    foreach (var billsettlement in oBill.billattributes.settlement.Billsettlement)
                    {
                        parameters["billsettlementid"] = billsettlement.id;
                        ExecuteQuery(@"update billsettlement set billno=@billno where id=@billsettlementid", parameters, conn, transaction);
                    }
                }
            }
        }
        public Bill Insert(Bill oBill, Billmaster billmaster)
        {
            NpgsqlTransaction transaction = null;
            try
            {

                //oBill = InfyPOS.Services.IncentiveManager.Instance.Process(oBill);
                string billsequence = "bill.billno";
                bool locationwisebillno = true;

                if (oBill.billtype == Bill.BillTypes.ReturnBill && Services.ConfigManager.Instance(Context.OrganizationID).Config.system_srbillno)
                {
                    locationwisebillno = Services.ConfigManager.Instance(Context.OrganizationID).Config.system_srbillnobylocation;
                    billsequence = "billreturn.billno";
                }
                /*else if(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_gstbillsequence)
                {
                    billsequence = "gstbill.billno";
                }*/

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBill.locationid) &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].system_saledatefreeze &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].system_saledatefreezeon > DateTime.MinValue)
                {
                    DateTime newbilldate = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].system_saledatefreezeon;
                    oBill.billdate = newbilldate.Date;
                }
                else if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_datechange &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBill.locationid))
                {
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].currentday == DateTimeNow.Date &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].nextdaycount > 0)
                    {
                        DateTime currentsettlementdate = DateTimeNow.Date.AddDays(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].nextdaycount);
                        oBill.billdate = currentsettlementdate;
                    }
                }

                //if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_validatesettlement &&
                //    oBill.billdate.Date != DateTimeNow.Date &&
                //    Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].system_salebillnodailyreset)
                //{
                //    oBill.backwardserialno = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_validatesettlement;
                //    oBill.billdate = DateTimeNow.Date;
                //}

                var sequence = SequenceManager.Instance(Context.OrganizationID).Get(billsequence, locationwisebillno ? oBill.locationid : 0, oBill.companyid,
                    Services.ConfigManager.Instance(Context.OrganizationID).Config.system_salefloorwisebillno ? oBill.floorid : 0);

                if (billsequence == "billreturn.billno")
                {
                    sequence = SequenceManager.Instance(Context.OrganizationID).Get(billsequence, locationwisebillno ? oBill.locationid : 0, oBill.companyid,
                    Services.ConfigManager.Instance(Context.OrganizationID).Config.system_srfloorwisebillno ? oBill.floorid : 0);
                }

                if (!sequence.Validate(Context, billsequence == "billreturn.billno" ? "Return Bill No" : "Bill No", false))
                {
                    if (locationwisebillno)
                    {
                        InfyPOS.Data.ReferencelistDataService reflist = new ReferencelistDataService() { Context = this.Context };
                        var location = reflist.Search(new ReferencelistCreteria()
                        {
                            id = oBill.locationid
                        });

                        if (location.Count == 1)
                            sequence.Validate(Context, billsequence == "billreturn.billno" ? "Return Bill No for " : "Bill No for " + location[0].name, true);
                    }
                    else
                    {
                        CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
                        var companies = companyDataService.Search(new CompanyCreteria()
                        {
                            id = oBill.companyid
                        });
                        if (companies.Count == 1)
                            sequence.Validate(Context, billsequence == "billreturn.billno" ? "Return Bill No for " : "Bill No for " + companies[0].name, true);
                    }
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    if (oBill.billtype == Bill.BillTypes.Cancel && !string.IsNullOrEmpty(oBill.returnbillno))
                    {
                        oBill.unsettledcancelled = true;
                        NpgsqlCommand command = GetCommand("Update bill set billtype=9,unsettledcancelled=true where billtype != 9 and settled = false and billno='" + WebAPI.Data.Utility.SafeSQL(oBill.returnbillno) + "'", conn, transaction);
                        if (command.ExecuteNonQuery() == 1)
                        {
                            oBill.billno = sequence.GetNextNumber(Context, conn, transaction);
                            if (Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBill.locationid) &&
                            Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].system_salebillnodailyreset
                            && oBill.billdate.Date != sequence.modifiedon.Date)
                            {
                                if (oBill.backwardserialno)
                                {
                                    var dateserialno = sequence.GetFormatedValue("~", oBill.billdate).Split('~')[0];
                                    var param = new SortedList<string, object>();
                                    param.Add("billdate", oBill.billdate.Date);
                                    using (var reader = ExecuteReader("select billno from bill where billdate::date=@billdate order by id desc limit 1", param))
                                    {
                                        if (reader.Read())
                                        {
                                            var lastbillno = reader.GetString(0).Substring(dateserialno.Length);
                                            int billidentity = 0;
                                            if (int.TryParse(lastbillno, out billidentity))
                                            {
                                                billidentity++;
                                                oBill.billno = dateserialno + billidentity.ToString("F0");
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    sequence.ResetNumber(conn, transaction, oBill.billdate.Date);
                                    oBill.billno = sequence.GetNextNumber(Context, Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].system_salebillnodailyreset,
                                        oBill.billdate.Date, conn, transaction);
                                }
                            }
                            while (true)
                            {
                                var billno = oBill.billno;
                                if (LicenseManager.Instance(Context.OrganizationID).GetLicense().isoffline &&
                                    !string.IsNullOrEmpty(Services.ConfigManager.Instance(Context.OrganizationID).Config.system_offlineprefix))
                                    billno = Services.ConfigManager.Instance(Context.OrganizationID).Config.system_offlineprefix + oBill.billno;
                                else if (!LicenseManager.Instance(Context.OrganizationID).GetLicense().isoffline && !string.IsNullOrEmpty(Services.ConfigManager.Instance(Context.OrganizationID).Config.system_onlineprefix))
                                    billno = Services.ConfigManager.Instance(Context.OrganizationID).Config.system_onlineprefix + oBill.billno;

                                var param = new SortedList<string, object>();
                                param.Add("billno", billno);
                                using (var reader = ExecuteReader("Select id from bill where billno=@billno", param))
                                {
                                    if (!reader.Read())
                                    {
                                        break;
                                    }
                                    if (oBill.backwardserialno)
                                    {
                                        var dateserialno = sequence.GetFormatedValue("~", oBill.billdate).Split('~')[0];
                                        var parameter = new SortedList<string, object>();
                                        param.Add("billdate", oBill.billdate.Date);
                                        using (var readers = ExecuteReader("select billno from bill where billdate::date=@billdate order by id desc limit 1", parameter))
                                        {
                                            if (readers.Read())
                                            {
                                                var lastbillno = readers.GetString(0).Substring(dateserialno.Length);
                                                int billidentity = 0;
                                                if (int.TryParse(lastbillno, out billidentity))
                                                {
                                                    billidentity++;
                                                    oBill.billno = dateserialno + billidentity.ToString("F0");
                                                }
                                            }
                                        }
                                    }
                                    else
                                    {
                                        oBill.billno = sequence.GetNextNumber(Context, Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].system_salebillnodailyreset,
                                            oBill.billdate.Date, conn, transaction);
                                    }
                                }
                            }
                            Insert(transaction, conn, oBill, billmaster);
                        }
                    }
                    else
                    {
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_billnoqueue)
                            oBill.billno = GenerateUnique25CharId();
                        else
                            UpdateBillNo(oBill, sequence, conn, transaction);

                        Insert(transaction, conn, oBill, billmaster);

                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_billnoqueue)
                            UpdateBillNo(oBill, sequence, conn, transaction);
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace, Context.UserID, oBill.locationid);
                oBill.billno = "";
                oBill.id = 0;
                throw exp;
            }

            return oBill;
        }


        public bool ValidateMinimumMargin(Bill bill)
        {

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_billmarginlock &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_minimumbillmargin > 0)
            {
                if (bill.Billitems.Any(e => e.additionaldiscount > 0))
                {
                    List<string> barcodes = new List<string>();
                    StockDataService stockdataservice = new StockDataService() { Context = this.Context };
                    stockdataservice.Context = this.Context;
                    var priceList = stockdataservice.SearchForPrice(bill.Billitems.ConvertAll(e => e.stockid));
                    decimal minimumbillvalue = 0;
                    decimal billvalue = 0;
                    foreach (var billitem in bill.Billitems)
                    {
                        if (billitem.qty < 0) continue;
                        if (priceList.Exists(e => e.id == billitem.stockid))
                        {
                            var buyingprice = priceList.Find(e => e.id == billitem.stockid).buyingprice;
                            var margin = Math.Round(buyingprice * InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_minimumbillmargin / 100, 0);
                            if (buyingprice + margin > billitem.rate)
                            {
                                barcodes.Add(billitem.barcode);
                            }
                            minimumbillvalue += buyingprice + margin;
                            billvalue += billitem.rate;
                        }
                    }
                    if (minimumbillvalue > billvalue)
                    {
                        throw new Exception("Minimum margin failed on - " + string.Join(",", barcodes));
                    }
                }
            }
            return true;
        }

        public void UpdateUANBillItems(Bill oBill, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            if (oBill.billtype == Bill.BillTypes.ReturnBill) return;

            List<BillItemUAN> billitemuan = new List<BillItemUAN>();

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5)
            {
                var grouped = oBill.Billitems.FindAll(e => e.attributes != null && e.attributes.serialno != null &&
                e.attributes.serialno.Length > 0).GroupBy(e => new { e.attributes.serialno, e.stockid }).ToList();
                if (grouped.Exists(x => x.Count() > 1))
                {
                    List<Billitems> newgrpbillitems = new List<Billitems>();
                    foreach (var grp in grouped)
                    {
                        var itm = JsonConvert.DeserializeObject<Billitems>(JsonConvert.SerializeObject(grp.First()));
                        itm.qty = grp.Sum(x => x.qty);
                        itm.piece = grp.Sum(x => x.piece);
                        itm.receivable = grp.Sum(x => x.receivable);
                        itm.tax = grp.Sum(x => x.tax);
                        List<BillItemAttributes.itemqty> allso = new List<BillItemAttributes.itemqty>();
                        foreach (var itmgrp in grp.ToList().FindAll(x => x.attributes != null & x.attributes.so != null).ConvertAll(x => x.attributes.so))
                        {
                            allso.AddRange(itmgrp);
                        }
                        itm.attributes.so = new List<BillItemAttributes.itemqty>();
                        foreach (var itmgrp in allso.GroupBy(x => x.id))
                        {
                            itm.attributes.so.Add(new BillItemAttributes.itemqty()
                            {
                                id = itmgrp.Key,
                                qty = itmgrp.Sum(x => x.qty)
                            });
                        }
                        newgrpbillitems.Add(itm);
                    }
                    oBill.Billitems = newgrpbillitems;
                }
            }

            foreach (var item in oBill.Billitems.FindAll(e => e.attributes != null && e.attributes.serialno != null &&
            e.attributes.serialno.Length > 0).GroupBy(e => new { e.attributes.serialno, e.stockid }))
            {
                billitemuan.Add(new BillItemUAN()
                {
                    uan = item.Key.serialno,
                    stockid = item.Key.stockid,
                    childs = item.ToList(),
                    qty = item.Sum(e => e.qty),
                    ids = item.ToList().ConvertAll(e => e.salablegoodsid),
                    stockmissmatch = false
                });
            }

            //if (billitemuan.All(e => e.qty == 1)) return;

            string query = string.Format(@"select salablegoods.id,salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) stockqty from salablegoods
            where salablegoods.locationid = {0} 
            and salablegoods.id in ({1})
            and salablegoods.isactive = true and salablegoods.hasstock = true", oBill.locationid, string.Join(",", oBill.Billitems.ConvertAll(e => e.salablegoodsid)));

            if (oBill.billtype == Bill.BillTypes.ReturnBill)
                query = string.Format(@"select salablegoods.id,salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) stockqty from salablegoods
            where salablegoods.locationid = {0} 
            and salablegoods.isactive = true and salablegoods.id in ({1})", oBill.locationid, string.Join(",", oBill.Billitems.ConvertAll(e => e.salablegoodsid)));

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.analyser_stock_removeaqty)
                query = query.Replace("salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)", "salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty+salablegoods.aqty)");

            SortedDictionary<long, decimal> stockbyids = new SortedDictionary<long, decimal>();
            using (var reader = ExecuteReader(query, new SortedList<string, object>(), conn, transaction))
            {
                while (reader.Read())
                {
                    stockbyids.Add(reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]), reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]));
                }
            }

            bool hasnegativestock = false;
            foreach (var item in oBill.Billitems.GroupBy(e => new { e.salablegoodsid }))
            {
                if (oBill.billtype == Bill.BillTypes.ReturnBill)
                {
                    if (!stockbyids.ContainsKey(item.Key.salablegoodsid) ||
                    stockbyids[item.Key.salablegoodsid] < Math.Abs(item.Sum(e => e.qty)))
                    {
                        hasnegativestock = true;
                        if (billitemuan.Exists(e => e.ids.Contains(item.Key.salablegoodsid)))
                        {
                            billitemuan.Find(e => e.ids.Contains(item.Key.salablegoodsid)).stockmissmatch = true;
                        }
                    }
                }
                else
                {
                    if (!stockbyids.ContainsKey(item.Key.salablegoodsid) ||
                    stockbyids[item.Key.salablegoodsid] < item.Sum(e => e.qty))
                    {
                        hasnegativestock = true;
                        if (billitemuan.Exists(e => e.ids.Contains(item.Key.salablegoodsid)))
                        {
                            billitemuan.Find(e => e.ids.Contains(item.Key.salablegoodsid)).stockmissmatch = true;
                        }
                    }
                }
            }
            if (!hasnegativestock || !billitemuan.Exists(e => e.stockmissmatch)) return;

            List<BillItemUANStock> billItemUANStocks = new List<BillItemUANStock>();
            query = string.Format(@"select stock.barcode, stock.serialno,stock.id stockid,salablegoods.id as salablegoodsid,
            salablegoods.iqty-(salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) stockqty from salablegoods
            inner join stock on stock.id = stockid
            where salablegoods.locationid = {0} 
            and stock.serialno in ({1})
            and salablegoods.isactive = true and salablegoods.hasstock = true", oBill.locationid, string.Join(",", billitemuan.FindAll(e => e.stockmissmatch).ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e.uan) + "'")));

            if (oBill.billtype == Bill.BillTypes.ReturnBill)
            {
                query = string.Format(@"select stock.barcode, stock.serialno,stock.id stockid,salablegoods.id as salablegoodsid,
                oqty+jqty+dqty stockqty from salablegoods
                inner join stock on stock.id = stockid
                where salablegoods.locationid = {0} 
                and salablegoods.isactive = true and stock.serialno in ({1})", oBill.locationid, string.Join(",", billitemuan.FindAll(e => e.stockmissmatch).ConvertAll(e => "'" + WebAPI.Data.Utility.SafeSQL(e.uan) + "'")));
            }

            using (var reader = ExecuteReader(query, new SortedList<string, object>(), conn, transaction))
            {
                while (reader.Read())
                {
                    billItemUANStocks.Add(new BillItemUANStock()
                    {
                        uan = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString(),
                        barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString(),
                        stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]),
                        stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]),
                        salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"])
                    });
                }
            }

            if (oBill.id > 0)
            {
                query = string.Format(@"select billitems.qty stockqty,stock.barcode, stock.serialno,stock.id stockid,salablegoods.id as salablegoodsid
                from billitems 
                inner join salablegoods on salablegoods.id = salablegoodsid
                inner join stock on stock.id = salablegoods.stockid
                where billid={0} and billitems.isactive=true and salablegoods.isactive = true", oBill.id);
                using (var reader = ExecuteReader(query, new SortedList<string, object>(), conn, transaction))
                {
                    while (reader.Read())
                    {
                        var newuan = new BillItemUANStock()
                        {
                            uan = reader["serialno"] == DBNull.Value ? "" : reader["serialno"].ToString(),
                            barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString(),
                            stockqty = reader["stockqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["stockqty"]),
                            stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]),
                            salablegoodsid = reader["salablegoodsid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salablegoodsid"])
                        };
                        if (billItemUANStocks.Exists(x => x.salablegoodsid == newuan.salablegoodsid))
                        {
                            billItemUANStocks.Find(x => x.salablegoodsid == newuan.salablegoodsid).stockqty += newuan.stockqty;
                        }
                        else
                        {
                            billItemUANStocks.Add(newuan);
                        }
                    }
                }
            }

            List<Billitems> newbillitems = new List<Billitems>();
            foreach (var item in billitemuan.FindAll(e => e.stockmissmatch))
            {
                if (billItemUANStocks.Exists(e => e.uan == item.uan))
                {
                    foreach (var citem in item.childs)
                    {
                        var requiredstock = citem.qty;
                        var availlist = billItemUANStocks.FindAll(e => e.uan == item.uan && e.availableqty > 0).OrderByDescending(e => e.stockid).ToList();
                        var availableqty = availlist.Sum(x => x.availableqty);
                        if (availlist.Count > 0)
                        {
                            if (availlist[0].availableqty >= Math.Abs(citem.qty))
                            {
                                citem.stockid = availlist[0].stockid;
                                citem.salablegoodsid = availlist[0].salablegoodsid;
                                citem.barcode = availlist[0].barcode;
                                availlist[0].consumed += Math.Abs(citem.qty);
                            }
                            else
                            {
                                var qty = Math.Abs(citem.qty);
                                citem.stockid = availlist[0].stockid;
                                citem.salablegoodsid = availlist[0].salablegoodsid;
                                citem.barcode = availlist[0].barcode;
                                citem.discount = citem.discount / citem.qty * availlist[0].availableqty;
                                citem.schemediscount = citem.schemediscount / citem.qty * availlist[0].availableqty;
                                citem.tax = citem.tax / citem.qty * availlist[0].availableqty;
                                citem.receivable = citem.receivable / citem.qty * availlist[0].availableqty;
                                citem.qty = oBill.billtype == Bill.BillTypes.ReturnBill ?
                                    (availlist[0].availableqty * -1) : availlist[0].availableqty;
                                citem.piece = citem.qty;
                                qty -= Math.Abs(citem.qty);

                                availlist[0].consumed += Math.Abs(citem.qty);
                                for (int i = 1; i < availlist.Count && qty > 0; i++)
                                {
                                    var nitem = Newtonsoft.Json.JsonConvert.DeserializeObject<Billitems>(Newtonsoft.Json.JsonConvert.SerializeObject(citem));
                                    var consumeqty = availlist[i].availableqty >= qty ? qty : availlist[i].availableqty;
                                    nitem.id = 0;
                                    nitem.discount = nitem.discount / nitem.qty * consumeqty;
                                    nitem.schemediscount = nitem.schemediscount / nitem.qty * consumeqty;
                                    nitem.tax = nitem.tax / nitem.qty * consumeqty;
                                    nitem.receivable = nitem.receivable / nitem.qty * consumeqty;
                                    nitem.qty = oBill.billtype == Bill.BillTypes.ReturnBill ? (consumeqty * -1) : consumeqty;
                                    nitem.piece = nitem.qty;
                                    nitem.stockid = availlist[i].stockid;
                                    nitem.salablegoodsid = availlist[i].salablegoodsid;
                                    nitem.barcode = availlist[i].barcode;
                                    newbillitems.Add(nitem);
                                    qty -= Math.Abs(consumeqty);
                                    availlist[i].consumed += Math.Abs(consumeqty);
                                }
                                if (qty > 0)
                                {
                                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportuannegative)
                                    {
                                        var nitem = Newtonsoft.Json.JsonConvert.DeserializeObject<Billitems>(Newtonsoft.Json.JsonConvert.SerializeObject(citem));
                                        nitem.id = 0;
                                        nitem.discount = nitem.discount / nitem.qty * qty;
                                        nitem.schemediscount = nitem.schemediscount / nitem.qty * qty;
                                        nitem.tax = nitem.tax / nitem.qty * qty;
                                        nitem.receivable = nitem.receivable / nitem.qty * qty;
                                        nitem.qty = oBill.billtype == Bill.BillTypes.ReturnBill ? (qty * -1) : qty;
                                        nitem.piece = nitem.qty;
                                        nitem.stockid = availlist[0].stockid;
                                        nitem.salablegoodsid = availlist[0].salablegoodsid;
                                        nitem.barcode = availlist[0].barcode;
                                        newbillitems.Add(nitem);

                                        WebAPI.Common.Logger.Instance(Context.OrganizationID).PerformanceLog(
                                            "Information : Don't have enough stock for " + item.uan + " required : " + requiredstock + " available : "
                                            + availableqty, WebAPI.Common.Logger.LogType.Info, "UpdateUANBillItems", oBill, Context.UserID, oBill.locationid);
                                    }
                                    else
                                    {
                                        throw new Exception("Don't have enough stock for " + item.uan + " required : " + requiredstock + " available : " + availableqty);
                                    }
                                    //citem.discount = citem.discount / citem.qty * (Math.Abs(citem.qty) + qty);
                                    //citem.schemediscount = citem.schemediscount / citem.qty * (Math.Abs(citem.qty) + qty);
                                    //citem.tax = citem.tax / citem.qty * (Math.Abs(citem.qty) + qty);
                                    //citem.receivable = citem.receivable / citem.qty * (Math.Abs(citem.qty) + qty);
                                    //citem.qty = oBill.billtype == Bill.BillTypes.ReturnBill ?
                                    //    (citem.qty + (-1 * qty)) : (citem.qty + qty);
                                    //citem.piece = citem.qty;
                                }
                            }
                        }
                        else if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportuannegative)
                        {
                            throw new Exception("Don't have enough stock for " + item.uan + " required : " + requiredstock + " available : " + availableqty);
                        }
                    }

                }
            }


            if (newbillitems.Count > 0)
            {
                var alluan = newbillitems.ConvertAll(x => x.attributes.serialno).Distinct();
                newbillitems.ForEach(x => x.attributes.so = new List<BillItemAttributes.itemqty>());

                var conflictuans = oBill.Billitems.FindAll(x => alluan.Contains(x.attributes.serialno));
                SortedDictionary<string, List<BillItemAttributes.itemqty>> uanso = new SortedDictionary<string, List<BillItemAttributes.itemqty>>();
                conflictuans.ForEach(x =>
                {
                    if (x.attributes.so != null)
                    {
                        if (!uanso.ContainsKey(x.attributes.serialno))
                            uanso.Add(x.attributes.serialno, new List<BillItemAttributes.itemqty>());
                        uanso[x.attributes.serialno].AddRange(x.attributes.so);
                    }
                });
                oBill.Billitems.AddRange(newbillitems);

                foreach (var guan in alluan)
                {
                    var uanbillitems = oBill.Billitems.FindAll(x => x.attributes.serialno == guan);
                    uanbillitems.ForEach(x => x.attributes.so = new List<BillItemAttributes.itemqty>());
                    if (uanso.ContainsKey(guan))
                    {
                        var availso = uanso[guan];
                        foreach (var bitem in uanbillitems)
                        {
                            var avilqty = bitem.qty;
                            availso.RemoveAll(x => x.qty == 0);
                            foreach (var soitem in availso)
                            {
                                if (soitem.qty > avilqty)
                                {
                                    bitem.attributes.so.Add(new BillItemAttributes.itemqty()
                                    {
                                        id = soitem.id,
                                        qty = avilqty
                                    });
                                    soitem.qty -= avilqty;
                                    avilqty = 0;
                                }
                                else
                                {
                                    bitem.attributes.so.Add(new BillItemAttributes.itemqty()
                                    {
                                        id = soitem.id,
                                        qty = soitem.qty
                                    });
                                    soitem.qty = 0;
                                    avilqty -= soitem.qty;
                                }
                                if (avilqty == 0) break;
                            }
                        }
                    }
                }
            }
        }

        public void Insert(Processors.OfflineClient.WindowsOfflineResponse response, List<Bill> bills)
        {
            NpgsqlTransaction transaction = null;
            Bill currentbill = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var bill in bills)
                    {
                        currentbill = bill;
                        bill.issynchronize = true;
                        Insert(transaction, conn, bill, new Billmaster()
                        {
                            id = bill.billmasterid,
                            total = bill.total
                        });
                    }
                    transaction.Commit();
                    conn.Close();
                }
                lock (response.lockobject)
                {
                    response.completed = true;
                    response.completedon = DateTimeNow;
                }
            }
            catch (Exception exp)
            {
                lock (response.lockobject)
                {
                    response.errormessage = currentbill == null ? exp.Message : (currentbill.billno + " - " + exp.Message);
                    response.error = true;
                    response.completed = true;
                    response.completedon = DateTimeNow;
                }
            }
        }
        public Bill Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill oBill, Billmaster billmaster)
        {
            if (oBill.companyid <= 0)
                throw new Exception("Please select the company");

            if (oBill.iscreditbill && oBill.customerid == 0)
            {
                throw new Exception("Credit bill - Customer mandatory, please select the customer");
            }

            if (oBill.billtype == Bill.BillTypes.Credit && oBill.customerid == 0)
            {
                throw new Exception("B2B bill - Customer mandatory, please select the customer");
            }

            if (oBill.Billitems.Exists(e => !e.isactive))
            {
                throw new Exception("Few barcode deleted, please try again or contact software vendor.");
            }

            var receivablediff = Math.Abs(oBill.receivable) - Math.Abs(oBill.Billitems.Sum(e => e.isactive ? e.receivable : 0));
            if (Math.Abs(receivablediff) > 1)
            {
                if (!isMigration)
                    throw new Exception("Barcode total receivable different from actual, please try again or contact software vendor.");
            }

            if (oBill.additionalchargeslist != null && oBill.additionalchargeslist.Exists(x => x.chargetype == "EXPORTGST"))
            {
                if (oBill.additionalchargeslist.FindAll(x => x.chargetype == "EXPORTGST").All(y => y.taxpercentage == 0))
                {
                    if (oBill.tax > 0)
                        throw new Exception("Export bill can't have gst");
                    if (oBill.Billitems.Any(x => x.taxpercentage > 0 || x.tax > 0))
                        throw new Exception("Export bill items can't have gst");
                }
            }

            if (!oBill.issynchronize)
            {
                if (this.Context != null)
                {
                    if (oBill.billattributes == null)
                        oBill.billattributes = new BillAttribute();
                    oBill.billattributes.biller = this.Context.EmployeeName;
                }

                if (oBill.billtype == Bill.BillTypes.Credit && (oBill.isreturn || oBill.receivable < 0))
                {
                    throw new Exception("Return bill can't be a credit bill entry");
                }
                if (oBill.billtype == Bill.BillTypes.ReturnBill)
                {
                    if (oBill.receivable > 0 || oBill.tax > 0 || oBill.discount > 0 || oBill.Billitems.Exists(e => e.discount > 0 || e.receivable > 0 || e.tax > 0 || e.qty > 0))
                    {
                        var exp = new Exception("Return bill contains positive values");
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBill, Context.UserID, oBill.locationid);
                        throw exp;
                    }
                    //expose separate credit bill type
                    //if (oBill.iscreditbill)
                    //    oBill.billtype = Bill.BillTypes.Credit;
                }
                else if (oBill.billtype == Bill.BillTypes.Cancel && !oBill.isinterstatetransfer)
                {
                    if (oBill.receivable > 0 || oBill.tax > 0 || oBill.discount > 0 || oBill.Billitems.Exists(e => e.discount > 0 || e.receivable > 0 || e.tax > 0 || e.qty > 0))
                    {
                        if (string.IsNullOrEmpty(oBill.returnbillno))
                        {
                            var exp = new Exception("Cancel bill contains positive values");
                            WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBill, Context.UserID, oBill.locationid);
                            throw exp;
                        }
                    }
                }
                else if (!oBill.isinterstatetransfer && oBill.Billitems.Count > 0)
                {
                    if (oBill.returnbillid == 0 && (oBill.receivable < 0 || oBill.tax < 0 || oBill.discount < 0
                        || oBill.Billitems.All(e => e.discount < 0 || e.receivable < 0 || e.tax < 0 || e.qty < 0)))
                    {
                        var exp = new Exception("Normal bill contains negative values");
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBill, Context.UserID, oBill.locationid);
                        throw exp;
                    }

                    if (!ValidateMinimumMargin(oBill))
                    {
                        var exp = new Exception("Normal bill didn't meet the minimum margin conditions");
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBill, Context.UserID, oBill.locationid);
                        throw exp;
                    }
                }

                if (LicenseManager.Instance(Context.OrganizationID).GetLicense().isoffline &&
                    !string.IsNullOrEmpty(Services.ConfigManager.Instance(Context.OrganizationID).Config.system_offlineprefix))
                    oBill.billno = Services.ConfigManager.Instance(Context.OrganizationID).Config.system_offlineprefix + oBill.billno;
                else if (!LicenseManager.Instance(Context.OrganizationID).GetLicense().isoffline && !string.IsNullOrEmpty(Services.ConfigManager.Instance(Context.OrganizationID).Config.system_onlineprefix))
                    oBill.billno = Services.ConfigManager.Instance(Context.OrganizationID).Config.system_onlineprefix + oBill.billno;
            }


            //if (oBill.billtype == Bill.BillTypes.ReturnBill && oBill.receivable < 0 && oBill.tax < 0)
            //    oBill.tax = oBill.tax * -1;

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.isactive;
            command.Parameters.Add("HasPromotionApplied", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.HasPromotionApplied;
            command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.isinterstatesale;
            command.Parameters.Add("unsettledcancelled", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.unsettledcancelled;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.createdby > 0 ? oBill.createdby : Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.modifiedby > 0 ? oBill.modifiedby : Context.UserID;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.locationid;
            command.Parameters.Add("counterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.counterid;
            command.Parameters.Add("unlimitedcouponid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.unlimitedcouponid;
            command.Parameters.Add("customerstateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.customerstateid;
            command.Parameters.Add("ecomorderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oBill.ecomorderno) ? "" : oBill.ecomorderno;


            command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.floorid;
            command.Parameters.Add("additionalcharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.additionalcharges;
            command.Parameters.Add("isb2b", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.isb2b;
            command.Parameters.Add("isreturn", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.isreturn;
            command.Parameters.Add("iscancel", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.iscancel;
            command.Parameters.Add("istransfer", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.istransfer;
            command.Parameters.Add("iscreditapproved", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.iscreditapproved;

            command.Parameters.Add("iscreditbill", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.iscreditbill;
            oBill.tzoffset = Context.UTCOffset;
            command.Parameters.Add("tzoffset", NpgsqlTypes.NpgsqlDbType.Integer).Value = Context.UTCOffset;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBill.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBill.createdon == DateTime.MinValue ? DateTimeNow : oBill.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBill.modifiedon == DateTime.MinValue ? DateTimeNow : oBill.modifiedon;
            command.Parameters.Add("billmasterid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.billmasterid;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBill.billno == null ? "" : oBill.billno;
            command.Parameters.Add("billattributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oBill.billattributes == null
                ? "null" : oBill.billattributes_json;

            command.Parameters.Add("emidetail", NpgsqlTypes.NpgsqlDbType.Json).Value = oBill.emidetail == null
                ? "null" : oBill.emidetail_json;

            command.Parameters.Add("hasemi", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.hasemi;

            command.Parameters.Add("offline", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.offline;
            command.Parameters.Add("isinterstatetransfer", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.isinterstatetransfer;

            command.Parameters.Add("returnreasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.returnreasonid;
            command.Parameters.Add("returnbillno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBill.returnbillno == null ? "" : oBill.returnbillno;
            command.Parameters.Add("billdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBill.billdate;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.companyid;
            command.Parameters.Add("billtype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBill.billtype;
            command.Parameters.Add("discountstatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)oBill.discountstatus;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.additionaldiscount;
            command.Parameters.Add("schemediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.schemediscount;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.tax;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.rounding;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.discount;
            oBill.qty = oBill.Billitems.Sum(e => e.qty);
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.qty;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.receivable + oBill.additionalcharges;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.customerid;

            command.Parameters.Add("salereasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.salereasonid;
            command.Parameters.Add("seasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.seasonid;
            command.Parameters.Add("areaid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.areaid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBill.id = Convert.ToInt64(reader[0].ToString());
                }
            }

            BillitemsDataService subBillitems = new BillitemsDataService() { Context = this.Context };
            subBillitems.Context = this.Context;
            if (oBill.DontChangeStock)
                subBillitems.IsMigration = true;

            foreach (var item in oBill.Billitems)
            {
                if (!oBill.issynchronize && oBill.billtype == Bill.BillTypes.ReturnBill && item.receivable < 0 && item.tax > 0)
                    item.tax = item.tax * -1;
                item.billid = oBill.id;
            }

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan &&
                oBill.Billitems.Exists(e => e.attributes != null && e.attributes.serialno != null &&
                 e.attributes.serialno.Length > 0))
            {

                var findallconflictso = oBill.Billitems.FindAll(x => x.attributes == null ||
                x.attributes.so == null ||
                x.attributes.so.Count == 0 ||
                x.attributes.so.Sum(y => y.qty) != x.qty);
                UpdateUANBillItems(oBill, transaction, conn);
            }


            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 5 &&
                oBill.billtype != Bill.BillTypes.ReturnBill &&
                oBill.istransfer != true && string.IsNullOrEmpty(oBill.ecomorderno) && !oBill.DontChangeStock)
            {
                var somissingitems = oBill.Billitems.FindAll(x => x.attributes != null &&
                (x.attributes.so == null || x.attributes.so.Count == 0));
                foreach (var bitm in somissingitems)
                {
                    if (bitm.attributes != null &&
                        (bitm.attributes.so == null || bitm.attributes.so.Count == 0) &&
                        !string.IsNullOrEmpty(bitm.attributes.serialno))
                    {
                        var existing = oBill.Billitems.FindAll(x => x.attributes != null &&
                        x.attributes.so != null && x.attributes.so.Count > 0 && x.attributes.serialno == bitm.attributes.serialno);
                        if (existing.Count > 0)
                        {
                            bitm.attributes.so = new List<BillItemAttributes.itemqty>();
                            bitm.attributes.so.Add(new BillItemAttributes.itemqty()
                            {
                                id = existing[0].attributes.so[0].id,
                                qty = bitm.qty
                            });
                        }
                    }
                }
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBill.locationid) &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].sales_so_mandatory)
            {
                var totalqty = oBill.Billitems.Sum(x => x.qty);
                var soqty = oBill.Billitems.Sum(x => (x.attributes == null ||
                x.attributes.so == null) ? 0 : x.attributes.so.Sum(y => y.qty));

                SalesorderDataService SalesorderData = new SalesorderDataService();
                List<SalesOrderDetail> result = SalesorderData.GetSaleOrder(oBill.locationid.ToString(), oBill.billattributes.orderrefid, "0");
                var somissing = oBill.Billitems.FindAll(x => (x.attributes == null ||
                x.attributes.so == null || x.attributes.so.Sum(y => y.qty) != x.qty));
                foreach (var soitem in somissing)
                {
                    if (soitem.attributes == null)
                        soitem.attributes = new BillItemAttributes();
                    if (soitem.attributes.so == null)
                        soitem.attributes.so = new List<BillItemAttributes.itemqty>();

                    if (!string.IsNullOrEmpty(soitem.attributes.serialno))
                    {
                        var availablesoitems = result.FindAll(x => x.uid == soitem.attributes.serialno);
                        decimal required = soitem.qty - (soitem.attributes.so.Sum(x => x.qty));
                        int i = 0;
                        while (availablesoitems.Count > 0 && availablesoitems.Count > i && required > 0)
                        {
                            if (availablesoitems[i].pendingqty == 0) continue;
                            if (availablesoitems[i].pendingqty <= required)
                            {
                                soitem.attributes.so.Add(new BillItemAttributes.itemqty()
                                {
                                    id = availablesoitems[i].soitemid,
                                    qty = required
                                });
                                availablesoitems[i].pendingqty -= required;
                                required = 0;
                                break;
                            }
                            else
                            {
                                soitem.attributes.so.Add(new BillItemAttributes.itemqty()
                                {
                                    id = availablesoitems[i].soitemid,
                                    qty = availablesoitems[i].pendingqty
                                });
                                required -= availablesoitems[i].pendingqty;
                                availablesoitems[i].pendingqty = 0;
                            }
                            i++;
                        }

                    }
                }
                throw new Exception("SO Qty not matched with Billed Qty");
            }

            foreach (var item in oBill.Billitems.ToArray())
            {
                subBillitems.Insert(transaction, conn, oBill, item, oBill.locationid);
            }

            oBill.Billitems.RemoveAll(x => x.isactive == false);

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_posvoid &&
                oBill.voiditems != null && oBill.voiditems.Count > 0)
            {
                foreach (var item in oBill.voiditems.ToArray())
                {
                    item.billid = oBill.id;
                    subBillitems.InsertInternal(transaction, conn, oBill, item, oBill.locationid);
                }
            }

            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBill.locationid) &&
            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].sale_negativestock &&
            !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportuannegative)
            {
                string query = string.Format(@"select salablegoods.barcode,stock.serialno from salablegoods 
                inner join stock on stock.id = stockid
                inner join billitems on billitems.billid = {0} and billitems.isactive=true and billitems.salablegoodsid = salablegoods.id and billitems.qty>0 
                where salablegoods.iqty-salablegoods.oqty-salablegoods.dqty-salablegoods.jqty<0", oBill.id);
                List<string> negativebarcodes = new List<string>();
                using (var reader = ExecuteReader(query, new SortedList<string, object>(), conn, transaction))
                {
                    while (reader.Read())
                    {
                        if (!reader.IsDBNull(1) && !string.IsNullOrEmpty(reader.GetString(1)))
                        {
                            negativebarcodes.Add(reader.GetString(1));
                        }
                        else
                        {
                            negativebarcodes.Add(reader.GetString(0));
                        }
                    }
                }
                if (negativebarcodes.Count > 0
                    && oBill.billtype != Bill.BillTypes.ReturnBill
                    && oBill.billtype != Bill.BillTypes.BulkSaleReturn)
                {
                    throw new Exception("Negative Stocks , " + string.Join(",", negativebarcodes.ToArray()));
                }
            }



            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBill.locationid) &&
                oBill.billtype != Bill.BillTypes.ReturnBill &&
               !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].system_disablemfgmeterwarning)
            {
                var vparam = new SortedList<string, object>();
                decimal tqty = 0;
                decimal tpiece = 0;
                int rows = 0;
                vparam.Add("billid", oBill.id);
                using (var readr = ExecuteReader("select sum(abs(qty)) qty,sum(abs(piece)) piece,count(*) noofrows from billitems where billid=@billid and isactive=true and qty>0", vparam, conn, transaction))
                {
                    if (readr.Read())
                    {
                        tqty = readr.IsDBNull(0) ? 0 : readr.GetDecimal(0);
                        tpiece = readr.IsDBNull(1) ? 0 : readr.GetDecimal(1);
                        rows = readr.IsDBNull(2) ? 0 : readr.GetInt32(2);
                    }
                }



                if (Math.Abs(oBill.totalqty) != Math.Abs(tqty) ||
                   Math.Abs(tqty) != Math.Abs(tpiece))
                {
                    var table = ExecuteDataSet("select id,qty,piece,attributes->>'serialno' uan from billitems where billid=@billid and isactive=true ", vparam, conn, transaction).Tables[0];
                    StringBuilder stringBuilder = new StringBuilder();
                    foreach (DataRow dr in table.Rows)
                    {
                        if (Convert.ToDecimal(dr["qty"]) != oBill.Billitems.Find(x => x.id == Convert.ToInt64(dr["id"])).qty ||
                            Convert.ToDecimal(dr["piece"]) != oBill.Billitems.Find(x => x.id == Convert.ToInt64(dr["id"])).piece)
                        {
                            var bitm = oBill.Billitems.Find(x => x.id == Convert.ToInt64(dr["id"]));
                            stringBuilder.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\r\n", dr["id"], dr["qty"], dr["piece"],
                            bitm.qty,
                            bitm.piece,
                            bitm.attributes.designid + " - " + bitm.attributes.serialno);
                        }
                    }

                    WebAPI.Common.Logger.Instance(Context.OrganizationID).PerformanceLog("Bill qty mismatching", WebAPI.Common.Logger.LogType.Info, "bill/insert", oBill,
                        Context.UserID, oBill.locationid);
                    throw new Exception("Some of the quantity got mismatched, please contact vendor - " + oBill.ToJSON());
                }
            }

            if (oBill.isb2b && oBill.customerid > 0)
            {
                CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
                var customer = customerDataService.Search(new CustomerCreteria()
                {
                    id = oBill.customerid
                });

                if (customer.Count == 1)
                {
                    BillinboxDataService billinboxDataService = new BillinboxDataService() { Context = this.Context };
                    Billinbox billinbox = new Billinbox();
                    billinbox.billid = oBill.id;
                    billinbox.billno = oBill.billno;
                    billinbox.billvalue = oBill.total;
                    billinbox.billdate = oBill.billdate;
                    billinbox.additionalcharges = oBill.additionalcharges;
                    billinbox.createdby = oBill.createdby;
                    billinbox.createdon = oBill.createdon;
                    billinbox.customerid = oBill.customerid;
                    billinbox.customername = customer[0].name;
                    billinbox.transportid = customer[0].preferredtransportid;
                    billinbox.modeofdelivery = 0;
                    billinbox.isactive = true;
                    billinbox.version = 1;
                    billinbox.tolocation = customer[0].cityname;
                    billinbox.tolocationid = customer[0].cityid;
                    if (oBill.isreturnbill)
                    {
                        if (oBill.billattributes != null && !string.IsNullOrEmpty(oBill.billattributes.printrequiredapproval))
                        {
                            billinbox.status = 1;
                            billinbox.remark = oBill.billattributes.printrequiredapproval;
                        }
                    }
                    else
                    {
                        billinbox.status = oBill.isb2b && !oBill.iscreditapproved ? 1 : 0;
                        billinbox.remark = oBill.isb2b && !oBill.iscreditapproved && oBill.billattributes != null ? oBill.billattributes.printrequiredapproval : "";
                    }
                    billinbox.address = new Billinbox.Address();
                    billinbox.address.detail = customer[0].address;
                    billinbox.address.area = customer[0].area;

                    if (oBill.billattributes != null && oBill.billattributes.shiptoid > 0)
                    {
                        customer = customerDataService.Search(new CustomerCreteria()
                        {
                            id = oBill.billattributes.shiptoid
                        });
                        billinbox.address.detail = customer[0].address;
                        billinbox.address.area = customer[0].area;
                        billinbox.transportid = customer[0].preferredtransportid;
                        billinbox.tolocation = customer[0].cityname;
                        billinbox.tolocationid = customer[0].cityid;
                    }
                    billinboxDataService.Insert(transaction, conn, billinbox);
                    oBill.billattributes.billinboxid = billinbox.id;
                }


                if (oBill.isreturn || oBill.isreturnbill)
                {
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_restrictextrareturn)
                    {
                        string returnvalidationquery = string.Format(@"select sum(billitems.qty) billedqty,salablegoodsid,barcode from bill
						inner join billitems on billitems.billid = bill.id and billitems.isactive=true
						where bill.customerid = @customerid and bill.billtype != 9 and bill.isactive=true and billitems.isactive=true
						and billitems.salablegoodsid in ({0})
						group by salablegoodsid,barcode
                        having sum(billitems.qty)<0", string.Join(",", oBill.Billitems.ConvertAll(x => x.salablegoodsid)));

                        var paramreturn = new SortedList<string, object>();
                        paramreturn.Add("currentbillid", oBill.id);
                        paramreturn.Add("customerid", oBill.customerid);

                        var errorMessage = new List<string>();
                        using (var reader = ExecuteReader(returnvalidationquery, paramreturn, conn, transaction))
                        {
                            while (reader.Read())
                            {
                                errorMessage.Add(String.Format("{0} Return Exceeds {1}",
                                                        reader["barcode"].ToString(),
                                                        Convert.ToDecimal(reader["billedqty"]) * -1));
                            }
                        }

                        if (errorMessage.Count > 0)
                        {
                            throw new Exception("Can't return more than purchase\n" + string.Join("\n", errorMessage));
                        }
                    }
                }
            }

            if (oBill.additionalchargeslist != null && oBill.additionalchargeslist.Count > 0)
            {
                BilladditionalchargesDataService billadditionalchargesDataService = new BilladditionalchargesDataService() { Context = this.Context };
                billadditionalchargesDataService.Context = this.Context;

                foreach (var item in oBill.additionalchargeslist)
                {
                    if (!oBill.issynchronize && oBill.billtype == Bill.BillTypes.ReturnBill)
                    {
                        item.Tax = Math.Abs(item.Tax) * -1;
                        item.Amount = Math.Abs(item.Amount) * -1;
                        item.Charge = Math.Abs(item.Charge) * -1;
                    }
                    item.billid = oBill.id;
                    item.createdby = Context.UserID;
                    item.modifiedby = Context.UserID;
                    item.createdon = DateTimeNow;
                    item.modifiedon = DateTimeNow;
                    item.isactive = true;
                    billadditionalchargesDataService.Insert(transaction, conn, item);
                }
            }

            if (oBill.billattributes == null) oBill.billattributes = new BillAttribute();
            if (!string.IsNullOrEmpty(oBill.billattributes.packslipid))
            {
                PackingslipDataService packingslipDataService = new PackingslipDataService();
                packingslipDataService.Context = this.Context;
                packingslipDataService.UpdateBill(oBill, conn, transaction);
            }

            //            if (!oBill.issynchronize && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
            //InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBill.locationid) &&
            //InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid] != null &&
            //InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].settlement_reward &&
            //InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].sales_reward)
            //            {
            //                CustomerrewardsDataService customerrewardsDataService = new CustomerrewardsDataService() { Context = this.Context };
            //                customerrewardsDataService.Context = this.Context;
            //                customerrewardsDataService.ProcessRewards(transaction, conn, oBill);
            //            }

            if (!oBill.issynchronize && oBill.hasrewardcoupons)
            {
                if (oBill.billtype == Bill.BillTypes.Cancel)
                {
                    oBill.billattributes.couponamount = Math.Abs(oBill.billattributes.couponamount) * -1;
                    oBill.billattributes.rewardamount = Math.Abs(oBill.billattributes.rewardamount) * -1;
                    oBill.billattributes.manualrewardpoint = Math.Abs(oBill.billattributes.manualrewardpoint) * -1;
                }
                if (oBill.billattributes.rewardamount != 0)
                {
                    CustomerrewardsDataService customerrewardsDataService = new CustomerrewardsDataService() { Context = this.Context };
                    customerrewardsDataService.Context = this.Context;
                    customerrewardsDataService.ApplyReward(transaction, conn, oBill);
                }
                else if (oBill.billattributes.manualrewardpoint != 0 || oBill.billattributes.manualrewardamount != 0)
                {
                    CustomerrewardsDataService customerrewardsDataService = new CustomerrewardsDataService() { Context = this.Context };
                    customerrewardsDataService.Context = this.Context;
                    customerrewardsDataService.ApplyManualReward(transaction, conn, oBill);
                }
                else if (oBill.billattributes.couponamount != 0
                    && oBill.billtype != Bill.BillTypes.ReturnBill
                    && oBill.billtype != Bill.BillTypes.Cancel)
                {
                    bool _leadmanagement = false;
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID) != null &&
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid] != null)
                        _leadmanagement = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBill.locationid].system_enableleadmgmt;

                    CustomerCouponsDataService giftCouponDataService = new CustomerCouponsDataService() { Context = this.Context };
                    giftCouponDataService.Context = this.Context;

                    LeadmanagementDataService leadmgmtDataService = new LeadmanagementDataService() { Context = this.Context };
                    leadmgmtDataService.Context = this.Context;
                    if (oBill.billattributes.coupons != null && oBill.billattributes.coupons.Count > 0)
                    {
                        if (oBill.billattributes.coupons.Count > 1)
                        {
                            var coupons = giftCouponDataService.Search(new CustomerCouponsCreteria()
                            {
                                ids = oBill.billattributes.coupons.ConvertAll(x => x.id)
                            });
                            if(coupons.Exists(x=>x.isranged))
                            {
                                decimal famount = 0;
                                decimal tamount = 0;
                                decimal amount = 0;
                                List<CustomerCoupons.RangeRule> allrules = new List<CustomerCoupons.RangeRule>();
                                coupons.ForEach(x =>
                                {
                                    famount += x.rangedrule.OrderByDescending(y => y.fromamount).First().fromamount;
                                    tamount += x.rangedrule.OrderByDescending(y => y.fromamount).First().toamount;
                                    amount += x.rangedrule.OrderByDescending(y => y.fromamount).First().amount;
                                    allrules.AddRange(x.rangedrule);
                                });
                                allrules.Add(new CustomerCoupons.RangeRule()
                                {
                                     fromamount = famount,
                                     toamount=tamount,
                                     amount = amount
                                });
                                var coupon = coupons.First();
                                coupon.rangedrule = allrules;
                                var claimedamount = oBill.billattributes.coupons.Sum(x => x.amount);
                                var result = giftCouponDataService.FindStatus(coupon, false, oBill, oBill.receivable+ oBill.adjustedcouponamount);
                                if (result != null && !string.IsNullOrEmpty(result.reason))
                                {
                                    throw new Exception(result.reason);
                                }
                                
                                if(claimedamount> result.convertable)
                                {
                                    throw new Exception("Allowed coupon discount is " + result.convertable);
                                }
                            }
                        }

                        foreach (var item in oBill.billattributes.coupons)
                        {
                            giftCouponDataService.ApplyCoupon(transaction, conn, item.code
                                , item.id
                                , item.amount
                                , oBill
                                , billmaster.total + billmaster.coupondiscount);

                            if (_leadmanagement)
                            {
                                leadmgmtDataService.UpdateLeadStatus(transaction, conn, item.code, oBill.id, true);
                            }
                        }
                    }
                    else
                    {
                        giftCouponDataService.ApplyCoupon(transaction, conn, oBill.billattributes.couponcode
                           , oBill.billattributes.couponid
                           , oBill.billattributes.couponamount
                           , oBill
                           , billmaster.total + billmaster.coupondiscount);

                        if (_leadmanagement)
                        {
                            leadmgmtDataService.UpdateLeadStatus(transaction, conn, oBill.billattributes.couponcode, oBill.id, true);
                        }
                    }
                }
            }

            StocktransactionDataService stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
            stocktransactionDataService.InsertBillItems(oBill, transaction, conn);


            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_billlog)
            {
                BillLog.Instance(this.Context.OrganizationID).Log(oBill, transaction, conn);
            }
            return oBill;
        }
        public Bill Update(Bill oBill)
        {
            return Update(oBill, true);
        }
        public Bill Update(Bill oBill, bool updateItems)
        {
            if (oBill.billtype == Bill.BillTypes.Credit && !oBill.iscreditbill)
                oBill.billtype = Bill.BillTypes.Cash;

            NpgsqlTransaction transaction = null;
            try
            {


                var existing = Search(new BillCreteria()
                {
                    id = oBill.id
                });

                if (existing.Count == 0)
                {
                    oBill.id = 0;
                    oBill.errormessage = "This is bill not available in the system to update";
                    return oBill;
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();


                    if (updateItems && existing[0].additionaldiscount < oBill.additionaldiscount)
                    {
                        oBill.discountstatus = Discountqueue.DiscountStatus.Requested;
                        var discountqueue = new DiscountqueueDataService() { Context = this.Context };
                        if (existing[0].additionaldiscount == 0)
                        {
                            Discountqueue dq = new Discountqueue()
                            {
                                billid = oBill.id,
                                notes = oBill.discountrequestreason,
                                requestedamount = oBill.additionaldiscount,
                                requestedby = oBill.modifiedby,
                                requestedon = oBill.billdate,
                                billamount = oBill.receivable,
                                status = Discountqueue.DiscountStatus.Requested,
                                billno = oBill.billno,
                                isactive = true,
                                companyid = oBill.companyid
                            };
                            discountqueue.Insert(transaction, conn, dq);
                        }
                        else if (oBill.additionaldiscount > 0)
                        {
                            var odiscount = discountqueue.Search(new DiscountqueueCreteria()
                            {
                                billid = oBill.id
                            });
                            if (odiscount.Count == 0)
                            {
                                Discountqueue dq = new Discountqueue()
                                {
                                    billid = oBill.id,
                                    notes = oBill.discountrequestreason,
                                    requestedamount = oBill.additionaldiscount,
                                    requestedby = oBill.modifiedby,
                                    requestedon = oBill.billdate,
                                    isactive = true,
                                    billamount = oBill.receivable,
                                    status = Discountqueue.DiscountStatus.Requested,
                                    billno = oBill.billno,
                                    companyid = oBill.companyid
                                };
                                discountqueue.Insert(transaction, conn, dq);
                            }
                            else
                            {
                                odiscount[0].status = Discountqueue.DiscountStatus.Requested;
                                odiscount[0].requestedamount = oBill.additionaldiscount;
                                discountqueue.Update(transaction, conn, odiscount[0]);
                            }
                        }

                    }
                    else if (updateItems && existing[0].additionaldiscount > 0)
                    {
                        oBill.discountstatus = Discountqueue.DiscountStatus.NoDiscount;
                        var discountqueue = new DiscountqueueDataService() { Context = this.Context };
                        var odiscount = discountqueue.Search(new DiscountqueueCreteria()
                        {
                            billid = oBill.id
                        });
                        if (odiscount.Count > 0)
                        {
                            odiscount[0].hasaccepted = true;
                            discountqueue.Update(transaction, conn, odiscount[0]);
                        }
                    }

                    Update(transaction, conn, oBill, false);

                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                        !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan &&
                        oBill.Billitems.Exists(e => e.attributes != null && e.attributes.serialno != null &&
                        e.attributes.serialno.Length > 0))
                    {

                        var findallconflictso = oBill.Billitems.FindAll(x => x.attributes == null ||
                        x.attributes.so == null ||
                        x.attributes.so.Count == 0 ||
                        x.attributes.so.Sum(y => y.qty) != x.qty);
                        UpdateUANBillItems(oBill, transaction, conn);
                    }

                    if (updateItems)
                    {
                        BillitemsDataService subBillitems = new BillitemsDataService() { Context = this.Context };
                        subBillitems.Context = this.Context;
                        subBillitems.Update(transaction, conn, oBill, oBill.Billitems, oBill.id, oBill.locationid);
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_posvoid &&
                                        oBill.voiditems != null && oBill.voiditems.Count > 0)
                        {
                            foreach (var item in oBill.voiditems.ToArray())
                            {
                                item.billid = oBill.id;
                                subBillitems.InsertInternal(transaction, conn, oBill, item, oBill.locationid);
                            }
                        }
                    }
                    if (oBill.isb2b && oBill.customerid > 0)
                    {
                        BillinboxDataService billinboxDataService = new BillinboxDataService() { Context = this.Context };
                        billinboxDataService.UpdateBillValue(transaction, conn, oBill.id, oBill.total);
                    }

                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_ismfgapplication)
                    {
                        var vparam = new SortedList<string, object>();
                        decimal tqty = 0;
                        decimal tpiece = 0;
                        vparam.Add("billid", oBill.id);
                        using (var readr = ExecuteReader("select sum(abs(qty)) qty,sum(abs(piece)) piece from billitems where billid=@billid and isactive=true", vparam, conn, transaction))
                        {
                            if (readr.Read())
                            {
                                tqty = readr.GetDecimal(0);
                                tpiece = readr.GetDecimal(1);
                            }
                        }
                        if (Math.Abs(oBill.totalqty) != Math.Abs(tqty) ||
                            Math.Abs(tqty) != Math.Abs(tpiece))
                        {
                            decimal totalqty = oBill.Billitems.Sum(x => x.qty);
                            decimal totalpiece = oBill.Billitems.Sum(x => x.piece);

                            var table = ExecuteDataSet("select id,qty,piece from billitems where billid=@billid and isactive=true ", vparam, conn, transaction).Tables[0];
                            StringBuilder stringBuilder = new StringBuilder();
                            foreach (DataRow dr in table.Rows)
                            {
                                if (Convert.ToDecimal(dr["qty"]) != oBill.Billitems.Find(x => x.id == Convert.ToInt64(dr["id"])).qty ||
                                    Convert.ToDecimal(dr["piece"]) != oBill.Billitems.Find(x => x.id == Convert.ToInt64(dr["id"])).piece)
                                {
                                    var bitm = oBill.Billitems.Find(x => x.id == Convert.ToInt64(dr["id"]));
                                    stringBuilder.AppendFormat("{0}\t{1}\t{2}\t{3}\t{4}\t{5}\r\n", dr["id"], dr["qty"], dr["piece"],
                                    bitm.qty,
                                    bitm.piece,
                                    bitm.attributes.designid + " - " + bitm.attributes.serialno);
                                }
                            }

                            WebAPI.Common.Logger.Instance(Context.OrganizationID).PerformanceLog("Bill qty mismatching", WebAPI.Common.Logger.LogType.Info,
                                "bill/insert", oBill,
                                Context.UserID, oBill.locationid);
                            throw new Exception("Some of the quantity got mismatched, please contact vendor - " + stringBuilder.ToString());
                        }
                    }

                    BilladditionalchargesDataService billadditionalchargesDataService = new BilladditionalchargesDataService() { Context = this.Context };
                    billadditionalchargesDataService.Context = this.Context;
                    billadditionalchargesDataService.DeleteAll(oBill.id, Context.UserID, conn, transaction);
                    if (oBill.additionalchargeslist != null && oBill.additionalchargeslist.Count > 0)
                    {
                        foreach (var item in oBill.additionalchargeslist)
                        {
                            if (!oBill.issynchronize && oBill.billtype == Bill.BillTypes.ReturnBill)
                            {
                                item.Tax = Math.Abs(item.Tax) * -1;
                                item.Amount = Math.Abs(item.Amount) * -1;
                                item.Charge = Math.Abs(item.Charge) * -1;
                            }
                            item.billid = oBill.id;
                            item.createdby = Context.UserID;
                            item.modifiedby = Context.UserID;
                            item.createdon = DateTimeNow;
                            item.modifiedon = DateTimeNow;
                            item.isactive = true;
                            billadditionalchargesDataService.Insert(transaction, conn, item);
                        }
                    }

                    CustomerinvoiceDataService invoicedata = new CustomerinvoiceDataService() { Context = this.Context };
                    List<Customerinvoice> lstinvoice = invoicedata.Search(new CustomerinvoiceCreteria() { billid = oBill.id });
                    if (lstinvoice != null && lstinvoice.Count > 0)
                    {
                        if (lstinvoice[0].paymentid > 0 || lstinvoice[0].paid != 0)
                        {
                            throw new Exception("Payment Received. Can't Edit the bill");
                        }
                        lstinvoice[0].receivable = oBill.receivable + oBill.rounding + oBill.additionalcharges;
                        lstinvoice[0].transactiondate = oBill.billdate;
                        lstinvoice[0].customerid = oBill.customerid;
                        invoicedata.Update(transaction, conn, lstinvoice[0]);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace, Context.UserID, oBill.locationid);
                throw exp;

            }

            return oBill;
        }


        public Bill UpdateInteral(Bill oBill)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    oBill.DontChangeStock = true;
                    isMigration = true;
                    transaction = conn.BeginTransaction();
                    Update(transaction, conn, oBill, false);
                    BillitemsDataService subBillitems = new BillitemsDataService() { Context = this.Context };
                    subBillitems.Context = this.Context;
                    subBillitems.IsMigration = true;
                    subBillitems.Update(transaction, conn, oBill, oBill.Billitems, oBill.id, oBill.locationid);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace, Context.UserID, oBill.locationid);
            }
            return oBill;
        }


        public Bill Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Bill oBill, bool updateitems)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.isactive;
            command.Parameters.Add("isinterstatetransfer", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.isinterstatetransfer;
            command.Parameters.Add("HasPromotionApplied", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.HasPromotionApplied;
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
            command.Parameters.Add("billtype", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)oBill.billtype;
            command.Parameters.Add("discountstatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)oBill.discountstatus;
            command.Parameters.Add("additionaldiscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.additionaldiscount;
            command.Parameters.Add("schemediscount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.schemediscount;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.tax;

            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.discount;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.receivable + oBill.additionalcharges;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.customerid;
            oBill.qty = oBill.Billitems.Sum(e => e.qty);
            command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.qty;
            command.Parameters.Add("billattributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oBill.billattributes == null ? "null" : oBill.billattributes_json;
            command.Parameters.Add("offline", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.offline;
            command.Parameters.Add("additionalcharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBill.additionalcharges;
            command.Parameters.Add("isb2b", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.isb2b;
            command.Parameters.Add("isreturn", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.isreturn;
            command.Parameters.Add("iscancel", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.iscancel;
            command.Parameters.Add("iscreditapproved", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.iscreditapproved;
            command.Parameters.Add("iscreditbill", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.iscreditbill;
            command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.floorid;
            command.Parameters.Add("emidetail", NpgsqlTypes.NpgsqlDbType.Json).Value = oBill.emidetail == null ? "null" : oBill.emidetail_json;
            command.Parameters.Add("hasemi", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.hasemi;
            command.Parameters.Add("salereasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.salereasonid;
            command.Parameters.Add("seasonid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.seasonid;
            command.Parameters.Add("areaid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.areaid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBill.version = Convert.ToInt32(reader[0]);
                }
                else
                {
                    updateitems = false;
                }
            }
            if (updateitems)
            {
                BillitemsDataService subBillitems = new BillitemsDataService() { Context = this.Context };
                subBillitems.Context = this.Context;
                subBillitems.IsMigration = oBill.DontChangeStock;
                foreach (var item in oBill.Billitems)
                {
                    if (oBill.billtype == Bill.BillTypes.ReturnBill && item.receivable < 0 && item.tax > 0)
                        item.tax = item.tax * -1;

                    item.billid = oBill.id;
                    subBillitems.Update(transaction, conn, oBill, item, item);
                }
            }
            return oBill;
        }

        public bool isMigration { get; set; }
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
                    BillitemsDataService subBillitems = new BillitemsDataService() { Context = this.Context };
                    subBillitems.Context = this.Context;
                    subBillitems.DeleteAll(oBill.id, oBill.modifiedby, conn, transaction);
                    var stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
                    stocktransactionDataService.DeleteBillItems(oBill, transaction, conn, true);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace, Context.UserID, oBill.locationid);
            }
            return successCount > 0;

        }

        public void ValidateRangeTax()
        {
            /*
            with salesloss
            as
            (
            select bill.billdate,billitems.barcode,billitems.receivable/billitems.qty rate,billitems.qty from billitems
            inner join bill on bill.id =billitems.billid
            where (billitems.receivable)/billitems.qty>1045 and 
            (billitems.receivable)/billitems.qty<1120 and billitems.qty>0
            and billitems.sellingmode!=3 and billitems.additionaldiscount=0 and billitems.schemediscount=0
            )
			select sum(round(998-(rate/112*100),2)*qty) lossvalue from salesloss

            select salesloss.*,round(998-(rate/112*100),2)*qty lossvalue from salesloss

            select round(998-((salablegoods.price-salablegoods.discount)/112*100),2),
            salablegoods.price,salablegoods.discount,salablegoods.barcode,iqty-(oqty+jqty+dqty) from stock 
            inner join salablegoods  on salablegoods.stockid = stock.id
            inner join products on products.id = stock.productid
			inner join tax on tax.id=products.salestaxid and tax.type=2
            where (salablegoods.price-salablegoods.discount)>1049 
            and (salablegoods.price-salablegoods.discount)<1120 and stock.isactive=true
            and salablegoods.hasstock=true and iqty-(oqty+jqty+dqty)>0
			
            */

        }
        public bool SetCancelled(NpgsqlTransaction transaction, NpgsqlConnection conn, long billid)
        {
            NpgsqlCommand command = GetCommand(SQL_Cancel, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = billid;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        public bool SetSettled(NpgsqlTransaction transaction, NpgsqlConnection conn, long billid)
        {
            NpgsqlCommand command = GetCommand(SQL_Settled, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = billid;
            int successCount = command.ExecuteNonQuery();
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

        internal void SyncEcommerceStock(bool isolbill, List<Billitems> stockAdjustment, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            if (isolbill)
            {
                foreach (var bitem in stockAdjustment)
                {
                    bitem.attributes.newolstock -= bitem.qty;
                    if (bitem.attributes.newolstock < 0)
                        bitem.attributes.newolstock = 0;
                    NpgsqlCommand command = GetCommand("Update salablegoods set olqty=@olqty where id=@id", conn, transaction);
                    command.Parameters.Add("olqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = bitem.attributes.newolstock;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = bitem.salablegoodsid;
                    command.ExecuteNonQuery();
                }
            }
            else
            {
                foreach (var bitem in stockAdjustment)
                {
                    if (bitem.attributes.newstock < 0)
                    {
                        bitem.attributes.newstock = 0;
                    }
                    bitem.attributes.newolstock = bitem.attributes.newstock;
                    NpgsqlCommand command = GetCommand("Update salablegoods set olqty=@olqty where id=@id", conn, transaction);
                    command.Parameters.Add("olqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = bitem.attributes.newolstock;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = bitem.salablegoodsid;
                    command.ExecuteNonQuery();
                }
            }
        }

        internal Bill PrepareTouchBill(Bill item)
        {
            var serialbased = Services.ConfigManager.Instance(Context.OrganizationID).Config.system_fixed_barcodes &&
                !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_hyper_uan;

            var outofstockitems = CacheManager.Instance(Context.OrganizationID).GetOutOfStockProducts(item.locationid, item.companyid);
            if (outofstockitems != null &&
                outofstockitems.Count > 0)
            {
                var outofstocklist = item.Billitems.FindAll(x => outofstockitems.Contains(x.productid));
                if (outofstocklist.Count > 0)
                {
                    throw new Exception("Out Of Stock - " + string.Join(",", outofstocklist.ConvertAll(x => x.printingname)));
                }
            }

            if (serialbased)
            {
                var barcodelist = string.Join(",", item.Billitems.ConvertAll(x => "'" + x.barcode + "'"));
                string query = string.Format(@"select stock.id stockid,stock.serialno,stock.barcode,
                             salablegoods.id salablegoodsid,(iqty-(oqty+jqty+aqty)) stock from stock
                             inner join salablegoods on salablegoods.stockid = stock.id and salablegoods.locationid=@locationid
                             where stock.serialno in ({0}) and stock.isactive=true
                             and salablegoods.isactive=true and salablegoods.hasstock=true
                             order by (iqty-(oqty+jqty+aqty)) desc",
                                 barcodelist);

                var param = new SortedList<string, object>();
                param.Add("locationid", item.locationid);
                item.Billitems.ForEach(x => x.barcode = "");

                using (var reader = ExecuteReader(query, param))
                {
                    while (reader.Read())
                    {
                        var serialno = Convert.ToString(reader["serialno"]);
                        var barcode = Convert.ToString(reader["barcode"]);
                        var stockid = Convert.ToInt64(reader["stockid"]);
                        var stock = Convert.ToDecimal(reader["stock"]);
                        var salablegoodsid = Convert.ToInt64(reader["salablegoodsid"]);
                        var glnitems = item.Billitems.FindAll(x => x.barcode == "" && x.attributes.serialno == serialno).OrderBy(x => x.qty).ToList();
                        foreach (var glnitem in glnitems)
                        {
                            if (stock >= glnitem.qty)
                            {
                                glnitem.barcode = barcode;
                                glnitem.salablegoodsid = salablegoodsid;
                                glnitem.stockid = stockid;
                                stock -= glnitem.qty;
                            }
                        }
                        if (!item.Billitems.Exists(x => x.salablegoodsid == 0))
                            break;
                    }
                }

                if (item.Billitems.Exists(x => x.barcode == ""))
                {
                    var missingbarcodes = item.Billitems.FindAll(x => x.barcode == "");
                    barcodelist = string.Join(",", missingbarcodes.ConvertAll(x => "'" + x.attributes.serialno + "'"));
                    query = string.Format(@"select stock.id stockid,stock.serialno,stock.barcode,
                             salablegoods.id salablegoodsid,(iqty-(oqty+jqty+aqty)) stock from stock
                             inner join salablegoods on salablegoods.stockid = stock.id and salablegoods.locationid=@locationid
                             where stock.serialno in ({0}) and stock.isactive=true
                             and salablegoods.isactive=true and salablegoods.hasstock=true
                             order by (iqty-(oqty+jqty+aqty)) desc",
                                     barcodelist);
                    using (var reader = ExecuteReader(query, param))
                    {
                        while (reader.Read())
                        {
                            var serialno = Convert.ToString(reader["serialno"]);
                            var barcode = Convert.ToString(reader["barcode"]);
                            var stockid = Convert.ToInt64(reader["stockid"]);
                            var stock = Convert.ToDecimal(reader["stock"]);
                            var salablegoodsid = Convert.ToInt64(reader["salablegoodsid"]);

                            var glnitems = item.Billitems.FindAll(x => x.barcode == "" && x.attributes.serialno == serialno).OrderBy(x => x.qty).ToList();
                            foreach (var glnitem in glnitems)
                            {
                                glnitem.barcode = barcode;
                                glnitem.salablegoodsid = salablegoodsid;
                                glnitem.stockid = stockid;
                            }
                        }
                    }
                }

                if (item.Billitems.Exists(x => x.barcode == ""))
                {
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(item.locationid) &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_negativestock &&
                        !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportuannegative)
                    {
                        var missingbarcodes = item.Billitems.FindAll(x => x.barcode == "").ConvertAll(x => new { x.printingname, x.attributes.serialno });
                        StringBuilder stringBuilder = new StringBuilder();
                        missingbarcodes.ForEach(x =>
                        {
                            stringBuilder.AppendLine(x.serialno + ":" + x.printingname);
                        });
                        throw new Exception("Don't have stock for below details\r\n" + stringBuilder.ToString());
                    }
                    else
                    {
                        barcodelist = string.Join(",", item.Billitems.FindAll(x => x.barcode == "").ConvertAll(x => "'" + x.barcode + "'"));

                        var missingbarcodes = item.Billitems.FindAll(x => x.barcode == "");
                        barcodelist = string.Join(",", missingbarcodes.ConvertAll(x => "'" + x.attributes.serialno + "'"));
                        query = string.Format(@"select stock.id stockid,stock.serialno,stock.barcode,
                             salablegoods.id salablegoodsid,(iqty-(oqty+jqty+aqty)) stock from stock
                             inner join salablegoods on salablegoods.stockid = stock.id and salablegoods.locationid=@locationid
                             where stock.serialno in ({0}) and stock.isactive=true
                             and salablegoods.isactive=true
                             order by salablegoods.id desc", barcodelist);
                        using (var reader = ExecuteReader(query, param))
                        {
                            while (reader.Read())
                            {
                                var serialno = Convert.ToString(reader["serialno"]);
                                var barcode = Convert.ToString(reader["barcode"]);
                                var stockid = Convert.ToInt64(reader["stockid"]);
                                var stock = Convert.ToDecimal(reader["stock"]);
                                var salablegoodsid = Convert.ToInt64(reader["salablegoodsid"]);

                                var glnitems = item.Billitems.FindAll(x => x.barcode == "" && x.attributes.serialno == serialno).OrderBy(x => x.qty).ToList();
                                foreach (var glnitem in glnitems)
                                {
                                    glnitem.barcode = barcode;
                                    glnitem.salablegoodsid = salablegoodsid;
                                    glnitem.stockid = stockid;
                                }
                            }
                        }

                    }
                }


                item.createdby = Context.UserID;
                item.modifiedby = Context.UserID;
                item.createdon = DateTimeNow;
                item.modifiedon = DateTimeNow;
                item.version = 1;
                return item;
            }
            else
            {
                item.Billitems.ForEach(x => x.attributes.serialno = "");
                var barcodelist = string.Join(",", item.Billitems.ConvertAll(x => "'" + x.barcode + "'"));
                string query = string.Format(@"select stock.id stockid,stock.serialno,stock.barcode,
                             salablegoods.id salablegoodsid,(iqty-(oqty+jqty+aqty)) stock from stock
                             inner join salablegoods on salablegoods.stockid = stock.id and salablegoods.locationid=@locationid
                             where stock.barcode in ({0}) and stock.isactive=true
                             and salablegoods.isactive=true and salablegoods.hasstock=true
                             order by (iqty-(oqty+jqty+aqty)) desc",
                                 barcodelist);

                var param = new SortedList<string, object>();
                param.Add("locationid", item.locationid);

                using (var reader = ExecuteReader(query, param))
                {
                    while (reader.Read())
                    {
                        var barcode = Convert.ToString(reader["barcode"]);
                        var stockid = Convert.ToInt64(reader["stockid"]);
                        var stock = Convert.ToDecimal(reader["stock"]);
                        var salablegoodsid = Convert.ToInt64(reader["salablegoodsid"]);
                        var glnitems = item.Billitems.FindAll(x => x.barcode == barcode).OrderBy(x => x.qty).ToList();
                        foreach (var glnitem in glnitems)
                        {
                            if (stock >= glnitem.qty)
                            {
                                glnitem.barcode = barcode;
                                glnitem.salablegoodsid = salablegoodsid;
                                glnitem.stockid = stockid;
                                stock -= glnitem.qty;
                            }
                        }
                        if (!item.Billitems.Exists(x => x.salablegoodsid == 0))
                            break;
                    }
                }


                if (item.Billitems.Exists(x => x.salablegoodsid == 0))
                {
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(item.locationid) &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[item.locationid].sale_negativestock &&
                        !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportuannegative)
                    {
                        var missingbarcodes = item.Billitems.FindAll(x => x.salablegoodsid == 0).ConvertAll(x => new { x.printingname, x.barcode });
                        StringBuilder stringBuilder = new StringBuilder();
                        missingbarcodes.ForEach(x =>
                        {
                            stringBuilder.AppendLine(x.barcode + ":" + x.printingname);
                        });
                        throw new Exception("Don't have stock for below details\r\n" + stringBuilder.ToString());
                    }
                    else
                    {
                        barcodelist = string.Join(",", item.Billitems.FindAll(x => x.salablegoodsid == 0).ConvertAll(x => "'" + x.barcode + "'"));
                        query = string.Format(@"select stock.id stockid,stock.serialno,stock.barcode,
                             salablegoods.id salablegoodsid,(iqty-(oqty+jqty+aqty)) stock from stock
                             inner join salablegoods on salablegoods.stockid = stock.id and salablegoods.locationid=@locationid
                             where stock.barcode in ({0}) and stock.isactive=true
                             and salablegoods.isactive=true 
                             order by salablegoods.id desc",
                                         barcodelist);

                        param = new SortedList<string, object>();
                        param.Add("locationid", item.locationid);

                        using (var reader = ExecuteReader(query, param))
                        {
                            while (reader.Read())
                            {
                                var barcode = Convert.ToString(reader["barcode"]);
                                var stockid = Convert.ToInt64(reader["stockid"]);
                                var stock = Convert.ToDecimal(reader["stock"]);
                                var salablegoodsid = Convert.ToInt64(reader["salablegoodsid"]);
                                var glnitems = item.Billitems.FindAll(x => x.barcode == barcode && x.salablegoodsid <= 0).OrderBy(x => x.qty).ToList();
                                foreach (var glnitem in glnitems)
                                {
                                    glnitem.barcode = barcode;
                                    glnitem.salablegoodsid = salablegoodsid;
                                    glnitem.stockid = stockid;
                                }
                                if (!item.Billitems.Exists(x => x.salablegoodsid == 0))
                                    break;
                            }
                        }
                    }
                }
                item.createdby = Context.UserID;
                item.modifiedby = Context.UserID;
                item.createdon = DateTimeNow;
                item.modifiedon = DateTimeNow;
                item.version = 1;
                return item;
            }
        }

        internal Bill UpdateAttributes(Bill oBill)
        {
            if (oBill.settled) throw new Exception("Settled bill attributes can't be changed.");

            CustomerinvoiceDataService customerdata = new CustomerinvoiceDataService() { Context = this.Context };
            List<Customerinvoice> lstinvoice = customerdata.Search(new CustomerinvoiceCreteria() { billid = oBill.id });

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string updateattribute = @"Update Bill set billtype=1,customerid=@customerid,billattributes=@billattributes,
                    isinterstatesale=@isinterstatesale,iscreditbill=@iscreditbill,isb2b=@isb2b,istransfer=@istransfer,isexport=@isexport,
                    modifiedby=@modifiedby,version=version+1,modifiedon=@modifiedon where id=@id  and isactive=true and version=@version RETURNING version";
                    NpgsqlCommand command = GetCommand(updateattribute, conn, transaction);
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBill.version;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.id;

                    command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBill.customerid;
                    command.Parameters.Add("billattributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oBill.billattributes_json;
                    command.Parameters.Add("isinterstatesale", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.isinterstatesale;
                    command.Parameters.Add("iscreditbill", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.iscreditbill;
                    command.Parameters.Add("isb2b", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.isb2b;
                    command.Parameters.Add("istransfer", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.istransfer;
                    command.Parameters.Add("isexport", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBill.isexport;
                    using (System.Data.IDataReader reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            oBill.version = Convert.ToInt32(reader[0]);
                        }
                    }

                    if (oBill.isb2b && lstinvoice.Count == 0)
                    {
                        customerdata.Insert(transaction, conn, new Customerinvoice
                        {
                            isactive = true,
                            createdby = -9,
                            createdon = DateTime.Now,
                            version = 1,
                            type = 2,
                            receivable = oBill.receivable + oBill.rounding,
                            transactiondate = oBill.billdate,
                            billid = oBill.id,
                            customerid = oBill.customerid,
                            billno = oBill.billno,
                            companyid = oBill.companyid
                        });
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace, Context.UserID, oBill.locationid);
            }
            return oBill;
        }

        public PromotionExchange PromotionExchange(PromotionExchange item)
        {
            BillitemsDataService dataService = new BillitemsDataService() { Context = Context };
            var billitems = dataService.Search(new BillitemsCreteria()
            {
                ids = item.items.ConvertAll(x => x.rtnsrcitemid)
            });
            item.items.ForEach(x =>
            {
                x.billitem = billitems.Find(y => y.id == x.rtnsrcitemid);
            });

            foreach (var grp in item.items.FindAll(x => x.sellingmode != 3).GroupBy(x => x.exchangeid))
            {
                var eqty = grp.Count();
                if (eqty > grp.First().eqty)
                {
                    throw new Exception(grp.First().exchangebarcode + " can be used maximum " + grp.First().eqty);
                }
            }

            List<string> eligiblebarcodes = new List<string>();
            foreach (var schemeitem in item.items.GroupBy(x => x.billitem.schemeid))
            {
                var barcodes = schemeitem.ToList().ConvertAll(x => x.exchangebarcode);
                barcodes.AddRange(schemeitem.ToList().ConvertAll(x => x.exchangeserialno));
                barcodes.RemoveAll(x => string.IsNullOrEmpty(x));
                List<Services.PromotionManager.ElibleReason> schemeids = Services.PromotionManager.Instance(Context.OrganizationID).FindAvailableMatchingSchmes(Context, new PromotionSearch()
                { 
                    locationid = item.locationid,
                    barcodes = barcodes,
                    schemeid = schemeitem.Key
                });
                
                foreach(var scheme in schemeids)
                {
                    eligiblebarcodes.AddRange(scheme.billitems.ConvertAll(x => x.barcode));
                }
            }

            item.items.ForEach(x =>
            {
                if(eligiblebarcodes.Contains(x.exchangebarcode) ||
                    (x.billitem.hasserial && eligiblebarcodes.Contains(x.exchangebarcode)))
                {
                    if(x.billitem.schemediscountpercentage>0)
                        x.discountpercentage = x.billitem.schemediscountpercentage;
                    if(x.billitem.schemediscount>0)
                        x.discountmax = x.billitem.schemediscount/x.billitem.qty;
                }
            });

            var promotiongroups = item.items.FindAll(x => x.discountpercentage > 0 || x.discountmax > 0).GroupBy(x => x.exchangebarcode);
            item.exchangeitems = promotiongroups.ToList().ConvertAll(x =>
            {
                return new PromotionExchange.Item()
                {
                    exchangebarcode = x.Key,
                    exchangeserialno = x.First().exchangeserialno,
                    qty = x.Sum(y=>y.qty),
                    eqty = x.First().eqty,
                    discountmax = x.First().discountmax,
                    discountpercentage = x.First().discountpercentage,
                };
            });

            return item;
        }

        internal void SendCreditOTP(Bill bill, string passcode)
        {
            if(bill.customerid>0)
            {
                CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
                var customer = customerDataService.Search(new CustomerCreteria() { id = bill.customerid });
                if (customer.Count == 1)
                    bill.customername = customer[0].name;
            }
            if (Processors.Schedule_Actions.Instance.HasConfig(this.Context.OrganizationID,
               Processors.Schedule_Actions.SourceType.OTP, Processors.Schedule_Actions.Actions.Otp_Credit))
            {
                Processors.Schedule_Actions.Instance.OnChange(this.Context.OrganizationID, new BillOTP()
                {
                    OTP = passcode,
                    BILLAMOUNT = bill.receivable + bill.rounding,
                    NETAMOUNT = bill.receivable + bill.rounding,
                    DISCOUNT = bill.discount,
                    CUSTOMER = bill.customername
                }, Processors.Schedule_Actions.Actions.Otp_Credit);
            }
        }
    }

}
