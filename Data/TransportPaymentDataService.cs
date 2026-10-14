using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;


namespace InfyPOS.Data

{
    public class TransportPaymentDataService : SAASData
    {
        const string SQL_Search = @"with lrdetail as (
SELECT transportpaymentid paymentid, string_agg(lrentry.lrentryno, ', ') as lrentrynos
FROM Transportpaymentinvoice inner join lrentry on lrentry.id = Transportpaymentinvoice.lrid
where Transportpaymentinvoice.isactive=true 
group by transportpaymentid
)
        select transportpayment.referencedetail, transportpayment.instrumentdate, transportpayment.paymentmode, transportpayment.bankid, transportpayment.sourcemode, transportpayment.createdon, transportpayment.locationid,
        transportpayment.version, transportpayment.modifiedby, transportpayment.createdby, transportpayment.isactive, transportpayment.paidamount, transportpayment.datatransmitted, transportpayment.paymentdate, transportpayment.receiptno, transportpayment.transportid, 
        transportpayment.modifiedon, transportpayment.id, transportpayment.companyid, transportpayment.remarks, transportpayment.hastds, transportpayment.tdspercentage, transportpayment.tdsamount, transportpayment.rounding,
        lrdetail.lrentrynos
        from transportpayment inner join lrdetail on lrdetail.paymentid = transportpayment.id ";

        const string SQL_Insert = @"INSERT INTO public.transportpayment(sourcemode,referencedetail, instrumentdate, paymentmode, bankid, createdon, version, modifiedby, createdby, isactive, paidamount, datatransmitted, paymentdate, receiptno, transportid, modifiedon, companyid, remarks, hastds, tdspercentage, tdsamount, rounding,locationid) 
        VALUES(@sourcemode,@referencedetail, @instrumentdate, @paymentmode, @bankid, @createdon, @version, @modifiedby, @createdby, @isactive, @paidamount, @datatransmitted, @paymentdate, @receiptno, @transportid, @modifiedon, @companyid, @remarks, @hastds, @tdspercentage, @tdsamount, @rounding,@locationid) RETURNING id";

        const string SQL_Update = @"UPDATE public.transportpayment SET referencedetail=@referencedetail, instrumentdate=@instrumentdate, paymentmode=@paymentmode,sourcemode=@sourcemode, bankid=@bankid, version=@version+1, modifiedby=@modifiedby, paidamount=@paidamount, datatransmitted=@datatransmitted, paymentdate=@paymentdate,
        receiptno=@receiptno, transportid=@transportid, modifiedon=@modifiedon, companyid=@companyid, hastds=@hastds, tdspercentage=@tdspercentage, tdsamount=@tdsamount, remarks=@remarks, rounding=@rounding,locationid=@locationid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.transportpayment set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_pendingcourier = @"select courier.id lrid,courier.amount totalamount, 
courier.taxamount,
0::bigint fromlocationid,
courier.bundles noofbundelsreceived,
0::bigint noofboxesreceived,
courier.description lrno,
courier.weight actualweight,courier.weight chargedweight,courier.amount frightcharge,
0::numeric othercharges,transport.name transportname,
receivedon,0::numeric loadingchargespaid,0::numeric loadingcharge,courier.entryno lrentryno,
case when courier.mode=0 then receivedon else senton end lrdate,
0::numeric frieghtchargespaid,courier.name supplier,courier.senton entrydate,
courier.podnumber invoiceno,courier.poddate invoicedate, '' as receiptno,
case when courier.supplierid>0 then scity.name else ccity.name end fromlocation,
paidinvoice.paidamount,tcity.name transportcity
from courier
left outer join (select sum(paidamount+discountamount) paidamount,lrid
from transportpaymentinvoice where isactive=true and sourcemode = 1
group by lrid) paidinvoice on paidinvoice.lrid = courier.id
inner join transport on transport.id = courier.companyid
left outer join suppliercompany on suppliercompany.supplierid = courier.supplierid
left outer join referencelist scity on  scity.id = suppliercompany.cityid
left outer join customer on customer.id = courier.customerid
left outer join referencelist ccity on  ccity.id = customer.cityid
left outer join referencelist tcity on  tcity.id = transport.cityid
where courier.isactive = true and amount>0
and courier.companyid=@transportid
and courier.receivingcompanyid=@companyid
and courier.receivedon >=@fromdate and courier.receivedon <@todate";


        const string SQL_pendingbills = @"select lrreceived.receiptno,lrreceivedlist.lrid,lrreceivedlist.totalamount, 
0::numeric taxamount,lrreceivedlist.fromlocationid,referencelist.name transportcity,
lrreceivedlist.noofbundelsreceived,lrreceivedlist.noofboxesreceived,lrreceivedlist.lrno,
lrreceivedlist.actualweight,lrreceivedlist.chargedweight,lrreceivedlist.frightcharge,
lrreceivedlist.insurance+lrreceivedlist.others+lrreceivedlist.packing+lrreceivedlist.forward othercharges,
lrentry.receiveddate receivedon,loadingchargespaid,lrreceivedlist.loadingcharge, lrentry.invoiceno, 
lrentry.lrentryno,lrentry.lrdate,lrentry.invoicedate,transport.name transportname,lrentry.lrentrydate entrydate,
frieghtchargespaid,supplier.name supplier,referencelist.name fromlocation,paidinvoice.paidamount from lrreceivedlist
left outer join (select sum(paidamount+discountamount) paidamount,lrid
from transportpaymentinvoice where isactive=true and isloadingcharge = false
and sourcemode = 0
group by lrid) paidinvoice on paidinvoice.lrid = lrreceivedlist.lrid
inner join lrreceived on lrreceived.id = lrreceivedid
inner join lrentry on lrentry.id = lrreceivedlist.lrid
inner join supplier on supplier.id = lrreceivedlist.supplierid
inner join transport on transport.id = lrreceived.bookingofficeid
left outer join referencelist on referencelist.id = lrreceivedlist.fromlocationid
left outer join referencelist tcity on tcity.id = transport.cityid
where lrreceived.isactive = true and lrreceivedlist.isactive = true and frieghtchargespaid=false
and lrreceived.bookingofficeid=@transportid 
and lrreceived.companyid=@companyid
and lrentry.receiveddate >=@fromdate and lrentry.receiveddate <@todate";

        const string SQL_supplierbills = @"
select lrreceivedlist.lrid,lrreceivedlist.totalamount,lrreceivedlist.fromlocationid,
lrreceivedlist.noofbundelsreceived,lrreceivedlist.noofboxesreceived,lrreceivedlist.lrno,
lrreceivedlist.actualweight,lrreceivedlist.chargedweight,lrreceivedlist.frightcharge,
receivedon,loadingchargespaid,lrreceivedlist.loadingcharge,transport.name transportname,
frieghtchargespaid,supplier.name,referencelist.name fromlocation,tcity.name transportcity from lrreceivedlist
left outer join (select sum(paidamount) paidamount,lrid
from transportpaymentinvoice where isloadingcharge = false
group by lrid) paidinvoice on paidinvoice.lrid = lrreceivedlist.lrid
inner join lrreceived on lrreceived.id = lrreceivedid
inner join supplier on supplier.id = lrreceivedlist.supplierid
inner join transport on transport.id = lrreceived.bookingofficeid
left outer join transport tcity on tcity.id = transport.cityid
inner join referencelist on referencelist.id = lrreceivedlist.fromlocationid
where lrreceivedlist.isactive = true";

        const string SQL_courierbills = @"select courier.id lrid,courier.amount totalamount,
0::bigint fromlocationid,
0::bigint noofbundelsreceived,
0::bigint noofboxesreceived,
'' lrno,
courier.weight actualweight,courier.weight chargedweight,courier.weight frightcharge,
0::numeric othercharges,
receivedon,0::numeric loadingchargespaid,0::numeric loadingcharge, courier.entryno lrentryno,
senton lrdate,transport.name transportname,
0::numeric frieghtchargespaid,courier.name supplier,
'' fromlocation,paidinvoice.paidamount from courier
left outer join (select sum(paidamount+discountamount) paidamount,lrid
from transportpaymentinvoice where isactive=true and sourcemode = 1
group by lrid) paidinvoice on paidinvoice.lrid = courier.id
inner join transport on transport.id = courier.companyid
where courier.isactive = true 
and courier.amount>0";

        public List<TransportPayment> Search(TransportPaymentCreteria _transportpayment)
        {
            List<TransportPayment> lst = new List<TransportPayment>();

            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_transportpayment.id != 0)
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search;
                    qb.AddParameter("transportpayment.isactive", true, "=");
                    qb.AddParameter("transportpayment.id", _transportpayment.id, "=");

                    command = qb.GetCommand(conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search;
                    qb.AddParameter("transportpayment.isactive", true, "=");

                    qb.AddParameter("transportpayment.sourcemode", _transportpayment.sourcemode, "=");

                    if (_transportpayment.transportid > 0)
                        qb.AddParameter("transportpayment.transportid", _transportpayment.transportid, "=");

                    if (!string.IsNullOrEmpty(_transportpayment.receiptno))
                        qb.AddParameter("transportpayment.receiptno", _transportpayment.receiptno, "=");

                    if (_transportpayment.fromdate.Date.Year > 1900)
                        qb.AddParameter("transportpayment.paymentdate", "paymentfrom", _transportpayment.fromdate.Date, ">=");
                    if (_transportpayment.todate.Date.Year > 1900)
                        qb.AddParameter("transportpayment.paymentdate", "paymentto", _transportpayment.todate.Date.AddDays(1), "<");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TransportPayment otransportpayment = new TransportPayment();
                        otransportpayment.referencedetail = reader["referencedetail"] == DBNull.Value ? "" : reader["referencedetail"].ToString();
                        otransportpayment.instrumentdate = reader["instrumentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["instrumentdate"]);
                        otransportpayment.paymentmode = (TransportPayment.Paymentmode)(reader["paymentmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["paymentmode"]));
                        otransportpayment.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                        otransportpayment.sourcemode = reader["sourcemode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sourcemode"]);
                        otransportpayment.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        otransportpayment.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        otransportpayment.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        otransportpayment.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        otransportpayment.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        otransportpayment.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        otransportpayment.datatransmitted = reader["datatransmitted"] == DBNull.Value ? false : Convert.ToBoolean(reader["datatransmitted"]);
                        otransportpayment.paymentdate = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                        otransportpayment.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        otransportpayment.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"]);
                        otransportpayment.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);

                        otransportpayment.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        otransportpayment.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otransportpayment.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        otransportpayment.tdspercentage = reader["tdspercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdspercentage"]);
                        otransportpayment.tdsamount = reader["tdsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdsamount"]);
                        otransportpayment.hastds = reader["hastds"] == DBNull.Value ? false : Convert.ToBoolean(reader["hastds"]);
                        otransportpayment.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        otransportpayment.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        otransportpayment.lrentrynos = reader["lrentrynos"] == DBNull.Value ? "" : reader["lrentrynos"].ToString();
                        lst.Add(otransportpayment);
                    }
                }
                if (lst.Count == 1)
                {
                    TransportpaymentinvoiceDataService subTransportpaymentinvoice = new TransportpaymentinvoiceDataService() { Context = this.Context };
                    lst[0].Transportpaymentinvoice = subTransportpaymentinvoice.SearchAll(lst[0].id, lst[0].sourcemode);
                    BankchequetransactionDataService bankchequetransactionDataService = new BankchequetransactionDataService() { Context = this.Context };
                    lst[0].bankcheques = bankchequetransactionDataService.Search(new BankchequetransactionCreteria()
                    {
                        sourceid = lst[0].id,
                        sourcetype = "TPMT"
                    });
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _transportpayment);
            }
            return lst;
        }


        internal List<PendingTransportInvoice> SearchPendingPayment(TransportPaymentCreteria _supplierpayment)
        {
            List<InfyPOS.Data.SupplierpaymentDataService.RangeValue> rangeValues = new List<InfyPOS.Data.SupplierpaymentDataService.RangeValue>();
            if (!string.IsNullOrEmpty(_supplierpayment.ageranges))
            {
                try
                {
                    var ranges = _supplierpayment.ageranges.Split('-');
                    if (ranges.Length == 0)
                    {
                        ranges = _supplierpayment.ageranges.Split(',');
                        int start = int.Parse(ranges[0]);
                        var current = start;
                        if (start > 0)
                        {
                            rangeValues.Add(new InfyPOS.Data.SupplierpaymentDataService.RangeValue()
                            {
                                from = 0,
                                to = start,
                                value = start
                            });
                        }
                        for (int i = 1; i < ranges.Length; i++)
                        {
                            start = int.Parse(ranges[i]);
                            rangeValues.Add(new InfyPOS.Data.SupplierpaymentDataService.RangeValue()
                            {
                                from = current,
                                to = start,
                                value = start
                            });
                            current = start;
                        }
                    }
                    else
                    {
                        int start = int.Parse(ranges[0]);
                        int increment = int.Parse(ranges[1]);
                        int end = int.Parse(ranges[2]);
                        var current = start;
                        rangeValues.Add(new InfyPOS.Data.SupplierpaymentDataService.RangeValue()
                        {
                            from = 0,
                            to = start,
                            value = start
                        });

                        while (current < end)
                        {
                            rangeValues.Add(new InfyPOS.Data.SupplierpaymentDataService.RangeValue()
                            {
                                from = current,
                                to = current + increment,
                                value = current + increment
                            });
                            current += increment;
                        }
                    }
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _supplierpayment);
                }
            }
            List<PendingTransportInvoice> lst = new List<PendingTransportInvoice>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                command = GetCommand(_supplierpayment.sourcemode == 0 ? SQL_pendingbills : SQL_pendingcourier, conn);

                if (_supplierpayment.transportid <= 0)
                {
                    command.CommandText = command.CommandText.Replace("and lrreceived.bookingofficeid=@transportid", "");
                    command.CommandText = command.CommandText.Replace("and courier.companyid=@transportid", "");
                }

                if (_supplierpayment.companyid <= 0)
                {
                    command.CommandText = command.CommandText.Replace("and courier.receivingcompanyid=@companyid", "");
                    command.CommandText = command.CommandText.Replace("and lrreceived.companyid=@companyid", "");
                }

                if (_supplierpayment.fromdate.Date.Year < 2000)
                {
                    command.CommandText = command.CommandText.Replace("and lrentry.receiveddate >=@fromdate and lrentry.receiveddate <@todate", "");
                    command.CommandText = command.CommandText.Replace("and courier.receivedon >=@fromdate and courier.receivedon <@todate", "");
                }
                if (_supplierpayment.fromdate.Date.Year > 2000)
                {
                    command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _supplierpayment.fromdate.Date;
                    command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _supplierpayment.todate.Date.AddDays(1);
                }
                command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _supplierpayment.transportid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _supplierpayment.companyid;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PendingTransportInvoice oinvoice = new PendingTransportInvoice();
                        oinvoice.lrreceiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        oinvoice.supplier = reader["supplier"] == DBNull.Value ? "" : reader["supplier"].ToString();
                        oinvoice.fromlocation = reader["fromlocation"] == DBNull.Value ? "" : reader["fromlocation"].ToString();
                        oinvoice.city = reader["transportcity"] == DBNull.Value ? "" : reader["transportcity"].ToString();
                        oinvoice.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();

                        oinvoice.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        oinvoice.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"]);
                        oinvoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        oinvoice.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        oinvoice.entrydate = reader["entrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                        oinvoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        oinvoice.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"]);
                        oinvoice.transportname = reader["transportname"] == DBNull.Value ? "" : reader["transportname"].ToString();
                        oinvoice.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                        oinvoice.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        oinvoice.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                        oinvoice.source = _supplierpayment.sourcemode == 0 ? "Transport" : "Courier";
                        oinvoice.actualweight = reader["actualweight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["actualweight"]);
                        oinvoice.chargedweight = reader["chargedweight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargedweight"]);
                        oinvoice.frightcharge = reader["frightcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["frightcharge"]);
                        oinvoice.loadingcharge = reader["loadingcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loadingcharge"]);
                        oinvoice.othercharges = reader["othercharges"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["othercharges"]);

                        if (oinvoice.totalamount == 0)
                            oinvoice.totalamount = oinvoice.frightcharge + oinvoice.loadingcharge + oinvoice.othercharges;

                        oinvoice.noofbundelsreceived = reader["noofbundelsreceived"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["noofbundelsreceived"]);
                        oinvoice.noofboxesreceived = reader["noofboxesreceived"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["noofboxesreceived"]);
                        var days = (DateTimeNow.Date - oinvoice.entrydate.Date).TotalDays;
                        if (rangeValues.Count > 0)
                        {
                            oinvoice.days = rangeValues.Exists(e => e.from <= days && e.to >= days) ?
                                rangeValues.Find(e => e.from <= days && e.to >= days).display : rangeValues.Last().to.ToString("N0") + " >";
                        }
                        else
                        {
                            oinvoice.days = days.ToString("N0");
                        }
                        if (oinvoice.totalamount - oinvoice.paidamount > 0)
                            lst.Add(oinvoice);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _supplierpayment);
            }
            return lst;

        }

        public List<PendingLrInvoice> SearchSupplierPaymentSummary(TransportPaymentCreteria _supplierpayment)
        {
            List<PendingTransportInvoice> lst = SearchPendingPayment(_supplierpayment);
            return lst.GroupBy(e => e.transportname).ToList().ConvertAll(e =>
              {
                  return new PendingLrInvoice()
                  {
                      transport = e.Key,
                      invoicecount = e.Count(),
                      totalamount = e.Sum(x => x.totalamount),
                      paidamount = e.Sum(x => x.paidamount),
                      balanceamount = e.Sum(x => x.balanceamount)
                  };
              });
        }


        public List<PendingLrInvoice> SearchSupplierPayment(TransportPaymentCreteria _supplierpayment)
        {
            List<PendingLrInvoice> lst = new List<PendingLrInvoice>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_supplierbills;

                if (_supplierpayment.entryfromdate != DateTime.MinValue && _supplierpayment.entrytodate != DateTime.MinValue)
                {
                    qb.AddParameter("lrinvoice.entrydate", "datefrom", _supplierpayment.entryfromdate.Date, ">=");
                    qb.AddParameter("lrinvoice.entrydate", "dateto", _supplierpayment.entrytodate.Date.AddDays(1), "<");
                }
                if (_supplierpayment.transportid > 0)
                    qb.AddParameter("lrinvoice.companyid", _supplierpayment.transportid, "=");

                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        PendingLrInvoice oinvoice = new PendingLrInvoice();
                        oinvoice.supplier = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oinvoice.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        oinvoice.duedate = reader["duedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["duedate"]);
                        oinvoice.lrinvoiceid = reader["lrinvoiceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrinvoiceid"]);
                        oinvoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        oinvoice.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                        oinvoice.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                        oinvoice.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        oinvoice.balanceamount = oinvoice.totalamount - oinvoice.paidamount;
                        oinvoice.isinvoice = true;
                        if (oinvoice.totalamount - oinvoice.paidamount > 0)
                            lst.Add(oinvoice);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _supplierpayment);
            }
            return lst;

        }

        public TransportPayment Insert(TransportPayment oTransportPayment)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("transportpayment.receiptno", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    oTransportPayment.receiptno = sequence.GetNextNumber(Context, conn, transaction);


                    Insert(transaction, conn, oTransportPayment);
                    TransportpaymentinvoiceDataService subTransportpaymentinvoice = new TransportpaymentinvoiceDataService() { Context = this.Context };
                    foreach (var item in oTransportPayment.Transportpaymentinvoice)
                    {
                        item.transportpaymentid = oTransportPayment.id;
                        item.version = 1;
                        item.isactive = true;
                        item.createdby = this.Context.UserID;
                        item.modifiedby = this.Context.UserID;
                        item.createdon = DateTimeNow;
                        item.modifiedon = DateTimeNow;
                        item.sourcemode = oTransportPayment.sourcemode;
                        subTransportpaymentinvoice.Insert(transaction, conn, item);
                    }

                    TransportDataService suppliercompanyDataService = new TransportDataService() { Context = this.Context };
                    var supplieraccounts = suppliercompanyDataService.Search(new TransportCreteria()
                    {
                        id = oTransportPayment.transportid
                    });

                    if (supplieraccounts.Count == 1)
                    {
                        if (oTransportPayment.paymentinfo == null)
                            oTransportPayment.paymentinfo = new TransportPayment.PaymentInfo();
                        oTransportPayment.paymentinfo.bankacno = supplieraccounts.First().bankaccountno;
                        oTransportPayment.paymentinfo.bankacname = supplieraccounts.First().bankaccountname;
                        oTransportPayment.paymentinfo.bankifsccode = supplieraccounts.First().bankifci;
                        oTransportPayment.paymentinfo.bankname = supplieraccounts.First().bankname;
                        oTransportPayment.paymentinfo.bankbranch = supplieraccounts.First().bankbranch;

                        foreach (var cheque in oTransportPayment.bankcheques)
                        {
                            cheque.entrydate = oTransportPayment.paymentdate;
                            cheque.locationid = oTransportPayment.locationid;
                            cheque.accountname = oTransportPayment.paymentinfo.bankacname;
                            cheque.bankbranch = oTransportPayment.paymentinfo.bankbranch;
                            cheque.bankaccountno = oTransportPayment.paymentinfo.bankacno;
                            cheque.bankifsc = oTransportPayment.paymentinfo.bankifsccode;
                            cheque.referenceno = oTransportPayment.receiptno;
                            cheque.ledgername = supplieraccounts.First().name;
                            cheque.accountname = oTransportPayment.paymentinfo.bankacname;
                        }
                    }

                    BankchequetransactionDataService bankchequetransactionDataService = new BankchequetransactionDataService() { Context = this.Context };
                    foreach (var item in oTransportPayment.bankcheques)
                    {
                        item.sourceid = oTransportPayment.id;
                        item.sourcetype = "TPMT";
                        item.version = 1;
                        item.isactive = true;
                        item.createdby = this.Context.UserID;
                        item.modifiedby = this.Context.UserID;
                        item.createdon = DateTimeNow;
                        item.modifiedon = DateTimeNow;
                        item.status = 0;
                        bankchequetransactionDataService.Insert(transaction, conn, item);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTransportPayment);
                throw exp;
            }

            return oTransportPayment;
        }

        public List<TransportPayment> SearchAll(TransportPaymentCreteria _Transportpayment)
        {
            string SQL_SearchAll = @"with invoicevalue as (
	select Transportpaymentid paymentid,sum(Transportpaymentinvoice.paidamount) paidamount,sum(Transportpaymentinvoice.discountamount) discountamount 
	from Transportpayment inner join Transportpaymentinvoice on Transportpaymentinvoice.Transportpaymentid = Transportpayment.id
where Transportpayment.isactive = true and Transportpaymentinvoice.isactive = true 
group by Transportpaymentid),

invoicedetail as (
SELECT transportpaymentid paymentid, string_agg(lrno, ', ') as invoiceno
FROM transportpaymentinvoice
group by transportpaymentid
)
select transportpayment.id, transportpayment.transportid, receiptno, paymentdate, datatransmitted, 
transportpayment.paidamount, transportpayment.isactive, transportpayment.createdby, 
transportpayment.modifiedby, transportpayment.version, transportpayment.createdon, transportpayment.modifiedon, 
transportpayment.bankid, paymentmode, referencedetail, companyid,
invoicevalue.paidamount invoicetotal, invoicevalue.discountamount invoicediscount, 
transportpayment.remarks, transportpayment.locationid,
bankaccountname,bankaccountno,bankifci,referencelist.name bankname,bankbranch,
tdspercentage,tdsamount,rounding,hastds,invoicedetail.invoiceno
from transportpayment 
inner join invoicevalue on transportpayment.id = invoicevalue.paymentid
left outer join transport on transport.id = transportpayment.transportid
left outer join referencelist on referencelist.id = transport.bankid
left outer join invoicedetail on invoicedetail.paymentid = transportpayment.id
{0}
";

            List<TransportPayment> lst = new List<TransportPayment>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;


                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = string.Format(SQL_SearchAll, "");
                qb.AddParameter("Transportpayment.isactive", true, "=");

                if (_Transportpayment.transportid > 0)
                    qb.AddParameter("Transportpayment.transportid", _Transportpayment.transportid, "=");


                if (_Transportpayment.companyid > 0)
                    qb.AddParameter("Transportpayment.companyid", _Transportpayment.companyid, "=");

                if (!string.IsNullOrEmpty(_Transportpayment.receiptno))
                    qb.AddParameter("Transportpayment.receiptno", _Transportpayment.receiptno, "=");

                if (_Transportpayment.fromdate != DateTime.MinValue && _Transportpayment.todate != DateTime.MinValue)
                {
                    qb.AddParameter("Transportpayment.paymentdate", "paymentfrom", _Transportpayment.fromdate.Date, ">=");
                    qb.AddParameter("Transportpayment.paymentdate", "paymentto", _Transportpayment.todate.AddDays(1).Date, "<");
                }

                qb.SortField = "Transportpayment.id desc";
                command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        TransportPayment oTransportpayment = new TransportPayment();
                        oTransportpayment.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oTransportpayment.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"]);
                        oTransportpayment.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oTransportpayment.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oTransportpayment.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        oTransportpayment.paymentdate = reader["paymentdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["paymentdate"]);
                        oTransportpayment.datatransmitted = reader["datatransmitted"] == DBNull.Value ? false : Convert.ToBoolean(reader["datatransmitted"]);
                        oTransportpayment.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        oTransportpayment.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);

                        oTransportpayment.tdspercentage = reader["tdspercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdspercentage"]);
                        oTransportpayment.tdsamount = reader["tdsamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["tdsamount"]);
                        oTransportpayment.hastds = reader["hastds"] == DBNull.Value ? false : Convert.ToBoolean(reader["hastds"]);
                        oTransportpayment.rounding = reader["rounding"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["rounding"]);
                        oTransportpayment.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oTransportpayment.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oTransportpayment.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oTransportpayment.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oTransportpayment.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        //oTransportpayment.suppliercompanyid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        //oTransportpayment.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oTransportpayment.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                        oTransportpayment.paymentmode = reader["paymentmode"] == DBNull.Value ? 0 : (TransportPayment.Paymentmode)Convert.ToInt16(reader["paymentmode"]);
                        oTransportpayment.referencedetail = reader["referencedetail"] == DBNull.Value ? "" : reader["referencedetail"].ToString();
                        oTransportpayment.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        if (_Transportpayment.id == 0)
                            oTransportpayment.invoicenos = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();

                        if (oTransportpayment.paymentinfo == null) oTransportpayment.paymentinfo = new TransportPayment.PaymentInfo();
                        oTransportpayment.invoicetotal = reader["invoicetotal"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["invoicetotal"]);
                        oTransportpayment.invoicediscount = reader["invoicediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["invoicediscount"]);
                        if (oTransportpayment.paymentinfo == null || oTransportpayment.paymentinfo.bankname == null || oTransportpayment.paymentinfo.bankname == "")
                        {
                            oTransportpayment.paymentinfo.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
                            oTransportpayment.paymentinfo.bankacname = reader["bankaccountname"] == DBNull.Value ? "" : reader["bankaccountname"].ToString();
                            oTransportpayment.paymentinfo.bankacno = reader["bankaccountno"] == DBNull.Value ? "" : reader["bankaccountno"].ToString();
                            oTransportpayment.paymentinfo.bankifsccode = reader["bankifci"] == DBNull.Value ? "" : reader["bankifci"].ToString();
                            oTransportpayment.paymentinfo.bankbranch = reader["bankbranch"] == DBNull.Value ? "" : reader["bankbranch"].ToString();
                        }
                        lst.Add(oTransportpayment);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _Transportpayment);
            }
            return lst;
        }

        public TransportPayment Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, TransportPayment oTransportpayment)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("referencedetail", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransportpayment.referencedetail == null ? "" : oTransportpayment.referencedetail;
            command.Parameters.Add("instrumentdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransportpayment.instrumentdate;
            command.Parameters.Add("paymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransportpayment.paymentmode;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpayment.bankid;
            command.Parameters.Add("sourcemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransportpayment.sourcemode;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransportpayment.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransportpayment.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpayment.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpayment.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransportpayment.isactive;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransportpayment.paidamount;
            command.Parameters.Add("datatransmitted", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransportpayment.datatransmitted;
            command.Parameters.Add("paymentdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransportpayment.paymentdate;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransportpayment.receiptno == null ? "" : oTransportpayment.receiptno;
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpayment.transportid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpayment.locationid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransportpayment.modifiedon;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpayment.companyid;
            command.Parameters.Add("tdspercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransportpayment.tdspercentage;
            command.Parameters.Add("tdsamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransportpayment.tdsamount;
            command.Parameters.Add("hastds", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransportpayment.hastds;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransportpayment.remarks == null ? "" : oTransportpayment.remarks;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransportpayment.rounding;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTransportpayment.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oTransportpayment;
        }

        public TransportPayment Update(TransportPayment oTransportpayment)
        {
            {

                NpgsqlTransaction transaction = null;
                try
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();

                        Update(transaction, conn, oTransportpayment);
                        TransportpaymentinvoiceDataService subTransportpaymentinvoice = new TransportpaymentinvoiceDataService() { Context = this.Context };
                        oTransportpayment.Transportpaymentinvoice.ForEach(e => e.sourcemode = oTransportpayment.sourcemode);
                        subTransportpaymentinvoice.Update(transaction, conn, oTransportpayment.Transportpaymentinvoice,
                            oTransportpayment.id, oTransportpayment.sourcemode);
                        BankchequetransactionDataService bankchequetransactionDataService = new BankchequetransactionDataService() { Context = this.Context };

                        TransportDataService suppliercompanyDataService = new TransportDataService() { Context = this.Context };
                        var supplieraccounts = suppliercompanyDataService.Search(new TransportCreteria()
                        {
                            id = oTransportpayment.transportid
                        });

                        if (supplieraccounts.Count == 1)
                        {
                            if (oTransportpayment.paymentinfo == null)
                                oTransportpayment.paymentinfo = new TransportPayment.PaymentInfo();
                            oTransportpayment.paymentinfo.bankacno = supplieraccounts.First().bankaccountno;
                            oTransportpayment.paymentinfo.bankacname = supplieraccounts.First().bankaccountname;
                            oTransportpayment.paymentinfo.bankifsccode = supplieraccounts.First().bankifci;
                            oTransportpayment.paymentinfo.bankname = supplieraccounts.First().bankname;
                            oTransportpayment.paymentinfo.bankbranch = supplieraccounts.First().bankbranch;

                            foreach (var cheque in oTransportpayment.bankcheques)
                            {
                                cheque.entrydate = oTransportpayment.paymentdate;
                                cheque.locationid = oTransportpayment.locationid;
                                cheque.accountname = oTransportpayment.paymentinfo.bankacname;
                                cheque.bankbranch = oTransportpayment.paymentinfo.bankbranch;
                                cheque.bankaccountno = oTransportpayment.paymentinfo.bankacno;
                                cheque.bankifsc = oTransportpayment.paymentinfo.bankifsccode;
                            }
                        }

                        bankchequetransactionDataService.Update(transaction, conn, oTransportpayment.bankcheques, oTransportpayment.id, "TPMT");
                        transaction.Commit();
                        conn.Close();
                    }
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTransportpayment);
                    throw exp;
                }

                return oTransportpayment;
            }
        }

        public TransportPayment Update(NpgsqlTransaction transaction, NpgsqlConnection conn, TransportPayment oTransportpayment)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("referencedetail", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransportpayment.referencedetail == null ? "" : oTransportpayment.referencedetail;
            command.Parameters.Add("instrumentdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransportpayment.instrumentdate;
            command.Parameters.Add("paymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransportpayment.paymentmode;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpayment.bankid;
            command.Parameters.Add("sourcemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransportpayment.sourcemode;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransportpayment.createdon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransportpayment.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpayment.modifiedby;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpayment.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransportpayment.isactive;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransportpayment.paidamount;
            command.Parameters.Add("datatransmitted", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransportpayment.datatransmitted;
            command.Parameters.Add("paymentdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransportpayment.paymentdate;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransportpayment.receiptno == null ? "" : oTransportpayment.receiptno;
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpayment.transportid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpayment.locationid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransportpayment.modifiedon;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpayment.id;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpayment.companyid;
            command.Parameters.Add("tdspercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransportpayment.tdspercentage;
            command.Parameters.Add("tdsamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransportpayment.tdsamount;
            command.Parameters.Add("hastds", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransportpayment.hastds;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransportpayment.remarks == null ? "" : oTransportpayment.remarks;
            command.Parameters.Add("rounding", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransportpayment.rounding;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTransportpayment.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oTransportpayment;
        }


        public bool Delete(TransportPayment oTransportpayment)
        {
            if (oTransportpayment.id == 0) throw new Exception("Invalid payment Id");

            {
                NpgsqlTransaction transaction = null;
                bool hasSuccessCount = false;
                try
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        hasSuccessCount = Delete(transaction, conn, oTransportpayment);
                        TransportpaymentinvoiceDataService subTransportpaymentinvoice = new TransportpaymentinvoiceDataService() { Context = this.Context };
                        subTransportpaymentinvoice.DeleteAll(oTransportpayment.id, oTransportpayment.modifiedby, conn, transaction);
                        BankchequetransactionDataService bankchequetransactionDataService = new BankchequetransactionDataService() { Context = this.Context };
                        bankchequetransactionDataService.DeleteAll(oTransportpayment.id, "TPMT", conn, transaction);
                        transaction.Commit();
                        conn.Close();
                    }
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTransportpayment);

                    throw exp;
                }
                return hasSuccessCount;
            }
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, TransportPayment oTransportPayment)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransportPayment.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransportPayment.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportPayment.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }
        public TransportPaymentPrint Print(TransportPayment oTransportPayment)
        {
            var payment = Search(new TransportPaymentCreteria()
            {
                id = oTransportPayment.id,
                includebalance = true
            });
            if (payment.Count != 1) return null;

            TransportPaymentPrint transportPaymentPrint = new TransportPaymentPrint();
            transportPaymentPrint.invoicelist = payment[0].Transportpaymentinvoice;
            transportPaymentPrint.Bills = transportPaymentPrint.invoicelist.Where(e => !e.isloadingcharge).ToList();
            transportPaymentPrint.BillPaid = transportPaymentPrint.Bills.Sum(e => e.paidamount);
            transportPaymentPrint.BillDiscount = transportPaymentPrint.Bills.Sum(e => e.discountamount);
            transportPaymentPrint.Returns = transportPaymentPrint.invoicelist.Where(e => e.isloadingcharge).ToList();
            transportPaymentPrint.ReturnAdjusted = transportPaymentPrint.Returns.Sum(e => e.paidamount);
            transportPaymentPrint.paymentdate = payment[0].paymentdate;
            transportPaymentPrint.paymentmode = (int)payment[0].paymentmode;
            transportPaymentPrint.receiptno = payment[0].receiptno;
            transportPaymentPrint.cheques = payment[0].bankcheques;
            transportPaymentPrint.rounding = payment[0].paidamount - (transportPaymentPrint.cheques != null ? transportPaymentPrint.cheques.Sum(e => e.amount) : 0);
            transportPaymentPrint.sourcemode = payment[0].sourcemode;
            transportPaymentPrint.hastds = payment[0].hastds;
            transportPaymentPrint.tdspercentage = payment[0].tdspercentage;
            transportPaymentPrint.tdsamount = payment[0].tdsamount;
            transportPaymentPrint.remarks = payment[0].remarks;
            transportPaymentPrint.taxableamount = payment[0].taxableamount;
            transportPaymentPrint.taxamount = payment[0].taxamount;
            transportPaymentPrint.cgsttaxamount = payment[0].cgsttaxamount;
            transportPaymentPrint.sgsttaxamount = payment[0].sgsttaxamount;

            using (var reader = ExecuteReader("select employee.name from users inner join employee on employeeid = employee.id where users.id=" + payment.First().createdby, null))
            {
                if (reader.Read())
                    transportPaymentPrint.createdby = reader.IsDBNull(0) ? "" : reader.GetString(0);
            }

            if (transportPaymentPrint.cheques != null)
            {
                Data.ReferencelistDataService locationdata = new Data.ReferencelistDataService() { Context = this.Context };
                var bankdata = locationdata.Search(new ReferencelistCreteria()
                {
                    type = "BANK"
                });
                transportPaymentPrint.cheques.ForEach(e =>
                {
                    if (bankdata.Exists(b => b.id == e.bankid))
                        e.bankname = bankdata.Find(b => b.id == e.bankid).name;
                });
            }
            TransportDataService supplierDataService = new TransportDataService() { Context = this.Context };
            var supplier = supplierDataService.Search(new TransportCreteria()
            {
                id = payment.First().transportid
            });

            if (supplier.Count == 1)
                transportPaymentPrint.transport = supplier.First();

            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            var company = companyDataService.Search(new CompanyCreteria()
            {
                id = payment[0].companyid
            });
            if (company.Count == 1)
            {
                transportPaymentPrint.company = company.First();
                transportPaymentPrint.company.parseaddress();
            }
            return transportPaymentPrint;
        }

        internal PrintableLedger PrintSummary(long transportid, long companyid, DateTime fromdate, DateTime todate)
        {
            PrintableLedger printableLedger = new PrintableLedger();
            TransportDataService suppliercompanyDataService = new TransportDataService() { Context = this.Context };
            var transports = suppliercompanyDataService.Search(new TransportCreteria()
            {
                id = transportid
            });

            string query = @"
select sum(lrreceivedlist.totalamount) from lrreceivedlist
inner join lrreceived on lrreceived.id = lrreceivedid
inner join lrentry on lrentry.id = lrreceivedlist.lrid
where lrreceivedlist.totalamount >0 and lrreceivedlist.isactive=true 
and lrreceived.bookingofficeid=@transportid and lrreceived.isactive=true
and lrreceived.companyid=@companyid and lrentry.isactive=true
and lrentry.receiveddate <@fromdate;

select sum(courier.amount)
from courier
inner join transport on transport.id = courier.companyid
where receivingcompanyid=@companyid and courier.companyid=@transportid
and courier.receivedon <@fromdate and courier.isactive=true;

select sum(paidamount) 
from transportpayment where isactive=true
and transportid=@transportid and paymentdate<@fromdate;

select lrentry.receiveddate entrydate, lrentry.lrno invoiceno,lrreceived.receivedon invoicedate,
lrreceivedlist.totalamount,
lrreceivedlist.createdby,lrreceivedlist.modifiedby,
lrreceivedlist.createdon,lrreceivedlist.modifiedon
from lrreceivedlist
inner join lrreceived on lrreceived.id = lrreceivedid
inner join lrentry on lrentry.id = lrreceivedlist.lrid
where lrreceivedlist.totalamount >0
and lrreceived.bookingofficeid=@transportid 
and lrreceived.companyid=@companyid
and lrentry.receiveddate >=@fromdate and lrentry.receiveddate <@todate;

select 
courier.receivedon entrydate, courier.awbnumber,
courier.podnumber,
courier.name,
courier.poddate,
courier.amount,
courier.createdby,courier.modifiedby,
courier.createdon,courier.modifiedon
from courier
inner join transport on transport.id = courier.companyid
where receivingcompanyid=@companyid and courier.companyid=@transportid
and courier.receivedon >=@fromdate and courier.receivedon <@todate;

select paymentdate,paidamount,receiptno,transportpayment.id,
transportpayment.createdby,transportpayment.modifiedby,
transportpayment.createdon,transportpayment.modifiedon
from transportpayment 
where transportpayment.isactive = true and paymentdate>=@fromdate and paymentdate<@todate
and transportid = @transportid and companyid=@companyid;

select transportpayment.id,STRING_AGG(lrno,',') invoiceno  from transportpayment 
inner join transportpaymentinvoice on transportpaymentinvoice.transportpaymentid = transportpayment.id
where transportpayment.isactive = true and transportpaymentinvoice.isactive=true and paymentdate>=@fromdate and paymentdate<@todate
and transportpayment.transportid = @transportid and companyid=@companyid
group by transportpayment.id;";

            var param = new SortedList<string, object>();
            param.Add("transportid", transports[0].id);
            param.Add("companyid", companyid);
            param.Add("fromdate", fromdate.Date);
            param.Add("todate", todate.Date.AddDays(1));

            printableLedger.ledgername = transports[0].name;
            printableLedger.address = transports[0].address;
            printableLedger.title = fromdate.ToString("dd-MM-yyyy") + " - " + todate.ToString("dd-MM-yyyy");

            CompanyDataService companyDataService = new CompanyDataService() { Context = this.Context };
            printableLedger.company = companyDataService.Search(new CompanyCreteria()
            {
                id = companyid
            }).First();

            using (var reader = ExecuteReader(query, param))
            {
                decimal pendingamount = 0;
                if (reader.Read() && !reader.IsDBNull(0))
                {
                    pendingamount = reader.GetDecimal(0);
                }
                if (reader.NextResult() && reader.Read() && !reader.IsDBNull(0))
                {
                    pendingamount += reader.GetDecimal(0);
                }
                if (reader.NextResult() && reader.Read() && !reader.IsDBNull(0))
                {
                    pendingamount -= reader.GetDecimal(0);
                }

                if (pendingamount > 0)
                {
                    printableLedger.obdebit = pendingamount.ToString("F2");
                }
                else
                {
                    printableLedger.obcredit = pendingamount.ToString("F2");
                }

                printableLedger.transactions = new List<PrintableLedger.Transaction>();
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        printableLedger.transactions.Add(new PrintableLedger.Transaction()
                        {
                            createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                            modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                            modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["issuedon"]) : Convert.ToDateTime(reader["modifiedon"]),
                            createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : Convert.ToDateTime(reader["createdon"]),
                            type = "Transport",
                            date = Convert.ToDateTime(reader["entrydate"]),
                            description = String.Format("{0}", reader["invoiceno"].ToString()
                            , Convert.ToDateTime(reader["invoicedate"]).ToString("dd-MM-yyyy")),
                            debit = Convert.ToDecimal(reader["totalamount"]).ToString("F2")
                        });
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        var transaction = new PrintableLedger.Transaction()
                        {
                            createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                            modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                            modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["issuedon"]) : Convert.ToDateTime(reader["modifiedon"]),
                            createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : Convert.ToDateTime(reader["createdon"]),
                            type = "Courier",
                            date = Convert.ToDateTime(reader["entrydate"]),
                            debit = Convert.ToDecimal(reader["amount"]).ToString("F2")
                        };

                        var awbnumber = reader["awbnumber"] == DBNull.Value ? "" : reader["awbnumber"].ToString();
                        var podnumber = reader["podnumber"] == DBNull.Value ? "" : reader["podnumber"].ToString();
                        var customername = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        var description = awbnumber;
                        if (string.IsNullOrEmpty(description))
                            description = podnumber;
                        if (string.IsNullOrEmpty(description))
                            description = customername;
                        transaction.description = String.Format("{0}", description
                            , Convert.ToDateTime(reader["poddate"]).ToString("dd-MM-yyyy"));
                        printableLedger.transactions.Add(transaction);
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        var payment = new PrintableLedger.Transaction()
                        {
                            createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]),
                            modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]),
                            modifiedon = reader["modifiedon"] == DBNull.Value ? Convert.ToDateTime(reader["issuedon"]) : Convert.ToDateTime(reader["modifiedon"]),
                            createdon = reader["createdon"] == DBNull.Value ? Convert.ToDateTime(reader["entrydate"]) : Convert.ToDateTime(reader["createdon"]),
                            type = "Payment",
                            date = Convert.ToDateTime(reader["paymentdate"]),
                            id = Convert.ToInt64(reader["id"]),
                            description = reader["receiptno"].ToString(),
                            credit = Convert.ToDecimal(reader["paidamount"]).ToString("F2")
                        };
                        printableLedger.transactions.Add(payment);
                    }
                }


                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        var id = Convert.ToInt64(reader["id"]);
                        var titem = printableLedger.transactions.Find(x => x.id == id && x.type == "Payment");
                        if (titem != null) titem.description = titem.description + "(" + reader["invoiceno"].ToString() + ")";
                    }
                }

            }

            printableLedger.transactions = printableLedger.transactions.OrderBy(x => x.date).ToList();

            var emplist = new List<long>();
            emplist.AddRange(printableLedger.transactions.ConvertAll(x => x.createdby));
            emplist.AddRange(printableLedger.transactions.ConvertAll(x => x.modifiedby));
            EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
            var empnames = employeeDataService.GetUserName(emplist.Distinct().ToList());
            printableLedger.transactions.ForEach(x =>
            {
                x.cuser = empnames.Exists(y => y.id == x.createdby) ? empnames.Find(y => y.id == x.createdby).name : "";
                x.muser = empnames.Exists(y => y.id == x.modifiedby) ? empnames.Find(y => y.id == x.modifiedby).name : "";
                x.muser = x.modifiedon.ToString("dd-MMM-yyyy") + " @ " + x.muser;
                x.cuser = x.createdon.ToString("dd-MMM-yyyy") + " @ " + x.cuser;
            });

            printableLedger.ProcessTotal();
            return printableLedger;
        }
    }

}
