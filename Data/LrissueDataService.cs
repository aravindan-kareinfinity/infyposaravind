using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class LrissueDataService : SAASData
    {
        const string SQL_Search = @"select id, isactive, createdby, modifiedby, version, createdon, modifiedon, bookingofficeid,companyid, collectingby, issueno, issuedate, receivinglocationid, fromlocationid, noofpieces, noofbundels, frieghtcharges from lrissue where id=@id";

        const string SQL_SearchbySupplier = @"select lrissue.id, lrissue.isactive, lrissue.createdby, lrissue.modifiedby, lrissue.version, lrissue.createdon, lrissue.modifiedon, lrissue.bookingofficeid,companyid, 
lrissue.collectingby, lrissue.issueno, lrissue.issuedate, lrissue.receivinglocationid, lrissue.fromlocationid, lrissue.noofpieces, lrissue.noofbundels, lrissue.frieghtcharges, lrissuelist.supplierid 
from lrissue left outer join lrissuelist on lrissue.id=lrissuelist.lrissueid
where lrissue.id=@id";

        const string SQL_Insert = @"INSERT INTO public.lrissue(isactive, createdby, modifiedby, version, createdon, modifiedon, bookingofficeid,companyid, collectingby, issueno, issuedate, receivinglocationid, fromlocationid, noofpieces, noofbundels, frieghtcharges) VALUES(@isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @bookingofficeid,@companyid, @collectingby, @issueno, @issuedate, @receivinglocationid, @fromlocationid, @noofpieces, @noofbundels, @frieghtcharges) RETURNING id";

        const string SQL_Update = @"UPDATE public.lrissue SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, bookingofficeid=@bookingofficeid,companyid=@companyid, collectingby=@collectingby, issueno=@issueno, issuedate=@issuedate, receivinglocationid=@receivinglocationid, fromlocationid=@fromlocationid, noofpieces=@noofpieces, noofbundels=@noofbundels, frieghtcharges=@frieghtcharges where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.lrissue set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        const string SQL_ChangeMode = @"update public.lrentry set status=@status where id=@id";

        public List<Lrissue> Search(LrissueCreteria _lrissue)
        {
            List<Lrissue> lst = new List<Lrissue>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_lrissue.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrissue.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    if (_lrissue.supplierid > 0)
                    {
                        qb.CurrentQuery = SQL_SearchbySupplier.Substring(0, SQL_SearchbySupplier.IndexOf("where"));
                        qb.AddParameter("lrissuelist.supplierid", _lrissue.supplierid, "=");
                    }
                    else
                        qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                    qb.AddParameter("lrissue.isactive", true, "=");
                    if (_lrissue.bookingofficeid > 0)
                        qb.AddParameter("lrissue.bookingofficeid", _lrissue.bookingofficeid, "=");

                    if (_lrissue.companyid > 0)
                        qb.AddParameter("lrissue.companyid", _lrissue.companyid, "=");

                    if (!string.IsNullOrEmpty(_lrissue.issueno))
                        qb.AddParameter("lrissue.issueno", _lrissue.issueno, "=");

                    if (_lrissue.issuedate != DateTime.MinValue)
                    {
                        qb.AddParameter("lrissue.issuedate", "issuedate", _lrissue.issuedate.Date, "=");
                    }

                    if (_lrissue.id > 0)
                        qb.AddParameter("lrissue.id", _lrissue.id, "=");

                    if (_lrissue.fromlocationid > 0)
                        qb.AddParameter("lrissue.fromlocationid", _lrissue.fromlocationid, "=");

                    qb.SortField = "lrissue.id desc";
                    qb.Limit = 100;
                    command = qb.GetCommand(conn);
                }
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lrissue olrissue = new Lrissue();
                        olrissue.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"].ToString());
                        olrissue.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"].ToString());
                        olrissue.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"].ToString());
                        olrissue.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"].ToString());
                        olrissue.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"].ToString());
                        olrissue.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"].ToString());
                        olrissue.bookingofficeid = reader["bookingofficeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bookingofficeid"].ToString());
                        olrissue.collectingby = reader["collectingby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["collectingby"].ToString());
                        olrissue.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"].ToString());
                        olrissue.issueno = reader["issueno"] == DBNull.Value ? "" : reader["issueno"].ToString();
                        olrissue.issuedate = reader["issuedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedate"].ToString());
                        olrissue.receivinglocationid = reader["receivinglocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivinglocationid"].ToString());
                        olrissue.fromlocationid = reader["fromlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromlocationid"].ToString());
                        olrissue.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"].ToString());
                        olrissue.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrissue.noofbundels = reader["noofbundels"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["noofbundels"].ToString());
                        olrissue.noofpieces = reader["noofpieces"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["noofpieces"].ToString());
                        olrissue.frieghtcharges = reader["frieghtcharges"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["frieghtcharges"].ToString());
                        if (_lrissue.supplierid > 0)
                            olrissue.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                        lst.Add(olrissue);
                    }
                }
                conn.Close();

                if (lst.Count == 1)
                {
                    LrissuelistDataService lrlistdata = new LrissuelistDataService() { Context = this.Context };
                    lst[0].lrissuelist = lrlistdata.Search(new LrissuelistCreteria { lrissueid = lst[0].id });
                    if (_lrissue.computecharges)
                    {
                        TransportDataService transportDataService = new TransportDataService() { Context = this.Context };
                        var transports = transportDataService.Search(new TransportCreteria()
                        {
                            id = lst[0].bookingofficeid
                        });
                        if(transports.Count == 1)
                        {
                            var transport = transports.First();
                            if (transport.loadingcharges != null && transport.loadingcharges.Count == 1)
                            {
                                foreach (var item in lst[0].lrissuelist)
                                {
                                    item.computedloadingcharge = (item.noofboxes * transport.loadingchargeperbox)+
                                        (item.noofbundels * transport.loadingchargeperbundle);
                                    if(transport.loadingcharges.Exists(e=>e.cityid == item.fromlocationid))
                                    {
                                        item.computedfrightcharge = item.weight * transport.loadingcharges.Find(e => e.cityid == item.fromlocationid).charge;
                                    }
                                }
                            }
                        }
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

        internal LRIssuePrint Print(Lrissue _item)
        {
            var returnissue = Search(new LrissueCreteria()
            {
                id = _item.id
            });

            LRIssuePrint ret = new LRIssuePrint();
            ret.issueno = returnissue[0].issueno;
            ret.issuedate = returnissue[0].issuedate;
            ret.noofbundels = returnissue[0].noofbundels;
            ret.noofpieces = returnissue[0].noofpieces;
            ret.receivinglocationid = returnissue[0].receivinglocationid;
            ret.fromlocationid = returnissue[0].fromlocationid;
            ret.frieghtcharges = returnissue[0].frieghtcharges;
            ret.collectingby = returnissue[0].collectingby;
            ret.companyid = returnissue[0].companyid;
            ret.bookingofficeid = returnissue[0].bookingofficeid;
            ret.lrids = returnissue[0].lrids;
            ret.lrissuelist = returnissue[0].lrissuelist;



            Data.CompanyDataService companyservice = new CompanyDataService() { Context = this.Context };
            companyservice.Context = this.Context;
            var comnanylist = companyservice.Search(new CompanyCreteria()
            {
            });

            ret.company = comnanylist[0];
            ret.company.parseaddress();
            return ret;
        }


        public Lrissue Insert(Lrissue oLrissue)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    InsertDirect(transaction, conn, oLrissue);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }
            return oLrissue;
        }

        public Lrissue InsertDirect(NpgsqlTransaction transaction, NpgsqlConnection conn,Lrissue oLrissue)
        {
            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            var sequence = SequenceManager.Instance(Context.OrganizationID).Get("lrissue.issueno", 0);
            oLrissue.issueno = sequence.GetNextNumber(Context, conn, transaction);
            Insert(transaction, conn, oLrissue);
            LrissuelistDataService lrissuelist = new LrissuelistDataService() { Context = this.Context };
            lrissuelist.Context = this.Context;
            foreach (var items in oLrissue.lrissuelist)
            {
                long lrentryid = items.lrid;
                items.lrissueid = oLrissue.id;
                lrissuelist.Insert(transaction, conn, items);
                command = GetCommand(SQL_ChangeMode, conn, transaction);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = lrentryid;
                command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)(Models.Inbox.StatusList.LRIssue);
                command.ExecuteNonQuery();
            }
            if (oLrissue.version >= 0)
                InboxManager.Instance(Context.OrganizationID).Update(Context, oLrissue, transaction, conn);
            return oLrissue;
        }

        public Lrissue Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrissue oLrissue)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrissue.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissue.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("bookingofficeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissue.bookingofficeid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissue.companyid;
            command.Parameters.Add("collectingby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissue.collectingby;
            command.Parameters.Add("issueno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrissue.issueno == null ? "" : oLrissue.issueno;
            command.Parameters.Add("issuedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrissue.issuedate;
            command.Parameters.Add("receivinglocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissue.receivinglocationid;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissue.fromlocationid;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissue.noofpieces;
            command.Parameters.Add("noofbundels", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissue.noofbundels;
            command.Parameters.Add("frieghtcharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrissue.frieghtcharges;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrissue.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oLrissue;
        }

        public Lrissue Update(Lrissue oLrissue)
        {
            NpgsqlCommand command = null;
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oLrissue);

                    LrissuelistDataService lrissuelist = new LrissuelistDataService() { Context = this.Context };
                    lrissuelist.Context = this.Context;
                    lrissuelist.Update(transaction, conn, oLrissue.lrissuelist, oLrissue.id);

                    foreach (var items in oLrissue.lrissuelist)
                    {
                        long lrentryid = items.lrid;
                        items.lrissueid = oLrissue.id;

                        command = GetCommand(SQL_ChangeMode, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = lrentryid;
                        command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)(Models.Inbox.StatusList.LRIssue);
                        command.ExecuteNonQuery();

                    }

                    InboxManager.Instance(Context.OrganizationID).Update(Context, oLrissue, transaction, conn);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oLrissue;
        }

        public Lrissue Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrissue oLrissue)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissue.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrissue.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissue.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissue.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrissue.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("bookingofficeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissue.bookingofficeid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissue.companyid;
            command.Parameters.Add("collectingby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissue.collectingby;
            command.Parameters.Add("issueno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrissue.issueno == null ? "" : oLrissue.issueno;
            command.Parameters.Add("issuedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrissue.issuedate;
            command.Parameters.Add("receivinglocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissue.receivinglocationid;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissue.fromlocationid;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissue.noofpieces;
            command.Parameters.Add("noofbundels", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissue.noofbundels;
            command.Parameters.Add("frieghtcharges", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrissue.frieghtcharges;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrissue.version = Convert.ToInt32(reader[0]);
                }
            }

            return oLrissue;
        }

        public bool Delete(Lrissue oLrissue)
        {

            NpgsqlTransaction transaction = null;
            bool successCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    successCount = Delete(transaction, conn, oLrissue);
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
        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrissue oLrissue)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrissue.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrissue.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrissue.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
