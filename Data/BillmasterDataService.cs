using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class BillmasterDataService : SAASData
    {
        const string SQL_Search = @"select billmaster.id, billmaster.isactive, billmaster.createdby, billmaster.modifiedby, billmaster.version, 
        billmaster.createdon, billmaster.modifiedon, billmaster.billdate, customerid, tax, discount, total, receivable, supportcredit
        other,billmaster.code,customer.name as customername,customer.mobileno as customermobileno,customer.code as customercode,locationid from billmaster
        left outer join customer on customer.id = billmaster.customerid
        where billmaster.id=@id";

        const string SQL_Insert = @"INSERT INTO public.billmaster(isactive, createdby, modifiedby, version, createdon, modifiedon, billdate, customerid, tax, discount, total, receivable, other,code,locationid,sourcebilltype) 
        VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @billdate, @customerid, @tax, @discount, @total, @receivable, @other,@code,@locationid,@sourcebilltype) RETURNING id";

        public int UnsettledBillCount(long locationid, long floorid)
        {
            DateTime fromdate = DateTimeNow.Date;
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_saledatefreeze &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locationid) &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].freezedate > DateTime.MinValue)
            {
                DateTime currentsettlementdate = DateTimeNow.Date.AddDays(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].nextdaycount);
                fromdate = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].freezedate;
            }
            else if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_datechange &&
                InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(locationid))
            {
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].currentday == DateTimeNow.Date &&
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].nextdaycount > 0)
                {
                    DateTime currentsettlementdate = DateTimeNow.Date.AddDays(InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[locationid].nextdaycount);
                    fromdate = currentsettlementdate;
                }
            }

            var param = new SortedList<string, object>();
            param.Add("fromdate", fromdate);
            param.Add("todate", fromdate.AddDays(1));
            param.Add("locationid", locationid);
            param.Add("floorid", floorid);
            var query = "Select count(*) from bill where bill.isactive = true " +
                "and bill.settled = false and bill.isb2b=false and billtype!=9 and billdate>=@fromdate and billdate<@todate and locationid=@locationid and floorid=@floorid";
            //if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.system_salefloorwisebillno)
            query = query.Replace(" and floorid=@floorid", "");

            var result = 0;

            using (NpgsqlConnection connection = CreateConnection())
            {
                connection.Open();
                using (var reader = ExecuteReader(query, param, connection))
                {
                    if (reader.Read())
                    {
                        result = Convert.ToInt32(reader[0]);
                    }
                }
                connection.Close();
            }
            return result;
        }

        public List<BillInfo> SearchLastBills()
        {
            try
            {
                var result = new List<BillInfo>();
                var param = new SortedList<string, object>();
                param.Add("createdon", DateTimeNow.Date);
                param.Add("createdby", Context.UserID);
                using (NpgsqlConnection connection = CreateConnection())
                {
                    long billmasterid = 0;
                    long billsettlementmasterid = 0;
                    connection.Open();
                    using (var reader = ExecuteReader(@"select bill.id,bill.billno,bill.receivable,bill.rounding,
                    billmasterid,billsettlementmasterid from bill
                    left outer join billsettlement on bill.id = billsettlement.billid
					where bill.billtype!=9 and bill.createdon>=@createdon and bill.createdby=@createdby 
					order by bill.id desc
					limit 1", param, connection, null))
                    {
                        while (reader.Read())
                        {
                            billmasterid = reader.GetInt64(4);
                            billsettlementmasterid = reader.IsDBNull(5) ? 0 : reader.GetInt64(5);
                            result.Add(new BillInfo()
                            {
                                id = reader.GetInt64(0),
                                billsettlementmasterid = billsettlementmasterid,
                                billno = reader.GetString(1),
                                total = reader.GetDecimal(2) + reader.GetDecimal(3)
                            });
                        }
                    }

                    if (billmasterid > 0 && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_multicompany)
                    {
                        param.Add("billmasterid", billmasterid);
                        result.Clear();
                        using (var reader = ExecuteReader(@"select bill.id,bill.billno,bill.receivable,bill.rounding from bill
					    where bill.billtype!=9 and billmasterid=@billmasterid", param, connection, null))
                        {
                            while (reader.Read())
                            {
                                result.Add(new BillInfo()
                                {
                                    billsettlementmasterid = billsettlementmasterid,
                                    id = reader.GetInt64(0),
                                    billno = reader.GetString(1),
                                    total = reader.GetDecimal(2) + reader.GetDecimal(3)
                                });
                            }
                        }
                    }
                    connection.Close();
                }
                return result;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,
                    exp.StackTrace, "Search Last Bills");
            }
            return null;
        }

        const string SQL_Update = @"UPDATE public.billmaster SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon,code=@code, billdate=@billdate, customerid=@customerid,locationid=@locationid, tax=@tax, discount=@discount, total=@total, receivable=@receivable, other=@other where id=@id and version=@version  RETURNING version";

        const string SQL_Delete = @"update public.billmaster set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        public List<Billmaster> Search(BillmasterCreteria _billmaster)
        {
            List<Billmaster> lst = new List<Billmaster>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_billmaster.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    if (!string.IsNullOrEmpty(_billmaster.code))
                        qb.AddParameter("billmaster.code", "code", _billmaster.code, "=");
                    command = qb.GetCommand(conn);
                    //command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _billmaster.Code;
                }
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billmaster.id;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Billmaster obillmaster = new Billmaster();
                        obillmaster.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obillmaster.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        obillmaster.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obillmaster.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obillmaster.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obillmaster.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obillmaster.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obillmaster.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obillmaster.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        obillmaster.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        obillmaster.discount = reader["discount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discount"]);
                        obillmaster.total = reader["total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["total"]);
                        obillmaster.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        obillmaster.other = reader["other"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["other"]);
                        obillmaster.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        obillmaster.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        obillmaster.customercode = reader["customercode"] == DBNull.Value ? "" : reader["customercode"].ToString();
                        obillmaster.customermobileno = reader["customermobileno"] == DBNull.Value ? "" : reader["customermobileno"].ToString();
                        obillmaster.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        obillmaster.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        lst.Add(obillmaster);
                    }
                }

                if (_billmaster.includebills)
                {
                    BillDataService billservice = new BillDataService() { Context = this.Context };
                    billservice.Context = this.Context;
                    lst[0].Bills = billservice.Search(new BillCreteria()
                    {
                        billmasterid = lst[0].id,
                        isimport = true
                    });
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace, Context.UserID, _billmaster.locationid);
                throw exp;
            }
            return lst;
        }
        public Billmaster Insert(Billmaster oBillmaster)
        {
            return Insert(oBillmaster, null);
        }

        public class Returnbillcollection
        {
            public long billid { get; set; }
            public string billno { get; set; }
            public List<Billitems> billitems { get; set; }
        }

        public void UpdateBillNo(Bill bill,SequenceNo billnosequence,Npgsql.NpgsqlConnection conn,NpgsqlTransaction transaction)
        {

            SortedList<string, object> parameters = new SortedList<string, object>();
            parameters.Add("billid", bill.id);
            if (bill.billattributes != null)
            {
                parameters.Add("billinboxid", bill.billattributes.billinboxid);
                parameters.Add("customerinvoiceid", bill.billattributes.customerinvoiceid);
            }
            parameters.Add("billnoguid", bill.billno);

            bill.billno = billnosequence.GetNextNumber(this.Context, Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[bill.locationid].system_salebillnodailyreset, bill.billdate.Date, conn, transaction);

            if (Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(bill.locationid) &&
                Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[bill.locationid].system_salebillnodailyreset
                && bill.billdate.Date != billnosequence.modifiedon.Date)
            {
                billnosequence.ResetNumber(conn, transaction, bill.billdate.Date);
                bill.billno = billnosequence.GetNextNumber(this.Context, Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[bill.locationid].system_salebillnodailyreset, bill.billdate.Date, conn, transaction);
            }

            int loopcount = 0;
            while (true)
            {
                var billno = bill.billno;
                if (LicenseManager.Instance(Context.OrganizationID).GetLicense().isoffline &&
                    !string.IsNullOrEmpty(Services.ConfigManager.Instance(Context.OrganizationID).Config.system_offlineprefix))
                    billno = Services.ConfigManager.Instance(Context.OrganizationID).Config.system_offlineprefix + bill.billno;
                else if (!LicenseManager.Instance(Context.OrganizationID).GetLicense().isoffline && !string.IsNullOrEmpty(Services.ConfigManager.Instance(Context.OrganizationID).Config.system_onlineprefix))
                    billno = Services.ConfigManager.Instance(Context.OrganizationID).Config.system_onlineprefix + bill.billno;

                var param = new SortedList<string, object>();
                param.Add("billno", billno);
                using (var reader = ExecuteReader("Select id from bill where billno=@billno", param))
                {
                    if (!reader.Read())
                    {
                        break;
                    }
                    bill.billno = billnosequence.GetNextNumber(this.Context, Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[bill.locationid].system_salebillnodailyreset, bill.billdate.Date, conn, transaction);
                }
                if (loopcount > 100) throw new Exception(string.Format("Duplicate Bill No found. Please verify number configuration. Last Bill No is {0}", bill.billno));
                loopcount++;
            }

            if (bill.id > 0)
            {
                parameters.Add("billno", bill.billno);
                parameters.Add("billsettlementid", long.Parse("0"));
                ExecuteQuery(@"update bill set billno=@billno,billnoguid=@billnoguid where id=@billid and isactive=true", parameters, conn, transaction);
                ExecuteQuery(@"update billinbox set billno=@billno where id=@billinboxid", parameters, conn, transaction);
                ExecuteQuery(@"update Customerinvoice set billno=@billno where id=@customerinvoiceid", parameters, conn, transaction);
                if (!string.IsNullOrEmpty(bill.billattributes.packslipid))
                {
                    PackingslipDataService packingslipDataService = new PackingslipDataService();
                    packingslipDataService.Context = this.Context;
                    packingslipDataService.UpdateBill(bill, conn, transaction);
                }

                if (bill.billattributes != null &&
                    bill.billattributes.settlement != null &&
                    bill.billattributes.settlement.Billsettlement != null &&
                    bill.billattributes.settlement.Billsettlement.Count > 0)
                {
                    foreach (var billsettlement in bill.billattributes.settlement.Billsettlement)
                    {
                        parameters["billsettlementid"] = billsettlement.id;
                        ExecuteQuery(@"update billsettlement set billno=@billno where id=@billsettlementid", parameters, conn, transaction);
                    }
                }
            }
        }
        public Billmaster Insert(Billmaster oBillmaster, string bulkBillNo)
        {
            InfyPOS.Data.ReferencelistDataService reflist = new ReferencelistDataService() { Context = this.Context };
            var location = reflist.Search(new ReferencelistCreteria()
            {
                id = oBillmaster.locationid
            });

            List<Referencelist> counter = new List<Referencelist>();
            if (oBillmaster.Bills[0].counterid > 0)
            {
                counter = reflist.Search(new ReferencelistCreteria()
                {
                    id = oBillmaster.Bills[0].counterid
                });
            }

            if (oBillmaster.Bills.Exists(e => e.billattributes != null && e.billattributes.gift == true))
                oBillmaster.Bills.ForEach(e => e.billattributes.gift = false);

            NpgsqlTransaction transaction = null;
            try
            {
                bool offline = LicenseManager.Instance(Context.OrganizationID).GetLicense().isoffline;
                InfyPOS.Data.DiscountqueueDataService discountqueue = new DiscountqueueDataService() { Context = this.Context };
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("billmaster.code", oBillmaster.locationid);
                SequenceNo billnosequence = null;
                Bill bulkBill = null;
                if (!string.IsNullOrEmpty(bulkBillNo) && (oBillmaster.billattributes == null || oBillmaster.billattributes.ebillno == null || oBillmaster.billattributes.ebillno.Count == 0))
                {
                    BulkBillDataService bulkbilldata = new BulkBillDataService() { Context = this.Context };
                    var bulkbills = bulkbilldata.Search(new BillCreteria()
                    {
                        billno = bulkBillNo,
                        locationid = oBillmaster.locationid,
                        isbulkreturn = true
                    });
                    if (bulkbills.Count == 1)
                    {
                        bulkBill = bulkbills[0];
                        oBillmaster.bulkbillid = bulkBill.id;
                        oBillmaster.returnbillno = "";
                        oBillmaster.Bills.ForEach(e => e.returnbillno = "");
                    }
                }
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    oBillmaster.code = sequence.GetNextNumber(this.Context, conn, transaction);

                    int loopcode = 0;
                    while (true)
                    {
                        var param = new SortedList<string, object>();
                        param.Add("code", oBillmaster.code);
                        using (var reader = ExecuteReader("Select id from billmaster where code=@code", param))
                        {
                            if (!reader.Read())
                            {
                                break;
                            }
                            oBillmaster.code = sequence.GetNextNumber(this.Context, conn, transaction);
                        }
                        if (loopcode > 50) throw new Exception(string.Format("Duplicate Bill Master Code found. Please verify number configuration. Last Bill No is {0}", oBillmaster.code));
                        loopcode++;
                    }

                    oBillmaster.sourcebilltype = oBillmaster.Bills.First().billtype;
                    oBillmaster.billdate = oBillmaster.Bills.First().billdate;

                    if (oBillmaster.sourcebilltype != Bill.BillTypes.ReturnBill && 
                        !oBillmaster.Bills.All(x=>x.Billitems.All(y=>y.amount<0)) &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportdeliveryrack)
                    {
                        DeliveryrackDataService deliveryrackDataService = new DeliveryrackDataService() { Context = this.Context };
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_deliveryrackauto)
                        {
                            var deliveryrack = deliveryrackDataService.ReleaseRack(oBillmaster, conn, transaction);
                            if (deliveryrack != null)
                            {
                                if (oBillmaster.billattributes == null)
                                    oBillmaster.billattributes = new BillAttribute();

                                oBillmaster.billattributes.deliveryrack = deliveryrack.code;
                                oBillmaster.billattributes.deliveryrackcard = deliveryrack.card;
                                oBillmaster.billattributes.deliveryrackid = deliveryrack.id;
                            }
                        }
                        else 
                        {
                            var deliveryrack = deliveryrackDataService.GetRack(oBillmaster, conn, transaction);
                            if (deliveryrack != null)
                            {
                                if (oBillmaster.billattributes == null)
                                    oBillmaster.billattributes = new BillAttribute();
                                oBillmaster.billattributes.deliveryrackcard = deliveryrack.card;
                                oBillmaster.billattributes.deliveryrack = deliveryrack.code;
                                oBillmaster.billattributes.deliveryrackid = deliveryrack.id;
                            }
                        }
                        if (oBillmaster.billattributes != null)
                        {
                            oBillmaster.Bills.ForEach(x =>
                            {
                                if (x.billattributes == null)
                                    x.billattributes = new BillAttribute();
                                x.billattributes.deliveryrackcard = oBillmaster.billattributes.deliveryrackcard;
                                x.billattributes.deliveryrack = oBillmaster.billattributes.deliveryrack;
                                x.billattributes.deliveryrackid = oBillmaster.billattributes.deliveryrackid;
                            });
                        }
                    }

                    Insert(transaction, conn, oBillmaster);


                    BillDataService BillData = new BillDataService() { Context = this.Context };
                    BillData.Context = this.Context;
                    if (oBillmaster.customerid == 0 && !string.IsNullOrEmpty(oBillmaster.customermobileno))
                    {
                        CustomerDataService custdata = new CustomerDataService() { Context = this.Context };
                        var cust = new Models.Customer();
                        cust.name = string.IsNullOrEmpty(oBillmaster.customername) ? "Customer" : oBillmaster.customername;
                        cust.mobileno = oBillmaster.customermobileno;
                        cust.registredlocationid = oBillmaster.locationid;
                        cust.isactive = true;
                        cust.version = 1;
                        cust.createdon = DateTimeNow;
                        cust.customertype = 4;
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillmaster.locationid))
                            cust.categoryid = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillmaster.locationid].sale_defaultcustomercategory;

                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcustomer10digits &&
                            cust.mobileno.Length != 10)
                        {
                            throw new Exception("Please enter 10 digits customer contact no. " + oBillmaster.customermobileno);
                        }

                        if (custdata.DuplicateValidation(cust) > 0)
                        {
                            throw new Exception("Mobileno already exists. please check. " + oBillmaster.customermobileno);
                        }

                        custdata.Insert(transaction, conn, cust);
                        oBillmaster.Bills.ForEach(e => { e.customerid = cust.id; e.customer = cust; });
                        oBillmaster.customerid = cust.id;
                        oBillmaster.customername = cust.name;
                        oBillmaster.customermobileno = cust.mobileno;
                    }
                    else if (oBillmaster.customerid > 0)
                    {
                        CustomerDataService custdata = new CustomerDataService() { Context = this.Context };
                        var customerlist = custdata.Search(new CustomerCreteria()
                        {
                            id = oBillmaster.customerid
                        });
                        if (customerlist != null && customerlist.Count == 1)
                        {
                            oBillmaster.Bills.ForEach(e => { e.customer = customerlist[0]; });
                        }
                    }
                    oBillmaster.Bills.ForEach(e => e.billmasterid = oBillmaster.id);
                    Billsettlementmaster settlement = null;

                    if (!oBillmaster.Bills.First().isb2b && (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_silentsettlement ||
                       (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillmaster.locationid)) &&
                       InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillmaster.locationid].system_silentsettlement))
                    {
                        settlement = new Billsettlementmaster();
                        settlement.settlementon = oBillmaster.Bills.First().billdate;
                        settlement.billamount = oBillmaster.total;
                        settlement.companyid = oBillmaster.Bills.First().companyid;
                        settlement.customerid = oBillmaster.customerid;
                        settlement.locationid = oBillmaster.locationid;
                        settlement.paidamount = oBillmaster.total;
                        settlement.receivable = oBillmaster.receivable;
                        settlement.tax = oBillmaster.tax;
                        settlement.version = 1;
                        settlement.isactive = true;
                        settlement.createdby = Context.UserID;
                        settlement.modifiedby = Context.UserID;
                        settlement.createdon = DateTimeNow;
                        settlement.modifiedon = DateTimeNow;
                        settlement.casherid = this.Context.EmployeeID;
                        settlement.cashiername = this.Context.EmployeeName;
                        settlement.counterid = oBillmaster.Bills.First().counterid;
                        settlement.countername = oBillmaster.Bills.First().countername;
                        settlement.floorid = oBillmaster.Bills.First().floorid;
                        settlement.floorname = oBillmaster.Bills.First().floorname;
                        settlement.paymentinfo = new Billsettlementmaster.Paymentinfo();
                        settlement.paymentinfo.cash_paid = oBillmaster.total;
                        settlement.Billsettlement = new List<Billsettlement>();
                    }
                    else if (oBillmaster.Bills.First().billattributes != null && oBillmaster.Bills.First().billattributes.settlement != null)
                    {
                        settlement = oBillmaster.Bills.First().billattributes.settlement;
                        settlement.settlementon = oBillmaster.Bills.First().billdate;
                        settlement.casherid = this.Context.EmployeeID;
                        settlement.cashiername = this.Context.EmployeeName;
                        settlement.counterid = oBillmaster.Bills.First().counterid;
                        settlement.countername = oBillmaster.Bills.First().countername;
                        settlement.floorid = oBillmaster.Bills.First().floorid;
                        settlement.isactive = true;
                        settlement.floorname = oBillmaster.Bills.First().floorname;
                        oBillmaster.Bills.ForEach(e => e.billattributes.settlement = null);
                    }

                    string deliveryno = null;
                    if (oBillmaster.Bills.First().billtype != Bill.BillTypes.ReturnBill &&
                        Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillmaster.locationid) &&
                        Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillmaster.locationid].sale_deliveryno)
                    {
                        var dsequence = SequenceManager.Instance(Context.OrganizationID).Get("bill.deliveryno", oBillmaster.locationid, 0,
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.system_salefloorwisebillno ||
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.system_floorwisedeliveryno ?
                            oBillmaster.Bills.First().floorid : 0);

                        if (!dsequence.Validate(this.Context, "Delivery No", false))
                        {
                            if (location.Count == 1)
                                dsequence.Validate(this.Context, "Delivery No for " + location[0].name, "{dn}", true);
                        }

                        deliveryno = dsequence.GetNextNumber(this.Context, Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillmaster.Bills[0].locationid].system_salebillnodailyreset, oBillmaster.Bills.First().billdate.Date, conn, transaction);
                        if (dsequence.modifiedon.Date != oBillmaster.Bills.First().billdate.Date)
                        {
                            dsequence.ResetNumber(conn, transaction, oBillmaster.Bills.First().billdate.Date);
                            deliveryno = dsequence.GetNextNumber(this.Context, Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillmaster.Bills[0].locationid].system_salebillnodailyreset, oBillmaster.Bills.First().billdate.Date, conn, transaction);
                        }

                        if (deliveryno.Contains("{dn}"))
                        {
                            deliveryno =
                                deliveryno.Replace("{dn}", dsequence.GetAmountBasedPrefix(oBillmaster.total));
                        }
                    }



                    oBillmaster.Bills.First().isfirst = true;
                    oBillmaster.Bills.Last().islast = true;

                    if (oBillmaster.Bills.Count > 1 &&
                        oBillmaster.Bills.First().billattributes != null &&
                        oBillmaster.Bills.First().billattributes.rewardpoint > 0 &&
                        oBillmaster.Bills.First().billattributes.rewardamount > 0)
                    {
                        var rewardsamount = oBillmaster.Bills.First().billattributes.rewardamount;
                        var rewardspoint = oBillmaster.Bills.First().billattributes.rewardpoint;
                        var treceivable = oBillmaster.Bills.Sum(x => x.Billitems.Sum(y => y.additionaldiscount));
                        for (var idx = 0; idx < oBillmaster.Bills.Count; idx++)
                        {
                            if (idx == oBillmaster.Bills.Count - 1)
                            {
                                oBillmaster.Bills[idx].billattributes.rewardpoint = rewardspoint;
                                oBillmaster.Bills[idx].billattributes.rewardamount = rewardsamount;
                            }
                            else
                            {
                                var percent = Math.Round(oBillmaster.Bills[idx].Billitems.Sum(x => x.additionaldiscount) / treceivable * 100, 4);
                                oBillmaster.Bills[idx].billattributes.rewardpoint = Convert.ToInt64(Math.Round(rewardspoint * percent / 100, 0));
                                oBillmaster.Bills[idx].billattributes.rewardamount = Convert.ToInt64(Math.Round(rewardsamount * percent / 100, 0));
                                rewardsamount -= oBillmaster.Bills[idx].billattributes.rewardamount;
                                rewardspoint -= oBillmaster.Bills[idx].billattributes.rewardpoint;
                            }
                        }
                    }
                    foreach (var bill in oBillmaster.Bills)
                    {
                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_salefloorwisebillno && bill.floorid == 0)
                            throw new Exception("Floor ID missing., Please check the billing counter association with floor.");

                        if (bill.billattributes == null)
                            bill.billattributes = new BillAttribute();

                        bill.billattributes.counterid = bill.counterid;
                        bill.billattributes.countername = bill.countername;
                        bill.billattributes.floorid = bill.floorid;
                        bill.billattributes.floorname = bill.floorname;
                        bill.billattributes.biller = this.Context != null ? this.Context.EmployeeName : "";
                        bill.areaid = oBillmaster.areaid;

                        if (bill.customerid > 0 && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[bill.locationid].settlement_reward
                            && !bill.customer.disableloyalty)
                        {
                            if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[bill.locationid].sale_canclaimloyaltyholderonly
                                || !string.IsNullOrEmpty(bill.customer.loyeltyno))
                            {

                                var _receivable = bill.receivable + bill.rounding;
                                CustomerrewardsDataService rewarddata = new CustomerrewardsDataService() { Context = this.Context };
                                if (rewarddata.CanGetRewardAtBilling())
                                {
                                    CustomerRewardStatus rewardstatus = rewarddata.SearchReward(new CustomerrewardsCreteria()
                                    {
                                        customerid = bill.customerid,
                                        locationid = bill.locationid,
                                        billvalue = _receivable,
                                        bill = bill,
                                        billid = bill.id
                                    });
                                    if (rewardstatus != null)
                                    {
                                        bill.billattributes.earnedreward = rewardstatus.billrewards;
                                        bill.billattributes.newreward = (bill.billattributes.newreward == 0 ? bill.billattributes.earnedreward : bill.billattributes.newreward);
                                        bill.billattributes.remainingreward = rewardstatus.rewards - bill.billattributes.rewardpoint;
                                        bill.billattributes.spentreward = bill.billattributes.rewardpoint;
                                        if (bill.billattributes.remainingreward < 0) bill.billattributes.remainingreward = 0;
                                    }
                                }
                                else
                                {
                                    bill.billattributes.spentreward = bill.adjustedrewardamount;
                                }
                            }
                        }
                        else if (bill.customerid > 0 && bill.billattributes.rewardpoint > 0
                            && !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[bill.locationid].settlement_reward)
                        {
                            bill.billattributes.spentreward = bill.billattributes.rewardpoint;
                        }

                        if (!string.IsNullOrEmpty(deliveryno))
                        {
                            bill.billattributes.deliveryno = deliveryno;
                        }

                        if (!bill.issynchronize && !bill.offline && offline)
                            bill.offline = true;

                        if (!bill.HasPendingPromotion &&
                            (bill.schemediscount != 0 || bill.Billitems.Exists(e => e.schemediscount != 0)))
                        {
                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[bill.locationid] != null &&
                                !InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[bill.locationid].sale_promotiononlyonestimate)
                            {
                                Services.PromotionManager.Instance(Context.OrganizationID).ClearPromotion(bill);
                            }
                        }

                        if (bill.billtype != Bill.BillTypes.ReturnBill && bill.billtype != Bill.BillTypes.Cancel)
                        {
                            if (bill.Billitems.Exists(e => e.qty > 0 && e.receivable < 0))
                            {
                                throw new Exception("Some barcodes contines negative values because of additional discount - " + string.Join(",", bill.Billitems.Where(e => e.additionaldiscount > 0 && e.receivable < 0).ToList().ConvertAll(e => e.barcode)));
                            }
                        }
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(bill.locationid) &&
                        InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[bill.locationid].sale_mandatorywalkintoken)
                        {
                            if (string.IsNullOrEmpty(bill.billattributes.walkintoken))
                                throw new Exception("Please scan the walkin Token., Its Mandatory.");
                        }

                        string billsequence = "bill.billno";
                        bool locationwisebillno = true;
                        if (bill.billtype == Bill.BillTypes.ReturnBill && Services.ConfigManager.Instance(Context.OrganizationID).Config.system_srbillno)
                        {
                            locationwisebillno = Services.ConfigManager.Instance(Context.OrganizationID).Config.system_srbillnobylocation;
                            billsequence = "billreturn.billno";
                        }

                        billnosequence = SequenceManager.Instance(Context.OrganizationID).Get(billsequence, locationwisebillno ? bill.locationid : 0, bill.companyid,
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.system_salefloorwisebillno ? bill.floorid : 0);

                        if (billsequence == "billreturn.billno")
                        {
                            billnosequence = SequenceManager.Instance(Context.OrganizationID).Get(billsequence, locationwisebillno ? bill.locationid : 0, bill.companyid,
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.system_srfloorwisebillno ? bill.floorid : 0);
                        }

                        if (!bill.issynchronize)
                        {
                            bill.version = 1;
                            if(Services.ConfigManager.Instance(Context.OrganizationID).Config.system_billnoqueue)
                                bill.billno = GenerateUnique25CharId();
                            else
                                UpdateBillNo(bill, billnosequence, conn, transaction);
                            if (bill.additionaldiscount > 0)
                                bill.discountstatus = Discountqueue.DiscountStatus.Requested;
                        }
                        if (bill.billtype == Bill.BillTypes.ReturnBill)
                            bill.isreturn = true;
                        Bill result = BillData.Insert(transaction, conn, bill, oBillmaster);

                        if (!bill.issynchronize && bill.returnbillid != 0
                            && bill.billtype != Bill.BillTypes.ReturnBill
                            && bill.Billitems.Exists(e => e.receivable < 0))
                        {
                            var newBIll = Newtonsoft.Json.JsonConvert.DeserializeObject<Bill>(Newtonsoft.Json.JsonConvert.SerializeObject(bill));
                            newBIll.Billitems.RemoveAll(e => e.returnbillitemid <= 0 || e.receivable > 0);
                            if (newBIll.Billitems.Count > 0)
                            {
                                newBIll.Billitems.ForEach(e =>
                                {
                                    e.qty = e.qty * -1;
                                    e.receivable = e.receivable * -1;
                                    e.saleamount = e.saleamount * -1;
                                    e.tax = e.tax * -1;
                                });

                                SortedDictionary<string, Returnbillcollection> multiplereturnbills = new SortedDictionary<string, Returnbillcollection>();
                                using (var reader = ExecuteReader(@"select bill.id,bill.billno,billitems.id billitemid, bill.billdate from bill
                                                                    inner join billitems on billitems.billid = bill.id and bill.isactive = true and billitems.isactive = true
                                                                    where billitems.id in (" + string.Join(",", newBIll.Billitems.ConvertAll(e => e.returnbillitemid)) + ") and billtype=8 and settled=false", new SortedList<string, object>(), conn, transaction))
                                {
                                    while (reader.Read())
                                    {
                                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillmaster.locationid) &&
                                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillmaster.locationid].sale_disableotherdayretbill &&
                                            bill.billdate.Date != Convert.ToDateTime(reader["billdate"].ToString()).Date)
                                        {
                                            throw new Exception("Other day return bill consumption disabled - " + reader.GetString(1));
                                        }

                                        if (!multiplereturnbills.ContainsKey(reader.GetString(1)))
                                            multiplereturnbills.Add(reader.GetString(1), new Returnbillcollection()
                                            {
                                                billitems = new List<Billitems>(),
                                                billid = reader.GetInt64(0),
                                                billno = reader.GetString(1)
                                            });
                                        multiplereturnbills[reader.GetString(1)].billitems.Add(newBIll.Billitems.Find(e => e.returnbillitemid == reader.GetInt64(2)));
                                    }
                                }
                                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillmaster.locationid) &&
                                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillmaster.locationid].sale_disableoffsetbill)
                                {
                                    List<long> billids = new List<long>();
                                    foreach (var items in multiplereturnbills)
                                    {
                                        foreach (var biitem in items.Value.billitems.ToList())
                                        {
                                            if (!billids.Contains(items.Value.billid))
                                                billids.Add(items.Value.billid);
                                        }
                                    }
                                    BillData.CancelOnly(new ActionRequest<BillCancel>()
                                    {
                                        Item = new BillCancel()
                                        {
                                            ids = billids
                                        }
                                    }
                                    , transaction, conn, false);
                                }
                                else
                                {
                                    foreach (var items in multiplereturnbills)
                                    {
                                        newBIll.Billitems = items.Value.billitems.ToList();
                                        newBIll = oBillmaster.CreateBill(newBIll, DateTimeNow);
                                        newBIll.billtype = Bill.BillTypes.Cancel;
                                        newBIll.returnbillid = items.Value.billid;
                                        newBIll.returnbillno = "~" + items.Value.billno;
                                        newBIll.unsettledcancelled = true;
                                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config != null && Services.ConfigManager.Instance(Context.OrganizationID).Config.system_srbillno)
                                        {
                                            bool locationwise = Services.ConfigManager.Instance(Context.OrganizationID).Config.system_srbillnobylocation;
                                            string retunbillsequence = "billreturn.billno";
                                            var returnbillseq = SequenceManager.Instance(Context.OrganizationID).Get(retunbillsequence, locationwise ? bill.locationid : 0,
                                                bill.companyid, Services.ConfigManager.Instance(Context.OrganizationID).Config.system_srfloorwisebillno ? bill.floorid : 0);
                                            newBIll.billno = returnbillseq.GetNextNumber(this.Context, Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[bill.locationid].system_salebillnodailyreset, newBIll.billdate.Date, conn, transaction);
                                            while (true)
                                            {
                                                var param = new SortedList<string, object>();
                                                param.Add("billno", newBIll.billno);
                                                using (var reader = ExecuteReader("Select id from bill where billno=@billno", param))
                                                {
                                                    if (!reader.Read())
                                                    {
                                                        break;
                                                    }
                                                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("Duplicate Return Bill No " + newBIll.billno),
                                                    WebAPI.Common.Logger.LogType.Error, newBIll, Context.UserID, newBIll.locationid);
                                                    newBIll.billno = returnbillseq.GetNextNumber(this.Context, Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[bill.locationid].system_salebillnodailyreset, newBIll.billdate.Date, conn, transaction);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            newBIll.billno = billnosequence.GetNextNumber(this.Context, Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[bill.locationid].system_salebillnodailyreset, newBIll.billdate.Date, conn, transaction);
                                            while (true)
                                            {
                                                var param = new SortedList<string, object>();
                                                param.Add("billno", newBIll.billno);
                                                using (var reader = ExecuteReader("Select id from bill where billno=@billno", param, conn, transaction))
                                                {
                                                    if (!reader.Read())
                                                    {
                                                        break;
                                                    }

                                                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("Duplicate Bill No " + newBIll.billno),
                                                        WebAPI.Common.Logger.LogType.Error, newBIll, Context.UserID, newBIll.locationid);

                                                    newBIll.billno = billnosequence.GetNextNumber(this.Context, Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[bill.locationid].system_salebillnodailyreset, newBIll.billdate.Date, conn, transaction);
                                                }
                                            }
                                        }
                                        BillData.Insert(transaction, conn, newBIll, oBillmaster);
                                        if (!BillData.SetCancelled(transaction, conn, newBIll.returnbillid))
                                        {
                                            throw new Exception("Some of the return bill trying to settled twice - " + items.Value.billno + " for " + result.billno + " billid = " + bill.returnbillid);
                                        }
                                    }
                                }
                            }
                            else if (bill.returnbillid > 0)
                            {
                                var param = new SortedList<string, object>();
                                param.Add("id", bill.returnbillid);
                                using (var reader = ExecuteReader("Select id, billno from bill where id=@id and settled=true and billtype=8", param, conn, transaction))
                                {
                                    if (reader.Read())
                                    {
                                        throw new Exception("Some of the return bill trying to settled twice - " + reader.GetString(1));
                                    }
                                }
                            }
                        }

                        if (bill.additionaldiscount - (bill.hasrewardcoupons ? (bill.billattributes.couponamount + bill.billattributes.rewardamount) : 0) > 0)
                        {
                            Discountqueue dq = new Discountqueue()
                            {
                                billid = bill.id,
                                notes = bill.discountrequestreason,
                                requestedamount = bill.additionaldiscount,
                                requestedby = bill.createdby,
                                requestedon = bill.billdate,
                                billamount = bill.receivable,
                                isactive = true,
                                status = Discountqueue.DiscountStatus.Requested,
                                billno = bill.billno,
                                companyid = bill.companyid
                            };
                            discountqueue.Insert(transaction, conn, dq);
                        }
                        if (bulkBill != null)
                        {
                            if (bulkBill.billtype != Bill.BillTypes.BulkSaleCompleted)
                            {
                                BulkBillItemsDataService bulkbillitems = new BulkBillItemsDataService() { Context = this.Context };
                                bulkbillitems.Context = this.Context;
                                bulkBill.returnbillno = bill.billno;
                                if (!bulkbillitems.SyncBillQty(bulkBill, bill, transaction, conn))
                                    throw new Exception("Approved bill already received");
                            }
                            else
                            {
                                throw new Exception("Approved bill already received - " + bulkBill.billno + " ,received bill " + bulkBill.returnbillno);
                            }
                        }
                    }

                    if (location.Count == 1 && location[0].additionalinfo != null &&
                        location[0].additionalinfo.autosettlement == true &&
                        oBillmaster.Bills.First().billtype == Bill.BillTypes.Cash &&
                        settlement == null)
                    {
                        throw new Exception("Settlement details are mandatory for the single window settlement LOCATION configuration - the bill is - " + oBillmaster.Bills[0].billno);
                    }

                    if (counter.Count == 1 && counter[0].additionalinfo != null &&
                    counter[0].additionalinfo.autosettlement == true &&
                    oBillmaster.Bills.First().billtype == Bill.BillTypes.Cash &&
                    settlement == null)
                    {
                        throw new Exception("Settlement details are mandatory for the single window settlement COUNTER configuration - the bill is - " + oBillmaster.Bills[0].billno);
                    }

                    if (oBillmaster.Bills[0].billattributes != null &&
                        oBillmaster.Bills[0].billattributes.ebillno != null &&
                        oBillmaster.Bills[0].billattributes.ebillno.Count > 0 &&
                        oBillmaster.Bills[0].billtype != Bill.BillTypes.ReturnBill)
                    {
                        BulkBillDataService bulkbillitems = new BulkBillDataService() { Context = this.Context };
                        bulkbillitems.UpdateReturnBillNo(conn, transaction, oBillmaster.Bills);
                    }
                    //if (oBillmaster.Bills.First().billtype == Bill.BillTypes.ReturnBill &&
                    //    ((location.Count == 1 && location[0].additionalinfo != null && location[0].additionalinfo.autosettlement == true)
                    //    || (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_silentsettlement ||
                    //    (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillmaster.locationid)) &&
                    //    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillmaster.locationid].system_silentsettlement)))
                    if (oBillmaster.Bills.First().billtype == Bill.BillTypes.ReturnBill && settlement != null)
                    {
                        settlement = null;
                    }

                    if (settlement != null)
                    {
                        settlement.paymentinfo.autosettlement = true;

                        if (settlement.Billsettlement == null)
                            settlement.Billsettlement = new List<Billsettlement>();
                        else
                            settlement.Billsettlement.Clear();
                        foreach (var bills in oBillmaster.Bills)
                        {
                            settlement.Billsettlement.Add(new Billsettlement()
                            {
                                companyid = bills.companyid,
                                billid = bills.id,
                                billno = bills.billno,
                                billamount = bills.receivable,
                                discount = bills.discount,
                                billadditionaldiscount = bills.additionaldiscount,
                                receivable = bills.total,
                                customerid = bills.customerid,
                                mode = (int)bills.billtype,
                                receiptdate = bills.billdate,
                                isactive = true,
                                version = 1,
                                discountreason = bills.discountrequestreason,
                                adjustablereward = (Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillmaster.locationid].sale_canclaimsamebillrewards
                                                        && (bills.billattributes != null && bills.billattributes.spentreward > 0) ? bills.billattributes.newreward : 0)
                            });
                        }

                        if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_silentsettlement ||
                            (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillmaster.locationid)) &&
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillmaster.locationid].system_silentsettlement)
                        {
                            var settleamount = Math.Round(settlement.Billsettlement.Sum(e => e.receivable), 0);
                            var receivableamt = Math.Round(settlement.receivable, 0);
                            if ((settleamount - receivableamt) > 2 || (settleamount - receivableamt) < -2)
                            {
                                throw new Exception("Silent Settlement Error Mismatch in bill receivable - " + receivableamt + " with " + settleamount);
                            }
                            if (settleamount < 0 && receivableamt < 0)
                            {
                                if (settlement.paymentinfo.creditnote_refund > 0)
                                {
                                    if (settlement.paymentinfo == null)
                                        settlement.paymentinfo = new Billsettlementmaster.Paymentinfo();
                                    settlement.paymentinfo.cash_refund = 0;
                                    settlement.paymentinfo.cash_return = 0;
                                    settlement.paymentinfo.cash_paid = 0;
                                }
                                else
                                {
                                    settlement.paid_refund = receivableamt;
                                    if (settlement.paymentinfo == null)
                                        settlement.paymentinfo = new Billsettlementmaster.Paymentinfo();
                                    settlement.paymentinfo.cash_refund = receivableamt * -1;
                                    settlement.paymentinfo.cash_return = receivableamt * -1;
                                    settlement.paymentinfo.cash_paid = 0;
                                }
                            }
                            if (settlement.Billsettlement.Exists(e => e.mode == 1))
                            {
                                settlement.paymentinfo.mode = "CREDIT";
                                settlement.paymentinfo.credit_paid = settlement.paymentinfo.cash_paid;
                                settlement.paymentinfo.cash_paid = 0;
                            }
                        }

                        BillsettlementmasterDataService billsettlementmasterDataService = new BillsettlementmasterDataService() { Context = this.Context };
                        billsettlementmasterDataService.Context = this.Context;
                        if (settlement.Billsettlement != null &&
                            settlement.Billsettlement.Count > 0)
                        {
                            settlement.customerid = oBillmaster.Bills.First().customerid;
                            settlement.customername = oBillmaster.Bills.First().customername;
                            settlement.customermobileno = oBillmaster.customermobileno;
                            settlement.floorid = oBillmaster.Bills.First().floorid;
                            settlement.floorname = oBillmaster.Bills.First().floorname;
                            settlement.locationid = oBillmaster.Bills.First().locationid;
                            settlement.counterid = oBillmaster.Bills.First().counterid;
                            billsettlementmasterDataService.InsertDirect(transaction, conn, settlement);
                            if (oBillmaster.Bills.First().billattributes == null)
                                oBillmaster.Bills.First().billattributes = new BillAttribute();
                            oBillmaster.Bills.First().billattributes.settlement = settlement;
                            //oBillmaster.Bills.First().billattributes.earnedreward = settlement.paymentinfo.rewardpoint;
                            //oBillmaster.Bills.First().billattributes.remainingreward = (long)settlement.paymentinfo.avlrewardpoint;
                            //oBillmaster.Bills.First().billattributes.spentreward = settlement.Billsettlement.Sum(e=>e.adjustablereward);
                            BillData.UpdateAttributes(transaction, conn, oBillmaster.Bills.First());
                        }
                        else
                        {
                            WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(new Exception("Auto settlement for return bills"),
                                WebAPI.Common.Logger.LogType.Error, oBillmaster, Context.UserID, oBillmaster.locationid);
                        }
                    }

                    if (oBillmaster.Bills.First().isb2b)
                    {
                        CustomerinvoiceDataService customerdata = new CustomerinvoiceDataService() { Context = this.Context };
                        customerdata.Context = this.Context;

                        foreach (var bills in oBillmaster.Bills)
                        {
                            var customerinvoice = customerdata.Insert(transaction, conn, new Customerinvoice
                            {
                                isactive = true,
                                createdby = Context.UserID,
                                createdon = DateTimeNow,
                                version = 1,
                                type = 2,
                                receivable = bills.receivable + bills.additionalcharges + bills.rounding,
                                transactiondate = bills.billdate,
                                billid = bills.id,
                                customerid = bills.customerid,
                                billno = bills.billno,
                                companyid = bills.companyid
                            });
                            bills.billattributes.customerinvoiceid = customerinvoice.id;
                        }
                    }

                    if (InfyPOS.Services.ConfigManager.Instance(BillData.Context.OrganizationID).Config.intray_notification_ecommerce)
                    {
                        foreach (var bitem in oBillmaster.Bills)
                        {
                            if (bitem.billattributes.online)
                            {
                                BillData.SyncEcommerceStock(true, bitem.Billitems, transaction, conn);
                                Processors.Schedule_Actions.Instance.OnChange(BillData.Context.OrganizationID, bitem,
                                    Processors.Schedule_Actions.Actions.ECommerce);
                            }
                            else if (bitem.Billitems.Any(x => x.attributes.isolstock))
                            {
                                var stockAdjustment = bitem.Billitems.FindAll(x => x.attributes.isolstock && x.attributes.newstock < x.attributes.newolstock);
                                if (stockAdjustment.Count > 0)
                                {
                                    BillData.SyncEcommerceStock(false, stockAdjustment, transaction, conn);
                                }

                                Processors.Schedule_Actions.Instance.OnChange(BillData.Context.OrganizationID, bitem,
                                    Processors.Schedule_Actions.Actions.ECommerce);
                            }
                        }
                    }


                    if(Services.ConfigManager.Instance(Context.OrganizationID).Config.system_billnoqueue)
                    {
                        foreach (var bill in oBillmaster.Bills)
                            UpdateBillNo(bill, billnosequence, conn, transaction);
                    }

                    if (oBillmaster.Bills[0].billtype == Bill.BillTypes.ReturnBill)
                    {
                        if (oBillmaster.Bills[0].customerid > 0 && 
                            oBillmaster.Bills.Exists(e => e.Billitems.Exists(a => a.attributes.rewarddiscount > 0)))
                        {
                            decimal toadjust = oBillmaster.Bills.Sum(e => e.Billitems.Sum(a => a.attributes.rewarddiscount));
                            CustomerrewardsDataService customerrewardsDataService = new CustomerrewardsDataService() { Context = this.Context };
                            customerrewardsDataService.Context = this.Context;
                            customerrewardsDataService.ReverseReward(transaction, conn, oBillmaster.Bills[0].customerid, toadjust, oBillmaster.Bills[0].locationid);
                        }
                    }


                    if (oBillmaster.sourcebilltype != Bill.BillTypes.ReturnBill &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportdeliveryrack &&
                        !Services.ConfigManager.Instance(Context.OrganizationID).Config.system_deliveryrackauto &&
                        oBillmaster.billattributes != null && oBillmaster.billattributes.deliveryrackid > 0)
                    {
                        DeliveryrackDataService deliveryrackDataService = new DeliveryrackDataService() { Context = this.Context };
                        var deliveryrack = deliveryrackDataService.UpdateSettlement(oBillmaster, conn, transaction);
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillmaster, Context.UserID, oBillmaster.locationid);
                oBillmaster.code = "";
                oBillmaster.id = 0;
                if (oBillmaster.Bills != null)
                {
                    oBillmaster.Bills.ForEach(e =>
                    {
                        e.id = 0;
                        e.billno = "";
                        e.printdocument = null;
                    });
                }
                oBillmaster.errormessage = exp.Message;
            }

            return oBillmaster;
        }


        public Billmaster InsertBulkBill(Billmaster oBillmaster, string bulkBillNo)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                InfyPOS.Data.DiscountqueueDataService discountqueue = new DiscountqueueDataService() { Context = this.Context };
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("billmaster.code", oBillmaster.locationid);

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    oBillmaster.code = sequence.GetNextNumber(this.Context, conn, transaction);


                    if (oBillmaster.customerid == 0 && !string.IsNullOrEmpty(oBillmaster.customermobileno))
                    {
                        CustomerDataService custdata = new CustomerDataService() { Context = this.Context };
                        var cust = new Models.Customer();
                        cust.name = string.IsNullOrEmpty(oBillmaster.customername) ? "Customer" : oBillmaster.customername;
                        cust.mobileno = oBillmaster.customermobileno;
                        cust.registredlocationid = oBillmaster.locationid;
                        cust.isactive = true;
                        cust.version = 1;
                        cust.createdon = DateTimeNow;
                        cust.customertype = 4;
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig != null &&
                            InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(oBillmaster.locationid))
                            cust.categoryid = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[oBillmaster.locationid].sale_defaultcustomercategory;

                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcustomer10digits &&
                            cust.mobileno.Length != 10)
                        {
                            throw new Exception("Please enter 10 digits customer contact no. " + oBillmaster.customermobileno);
                        }

                        if (custdata.DuplicateValidation(cust) > 0)
                        {
                            throw new Exception("Mobileno already exists. please check. " + oBillmaster.customermobileno);
                        }

                        custdata.Insert(transaction, conn, cust);
                        oBillmaster.customerid = cust.id;
                        oBillmaster.Bills.ForEach(e => { e.customerid = cust.id; e.customer = cust; });
                    }

                    if (oBillmaster.Bills.First().billtype == Bill.BillTypes.Estimate &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportdeliveryrack)
                    {
                        DeliveryrackDataService deliveryrackDataService = new DeliveryrackDataService() { Context = this.Context };
                        deliveryrackDataService.UpdateFreeRack(oBillmaster, conn, transaction);
                    }

                    Insert(transaction, conn, oBillmaster);
                    BulkBillDataService BillData = new BulkBillDataService() { Context = this.Context };
                    BillData.Context = this.Context;
                    //if (oBillmaster.customerid == 0 && !string.IsNullOrEmpty(oBillmaster.customermobileno) 
                    //    && !string.IsNullOrEmpty(oBillmaster.customername))
                    //{
                    //    CustomerDataService custdata = new CustomerDataService() { Context = this.Context };
                    //    var cust = new Models.Customer();
                    //    cust.name = oBillmaster.customername;
                    //    cust.mobileno = oBillmaster.customermobileno;
                    //    cust.isactive = true;
                    //    cust.createdon = DateTimeNow;
                    //    custdata.Insert(cust);
                    //    oBillmaster.Bills.ForEach(e => e.customerid = cust.id);
                    //}


                    oBillmaster.Bills.ForEach(e => e.billmasterid = oBillmaster.id);
                    foreach (var bill in oBillmaster.Bills)
                    {
                        if (bill.billattributes == null)
                            bill.billattributes = new BillAttribute();

                        bill.billattributes.counterid = bill.counterid;
                        bill.billattributes.countername = bill.countername;
                        bill.billattributes.floorid = bill.floorid;
                        bill.billattributes.floorname = bill.floorname;
                        bill.billattributes.biller = this.Context != null ? this.Context.EmployeeName : "";
                        bill.areaid = oBillmaster.areaid;

                        SequenceNo billnosequence = null;
                        if (bill.billtype == Bill.BillTypes.Estimate)
                        {
                            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_floorwisebulkbillno)
                            {
                                billnosequence = SequenceManager.Instance(Context.OrganizationID).Get("bulkbill.ebillno", bill.locationid, bill.companyid,
                                    Services.ConfigManager.Instance(Context.OrganizationID).Config.system_floorwisebulkbillno ? bill.floorid : 0);
                            }
                            else
                                billnosequence = SequenceManager.Instance(Context.OrganizationID).Get("bulkbill.ebillno", bill.locationid, bill.companyid);
                        }
                        else
                        {
                            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.system_floorwisebulkbillno)
                            {
                                billnosequence = SequenceManager.Instance(Context.OrganizationID).Get("bulkbill.billno", bill.locationid, bill.companyid,
                                    Services.ConfigManager.Instance(Context.OrganizationID).Config.system_floorwisebulkbillno ? bill.floorid : 0);
                            }
                            else
                                billnosequence = SequenceManager.Instance(Context.OrganizationID).Get("bulkbill.billno", bill.locationid, bill.companyid);
                        }

                        bill.version = 1;
                        bill.billno = billnosequence.GetNextNumber(this.Context, conn, transaction);

                        if (bill.additionaldiscount > 0)
                            bill.discountstatus = Discountqueue.DiscountStatus.Requested;

                        Bill result = bill.total < 0 ? BillData.InsertReturn(transaction, conn, bill) :
                            BillData.Insert(transaction, conn, bill);

                        if (bill.additionaldiscount > 0)
                        {
                            Discountqueue dq = new Discountqueue()
                            {
                                billid = bill.id,
                                notes = bill.discountrequestreason,
                                requestedamount = bill.additionaldiscount,
                                requestedby = bill.createdby,
                                requestedon = bill.billdate,
                                billamount = bill.receivable,
                                isactive = true,
                                status = Discountqueue.DiscountStatus.Requested,
                                billno = bill.billno,
                                companyid = bill.companyid
                            };
                            discountqueue.Insert(transaction, conn, dq);
                        }
                    }

                    List<Bill> cancelledestimates = new List<Bill>() ;
                    if (!string.IsNullOrEmpty(bulkBillNo) && oBillmaster.Bills.Count > 0
                        && oBillmaster.Bills[0].billtype == Bill.BillTypes.Estimate)
                    {
                        if (oBillmaster.Bills.First().billtype == Bill.BillTypes.Estimate &&
                                Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportdeliveryrack)
                        {
                            cancelledestimates = BillData.Search(new BillCreteria() { 
                                billno = bulkBillNo, 
                                locationid = oBillmaster.Bills[0].locationid 
                            });
                        }
                        
                        if (Context.HasPermission(PermissionSet.estimatesale, InfyPOS.Common.Action.Delete))
                        {
                            var cancelquery = @"Update bulkbill set billtype=9,modifiedby=@modifiedby,modifiedon=@modifiedon where billtype=20 and billno=@billno";

                            NpgsqlCommand cancelcmd = GetCommand(cancelquery, conn, transaction);
                            cancelcmd.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = bulkBillNo;
                            cancelcmd.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            cancelcmd.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            cancelcmd.ExecuteNonQuery();
                        }
                    }

                    if (oBillmaster.Bills.First().billtype == Bill.BillTypes.Estimate &&
                        Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportdeliveryrack)
                    {
                        DeliveryrackDataService deliveryrackDataService = new DeliveryrackDataService() { Context = this.Context };
                        deliveryrackDataService.AssignRack(oBillmaster, conn, transaction, cancelledestimates);
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error,oBillmaster, Context.UserID, oBillmaster.locationid);
                oBillmaster.code = "";
                oBillmaster.id = 0;
                if (oBillmaster.Bills != null)
                {
                    oBillmaster.Bills.ForEach(e =>
                    {
                        e.id = 0;
                        e.billno = "";
                        e.printdocument = null;
                    });
                }
                oBillmaster.errormessage = exp.Message;
            }

            return oBillmaster;
        }


        public Billmaster Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Billmaster oBillmaster)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillmaster.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillmaster.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("billdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillmaster.billdate;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillmaster.customerid;

            command.Parameters.Add("sourcebilltype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillmaster.sourcebilltype;

            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillmaster.locationid;

            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillmaster.code;

            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillmaster.tax;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillmaster.discount;
            command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillmaster.total;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillmaster.receivable;
            command.Parameters.Add("other", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillmaster.other;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillmaster.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oBillmaster;
        }

        public Billmaster Update(Billmaster oBillmaster)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBillmaster);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace, Context.UserID, oBillmaster.locationid);
            }

            return oBillmaster;
        }

        public Billmaster Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Billmaster oBillmaster)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillmaster.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillmaster.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillmaster.createdby;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillmaster.locationid;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillmaster.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillmaster.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("billdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillmaster.billdate;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillmaster.customerid;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillmaster.tax;
            command.Parameters.Add("discount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillmaster.discount;
            command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillmaster.total;
            command.Parameters.Add("receivable", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillmaster.receivable;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillmaster.code;
            command.Parameters.Add("other", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillmaster.other;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillmaster.version = Convert.ToInt32(reader[0]);
                }
            }

            return oBillmaster;
        }

        public bool Delete(Billmaster oBillmaster)
        {

            NpgsqlTransaction transaction = null;
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
                    command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillmaster.isactive;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillmaster.version;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillmaster.id;
                    successCount = command.ExecuteNonQuery();
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace, Context.UserID, oBillmaster.locationid);
            }
            return successCount > 0;
        }

    }

}
