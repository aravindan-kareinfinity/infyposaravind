using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class InboxDataService : SAASData
    {
        const string SQL_Search = @"select typeid, typedisplay, createdusername, inbox.referenceid, referenceno, addedon, inbox.supplierid, inbox.suppliername,
	    invoices, statusid, statusname, inbox.createdon, lrentry.transportid, lrentry.noofbundels, lrentry.fromlocationid cityid, lrentry.sourcesupplierid,
        inbox.modifiedon, inbox.createdby, inbox.modifiedby, inbox.isactive, hasclosed, closedby, closeduser, closedon, numbers, notes, inbox.version, inbox.id,
        parentid,referencedate,inbox.modeofdelivery,lrentry.invoiceno,invoicedate,lrentry.lrentryno,lrentry.receiveddate, lrentry.lrdate, lrentry.lrentrydate, 
        noofpieces,lrentry.totalamount,lrentry.lrcompanyid,inbox.lrcompanyid inboxcompanyid, lrentry.entrylocationid, employee.name username,sections.name section,
        lrinvoicereturn.locationid returnlocationid, lrinvoicereturn.qty returnqty, suppliercompany.cityid suppliercompanycityid,slipno, lrentry.agentid,
        coresupplier.msmeno from inbox
        left outer join lrentry on lrentry.id = inbox.referenceid and inbox.typeid = 1
        left outer join lrinvoicereturn on lrinvoicereturn.id = inbox.referenceid and inbox.typeid = 7
		left outer join suppliercompany on suppliercompany.id = lrinvoicereturn.suppliercompanyid
        left outer join suppliercompany coresupplier on coresupplier.supplierid = lrentry.supplierid
        left outer join users on users.id = lrentry.createdby
        left outer join employee on employee.id = users.employeeid
        left outer join referencelist sections on sections.id = lrentry.lrsectionid
        where inbox.id=@id";


        const string SQL_Insert = @"INSERT INTO public.inbox(typeid, typedisplay, createdusername, referenceid, referenceno, addedon, supplierid, suppliername, invoices, statusid, statusname, createdon, modifiedon, createdby, modifiedby, isactive, hasclosed, closedby, closeduser, closedon, numbers, notes, version,parentid,referencedate,modeofdelivery, lrcompanyid,tzoffset) 
        VALUES (@typeid, @typedisplay, @createdusername, @referenceid, @referenceno, @addedon, @supplierid, @suppliername, @invoices, @statusid, @statusname, @createdon, @modifiedon, @createdby, @modifiedby, @isactive, @hasclosed, @closedby, @closeduser, @closedon, @numbers, @notes, @version,@parentid,@referencedate,@modeofdelivery,@lrcompanyid,@tzoffset) RETURNING id";

        const string SQL_Update = @"UPDATE public.inbox SET typeid=@typeid, typedisplay=@typedisplay, createdusername=@createdusername, referenceid=@referenceid, referenceno=@referenceno, addedon=@addedon, supplierid=@supplierid, suppliername=@suppliername,modeofdelivery=@modeofdelivery, invoices=@invoices, statusid=@statusid, statusname=@statusname, modifiedon=@modifiedon, 
                        modifiedby=@modifiedby, hasclosed=@hasclosed, closedby=@closedby, closeduser=@closeduser, closedon=@closedon, numbers=@numbers,
                        notes=@notes, version=@version+1,referencedate=@referencedate,lrcompanyid=@lrcompanyid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.inbox set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        public List<Inbox> Search(InboxCreteria _inbox)
        {
            List<Inbox> lst = new List<Inbox>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                lst = Search(_inbox, conn,null);
            }
            finally
            {
                if (conn.State == System.Data.ConnectionState.Open)
                    conn.Close();
            }
            if (Context.accessiblecompany != null && Context.accessiblecompany.Count > 0)
            {
                if (!Context.accessiblecompany.Exists(e => e == -1))
                {
                    lst.RemoveAll(e => !Context.accessiblecompany.Exists(x => x == e.lrcompanyid));
                }
            }
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_showroompurchase && 
                Context.accessiblelocation != null && Context.accessiblelocation.Count > 0)
            {
                if (!Context.accessiblelocation.Exists(e => e == -1))
                {
                    lst.RemoveAll(e => !Context.accessiblelocation.Exists(x => x == e.entrylocationid));
                }
            }
            return lst;
        }

        public List<Inbox> Search(InboxCreteria _inbox, NpgsqlConnection conn,NpgsqlTransaction transaction)
        {
            List<Inbox> lst = new List<Inbox>();



            NpgsqlCommand command = null;
            if (_inbox.id != 0)
            {
                if(transaction == null)
                    command = GetCommand(SQL_Search, conn);
                else
                    command = GetCommand(SQL_Search, conn,transaction);
            }
            else
            {
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                qb.AddParameter("inbox.isactive", true, "=");
                if (_inbox.showallopen)
                {
                    qb.AddParameter("inbox.hasclosed", false, "=");
                    if (_inbox.currentuserid > 0)
                        qb.AddParameter("inbox.createdby", _inbox.currentuserid, "=");
                }
                else
                {
                    if (transaction == null)
                    {
                        if (_inbox.statusid != 0)
                        {
                            if(_inbox.statusid == 250)
                                qb.AddInParamer("inbox.statusid", "200,201,202,203,204");
                            else
                                qb.AddParameter("inbox.statusid", _inbox.statusid, "=");
                        }
                        else
                            qb.AddParameter("inbox.hasclosed", false, "=");

                        if (_inbox.supplierid > 0)
                            qb.AddParameter("inbox.supplierid", _inbox.supplierid, "=");
                    }

                    if (_inbox.referenceid > 0)
                        qb.AddParameter("inbox.referenceid", _inbox.referenceid, "=");

                    if (_inbox.typeid == 1)
                    {
                        qb.AddParameter("inbox.typeid", _inbox.typeid, "=");
                        //qb.AddParameter("inbox.modeofdelivery", 1, "=");
                    }
                    else if (_inbox.typeid == 7)
                    {
                        qb.AddParameter("inbox.typeid", 1, "=");
                        qb.AddParameter("inbox.modeofdelivery", 2, "=");
                    }
                    else if (_inbox.typeid == 8)
                    {
                        qb.AddParameter("inbox.typeid", 1, "=");
                        qb.AddParameter("inbox.modeofdelivery", 1, "=");
                    }
                    else if (_inbox.typeid == 9)
                    {
                        qb.AddParameter("inbox.typeid", 7, "=");
                    }
                    else if (_inbox.typeid > 0 && _inbox.typeid != 6)
                        qb.AddParameter("inbox.typeid", _inbox.typeid, "=");
                    else
                        qb.AddInParamer("inbox.typeid", "1,3");

                    if (_inbox.addedfrom.Year > 1900)
                        qb.AddParameter("inbox.addedon", "addedfrom", _inbox.addedfrom.Date, ">=");
                    if (_inbox.addedto.Year > 1900)
                        qb.AddParameter("inbox.addedon", "addedto", _inbox.addedto.Date.AddDays(1), "<");
                    if (_inbox.typeid == 6)
                        qb.AddParameter("inbox.hasclosed", false, "=");
                }
                qb.SortField = "inbox.modifiedon desc";
                qb.Limit = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_whinbox_length > 0 ? InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.system_whinbox_length : 1000;
                command = qb.GetCommand(conn,transaction);
            }
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _inbox.id;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Inbox oinbox = new Inbox();
                    oinbox.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["typeid"]);
                    oinbox.modeofdelivery = reader["modeofdelivery"] == DBNull.Value ? 0 : Convert.ToInt32(reader["modeofdelivery"]);
                    oinbox.msmeno = reader["msmeno"] == DBNull.Value ? "" : reader["msmeno"].ToString();
                    oinbox.typedisplay = reader["typedisplay"] == DBNull.Value ? "" : reader["typedisplay"].ToString();
                    oinbox.slipno = reader["slipno"] == DBNull.Value ? "" : reader["slipno"].ToString();
                    oinbox.createdusername = reader["username"] == DBNull.Value ? "" : reader["username"].ToString();
                    oinbox.referenceid = reader["referenceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["referenceid"]);
                    oinbox.referenceno = reader["referenceno"] == DBNull.Value ? "" : reader["referenceno"].ToString();
                    oinbox.addedon = reader["addedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["addedon"]);
                    oinbox.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                    oinbox.sourcesupplierid = reader["sourcesupplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sourcesupplierid"]);
                    if (oinbox.supplierid == oinbox.sourcesupplierid)
                        oinbox.sourcesupplierid = 0;
                    oinbox.agentid = reader["agentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["agentid"]);
                    oinbox.suppliername = reader["suppliername"] == DBNull.Value ? "" : reader["suppliername"].ToString();
                    oinbox.invoices = reader["invoices"] == DBNull.Value ? 0 : Convert.ToInt32(reader["invoices"]);
                    oinbox.statusid = reader["statusid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["statusid"]);
                    oinbox.statusname = reader["statusname"] == DBNull.Value ? "" : reader["statusname"].ToString();
                    oinbox.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                    oinbox.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                    oinbox.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                    oinbox.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                    oinbox.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                    oinbox.hasclosed = reader["hasclosed"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasclosed"]);
                    oinbox.closedby = reader["closedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["closedby"]);
                    oinbox.closeduser = reader["closeduser"] == DBNull.Value ? "" : reader["closeduser"].ToString();
                    oinbox.closedon = reader["closedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["closedon"]);
                    oinbox.numbers = reader["numbers"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["numbers"]);
                    oinbox.notes = reader["notes"] == DBNull.Value ? "" : reader["notes"].ToString();
                    oinbox.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                    oinbox.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    oinbox.parentid = reader["parentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["parentid"]);
                    oinbox.referencedate = reader["referencedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["referencedate"]);
                    oinbox.noofbundels = reader["noofbundels"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofbundels"]);
                    oinbox.invoiceno = reader["invoiceno"] == DBNull.Value ? "" : reader["invoiceno"].ToString();
                    oinbox.invoicedate = reader["invoicedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["invoicedate"]);
                    oinbox.lrentryno = reader["lrentryno"] == DBNull.Value ? "" : reader["lrentryno"].ToString();
                    oinbox.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"]);
                    oinbox.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                    oinbox.lrentrydetail = oinbox.referenceno;
                    if (oinbox.invoicedate == DateTime.MinValue)
                        oinbox.invoicenodetail = oinbox.invoiceno;
                    else
                        oinbox.invoicenodetail = oinbox.invoiceno + " - " + oinbox.invoicedate.ToString("dd/MM/yyyy");
                    oinbox.noofpieces = reader["noofpieces"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofpieces"]);
                    oinbox.totalamount = reader["totalamount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["totalamount"]);
                    oinbox.receiveddate = reader["receiveddate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["receiveddate"]);
                    oinbox.lrdate = reader["lrdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrdate"]);
                    oinbox.lrentrydate = reader["lrentrydate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["lrentrydate"]);
                    oinbox.lrcompanyid = reader["lrcompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["lrcompanyid"]);
                    oinbox.entrylocationid = reader["entrylocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["entrylocationid"]);
                    oinbox.section = reader["section"] == DBNull.Value ? "" : reader["section"].ToString();

                    if (oinbox.invoicedate != DateTime.MinValue)
                        oinbox.age = DateTimeNow.Subtract(oinbox.lrentrydate).Days+"/"+DateTimeNow.Subtract(oinbox.invoicedate).Days;

                    if (oinbox.lrcompanyid == 0)
                        oinbox.lrcompanyid = reader["inboxcompanyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["inboxcompanyid"]);
                    if (oinbox.typeid == 7)
                    {
                        oinbox.entrylocationid = reader["returnlocationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["returnlocationid"]);
                        oinbox.noofpieces = reader["returnqty"] == DBNull.Value ? 0 : Convert.ToInt32(reader["returnqty"]);
                        oinbox.cityid = reader["suppliercompanycityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["suppliercompanycityid"]);
                        oinbox.lrentryno = oinbox.referenceno;
                    }
                    lst.Add(oinbox);
                }
            }


            return lst;
        }

        public Inbox Insert(Inbox oInbox)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oInbox,Context);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oInbox);


            }

            return oInbox;
        }

        public string GetSupplierName(NpgsqlConnection conn, long[] ids)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            NpgsqlCommand scommand = GetCommand("select name from supplier where id in ("+ string.Join<long>(",",ids) +")",conn);
            using (System.Data.IDataReader reader = scommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (sb.Length > 0) sb.Append(",");
                    sb.Append(reader[0].ToString());
                }
            }
            if (sb.Length > 249)
                sb.Remove(250, sb.Length - 250);
            return sb.ToString();
        }

        public Inbox Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Inbox oInbox,UserClaim Context)
        {
            if (oInbox.supplierid != 0 && string.IsNullOrEmpty(oInbox.suppliername))
            {
                oInbox.suppliername = GetSupplierName(conn, new long[] { oInbox.supplierid });
            }

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oInbox.typeid;
            command.Parameters.Add("modeofdelivery", NpgsqlTypes.NpgsqlDbType.Integer).Value = oInbox.modeofdelivery;
            command.Parameters.Add("typedisplay", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oInbox.typedisplay == null ? "" : oInbox.typedisplay;
            command.Parameters.Add("createdusername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oInbox.createdusername == null ? "" : oInbox.createdusername;
            command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oInbox.referenceid;
            command.Parameters.Add("referenceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oInbox.referenceno == null ? "" : oInbox.referenceno;
            command.Parameters.Add("addedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oInbox.addedon;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oInbox.supplierid;
            command.Parameters.Add("suppliername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oInbox.suppliername == null ? "" : oInbox.suppliername;
            command.Parameters.Add("invoices", NpgsqlTypes.NpgsqlDbType.Integer).Value = oInbox.invoices;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oInbox.statusid;
            command.Parameters.Add("tzoffset", NpgsqlTypes.NpgsqlDbType.Integer).Value = Context.UTCOffset;
            command.Parameters.Add("statusname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oInbox.statusname == null ? "" : oInbox.statusname;
            command.Parameters.Add("referencedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oInbox.referencedate;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oInbox.isactive;
            command.Parameters.Add("hasclosed", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oInbox.hasclosed;
            command.Parameters.Add("closedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oInbox.closedby;
            command.Parameters.Add("closeduser", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oInbox.closeduser == null ? "" : oInbox.closeduser;
            command.Parameters.Add("closedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oInbox.closedon;
            command.Parameters.Add("numbers", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oInbox.numbers;
            command.Parameters.Add("notes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oInbox.notes == null ? "" : oInbox.notes;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oInbox.version;
            command.Parameters.Add("parentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oInbox.parentid;
            command.Parameters.Add("lrcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oInbox.lrcompanyid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oInbox.id = Convert.ToInt64(reader[0]);
                }
            }
            return oInbox;
        }

        public Inbox Update(Inbox oInbox)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oInbox);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oInbox);


            }

            return oInbox;
        }

        public void UpdateLRStatus(NpgsqlTransaction transaction, NpgsqlConnection conn, long lrid, Inbox.StatusList status)
        {
            NpgsqlCommand command = GetCommand("Update lrentry set status=@status,hasclosed=@hasclosed where typeid=@typeid and referenceid=@lrid", conn, transaction);
            command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)Models.Inbox.Types.LREntry;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)status;
            command.Parameters.Add("hasclosed", NpgsqlTypes.NpgsqlDbType.Boolean).Value = status == Inbox.StatusList.Cancel || status == Inbox.StatusList.Completed || status == Inbox.StatusList.Sent;
            command.Parameters.Add("lrid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = lrid;
            command.ExecuteNonQuery();
        }

        public Inbox Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Inbox oInbox)
        {
            long _supplierid = 0;
            string sql = @"select statusid, supplierid from inbox where typeid={0} and referenceid={1}";
            NpgsqlCommand scommand = GetCommand(string.Format(sql, oInbox.typeid, oInbox.referenceid), conn);
            using (System.Data.IDataReader reader = scommand.ExecuteReader())
            {
                while (reader.Read())
                {
                    if (!reader.IsDBNull(0) && Convert.ToInt32(reader[0].ToString()) == 2)
                    {
                        oInbox.statusid = 2;
                    }
                    _supplierid = Convert.ToInt64(reader[1].ToString());
                }
            }
            oInbox.supplierid = oInbox.supplierid == 0 ? _supplierid : oInbox.supplierid;
            oInbox.suppliername = oInbox.supplierid == _supplierid ? oInbox.suppliername : "";
            if (oInbox.supplierid != 0 && string.IsNullOrEmpty(oInbox.suppliername))
            {
                oInbox.suppliername = GetSupplierName(conn, new long[] { oInbox.supplierid });
            }

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oInbox.typeid;
            command.Parameters.Add("typedisplay", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oInbox.typedisplay == null ? "" : oInbox.typedisplay;
            command.Parameters.Add("createdusername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oInbox.createdusername == null ? "" : oInbox.createdusername;
            command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oInbox.referenceid;
            command.Parameters.Add("referenceno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oInbox.referenceno == null ? "" : oInbox.referenceno;
            command.Parameters.Add("addedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oInbox.addedon;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oInbox.supplierid;
            command.Parameters.Add("suppliername", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oInbox.suppliername == null ? "" : oInbox.suppliername;
            command.Parameters.Add("invoices", NpgsqlTypes.NpgsqlDbType.Integer).Value = oInbox.invoices;
            command.Parameters.Add("statusid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oInbox.statusid;
            command.Parameters.Add("statusname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oInbox.statusname == null ? "" : oInbox.statusname;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oInbox.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oInbox.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oInbox.isactive;
            command.Parameters.Add("hasclosed", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oInbox.hasclosed;
            command.Parameters.Add("closedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oInbox.closedby;
            command.Parameters.Add("modeofdelivery", NpgsqlTypes.NpgsqlDbType.Integer).Value = oInbox.modeofdelivery;
            command.Parameters.Add("closeduser", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oInbox.closeduser == null ? "" : oInbox.closeduser;
            command.Parameters.Add("closedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oInbox.closedon;
            command.Parameters.Add("numbers", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oInbox.numbers;
            command.Parameters.Add("notes", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oInbox.notes == null ? "" : oInbox.notes;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oInbox.version;
            command.Parameters.Add("referencedate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oInbox.referencedate;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oInbox.id;
            command.Parameters.Add("lrcompanyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oInbox.lrcompanyid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oInbox.version = Convert.ToInt32(reader[0]);
                }
            }

            return oInbox;
        }


        public bool Delete(Inbox oInbox)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oInbox);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oInbox);



            }
            return hasSuccessCount;
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Inbox oInbox)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oInbox.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oInbox.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oInbox.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
