using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class MfgtailoringDataService : SAASData
    {
        const string SQL_Search = @"select
referencelist.name sectionname,jobcharge.name jobname,
employee.name employeename,jobrate,jobcharge,iqty,rqty,
mfgtailoring.id, mfgtailoring.isactive, mfgtailoring.createdby, mfgtailoring.modifiedby, 
mfgtailoring.version,issuedon,receivedon, mfgtailoring.createdon, mfgtailoring.modifiedon, 
employeeid, jobchargeid, 
        paidcharge, barcode, workorderid, jobworkid, designid, colourid, sleeveid, sizegroupid, 
		cqty, sizelist, tqty,sizeid,sizename,mfgtailoring.remarks,sizegroupname,productid,
        approvedby from mfgtailoring
		left outer join employee on employee.id = employeeid
		left outer join referencelist on referencelist.id = employee.sectionid
		left outer join referencelist jobcharge on  jobcharge.id = jobchargeid
		where id=@id";

        const string SQL_Insert = @"INSERT INTO public.mfgtailoring(isactive, createdby, modifiedby,issuedon,receivedon, version, createdon, modifiedon, employeeid, jobchargeid, jobrate, jobcharge, paidcharge, barcode, workorderid, jobworkid, designid, colourid, sleeveid, sizegroupid, iqty, rqty, cqty, sizelist, tqty,sizeid,sizename,remarks,sizegroupname,productid,approvedby) 
        VALUES(@isactive, @createdby, @modifiedby,@issuedon,@receivedon, @version, @createdon, @modifiedon, @employeeid, @jobchargeid, @jobrate, @jobcharge, @paidcharge, @barcode, @workorderid, @jobworkid, @designid, @colourid, @sleeveid, @sizegroupid, @iqty, @rqty, @cqty, @sizelist, @tqty,@sizeid,@sizename,@remarks,@sizegroupname,@productid,@approvedby) RETURNING id";

        const string SQL_Update = @"UPDATE public.mfgtailoring SET modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, employeeid=@employeeid, jobchargeid=@jobchargeid, jobrate=@jobrate, jobcharge=@jobcharge, paidcharge=@paidcharge, barcode=@barcode, workorderid=@workorderid, jobworkid=@jobworkid, designid=@designid, colourid=@colourid, 
        sleeveid=@sleeveid, sizegroupid=@sizegroupid, iqty=@iqty, rqty=@rqty, cqty=@cqty, sizelist=@sizelist, tqty=@tqty,receivedon=@receivedon,productid=@productid,approvedby=@approvedby where id=@id and version=@version RETURNING version";


        const string SQL_Delete = @"update public.mfgtailoring set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_PrintSlip = @"select mfgtailoring.createdby, issuedon,receivedon, mfgtailoring.createdon, mfgtailoring.modifiedon, employeeid, jobchargeid, jobrate, jobcharge, paidcharge, barcode, workorderid, jobworkid, designid, colourid, sleeveid, sizegroupid, iqty, rqty, cqty, sizelist, tqty,sizeid,sizename,remarks,sizegroupname,colour.name as colourname,
                                       productid,approvedby,employeecode from mfgtailoring 
                                       inner join employee on mfgtailoring.employeeid=employee.id 
                                       left outer join referencelist colour on colour.id = mfgtailoring.colourid where mfgtailoring.id=@id";

        public List<Mfgtailoring> SearchSummary(MfgtailoringCreteria _mfgtailoring)
        {
            List<Mfgtailoring> lst = new List<Mfgtailoring>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;


                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = "select employeeid, sum(iqty - cqty) iqty,sum(rqty) rqty," +
                    "sum(jobcharge) jobcharge, sum(paidcharge) paidcharge from Mfgtailoring";
                qb.AddParameter("isactive", true, "=");

                if (_mfgtailoring.employeeid > 0)
                    qb.AddParameter("employeeid", _mfgtailoring.employeeid, "=");

                if (_mfgtailoring.productid > 0)
                    qb.AddParameter("productid", _mfgtailoring.productid, "=");

                if (_mfgtailoring.designid > 0)
                    qb.AddParameter("designid", _mfgtailoring.designid, "=");

                if (_mfgtailoring.fromdate.Year > 2000 || _mfgtailoring.todate.Year > 2000)
                {
                    if (_mfgtailoring.status == "Receive Detail")
                    {
                        qb.AddParameter("receivedon", "fromdate", _mfgtailoring.fromdate.Date, ">=");
                        qb.AddParameter("receivedon", "todate", _mfgtailoring.todate.Date.AddDays(1), "<");
                    }
                    else
                    {
                        qb.AddParameter("issuedon", "fromdate", _mfgtailoring.fromdate.Date, ">=");
                        qb.AddParameter("issuedon", "todate", _mfgtailoring.todate.Date.AddDays(1), "<");
                    }
                }

                if (_mfgtailoring.showpending || _mfgtailoring.status == "Pending")
                    qb.Add2Query(" (receivedon is null or receivedon = '0001-01-01') and iqty>(rqty+cqty) ");

                if (_mfgtailoring.status == "Paid")
                    qb.Add2Query(" paidcharge>0 and jobcharge>0 ");
                if (_mfgtailoring.status == "Unpaid")
                    qb.Add2Query(" paidcharge<=0 and jobcharge>0 ");

                qb.CurrentQuery += " group by employeeid";

                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgtailoring omfgtailoring = new Mfgtailoring();
                        omfgtailoring.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        omfgtailoring.jobcharge = reader["jobcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jobcharge"]);
                        omfgtailoring.paidcharge = reader["paidcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidcharge"]);
                        omfgtailoring.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["iqty"]);
                        omfgtailoring.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rqty"]);
                        lst.Add(omfgtailoring);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgtailoring);
            }
            return lst;
        }

        public List<Mfgtailoring> Search(MfgtailoringCreteria _mfgtailoring)
        {
            List<Mfgtailoring> lst = new List<Mfgtailoring>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_mfgtailoring.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _mfgtailoring.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("Mfgtailoring.isactive", true, "=");

                    if (_mfgtailoring.employeeid > 0)
                        qb.AddParameter("Mfgtailoring.employeeid", _mfgtailoring.employeeid, "=");
                    if (_mfgtailoring.productid > 0)
                        qb.AddParameter("Mfgtailoring.productid", _mfgtailoring.productid, "=");
                    if (_mfgtailoring.designid > 0)
                        qb.AddParameter("Mfgtailoring.designid", _mfgtailoring.designid, "=");

                    if (_mfgtailoring.fromdate.Year > 2000 || _mfgtailoring.todate.Year > 2000)
                    {
                        if (_mfgtailoring.status == "Receive Detail")
                        {
                            qb.AddParameter("receivedon", "fromdate", _mfgtailoring.fromdate.Date, ">=");
                            qb.AddParameter("receivedon", "todate", _mfgtailoring.todate.Date.AddDays(1), "<");
                        }
                        else
                        {
                            qb.AddParameter("issuedon", "fromdate", _mfgtailoring.fromdate.Date, ">=");
                            qb.AddParameter("issuedon", "todate", _mfgtailoring.todate.Date.AddDays(1), "<");
                        }
                    }

                    if (_mfgtailoring.showpending || _mfgtailoring.status == "Pending")
                        qb.Add2Query(" (receivedon is null or receivedon = '0001-01-01') and iqty>(rqty+cqty) ");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Mfgtailoring omfgtailoring = new Mfgtailoring();
                        omfgtailoring.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgtailoring.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgtailoring.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgtailoring.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgtailoring.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgtailoring.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgtailoring.issuedon = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                        omfgtailoring.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        omfgtailoring.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgtailoring.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        omfgtailoring.jobchargeid = reader["jobchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobchargeid"]);
                        omfgtailoring.jobrate = reader["jobrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jobrate"]);
                        omfgtailoring.jobcharge = reader["jobcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jobcharge"]);
                        omfgtailoring.paidcharge = reader["paidcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidcharge"]);
                        omfgtailoring.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();

                        omfgtailoring.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        omfgtailoring.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        omfgtailoring.sizegroupname = reader["sizegroupname"] == DBNull.Value ? "" : reader["sizegroupname"].ToString();
                        omfgtailoring.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);

                        omfgtailoring.workorderid = reader["workorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["workorderid"]);
                        omfgtailoring.jobworkid = reader["jobworkid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobworkid"]);
                        omfgtailoring.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        omfgtailoring.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgtailoring.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        omfgtailoring.sizegroupid = reader["sizegroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizegroupid"]);
                        omfgtailoring.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["iqty"]);
                        omfgtailoring.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rqty"]);
                        omfgtailoring.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cqty"]);
                        omfgtailoring.sizelist_json = reader["sizelist"] == DBNull.Value ? null : reader["sizelist"].ToString();
                        omfgtailoring.tqty = reader["tqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["tqty"]);
                        omfgtailoring.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        omfgtailoring.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);

                        omfgtailoring.department = reader["sectionname"] == DBNull.Value ? "" : reader["sectionname"].ToString();
                        omfgtailoring.jobname = reader["jobname"] == DBNull.Value ? "" : reader["jobname"].ToString();
                        omfgtailoring.employeename = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();
                        
                        lst.Add(omfgtailoring);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgtailoring);
            }
            return lst;
        }

        public List<Mfgtailoring> PrintSlip(MfgtailoringCreteria _mfgtailoring)
        {
            List<Mfgtailoring> lst = new List<Mfgtailoring>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_PrintSlip.Substring(0, SQL_PrintSlip.IndexOf("where"));
                qb.AddParameter("mfgtailoring.isactive", true, "=");

                if (_mfgtailoring.employeeid > 0)
                    qb.AddParameter("employeeid", _mfgtailoring.employeeid, "=");
                if (_mfgtailoring.designid > 0)

                    qb.AddParameter("designid", _mfgtailoring.designid, "=");
                if (_mfgtailoring.rqty <= 0)
                    qb.AddParameter("rqty", _mfgtailoring.rqty, "=");

                command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MfgtailoringCreteria omfgtailoring = new MfgtailoringCreteria();

                        omfgtailoring.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgtailoring.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgtailoring.issuedon = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                        omfgtailoring.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        omfgtailoring.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgtailoring.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        omfgtailoring.jobchargeid = reader["jobchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobchargeid"]);
                        omfgtailoring.jobrate = reader["jobrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jobrate"]);
                        omfgtailoring.jobcharge = reader["jobcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jobcharge"]);
                        omfgtailoring.paidcharge = reader["paidcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidcharge"]);
                        omfgtailoring.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();
                        omfgtailoring.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        omfgtailoring.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        omfgtailoring.sizegroupname = reader["sizegroupname"] == DBNull.Value ? "" : reader["sizegroupname"].ToString();
                        omfgtailoring.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);
                        omfgtailoring.workorderid = reader["workorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["workorderid"]);
                        omfgtailoring.jobworkid = reader["jobworkid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobworkid"]);
                        omfgtailoring.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        omfgtailoring.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgtailoring.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        omfgtailoring.sizegroupid = reader["sizegroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizegroupid"]);
                        omfgtailoring.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["iqty"]);
                        omfgtailoring.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rqty"]);
                        omfgtailoring.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cqty"]);
                        omfgtailoring.sizelist_json = reader["sizelist"] == DBNull.Value ? null : reader["sizelist"].ToString();
                        omfgtailoring.tqty = reader["tqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["tqty"]);
                        omfgtailoring.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        omfgtailoring.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);
                        omfgtailoring.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        omfgtailoring.colourname = reader["colourname"] == DBNull.Value ? "" : reader["colourname"].ToString();


                        lst.Add(omfgtailoring);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgtailoring);
            }
            return lst;
        }

        internal MfgReceived Received(MfgReceived received)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    received.item.jobcharge = received.item.sizelist.Sum(e => e.jobcharge * e.rqty);
                    Update(transaction, conn, received.item);

                    if (received.recieved != null)
                    {
                        MfgtailoringreceivedDataService mfgtailoringreceivedDataService = new MfgtailoringreceivedDataService() { Context = this.Context };
                        foreach (var item in received.recieved)
                        {
                            mfgtailoringreceivedDataService.Insert(transaction, conn, item);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, received);
                throw exp;
            }
            return received;
        }

        internal MfgPayment Payment(MfgPayment item)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    MfgtailoringwagesDataService mfgtailoringwagesDataService = new MfgtailoringwagesDataService() { Context = this.Context };
                    foreach (var mfgtailoring in item.items)
                    {
                        var paidlist = new List<Mfgtailoring>();
                        var pending = Search(new MfgtailoringCreteria()
                        {
                            employeeid = mfgtailoring.employeeid,
                            status = "Unpaid"
                        });
                        pending.ForEach(e =>
                        {
                            if (e.sizelist != null && e.sizelist.Count > 0)
                            {
                                e.jobcharge = e.sizelist.Sum(x => x.jobcharge * x.rqty);
                            }
                        });
                        var firstset = pending.OrderBy(e => e.iqty + e.cqty == e.rqty).ToList();
                        var available = mfgtailoring.paidamount;
                        if (available <= 0 || pending.Count == 0) continue;
                        Mfgtailoringwages mfgPayment = new Mfgtailoringwages()
                        {
                            fromdate = item.fromdate,
                            todate = item.todate,
                            employeeid = mfgtailoring.employeeid,
                            isactive = true,
                            createdby = Context.UserID,
                            modifiedby = Context.UserID,
                            modifiedon = DateTimeNow,
                            createdon = DateTimeNow,
                            jobcharge = pending.Sum(e => e.jobcharge),
                            paidcharge = available,
                            paidon = item.paidon,
                            qty = pending.Sum(e => e.rqty),
                            version = 1,
                            mfgtailoringids = new List<Mfgtailoringwages.Mfgtailoringids>()
                        };
                        foreach (var pnd in firstset)
                        {
                            if (pnd.jobcharge - pnd.paidcharge <= 0 || available <= 0) continue;
                            if (pnd.jobcharge - pnd.paidcharge >= available)
                            {
                                mfgPayment.mfgtailoringids.Add(new Mfgtailoringwages.Mfgtailoringids()
                                {
                                    id = pnd.id,
                                    amount = available
                                });
                                pnd.paidcharge += available;
                                available = 0;
                            }
                            else
                            {
                                mfgPayment.mfgtailoringids.Add(new Mfgtailoringwages.Mfgtailoringids()
                                {
                                    id = pnd.id,
                                    amount = pnd.jobcharge - pnd.paidcharge
                                });
                                available -= pnd.jobcharge - pnd.paidcharge;
                                pnd.paidcharge += (pnd.jobcharge - pnd.paidcharge);
                            }

                            paidlist.Add(pnd);
                            if (available <= 0) break;
                        }
                        if (available > 0)
                            mfgPayment.paidcharge -= available;

                        mfgtailoringwagesDataService.Insert(transaction, conn, mfgPayment);
                        foreach (var paid in paidlist)
                        {
                            paid.paidon = item.paidon;
                            Update(transaction, conn, paid);
                        }
                    }

                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                throw exp;
            }

            return item;
        }


        public int UpdateCharge()
        {
            int changed = 0;
            var pending = Search(new MfgtailoringCreteria()
            {
                status = "Unpaid"
            });

            MfgdesignDataService mfgdesignDataService = new MfgdesignDataService() { Context = this.Context };
            var designlist = mfgdesignDataService.Search(new MfgdesignCreteria()
            {
            });

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var item in pending)
                    {
                        bool haschaged = false;
                        var design = designlist.Find(e => e.id == item.designid);
                        if (item.sizegroupid > 0)
                        {
                            if (design.sizegroup.items.Exists(e => e.sizegroup == item.sizegroupname))
                            {
                                var sizegroups = design.sizegroup.items.FindAll(e => e.sizegroup == item.sizegroupname);
                                foreach (var size in item.sizelist)
                                {
                                    if (size.jobcharge != sizegroups.Find(e => e.sizeid == size.id).jobcharge)
                                    {
                                        haschaged = true;
                                        size.jobcharge = sizegroups.Find(e => e.sizeid == size.id).jobcharge;
                                    }
                                }
                            }
                        }
                        if (haschaged)
                        {
                            item.jobcharge = item.sizelist.Sum(e => e.jobcharge * e.rqty);
                            Update(transaction, conn, item);
                            changed = +1;
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "Upate Desing Job Charge");
                throw exp;
            }
            return changed;
        }

        public Mfgtailoring Insert(Mfgtailoring oMfgtailoring)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oMfgtailoring);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgtailoring);
                throw exp;
            }

            return oMfgtailoring;
        }

        public Mfgtailoring Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgtailoring oMfgtailoring)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgtailoring.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoring.version;

            command.Parameters.Add("issuedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoring.issuedon;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoring.receivedon;

            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.employeeid;
            command.Parameters.Add("jobchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.jobchargeid;
            command.Parameters.Add("jobrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgtailoring.jobrate;
            command.Parameters.Add("jobcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgtailoring.jobcharge;
            command.Parameters.Add("paidcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgtailoring.paidcharge;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgtailoring.barcode == null ? "" : oMfgtailoring.barcode;

            command.Parameters.Add("sizename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgtailoring.sizename == null ? "" : oMfgtailoring.sizename;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgtailoring.remarks == null ? "" : oMfgtailoring.remarks;
            command.Parameters.Add("sizegroupname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgtailoring.sizegroupname == null ? "" : oMfgtailoring.sizegroupname;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.sizeid;

            command.Parameters.Add("workorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.workorderid;
            command.Parameters.Add("jobworkid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.jobworkid;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.designid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.colourid;
            command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.sleeveid;
            command.Parameters.Add("sizegroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.sizegroupid;
            command.Parameters.Add("iqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoring.iqty;
            command.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoring.rqty;
            command.Parameters.Add("cqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoring.cqty;
            command.Parameters.Add("sizelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgtailoring.sizelist_json == null ? "null" : oMfgtailoring.sizelist_json;
            command.Parameters.Add("tqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoring.tqty;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.productid;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.approvedby;


            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgtailoring.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oMfgtailoring;
        }

        public Mfgtailoring Update(Mfgtailoring oMfgtailoring)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oMfgtailoring);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgtailoring);
                throw exp;
            }

            return oMfgtailoring;
        }

        public Mfgtailoring Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Mfgtailoring oMfgtailoring)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgtailoring.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.modifiedby;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoring.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoring.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoring.modifiedon;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.employeeid;
            command.Parameters.Add("jobchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.jobchargeid;
            command.Parameters.Add("jobrate", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgtailoring.jobrate;
            command.Parameters.Add("jobcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgtailoring.jobcharge;
            command.Parameters.Add("paidcharge", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oMfgtailoring.paidcharge;
            command.Parameters.Add("issuedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoring.issuedon;
            command.Parameters.Add("receivedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoring.receivedon;
            command.Parameters.Add("sizename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgtailoring.sizename == null ? "" : oMfgtailoring.sizename;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgtailoring.remarks == null ? "" : oMfgtailoring.remarks;
            command.Parameters.Add("sizegroupname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgtailoring.sizegroupname == null ? "" : oMfgtailoring.sizegroupname;
            command.Parameters.Add("sizeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.sizeid;
            command.Parameters.Add("barcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oMfgtailoring.barcode == null ? "" : oMfgtailoring.barcode;
            command.Parameters.Add("workorderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.workorderid;
            command.Parameters.Add("jobworkid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.jobworkid;
            command.Parameters.Add("designid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.designid;
            command.Parameters.Add("colourid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.colourid;
            command.Parameters.Add("sleeveid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.sleeveid;
            command.Parameters.Add("sizegroupid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.sizegroupid;
            command.Parameters.Add("iqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoring.iqty;
            command.Parameters.Add("rqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoring.rqty;
            command.Parameters.Add("cqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoring.cqty;
            command.Parameters.Add("sizelist", NpgsqlTypes.NpgsqlDbType.Json).Value = oMfgtailoring.sizelist_json == null ? "null" : oMfgtailoring.sizelist_json;
            command.Parameters.Add("tqty", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoring.tqty;
            command.Parameters.Add("productid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.productid;
            command.Parameters.Add("approvedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.approvedby;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oMfgtailoring.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oMfgtailoring;
        }


        public bool Delete(MfgtailoringCreteria oMfgtailoring)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oMfgtailoring);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oMfgtailoring);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, MfgtailoringCreteria oMfgtailoring)
        {
            if (oMfgtailoring.id > 0)
            {
                NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
                command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgtailoring.isactive;
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.modifiedby;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoring.modifiedon;
                command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoring.version;
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.id;
                int successCount = command.ExecuteNonQuery();
                return successCount > 0;
            }
            else if (oMfgtailoring.ids != null && oMfgtailoring.ids.Count > 0)
            {
                NpgsqlCommand command = new NpgsqlCommand("update Mfgtailoring set isactive=false where id in (" + string.Join(",", oMfgtailoring.ids) + ")", conn, transaction);
                command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oMfgtailoring.isactive;
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.modifiedby;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oMfgtailoring.modifiedon;
                command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oMfgtailoring.version;
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oMfgtailoring.id;
                int successCount = command.ExecuteNonQuery();
                return successCount > 0;
            }
            return false;
        }

        internal List<MfgtailoringDetail> Report(MfgtailoringCreteria _mfgtailoring)
        {
            List<MfgtailoringDetail> lst = new List<MfgtailoringDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                qb.AddParameter("Mfgtailoring.isactive", true, "=");

                if (_mfgtailoring.employeeid > 0)
                    qb.AddParameter("Mfgtailoring.employeeid", _mfgtailoring.employeeid, "=");
                if (_mfgtailoring.productid > 0)
                    qb.AddParameter("Mfgtailoring.productid", _mfgtailoring.productid, "=");
                if (_mfgtailoring.designid > 0)
                    qb.AddParameter("Mfgtailoring.designid", _mfgtailoring.designid, "=");

                if (_mfgtailoring.fromdate.Year > 2000 || _mfgtailoring.todate.Year > 2000)
                {
                    qb.AddParameter("issuedon", "fromdate", _mfgtailoring.fromdate.Date, ">=");
                    qb.AddParameter("issuedon", "todate", _mfgtailoring.todate.Date.AddDays(1), "<");
                }
                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        MfgtailoringDetail omfgtailoring = new MfgtailoringDetail();
                        omfgtailoring.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        omfgtailoring.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        omfgtailoring.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        omfgtailoring.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        omfgtailoring.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        omfgtailoring.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        omfgtailoring.issuedon = reader["issuedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["issuedon"]);
                        omfgtailoring.receivedon = reader["receivedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receivedon"]);
                        omfgtailoring.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        omfgtailoring.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        omfgtailoring.jobchargeid = reader["jobchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobchargeid"]);
                        omfgtailoring.jobrate = reader["jobrate"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jobrate"]);
                        omfgtailoring.jobcharge = reader["jobcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["jobcharge"]);
                        omfgtailoring.paidcharge = reader["paidcharge"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paidcharge"]);
                        omfgtailoring.barcode = reader["barcode"] == DBNull.Value ? "" : reader["barcode"].ToString();

                        omfgtailoring.sizename = reader["sizename"] == DBNull.Value ? "" : reader["sizename"].ToString();
                        omfgtailoring.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();
                        omfgtailoring.sizegroupname = reader["sizegroupname"] == DBNull.Value ? "" : reader["sizegroupname"].ToString();
                        omfgtailoring.sizeid = reader["sizeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizeid"]);

                        omfgtailoring.workorderid = reader["workorderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["workorderid"]);
                        omfgtailoring.jobworkid = reader["jobworkid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["jobworkid"]);
                        omfgtailoring.designid = reader["designid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designid"]);
                        omfgtailoring.colourid = reader["colourid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["colourid"]);
                        omfgtailoring.sleeveid = reader["sleeveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sleeveid"]);
                        omfgtailoring.sizegroupid = reader["sizegroupid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sizegroupid"]);
                        omfgtailoring.iqty = reader["iqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["iqty"]);
                        omfgtailoring.rqty = reader["rqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["rqty"]);
                        omfgtailoring.cqty = reader["cqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["cqty"]);
                        omfgtailoring.sizelist_json = reader["sizelist"] == DBNull.Value ? null : reader["sizelist"].ToString();
                        omfgtailoring.tqty = reader["tqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["tqty"]);
                        omfgtailoring.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        omfgtailoring.approvedby = reader["approvedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["approvedby"]);

                        lst.Add(omfgtailoring);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _mfgtailoring);
            }
            return lst;
        }
    }

}
