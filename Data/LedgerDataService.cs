using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using InfyPOS.GST;

namespace InfyPOS.Data
{
    public class LedgerDataService : SAASData
    {
        const string SQL_Search = @"select ledger.id, displayname, typeid, ledger.isactive, ledger.createdby, ledger.modifiedby, ledger.version, 
                                    ledger.createdon,opening,closing, ledger.modifiedon, refid, contactperson, muser.employeeid memployeeid,
                                    contactnumber, contactaddress, emailid,parentgroup,parentid,parentinfo,cityid,stateid,taxmode,
                                    taxpercentage,billwise,bankid,customerid,cuser.employeeid cemployeeid,transportid,tagid,
                                    supplierid,hsncode,pincode,billingname,gstno,isinterstate,attributes,eodcashledger 
                                    from ledger
                                    left outer join users cuser on cuser.id = ledger.createdby 
                                    left outer join users muser on muser.id = ledger.modifiedby
                                    where ledger.id=@id";

        const string SQL_Insert = @"INSERT INTO public.ledger(displayname, typeid, isactive, createdby, modifiedby, version, createdon, modifiedon, refid, contactperson, contactnumber, contactaddress, emailid,opening,closing,parentgroup,parentid,parentinfo,cityid,stateid,taxmode,taxpercentage,billwise,bankid,customerid,supplierid,transportid,tagid,hsncode,pincode,billingname,gstno,isinterstate,attributes,eodcashledger,activestatus)
                                    VALUES(@displayname, @typeid, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @refid, @contactperson, @contactnumber, @contactaddress, @emailid,@opening,@closing,@parentgroup,@parentid,@parentinfo,@cityid,@stateid,@taxmode,@taxpercentage,@billwise,@bankid,@customerid,@supplierid,@transportid,@tagid,@hsncode,@pincode,@billingname,@gstno,@isinterstate,@attributes,@eodcashledger,@activestatus) RETURNING id";


        const string SQL_Update = @"UPDATE public.ledger SET displayname=@displayname, typeid=@typeid,opening=@opening,closing=@closing, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, refid=@refid, parentid=@parentid,
                                    contactperson=@contactperson,parentinfo=@parentinfo,cityid=@cityid,stateid=@stateid,taxmode=@taxmode,taxpercentage=@taxpercentage,billwise=@billwise,isinterstate=@isinterstate,attributes=@attributes,tagid=@tagid,
                                    contactnumber=@contactnumber, parentgroup=@parentgroup, bankid=@bankid,customerid=@customerid,supplierid=@supplierid,transportid=@transportid,hsncode=@hsncode,pincode=@pincode,gstno=@gstno,billingname=@billingname,
                                    contactaddress=@contactaddress, emailid=@emailid,eodcashledger=@eodcashledger where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.ledger set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        const string SQL_TDSRules = @"select ledger.id,schemelist->>'mode' mode,schemelist->>'eligible' eligible,schemelist->>'value' value from ledger 
LEFT JOIN LATERAL
json_array_elements (attributes->'tdsrules') schemelist ON true
where isactive=true and typeid = 16 and taxmode=6 and attributes->'tdsrules' is not null";

        public List<LedgerHeader> LedgerHeaders()
        {
            List<LedgerHeader> lst = new List<LedgerHeader>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = GetCommand("Select id,displayname,typeid from ledger where isactive=true order by displayname", conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        LedgerHeader oledger = new LedgerHeader();
                        oledger.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oledger.displayname = reader["displayname"] == DBNull.Value ? "" : reader["displayname"].ToString();
                        oledger.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["typeid"]);
                        lst.Add(oledger);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "TDSLedger Rules");
            }

            return lst;
        }

        public List<Ledger.LedgerTDSInfo> TDSLedgerRules()
        {
            List<Ledger.LedgerTDSInfo> lst = new List<Ledger.LedgerTDSInfo>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(SQL_TDSRules, conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Ledger.LedgerTDSInfo oledger = new Ledger.LedgerTDSInfo();
                        oledger.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oledger.mode = reader["mode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["mode"]);
                        oledger.eligible = reader["eligible"] == DBNull.Value ? 0 : Convert.ToInt32(reader["eligible"]);
                        oledger.value = reader["value"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["value"]);
                        lst.Add(oledger);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "TDSLedger Rules");
            }

            return lst;
        }

        public List<LedgerDetail> Search(LedgerCreteria _ledger)
        {
            List<LedgerDetail> lst = new List<LedgerDetail>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                if (_ledger.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _ledger.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("ledger.isactive", true, "=");

                    if(_ledger.tagid!=0)
                    {
                        qb.AddParameter("ledger.tagid", _ledger.tagid, "=");
                    }
                    if (_ledger.typeids != null && _ledger.typeids.Count > 0)
                    {
                        qb.AddInParamer("typeid", string.Join(",", _ledger.typeids));
                    }

                    if (_ledger.ids != null && _ledger.ids.Count > 0)
                        qb.AddInParamer("ledger.id", string.Join(",", _ledger.ids));

                    switch (_ledger.filterid)
                    {
                        case LedgerCreteria.Filter.CGSTLedgers:
                            qb.Add2Query("ledger.typeid=16 and taxmode=5");
                            break;
                        case LedgerCreteria.Filter.SGSTLedgers:
                            qb.Add2Query("ledger.typeid=16 and taxmode=4");
                            break;
                        case LedgerCreteria.Filter.IGSTLedgers:
                            qb.Add2Query("ledger.typeid=16 and taxmode=3");
                            break;
                        case LedgerCreteria.Filter.GSTLedgers:
                            qb.Add2Query("ledger.typeid=16 and taxmode>=3 and taxmode<=5");
                            break;
                        case LedgerCreteria.Filter.TDSLedgers:
                            qb.Add2Query("ledger.typeid=16 and taxmode=6");
                            break;
                        case LedgerCreteria.Filter.TCSLedgers:
                            qb.Add2Query("ledger.typeid=16 and taxmode=7");
                            break;
                        case LedgerCreteria.Filter.CESSLedgers:
                            qb.Add2Query("ledger.typeid=16 and taxmode=2");
                            break;
                        case LedgerCreteria.Filter.BillwiseEnabled:
                            qb.Add2Query("ledger.billwise = true");
                            break;
                        case LedgerCreteria.Filter.BillwiseDisabled:
                            qb.Add2Query("ledger.billwise = false");
                            break;
                        case LedgerCreteria.Filter.GSTEnabled:
                            qb.Add2Query("cast(ledger.attributes->>'hasgst' as bool) = false");
                            break;
                        case LedgerCreteria.Filter.GSTNoMissing:
                            qb.Add2Query("(Ledger.gstno is null or Ledger.gstno = '')");
                            break;
                        case LedgerCreteria.Filter.PANMissing:
                            qb.Add2Query("(cast(ledger.attributes->>'panno' as varchar) is null or cast(ledger.attributes->>'panno' as varchar)='')");
                            break;
                        case LedgerCreteria.Filter.GSTAvailable:
                            qb.Add2Query("Ledger.gstno != ''");
                            break;
                        case LedgerCreteria.Filter.PANAvailable:
                            qb.Add2Query("cast(ledger.attributes->>'panno' as varchar) !=''");
                            break;
                        case LedgerCreteria.Filter.InventoryCustomer:
                            qb.Add2Query("Ledger.customerid >0");
                            break;
                        case LedgerCreteria.Filter.InventorySupplier:
                            qb.Add2Query("Ledger.customerid >0");
                            break;
                        case LedgerCreteria.Filter.InventoryBank:
                            qb.Add2Query("Ledger.bankid >0");
                            break;
                        case LedgerCreteria.Filter.ManagableAssets:
                            qb.Add2Query("cast(Ledger.attributes->>'assettypeid' as bigint)>0");
                            break;
                        case LedgerCreteria.Filter.PandLExpense:
                        case LedgerCreteria.Filter.DailyExpense:
                        case LedgerCreteria.Filter.WeeklyExpense:
                        case LedgerCreteria.Filter.MonthlyExpense:
                        case LedgerCreteria.Filter.YearlyExpense:
                        case LedgerCreteria.Filter.AssetDepreciation:
                        case LedgerCreteria.Filter.SalaryExpense:
                        case LedgerCreteria.Filter.SalesMarginAtExpense:
                        case LedgerCreteria.Filter.PurchaseMarginExpense:
                            qb.Add2Query("Ledger.tagid = " + _ledger.filterid);
                            break;
                        case LedgerCreteria.Filter.InventoryInterest:
                        case LedgerCreteria.Filter.InventoryStock:
                            qb.Add2Query("Ledger.tagid = " + (_ledger.filterid - 30));
                            break;
                        case LedgerCreteria.Filter.CashRelated:
                            qb.Add2Query("((Ledger.tagid >11 and Ledger.tagid <19) OR  (Ledger.tagid=1))");
                            break;
                    }

                    if (_ledger.suppliertds)
                    {
                        qb.AddParameter("typeid", 16, "=");
                        qb.AddParameter("taxmode", 6, "=");
                    }
                    else if (_ledger.typeid > 0)
                        qb.AddParameter("typeid", _ledger.typeid, "=");

                    if (_ledger.customerid > 0)
                        qb.AddParameter("customerid", _ledger.customerid, "=");

                    if (_ledger.supplierid > 0)
                        qb.AddParameter("supplierid", _ledger.supplierid, "=");

                    if (_ledger.transportid > 0)
                        qb.AddParameter("transportid", _ledger.transportid, "=");

                    if (!string.IsNullOrEmpty(_ledger.displayname))
                    {
                        if (_ledger.displayname.Contains("%"))
                            qb.AddParameter("ledger.displayname", _ledger.displayname, " ilike ");
                        else
                            qb.AddParameter("ledger.displayname", _ledger.displayname, "=");
                    }
                    qb.SortField = "ledger.typeid,ledger.displayname";
                    command = qb.GetCommand(conn);
                };

                SortedList<int, string> AccountTypes = new SortedList<int, string>
                {
                    { 1, "Capital" },
                    { 2, "Fixed asset" },
                    { 19, "Current asset" },
                    { 3, "Liablities" },
                    { 20, "Current Liablities" },
                    { 4, "Direct Income" },
                    { 5, "Direct Expense" },
                    { 6, "Cash-In-Hand" },
                    { 7, "Bank Account" },
                    { 8, "Stock-In-Hand" },
                    { 9, "Indirect Incomes" },
                    { 10, "Indirect Expenses" },
                    { 11, "Sundry Debtors" },
                    { 12, "Sundry Creditors" },
                    { 13, "Petty Cash" },
                    { 14, "Sales" },
                    { 15, "Purchase" },
                    { 16, "Tax & Duties" },
                    { 17, "Petty Cash Direct Expense" },
                    { 18, "Petty Cash In-Direct Expense" },
                    { 21, "Petty Cash Direct Income" },
                    { 22, "Petty Cash In-Direct Income" },
                    { 30, "Rounding" }
                };

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        LedgerDetail oledger = new LedgerDetail();
                        oledger.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oledger.parentid = reader["parentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["parentid"]);


                        oledger.tagid = reader["tagid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["tagid"]);
                        oledger.transportid = reader["transportid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["transportid"]);
                        oledger.supplierid = reader["supplierid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["supplierid"]);
                        oledger.customerid = reader["customerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["customerid"]);

                        oledger.displayname = reader["displayname"] == DBNull.Value ? "" : reader["displayname"].ToString();
                        oledger.hsncode = reader["hsncode"] == DBNull.Value ? "" : reader["hsncode"].ToString();
                        oledger.gstno = reader["gstno"] == DBNull.Value ? "" : reader["gstno"].ToString();
                        oledger.billingname = reader["billingname"] == DBNull.Value ? "" : reader["billingname"].ToString();
                        oledger.pincode = reader["pincode"] == DBNull.Value ? "" : reader["pincode"].ToString();


                        oledger.typeid = reader["typeid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["typeid"]);
                        oledger.parentinfo_json = reader["parentinfo"] == DBNull.Value ? "" : reader["parentinfo"].ToString();
                        oledger.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oledger.isinterstate = reader["isinterstate"] == DBNull.Value ? false : Convert.ToBoolean(reader["isinterstate"]);
                        oledger.attributes_json = reader["attributes"] == DBNull.Value ? "null" : reader["attributes"].ToString();

                        oledger.eodcashledger = reader["eodcashledger"] == DBNull.Value ? false : Convert.ToBoolean(reader["eodcashledger"]);
                        oledger.billwise = reader["billwise"] == DBNull.Value ? false : Convert.ToBoolean(reader["billwise"]);

                        oledger.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oledger.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oledger.parentgroup = reader["parentgroup"] == DBNull.Value ? "" : reader["parentgroup"].ToString();
                        oledger.opening = reader["opening"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["opening"]);
                        oledger.closing = reader["closing"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["closing"]);
                        oledger.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);

                        oledger.taxmode = reader["taxmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["taxmode"]);
                        oledger.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                        oledger.cityid = reader["cityid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cityid"]);
                        oledger.stateid = reader["stateid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["stateid"]);
                        oledger.taxpercentage = reader["taxpercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["taxpercentage"]);

                        oledger.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oledger.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oledger.refid = reader["refid"] == DBNull.Value ? "" : reader["refid"].ToString();
                        oledger.contactperson = reader["contactperson"] == DBNull.Value ? "" : reader["contactperson"].ToString();
                        oledger.contactnumber = reader["contactnumber"] == DBNull.Value ? "" : reader["contactnumber"].ToString();
                        oledger.contactaddress = reader["contactaddress"] == DBNull.Value ? "" : reader["contactaddress"].ToString();
                        oledger.emailid = reader["emailid"] == DBNull.Value ? "" : reader["emailid"].ToString();

                        oledger.cempid = reader["cemployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["cemployeeid"]);
                        oledger.mempid = reader["memployeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["memployeeid"]);

                        var roottypename = AccountTypes.ContainsKey(oledger.typeid) ? AccountTypes[oledger.typeid] : "";
                        if (oledger.parentinfo == null || oledger.parentinfo.levels == null || oledger.parentinfo.levels.Count() == 0)
                        {
                            oledger.parentinfo = new Ledger.ParentInfo() { level = 0, detail = roottypename, levels = new string[] { roottypename } };
                        }
                        else if (oledger.parentinfo.levels[0] != roottypename)
                        {
                            oledger.parentinfo.levels = new string[] { roottypename }.Concat(oledger.parentinfo.levels).ToArray();
                            oledger.parentinfo.detail = string.Join("/", oledger.parentinfo.levels);
                            oledger.parentinfo.level = oledger.parentinfo.levels.Length - 1;
                        }
                        lst.Add(oledger);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _ledger);
            }

            var empids = lst.ConvertAll(x => x.cempid);
            empids.AddRange(lst.ConvertAll(y => y.mempid));


            EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
            var names = employeeDataService.SearchHeader(new EmployeeCreteria()
            {
                ids = empids.Distinct().ToList()
            });

            lst.ForEach(x =>
            {
                if (names.Exists(y => y.id == x.cempid))
                {
                    x.cemp = names.Find(y => y.id == x.cempid).name;
                }
                if (names.Exists(y => y.id == x.mempid))
                {
                    x.memp = names.Find(y => y.id == x.mempid).name;
                }
            });
            return lst;
        }

        public Ledger Insert(Ledger oLedger)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    Insert(transaction, conn, oLedger);
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_ledger)
                    {
                        Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID,
                            Services.IntrayManager.EntryType.Ledger,
                        oLedger.id, string.Format("New Ledger {0} registered.", oLedger.displayname),
                        Context.UserID, 0, transaction, this.Context);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLedger);
            }

            return oLedger;
        }

        public Ledger Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Ledger oLedger)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

            if ((Services.ConfigManager.Instance(Context.OrganizationID).Config == null ||
                Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_ledger == false))
            {
                oLedger.isactive = true;
                oLedger.Activestatus = BaseEntity.ActiveStatus.Approved;
            }
            else
            {
                oLedger.isactive = false;
                oLedger.Activestatus = BaseEntity.ActiveStatus.UnApproved;
            }
            command.Parameters.Add("activestatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLedger.activestatus;
            command.Parameters.Add("displayname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.displayname == null ? "" : oLedger.displayname;
            command.Parameters.Add("parentgroup", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.parentgroup == null ? "" : oLedger.parentgroup;
            command.Parameters.Add("parentinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oLedger.parentinfo == null ? "null" : oLedger.parentinfo_json;
            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oLedger.attributes == null ? "null" : oLedger.attributes_json;
            command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLedger.typeid;
            command.Parameters.Add("parentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedger.parentid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLedger.isactive;

            command.Parameters.Add("billwise", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLedger.billwise;
            command.Parameters.Add("eodcashledger", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLedger.eodcashledger;

            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLedger.version;
            command.Parameters.Add("tagid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLedger.tagid;

            command.Parameters.Add("pincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.pincode == null ? "" : oLedger.pincode;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.hsncode == null ? "" : oLedger.hsncode;
            command.Parameters.Add("gstno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.gstno == null ? "" : oLedger.gstno;
            command.Parameters.Add("billingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.billingname == null ? "" : oLedger.billingname;

            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedger.customerid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedger.supplierid;
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedger.transportid;
            command.Parameters.Add("isinterstate", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLedger.isinterstate;

            command.Parameters.Add("opening", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLedger.opening;
            command.Parameters.Add("closing", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLedger.closing;

            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLedger.taxpercentage;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedger.bankid;
            command.Parameters.Add("cityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedger.cityid;
            command.Parameters.Add("stateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedger.stateid;
            command.Parameters.Add("taxmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLedger.taxmode;

            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("refid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.refid == null ? "" : oLedger.refid;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.contactperson == null ? "" : oLedger.contactperson;
            command.Parameters.Add("contactnumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.contactnumber == null ? "" : oLedger.contactnumber;
            command.Parameters.Add("contactaddress", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.contactaddress == null ? "" : oLedger.contactaddress;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.emailid == null ? "" : oLedger.emailid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLedger.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oLedger;
        }

        internal bool UpdateStatus(NpgsqlTransaction transaction, NpgsqlConnection conn, Ledger oBOMMaster)
        {
            NpgsqlCommand command = GetCommand("update ledger set isactive=@isactive,activestatus=@activestatus,activeby=@modifiedby,modifiedon=@modifiedon where id=@id", conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oBOMMaster.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("activestatus", NpgsqlTypes.NpgsqlDbType.Integer).Value = oBOMMaster.activestatus;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oBOMMaster.id;
            int successCount = command.ExecuteNonQuery();
            return successCount > 0;
        }
        internal bool ChangeStatus(ChangeAttributes item)
        {

            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    NpgsqlCommand command = GetCommand("update ledger set activestatus=@activestatus,activeby=@activeby,modifiedon=@modifiedon where id in (" + string.Join(",", item.ids) + ")", conn);
                    command.Parameters.Add("activestatus", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.statusid;
                    command.Parameters.Add("activeby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    int successCount = command.ExecuteNonQuery();
                    conn.Close();
                    return successCount > 0;
                }
            }
            catch (Exception exp)
            {
                return false;
            }
        }

        public Ledger Update(Ledger oLedger)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oLedger);
                    if (Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_ledger)
                    {
                        Services.IntrayManager.Instance(Context.OrganizationID).Add(Context.OrganizationID,
                            Services.IntrayManager.EntryType.Ledger,
                        oLedger.id, string.Format("Ledger {0} Updated.", oLedger.displayname),
                        Context.UserID, 0, transaction, this.Context);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLedger);
            }

            return oLedger;
        }

        public Ledger Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Ledger oLedger)
        {
            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedger.id;
            command.Parameters.Add("displayname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.displayname == null ? "" : oLedger.displayname;
            command.Parameters.Add("parentgroup", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.parentgroup == null ? "" : oLedger.parentgroup;
            command.Parameters.Add("parentinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oLedger.parentinfo == null ? "null" : oLedger.parentinfo_json;
            command.Parameters.Add("attributes", NpgsqlTypes.NpgsqlDbType.Json).Value = oLedger.attributes == null ? "null" : oLedger.attributes_json;
            command.Parameters.Add("parentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedger.parentid;
            command.Parameters.Add("typeid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLedger.typeid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLedger.isactive;
            command.Parameters.Add("billwise", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLedger.billwise;
            command.Parameters.Add("eodcashledger", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLedger.eodcashledger;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedger.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLedger.version;
            command.Parameters.Add("opening", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLedger.opening;
            command.Parameters.Add("closing", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLedger.closing;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oLedger.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("customerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedger.customerid;
            command.Parameters.Add("supplierid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedger.supplierid;
            command.Parameters.Add("transportid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedger.transportid;
            command.Parameters.Add("isinterstate", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLedger.isinterstate;
            command.Parameters.Add("tagid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLedger.tagid;

            command.Parameters.Add("pincode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.pincode == null ? "" : oLedger.pincode;
            command.Parameters.Add("hsncode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.hsncode == null ? "" : oLedger.hsncode;
            command.Parameters.Add("gstno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.gstno == null ? "" : oLedger.gstno;
            command.Parameters.Add("billingname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.billingname == null ? "" : oLedger.billingname;

            command.Parameters.Add("taxpercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oLedger.taxpercentage;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedger.bankid;
            command.Parameters.Add("cityid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedger.cityid;
            command.Parameters.Add("stateid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedger.stateid;
            command.Parameters.Add("taxmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLedger.taxmode;

            command.Parameters.Add("refid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.refid == null ? "" : oLedger.refid;
            command.Parameters.Add("contactperson", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.contactperson == null ? "" : oLedger.contactperson;
            command.Parameters.Add("contactnumber", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.contactnumber == null ? "" : oLedger.contactnumber;
            command.Parameters.Add("contactaddress", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.contactaddress == null ? "" : oLedger.contactaddress;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oLedger.emailid == null ? "" : oLedger.emailid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oLedger.version = Convert.ToInt32(reader[0].ToString());
                }
            }
            return oLedger;
        }


        public bool Delete(Ledger oLedger)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oLedger);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oLedger);

                throw exp;
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Ledger oLedger)
        {
            var param = new SortedList<string, object>();
            param.Add("ledgerid", oLedger.id);
            using (var reader = ExecuteReader(@"Select count(*),'Billwise Adjustment' as reason from Voucheritembillwise where ledgerid=@ledgerid and isactive=true
            union all Select count(*),'Voucher Items' as reason from Voucheritems where ledgerid=@ledgerid and isactive=true
            union all Select count(*),'Vouchers' as reason from Voucher where adjustmentledgerid=@ledgerid and isactive=true;", param))
            {
                while (reader.Read())
                {
                    if (reader.GetInt32(0) > 0)
                    {
                        throw new Exception(oLedger.displayname + " Contains " + reader.GetString(1));
                    }
                }
            }

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oLedger.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oLedger.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oLedger.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
