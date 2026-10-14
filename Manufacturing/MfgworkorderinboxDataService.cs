using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class MfgworkorderinboxDataService : SAASData
    {
        const string SQL_Search = @"select mfgworkorderinbox.id, mfgworkorderid, mfgworkorderinbox.workorderno, mfgworkorderinbox.statusid, mfgworkorderinbox.isactive, 
mfgworkorderinbox.createdby, mfgworkorderinbox.modifiedby, mfgworkorderinbox.version, mfgworkorderinbox.createdon, mfgworkorderinbox.modifiedon, 
mfgworkorderinbox.workorderdate, mfgworkorderinbox.supplierid, mfgworkorderinbox.description, mfgworkorderinbox.inboxtype, mfgworkorderinbox.mfgjoborderid,
mfgworkorderinbox.joborderno,mfgworkorderinbox.jobcardno,mfgworkorderinbox.jobname, mfgworkorderinbox.jobstatusid, mfgworkorderinbox.workorderstatusid, mfgworkorderinbox.woexpdate, 
mfgworkorderinbox.joborderdate, mfgworkorderinbox.joexpdate, mfgworkorderinbox.suppliername, mfgworkorderinbox.inqty, mfgworkorderinbox.outqty, mfgworkorderinbox.expqty, 
mfgworkorderinbox.locationid, mfgworkorderinbox.companyid,mfgworkorder.design,mfgworkorder.product,mfgworkorderinbox.fabricqty,mfgworkorderinbox.wasteqty,isbomprocess,
mfgworkorderinbox.secondqty,mfgworkorderinbox.transferedqty,mfgworkorderinbox.workflowid,mfgworkorderinbox.stageid,mfgworkorder.lotno,customer.name customername,
Mfgsampleorder.entryno,Mfgsampleorder.entrydate,Mfgsampleorder.designno,Mfgsampleorder.samplename,division.name division,employee.name merchant,agent.name marketing
from mfgworkorderinbox
inner join mfgworkorder on mfgworkorder.id = mfgworkorderid
left outer join Mfgsampleorder on Mfgsampleorder.id = mfgsampleorderid
left outer join referencelist division on division.id = prddivisionid
left outer join customer on customer.id = mfgworkorder.customerid
left outer join employee on employee.id = mfgsampleorder.preparedby
left outer join agent on agent.id = mfgsampleorder.agentid
  where mfgworkorderinbox.id=@id";


        const string SQL_Insert = @"INSERT INTO public.mfgworkorderinbox(mfgworkorderid, workorderno, statusid, isactive, createdby, modifiedby, version, createdon,
    modifiedon, workorderdate, supplierid, description, inboxtype, mfgjoborderid, joborderno,jobcardno, jobname, jobstatusid, workorderstatusid, woexpdate, joborderdate, joexpdate, 
    suppliername, inqty, outqty, expqty, locationid, companyid,fabricqty,wasteqty,isbomprocess,secondqty,transferedqty,workflowid,stageid) VALUES(@mfgworkorderid, @workorderno, @statusid, @isactive, @createdby,
    @modifiedby, @version, @createdon, @modifiedon, @workorderdate, @supplierid, @description, @inboxtype, @mfgjoborderid, @joborderno,@jobcardno, @jobname, @jobstatusid, 
    @workorderstatusid, @woexpdate, @joborderdate, @joexpdate, @suppliername, @inqty, @outqty, @expqty, 
    @locationid, @companyid,@fabricqty,@wasteqty,@isbomprocess,@secondqty,@transferedqty,@workflowid,@stageid) RETURNING id";



        const string SQL_Update = @"UPDATE public.mfgworkorderinbox SET mfgworkorderid=@mfgworkorderid, workorderno=@workorderno, statusid=@statusid, 
    modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, workorderdate=@workorderdate, supplierid=@supplierid, description=@description, 
    inboxtype=@inboxtype, mfgjoborderid=@mfgjoborderid, joborderno=@joborderno, jobname=@jobname, jobcardno=@jobcardno,jobstatusid=@jobstatusid, workorderstatusid=@workorderstatusid, 
    woexpdate=@woexpdate, joborderdate=@joborderdate, joexpdate=@joexpdate, suppliername=@suppliername, inqty=@inqty, outqty=@outqty, expqty=@expqty, locationid=@locationid, 
    companyid=@companyid,fabricqty=@fabricqty,wasteqty=@wasteqty,isbomprocess=@isbomprocess,secondqty=@secondqty,transferedqty=@transferedqty where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.mfgworkorderinbox set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Mfgworkorderinbox> Search(MfgworkorderinboxCreteria _mfgworkorderinbox)
        {
            List<Mfgworkorderinbox> lst = new List<Mfgworkorderinbox>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgworkorderinbox.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgworkorderinbox.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("mfgworkorderinbox.isactive", true, "=");
                    qb.AddParameter("mfgworkorder.isactive", true, "=");

                    if (_mfgworkorderinbox.statusid > 0)
                        qb.AddParameter("mfgworkorderinbox.statusid", _mfgworkorderinbox.statusid, "=");
                    else
                        qb.AddParameter("mfgworkorderinbox.statusid", 9, "!=");

                    if (_mfgworkorderinbox.mfgjoborderid > 0)
                        qb.AddParameter("mfgworkorderinbox.mfgjoborderid", _mfgworkorderinbox.mfgjoborderid, "=");

                    if (_mfgworkorderinbox.mfgworkorderid > 0)
                        qb.AddParameter("mfgworkorderinbox.mfgworkorderid", _mfgworkorderinbox.mfgworkorderid, "=");

                    if (_mfgworkorderinbox.customerid > 0)
                        qb.AddParameter("mfgworkorder.customerid", _mfgworkorderinbox.customerid, "=");

                    if (_mfgworkorderinbox.supplierid > 0)
                        qb.AddParameter("mfgworkorderinbox.supplierid", _mfgworkorderinbox.supplierid, "=");

                    if (_mfgworkorderinbox.addedfrom != DateTime.MinValue && _mfgworkorderinbox.addedto != DateTime.MinValue)
                    {
                        qb.AddParameter("mfgworkorder.wodate", "fromwodate", _mfgworkorderinbox.addedfrom.Date, ">=");
                        qb.AddParameter("mfgworkorder.wodate", "towodate", _mfgworkorderinbox.addedto.AddDays(1).Date, "<");
                    }

                    qb.CurrentQuery += " order by mfgworkorder.id desc";

                    //if (!string.IsNullOrEmpty(_mfgworkorderinbox.Code))
                    //    qb.AddParameter("code", _mfgworkorderinbox.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgworkorderinbox omfgworkorderinbox = new Mfgworkorderinbox();
                        omfgworkorderinbox.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgworkorderinbox.mfgworkorderid = reader["mfgworkorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgworkorderid"]);
                        omfgworkorderinbox.workorderno = reader["workorderno"] == DBNull.Value ? "" : reader["workorderno"].ToString();
                        omfgworkorderinbox.statusid = reader["statusid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["statusid"]);

                        omfgworkorderinbox.workflowid = reader["workflowid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["workflowid"]);
                        omfgworkorderinbox.stageid = reader["stageid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stageid"]);

                        omfgworkorderinbox.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgworkorderinbox.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgworkorderinbox.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgworkorderinbox.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgworkorderinbox.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgworkorderinbox.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgworkorderinbox.workorderdate = reader["workorderdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["workorderdate"]);
                        omfgworkorderinbox.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        omfgworkorderinbox.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        omfgworkorderinbox.inboxtype = reader["inboxtype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["inboxtype"]);
                        omfgworkorderinbox.mfgjoborderid = reader["mfgjoborderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["mfgjoborderid"]);
                        omfgworkorderinbox.joborderno = reader["joborderno"] == DBNull.Value ? "" : reader["joborderno"].ToString();
                        omfgworkorderinbox.jobcardno = reader["jobcardno"] == DBNull.Value ? "" : reader["jobcardno"].ToString();
                        omfgworkorderinbox.jobname = reader["jobname"] == DBNull.Value ? "" : reader["jobname"].ToString();
                        omfgworkorderinbox.lotno = reader["lotno"] == DBNull.Value ? "" : reader["lotno"].ToString();

                        omfgworkorderinbox.jobstatusid = reader["jobstatusid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["jobstatusid"]);
                        omfgworkorderinbox.workorderstatusid = reader["workorderstatusid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["workorderstatusid"]);
                        omfgworkorderinbox.woexpdate = reader["woexpdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["woexpdate"]);
                        omfgworkorderinbox.joborderdate = reader["joborderdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["joborderdate"]);
                        omfgworkorderinbox.joexpdate = reader["joexpdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["joexpdate"]);
                        omfgworkorderinbox.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                        omfgworkorderinbox.inqty = reader["inqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["inqty"]);
                        omfgworkorderinbox.outqty = reader["outqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["outqty"]);
                        omfgworkorderinbox.expqty = reader["expqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["expqty"]);

                        omfgworkorderinbox.fabricqty = reader["fabricqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["fabricqty"]);
                        omfgworkorderinbox.wasteqty = reader["wasteqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["wasteqty"]);
                        omfgworkorderinbox.secondqty = reader["secondqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["secondqty"]);
                        omfgworkorderinbox.isbomprocess = reader["isbomprocess"] == DBNull.Value ? false : Convert.ToBoolean(reader["isbomprocess"]);

                        omfgworkorderinbox.transferedqty = reader["transferedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["transferedqty"]);

                        omfgworkorderinbox.customername = reader["customername"] == DBNull.Value ? "" : reader["customername"].ToString();

                        omfgworkorderinbox.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        omfgworkorderinbox.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        omfgworkorderinbox.design = reader["design"] == DBNull.Value ? "" : reader["design"].ToString();
                        omfgworkorderinbox.product = reader["product"] == DBNull.Value ? "" : reader["product"].ToString();
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 6)
                        {
                            omfgworkorderinbox.mfglabel = new Mfgworkorderinbox.mfglabelinfo();
                            omfgworkorderinbox.mfglabel.samplename = reader["samplename"] == DBNull.Value ? "" : reader["samplename"].ToString();
                            omfgworkorderinbox.mfglabel.division = reader["division"] == DBNull.Value ? "" : reader["division"].ToString();
                            omfgworkorderinbox.mfglabel.entrydate = reader["designno"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["entrydate"]);
                            omfgworkorderinbox.mfglabel.designno = reader["designno"] == DBNull.Value ? "" : reader["designno"].ToString();
                            omfgworkorderinbox.mfglabel.entryno = reader["entryno"] == DBNull.Value ? "" : reader["entryno"].ToString();
                            omfgworkorderinbox.mfglabel.merchant = reader["merchant"] == DBNull.Value ? "" : reader["merchant"].ToString();
                            omfgworkorderinbox.mfglabel.marketing = reader["marketing"] == DBNull.Value ? "" : reader["marketing"].ToString();
                        }
                        lst.Add(omfgworkorderinbox);
                    }
                }

                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.mfg_mode == 6)
                {
                    string query = string.Format(@"with sampleorders as
                    (
                        select  mfgworkorderitems.mfgworkorderid,salesorderid,
                        sum(Mfgworkorderitemsizes.qty) expqty from mfgworkorderitems
                        inner join Mfgworkorderitemsizes on Mfgworkorderitemsizes.mfgworkorderitemsid = mfgworkorderitems.id
                        AND Mfgworkorderitemsizes.ISACTIVE=TRUE
                        where mfgworkorderitems.mfgworkorderid in ({0}) AND mfgworkorderitems.ISACTIVE=TRUE
                        group by mfgworkorderitems.mfgworkorderid,salesorderid
                    )
                    select mfgworkorderid,sono,sodate,sampleorders.expqty,sum(salesorderitems.oqty) soqty from sampleorders
                    inner join salesorder on salesorder.id = salesorderid
                    inner join salesorderitems on salesorderitems.salesorderid = salesorder.id and salesorderitems.isactive=true
                    group by mfgworkorderid,sono,sodate,sampleorders.expqty",
                   string.Join(",", lst.ConvertAll(x => x.mfgworkorderid)));
                    using (var reader = ExecuteReader(query, null))
                    {
                        while (reader.Read())
                        {
                            long mfgworkorderid = Convert.ToInt64(reader["mfgworkorderid"]);
                            var salesorders = lst.FindAll(x => x.mfgworkorderid == mfgworkorderid);
                            if(salesorders.Count>0)
                            {
                                salesorders.ForEach(salesorder =>
                                {
                                    if (salesorder.mfglabel == null) salesorder.mfglabel = new Mfgworkorderinbox.mfglabelinfo();
                                    salesorder.mfglabel.sono = reader["sono"].ToString();
                                    salesorder.mfglabel.sodate = Convert.ToDateTime(reader["sodate"]);
                                    salesorder.mfglabel.duedays = (int)DateTime.Now.Subtract(salesorder.mfglabel.sodate).TotalDays;
                                    salesorder.mfglabel.soqty = reader["soqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["soqty"]);
                                    salesorder.expqty = reader["expqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["expqty"]);
                                });
                            }
                            
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgworkorderinbox);
            }
            return lst;
        }

        public Mfgworkorderinbox Insert(Mfgworkorderinbox oMfgworkorderinbox)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oMfgworkorderinbox);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkorderinbox);
                throw exp;
            }

            return oMfgworkorderinbox;
        }

        public Mfgworkorderinbox Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkorderinbox oMfgworkorderinbox)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.mfgworkorderid;
            command.Parameters.Add("workorderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderinbox.workorderno == null ? "" : oMfgworkorderinbox.workorderno;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.statusid;
            command.Parameters.Add("workflowid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.workflowid;
            command.Parameters.Add("stageid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.stageid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkorderinbox.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderinbox.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderinbox.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderinbox.modifiedon;
            command.Parameters.Add("workorderdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderinbox.workorderdate;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.supplierid;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderinbox.description == null ? "" : oMfgworkorderinbox.description;
            command.Parameters.Add("inboxtype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderinbox.inboxtype;
            command.Parameters.Add("mfgjoborderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.mfgjoborderid;
            command.Parameters.Add("joborderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderinbox.joborderno == null ? "" : oMfgworkorderinbox.joborderno;
            command.Parameters.Add("jobcardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderinbox.jobcardno == null ? "" : oMfgworkorderinbox.jobcardno;
            command.Parameters.Add("jobname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderinbox.jobname == null ? "" : oMfgworkorderinbox.jobname;
            command.Parameters.Add("jobstatusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderinbox.jobstatusid;
            command.Parameters.Add("workorderstatusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderinbox.workorderstatusid;
            command.Parameters.Add("woexpdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderinbox.woexpdate;
            command.Parameters.Add("joborderdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderinbox.joborderdate;
            command.Parameters.Add("joexpdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderinbox.joexpdate;
            command.Parameters.Add("suppliername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderinbox.suppliername == null ? "" : oMfgworkorderinbox.suppliername;
            command.Parameters.Add("inqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderinbox.inqty;
            command.Parameters.Add("outqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderinbox.outqty;
            command.Parameters.Add("expqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderinbox.expqty;

            command.Parameters.Add("isbomprocess", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkorderinbox.isbomprocess;

            command.Parameters.Add("fabricqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderinbox.fabricqty;
            command.Parameters.Add("wasteqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderinbox.wasteqty;
            command.Parameters.Add("secondqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderinbox.secondqty;
            command.Parameters.Add("transferedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderinbox.transferedqty;

            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.companyid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgworkorderinbox.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfgworkorderinbox;
        }

        public Mfgworkorderinbox Update(Mfgworkorderinbox oMfgworkorderinbox)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfgworkorderinbox);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkorderinbox);
                throw exp;
            }

            return oMfgworkorderinbox;
        }

        public void Update(Mfgjoborder oMfgjoborder, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            var existing = Search(new MfgworkorderinboxCreteria()
            {
                inboxtype = 2,
                mfgjoborderid = oMfgjoborder.id
            });
            if (existing.Count == 0)
            {
                Insert(oMfgjoborder, conn, transaction);
            }
            else
            {

                var exist = existing.First();
                if (exist.locationid != oMfgjoborder.tolocationid ||
                    exist.companyid != oMfgjoborder.companyid ||
                    exist.supplierid != oMfgjoborder.supplierid ||
                    exist.outqty != oMfgjoborder.Mfgjoborderitems.Sum(e => e.oqty) ||
                    exist.jobname != oMfgjoborder.stagename)
                {
                    exist.locationid = oMfgjoborder.tolocationid;
                    exist.companyid = oMfgjoborder.companyid;
                    exist.supplierid = oMfgjoborder.supplierid;
                    exist.outqty = oMfgjoborder.Mfgjoborderitems.Sum(e => e.oqty);
                    exist.jobname = oMfgjoborder.stagename;
                    Update(transaction, conn, exist);
                }
            }
        }

        public void Update(Mfgworkorder oMfgworkorder, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            var existing = Search(new MfgworkorderinboxCreteria()
            {
                inboxtype = 1,
                mfgworkorderid = oMfgworkorder.id
            });
            if (existing.Count == 0)
            {
                Insert(oMfgworkorder, conn, transaction);
            }
            else
            {
                var exist = existing.First();
                if (exist.supplierid != oMfgworkorder.supplierid ||
                    exist.companyid != oMfgworkorder.companyid ||
                    exist.outqty != oMfgworkorder.totalqty ||
                    exist.woexpdate != oMfgworkorder.expectdate ||
                    exist.workorderdate != oMfgworkorder.wodate)
                {
                    exist.supplierid = oMfgworkorder.supplierid;
                    exist.companyid = oMfgworkorder.companyid;
                    exist.outqty = oMfgworkorder.totalqty;
                    exist.woexpdate = oMfgworkorder.expectdate;
                    exist.workorderdate = oMfgworkorder.wodate;
                    SupplierDataService supplierDataService = new SupplierDataService() { Context = this.Context };
                    exist.suppliername = supplierDataService.Search(new SupplierCreteria()
                    {
                        id = oMfgworkorder.supplierid
                    }).First().name;
                    Update(transaction, conn, exist);
                }
            }
        }


        public Mfgworkorderinbox Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkorderinbox oMfgworkorderinbox)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.id;
            command.Parameters.Add("mfgworkorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.mfgworkorderid;
            command.Parameters.Add("workorderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderinbox.workorderno == null ? "" : oMfgworkorderinbox.workorderno;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.statusid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkorderinbox.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderinbox.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderinbox.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderinbox.modifiedon;
            command.Parameters.Add("workorderdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderinbox.workorderdate;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.supplierid;
            command.Parameters.Add("description", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderinbox.description == null ? "" : oMfgworkorderinbox.description;
            command.Parameters.Add("inboxtype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderinbox.inboxtype;
            command.Parameters.Add("mfgjoborderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.mfgjoborderid;
            command.Parameters.Add("joborderno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderinbox.joborderno == null ? "" : oMfgworkorderinbox.joborderno;
            command.Parameters.Add("jobcardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderinbox.jobcardno == null ? "" : oMfgworkorderinbox.jobcardno;
            command.Parameters.Add("jobname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderinbox.jobname == null ? "" : oMfgworkorderinbox.jobname;
            command.Parameters.Add("jobstatusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderinbox.jobstatusid;
            command.Parameters.Add("workorderstatusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderinbox.workorderstatusid;
            command.Parameters.Add("woexpdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderinbox.woexpdate;
            command.Parameters.Add("joborderdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderinbox.joborderdate;
            command.Parameters.Add("joexpdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderinbox.joexpdate;
            command.Parameters.Add("suppliername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgworkorderinbox.suppliername == null ? "" : oMfgworkorderinbox.suppliername;
            command.Parameters.Add("inqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderinbox.inqty;
            command.Parameters.Add("outqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderinbox.outqty;
            command.Parameters.Add("expqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderinbox.expqty;
            command.Parameters.Add("fabricqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderinbox.fabricqty;
            command.Parameters.Add("wasteqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderinbox.wasteqty;
            command.Parameters.Add("isbomprocess", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkorderinbox.isbomprocess;
            command.Parameters.Add("secondqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderinbox.secondqty;
            command.Parameters.Add("transferedqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgworkorderinbox.transferedqty;

            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.locationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.companyid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgworkorderinbox.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfgworkorderinbox;
        }


        public bool Delete(Mfgworkorderinbox oMfgworkorderinbox)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfgworkorderinbox);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgworkorderinbox);

                throw exp;
            }
            return hasSuccessCount;

        }

        public void Insert(Mfgjoborder mfgjoborder, NpgsqlConnection connection,
            Npgsql.NpgsqlTransaction transaction)
        {
            MfgworkorderDataService mfgworkorderDataService = new MfgworkorderDataService() { Context = this.Context };
            var mfgworkorder = mfgworkorderDataService.Search(new MfgworkorderCreteria()
            {
                id = mfgjoborder.mfgworkorderid
            }).First();

            string suppliername = "";
            if (mfgjoborder.mode.ToLower() == "internal")
            {
                EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
                suppliername = employeeDataService.SearchHeader(new EmployeeCreteria()
                {
                    id = mfgjoborder.receivedby
                }).First().name;
            }
            else
            {
                SupplierDataService supplierDataService = new SupplierDataService() { Context = this.Context };
                suppliername = supplierDataService.Search(new SupplierCreteria()
                {
                    id = mfgjoborder.supplierid
                }).First().name;
            }

            decimal outqty = 0;
            decimal inqty = 0;
            decimal wastage = 0;
            decimal expqty = 0;
            decimal fabricqty = 0;
            if (mfgjoborder.sourcejobid > 0)
            {
                MfgjoborderDataService mfgjoborderDataService = new MfgjoborderDataService() { Context = this.Context };
                var sourcejoborder = mfgjoborderDataService.Search(new MfgjoborderCreteria()
                {
                    id = mfgjoborder.sourcejobid
                }).First();

                var existinginbox = Search(new MfgworkorderinboxCreteria()
                {
                    mfgjoborderid = mfgjoborder.sourcejobid,
                    mfgworkorderid = mfgjoborder.mfgworkorderid
                });

                if (existinginbox.Count > 0)
                {
                    if (mfgjoborder.corestageid == 1)
                    {
                        inqty = mfgjoborder.Mfgjoborderitems.Sum(e => !e.isbom ? e.rqty : 0);
                        outqty = mfgjoborder.Mfgjoborderitems.Sum(e => !e.isbom ? e.rqty : 0);
                        existinginbox.First().inqty += inqty;
                        existinginbox.First().outqty += outqty;
                        Update(transaction, connection, existinginbox.First());
                    }
                    else if (mfgjoborder.corestageid == 2)
                    {
                        inqty = mfgjoborder.Mfgjoborderitems.Sum(e => !e.isbom ? e.rqty : 0);
                        wastage = mfgjoborder.Mfgjoborderitems.Sum(e => !e.isbom ? e.cqty : 0);
                        existinginbox.First().inqty += inqty;
                        existinginbox.First().wasteqty += wastage;
                        Update(transaction, connection, existinginbox.First());
                    }
                    else if (mfgjoborder.corestageid == 3)
                    {
                        inqty = mfgjoborder.Mfgjoborderitems.Sum(e => !e.isbom ? e.rqty : 0);
                        wastage = mfgjoborder.Mfgjoborderitems.Sum(e => !e.isbom ? e.sqty : 0);
                        existinginbox.First().inqty += inqty;
                        existinginbox.First().secondqty += wastage;
                        Update(transaction, connection, existinginbox.First());
                    }
                    else
                    {
                        inqty = mfgjoborder.Mfgjoborderitems.Sum(e => !e.isbom ? e.rqty : 0);
                        existinginbox.First().inqty += inqty;
                        Update(transaction, connection, existinginbox.First());
                    }
                }

                //if (mfgjoborder.corestageid == 1 && mfgjoborder.inwardjob)
                //{
                //    expqty = mfgjoborder.Mfgjoborderitems.Sum(e => !e.isbom ? e.oqty : 0);
                //    inqty = mfgjoborder.Mfgjoborderitems.Sum(e => !e.isbom ? e.rqty : 0);
                //    wastage = mfgjoborder.Mfgjoborderitems.Sum(e => e.isbom && e.isfabric ? e.cqty : 0);
                //    consumed = mfgjoborder.Mfgjoborderitems.Sum(e => e.isbom && e.isfabric ? e.rqty : 0);
                //    var existinginbox = Search(new MfgworkorderinboxCreteria()
                //    {
                //        mfgjoborderid = mfgjoborder.sourcejobid,
                //        mfgworkorderid = mfgjoborder.mfgworkorderid
                //    });
                //    if (existinginbox.First().expqty == 0)
                //    {
                //        existinginbox.First().expqty = expqty;
                //    }
                //    existinginbox.First().inqty += inqty;
                //    existinginbox.First().wasteqty += wastage;
                //    Update(transaction, connection, existinginbox.First());
                //}

            }
            else
            {
                if (mfgjoborder.corestageid == 1 && !mfgjoborder.inwardjob)
                {
                    fabricqty = mfgjoborder.Mfgjoborderitems.Sum(e => e.isbom && e.isfabric ? e.oqty : 0);
                    expqty = mfgworkorder.Mfgworkorderitems.Sum(e => e.outqty);
                }

                if (mfgjoborder.corestageid == 2 && !mfgjoborder.inwardjob)
                {
                    expqty = mfgjoborder.Mfgjoborderitems.Sum(e => !e.isbom ? e.oqty : 0);
                    outqty = mfgjoborder.Mfgjoborderitems.Sum(e => !e.isbom ? e.oqty : 0);
                }

                if (mfgjoborder.corestageid == 3 && !mfgjoborder.inwardjob)
                {
                    expqty = mfgjoborder.Mfgjoborderitems.Sum(e => !e.isbom ? e.oqty : 0);
                    outqty = mfgjoborder.Mfgjoborderitems.Sum(e => !e.isbom ? e.oqty : 0);
                }

                Insert(transaction, connection, new Mfgworkorderinbox()
                {
                    createdby = mfgjoborder.createdby,
                    createdon = mfgjoborder.createdon,
                    woexpdate = mfgworkorder.expectdate,
                    mfgworkorderid = mfgworkorder.id,
                    mfgjoborderid = mfgjoborder.id,
                    locationid = mfgjoborder.fromlocationid,
                    companyid = mfgworkorder.companyid,
                    supplierid = mfgworkorder.supplierid,
                    outqty = outqty,
                    expqty = expqty,
                    inqty = inqty,
                    fabricqty = fabricqty,
                    workorderdate = mfgworkorder.wodate,
                    workorderno = mfgworkorder.workorderno,
                    isactive = true,
                    version = 1,
                    suppliername = suppliername,
                    joborderdate = mfgjoborder.joborderdate,
                    joborderno = mfgjoborder.joborderno,
                    jobcardno = mfgjoborder.jobcardno,
                    joexpdate = mfgjoborder.expectedate,
                    jobname = mfgjoborder.stagename,
                    inboxtype = mfgjoborder.mode.ToLower() == "internal" ? 3 : 4
                });
            }
        }
        public void Insert(Mfgworkorder mfgworkorder, NpgsqlConnection connection, Npgsql.NpgsqlTransaction transaction)
        {
            string suppliername = "";
            if (mfgworkorder.mode.ToLower() == "internal")
            {
                EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
                suppliername = employeeDataService.SearchHeader(new EmployeeCreteria()
                {
                    id = mfgworkorder.receivedby
                }).First().name;
            }
            else
            {
                SupplierDataService supplierDataService = new SupplierDataService() { Context = this.Context };
                suppliername = supplierDataService.Search(new SupplierCreteria()
                {
                    id = mfgworkorder.supplierid
                }).First().name;
            }
            var expqty = mfgworkorder.totalqty;
            var outqty = mfgworkorder.mode.ToLower() == "external" ? mfgworkorder.totalqty : 0;
            var fabricqty = mfgworkorder.fabricqty;
            if (mfgworkorder.onlybom)
            {
                expqty = mfgworkorder.fabricqty;
                fabricqty = mfgworkorder.totalqty;
            }
            Insert(transaction, connection, new Mfgworkorderinbox()
            {
                createdby = mfgworkorder.createdby,
                createdon = mfgworkorder.createdon,
                woexpdate = mfgworkorder.expectdate,
                mfgworkorderid = mfgworkorder.id,
                locationid = mfgworkorder.locationid,
                companyid = mfgworkorder.companyid,
                supplierid = mfgworkorder.supplierid,
                expqty = mfgworkorder.totalqty,
                outqty = mfgworkorder.mode.ToLower() == "external" ? mfgworkorder.totalqty : 0,
                isactive = true,
                version = 1,
                workflowid = mfgworkorder.woflowid,
                isbomprocess = mfgworkorder.onlybom,
                workorderdate = mfgworkorder.wodate,
                workorderno = mfgworkorder.workorderno,
                jobcardno = mfgworkorder.jobcardno,
                suppliername = suppliername,
                fabricqty = mfgworkorder.fabricqty,
                inboxtype = mfgworkorder.mode.ToLower() == "internal" ? 1 : 2
            });
        }
        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgworkorderinbox oMfgworkorderinbox)
        {
            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgworkorderinbox.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgworkorderinbox.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgworkorderinbox.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgworkorderinbox.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }

        internal List<MfgworkorderinboxStatus> UpdateStatus(List<MfgworkorderinboxStatus> items)
        {
            int successCount = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    string query =
                    string.Format("update mfgworkorderinbox set statusid=@statusid where id in ({0})",
                    string.Join(",", items.ConvertAll(x => x.id)));
                    NpgsqlCommand command = new NpgsqlCommand(query, conn);
                    command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = items.First().statusid;
                    successCount = command.ExecuteNonQuery();

                    query =
                    string.Format(@"update mfgworkorder set statusid=@statusid where id in (select mfgworkorderid from mfgworkorderinbox where mfgjoborderid = 0 and id in ({0}))",
                    string.Join(",", items.ConvertAll(x => x.id)));
                    command = new NpgsqlCommand(query, conn);
                    command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = items.First().statusid;
                    successCount = command.ExecuteNonQuery();

                    query =
                    string.Format(@"update mfgjoborder set status=@statusid where id in (select mfgjoborderid from mfgworkorderinbox where mfgjoborderid >0 and id in ({0}))",
                    string.Join(",", items.ConvertAll(x => x.id)));
                    command = new NpgsqlCommand(query, conn);
                    command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = items.First().statusid;
                    successCount = command.ExecuteNonQuery();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
                throw exp;
            }
            return items;
        }
    }

}
