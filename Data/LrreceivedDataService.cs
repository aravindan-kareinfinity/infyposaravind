using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class LrreceivedDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, receiptno, bookingofficeid, receivedon,
receivedby, issueid, issueno, issuedate, frieghtcharges, noofbundels, noofpieces,securityinwardno,securityinwarddate,loadingchargespaid,frieghtchargespaid,
noofboxes,actualweight,chargedweight,loadingcharges,companyid,othercharges,totalcharges,receivedweight from lrreceived where id=@id";

        const string SQL_SearchbySupplier = @"select lrreceived.id, lrreceived.isactive, lrreceived.createdby, lrreceived.modifiedby, lrreceived.version, lrreceived.createdon, lrreceived.modifiedon, lrreceived.receiptno, 
lrreceived.bookingofficeid, lrreceived.receivedon, lrreceived.receivedby, lrreceived.issueid, lrreceived.issueno, lrreceived.issuedate, lrreceived.frieghtcharges, 
lrreceived.noofbundels, lrreceived.noofpieces,lrreceived.securityinwardno,lrreceived.securityinwarddate,lrreceived.noofboxes,lrreceived.actualweight,
lrreceived.chargedweight,lrreceived.loadingcharges,loadingchargespaid,frieghtchargespaid,companyid,othercharges,totalcharges,lrreceived.receivedweight,lrreceivedlist.lrno
from lrreceived left outer join lrreceivedlist on lrreceived.id=lrreceivedlist.lrreceivedid 
where lrreceived.id=@id";

        const string SQL_Insert = @"INSERT INTO public.lrreceived(isactive, createdby, modifiedby, version, createdon, modifiedon, receiptno, bookingofficeid, receivedon, receivedby, issueid, issueno, issuedate, frieghtcharges, noofbundels, noofpieces,securityinwardno,securityinwarddate,noofboxes,actualweight,chargedweight,loadingcharges,loadingchargespaid,frieghtchargespaid,companyid,othercharges,totalcharges,receivedweight)
        VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @receiptno, @bookingofficeid, @receivedon, @receivedby, @issueid, @issueno, @issuedate, @frieghtcharges, @noofbundels, @noofpieces,@securityinwardno,@securityinwarddate,@noofboxes,@actualweight,@chargedweight,@loadingcharges,@loadingchargespaid,@frieghtchargespaid,@companyid,@othercharges,@totalcharges,@receivedweight) RETURNING id";


        const string SQL_Update = @"UPDATE public.lrreceived SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, receiptno=@receiptno, bookingofficeid=@bookingofficeid, receivedon=@receivedon, receivedby=@receivedby, issueid=@issueid,
            issueno=@issueno, issuedate=@issuedate, frieghtcharges=@frieghtcharges, noofbundels=@noofbundels, noofpieces=@noofpieces,loadingchargespaid=@loadingchargespaid,
            securityinwardno=@securityinwardno,securityinwarddate=@securityinwarddate,noofboxes=@noofboxes,actualweight=@actualweight,frieghtchargespaid=@frieghtchargespaid,
            chargedweight=@chargedweight,loadingcharges=@loadingcharges,companyid=@companyid,othercharges=@othercharges,totalcharges=@totalcharges,receivedweight=@receivedweight
            where id=@id and version=@version  RETURNING version";

        const string SQL_Delete = @"update public.lrreceived set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        const string SQL_ChangeMode = @"update public.lrentry set status=@status where id=@id";
        public List<Lrreceived> Search(LrreceivedCreteria _lrreceived)
        {
            List<Lrreceived> lst = new List<Lrreceived>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_lrreceived.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrreceived.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();

                    if (_lrreceived.supplierid > 0 || !string.IsNullOrEmpty(_lrreceived.lrno))
                        qb.CurrentQuery = SQL_SearchbySupplier.Substring(0, SQL_SearchbySupplier.IndexOf("where"));
                    else
                        qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("lrreceived.isactive", true, "=");

                    if (_lrreceived.bookingofficeid > 0)
                        qb.AddParameter("lrreceived.bookingofficeid", _lrreceived.bookingofficeid, "=");

                    if (!string.IsNullOrEmpty(_lrreceived.receiptno))
                        qb.AddParameter("lrreceived.receiptno", _lrreceived.receiptno, "=");

                    if (_lrreceived.receivedon != DateTime.MinValue)
                    {
                        qb.AddParameter("lrreceived.receivedon", "receivedfrom", _lrreceived.receivedon.Date, ">=");
                        qb.AddParameter("lrreceived.receivedon", "receivedto", _lrreceived.receivedon.AddDays(1).Date, "<");
                    }

                    if (_lrreceived.supplierid > 0)
                        qb.AddParameter("lrreceivedlist.supplierid", _lrreceived.supplierid, "=");
                    if (!string.IsNullOrEmpty(_lrreceived.lrno))
                        qb.AddParameter("lrreceivedlist.lrno", _lrreceived.lrno, "=");
                    qb.SortField = "lrreceived.id desc";
                    qb.Limit = 100;
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lrreceived olrreceived = new Lrreceived();
                        olrreceived.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"].ToString());
                        olrreceived.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        olrreceived.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"].ToString());
                        olrreceived.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"].ToString());
                        olrreceived.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"].ToString());
                        olrreceived.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"].ToString());
                        olrreceived.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"].ToString());
                        olrreceived.receiptno = reader["receiptno"] == DBNull.Value ? "" : reader["receiptno"].ToString();
                        olrreceived.bookingofficeid = reader["bookingofficeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bookingofficeid"].ToString());
                        olrreceived.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"].ToString());
                        olrreceived.receivedby = reader["receivedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivedby"].ToString());
                        olrreceived.issueid = reader["issueid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issueid"].ToString());
                        olrreceived.issueno = reader["issueno"] == DBNull.Value ? "" : reader["issueno"].ToString();
                        olrreceived.noofbundels = reader["noofbundels"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["noofbundels"].ToString());
                        olrreceived.noofpieces = reader["noofpieces"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["noofpieces"].ToString());
                        olrreceived.frieghtcharges = reader["frieghtcharges"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["frieghtcharges"].ToString());
                        olrreceived.noofboxes = reader["noofboxes"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["noofboxes"].ToString());
                        olrreceived.actualweight = reader["actualweight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["actualweight"].ToString());
                        olrreceived.chargedweight = reader["chargedweight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargedweight"].ToString());
                        olrreceived.loadingcharges = reader["loadingcharges"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loadingcharges"].ToString());
                        olrreceived.loadingchargespaid = reader["loadingchargespaid"] == DBNull.Value ? false : Convert.ToBoolean(reader["loadingchargespaid"].ToString());
                        olrreceived.frieghtchargespaid = reader["frieghtchargespaid"] == DBNull.Value ? false : Convert.ToBoolean(reader["frieghtchargespaid"].ToString());

                        if (_lrreceived.supplierid > 0 || !string.IsNullOrEmpty(_lrreceived.lrno))
                            olrreceived.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();

                        olrreceived.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrreceived.issuedate = reader["issuedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedate"].ToString());
                        olrreceived.securityinwardno = reader["securityinwardno"] == DBNull.Value ? "" : reader["securityinwardno"].ToString();
                        olrreceived.securityinwarddate = reader["securityinwarddate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["securityinwarddate"].ToString());
                        olrreceived.othercharges = reader["othercharges"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["othercharges"].ToString());
                        olrreceived.totalcharges = reader["totalcharges"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalcharges"].ToString());
                        olrreceived.receivedweight = reader["receivedweight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedweight"].ToString());
                        //receivedweight

                        lst.Add(olrreceived);
                    }
                }
                conn.Close();
                if (lst.Count == 1)
                {
                    LrreceivedlistDataService lrreceiveddata = new LrreceivedlistDataService() { Context = this.Context };
                    lst[0].receiptitem = lrreceiveddata.Search(new LrreceivedlistCreteria { lrreceivedid = lst[0].id });
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

        public Lrreceived Insert(Lrreceived oLrreceived)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    InsertDirect(transaction, conn, oLrreceived);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oLrreceived;
        }
        public Lrreceived InsertDirect(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrreceived oLrreceived)
        {
            var sequence = SequenceManager.Instance(Context.OrganizationID).Get("lrreceived.receiptno", 0);

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            oLrreceived.receiptno = sequence.GetNextNumber(Context, conn, transaction);
            Insert(transaction, conn, oLrreceived);

            LrreceivedlistDataService recieveditems = new LrreceivedlistDataService() { Context = this.Context };
            recieveditems.Context = this.Context;
            foreach (var item in oLrreceived.receiptitem)
            {
                long lrentryid = item.lrid;
                item.lrreceivedid = oLrreceived.id;
                recieveditems.Insert(transaction, conn, item);

                command = GetCommand(SQL_ChangeMode, conn, transaction);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = lrentryid;
                command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)(Models.Inbox.StatusList.LRIssue);
                command.ExecuteNonQuery();

                if (oLrreceived.version >= 0)
                    InboxManager.Instance(Context.OrganizationID).Update(Context, oLrreceived, item, transaction, conn);
            }
            if (oLrreceived.version >= 0)
                InboxManager.Instance(Context.OrganizationID).Update(Context, oLrreceived.issueid, oLrreceived, transaction, conn);
            return oLrreceived;
        }
        public Lrreceived Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrreceived oLrreceived)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrreceived.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrreceived.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrreceived.receiptno == null ? "" : oLrreceived.receiptno;
            command.Parameters.Add("bookingofficeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceived.bookingofficeid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceived.companyid;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrreceived.receivedon;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceived.receivedby;
            command.Parameters.Add("issueid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceived.issueid;
            command.Parameters.Add("issueno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrreceived.issueno == null ? "" : oLrreceived.issueno;
            command.Parameters.Add("issuedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrreceived.issuedate;
            command.Parameters.Add("frieghtcharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.frieghtcharges;

            command.Parameters.Add("noofboxes", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.noofboxes;
            command.Parameters.Add("actualweight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.actualweight;
            command.Parameters.Add("chargedweight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.chargedweight;
            command.Parameters.Add("loadingcharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.loadingcharges;
            command.Parameters.Add("frieghtchargespaid", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrreceived.frieghtchargespaid;
            command.Parameters.Add("loadingchargespaid", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrreceived.loadingchargespaid;

            command.Parameters.Add("noofbundels", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.noofbundels;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.noofpieces;
            command.Parameters.Add("securityinwardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrreceived.securityinwardno == null ? "" : oLrreceived.securityinwardno;
            command.Parameters.Add("securityinwarddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrreceived.securityinwarddate;
            command.Parameters.Add("othercharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.othercharges;
            command.Parameters.Add("totalcharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.totalcharges;
            command.Parameters.Add("receivedweight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.receivedweight;
            

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrreceived.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oLrreceived;
        }

        public Lrreceived Update(Lrreceived oLrreceived)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oLrreceived);
                    LrreceivedlistDataService recieveditems = new LrreceivedlistDataService() { Context = this.Context };
                    recieveditems.Context = this.Context;
                    foreach (var item in oLrreceived.receiptitem)
                    {
                        long lrissueid = item.id;
                        item.lrreceivedid = oLrreceived.id;
                        recieveditems.Update(transaction, conn, item);
                        InboxManager.Instance(Context.OrganizationID).Update(Context, oLrreceived.issueid, oLrreceived, transaction, conn);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oLrreceived;
        }

        public Lrreceived Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrreceived oLrreceived)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceived.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrreceived.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceived.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrreceived.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrreceived.createdon;
            command.Parameters.Add("frieghtchargespaid", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrreceived.frieghtchargespaid;
            command.Parameters.Add("loadingchargespaid", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrreceived.loadingchargespaid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("receiptno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrreceived.receiptno == null ? "" : oLrreceived.receiptno;
            command.Parameters.Add("bookingofficeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceived.bookingofficeid;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrreceived.receivedon;
            command.Parameters.Add("receivedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceived.receivedby;
            command.Parameters.Add("issueid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceived.issueid;
            command.Parameters.Add("issueno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrreceived.issueno == null ? "" : oLrreceived.issueno;
            command.Parameters.Add("issuedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrreceived.issuedate;
            command.Parameters.Add("frieghtcharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.frieghtcharges;
            command.Parameters.Add("noofbundels", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.noofbundels;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.noofpieces;
            command.Parameters.Add("securityinwardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrreceived.securityinwardno == null ? "" : oLrreceived.securityinwardno;
            command.Parameters.Add("securityinwarddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrreceived.securityinwarddate;
            command.Parameters.Add("noofboxes", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.noofboxes;
            command.Parameters.Add("actualweight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.actualweight;
            command.Parameters.Add("chargedweight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.chargedweight;
            command.Parameters.Add("loadingcharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.loadingcharges;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceived.companyid;
            command.Parameters.Add("othercharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.othercharges;
            command.Parameters.Add("totalcharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.totalcharges;
            command.Parameters.Add("receivedweight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrreceived.receivedweight;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrreceived.version = Convert.ToInt32(reader[0]);
                }
            }

            return oLrreceived;
        }

        public bool Delete(Lrreceived oLrreceived)
        {

            NpgsqlTransaction transaction = null;
            bool successCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    successCount = Delete(transaction, conn, oLrreceived);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return successCount;
        }
        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrreceived oLrreceived)
        {
            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrreceived.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrreceived.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrreceived.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
