using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using InfyGST;
using Newtonsoft.Json;
using InfyPOS.Services;
using InfyPOS.Processors.Migration;

namespace InfyPOS.Data
{
    public class BillinboxDataService : SAASData
    {
        const string SQL_Search = @"select billinbox.id, billinbox.isactive, billinbox.createdby, billinbox.modifiedby, billinbox.version,
	billinbox.createdon,billinbox.modifiedon, loadingcharges, frightcharges, noofbundles, noofpieces, transportid, lrno, lrdate,
    tolocationid, billid, billinbox.billno,(bill.receivable+bill.rounding) billvalue,bill.qty billqty, billinbox.additionalcharges, packedby,
    packedon, billinbox.status, bill.customerid, stitchedby,stitchededon,einvoicejosn->>'AckNo' ackno,bill.locationid billlocationid,
    customer.name customername, transportname,vehicleno , einvoicejosn ,gstexception ,ewayjson , haseinvoice,haseway,einvoice,ewb,
    tolocation, tocity.name tocityname, billinbox.address,bill.billdate,billinbox.modeofdelivery,billimg,orderby,remark,billinbox.hascancel,
	billattributes->>'remarks' billremarks,billattributes->>'orderrefid' billsono,billattributes->>'shiptoid' billshipto,
    bill.billtype,bill.companyid,handedtoid,handedoveron
    from bill
    inner join billinbox on billinbox.billid = bill.id and billinbox.isactive=true
    left outer join customer on customer.id = bill.customerid 
    left outer join referencelist tocity on tocity.id = billinbox.tolocationid
    left outer join salesorder on salesorder.sono = billattributes->>'orderrefid'
    where billinbox.id=@id";

        const string SQL_DeliveryDetails = @"select billinbox.loadingcharges, frightcharges, noofbundles, noofpieces, transportid, lrno, lrdate, tolocationid, transportid,
                    transport.name transportname,haseinvoice,haseway,transport.gst transportgst,handedtoid,handedoveron from billinbox 
                    left outer join transport on transport.id = billinbox.transportid where billid=@billid";

        const string SQL_Insert = @"INSERT INTO public.billinbox(isactive, createdby, modifiedby, version, createdon, modifiedon, loadingcharges, frightcharges, noofbundles, noofpieces, 
        transportid, lrno, lrdate, tolocationid, billid, billno, billvalue, additionalcharges, packedby,packedon, status, customerid, customername, transportname, tolocation, 
        address,billdate,modeofdelivery,einvoice,ewb,billimg,orderby,remark,vehicleno , einvoicejosn ,gstexception ,ewayjson , haseinvoice,haseway,stitchedby,stitchededon) 
        VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @loadingcharges, @frightcharges, @noofbundles, @noofpieces, @transportid, @lrno, @lrdate, @tolocationid, 
        @billid, @billno, @billvalue, @additionalcharges, @packedby,@packedon, @status, @customerid, @customername, @transportname, @tolocation, @address,@billdate,@modeofdelivery,@einvoice,@ewb,
        @billimg,@orderby,@remark,@vehicleno , @einvoicejosn ,@gstexception ,@ewayjson , @haseinvoice,@haseway,@stitchedby,@stitchededon) RETURNING id";



        const string SQL_Update = @"UPDATE public.billinbox SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, loadingcharges=@loadingcharges, frightcharges=@frightcharges, noofbundles=@noofbundles, 
        noofpieces=@noofpieces, transportid=@transportid, lrno=@lrno, lrdate=@lrdate, tolocationid=@tolocationid, billid=@billid, billno=@billno, billvalue=@billvalue, handedtoid=@handedtoid,handedoveron=@handedoveron,
        additionalcharges=@additionalcharges, packedby=@packedby,packedon=@packedon, status=@status, customerid=@customerid, customername=@customername, transportname=@transportname, 
        tolocation=@tolocation, address=@address,modeofdelivery=@modeofdelivery,einvoice=@einvoice,ewb=@ewb,billimg=@billimg,orderby=@orderby,remark=@remark,stitchedby=@stitchedby,stitchededon=@stitchededon,
        vehicleno=@vehicleno
        where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.billinbox set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Billinbox> Search(BillinboxCreteria _billinbox)
        {
            List<Billinbox> lst = new List<Billinbox>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_billinbox.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billinbox.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("bill.isactive", true, "=");
                    if (_billinbox.billid > 0)
                    {
                        qb.AddParameter("billid", _billinbox.billid, "=");
                    }
                    else
                    {
                        if (_billinbox.status == 10)
                        {
                            if (_billinbox.transportid >0)
                                qb.AddParameter("billinbox.transportid", _billinbox.transportid, "=");
                            if (_billinbox.handedtoid > 0)
                                qb.AddParameter("billinbox.handedtoid", _billinbox.handedtoid, "=");
                            else
                                qb.AddParameter("billinbox.handedtoid", 0, ">");
                        }
                        else
                        {
                            if (!string.IsNullOrEmpty(_billinbox.billno))
                                qb.AddParameter("billinbox.billno", _billinbox.billno, "=");

                            if (_billinbox.status != 1)
                            {
                                if (_billinbox.status == -9)
                                    qb.AddParameter("bill.billtype", 9, "=");
                                else
                                {
                                    qb.AddParameter("billinbox.isactive", true, "=");
                                    qb.AddParameter("bill.billtype", 9, "!=");
                                }
                            }
                            if (_billinbox.status >= 0)
                                qb.AddParameter("billinbox.status", _billinbox.status, "=");
                            if (_billinbox.typeid >= 0)
                                qb.AddParameter("modeofdelivery", _billinbox.typeid, "=");
                        }


                        if (string.IsNullOrEmpty(_billinbox.billno))
                        {
                            if (_billinbox.addedfrom.Year < 2000 && _billinbox.addedto.Year < 2000)
                            {
                                qb.AddParameter("bill.billdate", "addedfrom", DateTimeNow.Date.AddDays(-10), ">=");
                            }
                            else
                            {
                                if (_billinbox.addedfrom.Year > 1900)
                                    qb.AddParameter("bill.billdate", "addedfrom", _billinbox.addedfrom.Date, ">=");
                                if (_billinbox.addedto.Year > 1900)
                                    qb.AddParameter("bill.billdate", "addedto", _billinbox.addedto.Date.AddDays(1), "<");
                            }

                            if (_billinbox.customerid > 0)
                            {
                                qb.AddParameter("bill.customerid", _billinbox.customerid, "=");
                            }
                            if (_billinbox.billlocationid > 0)
                            {
                                qb.AddParameter("bill.locationid", _billinbox.billlocationid, "=");
                            }
                            if (_billinbox.agentid > 0)
                            {
                                qb.AddParameter("salesorder.agentid", _billinbox.agentid, "=");
                            }
                        }
                    }


                    if (Context.HasCustomerRestricted)
                    {
                        qb.Add2Query(@"bill.customerid in (SELECT itemlist::text::bigint AS customer_id
                        FROM users
                        CROSS JOIN LATERAL json_array_elements(users.accessiblecustomer::json) AS itemlist
                        WHERE id = @userid) AND customer.isactive = true ");
                    }

                    qb.SortField = "id desc";
                    command = qb.GetCommand(conn);
                    command.Parameters.Add("userid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Billinbox obillinbox = new Billinbox();
                        obillinbox.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        obillinbox.modeofdelivery = reader["modeofdelivery"] == DBNull.Value ? 0 : Convert.ToInt32(reader["modeofdelivery"]);
                        obillinbox.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        obillinbox.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        obillinbox.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        obillinbox.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        obillinbox.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        obillinbox.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        obillinbox.loadingcharges = reader["loadingcharges"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loadingcharges"]);
                        obillinbox.frightcharges = reader["frightcharges"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["frightcharges"]);
                        obillinbox.noofbundles = reader["noofbundles"] == DBNull.Value ? 0 : Convert.ToInt64(reader["noofbundles"]);
                        obillinbox.noofpieces = reader["noofpieces"] == DBNull.Value ? 0 : Convert.ToInt64(reader["noofpieces"]);
                        obillinbox.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"]);
                        obillinbox.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        obillinbox.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"]);
                        obillinbox.billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                        obillinbox.tolocationid = reader["tolocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tolocationid"]);
                        obillinbox.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        obillinbox.billid = reader["billid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billid"]);
                        obillinbox.billlocationid = reader["billlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billlocationid"]);

                        obillinbox.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();

                        obillinbox.billremarks = reader["billremarks"] == DBNull.Value ? "" : reader["billremarks"].ToString();
                        obillinbox.billsono = reader["billsono"] == DBNull.Value ? "" : reader["billsono"].ToString();
                        obillinbox.billshipto = reader["billshipto"] == DBNull.Value ? 0 : Convert.ToInt64(reader["billshipto"]);

                        obillinbox.einvoice = reader["einvoice"] == DBNull.Value ? "" : reader["einvoice"].ToString();
                        obillinbox.ewb = reader["ewb"] == DBNull.Value ? "" : reader["ewb"].ToString();
                        obillinbox.ackno = reader["ackno"] == DBNull.Value ? "" : reader["ackno"].ToString();
                        obillinbox.billimg = reader["billimg"] == DBNull.Value ? "" : reader["billimg"].ToString();

                        obillinbox.remark = reader["remark"] == DBNull.Value ? "" : reader["remark"].ToString();
                        obillinbox.orderby = reader["orderby"] == DBNull.Value ? "" : reader["orderby"].ToString();
                        obillinbox.hascancel = reader["hascancel"] == DBNull.Value ? false : Convert.ToBoolean(reader["hascancel"]);

                        if (reader["billtype"] != DBNull.Value && Convert.ToInt32(reader["billtype"]) == 9)
                        {
                            obillinbox.hascancel = true;
                        }
                        obillinbox.billvalue = reader["billvalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billvalue"]);
                        obillinbox.billqty = reader["billqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["billqty"]);
                        obillinbox.additionalcharges = reader["additionalcharges"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionalcharges"]);
                        obillinbox.packedby = reader["packedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["packedby"]);
                        obillinbox.packedon = reader["packedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["packedon"]);

                        obillinbox.handedtoid = reader["handedtoid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["handedtoid"]);
                        obillinbox.handedoveron = reader["handedoveron"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["handedoveron"]);

                        obillinbox.stitchedby = reader["stitchedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stitchedby"]);
                        obillinbox.stitchededon = reader["stitchededon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["stitchededon"]);

                        obillinbox.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        obillinbox.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        obillinbox.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        obillinbox.transportname = reader["transportname"] == DBNull.Value ? "" : reader["transportname"].ToString();
                        obillinbox.tolocation = reader["tolocation"] == DBNull.Value ? "" : reader["tolocation"].ToString();
                        obillinbox.tocityname = reader["tocityname"] == DBNull.Value ? "" : reader["tocityname"].ToString();
                        obillinbox.address_json = reader["address"] == DBNull.Value ? null : reader["address"].ToString();


                        obillinbox.vehicleno = reader["vehicleno"] == DBNull.Value ? "" : reader["vehicleno"].ToString();

                        obillinbox.gstexception = reader["gstexception"] == DBNull.Value ? "" : reader["gstexception"].ToString();
                        obillinbox.haseinvoice = reader["haseinvoice"] == DBNull.Value ? false : Convert.ToBoolean(reader["haseinvoice"]);
                        obillinbox.haseway = reader["haseway"] == DBNull.Value ? false : Convert.ToBoolean(reader["haseway"]);
                        if (_billinbox.billid > 0 || _billinbox.id > 0)
                        {
                            obillinbox.ewayjson = reader["ewayjson"] == DBNull.Value ? "" : reader["ewayjson"].ToString();
                            obillinbox.einvoicejosn = reader["einvoicejosn"] == DBNull.Value ? "" : reader["einvoicejosn"].ToString();
                        }
                        // Cancelled invoice indication
                        if (obillinbox.isactive == false || _billinbox.status == -9 || obillinbox.hascancel)
                        {
                            obillinbox.status = -9;
                        }


                        lst.Add(obillinbox);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _billinbox);
            }
            return lst;
        }

        internal string ChangeBillNo(BillNoChange item)
        {
            string result = "Bill No " + item.bill.billno + " changed to " + item.newbillno;
            NpgsqlTransaction transaction = null;
            try
            {

                var param = new SortedList<string, object>();
                param.Add("billno", item.newbillno);
                long existbillid = 0;
                int billtype = 0;
                bool isactive = true;
                bool duplicatebill = false;
                using (var reader = ExecuteReader("Select id,isactive,billtype from bill where billno=@billno", param))
                {
                    if (reader.Read())
                    {
                        existbillid = reader.GetInt64(0);
                        billtype = reader.GetInt32(2);
                        isactive = reader.GetBoolean(1);
                    }
                }

                if (existbillid > 0)
                {
                    if (isactive && billtype != 9)
                    {
                        return item.newbillno + " already exist can't change";
                    }
                    int version = 1;
                    var newbillno = item.newbillno + "_" + version.ToString("F0");
                    while (true)
                    {
                        newbillno = item.newbillno + "_" + version.ToString("F0");
                        param["billno"] = newbillno;
                        using (var reader = ExecuteReader("Select id,isactive,billtype from bill where billno=@billno", param))
                        {
                            if (!reader.Read())
                            {
                                break;
                            }
                        }
                    }
                    param["billno"] = item.newbillno;
                    param.Add("newbillno", newbillno);
                    duplicatebill = true;
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (duplicatebill)
                        ExecuteQuery("update bill set billno=@newbillno where billno=@billno and isactive=true", param, conn, transaction);


                    BillDataService billservice = new BillDataService() { Context = this.Context };
                    var oBill = billservice.Search(new BillCreteria()
                    {
                        id = item.bill.billid
                    }).First();
                    string billsequence = "bill.billno";
                    bool locationwisebillno = true;
                    int retry = 0;
                    while (retry < 3)
                    {
                        retry++;
                        var sequence = SequenceManager.Instance(Context.OrganizationID).Get(billsequence, locationwisebillno ? oBill.locationid : 0, oBill.companyid,
                  Services.ConfigManager.Instance(Context.OrganizationID).Config.system_salefloorwisebillno ? oBill.floorid : 0);
                        var currentbillno = sequence.GetCurrentNumber(false, item.bill.billdate, conn, transaction);
                        if (currentbillno == oBill.billno)
                        {
                            int maxtry = 100;
                            long replaceno = 0;
                            List<string> billnos = new List<string>();
                            string query = "select billno from bill where isactive = true and billtype !=9 order by id desc limit 100";
                            using (var reader = ExecuteReader(query, null, conn, transaction))
                            {
                                while (reader.Read())
                                    billnos.Add(reader.GetString(0));
                            }

                            for (int i = 0; i > (maxtry * -1); i--)
                            {
                                var existno = sequence.GetFormatedNumber(sequence.CurrentSlotID + i, item.bill.billdate);
                                if (!billnos.Contains(existno))
                                {
                                    replaceno = sequence.CurrentSlotID + i;
                                    for (; i > (maxtry * -1); i--)
                                    {
                                        existno = sequence.GetFormatedNumber(sequence.CurrentSlotID + i, item.bill.billdate);
                                        if (billnos.Contains(existno))
                                        {
                                            replaceno = sequence.CurrentSlotID + i;
                                            break;
                                        }
                                    }
                                    break;
                                }
                            }
                            if (replaceno > 0)
                            {
                                if (sequence.ChangeCurrentNo(replaceno, item.bill.billdate, conn, transaction))
                                    break;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }

                    param.Clear();
                    param.Add("id", item.bill.billid);
                    param.Add("newbillno", item.newbillno);
                    ExecuteQuery(@"update bill set billno=@newbillno where id=@id  and isactive=true;
                                   update billinbox set billno=@newbillno where billid=@id;", param, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                return exp.Message;
            }
            return result;
        }

        public List<BillDeliveryDetails> GetBillDeliveryDetails(BillinboxCreteria _billinbox)
        {
            List<BillDeliveryDetails> lst = new List<BillDeliveryDetails>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                command = new NpgsqlCommand(SQL_DeliveryDetails, conn);
                command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _billinbox.billid;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BillDeliveryDetails obillinbox = new BillDeliveryDetails();
                        obillinbox.loadingcharges = reader["loadingcharges"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loadingcharges"]);
                        obillinbox.frightcharges = reader["frightcharges"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["frightcharges"]);
                        obillinbox.noofbundles = reader["noofbundles"] == DBNull.Value ? 0 : Convert.ToInt64(reader["noofbundles"]);
                        obillinbox.noofpieces = reader["noofpieces"] == DBNull.Value ? 0 : Convert.ToInt64(reader["noofpieces"]);
                        obillinbox.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"]);
                        obillinbox.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        obillinbox.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"]);
                        obillinbox.transportname = reader["transportname"] == DBNull.Value ? "" : reader["transportname"].ToString();
                        obillinbox.transportgst = reader["transportgst"] == DBNull.Value ? "" : reader["transportgst"].ToString();
                        obillinbox.haseinvoice = reader["haseinvoice"] == DBNull.Value ? false : Convert.ToBoolean(reader["haseinvoice"]);
                        obillinbox.haseway = reader["haseway"] == DBNull.Value ? false : Convert.ToBoolean(reader["haseway"]);
                        lst.Add(obillinbox);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _billinbox);
            }
            if (lst != null && lst.Count > 0 && _billinbox.billid > 0)
            {
                lst[0].billinbox = Search(new BillinboxCreteria()
                {
                    billid = _billinbox.billid,
                }).First();
            }
            return lst;
        }

        public List<BillinboxStatus> ChangeStatus(List<BillinboxStatus> lstsource)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    string query = "Update billinbox set status=@status where id=@id";
                    var command = GetCommand(query, conn, transaction);
                    command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = 0;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = 0;
                    foreach (var entity in lstsource)
                    {
                        command.Parameters["status"].Value = entity.status;
                        command.Parameters["id"].Value = entity.id;
                        command.ExecuteNonQuery();
                        if (entity.status == 2)
                        {
                            BillDataService billDataService = new BillDataService() { Context = this.Context };
                            billDataService.UpdateCreditApproval(conn, transaction, entity);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, lstsource);
            }
            return null;
        }

        public Billinbox Insert(Billinbox oBillinbox)
        {
            if (oBillinbox.billid == 0)
            {
                if (!string.IsNullOrEmpty(oBillinbox.billno))
                {
                    var exist = Search(new BillinboxCreteria()
                    {
                        billno = oBillinbox.billno
                    });

                    if (exist.Count > 0) return exist[0];

                    BillDataService billDataService = new BillDataService() { Context = this.Context };
                    var bills = billDataService.Search(new BillCreteria()
                    {
                        billno = oBillinbox.billno,
                        isimport = true,
                        removecancelled = true
                    });

                    var oBill = bills.First();
                    if (oBill.customerid > 0)
                    {

                        CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
                        var customer = customerDataService.Search(new CustomerCreteria()
                        {
                            id = oBill.customerid
                        }).First();


                        Billinbox billinbox = new Billinbox();
                        billinbox.billid = oBill.id;
                        billinbox.billno = oBill.billno;
                        billinbox.billvalue = oBill.total;
                        billinbox.billdate = oBill.billdate;
                        billinbox.additionalcharges = oBill.additionalcharges;
                        billinbox.createdby = oBill.createdby;
                        billinbox.createdon = oBill.createdon;
                        billinbox.customerid = oBill.customerid;
                        billinbox.customername = customer.name;
                        billinbox.transportid = customer.preferredtransportid;
                        billinbox.modeofdelivery = 0;
                        billinbox.isactive = true;
                        billinbox.version = 1;
                        billinbox.tolocation = customer.cityname;
                        billinbox.tolocationid = customer.cityid;
                        if (oBill.billattributes != null)
                        {
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
                        }
                        billinbox.address = new Billinbox.Address();
                        billinbox.address.detail = customer.address;
                        billinbox.address.area = customer.area;
                        oBillinbox = billinbox;

                        if (oBill.billattributes != null && oBill.billattributes.shiptoid > 0)
                        {
                            customer = customerDataService.Search(new CustomerCreteria()
                            {
                                id = oBill.billattributes.shiptoid
                            }).First();
                            billinbox.address.detail = customer.address;
                            billinbox.address.area = customer.area;
                            billinbox.transportid = customer.preferredtransportid;
                            billinbox.tolocation = customer.cityname;
                            billinbox.tolocationid = customer.cityid;
                        }

                    }
                }
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oBillinbox);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillinbox);
            }

            return oBillinbox;
        }

        public Billinbox Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Billinbox oBillinbox)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillinbox.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillinbox.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("loadingcharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillinbox.loadingcharges;
            command.Parameters.Add("frightcharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillinbox.frightcharges;
            command.Parameters.Add("noofbundles", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.noofbundles;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.noofpieces;
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.transportid;
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.lrno == null ? "" : oBillinbox.lrno;

            command.Parameters.Add("einvoice", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.einvoice == null ? "" : oBillinbox.einvoice;
            command.Parameters.Add("ewb", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.ewb == null ? "" : oBillinbox.ewb;
            command.Parameters.Add("billimg", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.billimg == null ? "" : oBillinbox.billimg;

            command.Parameters.Add("remark", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.remark == null ? "" : oBillinbox.remark;
            command.Parameters.Add("orderby", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.orderby == null ? "" : oBillinbox.orderby;

            command.Parameters.Add("lrdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillinbox.lrdate;
            command.Parameters.Add("billdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillinbox.billdate;
            command.Parameters.Add("tolocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.tolocationid;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.billid;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.billno == null ? "" : oBillinbox.billno;
            command.Parameters.Add("billvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillinbox.billvalue;
            command.Parameters.Add("additionalcharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillinbox.additionalcharges;
            command.Parameters.Add("packedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.packedby;
            command.Parameters.Add("packedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillinbox.packedon;

            command.Parameters.Add("stitchedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.stitchedby;
            command.Parameters.Add("stitchededon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillinbox.stitchededon;

            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillinbox.status;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.customerid;
            command.Parameters.Add("customername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.customername == null ? "" : oBillinbox.customername;
            command.Parameters.Add("transportname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.transportname == null ? "" : oBillinbox.transportname;
            command.Parameters.Add("tolocation", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.tolocation == null ? "" : oBillinbox.tolocation;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillinbox.address_json == null ? "null" : oBillinbox.address_json;
            command.Parameters.Add("modeofdelivery", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillinbox.modeofdelivery;


            command.Parameters.Add("vehicleno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.vehicleno == null ? "" : oBillinbox.vehicleno;
            command.Parameters.Add("einvoicejosn", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillinbox.einvoicejosn == null ? "null" : oBillinbox.einvoicejosn;
            command.Parameters.Add("gstexception", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillinbox.gstexception == null ? "null" : oBillinbox.gstexception;
            command.Parameters.Add("ewayjson", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillinbox.ewayjson == null ? "null" : oBillinbox.ewayjson;
            command.Parameters.Add("haseinvoice", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillinbox.haseinvoice;
            command.Parameters.Add("haseway", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillinbox.haseway;


            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillinbox.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oBillinbox;
        }

        public Billinbox Update(Billinbox oBillinbox)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBillinbox);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillinbox);
            }

            return oBillinbox;
        }

        public bool UpdateBillValue(NpgsqlTransaction transaction, NpgsqlConnection conn, long billid, decimal value)
        {
            NpgsqlCommand command = new NpgsqlCommand("update billinbox set billvalue=@billvalue where billid=@billid RETURNING haseway,haseinvoice", conn, transaction);
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = billid;
            command.Parameters.Add("billvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = value;
            using (var reader = command.ExecuteReader())
            {
                if(reader.Read())
                {
                    if(reader["haseway"] != DBNull.Value &&
                       Convert.ToBoolean(reader["haseway"]))
                    {
                        throw new Exception("E-Way generated bill can't be updated");
                    }
                    if (reader["haseinvoice"] != DBNull.Value &&
                        Convert.ToBoolean(reader["haseinvoice"]))
                    {
                        throw new Exception("E-Invoice generated bill can't be updated");
                    }
                }
            }
            return true;
        }
        public Billinbox Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Billinbox oBillinbox)
        {
            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillinbox.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillinbox.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillinbox.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("loadingcharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillinbox.loadingcharges;
            command.Parameters.Add("frightcharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillinbox.frightcharges;
            command.Parameters.Add("noofbundles", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.noofbundles;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.noofpieces;
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.transportid;
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.lrno == null ? "" : oBillinbox.lrno;
            command.Parameters.Add("einvoice", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.einvoice == null ? "" : oBillinbox.einvoice;
            command.Parameters.Add("ewb", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.ewb == null ? "" : oBillinbox.ewb;
            command.Parameters.Add("billimg", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.billimg == null ? "" : oBillinbox.billimg;
            command.Parameters.Add("remark", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.remark == null ? "" : oBillinbox.remark;
            command.Parameters.Add("orderby", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.orderby == null ? "" : oBillinbox.orderby;

            command.Parameters.Add("lrdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillinbox.lrdate;
            command.Parameters.Add("tolocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.tolocationid;
            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.billid;
            command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.billno == null ? "" : oBillinbox.billno;
            command.Parameters.Add("billvalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillinbox.billvalue;
            command.Parameters.Add("additionalcharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBillinbox.additionalcharges;
            command.Parameters.Add("packedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.packedby;
            command.Parameters.Add("packedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillinbox.packedon;
            command.Parameters.Add("stitchedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.stitchedby;
            command.Parameters.Add("stitchededon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillinbox.stitchededon;

            command.Parameters.Add("handedtoid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.handedtoid;
            command.Parameters.Add("handedoveron", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBillinbox.handedoveron;

            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillinbox.status;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.customerid;
            command.Parameters.Add("customername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.customername == null ? "" : oBillinbox.customername;
            command.Parameters.Add("transportname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.transportname == null ? "" : oBillinbox.transportname;
            command.Parameters.Add("tolocation", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.tolocation == null ? "" : oBillinbox.tolocation;
            command.Parameters.Add("address", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillinbox.address_json == null ? "null" : oBillinbox.address_json;
            command.Parameters.Add("modeofdelivery", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillinbox.modeofdelivery;
            command.Parameters.Add("vehicleno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBillinbox.vehicleno == null ? "" : oBillinbox.vehicleno;
            command.Parameters.Add("einvoicejosn", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillinbox.einvoicejosn == null ? "null" : oBillinbox.einvoicejosn;
            command.Parameters.Add("gstexception", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillinbox.gstexception == null ? "null" : oBillinbox.gstexception;
            command.Parameters.Add("ewayjson", NpgsqlTypes.NpgsqlDbType.Json).Value = oBillinbox.ewayjson == null ? "null" : oBillinbox.ewayjson;
            command.Parameters.Add("haseinvoice", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillinbox.haseinvoice;
            command.Parameters.Add("haseway", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBillinbox.haseway;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBillinbox.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oBillinbox;
        }


        public bool Delete(Billinbox oBillinbox)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oBillinbox);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBillinbox);
            }
            return hasSuccessCount;
        }

        internal InfyGST.GSTPortal.EInvoiceResponse GenerateEInvoice(BillinboxCreteria item)
        {
            Gst_InboxDataService gst_InboxDataService = new Gst_InboxDataService() { Context = this.Context };
            InfyGST.GSTPortal.EInvoiceResponse response = null;
            if (item.mode == "INV" || item.mode == "INVEW")
            {
                response = gst_InboxDataService.CreateEInvoice4Bill(new GST_Invoice()
                {
                    billid = item.billid,
                    billinboxid = item.id,
                    transportid = item.transportid,
                    hsn4digit = item.hsn4digit,
                    hsn8digit = item.hsn8digit,
                    hsn6digit = item.hsn6digit
                });

                if (response.ErrorDetails == null || response.ErrorDetails.Count == 0)
                {
                    if (!string.IsNullOrEmpty(response.Irn))
                    {
                        string query = @"update billinbox set einvoicejosn=@einvoicejosn,einvoice=@einvoice,ewb=@ewb,haseinvoice=true,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id;";

                        if(!response.isb2b)
                        {
                            query += "\r\nupdate bill set isb2b=true where id=@billid;";
                        }

                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            var transaction = conn.BeginTransaction();
                            Update(transaction, conn, item);
                            NpgsqlCommand command = new NpgsqlCommand(query, conn);
                            command.Parameters.Add("einvoicejosn", NpgsqlTypes.NpgsqlDbType.Json).Value = response == null ? "null" : Newtonsoft.Json.JsonConvert.SerializeObject(response);
                            command.Parameters.Add("einvoice", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(response.Irn) ? "" : response.Irn;
                            command.Parameters.Add("ewb", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(response.EwbNo) ? "" : response.EwbNo;
                            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                            command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.billid;
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            conn.Close();
                        }
                        response.BillInbox = Search(new BillinboxCreteria() { id = item.id }).First();
                    }
                }

                if (item.mode == "INVEW")
                {
                    var ewayresonse = gst_InboxDataService.CreateEWay4IRN(new GST_Invoice()
                    {
                        billid = item.billid,
                        billinboxid = item.id,
                        transportid = item.transportid,
                        vehcicleno = item.vehicleno,
                        distance = item.distance,
                        modeofdelivery = item.modeofdelivery
                    });
                    if (ewayresonse.ErrorDetails == null ||
                        ewayresonse.ErrorDetails.Count == 0)
                    {
                        if (!string.IsNullOrEmpty(response.Irn))
                        {
                            string query = @"update billinbox set ewb=@ewb,haseway=true,ewayjson=@ewayjson,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id;  
                                             update bill set isb2b=true where id=@billid";
                            using (NpgsqlConnection conn = CreateConnection())
                            {
                                conn.Open();
                                var transaction = conn.BeginTransaction();
                                Update(transaction, conn, item);
                                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                                command.Parameters.Add("ewayjson", NpgsqlTypes.NpgsqlDbType.Json).Value = ewayresonse.raw;
                                command.Parameters.Add("ewb", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(ewayresonse.Eway.EwbNo) ? "" : ewayresonse.Eway.EwbNo;
                                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                                command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.billid;
                                command.ExecuteNonQuery();
                                transaction.Commit();
                                conn.Close();
                            }
                            response.BillInbox = Search(new BillinboxCreteria() { billid = item.billid }).First();
                        }
                    }
                }
            }
            else if (item.mode == "EWIRN" || item.mode == "EW")
            {
                var ewayresonse = gst_InboxDataService.CreateEWay4IRN(new GST_Invoice()
                {
                    billid = item.billid,
                    billinboxid = item.id,
                    transportid = item.transportid,
                    vehcicleno = item.vehicleno,
                    distance = item.distance,
                    modeofdelivery = item.modeofdelivery
                });
                if (ewayresonse.ErrorDetails == null ||
                    ewayresonse.ErrorDetails.Count == 0)
                {
                    if (ewayresonse.Eway != null && ewayresonse.Eway.EwbNo != null)
                    {
                        string query = @"update billinbox set ewb=@ewb,haseway=true,ewayjson=@ewayjson,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id";
                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            var transaction = conn.BeginTransaction();
                            Update(transaction, conn, item);
                            NpgsqlCommand command = new NpgsqlCommand(query, conn);
                            command.Parameters.Add("ewayjson", NpgsqlTypes.NpgsqlDbType.Json).Value = ewayresonse.raw;
                            command.Parameters.Add("ewb", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(ewayresonse.Eway.EwbNo) ? "" : ewayresonse.Eway.EwbNo;
                            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                            command.ExecuteNonQuery();
                            transaction.Commit();
                            conn.Close();
                        }
                        response = new GSTPortal.EInvoiceResponse();
                        response.BillInbox = Search(new BillinboxCreteria() { billid = item.billid }).First();
                    }
                }
                else
                {
                    response = new GSTPortal.EInvoiceResponse();
                    response.ErrorDetails = ewayresonse.ErrorDetails.ConvertAll(x => new GSTPortal.EInvoiceResponse.ErrorDetail()
                    {
                        ErrorCode = x.ErrorCode,
                        ErrorMessage = x.ErrorMessage
                    });
                }
            }

            return response;
        }
        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Billinbox oBillinbox)
        {
            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBillinbox.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBillinbox.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal GSTPortal.EWAYbillResponse GenerateEWay(BillinboxCreteria item)
        {
            Gst_InboxDataService gst_InboxDataService = new Gst_InboxDataService() { Context = this.Context };
            var response = gst_InboxDataService.CreateEWay4Bill(new GST_Invoice()
            {
                billid = item.billid,
                billinboxid = item.id,
                transportid = item.transportid,
                hsn4digit = item.hsn4digit,
                distance = item.distance,
                vehcicleno= item.vehicleno,
                modeofdelivery = item.modeofdelivery
            });

            if (response.ErrorDetails == null || response.ErrorDetails.Count == 0)
            {
                if (response.ewayBillNo > 0)
                {
                    string query = @"update billinbox set ewb=@ewb,haseway=true,ewayjson=@ewayjson,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id";
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        var transaction = conn.BeginTransaction();
                        Update(transaction, conn, item);
                        NpgsqlCommand command = new NpgsqlCommand(query, conn);
                        command.Parameters.Add("ewayjson", NpgsqlTypes.NpgsqlDbType.Json).Value = response.raw;
                        command.Parameters.Add("ewb", NpgsqlTypes.NpgsqlDbType.Varchar).Value = response.ewayBillNo.ToString();
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                        command.ExecuteNonQuery();
                        transaction.Commit();
                        conn.Close();
                    }
                }
            }
            return response;
        }


        internal GSTPortal.EInvoiceResponse CancelEInvoice(BillinboxCreteria item)
        {

            Gst_InboxDataService gst_InboxDataService = new Gst_InboxDataService() { Context = this.Context };
            InfyGST.GSTPortal.EInvoiceResponse response = null;

            var ewayresonse = gst_InboxDataService.CancelEinvoice(new GST_Invoice()
            {
                billid = item.billid,
                billinboxid = item.id,
                transportid = item.transportid,
                vehcicleno = item.vehicleno,
                irn = item.einvoice
            });

            if (ewayresonse.Status == "1")
            {
                string query = @"update billinbox set hascancel=true,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id;";
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var transaction = conn.BeginTransaction();
                    Update(transaction, conn, item);
                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    command.Parameters.Add("einvoicejosn", NpgsqlTypes.NpgsqlDbType.Json).Value = "null";
                    command.Parameters.Add("einvoice", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "";
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                    command.Parameters.Add("billid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.billid;
                    command.ExecuteNonQuery();
                    var billcancel = new BillCancel()
                    {
                        ids = new List<long>()
                    };
                    billcancel.ids.Add(item.billid);
                    BillDataService billDataService = new BillDataService() { Context = this.Context };
                    billDataService.CancelOnly(billcancel, transaction, conn, true);
                    transaction.Commit();
                    conn.Close();
                }
            }
            response = new GSTPortal.EInvoiceResponse();
            response.BillInbox = Search(new BillinboxCreteria() { id = item.id }).First();
            return response;
        }

        internal GSTPortal.EInvoiceResponse CancelEway(BillinboxCreteria item)
        {
            Gst_InboxDataService gst_InboxDataService = new Gst_InboxDataService() { Context = this.Context };
            InfyGST.GSTPortal.EInvoiceResponse response = null;

            long ewbNo;
            if (long.TryParse(item.ewb, out ewbNo))
            {
                var ewayresonse = gst_InboxDataService.CancelEWay(new GST_Invoice()
                {
                    billid = item.billid,
                    billinboxid = item.id,
                    transportid = item.transportid,
                    vehcicleno = item.vehicleno,
                    ewbNo = ewbNo
                });
            }

            string query = @"update billinbox set ewb=@ewb,haseway=true,ewayjson=@ewayjson,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id";
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                var transaction = conn.BeginTransaction();
                Update(transaction, conn, item);
                NpgsqlCommand command = new NpgsqlCommand(query, conn);
                command.Parameters.Add("ewayjson", NpgsqlTypes.NpgsqlDbType.Json).Value = "null";
                command.Parameters.Add("ewb", NpgsqlTypes.NpgsqlDbType.Varchar).Value = "";
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                command.ExecuteNonQuery();
                transaction.Commit();
                conn.Close();
            }
            response = new GSTPortal.EInvoiceResponse();
            response.BillInbox = Search(new BillinboxCreteria() { id = item.id }).First();
            return response;
        }

        public void CreateZeroStock(ECOMUANUploaded.Order osalesorder, NpgsqlTransaction transation, NpgsqlConnection conn)
        {
            var alluan = osalesorder.items.ConvertAll(x => x.UAN).Distinct().ToList();
            string query = @"select stock.serialno uid from stock where stock.serialno in ({0})";
            query = string.Format(query, string.Join(",", osalesorder.items.ConvertAll(x => "'" + x.UAN + "'")));
            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    alluan.Remove(reader["uid"].ToString());
                }
            }
            List<Tuple<string, long, long>> lst = new List<Tuple<string, long, long>>();
            if (alluan.Count > 0)
            {
                alluan.ForEach(x =>
                {
                    lst.Add(new Tuple<string, long, long>(x, 0, 0));
                });
            }

            query = @"
                            with alluids as
                            (
                                select stock.serialno uid,salablegoods.id saleid,stock.id stockid from stock
                                left outer join salablegoods on salablegoods.stockid = stock.id and 
                                            salablegoods.locationid = @locationid
                                where stock.serialno in ({0})
                                and (stock.id is  null or salablegoods.id is null)
                            ),availall as
                            (
                                select * from alluids where (alluids.stockid is not null and alluids.saleid is not null)	
                            ),availstock as
                            (
                                select * from alluids where (alluids.stockid is not null and alluids.saleid is  null)	
                            )
                            select alluids.uid,availstock.stockid,availall.saleid from alluids 
                            left outer join availall on availall.uid = alluids.uid
                            left outer join availstock on availstock.uid = alluids.uid
                            where (alluids.stockid is null or alluids.saleid is null)
                            and (availall.uid is null or availstock.uid is null)";


            var param = new SortedList<string, object>();
            param.Add("locationid", osalesorder.locationid);
            query = string.Format(query, string.Join(",", osalesorder.items.ConvertAll(x => "'" + x.UAN + "'")));

            using (var reader = ExecuteReader(query, param, conn, transation))
            {
                while (reader.Read())
                {
                    long saleid = reader["saleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["saleid"]);
                    long stockid = reader["stockid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stockid"]);
                    string uid = reader["uid"] == DBNull.Value ? "" : Convert.ToString(reader["uid"]);
                    if (lst.Exists(x => x.Item1 == uid)) continue;
                    lst.Add(new Tuple<string, long, long>(uid, stockid, saleid));
                }
            }


            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = Context };
            if (lst.Exists(x => x.Item2 == 0 && x.Item3 == 0))
            {
                var mlist = lst.FindAll(x => x.Item2 == 0 && x.Item3 == 0);
                var missingdesigns = mfgdesignDataService.SearchUANItem(mlist.ConvertAll(y => y.Item1));
                long lastid = 0;
                using (var reader = ExecuteReader("select barcode,id from stock order by id desc limit 1", null))
                {
                    if (reader.Read())
                    {
                        var lastbarcode = reader.GetString(0);
                        try
                        {
                            lastid = lastbarcode.LongFromBase62();
                        }
                        catch (Exception exp)
                        {
                            lastid = reader.GetInt64(1);
                        }
                    }
                }

                while (true)
                {
                    param = new SortedList<string, object>();
                    param.Add("barcode", lastid.ToBase62());
                    using (var reader = ExecuteReader("select id from stock where barcode=@barcode limit 1", param))
                    {
                        if (!reader.Read())
                        {
                            break;
                        }
                        else
                        {
                            lastid++;
                        }
                    }
                }

                InfyPOS.Data.StockDataService stockDataService = new InfyPOS.Data.StockDataService() { Context = Context };
                lst.RemoveAll(x => x.Item2 == 0 && x.Item3 == 0);
                foreach (var item in mlist)
                {
                    lastid++;
                    var sitem = missingdesigns.Find(x => x.uid == item.Item1);
                    InfyPOS.Models.Stock stock = new InfyPOS.Models.Stock();
                    stock.barcode = lastid.ToBase62();
                    stock.dealerprice = sitem.dealerprice;
                    stock.designid = sitem.code;
                    stock.printingname = sitem.pname;
                    stock.productid = sitem.productid;
                    stock.itemid = sitem.itemid;
                    stock.hsncode = sitem.hsn;
                    stock.basebuyingprice = sitem.dealerprice;
                    stock.buyingprice = sitem.dealerprice;
                    stock.buyingqty = 0;
                    stock.displaymrp = sitem.mrp;
                    stock.eprice = sitem.dealerprice;
                    stock.invoiceitemid = -1;
                    stock.ibuyingprice = 0;
                    stock.isactive = true;
                    stock.hsncode = sitem.hsn;
                    stock.price = sitem.mrp;
                    stock.serialno = sitem.uid;
                    stock.qty = 0;
                    stock.sizeid = sitem.sizeid;
                    stock.transfered = 0;
                    stock.companyid = osalesorder.companyid;
                    stockDataService.Insert(stock);
                    lst.Add(new Tuple<string, long, long>(item.Item1, stock.id, 0));
                }
            }


            if (lst.Exists(x => x.Item3 == 0))
            {
                var mlist = lst.FindAll(x => x.Item3 == 0);
                var missingdesigns = mfgdesignDataService.SearchUANItem(mlist.ConvertAll(y => y.Item1));
                SortedList<string, string> barcodes = new SortedList<string, string>();
                using (var reader = ExecuteReader("Select serialno,barcode from stock where id in (" +
                    string.Join(",", mlist.ConvertAll(y => y.Item2)) + ")", null, conn, transation))
                {
                    while (reader.Read())
                    {
                        var sno = reader["serialno"].ToString();
                        if (!barcodes.ContainsKey(sno))
                            barcodes.Add(sno, reader["barcode"].ToString());
                    }
                }
                foreach (var sitem in mlist)
                {
                    var dsnitem = missingdesigns.Find(x => x.uid == sitem.Item1);
                    InfyPOS.Data.SalablegoodsDataService salableDataService = new InfyPOS.Data.SalablegoodsDataService() { Context = Context };
                    Salablegoods salablegoods = new Salablegoods();
                    salablegoods.barcode = barcodes[sitem.Item1];
                    salablegoods.iqty = 0;
                    salablegoods.stockid = sitem.Item2;
                    salablegoods.companyid = osalesorder.companyid;
                    salablegoods.locationid = osalesorder.locationid;
                    salablegoods.itemid = dsnitem.itemid;
                    salablegoods.price = dsnitem.mrp;
                    salablegoods.sellingname = dsnitem.pname;
                    salablegoods.isactive = true;
                    salableDataService.Insert(salablegoods);
                }
            }
        }

        private Bill CreateECOMBill(ECOMUANUploaded.Order billorder, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            var param = new SortedList<string, object>();
            param.Add("ecomorderno", billorder.MOrderNo);
            using (var reader = ExecuteReader("Select billno,billdate,billno,ecomorderno,billattributes->'ecom'->>'orderid' orderid,billattributes->'ecom'->>'morderid' morderid,billattributes->'ecom'->>'invoicecode' invoicecode from bill where ecomorderno=@ecomorderno and isactive=true and billtype!=9", param))
            {
                while (reader.Read())
                {
                    string billno = reader["billno"].ToString();
                    string orderno = reader["ecomorderno"].ToString();
                    string orderid = reader["orderid"] == DBNull.Value ? "" : reader["orderid"].ToString();
                    string morderid = reader["morderid"] == DBNull.Value ? "" : reader["morderid"].ToString();
                    string invoicecode = reader["invoicecode"] == DBNull.Value ? "" : reader["invoicecode"].ToString();
                    DateTime billdate = reader["billdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["billdate"]);
                    if (billorder.invoicecode == invoicecode && billorder.BillDate == billdate)
                    {
                        return new Bill()
                        {
                            ecomorderno = billorder.MOrderNo,
                            errormessage = "Already imported as " + reader["billno"].ToString()
                        };
                    } else if (billorder.BillDate == billdate)
                    {
                        return new Bill()
                        {
                            ecomorderno = billorder.MOrderNo,
                            errormessage = "Already imported as " + reader["billno"].ToString()
                        };
                    }
                }
            }
            bool issalereturn = false;
            var totalqty = billorder.items.Sum(x => x.Qty);

            if (billorder.items.Exists(x => x.Qty < 0))
            {
                issalereturn = true;
                CreateZeroStock(billorder, transaction, conn);
            }

            var requireddetails = billorder.items.ToDictionary(x => x.UAN, x => x);
            string query = @"select stock.serialno,
                                stock.barcode,stock.itemid,
                                salablegoods.id,salablegoods.stockid,
                                (salablegoods.iqty - (salablegoods.oqty+salablegoods.dqty+salablegoods.jqty)) stock 
                                from stock
                                inner join salablegoods on salablegoods.stockid = stock.id and
	                            (salablegoods.iqty - (salablegoods.oqty+salablegoods.dqty+salablegoods.jqty))>0 
	                            and salablegoods.isactive=true
                                where stock.serialno in (" + string.Join(",", billorder.items.ConvertAll(x => "'" + x.UAN + "'")) + ") and salablegoods.locationid=" + billorder.locationid;


            if (issalereturn)
            {
                query = @"select stock.serialno,
                                stock.barcode,stock.itemid,
                                salablegoods.id,salablegoods.stockid,
                                (salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) stock 
                                from stock
                                inner join salablegoods on salablegoods.stockid = stock.id 
	                            and salablegoods.isactive=true
                                where stock.serialno in (" + string.Join(",", billorder.items.ConvertAll(x => "'" + x.UAN + "'")) +
                                    @") and salablegoods.locationid=" + billorder.locationid + " order by (salablegoods.oqty+salablegoods.dqty+salablegoods.jqty) desc";
            }

            List<ECOMUANUploaded.OrderItem> bundleitems = new List<ECOMUANUploaded.OrderItem>();

            using (var reader = ExecuteReader(query, null))
            {
                while (reader.Read())
                {
                    string serialno = Convert.ToString(reader["serialno"]);
                    string barcode = Convert.ToString(reader["barcode"]);
                    long id = Convert.ToInt64(reader["id"]);
                    long itemid = Convert.ToInt64(reader["itemid"]);
                    long stockid = Convert.ToInt64(reader["stockid"]);
                    decimal stock = Convert.ToDecimal(reader["stock"]);
                    var required = Math.Abs(requireddetails[serialno].Qty) - requireddetails[serialno].RQty;
                    if (issalereturn && requireddetails[serialno].RQty < required)
                    {
                        if (stock <= 0)
                            stock = required;
                        if (required < stock)
                            stock = required;
                        if (required < Math.Abs(stock))
                            stock = required;

                        ECOMUANUploaded.OrderItem nitem = JsonConvert.DeserializeObject<ECOMUANUploaded.OrderItem>(JsonConvert.SerializeObject(requireddetails[serialno]));
                        nitem.Qty = stock * -1;
                        nitem.salablegoodsid = id;
                        nitem.stockid = stockid;
                        nitem.barcode = barcode;
                        nitem.ItemID = itemid;
                        if (stock == 0 && issalereturn)
                        {
                            nitem.Qty = required * -1;
                            requireddetails[serialno].RQty += required;
                        }
                        else
                        {
                            requireddetails[serialno].RQty += stock;
                        }
                        bundleitems.Add(nitem);
                    }
                    else if (!issalereturn && requireddetails[serialno].Qty > requireddetails[serialno].RQty)
                    {
                        ECOMUANUploaded.OrderItem nitem = JsonConvert.DeserializeObject<ECOMUANUploaded.OrderItem>(JsonConvert.SerializeObject(requireddetails[serialno]));
                        if (stock >= required)
                        {
                            nitem.Qty = required;
                            nitem.salablegoodsid = id;
                            nitem.barcode = barcode;
                            nitem.stockid = stockid;
                            nitem.ItemID = itemid;
                            requireddetails[serialno].RQty += required;
                            bundleitems.Add(nitem);
                        }
                        else
                        {
                            nitem.Qty = stock;
                            nitem.salablegoodsid = id;
                            nitem.stockid = stockid;
                            nitem.barcode = barcode;
                            nitem.ItemID = itemid;
                            requireddetails[serialno].RQty += stock;
                            bundleitems.Add(nitem);
                        }
                    }
                }
            }

            var stockshortages = requireddetails.ToList().FindAll(x => (Math.Abs(x.Value.Qty) - x.Value.RQty) > 0);
            if (stockshortages.Count > 0)
            {
                if (issalereturn)
                {
                    stockshortages.ForEach(x =>
                    {
                        x.Value.RQty = x.Value.Qty;
                    });
                }
                else
                {
                    throw new Exception(" Stock shortage " + String.Join(",", stockshortages.ConvertAll(x => x.Key + " " + (x.Value.Qty - x.Value.RQty))));
                }
            }

            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            var company = companyDataService.Search(new CompanyCreteria()
            {
                id = billorder.companyid
            }).First();

            List<MfgDesignItem> mfgDesignItems = null;
            var serialnos = billorder.items.FindAll(x => !string.IsNullOrEmpty(x.UAN)).ConvertAll(x => x.UAN).Distinct().ToList();
            if (serialnos.Count > 0)
            {
                MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
                mfgDesignItems = mfgdesignDataService.SearchDesignList(serialnos);
            }

            //ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            //var location = referencelistDataService.Search(new ReferencelistCreteria() {id = billorder.locationid }).First();

            BillDataService billDataService = new BillDataService() { Context = this.Context };
            billDataService.Context = this.Context;

            Bill bill = new Bill();
            bill.isb2b = true;
            bill.createdby = Context.UserID;
            bill.modifiedby = Context.UserID;
            bill.createdon = DateTimeNow;
            bill.modifiedon = DateTimeNow;
            bill.companyid = billorder.companyid;
            bill.customerid = billorder.customerid;
            bill.billdate = billorder.BillDate;
            bill.ecomorderno = billorder.OrderNo;
            bill.customerstateid = billorder.stateid;
            if (billorder.stateid == 0)
                bill.customerstateid = company.stateid;
            bill.billtype = (issalereturn) ? Bill.BillTypes.ReturnBill : Bill.BillTypes.Credit;
            bill.iscreditbill = true;
            bill.isreturn = (issalereturn);
            bill.isinterstatesale = bill.customerstateid != company.stateid;
            bill.isinterstatetransfer = false;
            bill.isactive = true;
            bill.version = 1;
            bill.locationid = billorder.locationid;
            bill.Billitems = new List<Billitems>();
            bill.billattributes = new BillAttribute();
            bill.billattributes.ecom = billorder.ECommerce;
            bill.ecomorderno = billorder.MOrderNo;
            Data.TaxDataService taxDataService = new TaxDataService() { Context = this.Context };
            taxDataService.Context = this.Context;
            var taxsplits = taxDataService.RangedTaxProducts(bundleitems.ConvertAll(x => x.barcode));
            foreach (var item in bundleitems)
            {
                Billitems billitems = new Billitems();
                bill.Billitems.Add(billitems);
                billitems.createdby = Context.UserID;
                billitems.modifiedby = Context.UserID;
                billitems.createdon = DateTimeNow;
                billitems.modifiedon = DateTimeNow;
                billitems.barcode = item.barcode;
                billitems.salablegoodsid = item.salablegoodsid;
                billitems.stockid = item.stockid;
                billitems.companyid = billorder.companyid;
                billitems.isactive = true;
                billitems.locationid = billorder.locationid;
                billitems.barcode = item.barcode;
                billitems.attributes = new BillItemAttributes();
                billitems.qty = item.Qty;
                billitems.piece = item.Qty;
                if (!string.IsNullOrEmpty(item.UAN) &&
                    mfgDesignItems != null &&
                    mfgDesignItems.Exists(x => x.uid == item.UAN))
                {
                    var mfgitem = mfgDesignItems.Find(x => x.uid == item.UAN);
                    billitems.attributes.serialno = item.UAN;
                    billitems.attributes.size = mfgitem.size;
                    billitems.attributes.designid = mfgitem.code;
                    billitems.attributes.colourname = mfgitem.colour;
                    billitems.attributes.colourcode = mfgitem.colourcode;
                    billitems.attributes.size = mfgitem.size;
                    billitems.printingname = mfgitem.printingname;
                    billitems.productid = mfgitem.productid;
                    billitems.mrp = mfgitem.mrp;
                    billitems.hsncode = mfgitem.hsn;
                }
                var newPrice = item.Price - item.Discount;
                if (taxsplits.ContainsKey(billitems.barcode))
                {
                    if (taxsplits[billitems.barcode].taxsplit != null)
                    {
                        var _taxid = taxsplits[billitems.barcode].id;
                        var newtax = taxsplits[billitems.barcode].taxsplit.Find(newPrice);
                        if (newtax.taxpercent != billitems.taxpercentage)
                        {
                            billitems.taxpercentage = newtax.taxpercent;
                            billitems.taxid = _taxid;
                        }
                    }
                    else
                    {
                        billitems.taxpercentage = taxsplits[billitems.barcode].taxpercentage;
                        billitems.taxid = taxsplits[billitems.barcode].id;
                    }
                    billitems.tax = Math.Round(item.Qty * (newPrice / (100 + billitems.taxpercentage) * billitems.taxpercentage), 4);
                    billitems.itemtax = Math.Round(billitems.tax / item.Qty, 4);
                    billitems.price = newPrice;
                }
                billitems.price = (item.Price - item.Discount);
                billitems.discount = item.Discount;
                billitems.rate = (item.Price - item.Discount) - billitems.tax;
                billitems.saleamount = billitems.rate;
                billitems.salerate = billitems.saleamount + billitems.itemtax;
                billitems.receivable = Math.Round((item.Price - item.Discount) * billitems.qty, 2);
            }

            bill.qty = bill.Billitems.Sum(e => e.qty);
            bill.totalqty = bill.Billitems.Sum(e => e.qty);
            bill.totalpiece = bill.Billitems.Sum(e => e.piece);
            bill.tax = bill.Billitems.Sum(e => e.tax);
            bill.receivable = bill.Billitems.Sum(e => e.receivable);
            bill.rounding = Math.Round(bill.receivable, MidpointRounding.AwayFromZero) - bill.receivable;

            if (totalqty != bill.qty)
            {
                var missinguan = billorder.items.FindAll(x => !bill.Billitems.Exists(y => y.attributes.serialno == x.UAN));
                if (missinguan.Count > 0)
                {
                    bill.errormessage = "Can't find these barcodes while creating bills " +
                        String.Join(",", missinguan.ConvertAll(x => x.UAN));
                    return bill;
                }

                missinguan = billorder.items.FindAll(x => Math.Abs(x.Qty) != Math.Abs(bill.Billitems.Sum(y => y.attributes.serialno == x.UAN ? y.qty : 0)));
                if (missinguan.Count > 0)
                {
                    bill.errormessage = bill.ecomorderno + " - " +
                        "Can't match expected quantity for " +
                        String.Join(",", missinguan.ConvertAll(x => x.UAN));
                    return bill;
                }
            }

            SequenceNo sequence = null;
            string billsequence = "bill.billno";
            bool locationwisebillno = true;

            if (bill.billtype == Bill.BillTypes.ReturnBill &&
                Services.ConfigManager.Instance(Context.OrganizationID).Config.system_srbillno)
            {
                locationwisebillno = Services.ConfigManager.Instance(Context.OrganizationID).Config.system_srbillnobylocation;
                billsequence = "billreturn.billno";
            }

            sequence = SequenceManager.Instance(Context.OrganizationID).Get(billsequence, locationwisebillno ? bill.locationid : 0, bill.companyid,
                Services.ConfigManager.Instance(Context.OrganizationID).Config.system_salefloorwisebillno ? bill.floorid : 0);

            if (!sequence.Validate(Context, "Bill No", false))
            {
                InfyPOS.Data.ReferencelistDataService reflist = new ReferencelistDataService() { Context = this.Context };
                var location = reflist.Search(new ReferencelistCreteria()
                {
                    id = bill.locationid
                });
                if (location.Count == 1)
                    sequence.Validate(Context, "Bill No for " + location[0].name, true);
            }

            while (true)
            {
                if (!Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(bill.locationid))
                {
                    Services.ConfigManager.Instance(Context.OrganizationID).Initialize();
                }
                var system_salebillnodailyreset = false;

                if (Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig.ContainsKey(bill.locationid))
                    system_salebillnodailyreset = Services.ConfigManager.Instance(Context.OrganizationID).LocationConfig[bill.locationid].system_salebillnodailyreset;

                bill.billno = sequence.GetNextNumber(Context, system_salebillnodailyreset, bill.billdate.Date, conn, transaction);
                param = new SortedList<string, object>();
                param.Add("billno", bill.billno);
                using (var reader = ExecuteReader("Select id from bill where billno=@billno", param))
                {
                    if (!reader.Read())
                    {
                        break;
                    }
                }
            }

            billDataService.Insert(transaction, conn, bill, new Billmaster()
            {
                id = bill.id,
                total = bill.total
            });
            return bill;
        }

        public ProgressStatus SaveUANECOM(ECOMUANUploaded pouploaditem)
        {
            ProgressStatus progressStatus = InfyPOS.Processors.ProcessManager.Instance(Context.OrganizationID).GetEmptyStatus();
            System.Threading.Thread thread = new System.Threading.Thread(new System.Threading.ParameterizedThreadStart(SaveUANECOMInternal));
            thread.Start(new Tuple<ECOMUANUploaded, ProgressStatus>(pouploaditem, progressStatus));
            return progressStatus;
        }
        internal void SaveUANECOMInternal(object objpouploaditem)
        {
            var pouploaditemkeyval = objpouploaditem as Tuple<ECOMUANUploaded, ProgressStatus>;
            var pouploaditem = pouploaditemkeyval.Item1;
            var progressStatus = pouploaditemkeyval.Item2;
            System.Threading.Thread.Sleep(100);
            List<Bill> history = new List<Bill>();
            NpgsqlTransaction transaction = null;
            try
            {
                //CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
                //var allcompany = companyDataService.Search(new CompanyCreteria()
                //{
                //});
                //ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                //var alllocations = referencelistDataService.Search(new ReferencelistCreteria()
                //{
                //    type = "LOCATION"
                //});
                progressStatus.percentage = 0;
                progressStatus.hasstarted = true;
                progressStatus.status = "Started";

                foreach (var orderitem in pouploaditem.orders)
                {
                    try
                    {
                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();
                            progressStatus.totalrows = pouploaditem.orders.Count;
                            var bill = CreateECOMBill(orderitem, conn, transaction);
                            if (bill.id == 0)
                                progressStatus.failedrows++;
                            else
                                progressStatus.completedrows++;

                            progressStatus.status = "Created " +
                                    progressStatus.completedrows.ToString("F0") + ", Failed " +
                                    progressStatus.failedrows.ToString("F0");

                            transaction.Commit();
                            conn.Close();
                            history.Add(bill);
                        }
                    }
                    catch (Exception exp)
                    {
                        if (progressStatus.statuslist == null) progressStatus.statuslist = new List<string>();
                        progressStatus.statuslist.Add(exp.Message);
                        progressStatus.Exception = exp;
                        progressStatus.failedrows++;
                    }
                    progressStatus.percentage =(int) Math.Round((double)progressStatus.totalrows / (double)(progressStatus.completedrows + progressStatus.failedrows));
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, pouploaditem);
            }
            progressStatus.hascompleted = true;
            progressStatus.percentage = 100;
            progressStatus.result = history.ConvertAll(x=>new ProgressStatus.ResultItem()
            {
                 id = x.id,
                 sid = x.ecomorderno,
                 status = x.errormessage
            });
        }

        internal bool Publish(Billinbox item)
        {
            BillDataService billDataService = new BillDataService() { Context = this.Context };
            var bill = billDataService.Search(new BillCreteria() { id = item.billid }).First();

            bill.billdeliverydetails = GetBillDeliveryDetails(new BillinboxCreteria()
            {
                billid = bill.id
            }).First();

            if (bill.Billitems.Exists(x => x.attributes != null &&
                    x.attributes.so != null && x.attributes.so.Count > 0))
            {
                List<long> soitemids = new List<long>();
                bill.Billitems.ForEach(x =>
                {
                    foreach (var soitem in x.attributes.so)
                        soitemids.Add(soitem.id);
                });
                SalesorderitemsDataService salesorderitemsDataService = new SalesorderitemsDataService() { Context = this.Context };
                long agentid = salesorderitemsDataService.FindAgent(soitemids);
                long addnchargeid = salesorderitemsDataService.FindAdditionalCharge(soitemids);
                long salesmenid = salesorderitemsDataService.FindSalesmen(soitemids);

                if (agentid > 0)
                {
                    AgentDataService agentdatasource = new AgentDataService();
                    bill.agent = agentdatasource.Search(new AgentCreteria()
                    {
                        id = agentid
                    }).First();
                }
                if (salesmenid > 0)
                {
                    EmployeeDataService agentdatasource = new EmployeeDataService();
                    bill.salesmen = agentdatasource.Search(new EmployeeCreteria()
                    {
                        id = salesmenid,
                        hidesalaryinfo = true
                    }).First();
                }
            }
            else if (bill.customer.agentid > 0)
            {
                AgentDataService agentdatasource = new AgentDataService();
                bill.agent = agentdatasource.Search(new AgentCreteria()
                {
                    id = bill.customer.agentid
                }).First();
            }

            string licensekey = LicenseManager.License.Key.Replace("-", "");
            InfyPOS.Controllers.ImageController.HashDetail hashDetail = new InfyPOS.Controllers.ImageController.HashDetail(Context.OrganizationID, InfyPOS.Controllers.ImageController.HashDetail.GetLongHash(licensekey), bill.id);
            var url = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.hostedserver + (InfyPOS.Services.ConfigManager.Instance(new BaseData().Context.OrganizationID).Config.hostedserver.EndsWith("/") ? "" : "/") + "File/BP/" + licensekey + "/" + System.Web.HttpUtility.UrlEncode(hashDetail.Hash);
            bill.printurl = url;
            Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, bill, Processors.Schedule_Actions.Actions.Publish);
            return true;
        }

        internal List<BillinboxHandovers> SearchHandovers(BillinboxCreteria item)
        {
            var list = Search(item);
            var handovergroups = list.GroupBy(x => new { x.transportname,x.transportid , x.handedoveron, x.handedtoid }).ToList().ConvertAll(x => new BillinboxHandovers()
            {
                transportname = x.Key.transportname,
                handoveron = x.Key.handedoveron,
                handoverto = x.Key.handedtoid,
                transportid =x.Key.transportid,
                 items = x.ToList()
            });

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = Context };
            var handoverpersons = referencelistDataService.SearchList(new ReferencelistCreteria() { ids = handovergroups.ConvertAll(x => x.handoverto).Distinct().ToList() });
            handoverpersons.ForEach(x =>
            {
                handovergroups.FindAll(y => y.handoverto == x.id).ForEach(z => z.handoverperson = x.name);
            });
            return handovergroups;
        }

        internal bool SaveHandover(List<Billinbox> items)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var itm in items)
                    {
                        //,transportid=@transportid
                        NpgsqlCommand command = new NpgsqlCommand(@"Update billinbox set modifiedby=@modifiedby,modifiedon=@modifiedon,
                        handedoveron=@handedoveron,handedtoid=@handedtoid where id=@id", conn, transaction);
                        command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                        command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("handedtoid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = itm.handedtoid;
                        command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = itm.transportid;
                        command.Parameters.Add("handedoveron", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = itm.handedoveron;
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = itm.id;
                        int successCount = command.ExecuteNonQuery();
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
                return false;
            }
            return true;
        }
    }

}
