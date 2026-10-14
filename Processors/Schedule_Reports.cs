using InfyPOS.Common;
using InfyPOS.Data;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfyPOS.Processors
{
    public class Salesummary : ScheduleManager.IReportExecutor
    {
        public override ReportSource Run(long organizationid, int txoffset)
        {
            Common.BaseData baseData = new Common.BaseData()
            {
                Context = new Common.UserClaim()
                { OrganizationID = organizationid, TZ = txoffset }
            };
            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = "Select sum(receivable) receivable,sum(discount) discount,sum(additionaldiscount) additionaldiscount,sum(qty) billqty,sum(tax) tax, count(*) billcount from bill where isactive = true and billtype != 9";

            switch (Source.config.mode)
            {
                case 2:
                    qb.CurrentQuery = @"Select products.name product,sum(billitems.receivable) receivable,
                                        sum(billitems.discount) discount,sum(billitems.additionaldiscount) additionaldiscount,
                                        sum(billitems.qty) billqty,sum(stock.buyingprice*billitems.qty) buyingprice,
                                        sum(stock.ibuyingprice*billitems.qty) ibuyingprice,sum(billitems.tax) tax,count(*) billcount from bill 
                                        inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                                        inner join salablegoods on salablegoods.id = salablegoodsid
                                        inner join stock on stock.id = salablegoods.stockid
                                        inner join products on products.id = stock.productid
                                        where bill.isactive = true and bill.billtype != 9 ";
                    break;
                case 1:
                    qb.CurrentQuery = @"Select referencelist.name section,sum(billitems.receivable) receivable,
                                        sum(billitems.discount) discount,sum(billitems.additionaldiscount) additionaldiscount,
                                        sum(billitems.qty) billqty,sum(stock.buyingprice*billitems.qty) buyingprice,
                                        sum(stock.ibuyingprice*billitems.qty) ibuyingprice,sum(billitems.tax) tax,count(*) billcount from bill 
                                        inner join billitems on billitems.billid = bill.id and billitems.isactive=true
                                        inner join salablegoods on salablegoods.id = salablegoodsid
                                        inner join stock on stock.id = salablegoods.stockid
                                        inner join products on products.id = stock.productid
                                        inner join referencelist on referencelist.id = products.salescounterid
                                        where bill.isactive = true and bill.billtype != 9 ";
                    break;
            }

            DateTime fromdate = baseData.DateTimeNow.Date;
            DateTime todate = fromdate.AddDays(1);
            switch (Source.config.parameter)
            {
                case "Current Month":
                    fromdate = new DateTime(fromdate.Year, fromdate.Month, 1);
                    todate = fromdate.AddMonths(1);
                    break;
                case "Current Year":
                    fromdate = new DateTime(fromdate.Year, 1, 1);
                    todate = fromdate.AddYears(1);
                    break;
                case "Current Week":
                    fromdate = fromdate.AddDays(((int)fromdate.DayOfWeek) * -1);
                    todate = fromdate.AddDays(8);
                    break;
            }
            qb.AddParameter("billdate", "fromdate", fromdate, ">=");
            qb.AddParameter("billdate", "todate", todate, "<");
            if (Source.config.companyid > 0)
            {
                qb.AddParameter("bill.companyid", "companyid", Source.config.companyid, "=");
            }
            if (Source.config.locationid > 0)
                qb.AddParameter("bill.locationid", "locationid", Source.config.locationid, "=");

            switch (Source.config.mode)
            {
                case 2:
                    qb.CurrentQuery += @" group by products.name";
                    break;
                case 1:
                    qb.CurrentQuery += @" group by referencelist.name";
                    break;
            }

            var result = new ReportSource();
            Root root = new Root()
            {
                fromdate = fromdate,
                todate = todate,
                title = Source.description
            };
            List<Root> reporitemitems = new List<Root>();
            
            result.itemList = reporitemitems;
            using (NpgsqlConnection conn = baseData.CreateConnection())
            {
                try
                {
                    conn.Open();

                    NpgsqlCommand command = qb.GetCommand(conn);
                    using (var reader = command.ExecuteReader(System.Data.CommandBehavior.CloseConnection))
                    {
                        while (reader.Read())
                        {
                            root = new Root()
                            {
                                fromdate = fromdate,
                                todate = todate,
                                title = Source.description
                            };
                            root.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                            root.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                            root.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                            root.additionaldiscount = reader["additionaldiscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionaldiscount"]);
                            root.billcount = reader["billcount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billcount"]);
                            root.billqty = reader["billqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billqty"]);
                            if(Source.config.mode == 2)
                            {
                                root.product = reader["product"] == DBNull.Value ? "" : Convert.ToString(reader["product"]);
                                root.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                                root.ibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"]);
                            }
                            else if(Source.config.mode == 1)
                            {
                                root.section = reader["section"] == DBNull.Value ? "" : Convert.ToString(reader["section"]);
                                root.buyingprice = reader["buyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["buyingprice"]);
                                root.ibuyingprice = reader["ibuyingprice"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ibuyingprice"]);
                            }
                            reporitemitems.Add(root);
                        }
                    }

                    conn.Close();
                }
                catch (Exception exp)
                {

                }
            }
            result.RootObject = root;
            if (Source.config.companyid > 0)
            {
                using (var reader = baseData.ExecuteReader("Select name from company where id=" + Source.config.companyid, null))
                {
                    if (reader.Read())
                    {
                        root.company = reader.GetString(0);
                        if (reporitemitems.Count > 0)
                        {
                            reporitemitems.ForEach(x => x.company = root.company);
                        }
                    }
                }
            }
            if (Source.config.locationid > 0)
            {
                using (var reader = baseData.ExecuteReader("Select name from referencelist where id=" + Source.config.locationid, null))
                {
                    if (reader.Read())
                    {
                        root.location = reader.GetString(0);
                        if (reporitemitems.Count > 0)
                        {
                            reporitemitems.ForEach(x => x.location = root.location);
                        }
                    }
                }
            }
            return result;
        }

        public class Root : CommonRoot
        {
            public Root() : base()
            {

            }
            public decimal receivable { get; set; }
            public decimal additionaldiscount { get; set; }
            public decimal discount { get; set; }
            public decimal billcount { get; set; }
            public decimal billqty { get; set; }
            public decimal buyingprice { get; set; }
            public decimal ibuyingprice { get; set; }
            public string section { get; set; }
            public string product { get; set; }
            public decimal tax { get; set; }
            public decimal cost { get; set; }
            public decimal gross
            {
                get
                {
                    return receivable - tax;
                }
            }
        }
    }

    public class Dayclose : ScheduleManager.IReportExecutor
    {
        public override ReportSource Run(long organizationid, int tzoffset)
        {
            Common.BaseData baseData = new Common.BaseData()
            {
                Context = new Common.UserClaim()
                {
                    OrganizationID = organizationid,
                    TZ = tzoffset
                }
            };
            DateTime fromdate = baseData.DateTimeNow.Date;
            DateTime todate = fromdate.AddDays(1);

            var result = new ReportSource();
            CommonRoot root = new CommonRoot()
            {
                fromdate = fromdate,
                todate = todate,
                title = Source.description
            };
            if (Source.config.companyid > 0)
            {
                using (var reader = baseData.ExecuteReader("Select name from company where id=" + Source.config.companyid, null))
                {
                    if (reader.Read())
                        root.company = reader.GetString(0);
                }
            }
            if (Source.config.locationid > 0)
            {
                using (var reader = baseData.ExecuteReader("Select name from referencelist where id=" + Source.config.locationid, null))
                {
                    if (reader.Read())
                        root.location = reader.GetString(0);
                }
            }
            result.RootObject = root;

            RetailReportDataService RetailReportData = new RetailReportDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            result.itemList = RetailReportData.SearchDaySummary(new SaleRetailCreteria()
            {
                companyid = Source.config.parameter != "All Location" ? Source.config.companyid : 0,
                locationid = Source.config.parameter != "All Location" ? Source.config.locationid : 0,
                fromdate = fromdate,
                todate = todate.AddDays(-1)
            }).Items;
            return result;
        }


    }

    public class Daysummary : ScheduleManager.IReportExecutor
    {
        public override ReportSource Run(long organizationid, int tzoffset)
        {

            Common.BaseData baseData = new Common.BaseData()
            {
                Context = new Common.UserClaim()
                {
                    OrganizationID = organizationid,
                    TZ = tzoffset
                }
            };
            DateTime fromdate = baseData.DateTimeNow.Date;
            DateTime todate = fromdate.AddDays(1);
            switch (Source.config.parameter)
            {
                case "Current Month":
                    fromdate = new DateTime(fromdate.Year, fromdate.Month, 1);
                    todate = fromdate.AddMonths(1);
                    break;
                case "Current Year":
                    fromdate = new DateTime(fromdate.Year, 1, 1);
                    todate = fromdate.AddYears(1);
                    break;
                case "Current Week":
                    fromdate = fromdate.AddDays(((int)fromdate.DayOfWeek) * -1);
                    todate = fromdate.AddDays(8);
                    break;
            }

            var result = new ReportSource();
            CommonRoot root = new CommonRoot()
            {
                fromdate = fromdate,
                todate = todate,
                title = Source.description
            };
            if (Source.config.companyid > 0)
            {
                using (var reader = baseData.ExecuteReader("Select name from company where id=" + Source.config.companyid, null))
                {
                    if (reader.Read())
                        root.company = reader.GetString(0);
                }
            }
            if (Source.config.locationid > 0)
            {
                using (var reader = baseData.ExecuteReader("Select name from referencelist where id=" + Source.config.locationid, null))
                {
                    if (reader.Read())
                        root.location = reader.GetString(0);
                }
            }
            result.RootObject = root;

            RetailReportDataService RetailReportData = new RetailReportDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            result.itemList = RetailReportData.SearchDaySummary(new SaleRetailCreteria()
            {
                companyid = Source.config.companyid,
                locationid = Source.config.locationid,
                fromdate = fromdate,
                todate = todate.AddDays(-1)
            }).Items;
            return result;
        }


    }
    public class OnDayOccure : ScheduleManager.IReportExecutor
    {

        public override ReportSource Run(long organizationid, int tzoffset)
        {
            Common.BaseData baseData = new Common.BaseData()
            {
                Context = new Common.UserClaim()
                {
                    OrganizationID = organizationid,
                    TZ = tzoffset
                }
            };
            var result = new ReportSource();
            Root root = new Root();
            DateTime fromdate = baseData.DateTimeNow.Date;
            DateTime todate = fromdate.AddDays(1);

            QueryBuilder qb = null;
            switch (Source.config.parameter)
            {
                case "Employee DOB":
                    qb = new QueryBuilder();
                    qb.CurrentQuery = @"select id from employee where contactno is not null and length(contactno)>=10 
                            and dateofbirth>'1904-01-01'
                            and ('1904-'||to_char(dateofbirth,'MM-dd'))::date >= ('1904-'||to_char(@fromdate::date,'MM-dd'))::date
                            and ('1904-'||to_char(dateofbirth,'MM-dd'))::date < ('1904-'||to_char(@todate::date,'MM-dd'))::date";
                    break;
                case "Employee Anniversary":
                    qb = new QueryBuilder();
                    qb.CurrentQuery = "select id from employee where contactno is not null and length(contactno)>=10 and cast(Personal->>'marrieddate' as timestamp without time zone)>=@fromdate and cast(Personal->>'marrieddate' as timestamp without time zone)<@todate";
                    break;
                case "Employee Festival Holiday":
                    qb = new QueryBuilder();
                    qb.CurrentQuery = "select id,name from holiday where holidaytype = 'Festival' and isactive = true and holidayon>=@fromdate and holidayon<@todate";
                    break;
                case "Customer DOB":
                    qb = new QueryBuilder();
                    if (Source.config.customerconfig != null &&
                        Source.config.customerconfig.beforedays > 0)
                    {
                        fromdate = fromdate.AddDays(Source.config.customerconfig.beforedays);
                        todate = fromdate.AddDays(1);
                    }

                    qb.CurrentQuery = @"select id from customer where isactive = true and mobileno is not null and length(mobileno)>=10
                                        and dateofbirth>'1904-01-01'
                                        and ('1904-'||to_char(dateofbirth,'MM-dd'))::date >= ('1904-'||to_char(@fromdate::date,'MM-dd'))::date
                                        and ('1904-'||to_char(dateofbirth,'MM-dd'))::date < ('1904-'||to_char(@todate::date,'MM-dd'))::date ";
                    break;
                case "Customer Kids DOB":
                    qb = new QueryBuilder();
                    if (Source.config.customerconfig != null &&
                        Source.config.customerconfig.beforedays > 0)
                    {
                        fromdate = fromdate.AddDays(Source.config.customerconfig.beforedays);
                        todate = fromdate.AddDays(1);
                    }

                    qb.CurrentQuery = @"select id,mobileno,cast(itemlist->> 'name' as character varying(100)) kidname from customer INNER JOIN LATERAL json_array_elements (customer.family :: json) itemlist ON true 
                                        where isactive = true and mobileno is not null and length(mobileno)>=10 AND json_typeof(customer.family)='array' 
                                        and cast(itemlist->> 'dateofbirth' as timestamp without time zone)>'1900-01-01'
                                        and ('1904-'||to_char(cast(itemlist->> 'dateofbirth' as timestamp without time zone),'MM-dd'))::date >= ('1904-'||to_char(@fromdate::date,'MM-dd'))::date
                                        and ('1904-'||to_char(cast(itemlist->> 'dateofbirth' as timestamp without time zone),'MM-dd'))::date < ('1904-'||to_char(@todate::date,'MM-dd'))::date ";
                    break;
                case "Customer Anniversary":
                    qb = new QueryBuilder();
                    if (Source.config.customerconfig != null &&
                        Source.config.customerconfig.beforedays > 0)
                    {
                        fromdate = fromdate.AddDays(Source.config.customerconfig.beforedays);
                        todate = fromdate.AddDays(1);
                    }

                    qb.CurrentQuery = @"select id from customer where isactive = true and mobileno is not null and length(mobileno)>=10 
                                        and marriagedate>'1900-01-01'
                                        and ('1904-'||to_char(marriagedate,'MM-dd'))::date >= ('1904-'||to_char(@fromdate::date,'MM-dd'))::date
                                        and ('1904-'||to_char(marriagedate,'MM-dd'))::date < ('1904-'||to_char(@todate::date,'MM-dd'))::date ";
                    break;
                case "Customer Festival Holiday":
                    qb = new QueryBuilder();
                    qb.CurrentQuery = "select id,name from holiday where holidaytype = 'Festival' and isactive = true and holidayon>=@fromdate and holidayon<@todate";
                    break;
                case "Customer Payment Reminder":
                    qb = new QueryBuilder();
                    if (Source.config.customerconfig != null &&
                        Source.config.customerconfig.beforedays > 0)
                    {
                        fromdate = fromdate.AddDays((Source.config.customerconfig.beforedays * -1));
                    }
                    qb.CurrentQuery = @"select customerinvoice.customerid, sum(customerinvoice.receivable-customerinvoice.paid) balance
from customerinvoice inner join bill on bill.id = customerinvoice.billid and bill.billtype != 9 
Where customerinvoice.isactive=True And transactiondate<@fromdate
And customerinvoice.type in (1,2) and customerinvoice.receivable-customerinvoice.paid>0  
group by customerinvoice.customerid";
                    break;
                case "Supplier Payment Reminder":
                    qb = new QueryBuilder();
                    if (Source.config.customerconfig != null &&
                        Source.config.customerconfig.beforedays > 0)
                    {
                        fromdate = fromdate.AddDays((Source.config.customerconfig.beforedays * -1));
                    }
                    SupplierpaymentDataService supplierpaymentDataService = new SupplierpaymentDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                    var pendinglist = supplierpaymentDataService.SearchPendingPayment(new Models.SupplierpaymentCreteria()
                    {
                        duefromdate = fromdate
                    });
                    if (pendinglist.Count > 0)
                    {
                        qb.CurrentQuery = @"select suppliercompany.id,suppliercompany.companycontactno from suppliercompany
                    where id in (" + string.Join(",", pendinglist.ConvertAll(e => e.suppliercompanyid)) + ")";
                        result.SMSList = new List<SMSDetails>();
                        result.itemList = pendinglist;
                    }
                    break;
                case "Approved Bill Reminder":
                    qb = new QueryBuilder();
                    if (Source.config.customerconfig != null &&
                        Source.config.customerconfig.beforedays > 0)
                    {
                        fromdate = fromdate.AddDays((Source.config.customerconfig.beforedays * -1));
                    }
                    qb.CurrentQuery = @"Select customer.id, sum(receivable) amount, sum(qty) qty
from bulkbill inner join customer on customer.id = bulkbill.customerid 
Where bulkbill.isactive=True And bulkbill.billtype=10 And bulkbill.billdate<@fromdate 
group by customer.id";
                    break;
            }

            using (NpgsqlConnection conn = baseData.CreateConnection())
            {
                try
                {
                    conn.Open();
                    NpgsqlCommand command = qb.GetCommand(conn);
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = fromdate;
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = todate;

                    if (Source.config.parameter.Contains("Holiday"))
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                root.name = reader.GetString(0);
                            }
                        }
                        command = null;
                        if (!string.IsNullOrEmpty(root.name) && Source.config.parameter.Contains("Customer"))
                        {
                            qb = new QueryBuilder();
                            qb.CurrentQuery = "select id from customer where mobileno is not null and length(mobileno)>=10";
                            command = qb.GetCommand(conn);
                        }
                        else if (!string.IsNullOrEmpty(root.name) && Source.config.parameter.Contains("Employee"))
                        {
                            qb = new QueryBuilder();
                            qb.CurrentQuery = "select id from employee where contactno is not null and length(contactno)>=10";
                            command = qb.GetCommand(conn);
                        }
                    }

                    List<long> ids = new List<long>();
                    List<SMSDetails> kidslist = new List<SMSDetails>();


                    if (command != null)
                    {
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                ids.Add(reader.GetInt64(0));
                                if (Source.config.parameter == "Customer Kids DOB")
                                {
                                    SMSDetails _item = new SMSDetails();
                                    _item.mobileno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                                    _item.Entity = new Models.Customer()
                                    {
                                        mobileno = _item.mobileno,
                                        name = reader["kidname"] == DBNull.Value ? "" : reader["kidname"].ToString(),
                                        id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"])
                                    };
                                    kidslist.Add(_item);
                                }
                                else if (Source.config.parameter == "Supplier Payment Reminder")
                                {
                                    var suppliercompanyid = reader.GetInt64(0);
                                    var mobileno = reader["companycontactno"] == DBNull.Value ? "" : reader["companycontactno"].ToString();
                                    var supplierpendinglist = (result.itemList as List<Models.PendingLrInvoice>).FindAll(e => e.suppliercompanyid == suppliercompanyid);
                                    result.SMSList.AddRange(supplierpendinglist.ConvertAll<SMSDetails>(e =>
                                                           new SMSDetails()
                                                           {
                                                               mobileno = mobileno,
                                                               Entity = e
                                                           }));
                                }
                            }
                        }
                    }
                    conn.Close();

                    if (Source.config.parameter == "Customer Payment Reminder" && ids != null && ids.Count > 0)
                    {
                        CustomerinvoiceDataService CustomerinvoiceData = new CustomerinvoiceDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                        result.SMSList = CustomerinvoiceData.SearchForReminder(new Models.CustomerinvoiceCreteria()
                        {
                            fromdate = fromdate
                        }).ConvertAll(e => new SMSDetails()
                        {
                            mobileno = e.mobileno,
                            Entity = e
                        });
                    }
                    if (Source.config.parameter == "Approved Bill Reminder" && ids != null && ids.Count > 0)
                    {
                        BulkBillDataService bulkbillData = new BulkBillDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                        result.SMSList = bulkbillData.SearchForReminder(new Models.CustomerinvoiceCreteria()
                        {
                            fromdate = fromdate
                        }).ConvertAll(e => new SMSDetails()
                        {
                            mobileno = e.mobileno,
                            Entity = e
                        });
                    }
                    else if (Source.config.parameter.Contains("Customer") && ids != null && ids.Count > 0)
                    {
                        if (Source.config.parameter == "Customer Kids DOB")
                        {
                            result.SMSList = kidslist;
                        }
                        else
                        {
                            Data.CustomerDataService customerDataService = new CustomerDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                            result.SMSList = customerDataService.Search(new Models.CustomerCreteria()
                            {
                                ids = ids
                            }).ConvertAll(e => new SMSDetails()
                            {
                                mobileno = e.GetMobileNo(),
                                mailid = e.GetEmailID(),
                                Entity = e
                            });
                        }

                        if ((Source.config.parameter == "Customer DOB" || Source.config.parameter == "Customer Kids DOB" || Source.config.parameter == "Customer Anniversary") &&
                              (!string.IsNullOrEmpty(Source.config.format) && (Source.config.format.Contains("{COUPON}") || Source.config.format.Contains("[COUPON]")))
                              || (!string.IsNullOrEmpty(Source.config.url) && (Source.config.url.Contains("{COUPON}") || Source.config.url.Contains("[COUPON]")))
                              & Source.config.customerconfig != null &&
                        Source.config.customerconfig.beforedays > 0)
                        {
                            string uniquekey = "BDCPN" + baseData.DateTimeNow.Date.ToString("ddMMyyyy");
                            CustomerCouponsDataService customerCouponsDataservice = new CustomerCouponsDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                            SortedDictionary<long, string> customers = customerCouponsDataservice.SearchUnique(uniquekey);
                            foreach (var item in result.SMSList)
                            {
                                if (!customers.ContainsKey((item.Entity as Models.Customer).id))
                                {
                                    var voucher = new Models.CustomerCoupons()
                                    {
                                        issuedby = 0,
                                        issueto = (item.Entity as Models.Customer).id,
                                        amount = (Source.config.customerconfig.discountpercent > 0) ? Source.config.customerconfig.discountpercent : Source.config.customerconfig.disocuntvalue,
                                        isactive = true,
                                        vouchertype = Models.CustomerCoupons.CouponType.BirthdayCoupon,
                                        issuedon = baseData.DateTimeNow,
                                        validupto = baseData.DateTimeNow.Date.AddDays(Source.config.customerconfig.validity),
                                        minbillvalue = Source.config.customerconfig.min,
                                        maxbilldiscountpercentage = Source.config.customerconfig.max,
                                        amountaspercentage = Source.config.customerconfig.discountpercent > 0,
                                        uniquekey = uniquekey
                                    };
                                    customerCouponsDataservice.Insert(voucher);
                                    customers.Add((item.Entity as Models.Customer).id, voucher.code);
                                    item.coupon = voucher.code;
                                }
                                else
                                {
                                    item.coupon = customers[(item.Entity as Models.Customer).id];
                                }
                            }
                        }
                        else if (Source.config.parameter.Contains("Employee"))
                        {
                            Data.EmployeeDataService employeeDataService = new EmployeeDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                            result.SMSList = employeeDataService.Search(new Models.EmployeeCreteria()
                            {
                                ids = ids
                            }).ConvertAll(e => new SMSDetails()
                            {
                                mobileno = e.GetMobileNo(),
                                mailid = e.GetEmailID(),
                                Entity = e
                            });
                        }
                    }
                }
                catch (Exception exp)
                {
                    if (conn != null)
                        conn.Close();
                }
            }
            return result;
        }

        public class Root : CommonRoot
        {
            public Root() : base()
            {

            }

            public string name { get; set; }
        }
    }

    public class ReorderSummary : ScheduleManager.IReportExecutor
    {

        public override ReportSource Run(long organizationid, int tzoffset)
        {
            Common.BaseData baseData = new Common.BaseData()
            {
                Context = new Common.UserClaim()
                {
                    OrganizationID = organizationid,
                    TZ = tzoffset
                }
            };
            var result = new ReportSource();
            Root root = new Root()
            {
                fromdate = baseData.DateTimeNow,
                todate = baseData.DateTimeNow,
                title = Source.description
            };

            if (Source.config.companyid > 0)
            {
                using (var reader = baseData.ExecuteReader("Select name from company where id=" + Source.config.companyid, null))
                {
                    if (reader.Read())
                        root.company = reader.GetString(0);
                }
            }
            if (Source.config.locationid > 0)
            {
                using (var reader = baseData.ExecuteReader("Select name from referencelist where id=" + Source.config.locationid, null))
                {
                    if (reader.Read())
                        root.location = reader.GetString(0);
                }
            }
            result.RootObject = root;

            if (Source.config.parameter == "Stock Holding Period")
            {
                WarehouseReportDataService warehouseReportDataService = new WarehouseReportDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                result.itemList = warehouseReportDataService.ROLStoreView(new ROLStoreView()
                {
                    companyid = Source.config.companyid,
                    locationid = Source.config.locationid,
                    sectionid = Source.config.sectionid
                });
            }
            else if (Source.config.parameter == "Item Config")
            {
                WarehouseReportDataService warehouseReportDataService = new WarehouseReportDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                Data.ReorderrulesDataService reorderrulesDataService = new ReorderrulesDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                result.itemList = reorderrulesDataService.ReportItems(new Models.ReorderrulesCreteria()
                {
                    locationid = Source.config.locationid,
                    companyid = Source.config.companyid,
                    sectionid = Source.config.sectionid
                });
            }
            else
            {
                Data.ReorderrulesDataService reorderrulesDataService = new ReorderrulesDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
                result.itemList = reorderrulesDataService.Report(new Models.ReorderrulesCreteria()
                {
                    locationid = Source.config.locationid,
                    companyid = Source.config.companyid,
                    sectionid = Source.config.sectionid
                });
            }
            return result;
        }

        public class Root : CommonRoot
        {
        }
    }

    public class InwardDetails : ScheduleManager.IReportExecutor
    {
        public override ReportSource Run(long organizationid, int tzoffset)
        {
            Common.BaseData baseData = new Common.BaseData()
            {
                Context = new Common.UserClaim()
                {
                    OrganizationID = organizationid,
                    TZ = tzoffset
                }
            };
            var result = new ReportSource();
            Root root = new Root()
            {
                fromdate = baseData.DateTimeNow,
                todate = baseData.DateTimeNow,
                title = Source.description
            };

            DateTime fromdate = baseData.DateTimeNow.Date;
            DateTime todate = fromdate.AddDays(1);
            switch (Source.config.parameter)
            {
                case "Current Month":
                    fromdate = new DateTime(fromdate.Year, fromdate.Month, 1);
                    todate = fromdate.AddMonths(1);
                    break;
                case "Current Year":
                    fromdate = new DateTime(fromdate.Year, 1, 1);
                    todate = fromdate.AddYears(1);
                    break;
                case "Current Week":
                    fromdate = fromdate.AddDays(((int)fromdate.DayOfWeek) * -1);
                    todate = fromdate.AddDays(8);
                    break;
            }

            if (Source.config.companyid > 0)
            {
                using (var reader = baseData.ExecuteReader("Select name from company where id=" + Source.config.companyid, null))
                {
                    if (reader.Read())
                        root.company = reader.GetString(0);
                }
            }
            if (Source.config.locationid > 0)
            {
                using (var reader = baseData.ExecuteReader("Select name from referencelist where id=" + Source.config.locationid, null))
                {
                    if (reader.Read())
                        root.location = reader.GetString(0);
                }
            }
            result.RootObject = root;

            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            result.itemList = WarehouseReportData.SearchOutwardCost(new WarehouseReportCreteria()
            {
                pendinginward = true,
                fromdate = fromdate,
                todate = todate
            });

            return result;
        }

        public class Root : CommonRoot
        {
        }
    }


    public class SalesStockSummary : ScheduleManager.IReportExecutor
    {
        public override ReportSource Run(long organizationid, int tzoffset)
        {
            Common.BaseData baseData = new Common.BaseData()
            {
                Context = new Common.UserClaim()
                {
                    OrganizationID = organizationid,
                    TZ = tzoffset
                }
            };
            var result = new ReportSource();
            Root root = new Root()
            {
                fromdate = baseData.DateTimeNow,
                todate = baseData.DateTimeNow,
                title = Source.description
            };

            DateTime fromdate = baseData.DateTimeNow.Date;
            DateTime todate = fromdate.AddDays(1);
            switch (Source.config.parameter)
            {
                case "Yesterday":
                    fromdate = fromdate.AddDays(-1);
                    todate = baseData.DateTimeNow.Date;
                    break;
                case "Last Week":
                    todate = fromdate.AddDays(((int)fromdate.DayOfWeek) * -1);
                    fromdate = todate.AddDays(-8);
                    break;
                case "Last Month":
                    todate = new DateTime(fromdate.Year, fromdate.Month, 1);
                    fromdate = todate.AddMonths(-1);
                    break;
                case "Current Month":
                    fromdate = new DateTime(fromdate.Year, fromdate.Month, 1);
                    todate = fromdate.AddMonths(1);
                    break;
                case "Current Year":
                    fromdate = new DateTime(fromdate.Year, 1, 1);
                    todate = fromdate.AddYears(1);
                    break;
                case "Current Week":
                    fromdate = fromdate.AddDays(((int)fromdate.DayOfWeek) * -1);
                    todate = fromdate.AddDays(8);
                    break;
            }

            if (Source.config.companyid > 0)
            {
                using (var reader = baseData.ExecuteReader("Select name from company where id=" + Source.config.companyid, null))
                {
                    if (reader.Read())
                        root.company = reader.GetString(0);
                }
            }
            if (Source.config.locationid > 0)
            {
                using (var reader = baseData.ExecuteReader("Select name from referencelist where id=" + Source.config.locationid, null))
                {
                    if (reader.Read())
                        root.location = reader.GetString(0);
                }
            }
            result.RootObject = root;

            WarehouseReportDataService WarehouseReportData = new WarehouseReportDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var rpt = WarehouseReportData.SearchSalesStockSummary(new SalesStockCreteria()
            {
                companyid = Source.config.companyid,
                locationid = Source.config.locationid,
                showdesign = Source.config.stocksummaryconfig == null ? false : Source.config.stocksummaryconfig.showdesign,
                showproduct = Source.config.stocksummaryconfig == null ? false : Source.config.stocksummaryconfig.showproduct,
                showsupplier = Source.config.stocksummaryconfig == null ? false : Source.config.stocksummaryconfig.showsupplier,
                showlocation = Source.config.stocksummaryconfig == null ? false : Source.config.stocksummaryconfig.showlocation,
                productid = Source.config.stocksummaryconfig == null ? 0 : Source.config.stocksummaryconfig.productid,
                supplierid = Source.config.stocksummaryconfig == null ? 0 : Source.config.stocksummaryconfig.supplierid,
                fromdate = fromdate,
                todate = todate,
                title = Source.config.parameter + " (" + fromdate.ToString("dd-MM-yyyy") + " - " + todate.AddDays(-1).ToString("dd-MM-yyyy") + ")"
            });
            result.itemList = rpt.items;
            result.RootObject = new ReportRoot(rpt);
            return result;
        }

        public class Root : CommonRoot
        {
        }
        public class ReportRoot : CommonRoot
        {
            private SalesStockReport rpt;
            public decimal stockqty { get; set; }
            public decimal salesqty { get; set; }
            public decimal stockvalue { get; set; }
            public decimal salesvalue { get; set; }


            public ReportRoot()
            {

            }

            public ReportRoot(SalesStockReport rpt)
            {
                this.stockqty = rpt.stockqty;
                this.salesqty = rpt.salesqty;
                this.stockvalue = rpt.stockvalue;
                this.salesvalue = rpt.salesvalue;
                this.title = rpt.title;
            }
        }
    }

    public class SupplierTransaction : ScheduleManager.IReportExecutor
    {
        public override ReportSource Run(long organizationid, int tzoffset)
        {
            Common.BaseData baseData = new Common.BaseData()
            {
                Context = new Common.UserClaim()
                {
                    OrganizationID = organizationid,
                    TZ = tzoffset
                }
            };
            DateTime fromdate = baseData.DateTimeNow.Date;
            DateTime todate = fromdate.AddDays(1);

            switch (Source.config.parameter)
            {
                case "Yesterday":
                    fromdate = fromdate.AddDays(-1);
                    todate = baseData.DateTimeNow.Date;
                    break;
                case "Last Week":
                    todate = fromdate.AddDays(((int)fromdate.DayOfWeek) * -1);
                    fromdate = todate.AddDays(-8);
                    break;
                case "Last Month":
                    todate = new DateTime(fromdate.Year, fromdate.Month, 1);
                    fromdate = todate.AddMonths(-1);
                    break;
                case "Current Month":
                    fromdate = new DateTime(fromdate.Year, fromdate.Month, 1);
                    todate = fromdate.AddMonths(1);
                    break;
                case "Current Year":
                    fromdate = new DateTime(fromdate.Year, 1, 1);
                    todate = fromdate.AddYears(1);
                    break;
                case "Current Week":
                    fromdate = fromdate.AddDays(((int)fromdate.DayOfWeek) * -1);
                    todate = fromdate.AddDays(8);
                    break;
            }

            var result = new ReportSource();
            CommonRoot root = new CommonRoot()
            {
                fromdate = fromdate,
                todate = todate,
                title = Source.description
            };
            if (Source.config.companyid > 0)
            {
                using (var reader = baseData.ExecuteReader("Select name from company where id=" + Source.config.companyid, null))
                {
                    if (reader.Read())
                        root.company = reader.GetString(0);
                }
            }
            if (Source.config.locationid > 0)
            {
                using (var reader = baseData.ExecuteReader("Select name from referencelist where id=" + Source.config.locationid, null))
                {
                    if (reader.Read())
                        root.location = reader.GetString(0);
                }
            }
            result.RootObject = root;
            QuantoAPI.QuantoAPIDataService ReferencelistDatafULL = new QuantoAPI.QuantoAPIDataService();
            switch (Source.name)
            {
                case "SupplierSales":
                    result.itemList = ReferencelistDatafULL.SalesAPI(new QuantoAPI.Criteria()
                    {
                        from = fromdate.ToString("yyyyMMdd"),
                        to = todate.ToString("yyyyMMdd"),
                        supplierid = Source.config.supplierid,
                        brand = Source.config.brand
                    });
                    result.file = ReferencelistDatafULL.Excel(result.itemList as List<QuantoAPI.SalesData>, "SupplierSales");
                    break;
                case "SupplierStock":
                    result.itemList = ReferencelistDatafULL.StockAPI(new QuantoAPI.Criteria()
                    {
                        from = fromdate.ToString("yyyyMMdd"),
                        to = todate.ToString("yyyyMMdd"),
                        supplierid = Source.config.supplierid,
                        brand = Source.config.brand
                    });
                    result.file = ReferencelistDatafULL.Excel(result.itemList as List<QuantoAPI.StockData>, "CurrentStock");
                    break;
                case "SupplierPurchase":
                    result.itemList = ReferencelistDatafULL.PurchaseAPI(new QuantoAPI.Criteria()
                    {
                        from = fromdate.ToString("yyyyMMdd"),
                        to = todate.ToString("yyyyMMdd"),
                        supplierid = Source.config.supplierid,
                        brand = Source.config.brand
                    });
                    result.file = ReferencelistDatafULL.Excel(result.itemList as List<QuantoAPI.PurchaseData>, "Purchase");
                    break;
            }
            return result;
        }


    }
}