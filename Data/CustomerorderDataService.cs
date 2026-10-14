using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class CustomerorderDataService : SAASData
    {
        const string SQL_Search = @"select customer.name as name,customer.cityid,customer.mobileno as contactno,customer.address as address,customerorder.id,customerorder.createdby, customerorder.modifiedby, 
customerorder.companyid, customerorder.locationid, customerid, 
customerorder.version, status, modeofdelivery, customerorder.isactive, customerorder.createdon, customerorder.modifiedon, orderdate, planneddeliverydate, 
deliverydate, orderamount, paidamount,totalqty, customerorderno, notes, deliveryinfo, orderitems, statushistory, customer.categoryid,
communication, paymentinfo, vendorinfo, voucherinfo, employee.name ordertakenby from customerorder inner join customer on customer.id = customerorder.customerid  
left outer join users on users.id=customerorder.createdby
left outer join employee on employee.id=users.employeeid where customerorder.id=@id";


        const string SQL_Insert = @"INSERT INTO public.customerorder(createdby, modifiedby, companyid, locationid, customerid,version, status, modeofdelivery, isactive, createdon, modifiedon, orderdate, planneddeliverydate, deliverydate, orderamount, paidamount, customerorderno, notes, deliveryinfo, orderitems, statushistory, communication, paymentinfo, vendorinfo,totalqty,voucherinfo,cashpayment,bankpayment)
        VALUES(@createdby, @modifiedby, @companyid, @locationid, @customerid, @version, @status, @modeofdelivery, @isactive, @createdon, @modifiedon, @orderdate, @planneddeliverydate, @deliverydate, @orderamount, @paidamount, @customerorderno, @notes, @deliveryinfo, @orderitems, @statushistory, @communication, @paymentinfo, @vendorinfo,@totalqty,@voucherinfo,@cashpayment,@bankpayment) RETURNING id";


        const string SQL_Update = @"UPDATE public.customerorder SET id=@id, modifiedby=@modifiedby, companyid=@companyid, locationid=@locationid, customerid=@customerid,version=@version+1, status=@status, modeofdelivery=@modeofdelivery, modifiedon=@modifiedon, orderdate=@orderdate, planneddeliverydate=@planneddeliverydate, deliverydate=@deliverydate, orderamount=@orderamount, 
        paidamount=@paidamount, customerorderno=@customerorderno, notes=@notes, deliveryinfo=@deliveryinfo, orderitems=@orderitems, statushistory=@statushistory, communication=@communication, paymentinfo=@paymentinfo, vendorinfo=@vendorinfo,totalqty=@totalqty,
        cashpayment=@cashpayment,bankpayment=@bankpayment,voucherinfo=@voucherinfo where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.customerorder set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_AdvanceCollection = @"with advancecollection as 
(select customerorder.id, customerorder.paidamount, customerorder.createdby, customerorder.paymentinfo, customer.name, customer.mobileno,
cast(paymentlist->>'mode' as numeric) paymentmode, 
cast(paymentlist->>'amount' as numeric) paid, 
customerorder.locationid, customerorder.companyid  
from customerorder INNER JOIN LATERAL json_array_elements (customerorder.paymentinfo :: json) paymentlist ON true
left outer join customer on customer.id = customerorder.customerid 
where customerorder.isactive=true and customerorder.createdon>=@fromdate and customerorder.createdon<@todate
and case when @locationid=0 then customerorder.locationid <> 0 else customerorder.locationid = @locationid end
and case when @companyid=0 then customerorder.companyid >= 0 else customerorder.companyid = @companyid end
)
select location.name as location,  employee.name employeename, paymentmode, advancecollection.companyid, 
advancecollection.name, advancecollection.mobileno, sum(paidamount) as amount,sum(paid) paid 
from advancecollection inner join referencelist location on location.id = advancecollection.locationid
inner join users on users.id = advancecollection.createdby
left outer join employee on employeeid = employee.id
group by location.name, employee.name, paymentmode, advancecollection.companyid, advancecollection.name, advancecollection.mobileno
order by location, employeename";
        public List<CustomerorderInfo> SearchInfo(CustomerorderCreteria _customerorder)
        {
            List<CustomerorderInfo> lst = new List<CustomerorderInfo>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_customerorder.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _customerorder.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("customerorder.isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_customerorder.Code))
                    //    qb.AddParameter("code", _customerorder.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        CustomerorderInfo ocustomerorder = new CustomerorderInfo();
                        ocustomerorder.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocustomerorder.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ocustomerorder.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ocustomerorder.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ocustomerorder.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ocustomerorder.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        ocustomerorder.categoryid = reader["categoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["categoryid"]);
                        ocustomerorder.prfid = reader["prfid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prfid"]);
                        ocustomerorder.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ocustomerorder.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        ocustomerorder.modeofdelivery = reader["modeofdelivery"] == DBNull.Value ? 0 : Convert.ToInt32(reader["modeofdelivery"]);
                        ocustomerorder.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ocustomerorder.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ocustomerorder.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ocustomerorder.orderdate = reader["orderdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["orderdate"]);
                        ocustomerorder.planneddeliverydate = reader["planneddeliverydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["planneddeliverydate"]);
                        ocustomerorder.deliverydate = reader["deliverydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["deliverydate"]);
                        ocustomerorder.orderamount = reader["orderamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["orderamount"]);
                        ocustomerorder.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        ocustomerorder.customerorderno = reader["customerorderno"] == DBNull.Value ? "" : reader["customerorderno"].ToString();
                        ocustomerorder.notes = reader["notes"] == DBNull.Value ? "" : reader["notes"].ToString();
                        ocustomerorder.totalqty = reader["totalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalqty"]);

                        lst.Add(ocustomerorder);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customerorder);
            }
            return lst;
        }


        public List<Customerorder> Search(CustomerorderCreteria _customerorder)
        {
            List<Customerorder> lst = new List<Customerorder>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_customerorder.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _customerorder.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("customerorder.isactive", true, "=");

                    if (!string.IsNullOrEmpty(_customerorder.customerorderno))
                    {
                        qb.AddParameter("customerorderno", _customerorder.customerorderno, "=");
                    }
                    if (_customerorder.status >= 0)
                        qb.AddParameter("status", _customerorder.status, "=");
                    if (_customerorder.fromdate != DateTime.MinValue && _customerorder.todate != DateTime.MinValue)
                    {
                        qb.AddParameter("customerorder.orderdate", "datefrom", _customerorder.fromdate.Date, ">=");
                        qb.AddParameter("customerorder.orderdate", "dateto", _customerorder.todate.Date.AddDays(1), "<");
                    }
                    qb.SortField = "customerorder.id desc";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Customerorder ocustomerorder = new Customerorder();
                        ocustomerorder.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ocustomerorder.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ocustomerorder.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ocustomerorder.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ocustomerorder.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ocustomerorder.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        ocustomerorder.categoryid = reader["categoryid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["categoryid"]);
                        ocustomerorder.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ocustomerorder.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        ocustomerorder.modeofdelivery = reader["modeofdelivery"] == DBNull.Value ? 0 : Convert.ToInt32(reader["modeofdelivery"]);
                        ocustomerorder.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ocustomerorder.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ocustomerorder.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ocustomerorder.orderdate = reader["orderdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["orderdate"]);
                        ocustomerorder.planneddeliverydate = reader["planneddeliverydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["planneddeliverydate"]);
                        ocustomerorder.deliverydate = reader["deliverydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["deliverydate"]);
                        ocustomerorder.orderamount = reader["orderamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["orderamount"]);
                        ocustomerorder.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        ocustomerorder.customerorderno = reader["customerorderno"] == DBNull.Value ? "" : reader["customerorderno"].ToString();
                        ocustomerorder.notes = reader["notes"] == DBNull.Value ? "" : reader["notes"].ToString();
                        ocustomerorder.delieryinfo_json = reader["deliveryinfo"] == DBNull.Value ? null : reader["deliveryinfo"].ToString();
                        ocustomerorder.orderitems_json = reader["orderitems"] == DBNull.Value ? null : reader["orderitems"].ToString();
                        ocustomerorder.statushistory_json = reader["statushistory"] == DBNull.Value ? null : reader["statushistory"].ToString();
                        ocustomerorder.communication_json = reader["communication"] == DBNull.Value ? null : reader["communication"].ToString();
                        ocustomerorder.paymentinfo_json = reader["paymentinfo"] == DBNull.Value ? null : reader["paymentinfo"].ToString();
                        ocustomerorder.venderinfo_json = reader["vendorinfo"] == DBNull.Value ? null : reader["vendorinfo"].ToString();
                        ocustomerorder.voucherinfo_json = reader["voucherinfo"] == DBNull.Value ? null : reader["voucherinfo"].ToString();
                        ocustomerorder.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        ocustomerorder.address = reader["address"] == DBNull.Value ? "" : reader["address"].ToString();
                        ocustomerorder.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        ocustomerorder.contactno = reader["contactno"] == DBNull.Value ? "" : reader["contactno"].ToString();
                        ocustomerorder.totalqty = reader["totalqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalqty"]);
                        ocustomerorder.ordertakenby = reader["ordertakenby"] == DBNull.Value ? "" : reader["ordertakenby"].ToString();

                        lst.Add(ocustomerorder);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _customerorder);
            }
            return lst;
        }

        public Customerorder Insert(Customerorder oCustomerorder)
        {
            if (oCustomerorder.orderitems == null) return oCustomerorder;
            NpgsqlTransaction transaction = null;
            try
            {
                //var sequence = SequenceManager.Instance(Context.OrganizationID).Get("customerorderno", oCustomerorder.locationid);
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("customerorder.customerorderno", 0, InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ?
                    oCustomerorder.companyid : 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    oCustomerorder.customerorderno = sequence.GetNextNumber(Context,conn, transaction);

                    if (oCustomerorder.customerid == 0 && !string.IsNullOrEmpty(oCustomerorder.contactno))
                    {
                        CustomerDataService custdata = new CustomerDataService() { Context = this.Context };
                        var cust = new Models.Customer();
                        cust.name = string.IsNullOrEmpty(oCustomerorder.name) ? "Customer" : oCustomerorder.name;
                        cust.mobileno = oCustomerorder.contactno;
                        cust.address = oCustomerorder.address;
                        cust.cityid = oCustomerorder.cityid;
                        cust.isactive = true;
                        cust.version = 1;
                        cust.createdon = DateTimeNow;
                        cust.customertype = 4;
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oCustomerorder.locationid))
                            cust.categoryid = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oCustomerorder.locationid].sale_defaultcustomercategory;

                        if (custdata.DuplicateValidation(cust) > 0)
                        {
                            throw new Exception("Mobileno already exists. please check. " + oCustomerorder.contactno);
                        }
                        custdata.Insert(transaction, conn, cust);
                        oCustomerorder.customerid = cust.id;
                    }

                    oCustomerorder.orderamount = oCustomerorder.orderitems.Sum(e => e.price * e.qty);
                    oCustomerorder.totalqty = oCustomerorder.orderitems.Sum(e => e.qty);
                    if (oCustomerorder.paymentinfo != null)
                    {
                        oCustomerorder.paidamount = oCustomerorder.paymentinfo.Sum(e => e.amount);
                        oCustomerorder.paymentinfo.ForEach(e => e.receiptgenerated = true);
                    }

                    long counterid = oCustomerorder.paymentinfo != null && oCustomerorder.paymentinfo.Count > 0 ? oCustomerorder.paymentinfo[0].counterid : 0;
                    // Customer Advance voucher Issue for the amount received
                    Customerorder.Voucherinfo voucherinfo = null;
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_custadvnvoucher)
                    {
                        GiftCouponDataService CouponData = new GiftCouponDataService() { Context = this.Context };
                        

                        GiftVoucher gtvoucher = new GiftVoucher()
                        {
                            isactive = true,
                            createdby = Context.UserID,
                            createdon = DateTimeNow,
                            version = 1,
                            validfrom = DateTimeNow,
                            validto = DateTimeNow.AddDays(180),
                            customerid = oCustomerorder.customerid,
                            counterid = counterid,
                            issuemode = 5,
                            issuedon = oCustomerorder.orderdate,
                            locationid = oCustomerorder.locationid,
                            name = "CUSTOMER ADVANCE - " + oCustomerorder.customerorderno,
                            mobileno = oCustomerorder.customermobileno,
                            receivedamt = oCustomerorder.paidamount,
                            vouchervalue = oCustomerorder.paidamount,
                            issuedby = Context.EmployeeID,
                            denomination = new List<GiftVoucher.Denomination>()
                        };
                        CouponData.Insert(transaction, conn, gtvoucher);

                        CustomerCouponsDataService voucherservice = new CustomerCouponsDataService() { Context = this.Context };
                        voucherservice.Context = this.Context;
                        gtvoucher.vouchercode = DateTimeNow.Year.ToString().Substring(2, 2) + WebAPI.Data.Token.RandomString(14, "0123456789");
                        voucherservice.Insert(transaction, conn, new CustomerCoupons()
                        {
                            code = gtvoucher.vouchercode,
                            issuedby = gtvoucher.issuedby,
                            issueto = gtvoucher.receivedby,
                            amount = gtvoucher.vouchervalue,
                            isactive = true,
                            issuedon = DateTimeNow,
                            coupenid = gtvoucher.id,
                            validupto = gtvoucher.validto,
                            vouchertype = CustomerCoupons.CouponType.CustomerAdvance
                        });
                        voucherinfo = new Customerorder.Voucherinfo()
                        {
                            code = gtvoucher.vouchercode,
                            amount = gtvoucher.vouchervalue,
                            validupto = gtvoucher.validto,
                            couponid = gtvoucher.id
                        };
                        oCustomerorder.voucherinfo = new List<CustomerorderInfo.Voucherinfo>();
                        oCustomerorder.voucherinfo.Add(voucherinfo);
                    }

                    Insert(transaction, conn, oCustomerorder);

                    // Customer Invoice & Customer Payment will be created for this Customer Order ( type = 3 = CustomerOrder = 3) for payment tracking
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_custadvnpaymenttracking)
                    {
                        CustomerinvoiceDataService customerinvoiceDataService = new CustomerinvoiceDataService() { Context = this.Context };
                        var customerinvoice = new Customerinvoice()
                        {
                            advance = oCustomerorder.paidamount,
                            approvedby = Context.UserID,
                            companyid = oCustomerorder.companyid,
                            customerid = oCustomerorder.customerid,
                            transactiondate = oCustomerorder.orderdate,
                            type = 3,
                            billno = "CUSTOMER ORDER - " + oCustomerorder.customerorderno,
                            orderno = oCustomerorder.customerorderno,
                            orderid = oCustomerorder.id,
                            createdby = Context.UserID,
                            createdon = DateTimeNow,
                            modifiedby = Context.UserID,
                            modifiedon = DateTimeNow,
                            isactive = true,
                            paid = oCustomerorder.paidamount,
                            receivable = oCustomerorder.orderamount
                        };
                        customerinvoiceDataService.Insert(transaction, conn, customerinvoice);

                        if (oCustomerorder.paymentinfo != null && oCustomerorder.paymentinfo.Count > 0)
                        {
                            foreach (var pitem in oCustomerorder.paymentinfo)
                            {
                                CustomerpaymentDataService customerpaymentDataService = new CustomerpaymentDataService() { Context = this.Context };
                                Models.Customerpayment customerpayment = new Customerpayment();
                                customerpayment.customerid = oCustomerorder.customerid;
                                customerpayment.counterid = pitem.counterid;
                                customerpayment.companyid = oCustomerorder.companyid;
                                customerpayment.amount = pitem.amount;
                                customerpayment.orderid = oCustomerorder.id;
                                customerpayment.orderno = oCustomerorder.customerorderno;
                                customerpayment.isactive = true;
                                customerpayment.locationid = oCustomerorder.locationid;
                                customerpayment.paidon = pitem.actionon;
                                customerpayment.receivedby = pitem.receivedby;
                                customerpayment.ledgerid = pitem.ledgerid;
                                customerpayment.paymentinfo = new Customerpayment.Paymentinfo();
                                customerpayment.paymentinfo.paymentmode = pitem.mode;
                                switch (pitem.mode)
                                {
                                    case 0:
                                        customerpayment.paymentinfo.card_paid = pitem.amount;
                                        break;
                                    case 1:
                                        customerpayment.paymentinfo.voucher_paid = pitem.amount;
                                        break;
                                    case 2:
                                        customerpayment.paymentinfo.cash_paid = pitem.amount;
                                        break;
                                    case 3:
                                        customerpayment.paymentinfo.upi_paid = pitem.amount;
                                        break;
                                }
                                customerpayment.receivedby = Context.UserID;
                                var receiptsequence = SequenceManager.Instance(Context.OrganizationID).Get("customerpayment.receipt", 0);
                                customerpayment.receiptno = receiptsequence.GetNextNumber(Context, conn, transaction);
                                customerpaymentDataService.Insert(transaction, conn, customerpayment);
                                pitem.paymentid = customerpayment.id;
                                var customerinvoicepaid = new Customerinvoice()
                                {
                                    approvedby = Context.UserID,
                                    companyid = oCustomerorder.companyid,
                                    customerid = oCustomerorder.customerid,
                                    transactiondate = pitem.actionon,
                                    receivable = oCustomerorder.orderamount,
                                    type = 4,
                                    orderno = oCustomerorder.customerorderno,
                                    orderid = oCustomerorder.id,
                                    createdby = Context.UserID,
                                    createdon = DateTimeNow,
                                    modifiedby = Context.UserID,
                                    modifiedon = DateTimeNow,
                                    isactive = true,
                                    paid = pitem.amount,
                                    paymentid = customerpayment.id,
                                    receiptno = customerpayment.receiptno,
                                    billno = "CUSTOMER ORDER - " + oCustomerorder.customerorderno
                                };
                                customerinvoiceDataService.Insert(transaction, conn, customerinvoicepaid);
                            }
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerorder);
            }

            return oCustomerorder;
        }

        public Customerorder Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerorder oCustomerorder)
        {
            int count = 1;
            oCustomerorder.paymentinfo.ForEach(e =>
            {
                if (e.receivedby == 0)
                {
                    e.receivedby = Context.UserID;
                    e.actionon = DateTimeNow;
                    e.receiptgenerated = true;
                }
                if (e.createdby == 0)
                {
                    e.createdby = Context.UserID;
                    e.employeename = Context.EmployeeName;
                }
                e.id = count++;
            });

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerorder.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerorder.locationid;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerorder.customerid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerorder.version;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerorder.status;
            command.Parameters.Add("modeofdelivery", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerorder.modeofdelivery;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerorder.isactive;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("orderdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerorder.orderdate;
            command.Parameters.Add("planneddeliverydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerorder.planneddeliverydate;
            command.Parameters.Add("deliverydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerorder.deliverydate;
            command.Parameters.Add("orderamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerorder.orderamount;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerorder.paidamount;
            
            command.Parameters.Add("cashpayment", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerorder.paidamount>0 ? oCustomerorder.paymentinfo.Sum(x=>x.paymentmode == "3" ? x.amount : 0 ):0 ;
            command.Parameters.Add("bankpayment", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerorder.paidamount > 0 ? oCustomerorder.paymentinfo.Sum(x => x.paymentmode != "3" ? x.amount : 0) : 0;

            command.Parameters.Add("customerorderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerorder.customerorderno == null ? "" : oCustomerorder.customerorderno;
            command.Parameters.Add("notes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerorder.notes == null ? "" : oCustomerorder.notes;
            command.Parameters.Add("deliveryinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerorder.delieryinfo_json == null ? "null" : oCustomerorder.delieryinfo_json;
            command.Parameters.Add("orderitems", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerorder.orderitems_json == null ? "null" : oCustomerorder.orderitems_json;
            command.Parameters.Add("statushistory", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerorder.statushistory_json == null ? "null" : oCustomerorder.statushistory_json;
            command.Parameters.Add("communication", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerorder.communication_json == null ? "null" : oCustomerorder.communication_json;
            command.Parameters.Add("paymentinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerorder.paymentinfo_json == null ? "null" : oCustomerorder.paymentinfo_json;
            command.Parameters.Add("vendorinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerorder.venderinfo_json == null ? "null" : oCustomerorder.venderinfo_json;
            command.Parameters.Add("voucherinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerorder.voucherinfo_json == null ? "null" : oCustomerorder.voucherinfo_json;
            command.Parameters.Add("totalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerorder.totalqty;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomerorder.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oCustomerorder;
        }

        public Customerorder Update(Customerorder oCustomerorder)
        {
            if (oCustomerorder.orderitems == null) return oCustomerorder;
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    CustomerpaymentDataService customerpaymentDataService = new CustomerpaymentDataService() { Context = this.Context };
                    List<Models.Customerpayment> customerpayments = customerpaymentDataService.Search(new CustomerpaymentCreteria()
                    {
                         customerid = oCustomerorder.customerid,
                         orderid = oCustomerorder.id
                    });
                    if(customerpayments.Count>0)
                    {
                        var removedpayments = customerpayments.FindAll(e => oCustomerorder.paymentinfo == null || !oCustomerorder.paymentinfo.Exists(x => x.paymentid == e.id));
                        foreach(var removepayment in customerpayments)
                        {
                            customerpaymentDataService.Delete(transaction, conn, removepayment);
                        }
                    }

                    if (oCustomerorder.orderitems != null)
                    {
                        oCustomerorder.orderamount = oCustomerorder.orderitems.Sum(e => e.price * e.qty);
                        oCustomerorder.totalqty = oCustomerorder.orderitems.Sum(e => e.qty);
                    }
                    if (oCustomerorder.paymentinfo != null)
                    {
                        oCustomerorder.paidamount = oCustomerorder.paymentinfo.Sum(e => e.amount);
                    }

                    CustomerinvoiceDataService customerinvoiceDataService = new CustomerinvoiceDataService() { Context = this.Context };
                    var customerinvoice = customerinvoiceDataService.Search(new CustomerinvoiceCreteria()
                    {
                        orderid = oCustomerorder.id
                    }).Find(e => e.type == 3);

                    if(customerinvoice != null && (customerinvoice.advance != oCustomerorder.paidamount ||
                        customerinvoice.paid != oCustomerorder.paidamount ||
                        customerinvoice.receivable != oCustomerorder.orderamount ||
                        customerinvoice.transactiondate != oCustomerorder.orderdate))
                    {
                        customerinvoice.transactiondate = oCustomerorder.orderdate;
                        customerinvoice.advance = oCustomerorder.paidamount;
                        customerinvoice.paid = oCustomerorder.paidamount;
                        customerinvoice.receivable = oCustomerorder.orderamount;
                        customerinvoiceDataService.Update(transaction, conn, customerinvoice);
                    }

                    if (oCustomerorder.paymentinfo != null && oCustomerorder.paymentinfo.Count > 0)
                    {
                        foreach (var pitem in oCustomerorder.paymentinfo.FindAll(e=>e.paymentid==0))
                        {
                            Models.Customerpayment customerpayment = new Customerpayment();
                            customerpayment.customerid = oCustomerorder.customerid;
                            customerpayment.counterid = pitem.counterid;
                            customerpayment.ledgerid = pitem.ledgerid;
                            customerpayment.companyid = oCustomerorder.companyid;
                            customerpayment.amount = pitem.amount;
                            customerpayment.orderid = oCustomerorder.id;
                            customerpayment.orderno = oCustomerorder.customerorderno;
                            customerpayment.isactive = true;
                            customerpayment.locationid = oCustomerorder.locationid;
                            customerpayment.paidon = pitem.actionon;
                            customerpayment.receivedby = pitem.receivedby;
                            customerpayment.sourcetype = 1;
                            customerpayment.paymentinfo = new Customerpayment.Paymentinfo();
                            customerpayment.paymentinfo.paymentmode = pitem.customerpaymentmode;
                            
                            switch (pitem.mode)
                            {
                                case 1:
                                    customerpayment.paymentinfo.card_paid = pitem.amount;
                                    customerpayment.paymentinfo.bankid = pitem.bankid;
                                    break;
                                case 3:
                                    customerpayment.paymentinfo.cash_paid = pitem.amount;
                                    break;
                                case 2:
                                case 4:
                                case 5:
                                    customerpayment.paymentinfo.upi_paid = pitem.amount;
                                    customerpayment.paymentinfo.bankid = pitem.bankid;
                                    break;
                            }

                            /*
        $scope.PaymentModes = [{ 'id': 1, 'name': 'Inter Bank Transfer' },
        { 'id': 2, 'name': 'Cheque/DD' },
        { 'id': 3, 'name': 'Cash' },
        { 'id': 4, 'name': 'Card' },
            { 'id': 5, 'name': 'UPI' }];*/

                            customerpayment.receivedby = Context.UserID;
                            var receiptsequence = SequenceManager.Instance(Context.OrganizationID).Get("customerpayment.receipt", 0);
                            customerpayment.receiptno = receiptsequence.GetNextNumber(Context,conn, transaction);
                            customerpaymentDataService.Insert(transaction, conn, customerpayment);
                            pitem.paymentid = customerpayment.id;
                            var customerinvoicepaid = new Customerinvoice()
                            {
                                approvedby = Context.UserID,
                                companyid = oCustomerorder.companyid,
                                customerid = oCustomerorder.customerid,
                                transactiondate =pitem.actionon,
                                type = 3,
                                orderno = oCustomerorder.customerorderno,
                                orderid = oCustomerorder.id,
                                createdby = Context.UserID,
                                createdon = DateTimeNow,
                                modifiedby = Context.UserID,
                                modifiedon = DateTimeNow,
                                isactive = true,
                                paid = pitem.amount,
                                paymentid = customerpayment.id
                            };
                            customerinvoiceDataService.Insert(transaction, conn, customerinvoicepaid);
                        }
                    }

                    if(customerinvoice != null && (customerinvoice.companyid != oCustomerorder.companyid ||
                       customerinvoice.customerid != oCustomerorder.customerid) )
                    {
                        string updatecustomercompany = @"update customerpayment set companyid=@companyid,customerid=@customerid where orderid=@orderid;
                                                         update customerinvoice set companyid=@companyid,customerid=@customerid where orderid=@orderid";
                        var command = GetCommand(updatecustomercompany, conn, transaction);
                        command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerorder.customerid;
                        command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerorder.companyid;
                        command.Parameters.Add("orderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerorder.id;
                        command.ExecuteNonQuery();
                    }

                    long counterid = oCustomerorder.paymentinfo != null && oCustomerorder.paymentinfo.Count > 0 ? oCustomerorder.paymentinfo[0].counterid : 0;
                    
                    // Customer Advance voucher Issue for the amount received
                    Customerorder.Voucherinfo voucherinfo = null;
                    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_custadvnvoucher)
                    {
                        List<Customerorder> existing = Search(new CustomerorderCreteria() { id = oCustomerorder.id });
                        if (existing != null && existing.Count > 0 
                            && existing[0].voucherinfo != null && existing[0].voucherinfo.Count > 0)
                        {
                            GiftCouponDataService CouponData = new GiftCouponDataService() { Context = this.Context };
                            List<GiftVoucher> existingcoupons = CouponData.SearchVoucher(new GiftVoucherCreteria() { id = existing[0].voucherinfo[0].couponid });

                            CustomerCouponsDataService voucherData = new CustomerCouponsDataService() { Context = this.Context };
                            List<CustomerCoupons> lstvouchers = voucherData.Search(new CustomerCouponsCreteria() { code = existing[0].voucherinfo[0].code });

                            if (lstvouchers.Exists(e => e.vouchertype == CustomerCoupons.CouponType.GiftClaim))
                                throw new Exception("Customer Advance Coupon Can't be Regenerated, Its already consumed.");

                            if (existingcoupons != null && existingcoupons.Count > 0 && 
                                existingcoupons[0].vouchervalue != oCustomerorder.paidamount)
                            {
                                string updatecoupons = @"update coupons set vouchervalue=@vouchervalue, receivedamt=@receivedamt, validto=@validto, version=version+1, modifiedon=@modifiedon, modifiedby=@modifiedby where id=@couponid;
                                                         update vouchers set amount=@receivedamt,validupto=@validto, version=version+1, modifiedon=@modifiedon, modifiedby=@modifiedby where id=@voucherid and vouchertype=8";
                                var command = GetCommand(updatecoupons, conn, transaction);
                                command.Parameters.Add("vouchervalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerorder.paidamount;
                                command.Parameters.Add("receivedamt", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerorder.paidamount;
                                command.Parameters.Add("validto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow.AddDays(180);
                                command.Parameters.Add("couponid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = existingcoupons[0].id;
                                command.Parameters.Add("voucherid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = lstvouchers[0].id;
                                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                                command.ExecuteNonQuery();

                                voucherinfo = new Customerorder.Voucherinfo()
                                {
                                    code = lstvouchers[0].code,
                                    amount = lstvouchers[0].amount,
                                    validupto = DateTimeNow.AddDays(180),
                                    couponid = existingcoupons[0].id
                                };
                                oCustomerorder.voucherinfo = new List<CustomerorderInfo.Voucherinfo>();
                                oCustomerorder.voucherinfo.Add(voucherinfo);
                            }
                        }
                        else
                        {
                            GiftCouponDataService CouponData = new GiftCouponDataService() { Context = this.Context };

                            GiftVoucher gtvoucher = new GiftVoucher()
                            {
                                isactive = true,
                                createdby = Context.UserID,
                                createdon = DateTimeNow,
                                version = 1,
                                validfrom = DateTimeNow,
                                validto = DateTimeNow.AddDays(180),
                                customerid = oCustomerorder.customerid,
                                counterid = counterid,
                                name = "CUSTOMER ADVANCE - " + oCustomerorder.customerorderno,
                                mobileno = oCustomerorder.customermobileno,
                                receivedamt = oCustomerorder.paidamount,
                                vouchervalue = oCustomerorder.paidamount,
                                issuedby = Context.EmployeeID,
                                locationid = oCustomerorder.locationid, 
                                denomination = new List<GiftVoucher.Denomination>()
                            };
                            CouponData.Insert(transaction, conn, gtvoucher);

                            CustomerCouponsDataService voucherservice = new CustomerCouponsDataService() { Context = this.Context };
                            voucherservice.Context = this.Context;
                            gtvoucher.vouchercode = DateTimeNow.Year.ToString().Substring(2, 2) + WebAPI.Data.Token.RandomString(14, "0123456789");
                            voucherservice.Insert(transaction, conn, new CustomerCoupons()
                            {
                                code = gtvoucher.vouchercode,
                                issuedby = gtvoucher.issuedby,
                                issueto = gtvoucher.receivedby,
                                amount = gtvoucher.vouchervalue,
                                isactive = true,
                                issuedon = DateTimeNow,
                                coupenid = gtvoucher.id,
                                validupto = gtvoucher.validto,
                                vouchertype = CustomerCoupons.CouponType.CustomerAdvance
                            });
                            voucherinfo = new Customerorder.Voucherinfo()
                            {
                                code = gtvoucher.vouchercode,
                                amount = gtvoucher.vouchervalue,
                                validupto = gtvoucher.validto,
                                couponid = gtvoucher.id
                            };
                            oCustomerorder.voucherinfo = new List<CustomerorderInfo.Voucherinfo>();
                            oCustomerorder.voucherinfo.Add(voucherinfo);
                        }
                    }

                    Update(transaction, conn, oCustomerorder);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerorder);
            }
            return oCustomerorder;
        }

        public Customerorder Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerorder oCustomerorder)
        {
            int count = 1;
            oCustomerorder.paymentinfo.ForEach(e =>
            {
                if (e.receivedby == 0)
                {
                    e.receivedby = Context.UserID;
                    e.actionon = DateTimeNow;
                    e.receiptgenerated = true;
                }
                if (e.createdby == 0)
                {
                    e.createdby = Context.UserID;
                    e.employeename = Context.EmployeeName;
                }
                e.id = count++;
            });
            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerorder.id;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerorder.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerorder.locationid;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerorder.customerid;
            command.Parameters.Add("prfid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerorder.prfid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerorder.version;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerorder.status;
            command.Parameters.Add("modeofdelivery", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerorder.modeofdelivery;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerorder.isactive;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("orderdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerorder.orderdate;
            command.Parameters.Add("planneddeliverydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerorder.planneddeliverydate;
            command.Parameters.Add("deliverydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerorder.deliverydate;
            command.Parameters.Add("orderamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerorder.orderamount;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerorder.paidamount;
            command.Parameters.Add("cashpayment", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerorder.paidamount > 0 ? oCustomerorder.paymentinfo.Sum(x => x.paymentmode == "3" ? x.amount : 0) : 0;
            command.Parameters.Add("bankpayment", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerorder.paidamount > 0 ? oCustomerorder.paymentinfo.Sum(x => x.paymentmode != "3" ? x.amount : 0) : 0;
            command.Parameters.Add("customerorderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerorder.customerorderno == null ? "" : oCustomerorder.customerorderno;
            command.Parameters.Add("notes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oCustomerorder.notes == null ? "" : oCustomerorder.notes;
            command.Parameters.Add("deliveryinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerorder.delieryinfo_json == null ? "null" : oCustomerorder.delieryinfo_json;
            command.Parameters.Add("orderitems", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerorder.orderitems_json == null ? "null" : oCustomerorder.orderitems_json;
            command.Parameters.Add("statushistory", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerorder.statushistory_json == null ? "null" : oCustomerorder.statushistory_json;
            command.Parameters.Add("communication", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerorder.communication_json == null ? "null" : oCustomerorder.communication_json;
            command.Parameters.Add("paymentinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerorder.paymentinfo_json == null ? "null" : oCustomerorder.paymentinfo_json;
            command.Parameters.Add("vendorinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerorder.venderinfo_json == null ? "null" : oCustomerorder.venderinfo_json;
            command.Parameters.Add("voucherinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oCustomerorder.voucherinfo_json == null ? "null" : oCustomerorder.voucherinfo_json;
            command.Parameters.Add("totalqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oCustomerorder.totalqty;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oCustomerorder.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oCustomerorder;
        }

        public Customerorder UpdateStatus(Customerorder oCustomerorder)
        {
            const string SQL_UpdateStatus = @"update public.customerorder set isactive=true,status=@status,version=@version+1,
                    modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version and isactive=true
                    RETURNING version";

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    NpgsqlCommand command = GetCommand(SQL_UpdateStatus, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerorder.isactive;
                    command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerorder.status;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerorder.version;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerorder.id;
                    int successCount = command.ExecuteNonQuery();

                    if (oCustomerorder.status == 3)
                        CheckAdvanceVoucher(transaction, conn, oCustomerorder);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerorder);

            }
            return oCustomerorder;
        }
        public bool CheckAdvanceVoucher(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerorder oCustomerorder)
        {
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_custadvnvoucher)
            {
                List<Customerorder> existing = Search(new CustomerorderCreteria() { id = oCustomerorder.id });
                if (existing != null && existing.Count > 0
                    && existing[0].voucherinfo != null && existing[0].voucherinfo.Count > 0)
                {
                    GiftCouponDataService CouponData = new GiftCouponDataService();
                    CouponData.Context = this.Context;
                    List<GiftVoucher> existingcoupons = CouponData.SearchVoucher(new GiftVoucherCreteria() { id = existing[0].voucherinfo[0].couponid });

                    CustomerCouponsDataService voucherData = new CustomerCouponsDataService() { Context = this.Context };
                    List<CustomerCoupons> lstvouchers = voucherData.Search(new CustomerCouponsCreteria() { code = existing[0].voucherinfo[0].code });

                    if (lstvouchers.Exists(e => e.vouchertype == CustomerCoupons.CouponType.GiftClaim))
                        throw new Exception("Customer Advance Coupon already consumed. Can't Delete.");

                    if (existingcoupons != null && existingcoupons.Count > 0 && lstvouchers.Count > 0)
                    {
                        string updatecoupons = @"update coupons set isactive=@isactive, version=version+1, modifiedon=@modifiedon, modifiedby=@modifiedby where id=@couponid;
                                                         update vouchers set isactive=@isactive, version=version+1, modifiedon=@modifiedon, modifiedby=@modifiedby where id=@voucherid and vouchertype=8";
                        var command = GetCommand(updatecoupons, conn, transaction);
                        command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
                        command.Parameters.Add("couponid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = existingcoupons[0].id;
                        command.Parameters.Add("voucherid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = lstvouchers[0].id;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.ExecuteNonQuery();
                    }
                }
            }
            return true;
        }
        public bool Delete(Customerorder oCustomerorder)
        {
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oCustomerorder);
                    hasSuccessCount = CheckAdvanceVoucher(transaction, conn, oCustomerorder);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oCustomerorder);

            }
            return hasSuccessCount;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Customerorder oCustomerorder)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oCustomerorder.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerorder.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oCustomerorder.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oCustomerorder.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oCustomerorder.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public List<SettlementReport> SearchAdvanceCollection(RetailReportCreteria _reportcreteria)
        {
            List<SettlementReport> lst = new List<SettlementReport>();
            DateTime today = _reportcreteria.fromdate.Date;
            DateTime nextdate = _reportcreteria.todate.AddDays(1).Date;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                command = GetCommand(SQL_AdvanceCollection, conn);
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.locationid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.companyid;
                command.Parameters.Add("cashierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _reportcreteria.employeeid;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        SettlementReport oitem = new SettlementReport();
                        oitem.mode = reader["paymentmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["paymentmode"]);
                        oitem.paid_cash = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        oitem.location = reader["location"] == DBNull.Value ? "" : reader["location"].ToString();
                        oitem.employeename = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();
                        oitem.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oitem.customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oitem.operatorname = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                        lst.Add(oitem);
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

    }

}
