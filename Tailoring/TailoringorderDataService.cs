using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class TailoringorderDataService : BaseData
    {
        const string SQL_Search = @"select id, customername, customermobile, notes, orderno, customerid, locationid, 
    supplierid, companyid, receivedon, expectedon, status, statuson, gross, tax, total, advancepaid, expectedqty, 
    receivedqty, returnqty, createdon, version, isactive, modifiedby, createdby, modifiedon,customeraddress,entryno,
    receivedby,issuedqty,deliveredqty,stitchinglocationid,rackid,cardno,rackno from tailoringorder where id=@id";

        const string SQL_Insert = @"INSERT INTO public.tailoringorder(customername, customermobile, notes, orderno, customerid, locationid, supplierid, companyid, receivedon, expectedon, status, statuson, gross, tax, total, advancepaid, expectedqty, receivedqty, returnqty, createdon, version, isactive, modifiedby, createdby, modifiedon,customeraddress,entryno,receivedby,stitchinglocationid,rackid,cardno,rackno,issuedqty)
        VALUES(@customername, @customermobile, @notes, @orderno, @customerid, @locationid, @supplierid, @companyid, @receivedon, @expectedon, @status, @statuson, @gross, @tax, @total, @advancepaid, @expectedqty, @receivedqty, @returnqty, @createdon, @version, @isactive, @modifiedby, @createdby, @modifiedon,@customeraddress,@entryno,@receivedby,@stitchinglocationid,@rackid,@cardno,@rackno,@issuedqty) RETURNING id";


        const string SQL_Update = @"UPDATE public.tailoringorder SET customername=@customername, customermobile=@customermobile, notes=@notes, orderno=@orderno, customerid=@customerid, locationid=@locationid, supplierid=@supplierid, companyid=@companyid, receivedon=@receivedon, expectedon=@expectedon, status=@status, receivedby=@receivedby,
        statuson=@statuson, gross=@gross, tax=@tax, total=@total, advancepaid=@advancepaid, expectedqty=@expectedqty, receivedqty=@receivedqty, returnqty=@returnqty, version=@version+1, modifiedby=@modifiedby, modifiedon=@modifiedon,customeraddress=@customeraddress,entryno=@entryno,stitchinglocationid=@stitchinglocationid,issuedqty=@issuedqty,
        rackid=@rackid,cardno=@cardno,rackno=@rackno
        where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.tailoringorder set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";


        public List<Tailoringorder> Search(TailoringorderCreteria _tailoringorder)
        {
            List<Tailoringorder> lst = new List<Tailoringorder>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_tailoringorder.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _tailoringorder.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if(_tailoringorder.ids != null && _tailoringorder.ids.Count>0)
                    {
                        qb.AddInParamer("tailoringorder.id", string.Join<long>(",", _tailoringorder.ids));
                    }

                    if (_tailoringorder.status > 0)
                        qb.AddParameter("tailoringorder.status", _tailoringorder.status, "=");

                    if (_tailoringorder.id > 0)
                        qb.AddParameter("tailoringorder.id", _tailoringorder.id, "=");
                    if (_tailoringorder.fromdate.Year > 1900)
                        qb.AddParameter("tailoringorder.receivedon", "fromdate", _tailoringorder.fromdate.Date, ">=");
                    if (_tailoringorder.todate.Year > 1900)
                        qb.AddParameter("tailoringorder.receivedon", "todate", _tailoringorder.todate.Date.AddDays(1), "<");
                    if (_tailoringorder.expfromdate.Year > 1900)
                        qb.AddParameter("tailoringorder.expectedon", "expfromdate", _tailoringorder.expfromdate.Date, ">=");
                    if (_tailoringorder.exptodate.Year > 1900)
                        qb.AddParameter("tailoringorder.expectedon", "exptodate", _tailoringorder.exptodate.Date.AddDays(1), "<");

                    if (_tailoringorder.rackid > 0)
                    {
                        qb.AddParameter("tailoringorder.rackid", _tailoringorder.rackid, "=");
                        qb.AddParameter("tailoringorder.hasbilled", false, "=");
                    }
                    if (_tailoringorder.customerid > 0)
                        qb.AddParameter("tailoringorder.customerid", _tailoringorder.customerid, "=");
                    if (_tailoringorder.companyid > 0)
                        qb.AddParameter("tailoringorder.companyid", _tailoringorder.companyid, "=");
                    if (_tailoringorder.locationid > 0)
                        qb.AddParameter("tailoringorder.locationid", _tailoringorder.locationid, "=");
                    if (_tailoringorder.stitchinglocationid > 0)
                        qb.AddParameter("tailoringorder.stitchinglocationid", _tailoringorder.stitchinglocationid, "=");

                    if (!string.IsNullOrEmpty(_tailoringorder.customermobile))
                        qb.AddParameter("customermobile", _tailoringorder.customermobile, "=");

                    if (!string.IsNullOrEmpty(_tailoringorder.entryno))
                        qb.AddParameter("entryno", _tailoringorder.entryno, "=");
                    else if (!string.IsNullOrEmpty(_tailoringorder.orderno))
                        qb.AddParameter("orderno", _tailoringorder.orderno, "=");

                    if (_tailoringorder.forinward)
                        qb.Add2Query("issuedqty>receivedqty");

                    qb.SortField = "tailoringorder.id desc";
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Tailoringorder otailoringorder = new Tailoringorder();
                        otailoringorder.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otailoringorder.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        otailoringorder.customermobile = reader["customermobile"] == DBNull.Value ? "" : reader["customermobile"].ToString();
                        otailoringorder.customeraddress = reader["customeraddress"] == DBNull.Value ? "" : reader["customeraddress"].ToString();
                        otailoringorder.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                        otailoringorder.notes = reader["notes"] == DBNull.Value ? "" : reader["notes"].ToString();
                        otailoringorder.orderno = reader["orderno"] == DBNull.Value ? "" : reader["orderno"].ToString();
                        otailoringorder.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);
                        otailoringorder.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        otailoringorder.stitchinglocationid = reader["stitchinglocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stitchinglocationid"]);

                        otailoringorder.rackid = reader["rackid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["rackid"]);
                        otailoringorder.cardno = reader["cardno"] == DBNull.Value ? "" : reader["cardno"].ToString();
                        otailoringorder.rackno = reader["rackno"] == DBNull.Value ? "" : reader["rackno"].ToString();

                        otailoringorder.receivedby = reader["receivedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedby"]);
                        otailoringorder.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        otailoringorder.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        otailoringorder.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        otailoringorder.expectedon = reader["expectedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedon"]);
                        otailoringorder.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        otailoringorder.statuson = reader["statuson"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["statuson"]);
                        otailoringorder.gross = reader["gross"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["gross"]);
                        otailoringorder.tax = reader["tax"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tax"]);
                        otailoringorder.total = reader["total"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["total"]);
                        otailoringorder.advancepaid = reader["advancepaid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["advancepaid"]);
                        otailoringorder.expectedqty = reader["expectedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["expectedqty"]);
                        otailoringorder.receivedqty = reader["receivedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedqty"]);
                        otailoringorder.issuedqty = reader["issuedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["issuedqty"]);
                        otailoringorder.deliveredqty = reader["deliveredqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["deliveredqty"]);
                        otailoringorder.returnqty = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["returnqty"]);
                        otailoringorder.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        otailoringorder.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        otailoringorder.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        otailoringorder.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        otailoringorder.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        otailoringorder.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(otailoringorder);
                    }
                }
                if (lst.Count > 0 && (_tailoringorder.showdetail || _tailoringorder.id > 0 || _tailoringorder.customerid > 0 || _tailoringorder.rackid > 0
                    || !string.IsNullOrEmpty(_tailoringorder.entryno) || !string.IsNullOrEmpty(_tailoringorder.orderno)))
                {
                    TailoringitemsDataService subTailoringitems = new TailoringitemsDataService();
                    TailoringpaymentDataService subTailoringpayments = new TailoringpaymentDataService();
                    TailoringitemsdeliveredDataService subTailoringdelivery = new TailoringitemsdeliveredDataService();
                    foreach (var item in lst)
                    {
                        item.Tailoringitems = subTailoringitems.SearchAll(item.id,null, _tailoringorder.foroutward);
                        item.TailoringDelivery = subTailoringdelivery.SearchAll(item.id);
                        item.TailoringPayments = subTailoringpayments.Search(new TailoringpaymentCreteria()
                        {
                            tailoringorderid = item.id
                        });
                    }
                    lst = lst.OrderByDescending(e => e.id).ToList();
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _tailoringorder);
            }
            return lst;
        }

        internal Tailoringorder Deliver(Tailoringorder oTailoringorder)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (oTailoringorder.TailoringDelivery != null)
                    {
                        TailoringitemsdeliveredDataService subdelivery = new TailoringitemsdeliveredDataService();
                        foreach (var pitem in oTailoringorder.TailoringDelivery.Where(e => e.id == 0))
                        {
                            pitem.tailoringorderid = oTailoringorder.id;
                            pitem.tailoringitemsid = pitem.tailoringitemsid;
                            pitem.deliveredon = DateTimeNow;
                            pitem.deliveredby = Context.UserID;
                            pitem.createdon = DateTimeNow;
                            pitem.modifiedon = DateTimeNow;
                            pitem.createdby = Context.UserID;
                            pitem.modifiedby = Context.UserID;
                            pitem.isactive = true;
                            pitem.version = 1;
                            subdelivery.Insert(transaction, conn, pitem);
                        }
                    }
                    if (oTailoringorder.TailoringPayments != null)
                    {
                        TailoringpaymentDataService subPayments = new TailoringpaymentDataService();
                        foreach (var pitem in oTailoringorder.TailoringPayments.Where(e => e.id == 0))
                        {
                            pitem.tailoringorderid = oTailoringorder.id;
                            pitem.createdon = DateTimeNow;
                            pitem.modifiedon = DateTimeNow;
                            pitem.createdby = Context.UserID;
                            pitem.modifiedby = Context.UserID;
                            pitem.customerid = oTailoringorder.customerid;
                            pitem.collectedby = oTailoringorder.receivedby;
                            pitem.companyid = oTailoringorder.companyid;
                            pitem.locationid = oTailoringorder.locationid;
                            subPayments.Insert(transaction, conn, pitem);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }

                oTailoringorder = Search(new TailoringorderCreteria()
                {
                    id = oTailoringorder.id,
                    showdetail = true
                }).First();

                if (oTailoringorder.Tailoringitems.All(e => e.expectedqty >= e.deliveredqty))
                {
                    var partialdelivery = oTailoringorder.Tailoringitems.Any(e => e.expectedqty > e.deliveredqty);
                    var deliveredqty = oTailoringorder.TailoringDelivery.Sum(e => e.deliveredqty);
                    var sqlquery = string.Format("Update Tailoringorder set status={0}, deliveredqty={1} where id={2};", (partialdelivery ? "5" : "4"), deliveredqty, oTailoringorder.id);
                    if (ExecuteQuery(sqlquery, null) == 1)
                        oTailoringorder.status = (partialdelivery ? 5 : 4);
                }

                if (oTailoringorder.Tailoringitems.Count > 0)
                {
                    var allorderid = oTailoringorder.Tailoringitems.GroupBy(x => x.tailoringorderid).ToList().ConvertAll(X => X.Key);

                    string query = @"with completedorders as
                        (
                            select tailoringorderid,sum(deliveredqty) readyqty from Tailoringitems
                            where tailoringorderid in (" + string.Join(",", allorderid) + @") 
                            group by tailoringorderid
                        )
                        select Tailoringorder.receivedon,customer.mobileno,customer.name,readyqty,
                        Tailoringorder.entryno,Tailoringorder.orderno,Tailoringorder.expectedon from completedorders
                        inner join Tailoringorder on Tailoringorder.id = tailoringorderid
                        inner join customer on customer.id = Tailoringorder.customerid";
                    List<Delivery> deliverymsgs = new List<Delivery>();
                    using (var reader = ExecuteReader(query, null))
                    {
                        while (reader.Read())
                        {
                            Delivery delivery = new Delivery();
                            delivery.billdate = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                            delivery.contactno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                            delivery.customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                            delivery.qty = reader["readyqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["readyqty"].ToString());
                            delivery.orderno = reader["orderno"] == DBNull.Value ? "" : reader["orderno"].ToString();
                            delivery.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                            delivery.expectedon = reader["expectedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedon"]);
                            deliverymsgs.Add(delivery);
                        }
                    }
                    foreach (var delivery in deliverymsgs)
                        Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, delivery, Processors.Schedule_Actions.Actions.AlterationDelivered);
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringorder);

            }

            return oTailoringorder;
        }


        public Tailoringorder Insert(Tailoringorder oTailoringorder)
        {
            if (oTailoringorder == null || oTailoringorder.Tailoringitems == null)
                throw new Exception("Order Items missing.");
            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("tailoringorder.orderno", oTailoringorder.locationid, oTailoringorder.companyid);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    oTailoringorder.entryno = sequence.GetNextNumber(Context, conn, transaction);
                    oTailoringorder.status = 1;
                    oTailoringorder.expectedqty = oTailoringorder.Tailoringitems.Sum(e => e.expectedqty);
                    if (oTailoringorder.customerid == 0)
                    {
                        CustomerDataService customerDataService = new CustomerDataService();
                        var customer = customerDataService.Insert(new Customer()
                        {
                            mobileno = oTailoringorder.customermobile,
                            name = oTailoringorder.customername,
                            address = oTailoringorder.customeraddress,
                            isactive = true,
                            version = 1,
                            createdby = Context.UserID,
                            modifiedby = Context.UserID,
                            createdon = DateTimeNow,
                            modifiedon = DateTimeNow
                        });
                        oTailoringorder.customerid = customer.id;
                    }
                    Insert(transaction, conn, oTailoringorder);
                    TailoringitemsDataService subTailoringitems = new TailoringitemsDataService();
                    foreach (var item in oTailoringorder.Tailoringitems)
                    {
                        item.tailoringorderid = oTailoringorder.id;
                        item.createdon = DateTimeNow;
                        item.modifiedon = DateTimeNow;
                        item.createdby = Context.UserID;
                        item.modifiedby = Context.UserID;
                        subTailoringitems.Insert(transaction, conn, item);
                    }

                    if (oTailoringorder.TailoringPayments != null)
                    {
                        TailoringpaymentDataService subPayments = new TailoringpaymentDataService();
                        foreach (var pitem in oTailoringorder.TailoringPayments)
                        {
                            pitem.tailoringorderid = oTailoringorder.id;
                            //pitem.collectedon = DateTimeNow;
                            pitem.createdon = DateTimeNow;
                            pitem.modifiedon = DateTimeNow;
                            pitem.createdby = Context.UserID;
                            pitem.modifiedby = Context.UserID;
                            pitem.customerid = oTailoringorder.customerid;
                            pitem.collectedby = oTailoringorder.receivedby;
                            pitem.companyid = oTailoringorder.companyid;
                            pitem.locationid = oTailoringorder.locationid;
                            subPayments.Insert(transaction, conn, pitem);
                        }
                    }
                    transaction.Commit();
                    conn.Close();

                    if (oTailoringorder.Tailoringitems.Count > 0)
                    {
                        var allorderid = oTailoringorder.Tailoringitems.GroupBy(x => x.tailoringorderid).ToList().ConvertAll(X => X.Key);


                        string query = @"with completedorders as
                        (
                            select tailoringorderid,sum(expectedqty) readyqty from Tailoringitems
                            where tailoringorderid in (" + string.Join(",", allorderid) + @") 
                            group by tailoringorderid
                        )
                        select Tailoringorder.receivedon,customer.mobileno,customer.name,readyqty,
                        Tailoringorder.entryno,Tailoringorder.orderno,Tailoringorder.expectedon from completedorders
                        inner join Tailoringorder on Tailoringorder.id = tailoringorderid
                        inner join customer on customer.id = Tailoringorder.customerid";
                        List<Delivery> deliverymsgs = new List<Delivery>();
                        using (var reader = ExecuteReader(query, null))
                        {
                            while (reader.Read())
                            {
                                Delivery delivery = new Delivery();
                                delivery.billdate = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                                delivery.expectedon = reader["expectedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedon"]); 
                                delivery.contactno = reader["mobileno"] == DBNull.Value ? "" : reader["mobileno"].ToString();
                                delivery.orderno = reader["orderno"] == DBNull.Value ? "" : reader["orderno"].ToString();
                                delivery.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                                delivery.customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                                delivery.qty = reader["readyqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["readyqty"].ToString());
                                deliverymsgs.Add(delivery);
                            }
                        }
                        foreach (var delivery in deliverymsgs)
                            Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, delivery, Processors.Schedule_Actions.Actions.AlterationReceived);
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringorder);

            }

            return oTailoringorder;
        }

        public Tailoringorder Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringorder oTailoringorder)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("customername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringorder.customername == null ? "" : oTailoringorder.customername;
            command.Parameters.Add("customermobile", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringorder.customermobile == null ? "" : oTailoringorder.customermobile;
            command.Parameters.Add("customeraddress", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringorder.customeraddress == null ? "" : oTailoringorder.customeraddress;
            command.Parameters.Add("entryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringorder.entryno == null ? "" : oTailoringorder.entryno;
            command.Parameters.Add("notes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringorder.notes == null ? "" : oTailoringorder.notes;
            command.Parameters.Add("orderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringorder.orderno == null ? "" : oTailoringorder.orderno;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.customerid;

            command.Parameters.Add("rackid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.rackid;
            command.Parameters.Add("cardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringorder.cardno == null ? "" : oTailoringorder.cardno;
            command.Parameters.Add("rackno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringorder.rackno == null ? "" : oTailoringorder.rackno;

            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.locationid;
            command.Parameters.Add("stitchinglocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.stitchinglocationid;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.receivedby;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.supplierid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.companyid;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringorder.receivedon;
            command.Parameters.Add("expectedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringorder.expectedon;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringorder.status;
            command.Parameters.Add("statuson", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringorder.statuson;
            command.Parameters.Add("gross", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringorder.gross;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringorder.tax;
            command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringorder.total;
            command.Parameters.Add("advancepaid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringorder.advancepaid;
            command.Parameters.Add("expectedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringorder.expectedqty;
            command.Parameters.Add("receivedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringorder.receivedqty;
            command.Parameters.Add("returnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringorder.returnqty;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringorder.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringorder.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("issuedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringorder.issuedqty;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTailoringorder.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oTailoringorder;
        }

        public Tailoringorder Update(Tailoringorder oTailoringorder)
        {
            List<Tailoringorder> _order = Search(new TailoringorderCreteria() { id = oTailoringorder.id });
            if (_order.Count > 1 && _order[0].status != 1)
            {
                throw new Exception("Order update can't be done.");
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    if (oTailoringorder.customerid == 0)
                    {
                        CustomerDataService customerDataService = new CustomerDataService();
                        var customer = customerDataService.Insert(new Customer()
                        {
                            mobileno = oTailoringorder.customermobile,
                            name = oTailoringorder.customername,
                            address = oTailoringorder.customeraddress,
                            isactive = true,
                            version = 1,
                            createdby = Context.UserID,
                            modifiedby = Context.UserID,
                            createdon = DateTimeNow,
                            modifiedon = DateTimeNow
                        });
                        oTailoringorder.customerid = customer.id;
                    }
                    Update(transaction, conn, oTailoringorder);
                    TailoringitemsDataService subTailoringitems = new TailoringitemsDataService();
                    subTailoringitems.Update(transaction, conn, oTailoringorder.Tailoringitems, oTailoringorder.id);
                    TailoringpaymentDataService subTailoringpayments = new TailoringpaymentDataService();
                    subTailoringpayments.Update(transaction, conn, oTailoringorder.TailoringPayments, oTailoringorder.id);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringorder);

            }

            return oTailoringorder;
        }

        public Tailoringorder Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringorder oTailoringorder)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.id;
            command.Parameters.Add("customername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringorder.customername == null ? "" : oTailoringorder.customername;
            command.Parameters.Add("customermobile", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringorder.customermobile == null ? "" : oTailoringorder.customermobile;
            command.Parameters.Add("customeraddress", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringorder.customeraddress == null ? "" : oTailoringorder.customeraddress;
            command.Parameters.Add("notes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringorder.notes == null ? "" : oTailoringorder.notes;
            command.Parameters.Add("orderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringorder.orderno == null ? "" : oTailoringorder.orderno;
            command.Parameters.Add("entryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringorder.entryno == null ? "" : oTailoringorder.entryno;
            command.Parameters.Add("rackid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.rackid;
            command.Parameters.Add("cardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringorder.cardno == null ? "" : oTailoringorder.cardno;
            command.Parameters.Add("rackno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTailoringorder.rackno == null ? "" : oTailoringorder.rackno;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.customerid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.locationid;
            command.Parameters.Add("stitchinglocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.stitchinglocationid;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.receivedby;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.supplierid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.companyid;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringorder.receivedon;
            command.Parameters.Add("expectedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringorder.expectedon;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringorder.status;
            command.Parameters.Add("statuson", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringorder.statuson;
            command.Parameters.Add("gross", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringorder.gross;
            command.Parameters.Add("tax", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringorder.tax;
            command.Parameters.Add("total", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringorder.total;
            command.Parameters.Add("advancepaid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringorder.advancepaid;
            command.Parameters.Add("expectedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringorder.expectedqty;
            command.Parameters.Add("receivedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringorder.receivedqty;
            command.Parameters.Add("returnqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringorder.returnqty;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringorder.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringorder.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.createdby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("issuedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTailoringorder.issuedqty;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTailoringorder.version = Convert.ToInt32(reader[0].ToString());
                }
            }
            return oTailoringorder;
        }


        public bool Delete(Tailoringorder oTailoringorder)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oTailoringorder);
                    TailoringitemsDataService subTailoringitems = new TailoringitemsDataService();
                    subTailoringitems.DeleteAll(oTailoringorder.id, oTailoringorder.modifiedby, conn, transaction);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTailoringorder);
            }
            return hasSuccessCount;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Tailoringorder oTailoringorder)
        {
            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTailoringorder.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTailoringorder.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTailoringorder.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTailoringorder.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        internal List<TailoringitemInbox> SearchInbox(TailoringorderCreteria _tailoringorder)
        {
            List<TailoringitemInbox> response = new List<TailoringitemInbox>();
            string query = @"select tailoringitems.id,tailor.name tailorname,receivedon,expectedon,entryno,
                            tailoringitems.product,TailoringOrder.orderno,
                            tailoringitemsdelivered.deliveredon,
                            tailoringitems.charge*tailoringitems.receivedqty payable,
                            paidamount,tailoringitems.expectedqty,tailoringitems.delaycharge,prioritycharge,
                            tailoringitems.expectedqty*tailoringitems.additionalchargeamount additionalcharge,
							tailoringitems.receivedqty,measurementname,measurement,designname,tailoringitems.charge,
                            (tailoringitems.charge*tailoringitems.receivedqty - paidamount) balance ,paidon,
                            TailoringOrder.customername,TailoringOrder.customermobile,TailoringOrder.notes,
                            tailoringitems.deliveredqty,tailorid, bill.billno
                            from tailoringitems
                            inner join TailoringOrder on TailoringOrder.id = tailoringitems.TailoringOrderid
                            left outer join tailor on tailor.id = tailorid
							left outer join tailoringitemsdelivered on tailoringitemsdelivered.tailoringitemsid = tailoringitems.id
                            left outer join bill on cast(bill.billattributes->> 'tailoringorderid' as bigint) = TailoringOrder.id
                            where TailoringOrder.isactive=true and tailoringitems.isactive=true ";
            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = query.Substring(0, query.IndexOf("where"));

            if (_tailoringorder.fromdate.Year > 1900)
                qb.AddParameter("tailoringorder.receivedon", "fromdate", _tailoringorder.fromdate.Date, ">=");
            if (_tailoringorder.todate.Year > 1900)
                qb.AddParameter("tailoringorder.receivedon", "todate", _tailoringorder.todate.Date.AddDays(1), "<");
            if (_tailoringorder.companyid > 0)
                qb.AddParameter("tailoringorder.companyid", _tailoringorder.companyid, "=");
            if (_tailoringorder.locationid > 0)
                qb.AddParameter("tailoringorder.locationid", _tailoringorder.locationid, "=");
            if (_tailoringorder.tailorid > 0)
                qb.AddParameter("tailoringitems.tailorid", _tailoringorder.tailorid, "=");
            if (_tailoringorder.stitchinglocationid > 0)
                qb.AddParameter("tailoringorder.stitchinglocationid", _tailoringorder.stitchinglocationid, "=");
            if (_tailoringorder.measurementid > 0)
                qb.AddParameter("tailoringitems.measurementid", _tailoringorder.measurementid, "=");
            if (_tailoringorder.status == 6)
                qb.Add2Query("(tailoringitems.charge*tailoringitems.receivedqty - paidamount)>0");

            if (_tailoringorder.status == 7)
                qb.Add2Query("(tailoringitems.charge*tailoringitems.receivedqty) = paidamount and paidamount>0");

            if (_tailoringorder.status == 1)
                qb.Add2Query("tailoringitems.receivedqty=0");

            if (_tailoringorder.status == 2)
                qb.Add2Query("tailoringitems.issuedqty>0");

            if (_tailoringorder.status == 3)
                qb.Add2Query("tailoringitems.receivedqty=tailoringitems.expectedqty");

            if (_tailoringorder.status == 4)
                qb.Add2Query("tailoringitems.expectedqty=tailoringitems.deliveredqty and tailoringitems.deliveredqty>0");

            if (_tailoringorder.status == 5)
                qb.Add2Query("tailoringitems.expectedqty>tailoringitems.deliveredqty and tailoringitems.deliveredqty>0");


            qb.SortField = "tailoringorder.id desc";
            List<Tailoringorder> lst = new List<Tailoringorder>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TailoringitemInbox otailoringorder = new TailoringitemInbox();
                        otailoringorder.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otailoringorder.tailorid = reader["tailorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tailorid"]);
                        otailoringorder.orderdate = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);

                        otailoringorder.expectedon = reader["expectedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["expectedon"]);
                        otailoringorder.deliveredon = reader["deliveredon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["deliveredon"]);

                        otailoringorder.orderno = reader["orderno"] == DBNull.Value ? "" : reader["orderno"].ToString();
                        otailoringorder.billno = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        otailoringorder.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                        otailoringorder.paidon = reader["paidon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paidon"]);
                        otailoringorder.measurement_json = reader["measurement"] == DBNull.Value ? null : reader["measurement"].ToString();
                        otailoringorder.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                        otailoringorder.tailorname = reader["tailorname"] == DBNull.Value ? "" : reader["tailorname"].ToString();
                        otailoringorder.additionalcharge = reader["additionalcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["additionalcharge"]);
                        otailoringorder.prioritycharge = reader["prioritycharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["prioritycharge"]);
                        otailoringorder.delaycharge = reader["delaycharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["delaycharge"]);
                        otailoringorder.amount = reader["payable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["payable"]);
                        otailoringorder.paid = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        otailoringorder.balance = reader["balance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["balance"]);
                        if(otailoringorder.prioritycharge>0 || otailoringorder.delaycharge>0)
                        {
                            otailoringorder.amount = otailoringorder.amount + otailoringorder.prioritycharge - otailoringorder.delaycharge;
                            otailoringorder.balance = otailoringorder.amount-otailoringorder.paid;
                        }
                        otailoringorder.expectedqty = reader["expectedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["expectedqty"]);
                        otailoringorder.receivedqty = reader["receivedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedqty"]);
                        otailoringorder.charge = reader["charge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["charge"]);
                        otailoringorder.deliveredqty = reader["deliveredqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["deliveredqty"]);
                        otailoringorder.measurementname = reader["measurementname"] == DBNull.Value ? "" : reader["measurementname"].ToString();
                        otailoringorder.designname = reader["designname"] == DBNull.Value ? "" : reader["designname"].ToString();
                        otailoringorder.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();
                        otailoringorder.customermobile = reader["customermobile"] == DBNull.Value ? "" : reader["customermobile"].ToString();
                        otailoringorder.notes = reader["notes"] == DBNull.Value ? "" : reader["notes"].ToString();

                        if (response.Exists(x => x.id == otailoringorder.id)) continue;
                        response.Add(otailoringorder);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _tailoringorder);
            }
            return response;
        }

        internal List<TailoringitemPayments> SearchPayment(TailoringorderCreteria _tailoringorder)
        {
            List<TailoringitemPayments> response = new List<TailoringitemPayments>();
            string query = @"select tailoringitems.id,tailor.name tailorname,receivedon,entryno,product,
                            tailoringitems.charge*tailoringitems.deliveredqty payable,paidamount,
                            measurementname,designname,tailoringitems.charge,tailoringitems.deliveredqty,
                            (tailoringitems.charge*tailoringitems.deliveredqty - paidamount) balance ,paidon 
                            from tailoringitems
                            inner join TailoringOrder on TailoringOrder.id = tailoringitems.TailoringOrderid
                            left outer join tailor on tailor.id = tailorid
                            where TailoringOrder.isactive=true and tailoringitems.isactive=true";
            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = query.Substring(0, query.IndexOf("where"));

            if (_tailoringorder.fromdate.Year > 1900)
                qb.AddParameter("tailoringorder.receivedon", "fromdate", _tailoringorder.fromdate.Date, ">=");
            if (_tailoringorder.todate.Year > 1900)
                qb.AddParameter("tailoringorder.receivedon", "todate", _tailoringorder.todate.Date.AddDays(1), "<");
            if (_tailoringorder.companyid > 0)
                qb.AddParameter("tailoringorder.companyid", _tailoringorder.companyid, "=");
            if (_tailoringorder.locationid > 0)
                qb.AddParameter("tailoringorder.locationid", _tailoringorder.locationid, "=");
            if (_tailoringorder.tailorid > 0)
                qb.AddParameter("tailoringitems.tailorid", _tailoringorder.tailorid, "=");
            if (_tailoringorder.stitchinglocationid > 0)
                qb.AddParameter("tailoringorder.stitchinglocationid", _tailoringorder.stitchinglocationid, "=");

            qb.SortField = "tailoringorder.id desc";
            List<Tailoringorder> lst = new List<Tailoringorder>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TailoringitemPayments otailoringorder = new TailoringitemPayments();
                        otailoringorder.Tailoringitemid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otailoringorder.orderdate = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        otailoringorder.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                        otailoringorder.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                        otailoringorder.tailorname = reader["tailorname"] == DBNull.Value ? "" : reader["tailorname"].ToString();
                        otailoringorder.amount = reader["payable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["payable"]);
                        otailoringorder.balance = reader["balance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["balance"]);
                        otailoringorder.paidon = reader["paidon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paidon"]);
                        otailoringorder.paid = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        otailoringorder.charge = reader["charge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["charge"]);
                        otailoringorder.deliveredqty = reader["deliveredqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["deliveredqty"]);
                        otailoringorder.measurementname = reader["measurementname"] == DBNull.Value ? "" : reader["measurementname"].ToString();
                        otailoringorder.designname = reader["designname"] == DBNull.Value ? "" : reader["designname"].ToString();

                        //measurementname,designname,tailoringitems.charge,
                        response.Add(otailoringorder);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _tailoringorder);
            }
            return response;
        }

        internal void UpdateAdditionalCharges(Bill bill)
        {
            if (bill.billattributes != null &&
                bill.billattributes.deliveryrackid > 0)
            {
                var tailoring = Search(new TailoringorderCreteria()
                {
                    rackid = bill.billattributes.deliveryrackid
                });
                if(tailoring!=null &&
                    tailoring.Count>0)
                {
                    bill.billattributes.tailoringorderid = tailoring[0].id;
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                    var additionalchargelist = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        type = "ADDITIONALCHARGES"
                    });

                    TaxDataService taxDataService = new TaxDataService() { Context = this.Context };
                    var taxlist = taxDataService.Search(new TaxCreteria());
                    bill.additionalchargeslist = new List<Billadditionalcharges>();

                    bill.billattributes.tailoringorderno = string.Join(",", tailoring.ConvertAll(x => x.entryno));

                    foreach (var titem in tailoring)
                    {
                        foreach(var aditem in titem.Tailoringitems)
                        {
                            if(aditem.additionalchargeid>0 &&
                                 additionalchargelist.Exists(x => x.id == aditem.additionalchargeid))
                            {
                                var adcharge = additionalchargelist.Find(x => x.id == aditem.additionalchargeid);
                                Billadditionalcharges billadditionalcharges = new Billadditionalcharges()
                                {
                                    additionalchargestypeid = aditem.additionalchargeid,
                                    Charge = aditem.additionalchargeamount*aditem.expectedqty,
                                    Amount = aditem.additionalchargeamount * aditem.expectedqty,
                                    hsncode = adcharge.additionalinfo.formula,
                                    taxid = adcharge.referenceid
                                };
                                if(taxlist.Exists(x=>x.id == adcharge.referenceid))
                                {
                                    var taxitem = taxlist.Find(x => x.id == adcharge.referenceid);
                                    billadditionalcharges.taxpercentage = taxitem.taxsplit.Find(aditem.additionalchargeamount).taxpercent;
                                    if(billadditionalcharges.taxpercentage>0)
                                    {
                                        billadditionalcharges.Tax = Math.Round(billadditionalcharges.Charge / (100 + billadditionalcharges.taxpercentage) * billadditionalcharges.taxpercentage, 0);
                                        billadditionalcharges.Charge -= billadditionalcharges.Tax;
                                    }
                                }
                                bill.additionalchargeslist.Add(billadditionalcharges);
                                bill.additionalcharges += billadditionalcharges.Amount;
                            }
                        }
                    }
                }
            }
        }
    }

}
