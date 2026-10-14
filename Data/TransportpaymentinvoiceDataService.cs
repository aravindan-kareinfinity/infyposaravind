using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class TransportpaymentinvoiceDataService : SAASData
    {
        const string SQL_Search = @"select transportid, createdby, isactive, createdon, paidamount, lrno, isloadingcharge, transportpaymentid, modifiedon, lrid, version, modifiedby, id, discountamount, discountamountpercent, receivedon, lrdate,sourcemode from transportpaymentinvoice where id=@id";
        const string SQL_SearchAll = @"select Transportpaymentinvoice.*,
supplier.name supplier,referencelist.name fromlocation,1 as paymode,
lrreceivedlist.noofbundelsreceived,lrreceivedlist.noofboxesreceived,lrreceivedlist.lrno,
lrreceivedlist.actualweight,lrreceivedlist.chargedweight,lrreceivedlist.frightcharge,
lrreceivedlist.others+lrreceivedlist.packing+lrreceivedlist.forward othercharges,
paidinvoice.paidamount balance,lrentryno,lrreceivedlist.totalamount,lrreceivedlist.loadingcharge,
lrentry.invoiceno, 0::numeric taxpercentage, 0::numeric taxamount 
from Transportpaymentinvoice
inner join lrreceivedlist on lrreceivedlist.lrid = Transportpaymentinvoice.lrid
inner join lrentry on lrentry.id = Transportpaymentinvoice.lrid
inner join supplier on supplier.id = lrreceivedlist.supplierid
inner join referencelist on referencelist.id = lrreceivedlist.fromlocationid
left outer join (select sum(paidamount+discountamount) paidamount,lrid
from transportpaymentinvoice where isloadingcharge = false
and transportpaymentinvoice.transportpaymentid != @transportpaymentid 
group by lrid) paidinvoice on paidinvoice.lrid = Transportpaymentinvoice.lrid 
where Transportpaymentinvoice.isactive=true and lrreceivedlist.isactive=true and transportpaymentid=@transportpaymentid";

        const string SQL_CourierAll = @"select Transportpaymentinvoice.*,courier.description,
courier.name supplier,case when courier.supplierid>0 then scity.name else ccity.name end fromlocation,
courier.bundles noofbundelsreceived,0::bigint noofboxesreceived,courier.description lrno,
courier.weight actualweight,courier.weight chargedweight,courier.amount frightcharge,
0::numeric othercharges,courier.paymode,
paidinvoice.paidamount balance,courier.entryno lrentryno,
courier.amount totalamount,0::numeric loadingcharge,
courier.podnumber invoiceno, courier.taxpercentage, courier.taxamount
from Transportpaymentinvoice
inner join courier on courier.id = Transportpaymentinvoice.lrid
left outer join suppliercompany on suppliercompany.supplierid = courier.supplierid
left outer join referencelist scity on  scity.id = suppliercompany.cityid
left outer join customer on customer.id = courier.customerid
left outer join referencelist ccity on  ccity.id = customer.cityid
left outer join (select sum(paidamount+discountamount) paidamount,lrid
from transportpaymentinvoice where isloadingcharge = false
and transportpaymentinvoice.transportpaymentid != @transportpaymentid 
group by lrid) paidinvoice on paidinvoice.lrid = Transportpaymentinvoice.lrid 
where Transportpaymentinvoice.isactive=true and transportpaymentid=@transportpaymentid
and (Transportpaymentinvoice.paidamount>0 or Transportpaymentinvoice.discountamount>0)";

        const string SQL_Insert = @"INSERT INTO public.transportpaymentinvoice(sourcemode,transportid, createdby, isactive, createdon, paidamount, lrno, isloadingcharge, transportpaymentid, modifiedon, lrid, version, modifiedby, discountamount, discountamountpercent, receivedon, lrdate) 
        VALUES(@sourcemode,@transportid, @createdby, @isactive, @createdon, @paidamount, @lrno, @isloadingcharge, @transportpaymentid, @modifiedon, @lrid, @version, @modifiedby, @discountamount, @discountamountpercent, @receivedon, @lrdate) RETURNING id";


        const string SQL_Update = @"UPDATE public.transportpaymentinvoice SET transportid=@transportid, paidamount=@paidamount, lrno=@lrno,isloadingcharge=@isloadingcharge, transportpaymentid=@transportpaymentid, modifiedon=@modifiedon, lrid=@lrid, version=@version+1, modifiedby=@modifiedby, discountamount=@discountamount, discountamountpercent=@discountamountpercent, receivedon=@receivedon, lrdate=@lrdate where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.transportpaymentinvoice set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";
        const string SQL_DeleteAll = @"update public.transportpaymentinvoice set isactive=false,modifiedby=@modifiedby,modifiedon=@modifiedon where transportpaymentid=@transportpaymentid";
        public List<Transportpaymentinvoice> Search(TransportpaymentinvoiceCreteria _transportpaymentinvoice)
        {
            List<Transportpaymentinvoice> lst = new List<Transportpaymentinvoice>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_transportpaymentinvoice.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _transportpaymentinvoice.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    //if (!string.IsNullOrEmpty(_transportpaymentinvoice.Code))
                    //    qb.AddParameter("code", _transportpaymentinvoice.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Transportpaymentinvoice otransportpaymentinvoice = new Transportpaymentinvoice();
                        otransportpaymentinvoice.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"]);
                        otransportpaymentinvoice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        otransportpaymentinvoice.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        otransportpaymentinvoice.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        otransportpaymentinvoice.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        otransportpaymentinvoice.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        otransportpaymentinvoice.sourcemode = reader["sourcemode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sourcemode"]);
                        otransportpaymentinvoice.isloadingcharge = reader["isloadingcharge"] == DBNull.Value ? false : Convert.ToBoolean(reader["isloadingcharge"]);
                        otransportpaymentinvoice.transportpaymentid = reader["transportpaymentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportpaymentid"]);
                        otransportpaymentinvoice.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        otransportpaymentinvoice.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"]);
                        otransportpaymentinvoice.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        otransportpaymentinvoice.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        otransportpaymentinvoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otransportpaymentinvoice.discountamount = reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"]);
                        otransportpaymentinvoice.discountamountpercent = reader["discountamountpercent"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamountpercent"]);
                        otransportpaymentinvoice.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        otransportpaymentinvoice.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"]);
                        lst.Add(otransportpaymentinvoice);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _transportpaymentinvoice);
            }
            return lst;
        }

        public List<Transportpaymentinvoicedetail> SearchAll(long _transportpaymentid,int sourcemode)
        {
            List<Transportpaymentinvoicedetail> lst = new List<Transportpaymentinvoicedetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(sourcemode == 0? SQL_SearchAll: SQL_CourierAll, conn);
                command.Parameters.Add("transportpaymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _transportpaymentid;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Transportpaymentinvoicedetail otransportpaymentinvoice = new Transportpaymentinvoicedetail();
                        otransportpaymentinvoice.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"]);
                        otransportpaymentinvoice.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        otransportpaymentinvoice.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        otransportpaymentinvoice.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        otransportpaymentinvoice.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        if(sourcemode!=0)
                        {
                            otransportpaymentinvoice.lrno = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        }
                        otransportpaymentinvoice.sourcemode = reader["sourcemode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sourcemode"]);
                        otransportpaymentinvoice.isloadingcharge = reader["isloadingcharge"] == DBNull.Value ? false : Convert.ToBoolean(reader["isloadingcharge"]);
                        otransportpaymentinvoice.transportpaymentid = reader["transportpaymentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportpaymentid"]);
                        otransportpaymentinvoice.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        otransportpaymentinvoice.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"]);
                        otransportpaymentinvoice.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        otransportpaymentinvoice.paymode = reader["paymode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["paymode"]);
                        otransportpaymentinvoice.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        otransportpaymentinvoice.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        otransportpaymentinvoice.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        otransportpaymentinvoice.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"]);
                        otransportpaymentinvoice.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();

                        otransportpaymentinvoice.paidamount = reader["paidamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidamount"]);
                        otransportpaymentinvoice.discountamount = reader["discountamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["discountamount"]);
                        otransportpaymentinvoice.balanceamount = reader["balance"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["balance"]);

                        otransportpaymentinvoice.supplier = reader["supplier"] == DBNull.Value ? "" : reader["supplier"].ToString();
                        otransportpaymentinvoice.fromlocation = reader["fromlocation"] == DBNull.Value ? "" : reader["fromlocation"].ToString();
                        otransportpaymentinvoice.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        otransportpaymentinvoice.lrid = reader["lrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrid"]);
                        otransportpaymentinvoice.sourcemode = reader["sourcemode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["sourcemode"]);
                        otransportpaymentinvoice.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        otransportpaymentinvoice.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"]);
                        otransportpaymentinvoice.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                        otransportpaymentinvoice.actualweight = reader["actualweight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["actualweight"]);
                        otransportpaymentinvoice.chargedweight = reader["chargedweight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargedweight"]);
                        otransportpaymentinvoice.frightcharge = reader["frightcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["frightcharge"]);
                        otransportpaymentinvoice.loadingcharge = reader["loadingcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loadingcharge"]);
                        otransportpaymentinvoice.othercharges = reader["othercharges"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["othercharges"]);
                        otransportpaymentinvoice.noofbundelsreceived = reader["noofbundelsreceived"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["noofbundelsreceived"]);
                        otransportpaymentinvoice.noofboxesreceived = reader["noofboxesreceived"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["noofboxesreceived"]);
                        otransportpaymentinvoice.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        otransportpaymentinvoice.taxamount = reader["taxamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxamount"]);
                        otransportpaymentinvoice.taxableamount = otransportpaymentinvoice.totalamount - otransportpaymentinvoice.taxamount;

                        lst.Add(otransportpaymentinvoice);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _transportpaymentid);
            }
            return lst;
        }

        public Transportpaymentinvoice Insert(Transportpaymentinvoice oTransportpaymentinvoice)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oTransportpaymentinvoice);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTransportpaymentinvoice);
                throw exp;
            }

            return oTransportpaymentinvoice;
        }

        public Transportpaymentinvoice Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Transportpaymentinvoice oTransportpaymentinvoice)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpaymentinvoice.transportid;
            command.Parameters.Add("sourcemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransportpaymentinvoice.sourcemode;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpaymentinvoice.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransportpaymentinvoice.isactive;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransportpaymentinvoice.createdon;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransportpaymentinvoice.paidamount;
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransportpaymentinvoice.lrno == null ? "" : oTransportpaymentinvoice.lrno;
            command.Parameters.Add("isloadingcharge", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransportpaymentinvoice.isloadingcharge;
            command.Parameters.Add("transportpaymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpaymentinvoice.transportpaymentid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransportpaymentinvoice.modifiedon;
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpaymentinvoice.lrid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransportpaymentinvoice.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpaymentinvoice.modifiedby;
            command.Parameters.Add("discountamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransportpaymentinvoice.discountamount;
            command.Parameters.Add("discountamountpercent", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransportpaymentinvoice.discountamountpercent;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransportpaymentinvoice.receivedon;
            command.Parameters.Add("lrdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransportpaymentinvoice.lrdate;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTransportpaymentinvoice.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oTransportpaymentinvoice;
        }

        public Transportpaymentinvoice Update(Transportpaymentinvoice oTransportpaymentinvoice)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oTransportpaymentinvoice);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTransportpaymentinvoice);
                throw exp;
            }

            return oTransportpaymentinvoice;
        }

        public Transportpaymentinvoice Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Transportpaymentinvoice oTransportpaymentinvoice)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpaymentinvoice.transportid;
            command.Parameters.Add("sourcemode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransportpaymentinvoice.sourcemode;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpaymentinvoice.createdby;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransportpaymentinvoice.isactive;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransportpaymentinvoice.createdon;
            command.Parameters.Add("paidamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransportpaymentinvoice.paidamount;
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oTransportpaymentinvoice.lrno == null ? "" : oTransportpaymentinvoice.lrno;
            command.Parameters.Add("isloadingcharge", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransportpaymentinvoice.isloadingcharge;
            command.Parameters.Add("transportpaymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpaymentinvoice.transportpaymentid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransportpaymentinvoice.modifiedon;
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpaymentinvoice.lrid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransportpaymentinvoice.version;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpaymentinvoice.modifiedby;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpaymentinvoice.id;
            command.Parameters.Add("discountamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransportpaymentinvoice.discountamount;
            command.Parameters.Add("discountamountpercent", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oTransportpaymentinvoice.discountamountpercent;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransportpaymentinvoice.receivedon;
            command.Parameters.Add("lrdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransportpaymentinvoice.lrdate;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oTransportpaymentinvoice.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oTransportpaymentinvoice;
        }

        public bool Update(NpgsqlTransaction transaction, NpgsqlConnection conn, 
            List<Transportpaymentinvoicedetail> list, long id,int sourcemode)
        {
            var existingItems = SearchAll(id, sourcemode);
            if (existingItems != null && existingItems.Count > 0)
            {
                var deletedItems = existingItems.FindAll(e => !list.Exists(f => f.id == e.id));
                foreach (var item in deletedItems)
                    Delete(transaction, conn, item);
            }

            foreach (var item in list)
            {
                if (item.id > 0)
                {
                    Update(transaction, conn, item);
                }
                else
                {
                    item.transportpaymentid = id;
                    Insert(transaction, conn, item);
                }
            }
            return true;
        }

        public bool Delete(Transportpaymentinvoice oTransportpaymentinvoice)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oTransportpaymentinvoice);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oTransportpaymentinvoice);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Transportpaymentinvoice oTransportpaymentinvoice)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oTransportpaymentinvoice.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpaymentinvoice.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oTransportpaymentinvoice.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oTransportpaymentinvoice.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oTransportpaymentinvoice.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public bool DeleteAll(long transportpaymentid, long userid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {


            int successCount = 0;
            NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = userid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("transportpaymentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = transportpaymentid;
            successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
