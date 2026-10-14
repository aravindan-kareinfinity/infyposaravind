using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class BOMLrentryDataService : SAASData
    {
        const string SQL_Search = @"select id, BOMLrentryno, lrno, receiveddate, lrdate, locationid, receivinglocationid, retaillocationid, fromlocationid, supplierid, hasfreightcharge, noofbundels, noofpieces, weight, issuedlrno, issuedlrid, totalamount, premargin,
        attachment,  modifiedon, createdon, createdby, modifiedby, version, isactive, status, transportid,modeofdelivery,duedate,invoiceno,invoicedate,slipno,slipdate,BOMLrentrydate,sourcesupplierid,paymentmode,noofboxes,agentid,entrylocationid,lrcompanyid,purchasemanagerid,stockholdingperiod from BOMLrentry where id=@id";

        const string SQL_SearchLast = @"select BOMLrentryno, lrdate, employee.name,BOMLrentry.invoiceno,BOMLrentry.invoicedate,grossamount from BOMLrentry 
                                        left outer join employee on employee.id = BOMLrentry.createdby
                                        left outer join lrinvoice on lrinvoice.lrid = BOMLrentry.id
                                        where lrno=@lrno and supplierid=@supplierid and BOMLrentry.invoiceno=@invoiceno 
                                        limit 1";

        const string SQL_Insert = @"INSERT INTO public.BOMLrentry(BOMLrentryno , lrno, receiveddate, lrdate, receivinglocationid, fromlocationid, supplierid, hasfreightcharge, noofbundels, noofpieces, weight, issuedlrno, 
        issuedlrid, totalamount, attachment, modifiedon, createdon, createdby, modifiedby, version, isactive, status, transportid,modeofdelivery,duedate,invoiceno,invoicedate,slipno,slipdate,BOMLrentrydate,locationid,retaillocationid,sourcesupplierid,paymentmode,noofboxes,agentid,entrylocationid,lrcompanyid,purchasemanagerid,stockholdingperiod,premargin) 
        VALUES(@BOMLrentryno, @lrno, @receiveddate, @lrdate, @receivinglocationid, @fromlocationid, @supplierid, @hasfreightcharge, @noofbundels, @noofpieces, @weight, @issuedlrno, @issuedlrid, @totalamount, 
        @attachment, @modifiedon, @createdon, @createdby, @modifiedby, @version, @isactive, @status, @transportid,@modeofdelivery,@duedate,@invoiceno,@invoicedate,@slipno,@slipdate,@BOMLrentrydate,@locationid,@retaillocationid,@sourcesupplierid,@paymentmode,@noofboxes,@agentid,@entrylocationid,@lrcompanyid,@purchasemanagerid,@stockholdingperiod,@premargin) RETURNING id";

        public LrInformation SearchLast(BOMLrentryCreteria _BOMLrentry)
        {
            if (_BOMLrentry.lrno == null)
                return new LrInformation();
            NpgsqlConnection conn = CreateConnection();
            LrInformation ret = new LrInformation();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();

                command = GetCommand(SQL_SearchLast, conn);
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _BOMLrentry.supplierid;
                command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _BOMLrentry.lrno;
                command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _BOMLrentry.invoiceno;
                command.Parameters.Add("invoicedate",NpgsqlTypes.NpgsqlDbType.Date).Value =_BOMLrentry.invoicedate;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        ret.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        ret.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        //ret.Lrentryno = reader["Lrentryno"] == DBNull.Value ? "" : reader["BOMLrentryno"].ToString();
                        ret.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"].ToString());
                        ret.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"].ToString());
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _BOMLrentry);
            }
            return ret;
        }

        const string SQL_Update = @"UPDATE public.BOMLrentry SET BOMLrentryno=@BOMLrentryno,duedate=@duedate, lrno=@lrno, receiveddate=@receiveddate, lrdate=@lrdate, receivinglocationid=@receivinglocationid, sourcesupplierid=@sourcesupplierid,
    fromlocationid=@fromlocationid, supplierid=@supplierid, hasfreightcharge=@hasfreightcharge, noofbundels=@noofbundels, noofpieces=@noofpieces, weight=@weight, issuedlrno=@issuedlrno, issuedlrid=@issuedlrid,paymentmode=@paymentmode,noofboxes=@noofboxes,
    totalamount=@totalamount, attachment=@attachment,premargin=@premargin, locationid = @locationid, retaillocationid=@retaillocationid, modifiedon=@modifiedon, modifiedby=@modifiedby, version=@version+1, status=@status, transportid=@transportid,modeofdelivery=@modeofdelivery,
    invoiceno=@invoiceno,invoicedate=@invoicedate,slipno=@slipno,slipdate=@slipdate,BOMLrentrydate=@BOMLrentrydate,agentid=@agentid,entrylocationid=@entrylocationid,lrcompanyid=@lrcompanyid,purchasemanagerid=@purchasemanagerid,stockholdingperiod=@stockholdingperiod where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.BOMLrentry set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        public List<BOMLrentry> Search(BOMLrentryCreteria _BOMLrentry)
        {
            List<BOMLrentry> lst = new List<BOMLrentry>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_BOMLrentry.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _BOMLrentry.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    qb.Limit = 100;
                    qb.SortField = "id desc";
                    if (!string.IsNullOrEmpty(_BOMLrentry.lrno))
                    {
                        qb.AddParameter("lrno", _BOMLrentry.lrno, "=");
                    }
                    if (!string.IsNullOrEmpty(_BOMLrentry.BOMLrentryno))
                    {
                        qb.AddParameter("BOMLrentryno", _BOMLrentry.BOMLrentryno, "=");
                    }
                    if (_BOMLrentry.supplierid > 0)
                    {
                        qb.AddParameter("supplierid", _BOMLrentry.supplierid, "=");
                    }
                    if (_BOMLrentry.modeofdelivery > 0)
                        qb.AddParameter("modeofdelivery", _BOMLrentry.modeofdelivery, "=");

                    if (_BOMLrentry.fromdate.Year > 1900)
                        qb.AddParameter("lrdate", "fromdate", _BOMLrentry.fromdate.Date, ">=");
                    if (_BOMLrentry.todate.Year > 1900)
                        qb.AddParameter("lrdate", "todate", _BOMLrentry.todate.Date.AddDays(1), "<");

                    if (!string.IsNullOrEmpty(_BOMLrentry.lrids))
                    {
                        qb.AddInParamer("id", _BOMLrentry.lrids);
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        BOMLrentry oBOMLrentry = new BOMLrentry();
                        oBOMLrentry.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oBOMLrentry.lrcompanyid = reader["lrcompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrcompanyid"]);
                        oBOMLrentry.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        oBOMLrentry.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        oBOMLrentry.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        oBOMLrentry.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"].ToString());
                        oBOMLrentry.BOMLrentrydate = reader["BOMLrentrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["BOMLrentrydate"].ToString());
                        oBOMLrentry.slipno = reader["slipno"] == DBNull.Value ? "" : reader["slipno"].ToString();
                        oBOMLrentry.slipdate = reader["slipdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["slipdate"].ToString());
                        oBOMLrentry.BOMLrentryno = reader["BOMLrentryno"] == DBNull.Value ? "" : reader["BOMLrentryno"].ToString();
                        oBOMLrentry.receiveddate = reader["receiveddate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receiveddate"].ToString());
                        oBOMLrentry.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"].ToString());
                        oBOMLrentry.duedate = reader["duedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["duedate"].ToString());
                        oBOMLrentry.receivinglocationid = reader["receivinglocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivinglocationid"].ToString());
                        oBOMLrentry.fromlocationid = reader["fromlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromlocationid"].ToString());
                        oBOMLrentry.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"].ToString());
                        oBOMLrentry.retaillocationid = reader["retaillocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["retaillocationid"].ToString());
                        oBOMLrentry.entrylocationid = reader["entrylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["entrylocationid"].ToString());
                        oBOMLrentry.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                        oBOMLrentry.sourcesupplierid = reader["sourcesupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourcesupplierid"].ToString());
                        if (oBOMLrentry.sourcesupplierid == oBOMLrentry.supplierid)
                            oBOMLrentry.sourcesupplierid = 0;
                        oBOMLrentry.hasfreightcharge = reader["hasfreightcharge"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasfreightcharge"].ToString());
                        oBOMLrentry.issuedlrno = reader["issuedlrno"] == DBNull.Value ? "" : reader["issuedlrno"].ToString();
                        oBOMLrentry.issuedlrid = reader["issuedlrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issuedlrid"].ToString());
                        oBOMLrentry.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"].ToString());
                        oBOMLrentry.premargin = reader["premargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["premargin"].ToString());
                        oBOMLrentry.attachment_json = reader["attachment"] == DBNull.Value ? "null" : reader["attachment"].ToString();
                        oBOMLrentry.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oBOMLrentry.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oBOMLrentry.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oBOMLrentry.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oBOMLrentry.noofbundels = reader["noofbundels"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofbundels"].ToString());
                        oBOMLrentry.noofpieces = reader["noofpieces"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofpieces"].ToString());
                        oBOMLrentry.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        oBOMLrentry.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oBOMLrentry.weight = reader["weight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["weight"]);
                        oBOMLrentry.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"].ToString());
                        oBOMLrentry.modeofdelivery = reader["modeofdelivery"] == DBNull.Value ? 0 : Convert.ToInt32(reader["modeofdelivery"].ToString());
                        oBOMLrentry.paymentmode = reader["paymentmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["paymentmode"].ToString());
                        oBOMLrentry.noofboxes = reader["noofboxes"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofboxes"].ToString());
                        oBOMLrentry.purchasemanagerid = reader["purchasemanagerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["purchasemanagerid"]);
                        oBOMLrentry.stockholdingperiod = reader["stockholdingperiod"] == DBNull.Value ? 0 : Convert.ToInt32(reader["stockholdingperiod"].ToString());
                        lst.Add(oBOMLrentry);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _BOMLrentry);
            }

            return lst;
        }

        private bool DuplicateValidation(BOMLrentry oBOMLrentry)
        {
            if (string.IsNullOrEmpty(oBOMLrentry.lrno)) return false;

            string _Query = "select count(id) items from BOMLrentry where isactive=true and supplierid=@supplierid and lrno=@lrno and invoiceno=@invoiceno and date(invoicedate)=@invoicedate";
            int count = 0;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(_Query, conn);
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.supplierid;
                command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMLrentry.invoiceno;
                command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMLrentry.lrno;
                command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMLrentry.invoicedate.Date;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        count = reader["items"] == DBNull.Value ? 0 : Convert.ToInt32(reader["items"]);
                    }
                }
                conn.Close();
            }
            return count > 0;
        }

        public BOMLrentry Insert(BOMLrentry oBOMLrentry)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("Lrentry.Lrentryno",0, InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno?
                    oBOMLrentry.lrcompanyid:0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    
                    oBOMLrentry.BOMLrentryno = sequence.GetNextNumber(Context,conn, transaction);

                    Insert(transaction, conn, oBOMLrentry);

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMLrentry);


            }

            return oBOMLrentry;
        }

        public BOMLrentry Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMLrentry oBOMLrentry)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMLrentry.lrno == null ? "" : oBOMLrentry.lrno;
            command.Parameters.Add("BOMLrentryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMLrentry.BOMLrentryno == null ? "" : oBOMLrentry.BOMLrentryno;
            command.Parameters.Add("receiveddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMLrentry.receiveddate;
            command.Parameters.Add("BOMLrentrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMLrentry.BOMLrentrydate;
            command.Parameters.Add("lrdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMLrentry.lrdate;
            command.Parameters.Add("duedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMLrentry.duedate;
            command.Parameters.Add("receivinglocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.receivinglocationid;
            command.Parameters.Add("retaillocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.retaillocationid;
            command.Parameters.Add("entrylocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.entrylocationid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.locationid;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.agentid;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.fromlocationid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.supplierid;
            command.Parameters.Add("sourcesupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.sourcesupplierid <= 0 ? oBOMLrentry.supplierid : oBOMLrentry.sourcesupplierid;
            command.Parameters.Add("hasfreightcharge", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMLrentry.hasfreightcharge;
            command.Parameters.Add("noofbundels", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMLrentry.noofbundels;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMLrentry.noofpieces;
            command.Parameters.Add("weight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMLrentry.weight;
            command.Parameters.Add("issuedlrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMLrentry.issuedlrno == null ? "" : oBOMLrentry.issuedlrno;
            command.Parameters.Add("attachment", NpgsqlTypes.NpgsqlDbType.Json).Value = oBOMLrentry.attachment == null ? "null" : oBOMLrentry.attachment_json;
            command.Parameters.Add("issuedlrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.issuedlrid;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMLrentry.totalamount;
            command.Parameters.Add("premargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMLrentry.premargin;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMLrentry.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMLrentry.status;
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.transportid;
            command.Parameters.Add("modeofdelivery", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMLrentry.modeofdelivery;
            command.Parameters.Add("paymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMLrentry.paymentmode;
            command.Parameters.Add("noofboxes", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMLrentry.noofboxes;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMLrentry.invoiceno == null ? "" : oBOMLrentry.invoiceno;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMLrentry.invoicedate;
            command.Parameters.Add("slipno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMLrentry.slipno == null ? "" : oBOMLrentry.slipno;
            command.Parameters.Add("slipdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMLrentry.slipdate;
            command.Parameters.Add("lrcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.lrcompanyid;
            command.Parameters.Add("purchasemanagerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.purchasemanagerid;
            command.Parameters.Add("stockholdingperiod", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMLrentry.stockholdingperiod;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBOMLrentry.id = Convert.ToInt64(reader[0].ToString());
                }
            }


            return oBOMLrentry;
        }

        public BOMLrentry Update(BOMLrentry oBOMLrentry)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oBOMLrentry);
                    //BOMLrentryfreightchargesDataService subBOMLrentryfreightcharges = new BOMLrentryfreightchargesDataService() { Context = this.Context };
                    //subBOMLrentryfreightcharges.Context = this.Context;
                    //subBOMLrentryfreightcharges.Update(transaction, conn, oBOMLrentry.BOMLrentryfreightcharges, oBOMLrentry.id);

                    //InboxManager.Instance(Context.OrganizationID).Update(false, oBOMLrentry, transaction, conn);

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMLrentry);


            }

            return oBOMLrentry;
        }

        public BOMLrentry Update(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMLrentry oBOMLrentry)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.id;
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMLrentry.lrno == null ? "" : oBOMLrentry.lrno;
            command.Parameters.Add("BOMLrentryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMLrentry.BOMLrentryno == null ? "" : oBOMLrentry.BOMLrentryno;
            command.Parameters.Add("receiveddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMLrentry.receiveddate;
            command.Parameters.Add("BOMLrentrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMLrentry.BOMLrentrydate;
            command.Parameters.Add("lrdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMLrentry.lrdate;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.locationid;
            command.Parameters.Add("retaillocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.retaillocationid;
            command.Parameters.Add("entrylocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.entrylocationid;
            command.Parameters.Add("receivinglocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.receivinglocationid;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.fromlocationid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.supplierid;
            command.Parameters.Add("sourcesupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.sourcesupplierid >0 ? oBOMLrentry.sourcesupplierid : oBOMLrentry.sourcesupplierid;
            command.Parameters.Add("hasfreightcharge", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMLrentry.hasfreightcharge;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.agentid;
            command.Parameters.Add("paymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMLrentry.paymentmode;
            command.Parameters.Add("noofboxes", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMLrentry.noofboxes;
            command.Parameters.Add("noofbundels", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMLrentry.noofbundels;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMLrentry.noofpieces;
            command.Parameters.Add("weight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMLrentry.weight;
            command.Parameters.Add("issuedlrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMLrentry.issuedlrno == null ? "" : oBOMLrentry.issuedlrno;
            command.Parameters.Add("issuedlrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.issuedlrid;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMLrentry.totalamount;
            command.Parameters.Add("premargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oBOMLrentry.premargin;
            command.Parameters.Add("attachment", NpgsqlTypes.NpgsqlDbType.Json).Value = oBOMLrentry.attachment_json == null ? "null" : oBOMLrentry.attachment_json;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMLrentry.createdon;
            command.Parameters.Add("duedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMLrentry.duedate;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMLrentry.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMLrentry.status;
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.transportid;
            command.Parameters.Add("modeofdelivery", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMLrentry.modeofdelivery;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMLrentry.invoiceno == null ? "" : oBOMLrentry.invoiceno;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMLrentry.invoicedate;
            command.Parameters.Add("slipno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oBOMLrentry.slipno == null ? "" : oBOMLrentry.slipno;
            command.Parameters.Add("slipdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oBOMLrentry.slipdate;
            command.Parameters.Add("lrcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.lrcompanyid;
            command.Parameters.Add("purchasemanagerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.purchasemanagerid;
            command.Parameters.Add("stockholdingperiod", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMLrentry.stockholdingperiod;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oBOMLrentry.version = Convert.ToInt32(reader[0]);
                }
            }

            return oBOMLrentry;
        }


        public bool Delete(BOMLrentry oBOMLrentry)
        {

            NpgsqlTransaction transaction = null;
            bool successCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    successCount = Delete(transaction, conn, oBOMLrentry);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oBOMLrentry);
            }
            return successCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, BOMLrentry oBOMLrentry)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMLrentry.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMLrentry.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

    }

}
