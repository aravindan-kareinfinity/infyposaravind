using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class LrentryDataService : SAASData
    {
        const string SQL_Search = @"select lrentry.id, lrentry.lrentryno, lrentry.lrno, lrentry.receiveddate, lrentry.lrdate, lrentry.locationid, lrentry.receivinglocationid, lrentry.retaillocationid, lrentry.fromlocationid, lrentry.supplierid, lrentry.hasfreightcharge, lrentry.noofbundels, lrentry.noofpieces, lrentry.weight, lrentry.issuedlrno, lrentry.issuedlrid, lrentry.totalamount, lrentry.premargin,
        lrentry.attachment,  lrentry.modifiedon, lrentry.createdon, lrentry.createdby, lrentry.modifiedby, lrentry.version, lrentry.isactive, lrentry.status, lrentry.transportid, lrentry.modeofdelivery, lrentry.duedate, lrentry.invoiceno, lrentry.invoicedate, lrentry.slipno, lrentry.slipdate, lrentry.lrentrydate, lrentry.sourcesupplierid, lrentry.paymentmode, lrentry.noofboxes, lrentry.agentid, 
        lrentry.entrylocationid, lrentry.lrcompanyid, lrentry.purchasemanagerid, lrentry.stockholdingperiod,lrsectionid, lrentry.agentcommission, lrentry.chargeweight, inbox.statusname,lrentry.lrexpense,lrentry.hasloadingcharge ,lrentry.loadingcharge ,lrentry.freightcharge, lrentry.remarks, lrentry.bundlerackid,
        createdemployee.name createduser, modifiedemployee.name modifieduser,importreferenceid,importreferencetype
        from lrentry left outer join inbox on inbox.referenceid = lrentry.id and inbox.typeid = 1
        left outer join users createduser on createduser.id = lrentry.createdby
        left outer join employee createdemployee on createdemployee.id = createduser.employeeid
        left outer join users modifieduser on modifieduser.id = lrentry.modifiedby
        left outer join employee modifiedemployee on modifiedemployee.id = modifieduser.employeeid
        where lrentry.id=@id";

        const string SQL_SearchLast = @"select lrentry.id, lrentryno, lrdate,lrno, employee.name,lrentry.invoiceno,lrentry.invoicedate,grossamount from lrentry 
                                        left outer join users on users.id = lrentry.createdby
                                        left outer join employee on employee.id = users.employeeid
                                        left outer join lrinvoice on lrinvoice.lrid = lrentry.id
                                        where lrentry.isactive=true and supplierid=@supplierid and lrentry.invoiceno=@invoiceno and lrentry.id != @id and modeofdelivery != 5
                                        AND lrentry.invoicedate >=
                                              CASE
                                                  WHEN EXTRACT(MONTH FROM @invoicedate::DATE) >= 4
                                                       THEN DATE_TRUNC('year', @invoicedate::DATE) + INTERVAL '3 months'
                                                  ELSE DATE_TRUNC('year', @invoicedate::DATE) - INTERVAL '9 months'
                                              END
                                        AND lrentry.invoicedate <
                                              CASE
                                                  WHEN EXTRACT(MONTH FROM @invoicedate::DATE) >= 4
                                                       THEN DATE_TRUNC('year', @invoicedate::DATE) + INTERVAL '15 months'
                                                  ELSE DATE_TRUNC('year', @invoicedate::DATE) + INTERVAL '3 months'
                                              END

                                        limit 1";

        const string SQL_SearchEntry = @"select lrentry.id, lrentryno, lrdate, lrno, employee.name,lrentry.invoiceno,lrentry.invoicedate,grossamount from lrentry 
                                        left outer join users on users.id = lrentry.createdby
                                        left outer join employee on employee.id = users.employeeid
                                        left outer join lrinvoice on lrinvoice.lrid = lrentry.id
                                        where lrentryno=@lrentryno
                                        limit 1";

        const string SQL_Insert = @"INSERT INTO public.lrentry(lrentryno , lrno, receiveddate, lrdate, receivinglocationid, fromlocationid, supplierid, hasfreightcharge, noofbundels, noofpieces, weight, issuedlrno, 
        issuedlrid, totalamount, attachment, modifiedon, createdon, createdby, modifiedby, version, isactive, status, transportid,modeofdelivery,duedate,invoiceno,invoicedate,slipno,slipdate,lrentrydate,locationid,
        retaillocationid,sourcesupplierid,paymentmode,noofboxes,agentid,entrylocationid,lrcompanyid,purchasemanagerid,stockholdingperiod,lrsectionid,premargin,agentcommission,chargeweight,hasloadingcharge ,loadingcharge ,
        freightcharge, remarks, bundlerackid,importreferenceid,importreferencetype) 
        VALUES(@lrentryno, @lrno, @receiveddate, @lrdate, @receivinglocationid, @fromlocationid, @supplierid, @hasfreightcharge, @noofbundels, @noofpieces, @weight, @issuedlrno, @issuedlrid, @totalamount, 
        @attachment, @modifiedon, @createdon, @createdby, @modifiedby, @version, @isactive, @status, @transportid,@modeofdelivery,@duedate,@invoiceno,@invoicedate,@slipno,@slipdate,@lrentrydate,@locationid,
        @retaillocationid,@sourcesupplierid,@paymentmode,@noofboxes,@agentid,@entrylocationid,@lrcompanyid,@purchasemanagerid,@stockholdingperiod,@lrsectionid,@premargin,@agentcommission,@chargeweight,
        @hasloadingcharge ,@loadingcharge ,@freightcharge, @remarks, @bundlerackid,@importreferenceid,@importreferencetype) RETURNING id";

        internal LrInformation SearchLast(LrentryCreteria _lrentry)
        {
            if (string.IsNullOrEmpty(_lrentry.invoiceno) && string.IsNullOrEmpty(_lrentry.lrentryno))
                return new LrInformation();
            NpgsqlConnection conn = CreateConnection();
            LrInformation ret = new LrInformation();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();

                if (string.IsNullOrEmpty(_lrentry.lrentryno))
                {
                    command = GetCommand(SQL_SearchLast, conn);
                    command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrentry.supplierid;
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrentry.id;
                    command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _lrentry.lrno;
                    command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(_lrentry.invoiceno) ? "" : _lrentry.invoiceno;
                    command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Date).Value = _lrentry.invoicedate;
                }
                else
                {
                    command = GetCommand(SQL_SearchEntry, conn);
                    command.Parameters.Add("lrentryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = _lrentry.lrentryno;
                }
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        ret.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"].ToString());
                        ret.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        ret.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        ret.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        ret.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        ret.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"].ToString());
                        ret.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"].ToString());
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _lrentry);
            }
            if (_lrentry.id > 0 && ret.id == _lrentry.id)
                return new LrInformation();
            return ret;
        }

        internal string LastEntry(string companyid, string locationid)
        {
            string entryno = "";
            using (NpgsqlConnection conn = CreateConnection())
            {
                LrInformation ret = new LrInformation();

                conn.Open();

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = "select lrentryno from lrentry ";
                qb.AddParameter("lrentry.isactive", true, "=");
                long cid = 0;
                long lid = 0;

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_showroomentryno &&
                    long.TryParse(companyid, out cid) && cid > 0)
                {
                    qb.AddParameter("lrentry.lrcompanyid", cid, "=");
                }
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno && long.TryParse(locationid, out lid) && cid > 0)
                {
                    qb.AddParameter("lrentry.retaillocationid", lid, "=");
                }
                qb.Limit = 1;
                using (var reader = qb.GetCommand(conn).ExecuteReader())
                {
                    if (reader.Read())
                    {
                        entryno = reader.GetString(0);
                    }
                }
                conn.Close();
            }
            return entryno;
        }

        internal Lrentry UpdateLRExpense(Lrentry item)
        {
            var param = new SortedList<string, object>();
            param.Add("lrexpense", item.lrexpense);
            param.Add("id", item.id);
            string query = "update lrentry set lrexpense=@lrexpense where id=@id";
            if (ExecuteQuery(query, param) > 0)
            {
                return item;
            }
            return null;
        }

        const string SQL_Update = @"UPDATE public.lrentry SET lrentryno=@lrentryno,duedate=@duedate, lrno=@lrno, receiveddate=@receiveddate, lrdate=@lrdate, receivinglocationid=@receivinglocationid, sourcesupplierid=@sourcesupplierid,
    fromlocationid=@fromlocationid, supplierid=@supplierid, hasfreightcharge=@hasfreightcharge, noofbundels=@noofbundels, noofpieces=@noofpieces, weight=@weight, issuedlrno=@issuedlrno, issuedlrid=@issuedlrid,paymentmode=@paymentmode,noofboxes=@noofboxes,
    totalamount=@totalamount, attachment=@attachment,premargin=@premargin, locationid = @locationid, retaillocationid=@retaillocationid, modifiedon=@modifiedon, modifiedby=@modifiedby, version=@version+1, status=@status, transportid=@transportid,modeofdelivery=@modeofdelivery,
    invoiceno=@invoiceno,invoicedate=@invoicedate,slipno=@slipno,slipdate=@slipdate,lrentrydate=@lrentrydate,agentid=@agentid,entrylocationid=@entrylocationid,lrcompanyid=@lrcompanyid,purchasemanagerid=@purchasemanagerid,lrsectionid=@lrsectionid,stockholdingperiod=@stockholdingperiod,
    hasloadingcharge=@hasloadingcharge,loadingcharge=@loadingcharge,freightcharge=@freightcharge,agentcommission=@agentcommission,chargeweight=@chargeweight,remarks=@remarks, bundlerackid=@bundlerackid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.lrentry set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        public bool IsMigration { get; set; }

        public List<Lrentry> Search(LrentryCreteria _lrentry)
        {

            List<Lrentry> lst = new List<Lrentry>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_lrentry.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrentry.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("lrentry.isactive", true, "=");
                    //qb.Limit = 100;
                    qb.SortField = "lrentry.id desc";
                    if (!string.IsNullOrEmpty(_lrentry.lrno))
                    {
                        qb.AddParameter("lrentry.lrno", _lrentry.lrno, "=");
                    }
                    if (!string.IsNullOrEmpty(_lrentry.lrentryno))
                    {
                        qb.AddParameter("lrentry.lrentryno", _lrentry.lrentryno, "=");
                    }
                    if (_lrentry.transportid > 0)
                    {
                        qb.AddParameter("lrentry.transportid", _lrentry.transportid, "=");
                    }
                    if (_lrentry.fromlocationid > 0)
                    {
                        qb.AddParameter("lrentry.fromlocationid", _lrentry.fromlocationid, "=");
                    }
                    if (_lrentry.supplierid > 0)
                    {
                        qb.AddParameter("lrentry.supplierid", _lrentry.supplierid, "=");
                    }
                    if (_lrentry.modeofdelivery > 0)
                        qb.AddParameter("lrentry.modeofdelivery", _lrentry.modeofdelivery, "=");

                    if (_lrentry.fromdate.Year > 1900)
                        qb.AddParameter("lrentry.lrdate", "fromdate", _lrentry.fromdate.Date, ">=");
                    if (_lrentry.todate.Year > 1900)
                        qb.AddParameter("lrentry.lrdate", "todate", _lrentry.todate.Date.AddDays(1), "<");

                    if (_lrentry.entryfromdate.Year > 1900)
                        qb.AddParameter("lrentry.lrentrydate", "entryfromdate", _lrentry.entryfromdate.Date, ">=");
                    if (_lrentry.entrytodate.Year > 1900)
                        qb.AddParameter("lrentry.lrentrydate", "entrytodate", _lrentry.entrytodate.Date.AddDays(1), "<");

                    if (_lrentry.importreferenceid > 0)
                    {
                        qb.AddParameter("lrentry.importreferenceid", _lrentry.importreferenceid, "=");
                        qb.AddParameter("lrentry.importreferencetype", (int)_lrentry.importreferencetype, "=");
                    }
                    if (!string.IsNullOrEmpty(_lrentry.lrids))
                    {
                        qb.AddInParamer("lrentry.id", _lrentry.lrids);
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Lrentry olrentry = new Lrentry();
                        olrentry.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        olrentry.lrcompanyid = reader["lrcompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrcompanyid"]);
                        olrentry.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                        olrentry.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        olrentry.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        olrentry.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"].ToString());
                        olrentry.lrentrydate = reader["lrentrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrentrydate"].ToString());
                        olrentry.slipno = reader["slipno"] == DBNull.Value ? "" : reader["slipno"].ToString();
                        olrentry.slipdate = reader["slipdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["slipdate"].ToString());
                        olrentry.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        olrentry.receiveddate = reader["receiveddate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receiveddate"].ToString());
                        olrentry.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"].ToString());
                        olrentry.duedate = reader["duedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["duedate"].ToString());
                        olrentry.receivinglocationid = reader["receivinglocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["receivinglocationid"].ToString());
                        olrentry.fromlocationid = reader["fromlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromlocationid"].ToString());
                        olrentry.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"].ToString());
                        olrentry.retaillocationid = reader["retaillocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["retaillocationid"].ToString());
                        olrentry.entrylocationid = reader["entrylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["entrylocationid"].ToString());
                        olrentry.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"].ToString());
                        olrentry.sourcesupplierid = reader["sourcesupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourcesupplierid"].ToString());
                        if (olrentry.sourcesupplierid == olrentry.supplierid)
                            olrentry.sourcesupplierid = 0;
                        olrentry.hasfreightcharge = reader["hasfreightcharge"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasfreightcharge"].ToString());
                        olrentry.issuedlrno = reader["issuedlrno"] == DBNull.Value ? "" : reader["issuedlrno"].ToString();
                        olrentry.issuedlrid = reader["issuedlrid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["issuedlrid"].ToString());
                        olrentry.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"].ToString());
                        olrentry.lrexpense = reader["lrexpense"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["lrexpense"].ToString());

                        olrentry.freightcharge = reader["freightcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["freightcharge"].ToString());
                        olrentry.loadingcharge = reader["loadingcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["loadingcharge"].ToString());
                        olrentry.hasloadingcharge = reader["hasloadingcharge"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasloadingcharge"]);

                        olrentry.premargin = reader["premargin"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["premargin"].ToString());
                        olrentry.attachment_json = reader["attachment"] == DBNull.Value ? "null" : reader["attachment"].ToString();
                        olrentry.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        olrentry.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        olrentry.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        olrentry.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        olrentry.noofbundels = reader["noofbundels"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofbundels"].ToString());
                        olrentry.noofpieces = reader["noofpieces"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofpieces"].ToString());
                        olrentry.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"].ToString());
                        olrentry.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        olrentry.weight = reader["weight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["weight"]);
                        olrentry.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"].ToString());
                        olrentry.modeofdelivery = reader["modeofdelivery"] == DBNull.Value ? 0 : Convert.ToInt32(reader["modeofdelivery"].ToString());
                        olrentry.paymentmode = reader["paymentmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["paymentmode"].ToString());
                        olrentry.noofboxes = reader["noofboxes"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofboxes"].ToString());
                        olrentry.purchasemanagerid = reader["purchasemanagerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["purchasemanagerid"]);
                        olrentry.stockholdingperiod = reader["stockholdingperiod"] == DBNull.Value ? 0 : Convert.ToInt32(reader["stockholdingperiod"].ToString());
                        olrentry.lrsectionid = reader["lrsectionid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["lrsectionid"].ToString());
                        olrentry.agentcommission = reader["agentcommission"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["agentcommission"]);
                        olrentry.chargeweight = reader["chargeweight"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["chargeweight"]);
                        olrentry.statusname = reader["statusname"] == DBNull.Value ? "" : reader["statusname"].ToString();
                        olrentry.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        olrentry.bundlerackid = reader["bundlerackid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bundlerackid"]);
                        olrentry.createduser = reader["createduser"] == DBNull.Value ? "" : reader["createduser"].ToString();
                        olrentry.modifieduser = reader["modifieduser"] == DBNull.Value ? "" : reader["modifieduser"].ToString();
                        olrentry.importreferenceid = reader["importreferenceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["importreferenceid"].ToString());
                        olrentry.importreferencetype = reader["importreferencetype"] == DBNull.Value ? Lrentry.ImportReferenceTypes.Direct : (Lrentry.ImportReferenceTypes)Convert.ToInt32(reader["importreferencetype"].ToString());
                        lst.Add(olrentry);
                    }
                }
                if (_lrentry.id > 0 && lst.Count == 1)
                {
                    LrentryfreightchargesDataService subLrentryfreightcharges = new LrentryfreightchargesDataService() { Context = this.Context };
                    subLrentryfreightcharges.Context = this.Context;
                    lst[0].Lrentryfreightcharges = subLrentryfreightcharges.SearchAll(_lrentry.id);
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _lrentry);
            }
            //LrentryfreightchargesDataService subDataservice = new LrentryfreightchargesDataService() { Context = this.Context };
            //subDataservice.Context = this.Context;
            //if (_lrentry.id > 0 && lst.Count == 1)
            //{

            //    lst[0].Lrentryfreightcharges = subDataservice.SearchAll(_lrentry.id);
            //}
            return lst;
        }

        internal List<LrEntry2Print> SearchForLable(LrentryCreteria _lrentry)
        {
            const string SQL_SearchForPrint = @"select lrentry.id, lrentry.lrentryno, lrentry.lrno, lrentry.lrdate, lrentry.supplierid, lrentry.noofbundels, lrentry.noofpieces, lrentry.totalamount, 
        lrentry.invoiceno, lrentry.invoicedate, lrentry.slipno, lrentry.slipdate, lrentry.lrentrydate, lrentry.noofboxes, lrentry.lrcompanyid, supplier.name suppliername,
        transport.name transportname,section.name section, fromcity.name fromcityname, createdemployee.name createduser, modifiedemployee.name modifieduser,
        lrentry.remarks,purchasemanagerid, purchaseemployee.name purchasemanagername
        from lrentry left outer join supplier on supplier.id = lrentry.supplierid 
        left outer join transport on transport.id = lrentry.transportid
        left outer join referencelist section on section.id = lrentry.lrsectionid
        left outer join referencelist fromcity on fromcity.id = lrentry.fromlocationid
        left outer join users createduser on createduser.id = lrentry.createdby
        left outer join employee createdemployee on createdemployee.id = createduser.employeeid
        left outer join users modifieduser on modifieduser.id = lrentry.modifiedby
        left outer join employee modifiedemployee on modifiedemployee.id = modifieduser.employeeid
		left outer join employee purchaseemployee on purchaseemployee.id = lrentry.purchasemanagerid
        where lrentry.id=@id";

            NpgsqlConnection conn = CreateConnection();
            List<LrEntry2Print> lst = new List<LrEntry2Print>();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();

                command = GetCommand(SQL_SearchForPrint, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _lrentry.id;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        LrEntry2Print olrentry = new LrEntry2Print();
                        olrentry.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        olrentry.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        olrentry.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                        olrentry.transportname = reader["transportname"] == DBNull.Value ? "" : reader["transportname"].ToString();
                        olrentry.sectionname = reader["section"] == DBNull.Value ? "" : reader["section"].ToString();
                        olrentry.createdusername = reader["createduser"] == DBNull.Value ? "" : reader["createduser"].ToString();
                        olrentry.modifiedusername = reader["modifieduser"] == DBNull.Value ? "" : reader["modifieduser"].ToString();
                        olrentry.fromcityname = reader["fromcityname"] == DBNull.Value ? "" : reader["fromcityname"].ToString();
                        olrentry.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                        olrentry.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"].ToString());
                        olrentry.lrentrydate = reader["lrentrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrentrydate"].ToString());
                        olrentry.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        olrentry.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"].ToString());
                        olrentry.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"].ToString());
                        olrentry.noofbundels = reader["noofbundels"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofbundels"].ToString());
                        olrentry.noofpieces = reader["noofpieces"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofpieces"].ToString());
                        olrentry.noofboxes = reader["noofboxes"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofboxes"].ToString());
                        olrentry.purchasemanager = reader["purchasemanagername"] == DBNull.Value ? "" : reader["purchasemanagername"].ToString();
                        olrentry.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        lst.Add(olrentry);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _lrentry);
            }
            return lst;
        }
        private bool DuplicateValidation(Lrentry oLrentry)
        {
            if (string.IsNullOrEmpty(oLrentry.lrno)) return false;

            string _Query = "select count(id) items from lrentry where isactive=true and supplierid=@supplierid and invoiceno=@invoiceno and lrno=@lrno and date(invoicedate)=@invoicedate";
            int count = 0;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(_Query, conn);
                command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.supplierid;
                command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oLrentry.invoiceno) ? "" : oLrentry.invoiceno;
                command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = string.IsNullOrEmpty(oLrentry.lrno) ? "" : oLrentry.lrno;
                command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrentry.invoicedate.Date;
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

        public Lrentry Insert(Lrentry oLrentry)
        {
            //if (DuplicateValidation(oLrentry)) throw new Exception("Duplicate LR Entry");

            if (oLrentry.supplierid == 0) throw new Exception("Please Select Supplier");

            NpgsqlTransaction transaction = null;
            try
            {
                if (DuplicateValidation(oLrentry))
                {
                    oLrentry.ErrorMessage = "Duplicate LR Entry - Same Supplier, LRNO, Invoice No and Invoice Date Exists";
                    return oLrentry;
                }

                LrInformation _entry = SearchLast(new LrentryCreteria()
                {
                    lrentryno = oLrentry.lrentryno,
                    lrno = oLrentry.lrno,
                    supplierid = oLrentry.supplierid,
                    invoiceno = oLrentry.invoiceno,
                    invoicedate = oLrentry.invoicedate
                });

                if (_entry != null && _entry.id > 0)
                {
                    oLrentry.ErrorMessage = "Duplicate LR Entry - Same Supplier, LRNO, Invoice No and Invoice Date Exists";
                    return oLrentry;
                }

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_showroomentryno &&
                    oLrentry.retaillocationid <= 0)
                {
                    oLrentry.ErrorMessage = "Please select the retail location, to generate entry no.";
                    return oLrentry;
                }


                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();


                    Insert(transaction, conn, oLrentry);

                    if (oLrentry.modeofdelivery == 3 && !string.IsNullOrEmpty(oLrentry.invoiceno))
                    {
                        string updateQuery = "Update bulkbill set lrid=@id where billno=@billno";
                        NpgsqlCommand command = GetCommand(updateQuery, conn, transaction);
                        command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.id;
                        command.Parameters.Add("billno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrentry.invoiceno;
                        int noofrecords = command.ExecuteNonQuery();
                        if (noofrecords != 1)
                            throw new Exception("Tailorying Bulk bill no " + oLrentry.invoiceno + " not available");
                    }

                    UpdateLRFreight(false, transaction, conn, oLrentry);

                    LrentryfreightchargesDataService subLrentryfreightcharges = new LrentryfreightchargesDataService() { Context = this.Context };
                    subLrentryfreightcharges.Context = this.Context;
                    if (oLrentry.Lrentryfreightcharges != null)
                    {
                        foreach (var item in oLrentry.Lrentryfreightcharges)
                        {
                            item.lrid = oLrentry.id;
                            subLrentryfreightcharges.Insert(transaction, conn, item);
                        }
                    }

                    if (!IsMigration)
                    {
                        if (oLrentry.importreferenceid > 0 &&
                            oLrentry.importreferencetype == Lrentry.ImportReferenceTypes.UANPO)
                        {
                            PurchaseorderDataService purchaseorderData = new PurchaseorderDataService() { Context = this.Context };
                            var po = purchaseorderData.Search(new PurchaseorderCreteria() { id = oLrentry.importreferenceid });
                            if (po.Count > 0 &&
                                po[0].internalstatus < Purchaseorder.InternalStatus.GRN)
                            {
                                po[0].internalstatus = Purchaseorder.InternalStatus.GRN;
                                po[0].status = Purchaseorder.POStatus.PARTIALLYCOMPLETED;
                                purchaseorderData.UpdateStatus(transaction, conn, po[0]);
                            }
                        }
                        InboxManager.Instance(Context.OrganizationID).Update(Context, true, oLrentry, transaction, conn);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrentry);
            }
            return oLrentry;
        }
        public void UpdateLRFreight(bool isUpdate, NpgsqlTransaction transaction, NpgsqlConnection conn, Lrentry oLrentry)
        {
            UpdateLRFreight(isUpdate, transaction, conn, oLrentry, false);
        }
        public void UpdateLRFreight(bool isUpdate, NpgsqlTransaction transaction, NpgsqlConnection conn, Lrentry oLrentry, bool isDelete)
        {
            bool insertfrieghtrecords = true;
            if (isUpdate)
            {
                //if (oLrentry.modeofdelivery != 1 || oLrentry.version > 1)
                {
                    bool allminus1version = false;
                    bool hasrecords = false;
                    int modeofdelivery = 0;
                    long Lrreceivedlistid = 0;
                    long Lrreceivedid = 0;
                    long lrissueid = 0;
                    long Lrissuelistid = 0;
                    string query = @"
                    select modeofdelivery,Lrreceivedlist.id Lrreceivedlistid,
                    Lrreceived.id Lrreceivedid, lrissue.id lrissueid,Lrissuelist.id Lrissuelistid,
                    lrissue.version,Lrreceived.version,Lrreceivedlist.version,Lrissuelist.version from Lrreceivedlist
                    inner join lrentry on lrentry.id = Lrreceivedlist.lrid
                    inner join Lrreceived on Lrreceived.id = Lrreceivedlist.lrreceivedid
                    inner join lrissue on lrissue.id = lrreceived.issueid
                    inner join Lrissuelist on Lrissuelist.lrissueid = lrissue.id
                    where Lrreceivedlist.lrid = @lrid";
                    var param = new SortedList<string, object>();
                    param.Add("lrid", oLrentry.id);
                    using (var reader = ExecuteReader(query, param, conn, transaction))
                    {
                        if (reader.Read())
                        {
                            hasrecords = true;
                            if (reader.GetInt32(5) == reader.GetInt32(6) &&
                                 reader.GetInt32(6) == reader.GetInt32(7) &&
                                reader.GetInt32(7) == reader.GetInt32(8))
                            {
                                allminus1version = reader.GetInt32(5) == -1;
                            }
                            modeofdelivery = reader["modeofdelivery"] == DBNull.Value ? 0 : Convert.ToInt32(reader["modeofdelivery"]);
                            Lrreceivedlistid = reader["Lrreceivedlistid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Lrreceivedlistid"]);
                            Lrreceivedid = reader["Lrreceivedid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Lrreceivedid"]);
                            lrissueid = reader["lrissueid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrissueid"]);
                            Lrissuelistid = reader["Lrissuelistid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["Lrissuelistid"]);
                        }
                    }
                    if (hasrecords)
                    {
                        if (!allminus1version)
                        {
                            insertfrieghtrecords = false;
                        }
                        else
                        {
                            query = @"delete from Lrreceivedlist where id=@Lrreceivedlistid;
                            delete from Lrreceived where id=@Lrreceivedid;
                            delete from lrissue where id=@lrissueid;
                            delete from Lrissuelist where id=@Lrissuelistid;";
                            param.Add("Lrreceivedlistid", Lrreceivedlistid);
                            param.Add("Lrreceivedid", Lrreceivedid);
                            param.Add("lrissueid", lrissueid);
                            param.Add("Lrissuelistid", Lrissuelistid);
                            ExecuteQuery(query, param, conn, transaction);
                        }
                    }
                }
            }

            if (isDelete)
                return;

            if (insertfrieghtrecords)
            {
                if (oLrentry.hasfreightcharge || oLrentry.hasloadingcharge)
                {
                    var issued = new Lrissue()
                    {
                        bookingofficeid = oLrentry.transportid,
                        frieghtcharges = oLrentry.freightcharge,
                        companyid = oLrentry.lrcompanyid,
                        createdby = Context.UserID,
                        createdon = DateTimeNow,
                        fromlocationid = oLrentry.fromlocationid,
                        isactive = true,
                        issuedate = oLrentry.lrentrydate,
                        receivinglocationid = oLrentry.receivinglocationid,
                        supplierid = oLrentry.supplierid,
                        version = -1,
                        noofbundels = oLrentry.noofbundels,
                        noofpieces = oLrentry.noofpieces,
                        lrissuelist = new List<Lrissuelist>()
                    };

                    issued.lrissuelist.Add(new Lrissuelist()
                    {
                        createdby = Context.UserID,
                        createdon = DateTimeNow,
                        fromlocationid = oLrentry.fromlocationid,
                        isactive = true,
                        lrid = oLrentry.id,
                        lrno = oLrentry.lrno,
                        noofboxes = oLrentry.noofboxes,
                        noofbundels = oLrentry.noofbundels,
                        noofpieces = oLrentry.noofpieces,
                        supplierid = oLrentry.supplierid,
                        totalamount = (oLrentry.hasfreightcharge ? oLrentry.freightcharge : 0) + (oLrentry.hasloadingcharge ? oLrentry.loadingcharge : 0),
                        lrdate = oLrentry.lrdate,
                        lrentryno = oLrentry.lrentryno,
                        weight = oLrentry.weight,
                        version = -1
                    });

                    LrissueDataService lrissuelistDataService = new LrissueDataService() { Context = this.Context };
                    lrissuelistDataService.InsertDirect(transaction, conn, issued);

                    var received = new Lrreceived()
                    {
                        isactive = true,
                        version = -1,
                        createdon = DateTimeNow,
                        createdby = Context.UserID,
                        noofboxes = oLrentry.noofboxes,
                        noofbundels = oLrentry.noofbundels,
                        noofpieces = oLrentry.noofpieces,
                        actualweight = oLrentry.weight,
                        bookingofficeid = oLrentry.transportid,
                        chargedweight = oLrentry.chargeweight,
                        frieghtcharges = oLrentry.freightcharge,
                        companyid = oLrentry.lrcompanyid,
                        loadingcharges = oLrentry.loadingcharge,
                        receiptitem = new List<receiptitem>(),
                        totalcharges = (oLrentry.hasfreightcharge ? oLrentry.freightcharge : 0) + (oLrentry.hasloadingcharge ? oLrentry.loadingcharge : 0),
                        issueid = issued.id,
                        issuedate = issued.issuedate,
                        issueno = issued.issueno,
                        receivedon = oLrentry.receiveddate
                    };
                    received.receiptitem.Add(new receiptitem()
                    {
                        actualweight = oLrentry.weight,
                        chargedweight = oLrentry.chargeweight,
                        createdby = Context.UserID,
                        createdon = DateTimeNow,
                        frightcharge = oLrentry.freightcharge,
                        fromlocationid = oLrentry.fromlocationid,
                        isactive = true,
                        version = -1,
                        lrid = oLrentry.id,
                        lrno = oLrentry.lrno,
                        noofboxes = oLrentry.noofboxes,
                        noofbundels = oLrentry.noofbundels,
                        noofpieces = oLrentry.noofpieces,
                        noofboxesreceived = oLrentry.noofboxes,
                        loadingcharge = oLrentry.loadingcharge,
                        noofbundelsreceived = oLrentry.noofbundels,
                        receivedweight = oLrentry.weight,
                        supplierid = oLrentry.supplierid,
                        totalamount = (oLrentry.hasfreightcharge ? oLrentry.freightcharge : 0) + (oLrentry.hasloadingcharge ? oLrentry.loadingcharge : 0)
                    });
                    LrreceivedDataService lrreceiveddataservice = new LrreceivedDataService() { Context = this.Context };
                    lrreceiveddataservice.InsertDirect(transaction, conn, received);
                }
            }
        }
        public Lrentry Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrentry oLrentry)
        {
            if (string.IsNullOrEmpty(oLrentry.lrentryno))
            {

                SequenceNo sequence = null;
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_expenselrentry &&
                    oLrentry.modeofdelivery == 9)
                {
                    sequence = SequenceManager.Instance(Context.OrganizationID).Get("lrentry.lrexpenseno",
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_showroomentryno ? oLrentry.entrylocationid : 0,
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ?
                    oLrentry.lrcompanyid : 0);
                }
                else if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_expenselrentry &&
                    oLrentry.modeofdelivery == 8)
                {
                    sequence = SequenceManager.Instance(Context.OrganizationID).Get("lrentry.lrintertransfer",
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_showroomentryno ? oLrentry.entrylocationid : 0,
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ?
                    oLrentry.lrcompanyid : 0);
                }
                else
                {
                    sequence = SequenceManager.Instance(Context.OrganizationID).Get("lrentry.lrentryno",
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_showroomentryno ? oLrentry.entrylocationid : 0,
                    InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_supportcompanywiseentryno ?
                    oLrentry.lrcompanyid : 0);
                }

                if (string.IsNullOrEmpty(oLrentry.lrentryno))
                    oLrentry.lrentryno = sequence.GetNextNumber(Context, conn, transaction);
            }

            if (!string.IsNullOrEmpty(oLrentry.lrno) && oLrentry.lrno.Length > 50)
                oLrentry.lrno = oLrentry.lrno.Substring(0, 50);
            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrentry.lrno == null ? "" : oLrentry.lrno;
            command.Parameters.Add("lrentryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrentry.lrentryno == null ? "" : oLrentry.lrentryno;
            command.Parameters.Add("receiveddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrentry.receiveddate;
            command.Parameters.Add("lrentrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrentry.lrentrydate;
            command.Parameters.Add("lrdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrentry.lrdate;
            command.Parameters.Add("duedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrentry.duedate;
            command.Parameters.Add("receivinglocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.receivinglocationid;
            command.Parameters.Add("retaillocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.retaillocationid;
            command.Parameters.Add("entrylocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.entrylocationid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.locationid;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.agentid;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.fromlocationid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.supplierid;
            command.Parameters.Add("sourcesupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.sourcesupplierid <= 0 ? oLrentry.supplierid : oLrentry.sourcesupplierid;
            command.Parameters.Add("hasfreightcharge", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrentry.hasfreightcharge;
            command.Parameters.Add("noofbundels", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.noofbundels;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.noofpieces;
            command.Parameters.Add("weight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrentry.weight;
            command.Parameters.Add("issuedlrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrentry.issuedlrno == null ? "" : oLrentry.issuedlrno;
            command.Parameters.Add("attachment", NpgsqlTypes.NpgsqlDbType.Json).Value = oLrentry.attachment == null ? "null" : oLrentry.attachment_json;
            command.Parameters.Add("issuedlrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.issuedlrid;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrentry.totalamount;
            command.Parameters.Add("premargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrentry.premargin;

            command.Parameters.Add("freightcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrentry.freightcharge;
            command.Parameters.Add("loadingcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrentry.loadingcharge;
            command.Parameters.Add("hasloadingcharge", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrentry.hasloadingcharge;

            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.status;
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.transportid;
            command.Parameters.Add("modeofdelivery", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.modeofdelivery;
            command.Parameters.Add("paymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.paymentmode;
            command.Parameters.Add("noofboxes", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.noofboxes;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrentry.invoiceno == null ? "" : oLrentry.invoiceno;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrentry.invoicedate;
            command.Parameters.Add("slipno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrentry.slipno == null ? "" : oLrentry.slipno;
            command.Parameters.Add("slipdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrentry.slipdate;
            command.Parameters.Add("lrcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.lrcompanyid;
            command.Parameters.Add("purchasemanagerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.purchasemanagerid;
            command.Parameters.Add("stockholdingperiod", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.stockholdingperiod;
            command.Parameters.Add("lrsectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.lrsectionid;
            command.Parameters.Add("agentcommission", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrentry.agentcommission;
            command.Parameters.Add("chargeweight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrentry.chargeweight;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrentry.remarks == null ? "" : oLrentry.remarks;
            command.Parameters.Add("bundlerackid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.bundlerackid;
            command.Parameters.Add("importreferencetype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.importreferencetype;
            command.Parameters.Add("importreferenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.importreferenceid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrentry.id = Convert.ToInt64(reader[0].ToString());
                }
            }


            return oLrentry;
        }

        public Lrentry Update(Lrentry oLrentry)
        {
            if (oLrentry.id == 0) throw new Exception("Entry identifier missing.");
            List<Lrentry> lstoldentry = Search(new LrentryCreteria() { id = oLrentry.id });

            bool invoicenochanged = false;
            bool invoicedatechanged = false;
            bool sourcesupplierchanged = false;
            if (lstoldentry != null && lstoldentry.Count > 0)
            {
                invoicenochanged = (oLrentry.invoiceno.ToLower() != lstoldentry[0].invoiceno.ToLower());
                invoicedatechanged = (oLrentry.invoicedate.Date != lstoldentry[0].invoicedate.Date);
                sourcesupplierchanged = (oLrentry.sourcesupplierid != lstoldentry[0].sourcesupplierid);
            }
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    UpdateLRFreight(true, transaction, conn, oLrentry);

                    if (oLrentry.Changed_lrcompanyid || oLrentry.Changed_retaillocationid || oLrentry.Changed_supplierid
                        || invoicenochanged || invoicedatechanged)
                    {
                        string updatesupplier = @"update lrinvoice set lrsupplierid=@lrsupplierid,suppliercompanyid=@lrsuppliercompanyid where lrid=@lrid;
                        update stock set supplierid=@lrsupplierid where invoiceitemid in (
                        select lrinvoiceitems.id from lrinvoiceitems
                        inner join lrinvoice on lrinvoice.id = lrinvoiceid
                        where lrinvoice.lrid=@lrid);
                        update inbox set supplierid =@lrsupplierid,suppliername=supplier.name from supplier
                        where supplier.id = @lrsupplierid and inbox.typeid=1 and referenceid=@lrid;";

                        string updatecompany = @"update lrinvoice set companyid=@lrcompanyid where lrid=@lrid;
                        update stock set companyid=@lrcompanyid where invoiceitemid in (
                        select lrinvoiceitems.id from lrinvoiceitems
                        inner join lrinvoice on lrinvoice.id = lrinvoiceid
                        where lrinvoice.lrid=@lrid);

                        update salablegoods set companyid=@lrcompanyid where stockid in 
                        (select stock.id from stock 
                        inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                        inner join lrinvoice on lrinvoice.id = lrinvoiceid
                        where lrinvoice.lrid=@lrid);

                        update inbox set lrcompanyid =@lrcompanyid 
                        where inbox.typeid=1 and referenceid=@lrid;";

                        string updatesourcesupplier = @"update stock set srcsupplierid=@lrsrcsupplierid where invoiceitemid in (
                        select lrinvoiceitems.id from lrinvoiceitems
                        inner join lrinvoice on lrinvoice.id = lrinvoiceid
                        where lrinvoice.lrid=@lrid); ";

                        string updatelocation = @"
                        update salablegoods set locationid=@locationid where oqty=0 and locationid=@retaillocationid and stockid in 
                        (select stock.id from stock 
                        inner join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
                        inner join lrinvoice on lrinvoice.id = lrinvoiceid
                        where lrinvoice.lrid=@lrid);";
                        long suppliercompanyid = 0;
                        long retaillocationid = 0;

                        string query = "";
                        if (oLrentry.Changed_lrcompanyid && oLrentry.lrcompanyid > 0)
                            query += updatecompany + "\r\n";
                        if (oLrentry.Changed_supplierid && oLrentry.supplierid > 0)
                        {
                            SuppliercompanyDataService suppliercompanyDataService = new SuppliercompanyDataService() { Context = this.Context };
                            suppliercompanyid = suppliercompanyDataService.Search(new SuppliercompanyCreteria()
                            {
                                supplierid = oLrentry.supplierid
                            }).First().id;
                            query += updatesupplier + "\r\n";
                        }
                        if (oLrentry.Changed_retaillocationid && oLrentry.retaillocationid > 0)
                        {
                            retaillocationid = Search(new LrentryCreteria()
                            {
                                id = oLrentry.id
                            }).First().retaillocationid;
                            query += updatelocation + "\r\n";
                        }

                        if (invoicenochanged && invoicedatechanged)
                            query += "update lrinvoice set invoiceno=@invoiceno, invoicedate=@invoicedate where lrid=@lrid;" + "\r\n";
                        else if (invoicenochanged)
                            query += "update lrinvoice set invoiceno=@invoiceno where lrid=@lrid;" + "\r\n";
                        else if (invoicedatechanged)
                            query += "update lrinvoice set invoicedate=@invoicedate where lrid=@lrid;" + "\r\n";

                        if (sourcesupplierchanged)
                            query += updatesourcesupplier + "\r\n";

                        var param = new SortedList<string, object>();
                        param.Add("lrid", oLrentry.id);
                        param.Add("locationid", oLrentry.retaillocationid);
                        param.Add("retaillocationid", retaillocationid);
                        param.Add("lrcompanyid", oLrentry.lrcompanyid);
                        param.Add("lrsupplierid", oLrentry.supplierid);
                        param.Add("lrsuppliercompanyid", suppliercompanyid);
                        param.Add("invoiceno", oLrentry.invoiceno);
                        param.Add("invoicedate", oLrentry.invoicedate);
                        param.Add("lrsrcsupplierid", oLrentry.sourcesupplierid > 0 ? oLrentry.sourcesupplierid : oLrentry.supplierid);
                        ExecuteQuery(query, param, conn, transaction);
                    }
                    Update(transaction, conn, oLrentry);
                    LrentryfreightchargesDataService subLrentryfreightcharges = new LrentryfreightchargesDataService() { Context = this.Context };
                    subLrentryfreightcharges.Context = this.Context;
                    subLrentryfreightcharges.Update(transaction, conn, oLrentry.Lrentryfreightcharges, oLrentry.id);
                    if (!IsMigration)
                        InboxManager.Instance(Context.OrganizationID).Update(Context, false, oLrentry, transaction, conn);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrentry);
            }

            return oLrentry;
        }

        public Lrentry Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrentry oLrentry)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.id;
            command.Parameters.Add("lrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrentry.lrno == null ? "" : oLrentry.lrno;
            command.Parameters.Add("lrentryno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrentry.lrentryno == null ? "" : oLrentry.lrentryno;
            command.Parameters.Add("receiveddate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrentry.receiveddate;
            command.Parameters.Add("lrentrydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrentry.lrentrydate;
            command.Parameters.Add("lrdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrentry.lrdate;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.locationid;
            command.Parameters.Add("retaillocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.retaillocationid;
            command.Parameters.Add("entrylocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.entrylocationid;
            command.Parameters.Add("receivinglocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.receivinglocationid;
            command.Parameters.Add("fromlocationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.fromlocationid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.supplierid;
            command.Parameters.Add("sourcesupplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.sourcesupplierid > 0 ? oLrentry.sourcesupplierid : oLrentry.supplierid;
            command.Parameters.Add("hasfreightcharge", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrentry.hasfreightcharge;
            command.Parameters.Add("agentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.agentid;
            command.Parameters.Add("paymentmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.paymentmode;
            command.Parameters.Add("noofboxes", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.noofboxes;
            command.Parameters.Add("noofbundels", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.noofbundels;
            command.Parameters.Add("noofpieces", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.noofpieces;
            command.Parameters.Add("weight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrentry.weight;
            command.Parameters.Add("issuedlrno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrentry.issuedlrno == null ? "" : oLrentry.issuedlrno;
            command.Parameters.Add("issuedlrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.issuedlrid;
            command.Parameters.Add("totalamount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrentry.totalamount;
            command.Parameters.Add("premargin", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrentry.premargin;
            command.Parameters.Add("freightcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrentry.freightcharge;
            command.Parameters.Add("loadingcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrentry.loadingcharge;
            command.Parameters.Add("hasloadingcharge", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLrentry.hasloadingcharge;

            command.Parameters.Add("attachment", NpgsqlTypes.NpgsqlDbType.Json).Value = oLrentry.attachment_json == null ? "null" : oLrentry.attachment_json;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrentry.createdon;
            command.Parameters.Add("duedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrentry.duedate;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.status;
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.transportid;
            command.Parameters.Add("modeofdelivery", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.modeofdelivery;
            command.Parameters.Add("invoiceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrentry.invoiceno == null ? "" : oLrentry.invoiceno;
            command.Parameters.Add("invoicedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrentry.invoicedate;
            command.Parameters.Add("slipno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrentry.slipno == null ? "" : oLrentry.slipno;
            command.Parameters.Add("slipdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLrentry.slipdate;
            command.Parameters.Add("lrcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.lrcompanyid;
            command.Parameters.Add("purchasemanagerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.purchasemanagerid;
            command.Parameters.Add("stockholdingperiod", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.stockholdingperiod;
            command.Parameters.Add("lrsectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.lrsectionid;
            command.Parameters.Add("agentcommission", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrentry.agentcommission;
            command.Parameters.Add("chargeweight", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLrentry.chargeweight;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLrentry.remarks == null ? "" : oLrentry.remarks;
            command.Parameters.Add("bundlerackid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.bundlerackid;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLrentry.version = Convert.ToInt32(reader[0]);
                }
            }

            return oLrentry;
        }


        public bool Delete(Lrentry oLrentry)
        {
            string candeletequery = string.Format(@"select count(id) from lrinvoice
            where lrid = {0} and isactive=true", oLrentry.id);
            using (var reader = ExecuteReader(candeletequery, new SortedList<string, object>()))
            {
                if (reader.Read() && Convert.ToInt32(reader[0]) > 0)
                {
                    throw new Exception("Please remove invoice entries before deleting Transport Entry.");
                }
            }

            NpgsqlTransaction transaction = null;
            bool successCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    successCount = Delete(transaction, conn, oLrentry);
                    LrentryfreightchargesDataService subLrentryfreightcharges = new LrentryfreightchargesDataService() { Context = this.Context };
                    subLrentryfreightcharges.Context = this.Context;
                    subLrentryfreightcharges.DeleteAll(oLrentry.id, oLrentry.modifiedby, conn, transaction);

                    UpdateLRFreight(true, transaction, conn, oLrentry, true);

                    if (!IsMigration)
                        InboxManager.Instance(Context.OrganizationID).Delete(Context, oLrentry, transaction, conn);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLrentry);

            }
            return successCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Lrentry oLrentry)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = false;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLrentry.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLrentry.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }


        internal Lrentry ImportQuantoBill(LRBillImport item)
        {
            long fromcompanyid = 0;
            long tocompanyid = 0;
            long fromlocationid = 0;
            long tolocationid = 0;

            if (item.lrentry.id > 0)
            {
                item.lrentry.ErrorMessage = "Can't Update the Quanto bill import";
                return item.lrentry;
            }

            if (item.suppliercompanyid == 0 || item.lrentry.lrcompanyid == 0 || item.lrentry.supplierid == 0 || item.lrentry.retaillocationid <= 0)
            {
                item.lrentry.ErrorMessage = "Please select the mandatory fileds Company, Supplier, Supplier Company and Transfer Location.";
                return item.lrentry;
            }

            item.lrentry.invoicedate = item.bill.billdate;
            item.lrentry.invoiceno = item.bill.billno;

            if (DuplicateValidation(item.lrentry))
            {
                item.lrentry.ErrorMessage = string.Format("Duplicate LR Entry - The given bill {0} already imported", item.bill.billno);
                return item.lrentry;
            }

            fromcompanyid = item.bill.companyid;
            fromlocationid = item.bill.locationid;
            tocompanyid = item.lrentry.lrcompanyid;
            tolocationid = item.lrentry.retaillocationid;

            if (fromcompanyid == tocompanyid)
            {
                item.lrentry.ErrorMessage = "Transfer company and Receiving company are the same. Use stock transfer.";
                return item.lrentry;
            }
            if (fromlocationid == tolocationid)
            {
                item.lrentry.ErrorMessage = "Transfer location and Receiving locations are the same.";
                return item.lrentry;
            }


            InfyPOS.Data.LrentryDataService lrentryDataService = new InfyPOS.Data.LrentryDataService() { Context = this.Context }; ;
            InfyPOS.Data.LrinvoiceDataService lrinvoiceDataService = new InfyPOS.Data.LrinvoiceDataService() { Context = this.Context }; ;
            List<Salablegoods> transfergoods = new List<Salablegoods>();

            StockDataService stockDataService = new StockDataService() { Context = this.Context };
            var stockdetails = stockDataService.Search(item.bill.companyid, item.bill.Billitems.ConvertAll(e => e.barcode));

            bool onlyquantobarcode = !stockdetails.Exists(e => !string.IsNullOrEmpty(e.serialno));

            if (stockdetails.Exists(e => !string.IsNullOrEmpty(e.serialno)) &&
                stockdetails.Exists(e => !string.IsNullOrEmpty(e.serialno)))
            {
                throw new Exception("UAN based barcodes and Non UAN based barcode both can't be imported as single invoice");
            }

            Lrentry lrentry = new Lrentry();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                var transaction = conn.BeginTransaction();

                bool isnew = item.lrentry.id > 0;
                lrentry = item.lrentry.id == 0 ? Insert(transaction, conn, item.lrentry) : Update(transaction, conn,item.lrentry);

                if(isnew)
                    InboxManager.Instance(Context.OrganizationID).Update(Context, true, lrentry, transaction, conn);
                else
                    InboxManager.Instance(Context.OrganizationID).Update(Context, false, lrentry, transaction, conn);

                var lrinvoice = new InfyPOS.Models.LrinvoiceDetail()
                {
                    lrid = lrentry.id,
                    companyid = lrentry.lrcompanyid,
                    isinterstatesale = item.interstate || item.bill.BillTax.Exists(e => e.igsttax > 0),
                    isintercompanytransfer = onlyquantobarcode,
                    entrydate = DateTimeNow.Date,
                    invoicedate = item.bill.billdate,
                    invoiceno = item.bill.billno,
                    entrylocationid = lrentry.entrylocationid,
                    retaillocationid = lrentry.retaillocationid,
                    isactive = true,
                    noofpieces = (int)item.bill.totalpiece,
                    Lrinvoiceitems = new List<InfyPOS.Models.LrinvoiceItemDetail>(),
                    Lrinvoicetaxitems = new List<InfyPOS.Models.Lrinvoicetax>(),
                    suppliercompanyid = item.suppliercompanyid,
                    supplierid = lrentry.supplierid,
                    lrsupplierid = lrentry.supplierid,
                    totalamount = item.bill.total,
                    baseamount = item.bill.receivable - item.bill.tax,
                    billvalue = item.bill.total,
                    grossamount = item.bill.receivable,
                    taxamount = item.bill.tax,
                    discount = item.bill.discount,
                    otheramount = item.bill.rounding
                };

                foreach (var taxgroup in item.bill.BillTax)
                {
                    lrinvoice.Lrinvoicetaxitems.Add(new InfyPOS.Models.Lrinvoicetax()
                    {
                        isactive = true,
                        isinterstatesale = taxgroup.igsttax > 0,
                        percentage = taxgroup.taxpercentage,
                        taxid = taxgroup.taxid,
                        type = 0,
                        //amounton = taxgroup.amount - taxgroup.tax,
                        amounton = taxgroup.amount,
                        taxamount = taxgroup.tax,
                        //netamount = taxgroup.amount
                        netamount = taxgroup.totalamount

                    });
                }

                int serialno = 1;
                foreach (var lrinvoiceitem in item.bill.Billitems)
                {
                    var stockitem = stockdetails.Find(e => e.barcode == lrinvoiceitem.barcode);
                    var buyingprice = (lrinvoiceitem.receivable / lrinvoiceitem.qty) - (lrinvoiceitem.tax / lrinvoiceitem.qty);
                    var margin = Math.Round((lrinvoiceitem.price / buyingprice * 100) - 100, 2);
                    var newinvoiceitem = (new InfyPOS.Models.LrinvoiceItemDetail()
                    {
                        sno = serialno++,
                        productid = lrinvoiceitem.productid,
                        //basebuyingprice = lrinvoiceitem.price + (lrinvoiceitem.discount / lrinvoiceitem.qty) + lrinvoiceitem.additionaldiscount,
                        basebuyingprice = buyingprice + (lrinvoiceitem.discount / lrinvoiceitem.qty) + lrinvoiceitem.additionaldiscount,
                        buyingprice = (lrinvoiceitem.receivable / lrinvoiceitem.qty) - (lrinvoiceitem.tax / lrinvoiceitem.qty),
                        buyingqty = lrinvoiceitem.qty,
                        actualqty = lrinvoiceitem.qty,
                        designid = stockitem.designid,
                        amount = lrinvoiceitem.amount,
                        hsncode = lrinvoiceitem.hsncode,
                        itemid = stockitem.itemid,
                        lrid = lrentry.id,
                        mrp = stockitem.mrp > 0 ? stockitem.mrp : lrinvoiceitem.mrpdisplay,
                        productname = stockitem.patternname,
                        ptaxid = lrinvoiceitem.taxid,
                        purchasetax = lrinvoiceitem.tax,
                        purchasetaxpercentage = lrinvoiceitem.taxpercentage,
                        salediscountpercentage = lrinvoiceitem.discountpercentage,
                        salediscount = lrinvoiceitem.discount,
                        sellingprice = lrinvoiceitem.price,
                        supplierid = lrentry.supplierid,
                        srcsupplierid = stockitem.supplierid,
                        purchasetotaldiscount = Math.Abs(lrinvoiceitem.totaldiscount),
                        purchasediscount = lrinvoiceitem.discount == 0 ? 0 : (Math.Abs(lrinvoiceitem.totaldiscount) / lrinvoiceitem.qty),
                        purchasediscountpercentage = lrinvoiceitem.totaldiscountpercentage,
                        marginpercentage = margin,
                        taxid = lrinvoiceitem.taxid,
                        salestaxpercentage = lrinvoiceitem.taxpercentage,
                        isactive = true,
                        uan = stockitem.serialno,
                        Details = new InfyPOS.Models.Items()
                        {
                            id = stockitem.itemid,
                            productid = lrinvoiceitem.productid,
                            sellingname = stockitem.printingname,
                            productname = stockitem.printingname,
                            printingname = stockitem.printingname,
                            serialno = stockitem.serialno,
                            quantoexistingbarcode = stockitem.barcode
                        }
                    });
                    lrinvoice.Lrinvoiceitems.Add(newinvoiceitem);

                    if (item.supportmrp && newinvoiceitem.mrp > 0)
                    {
                        newinvoiceitem.sellingprice = newinvoiceitem.mrp;
                    }
                    else if (item.marginpercentage > 0)
                    {
                        newinvoiceitem.marginpercentage = item.marginpercentage;
                        newinvoiceitem.sellingprice = Math.Round((newinvoiceitem.amount / newinvoiceitem.buyingqty) * item.marginpercentage / 100, 0);
                    }

                    if (item.discountpercentage > 0)
                    {
                        newinvoiceitem.salediscount = Math.Round((newinvoiceitem.sellingprice) * item.discountpercentage / 100, 0);
                        newinvoiceitem.salediscountpercentage = item.discountpercentage;
                    }

                    if (transfergoods.Exists(e => e.id == lrinvoiceitem.salablegoodsid
                    && e.barcode == lrinvoiceitem.barcode
                    && e.locationid == tolocationid))
                    {
                        var existingitem = transfergoods.Find(e => e.id == lrinvoiceitem.salablegoodsid
                        && e.barcode == lrinvoiceitem.barcode
                        && e.locationid == tolocationid);
                        existingitem.iqty += lrinvoiceitem.qty;
                        existingitem.aqty += lrinvoiceitem.qty;
                    }
                    else
                    {
                        var tranitem = (new InfyPOS.Models.Salablegoods()
                        {
                            id = lrinvoiceitem.salablegoodsid,
                            iqty = lrinvoiceitem.qty,
                            aqty = lrinvoiceitem.qty,
                            barcode = lrinvoiceitem.barcode,
                            supplierid = lrentry.supplierid,
                            stockid = lrinvoiceitem.stockid,
                            dqty = 0,
                            itemid = stockitem.itemid,
                            locationid = tolocationid
                        });
                        transfergoods.Add(tranitem);
                    }
                }
                if (transfergoods.Any(e => e.id == 0 || e.stockid == 0 || e.itemid == 0 || string.IsNullOrEmpty(e.barcode)))
                    throw new Exception("Input Transfer details missing.");


                lrinvoiceDataService.Insert(lrinvoice, conn, transaction);

                if (item.autogeneratebarcode)
                {
                    var items = lrinvoice.Lrinvoiceitems.ConvertAll(e => new Lrinvoiceitemsbarcode()
                    {
                        id = e.id,
                        buyingqty = e.buyingqty,
                        freeqty = e.freeqty,
                        companyid = lrinvoice.companyid,
                        Details = e.Details,
                        hsncode = e.hsncode,
                        itemid = e.itemid,
                        itemdetail = e.itemdetail,
                        lrid = e.lrid,
                        newbarcode = 1,
                        buyingprice = e.buyingprice,
                        printingname = e.productname,
                        productid = e.productid,
                        salediscount = e.salediscount,
                        sellingprice = e.sellingprice,
                        stocklocationid = e.stocklocationid,
                        srcsupplierid = e.srcsupplierid,
                        version = e.version,
                        isQuantoImport = true,
                        mrp = e.mrp,
                        quantoexistingbarcode = item.supportuanno && e.Details != null ? e.Details.quantoexistingbarcode : "",
                        gln = item.supportuanno && e.Details != null ? e.Details.serialno : ""
                    });

                    // Quanto Bill Import feature is only for Inter company transfer 
                    // So no need to create new barcodes for existing Quanto barcodes
                    // Do Stock import alone 

                    if (item.supportuanno && items.All(x => !string.IsNullOrEmpty(x.gln)))
                    {
                        stockDataService.CreateBarcode(items, transaction, conn);
                    }

                    if (lrinvoice.id > 0)
                    {
                        var query = @"update lrinvoiceitems set barcodedon=@barcodedon where lrinvoiceid=@lrinvoiceid and isactive=true;";
                        var command = GetCommand(query, conn, transaction);
                        command.CommandTimeout = 10000;
                        command.Parameters.Add("barcodedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                        command.Parameters.Add("lrinvoiceid", NpgsqlTypes.NpgsqlDbType.Numeric).Value = lrinvoice.id;
                        command.ExecuteNonQuery();
                    }

                    // UAN barcode won't be transfered, have to generate barcode 
                    if (!(item.supportuanno && items.All(x => !string.IsNullOrEmpty(x.gln))) && onlyquantobarcode)
                        CompanyTransfer(lrinvoice,transfergoods, fromcompanyid, fromlocationid, tocompanyid, tolocationid, conn, transaction);
                }


                transaction.Commit();
                conn.Close();

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                throw exp;
            }
            lrentry.ErrorMessage = "";
            return lrentry;
        }

        public class SWAPgoods
        {
            public long fromid { get; set; }
            public long toid { get; set; }
            public decimal qty { get; set; }
        }
        internal bool CompanyTransfer(Lrinvoice invoice, List<Salablegoods> salablegoods, long fromcompanyid, long fromlocationid,
            long tocompanyid, long tolocationid, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            List<Stocktransaction> stocktransactions = new List<Stocktransaction>();

            const string SQL_QTYIncrease = @"Update public.salablegoods set iqty=iqty+@qty,tiqty=COALESCE(tiqty,0)+@qty,version=COALESCE(version,0::int)+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id";

            SalablegoodsDataService subSalablegoods = new SalablegoodsDataService() { Context = this.Context };
            var fromstock = subSalablegoods.Search(new SalablegoodsCreteria()
            {
                locationid = fromlocationid,
                ids = salablegoods.ConvertAll<long>(e => e.id)
            }, conn, transaction);

            var tostock = subSalablegoods.Search(new SalablegoodsCreteria()
            {
                locationid = tolocationid,
                barcodes = salablegoods.ConvertAll<string>(e => WebAPI.Data.Utility.SafeSQL(e.barcode))
            }, conn, transaction);

            List<SWAPgoods> swap = new List<SWAPgoods>();
            var existinggoods = tostock.FindAll(e => fromstock.Exists(x => x.barcode == e.barcode));
            var newsalablegoods = fromstock.FindAll(e => !tostock.Exists(x => x.barcode == e.barcode));

            foreach (var item in existinggoods)
            {
                var qty = salablegoods.Find(e => e.barcode == item.barcode).aqty;

                NpgsqlCommand command = GetCommand(SQL_QTYIncrease, conn, transaction);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.id;
                command.Parameters.Add("qty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = qty;
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                command.ExecuteNonQuery();
                stocktransactions.Add(new Stocktransaction()
                {
                    qty = qty,
                    locationid = item.locationid,
                    salablegoodsid = item.id,
                    type = Stocktransaction.TransactionType.Purchase,
                    refid = invoice.id,
                    reftype = "PU"
                });
            }

            foreach (var item in newsalablegoods)
            {
                var qty = salablegoods.Find(e => e.barcode == item.barcode).aqty;
                var citem = item.Clone();
                citem.id = 0;
                citem.createdby = Context.UserID;
                citem.modifiedby = Context.UserID;
                citem.createdon = DateTimeNow;
                citem.modifiedon = DateTimeNow;
                citem.isactive = true;
                citem.version = 1;
                citem.bundleid = 0;
                citem.iqty = qty;
                citem.oqty = 0;
                citem.dqty = 0;
                citem.jqty = 0;
                citem.pqty = 0;
                citem.rqty = 0;
                citem.tiqty = qty;
                citem.toqty = 0;
                citem.salereturnqty = 0;
                citem.locationid = tolocationid;
                citem.companyid = tocompanyid;

                subSalablegoods.isMigration = true;
                if (!subSalablegoods.Insert(transaction, conn, citem))
                    throw new Exception(citem.barcode + " don't have enough stock to transfer.");

                stocktransactions.Add(new Stocktransaction()
                {
                    qty = qty,
                    locationid = citem.locationid,
                    salablegoodsid = citem.id,
                    type = Stocktransaction.TransactionType.Purchase,
                    refid = invoice.id,
                    reftype = "PU"
                });
            }

            StocktransactionDataService stocktransactionDataService = new StocktransactionDataService() { Context = this.Context };
            stocktransactions.ForEach(x =>
            {
                stocktransactionDataService.Insert(transaction, conn, x);
            });
            return true;
        }

        internal LrinvoiceCover.TDSInformation ComputeTDS(Lrinvoice lrinvoice)
        {
            LrinvoiceCover.TDSInformation tDSInformation = new LrinvoiceCover.TDSInformation();
            string query = @"
            select tdsgroupid from suppliercompany where id=@suppliercompanyid;

            select sum(baseamount-discount)  from lrinvoice
            where suppliercompanyid=@suppliercompanyid
            and isactive=true 
            and lrinvoice.id != @invoiceid
            and entrydate>=@from and entrydate<@to;

            select sum(amount) from lrinvoicereturn
            where suppliercompanyid=@suppliercompanyid
            and isactive=true
            and returndate>=@from and returndate<@to;";

            DateTime from = DateTime.MinValue;
            DateTime to = DateTime.MinValue;
            if (lrinvoice.entrydate.Month > 3)
            {
                from = new DateTime(lrinvoice.entrydate.Year, 4, 1);
                to = new DateTime(lrinvoice.entrydate.Year + 1, 3, 31);
            }
            else
            {
                from = new DateTime(lrinvoice.entrydate.Year - 1, 4, 1);
                to = new DateTime(lrinvoice.entrydate.Year, 3, 31);
            }
            var param = new SortedList<string, object>();
            param.Add("from", from);
            param.Add("to", to.AddDays(1));
            param.Add("suppliercompanyid", lrinvoice.suppliercompanyid);
            param.Add("invoiceid", lrinvoice.id);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    tDSInformation.tdsledgerid = reader["tdsgroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tdsgroupid"]);
                }
                if (reader.NextResult() && reader.Read())
                    tDSInformation.invoiceamount = reader[0] == DBNull.Value ? 0 : Convert.ToInt64(reader[0]);
                if (reader.NextResult() && reader.Read())
                    tDSInformation.returnamount = reader[0] == DBNull.Value ? 0 : Convert.ToInt64(reader[0]);
            }
            tDSInformation.returnamount = 0;
            if (tDSInformation.tdsledgerid > 0)
            {
                LedgerDataService ledgerDataService = new LedgerDataService() { Context = this.Context };
                var ledgers = ledgerDataService.Search(new LedgerCreteria()
                {
                    id = tDSInformation.tdsledgerid
                });
                if (ledgers.Count > 0)
                {
                    tDSInformation.tdsledger = ledgers[0];
                    if (tDSInformation.tdsledger.attributes != null &&
                        tDSInformation.tdsledger.attributes.tdsrules != null)
                    {
                        if (tDSInformation.tdsledger.attributes.tdsrules.Exists(x => x.eligible == 1))
                        {
                            tDSInformation.singletransaction = tDSInformation.tdsledger.attributes.tdsrules.Find(x => x.eligible == 1).value;
                        }
                        if (tDSInformation.tdsledger.attributes.tdsrules.Exists(x => x.eligible == 2))
                        {
                            tDSInformation.totaltransaction = tDSInformation.tdsledger.attributes.tdsrules.Find(x => x.eligible == 2).value;
                        }
                    }
                }
            }
            return tDSInformation;
        }

        internal List<LrRCMEntry> SearchRCMTransport(LrentryCreteria _lrentry)
        {
            string query = @"select transportid ,
            lrentrydate ,
            lrdate ,
            lrno ,
            lrentryno ,
            supplierid,
            freightcharge,
		    company.tinanddate cgst,
		    transport.gst tgst,
		    transport.rcm,
		    tax.taxpercentage,
            hasfreightcharge
            from lrentry
            inner join transport on transport.id = lrentry.transportid
            inner join company on company.id = lrentry.lrcompanyid
            left outer join tax on tax.id = transport.taxid 
            where transport.rcm=true";
            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = query;
            qb.AddParameter("lrentry.isactive", true, "=");
            //qb.Limit = 100;
            qb.SortField = "lrentry.id desc";
            if (!string.IsNullOrEmpty(_lrentry.lrno))
            {
                qb.AddParameter("lrentry.lrno", _lrentry.lrno, "=");
            }
            if (!string.IsNullOrEmpty(_lrentry.lrentryno))
            {
                qb.AddParameter("lrentry.lrentryno", _lrentry.lrentryno, "=");
            }
            if (_lrentry.transportid > 0)
            {
                qb.AddParameter("lrentry.transportid", _lrentry.transportid, "=");
            }
            if (_lrentry.fromlocationid > 0)
            {
                qb.AddParameter("lrentry.fromlocationid", _lrentry.fromlocationid, "=");
            }
            if (_lrentry.supplierid > 0)
            {
                qb.AddParameter("lrentry.supplierid", _lrentry.supplierid, "=");
            }

            if (_lrentry.fromdate.Year > 1900)
                qb.AddParameter("lrentry.lrdate", "fromdate", _lrentry.fromdate.Date, ">=");
            if (_lrentry.todate.Year > 1900)
                qb.AddParameter("lrentry.lrdate", "todate", _lrentry.todate.Date.AddDays(1), "<");

            if (_lrentry.entryfromdate.Year > 1900)
                qb.AddParameter("lrentry.lrentrydate", "entryfromdate", _lrentry.entryfromdate.Date, ">=");
            if (_lrentry.entrytodate.Year > 1900)
                qb.AddParameter("lrentry.lrentrydate", "entrytodate", _lrentry.entrytodate.Date.AddDays(1), "<");


            NpgsqlConnection conn = CreateConnection();
            List<LrRCMEntry> results = new List<LrRCMEntry>();
            try
            {
                conn.Open();

                NpgsqlCommand command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        LrRCMEntry ret = new LrRCMEntry();
                        results.Add(ret);
                        ret.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        ret.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);
                        ret.tgst = reader["tgst"] == DBNull.Value ? "" : Convert.ToString(reader["tgst"]);
                        if (reader["hasfreightcharge"] != DBNull.Value &&
                        Convert.ToBoolean(reader["hasfreightcharge"]))
                        {
                            ret.freightcharge = reader["freightcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["freightcharge"]);
                            if (ret.taxpercentage > 0 &&
                                ret.freightcharge > 0)
                            {
                                ret.tax = ret.freightcharge * ret.taxpercentage / 100;
                                ret.total = ret.tax + ret.freightcharge;
                            }
                            if (reader["tgst"] != DBNull.Value && Convert.ToString(reader["tgst"]) != "" &&
                                Convert.ToString(reader["tgst"]).Substring(0, 2) != Convert.ToString(reader["cgst"]).Substring(0, 2))
                            {
                                ret.isinterstate = true;
                            }
                        }
                        ret.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"]);
                        ret.lrno = reader["lrno"] == DBNull.Value ? "" : reader["lrno"].ToString();
                        ret.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                        ret.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"].ToString());
                        ret.lrentrydate = reader["lrentrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrentrydate"].ToString());
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _lrentry);
            }
            return results;
        }
    }

}
