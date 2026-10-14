using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class GatepassDataService : BaseData
    {
        const string SQL_Search = @"select modifiedby, locationid, isactive, version, attachments, actionon, isoutward, packagereferenceid,prddivisionid, packagedate,pono, packagevalue, packageqty, packageuom, createdon, createdby,inchargeid, id, modifiedon, actionby, gatepassno, vehicleno, packagetype, packagecode, city, driver, drivercontactno,vendor,transport,remarks from gatepass where id=@id";

        const string SQL_Insert = @"INSERT INTO public.gatepass(modifiedby, locationid, isactive, version, attachments, actionon, isoutward, packagereferenceid,prddivisionid, packagedate,pono, packagevalue, packageqty, packageuom, createdon, createdby,inchargeid, modifiedon, actionby, gatepassno, vehicleno, packagetype, packagecode, city, driver, drivercontactno,vendor,transport,remarks) VALUES(@modifiedby, @locationid, @isactive, @version, @attachments, @actionon, @isoutward, @packagereferenceid,@prddivisionid, @packagedate,@pono, @packagevalue, @packageqty, @packageuom, @createdon, @createdby,@inchargeid, @modifiedon, @actionby, @gatepassno, @vehicleno, @packagetype, @packagecode, @city, @driver, @drivercontactno,@vendor,@transport,@remarks) RETURNING id";

        const string SQL_Update = @"UPDATE public.gatepass SET modifiedby=@modifiedby, locationid=@locationid, version=@version+1, attachments=@attachments, actionon=@actionon, isoutward=@isoutward, packagereferenceid=@packagereferenceid,prddivisionid=@prddivisionid, packagedate=@packagedate,pono=@pono, packagevalue=@packagevalue, packageqty=@packageqty, packageuom=@packageuom, modifiedon=@modifiedon, actionby=@actionby, gatepassno=@gatepassno, vehicleno=@vehicleno, packagetype=@packagetype, packagecode=@packagecode, city=@city, driver=@driver, drivercontactno=@drivercontactno,vendor=@vendor,transport=@transport,remarks=@remarks where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.gatepass set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Gatepass> Search(GatepassCreteria _gatepass)
        {

            if (_gatepass.movement == "PENDING")
            {
                return SearchPending(_gatepass);
            }
            List<Gatepass> lst = new List<Gatepass>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_gatepass.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _gatepass.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    if (_gatepass.showtoday)
                    {
                        qb.AddParameter("actionon", "fromdate", DateTime.Now.Date, ">=");
                        qb.AddParameter("actionon", "todate", DateTime.Now.Date.AddDays(1), "<");
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(_gatepass.packagetype))
                        {
                            qb.AddParameter("packagetype", _gatepass.packagetype, "=");
                        }
                        if (_gatepass.addedfrom.Year > 2000)
                        {
                            qb.AddParameter("actionon", "fromdate", _gatepass.addedfrom.Date, ">=");
                            qb.AddParameter("actionon", "todate", _gatepass.addedto.AddDays(1), "<");
                        }
                    }

                    if (_gatepass.movement == "INWARD")
                        qb.AddParameter("isoutward", "isoutward", false, "=");

                    if (_gatepass.movement == "OUTWARD")
                        qb.AddParameter("isoutward", "isoutward", true, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Gatepass ogatepass = new Gatepass();
                        ogatepass.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ogatepass.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ogatepass.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ogatepass.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ogatepass.attachments_json = reader["attachments"] == DBNull.Value ? null : reader["attachments"].ToString();
                        ogatepass.actionon = reader["actionon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["actionon"]);
                        ogatepass.isoutward = reader["isoutward"] == DBNull.Value ? false : Convert.ToBoolean(reader["isoutward"]);
                        ogatepass.packagereferenceid = reader["packagereferenceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["packagereferenceid"]);
                        ogatepass.prddivisionid = reader["prddivisionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prddivisionid"]);
                        ogatepass.packagedate = reader["packagedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["packagedate"]);
                        ogatepass.packagevalue = reader["packagevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["packagevalue"]);
                        ogatepass.packageqty = reader["packageqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["packageqty"]);
                        ogatepass.packageuom = reader["packageuom"] == DBNull.Value ? 0 : Convert.ToInt64(reader["packageuom"]);
                        ogatepass.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ogatepass.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ogatepass.inchargeid = reader["inchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["inchargeid"]);
                        ogatepass.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ogatepass.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ogatepass.actionby = reader["actionby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["actionby"]);
                        ogatepass.gatepassno = reader["gatepassno"] == DBNull.Value ? "" : reader["gatepassno"].ToString();
                        ogatepass.vehicleno = reader["vehicleno"] == DBNull.Value ? "" : reader["vehicleno"].ToString();
                        ogatepass.pono = reader["pono"] == DBNull.Value ? "" : reader["pono"].ToString();
                        ogatepass.packagetype = reader["packagetype"] == DBNull.Value ? "" : reader["packagetype"].ToString();

                        ogatepass.transport = reader["transport"] == DBNull.Value ? "" : reader["transport"].ToString();
                        ogatepass.vendor = reader["vendor"] == DBNull.Value ? "" : reader["vendor"].ToString();
                        ogatepass.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();

                        ogatepass.packagecode = reader["packagecode"] == DBNull.Value ? "" : reader["packagecode"].ToString();
                        ogatepass.city = reader["city"] == DBNull.Value ? "" : reader["city"].ToString();
                        ogatepass.driver = reader["driver"] == DBNull.Value ? "" : reader["driver"].ToString();
                        ogatepass.drivercontactno = reader["drivercontactno"] == DBNull.Value ? "" : reader["drivercontactno"].ToString();
                        lst.Add(ogatepass);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _gatepass);
            }
            return lst;
        }

        private List<Gatepass> SearchPending(GatepassCreteria _gatepass)
        {
            List<Gatepass> lst = new List<Gatepass>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_gatepass.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _gatepass.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    const string SQL_SearchPending = @"select  gatepass.modifiedby, gatepass.locationid, gatepass.isactive, gatepass.version, 
                    gatepass.attachments, gatepass.actionon, 
                    gatepass.isoutward, gatepass.packagereferenceid,gatepass.prddivisionid, 
                    gatepass.packagedate,gatepass.pono, gatepass.packagevalue, 
                    gatepass.packageqty, gatepass.packageuom, gatepass.createdon, gatepass.createdby,
                    gatepass.inchargeid, gatepass.id, gatepass.modifiedon, gatepass.actionby, 
                    gatepass.gatepassno, gatepass.vehicleno, gatepass.packagetype, gatepass.packagecode, 
                    gatepass.city, gatepass.driver, gatepass.drivercontactno,
                    gatepass.vendor,gatepass.transport,gatepass.remarks
                    from gatepass
                    left outer join jobworkbill on gatepass.packagetype = 'JOBWORK' and jobworkbill.id = gatepass.packagereferenceid
                    left outer join mfgjoborder on gatepass.packagetype = 'JOBORDER' and mfgjoborder.id = gatepass.packagereferenceid
                    left outer join mfgworkorder on gatepass.packagetype = 'WORKORDER' and mfgworkorder.id = gatepass.packagereferenceid
                    left outer join bombundles on gatepass.packagetype = 'BOMTRANSFER' and bombundles.id = gatepass.packagereferenceid
                    left outer join bundles on gatepass.packagetype = 'GOODSTRANSFER' and bundles.id = gatepass.packagereferenceid
                    left outer join componentpo on gatepass.packagetype = 'BOM' and componentpo.id = gatepass.packagereferenceid
                    left outer join purchaseorder on gatepass.packagetype = 'PURCHASE' and purchaseorder.id = gatepass.packagereferenceid
                    left outer join bill on gatepass.packagetype = 'BILL' and bill.id = gatepass.packagereferenceid
                     where gatepass.isactive=true
                     and ((gatepass.packagetype = 'JOBWORK' and jobworkbill.id is null) or 
                    (gatepass.packagetype = 'JOBORDER' and mfgjoborder.id is null) or
                    (gatepass.packagetype = 'WORKORDER' and mfgworkorder.id is null) or
                    (gatepass.packagetype = 'BOMTRANSFER' and bombundles.id is null) or
                    (gatepass.packagetype = 'GOODSTRANSFER' and bundles.id is null) or
                    (gatepass.packagetype = 'BOM' and componentpo.id is null) or
                    (gatepass.packagetype = 'PURCHASE' and jobworkbill.id is null) or
                    (gatepass.packagetype = 'BILL' and jobworkbill.id is null)) ";
                    qb.CurrentQuery = SQL_SearchPending;
                    if (!string.IsNullOrEmpty(_gatepass.packagetype))
                    {
                        qb.AddParameter("packagetype", _gatepass.packagetype, "=");
                    }
                    if (_gatepass.addedfrom.Year > 2000)
                    {
                        qb.AddParameter("actionon", "fromdate", _gatepass.addedfrom.Date, ">=");
                        qb.AddParameter("actionon", "todate", _gatepass.addedto.AddDays(1), "<");
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Gatepass ogatepass = new Gatepass();
                        ogatepass.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ogatepass.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ogatepass.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ogatepass.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ogatepass.attachments_json = reader["attachments"] == DBNull.Value ? null : reader["attachments"].ToString();
                        ogatepass.actionon = reader["actionon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["actionon"]);
                        ogatepass.isoutward = reader["isoutward"] == DBNull.Value ? false : Convert.ToBoolean(reader["isoutward"]);
                        ogatepass.packagereferenceid = reader["packagereferenceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["packagereferenceid"]);
                        ogatepass.prddivisionid = reader["prddivisionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["prddivisionid"]);
                        ogatepass.packagedate = reader["packagedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["packagedate"]);
                        ogatepass.packagevalue = reader["packagevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["packagevalue"]);
                        ogatepass.packageqty = reader["packageqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["packageqty"]);
                        ogatepass.packageuom = reader["packageuom"] == DBNull.Value ? 0 : Convert.ToInt64(reader["packageuom"]);
                        ogatepass.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ogatepass.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ogatepass.inchargeid = reader["inchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["inchargeid"]);
                        ogatepass.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ogatepass.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ogatepass.actionby = reader["actionby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["actionby"]);
                        ogatepass.gatepassno = reader["gatepassno"] == DBNull.Value ? "" : reader["gatepassno"].ToString();
                        ogatepass.vehicleno = reader["vehicleno"] == DBNull.Value ? "" : reader["vehicleno"].ToString();
                        ogatepass.pono = reader["pono"] == DBNull.Value ? "" : reader["pono"].ToString();
                        ogatepass.packagetype = reader["packagetype"] == DBNull.Value ? "" : reader["packagetype"].ToString();

                        ogatepass.transport = reader["transport"] == DBNull.Value ? "" : reader["transport"].ToString();
                        ogatepass.vendor = reader["vendor"] == DBNull.Value ? "" : reader["vendor"].ToString();
                        ogatepass.remarks = reader["remarks"] == DBNull.Value ? "" : reader["remarks"].ToString();

                        ogatepass.packagecode = reader["packagecode"] == DBNull.Value ? "" : reader["packagecode"].ToString();
                        ogatepass.city = reader["city"] == DBNull.Value ? "" : reader["city"].ToString();
                        ogatepass.driver = reader["driver"] == DBNull.Value ? "" : reader["driver"].ToString();
                        ogatepass.drivercontactno = reader["drivercontactno"] == DBNull.Value ? "" : reader["drivercontactno"].ToString();
                        lst.Add(ogatepass);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _gatepass);
            }
            return lst;
        }

        public Gatepass Insert(Gatepass oGatepass)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("gatepass.gatepassno", oGatepass.locationid);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
                    oGatepass.gatepassno = sequence.GetNextNumber(Context, conn, transaction);
                    oGatepass.createdby = Context.UserID;
                    oGatepass.createdon = DateTimeNow;
                    oGatepass.modifiedon = DateTimeNow;
                    oGatepass.modifiedby = Context.UserID;
                    oGatepass.isactive = true;
                    oGatepass.version = 1;
                    Insert(transaction, conn, oGatepass);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oGatepass);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oGatepass;
        }

        public Gatepass Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Gatepass oGatepass)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.modifiedby;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.locationid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oGatepass.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oGatepass.version;
            command.Parameters.Add("attachments", NpgsqlTypes.NpgsqlDbType.Jsonb).Value = oGatepass.attachments_json == null ? "null" : oGatepass.attachments_json;
            command.Parameters.Add("actionon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oGatepass.actionon;
            command.Parameters.Add("isoutward", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oGatepass.isoutward;
            command.Parameters.Add("packagereferenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.packagereferenceid;
            command.Parameters.Add("prddivisionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.prddivisionid;
            command.Parameters.Add("packagedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oGatepass.packagedate;
            command.Parameters.Add("packagevalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oGatepass.packagevalue;
            command.Parameters.Add("packageqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oGatepass.packageqty;
            command.Parameters.Add("packageuom", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.packageuom;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oGatepass.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.createdby;
            command.Parameters.Add("inchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.inchargeid;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oGatepass.modifiedon;
            command.Parameters.Add("actionby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.actionby;
            command.Parameters.Add("vendor", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.vendor == null ? "" : oGatepass.vendor;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.remarks == null ? "" : oGatepass.remarks;
            command.Parameters.Add("transport", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.transport == null ? "" : oGatepass.transport;
            command.Parameters.Add("gatepassno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.gatepassno == null ? "" : oGatepass.gatepassno;
            command.Parameters.Add("vehicleno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.vehicleno == null ? "" : oGatepass.vehicleno;
            command.Parameters.Add("pono", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.pono == null ? "" : oGatepass.pono;
            command.Parameters.Add("packagetype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.packagetype == null ? "" : oGatepass.packagetype;
            command.Parameters.Add("packagecode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.packagecode == null ? "" : oGatepass.packagecode;
            command.Parameters.Add("city", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.city == null ? "" : oGatepass.city;
            command.Parameters.Add("driver", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.driver == null ? "" : oGatepass.driver;
            command.Parameters.Add("drivercontactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.drivercontactno == null ? "" : oGatepass.drivercontactno;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oGatepass.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oGatepass;
        }

        public Gatepass Update(Gatepass oGatepass)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    oGatepass.modifiedon = DateTimeNow;
                    oGatepass.modifiedby = Context.UserID;
                    Update(transaction, conn, oGatepass);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oGatepass);
                if (transaction != null)
                    transaction.Rollback();
            }

            return oGatepass;
        }

        public Gatepass Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Gatepass oGatepass)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.modifiedby;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.locationid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oGatepass.isactive;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oGatepass.version;
            command.Parameters.Add("attachments", NpgsqlTypes.NpgsqlDbType.Jsonb).Value = oGatepass.attachments_json == null ? "null" : oGatepass.attachments_json;
            command.Parameters.Add("actionon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oGatepass.actionon;
            command.Parameters.Add("isoutward", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oGatepass.isoutward;
            command.Parameters.Add("packagereferenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.packagereferenceid;
            command.Parameters.Add("prddivisionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.prddivisionid;
            command.Parameters.Add("packagedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oGatepass.packagedate;
            command.Parameters.Add("packagevalue", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oGatepass.packagevalue;
            command.Parameters.Add("packageqty", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oGatepass.packageqty;
            command.Parameters.Add("packageuom", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.packageuom;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oGatepass.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.createdby;
            command.Parameters.Add("inchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.inchargeid;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.id;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oGatepass.modifiedon;
            command.Parameters.Add("actionby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.actionby;
            command.Parameters.Add("vendor", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.vendor == null ? "" : oGatepass.vendor;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.remarks == null ? "" : oGatepass.remarks;
            command.Parameters.Add("transport", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.transport == null ? "" : oGatepass.transport;
            command.Parameters.Add("gatepassno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.gatepassno == null ? "" : oGatepass.gatepassno;
            command.Parameters.Add("vehicleno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.vehicleno == null ? "" : oGatepass.vehicleno;
            command.Parameters.Add("pono", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.pono == null ? "" : oGatepass.pono;
            command.Parameters.Add("packagetype", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.packagetype == null ? "" : oGatepass.packagetype;
            command.Parameters.Add("packagecode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.packagecode == null ? "" : oGatepass.packagecode;
            command.Parameters.Add("city", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.city == null ? "" : oGatepass.city;
            command.Parameters.Add("driver", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.driver == null ? "" : oGatepass.driver;
            command.Parameters.Add("drivercontactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oGatepass.drivercontactno == null ? "" : oGatepass.drivercontactno;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oGatepass.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oGatepass;
        }


        public bool Delete(Gatepass oGatepass)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oGatepass);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oGatepass);

                if (transaction != null)
                    transaction.Rollback();
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Gatepass oGatepass)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oGatepass.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oGatepass.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oGatepass.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oGatepass.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal Gatepass Defaults(GatepassCreteria item)
        {
            string jobwork = @"select jobworkbill.id,invoicedate,invoiceno,totalamount amount,
oqty qty,suppliercompany.supplierid,suppliercompany.id suppliercompanyid,suppliercompany.cityid,
suppliercompany.name supplier, ('Job work '||referencelist.name || ' / ' || 
(case when jobworkbill.remarks = '' then 'No Remarks' else jobworkbill.remarks end)) remarks from jobworkbill
inner join suppliercompany on suppliercompany.supplierid = jobworkbill.supplierid
left outer join referencelist on referencelist.id = jobchargeid
where jobwork.isactive=true and jobwork.invoiceno=@packagecode";

            string joborder = @"
select mfgjoborder.id,joborderdate invoicedate, joborderno invoiceno,total amount,
(select sum(oqty) from mfgjoborder
inner join mfgjoborderitems on mfgjoborderitems.mfgjoborderid = mfgjoborder.id
where mfgjoborderitems.isactive=true
where mfgjoborder.isactive=true and mfgjoborder.joborderno=@packagecode) qty,
mfgjoborder.supplierid,suppliercompany.id suppliercompanyid,suppliercompany.cityid,
suppliercompany.name supplier, ('Stage '||stagename) remarks from mfgjoborder
left outer join suppliercompany on suppliercompany.supplierid = mfgjoborder.supplierid
where mfgjoborder.isactive=true and mfgjoborder.joborderno=@packagecode";

            string workorder = @"select mfgworkorder.id,wodate invoicedate, workorderno invoiceno,total amount,
totalqty qty,mfgworkorder.supplierid,suppliercompanyid,suppliercompany.cityid,
case when suppliercompany.id is null then customer.name else suppliercompany.name end supplier,
('Product '||mfgworkorder.product|| ' '||design) remarks from mfgworkorder
left outer join suppliercompany on suppliercompany.supplierid = mfgworkorder.supplierid
left outer join customer on customer.id = customerid
where mfgworkorder.isactive=true and mfgworkorder.workorderno=@packagecode";

            string bombundle = @"select bombundles.id,packedon invoicedate, bombundles.code invoiceno,0 amount,
qty,bombundles.fromlocationid supplierid,bombundles.tolocationid suppliercompanyid,
'' supplier,('From '||fromlocation.name||' To '||tolocation.name || ' ' || notes) remarks,
cast(fromlocation.additionalinfo->'locationconfig'->>'cityid' as bigint) fromcityid,
cast(tolocation.additionalinfo->'locationconfig'->>'cityid' as bigint) tocityid
from bombundles
left outer join referencelist fromlocation on fromlocation.id = bombundles.fromlocationid
left outer join referencelist tolocation on tolocation.id = bombundles.tolocationid
where bombundles.isactive=true and bombundles.code=@packagecode";

            string bundles = @"select bundles.id,packedon invoicedate, bundles.code invoiceno,0 amount,
qty,bundles.fromlocationid supplierid,bundles.tolocationid suppliercompanyid,
'' supplier,'From '||fromlocation.name||' To '||tolocation.name || ' ' || notes remarks,
cast(fromlocation.additionalinfo->'locationconfig'->>'cityid' as bigint) fromcityid,
cast(tolocation.additionalinfo->'locationconfig'->>'cityid' as bigint) tocityid
from bundles
left outer join referencelist fromlocation on fromlocation.id = bundles.fromlocationid
left outer join referencelist tolocation on tolocation.id = bundles.tolocationid
where bundles.isactive=true and bundles.code=@packagecode";

            string componentpo = @"select componentpo.id,podate invoicedate, pono invoiceno,0 amount,
0 qty,suppliercompany.supplierid,suppliercompany.id suppliercompanyid,suppliercompany.cityid,
suppliercompany.name supplier,remarks from componentpo
left outer join suppliercompany on suppliercompany.supplierid = componentpo.supplierid
where componentpo.isactive=true and componentpo.pono =@packagecode";

            string puchaseorder = @"select purchaseorder.id,podate invoicedate, pono invoiceno,0 amount,
0 qty,suppliercompany.supplierid,suppliercompany.id suppliercompanyid,suppliercompany.cityid,
suppliercompany.name supplier,remarks from purchaseorder
left outer join suppliercompany on suppliercompany.supplierid = purchaseorder.supplierid
where purchaseorder.isactive=true and purchaseorder.pono =@packagecode";

            string query = @"select bill.id,billdate invoicedate, billdate invoiceno,receivable amount,
qty,customerid supplierid,0 suppliercompanyid,customer.cityid,referencelist.name cityname,
customer.name supplier,'SO '||billedsalesorderno remarks from bill
left outer join customer  on customer.id = bill.customerid
left outer join referencelist on referencelist.id = cityid
where bill.isactive=true and bill.billno=@packagecode";

            switch (item.packagetype)
            {
                case "JOBWORK":
                    query = jobwork;
                    break;
                case "JOBORDER":
                    query = joborder;
                    break;
                case "WORKORDER":
                    query = workorder;
                    break;
                case "BOMTRANSFER":
                    query = bombundle;
                    break;
                case "GOODSTRANSFER":
                    query = bundles;
                    break;
                case "BOM":
                    query = componentpo;
                    break;
                case "PURCHASE":
                    query = puchaseorder;
                    break;
            }
            Gatepass gatepass = new Gatepass();
            var param = new SortedList<string, object>();
            param.Add("packagecode", item.packagecode);
            using (var reader = ExecuteReader(query, param))
            {
                if (reader.Read())
                {
                    gatepass.packagereferenceid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    gatepass.packagedate = reader["invoicedate"] == DBNull.Value ? DateTime.Now.Date : Convert.ToDateTime(reader["invoicedate"]);
                    gatepass.packagevalue = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                    gatepass.packageqty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    gatepass.vendor = reader["supplier"] == DBNull.Value ? "" : Convert.ToString(reader["supplier"]);
                    gatepass.remarks = reader["remarks"] == DBNull.Value ? "" : Convert.ToString(reader["remarks"]);
                    gatepass.vendorid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                    if (item.packagetype == "BILL")
                    {
                        gatepass.cityid = reader["suppliercompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanyid"]);
                        gatepass.city = reader["cityname"] == DBNull.Value ? "" : Convert.ToString(reader["cityname"]);
                    }
                    if (item.packagetype == "BOMTRANSFER" || item.packagetype == "GOODSTRANSFER")
                    {
                        if (gatepass.isoutward)
                            gatepass.cityid = reader["tocityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["tocityid"]);
                        else
                            gatepass.cityid = reader["fromcityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["fromcityid"]);
                    }
                    else
                    {
                        gatepass.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                    }
                }
            }
            return gatepass;
        }
    }

}
