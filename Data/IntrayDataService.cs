using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class IntrayDataService : SAASData
    {
        const string SQL_Search = @"with users as
(
select users.id,employee.name from users left outer join employee on employee.id = users.employeeid 
)												
select intray.referenceid, intray.referencetype, intray.typename, intray.createdon, intray.modifiedon, intray.companyid,
intray.createdby, intray.modifiedby, intray.isactive, intray.actiontype, intray.version, intray.actionby, intray.actionuser, 
intray.actionon, intray.id, intray.detail,intray.locationid , createduser.name employeename, modifieduser.name modifieduser
from intray 
left outer join users createduser on createduser.id = intray.createdby
left outer join users modifieduser on modifieduser.id = intray.modifiedby
            where intray.id=@id";


        const string SQL_Insert = @"INSERT INTO public.intray(referenceid, referencetype, typename, createdon, modifiedon, createdby, modifiedby, isactive, actiontype, version, actionby, actionuser, actionon, detail,locationid,companyid) 
        VALUES(@referenceid, @referencetype, @typename, @createdon, @modifiedon, @createdby, @modifiedby, @isactive, @actiontype, @version, @actionby, @actionuser, @actionon, @detail,@locationid,@companyid) RETURNING id";


        const string SQL_Update = @"UPDATE public.intray SET referenceid=@referenceid, referencetype=@referencetype, typename=@typename, modifiedon=@modifiedon, modifiedby=@modifiedby, actiontype=@actiontype, version=@version+1, actionby=@actionby, actionuser=@actionuser, actionon=@actionon, detail=@detail where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.intray set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Intray> Search(IntrayCreteria _intray)
        {
            List<Intray> lst = new List<Intray>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_intray.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _intray.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                    qb.AddParameter("intray.isactive", true, "=");

                    if (_intray.referencetype > 0)
                        qb.AddParameter("intray.referencetype", _intray.referencetype, "=");

                    if (_intray.referenceid > 0)
                        qb.AddParameter("intray.referenceid", _intray.referenceid, "=");

                    if (_intray.actiontype > 0)
                        qb.AddParameter("intray.actiontype", _intray.actiontype, "=");
                    else if(_intray.actiontype == 0)
                        qb.AddParameter("intray.actiontype", 1, "=");

                    if (_intray.fromdate != DateTime.MinValue)
                    {
                        qb.AddParameter("intray.createdon", "createdfrom", _intray.fromdate.Date, ">=");
                    }
                    if (_intray.todate != DateTime.MinValue)
                    {
                        qb.AddParameter("intray.createdon", "createdto", _intray.todate.AddDays(1).Date, "<");
                    }
                    qb.Add2Query(" (referencetype != 6 or referenceid=@currentuserid) ");
                    if (_intray.lastvisitedid > 0)
                        qb.AddParameter("intray.id", "lastvisitedid", _intray.lastvisitedid, "<");

                    qb.SortField = "intray.id desc";
                    if(_intray.todate == DateTime.MinValue)
                        qb.Limit = _intray.pagelength > 0 ? _intray.pagelength : 250;
                    command = qb.GetCommand(conn);
                    command.Parameters.AddWithValue("@currentuserid", Context.UserID);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Intray ointray = new Intray();
                        ointray.referenceid = reader["referenceid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["referenceid"]);
                        ointray.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        ointray.referencetype = reader["referencetype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["referencetype"]);
                        ointray.typename = reader["typename"] == DBNull.Value ? "" : reader["typename"].ToString();
                        ointray.employeename = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();
                        ointray.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        ointray.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        ointray.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        ointray.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        ointray.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        ointray.actiontype = reader["actiontype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["actiontype"]);
                        ointray.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        ointray.actionby = reader["actionby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["actionby"]);
                        ointray.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        ointray.actionuser = reader["actionuser"] == DBNull.Value ? "" : reader["actionuser"].ToString();
                        ointray.actionon = reader["actionon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["actionon"]);
                        ointray.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ointray.intrayid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        ointray.detail_json = reader["detail"] == DBNull.Value ? null : reader["detail"].ToString();
                        ointray.modifieduser = reader["modifieduser"] == DBNull.Value ? "" : reader["modifieduser"].ToString();
                        lst.Add(ointray);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _intray);
            }
            return lst;
        }

        public Intray Insert(Intray oIntray)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oIntray);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oIntray);
            }

            return oIntray;
        }

        public Intray Insert(System.Data.Common.DbTransaction transaction, System.Data.Common.DbConnection conn, Intray oIntray)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, (NpgsqlConnection)conn, (NpgsqlTransaction)transaction);
            command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIntray.referenceid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIntray.companyid;
            command.Parameters.Add("referencetype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIntray.referencetype;
            command.Parameters.Add("typename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oIntray.typename == null ? "" : oIntray.typename;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIntray.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oIntray.isactive;
            command.Parameters.Add("actiontype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIntray.actiontype;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIntray.version;
            command.Parameters.Add("actionby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIntray.actionby;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIntray.locationid;
            command.Parameters.Add("actionuser", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oIntray.actionuser == null ? "" : oIntray.actionuser;
            command.Parameters.Add("actionon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oIntray.actionon;
            command.Parameters.Add("detail", NpgsqlTypes.NpgsqlDbType.Json).Value = oIntray.detail_json == null ? "null" : oIntray.detail_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oIntray.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oIntray;
        }
        public Intray UpdateStatus(Intray oIntray)
        {
            if (oIntray.id == 0) return oIntray;

            NpgsqlTransaction transaction = null;
            EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
            employeeDataService.Context = this.Context;

            List<Intray> intraylist = Search(new IntrayCreteria() { id = oIntray.id });
            if (Context.UserID > 0 && intraylist.Count == 1)
            {
                var actionbedby = employeeDataService.GetUserName(new List<long>(new long[] { Context.UserID }));
                if (actionbedby.Exists(e => e.id == Context.UserID))
                {
                    intraylist[0].actionby = Context.UserID;
                    intraylist[0].actionuser = actionbedby.Find(e => e.id == Context.UserID).name;
                    intraylist[0].modifiedby = Context.UserID;
                }
            }
            if (intraylist != null && intraylist.Count > 0 && (oIntray.actiontype == 3 || oIntray.actiontype == 4))
            {
                intraylist[0].actiontype = oIntray.actiontype;

                if (intraylist[0].referencetype == (int)Services.IntrayManager.EntryType.Supplier)
                {
                    SupplierDataService supplierdata = new SupplierDataService() { Context = this.Context };
                    supplierdata.Context = this.Context;
                    var changestatus = new ChangeAttributes()
                    {
                        ids = new  List<long>(),
                    };
                    changestatus.ids.Add(intraylist[0].referenceid);
                    if (oIntray.actiontype == 3)
                    {
                        changestatus.statusid = 3;
                    }
                    else if (oIntray.actiontype == 4)
                    {
                        changestatus.statusid =2;
                    }
                    supplierdata.ChangeStatus(changestatus);
                }
                else if (intraylist[0].referencetype == (int)Services.IntrayManager.EntryType.Product)
                {
                    ProductsDataService productsdata = new ProductsDataService() { Context = this.Context };
                    productsdata.Context = this.Context;
                    List<Products> productslist = productsdata.Search(new ProductsCreteria() { id = intraylist[0].referenceid });
                    if (productslist != null && productslist.Count == 1)
                    {
                        if (oIntray.actiontype == 3)
                            productslist[0].isactive = false;
                        else if (oIntray.actiontype == 4)
                            productslist[0].isactive = true;

                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();

                            if (oIntray.actiontype == 4)
                                productsdata.Update(transaction, conn, productslist[0]);
                            Update(transaction, conn, intraylist[0]);

                            transaction.Commit();
                            conn.Close();
                        }

                    }
                }
                else if (intraylist[0].referencetype == (int)Services.IntrayManager.EntryType.Agent)
                {
                    AgentDataService agentdata = new AgentDataService() { Context = this.Context };
                    agentdata.Context = this.Context;
                    List<Agent> agentlist = agentdata.Search(new AgentCreteria() { id = intraylist[0].referenceid });
                    if (agentlist != null && agentlist.Count == 1)
                    {

                        if (oIntray.actiontype == 3)
                            agentlist[0].isactive = false;
                        else if (oIntray.actiontype == 4)
                            agentlist[0].isactive = true;

                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();

                            if (oIntray.actiontype == 4)
                                agentdata.Update(transaction, conn, agentlist[0]);
                            Update(transaction, conn, intraylist[0]);

                            transaction.Commit();
                            conn.Close();
                        }

                    }
                }
                else if (intraylist[0].referencetype == (int)Services.IntrayManager.EntryType.MfgDesign)
                {
                    MfgdesignDataService agentdata = new MfgdesignDataService() { Context = this.Context };
                    agentdata.Context = this.Context;
                    List<Mfgdesign> agentlist = agentdata.Search(new MfgdesignCreteria() { id = intraylist[0].referenceid });
                    if (agentlist != null && agentlist.Count == 1)
                    {

                        if (oIntray.actiontype == 3)
                        {
                            agentlist[0].isactive = false;
                        }
                        else if (oIntray.actiontype == 4)
                        {
                            agentlist[0].isactive = true;
                            agentlist[0].Activestatus = BaseEntity.ActiveStatus.Approved;
                        }

                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();

                            if (oIntray.actiontype == 4)
                                agentdata.UpdateStatus(transaction, conn, agentlist[0]);
                            Update(transaction, conn, intraylist[0]);

                            transaction.Commit();
                            conn.Close();
                        }

                    }
                }
                else if (intraylist[0].referencetype == (int)Services.IntrayManager.EntryType.BomMaster)
                {
                    BOMMasterDataService agentdata = new BOMMasterDataService() { Context = this.Context };
                    agentdata.Context = this.Context;
                    List<BOMMaster> agentlist = agentdata.Search(new BOMMasterCreteria() { id = intraylist[0].referenceid });
                    if (agentlist != null && agentlist.Count == 1)
                    {

                        if (oIntray.actiontype == 3)
                        {
                            agentlist[0].isactive = false;
                        }
                        else if (oIntray.actiontype == 4)
                        {
                            agentlist[0].isactive = true;
                            agentlist[0].Activestatus = BaseEntity.ActiveStatus.Approved;
                        }

                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();

                            if (oIntray.actiontype == 4)
                                agentdata.UpdateStatus(transaction, conn, agentlist[0]);
                            Update(transaction, conn, intraylist[0]);

                            transaction.Commit();
                            conn.Close();
                        }

                    }
                }
                else if (intraylist[0].referencetype == (int)Services.IntrayManager.EntryType.Ledger)
                {
                    LedgerDataService agentdata = new LedgerDataService() { Context = this.Context };
                    agentdata.Context = this.Context;
                    List<LedgerDetail> agentlist = agentdata.Search(new LedgerCreteria() { id = intraylist[0].referenceid });
                    if (agentlist != null && agentlist.Count == 1)
                    {

                        if (oIntray.actiontype == 3)
                        {
                            agentlist[0].isactive = false;
                        }
                        else if (oIntray.actiontype == 4)
                        {
                            agentlist[0].isactive = true;
                            agentlist[0].Activestatus = BaseEntity.ActiveStatus.Approved;
                        }

                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();
                            if (oIntray.actiontype == 4)
                                agentdata.UpdateStatus(transaction, conn, agentlist[0]);
                            Update(transaction, conn, intraylist[0]);
                            transaction.Commit();
                            conn.Close();
                        }

                    }
                }
                else if (intraylist[0].referencetype == (int)Services.IntrayManager.EntryType.Brand)
                {
                    BrandDataService agentdata = new BrandDataService() { Context = this.Context };
                    agentdata.Context = this.Context;
                    var agentlist = agentdata.Search(new BrandCreteria() { id = intraylist[0].referenceid });
                    if (agentlist != null && agentlist.Count == 1)
                    {
                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();
                            if (oIntray.actiontype == 4)
                            {
                                ExecuteQuery("update brand set isactive=true where id=" + intraylist[0].referenceid,
                                    new SortedList<string, object>(), conn, transaction);
                            }
                            else if (oIntray.actiontype == 3)
                            {
                                ExecuteQuery("update brand set isactive=false where id=" + intraylist[0].referenceid,
                                    new SortedList<string, object>(), conn, transaction);
                            }
                            Update(transaction, conn, intraylist[0]);
                            transaction.Commit();
                            conn.Close();
                        }
                    }
                }
                else if (intraylist[0].referencetype == (int)Services.IntrayManager.EntryType.BomPO)
                {
                    ComponentPODataService agentdata = new ComponentPODataService() { Context = this.Context };
                    agentdata.Context = this.Context;
                    List<ComponentPO> agentlist = agentdata.Search(new ComponentPOCreteria() { id = intraylist[0].referenceid });
                    if (agentlist != null && agentlist.Count == 1)
                    {
                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();
                            if (oIntray.actiontype == 4)
                            {
                                ExecuteQuery("update componentpo set hasapproved=true,statusid=1,approvedby=" + Context.UserID + " where id=" + intraylist[0].referenceid,
                                    new SortedList<string, object>(),conn,transaction);
                            }
                            else if (oIntray.actiontype == 3)
                            {
                                ExecuteQuery("update componentpo set hasapproved=false,statusid=2,approvedby=" + Context.UserID + " where id=" + intraylist[0].referenceid,
                                    new SortedList<string, object>(), conn, transaction);
                            }
                            Update(transaction, conn, intraylist[0]);
                            transaction.Commit();
                            conn.Close();
                        }
                    }
                }
                else if (intraylist[0].referencetype == (int)Services.IntrayManager.EntryType.Customer)
                {
                    CustomerDataService agentdata = new CustomerDataService() { Context = this.Context };
                    agentdata.Context = this.Context;
                    List<Customer> agentlist = agentdata.Search(new CustomerCreteria() { id = intraylist[0].referenceid });
                    if (agentlist != null && agentlist.Count == 1)
                    {

                        if (oIntray.actiontype == 3)
                        {
                            agentlist[0].isactive = false;
                        }
                        else if (oIntray.actiontype == 4)
                        {
                            agentlist[0].isactive = true;
                            agentlist[0].Activestatus = BaseEntity.ActiveStatus.Approved;
                        }

                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();

                            if (oIntray.actiontype == 4)
                                agentdata.UpdateStatus(transaction, conn, agentlist[0]);
                            Update(transaction, conn, intraylist[0]);

                            transaction.Commit();
                            conn.Close();
                        }

                    }
                }
                else if (intraylist[0].referencetype == (int)Services.IntrayManager.EntryType.Attributes)
                {
                    ReferencelistDataService refDate = new ReferencelistDataService() { Context = this.Context };
                    refDate.Context = this.Context;
                    List<Referencelist> refList = refDate.Search(new ReferencelistCreteria() { id = intraylist[0].referenceid });
                    if (refList != null && refList.Count == 1)
                    {

                        if (oIntray.actiontype == 3)
                            refList[0].isactive = false;
                        else if (oIntray.actiontype == 4)
                            refList[0].isactive = true;

                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();

                            if (oIntray.actiontype == 4)
                                refDate.UpdateStatus(transaction, conn, refList[0]);
                            Update(transaction, conn, intraylist[0]);

                            transaction.Commit();
                            conn.Close();
                        }

                    }
                }
                else if (intraylist[0].referencetype == (int)Services.IntrayManager.EntryType.Transfer)
                {
                    BundlesDataService podata = new BundlesDataService(false) { Context = this.Context };
                    var bundles = podata.Search(new BundlesCreteria()
                    {
                        id = intraylist[0].referenceid
                    });
                    podata.Context = this.Context;
                    if (bundles != null && bundles.Count == 1)
                    {
                        if (oIntray.actiontype == 3)
                            bundles[0].status = 9;      // Rejected bundle
                        else if (oIntray.actiontype == 4)
                            bundles[0].status = 8;      // Approved for Partial bundle transfer
                        var param = new SortedList<string, object>();
                        param.Add("status", bundles[0].status);
                        param.Add("id", bundles[0].id);
                        ExecuteQuery("update Bundles set status=@status where id=@id", param);
                    }
                }
                else if (intraylist[0].referencetype == (int)Services.IntrayManager.EntryType.PurchaseOrder)
                {
                    PurchaseorderDataService podata = new PurchaseorderDataService() { Context = this.Context };
                    podata.Context = this.Context;
                    List<Purchaseorder> polist = podata.Search(new PurchaseorderCreteria() { id = intraylist[0].referenceid });
                    if (polist != null && polist.Count == 1)
                    {
                        if (oIntray.actiontype == 3)
                            polist[0].status = Purchaseorder.POStatus.REJECTED;
                        else if (oIntray.actiontype == 4)
                            polist[0].status = Purchaseorder.POStatus.APPROVED;

                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();

                            podata.UpdateStatus(transaction, conn, polist[0]);
                            Update(transaction, conn, intraylist[0]);

                            transaction.Commit();
                            conn.Close();
                        }

                        if (polist[0].status == Purchaseorder.POStatus.APPROVED &&
                            Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_purchaseorder)
                        {
                            //PurchaseorderPrint print = podata.Print(new Purchaseorder() { id = polist[0].id });
                            //podata.Publish(podata.Context.OrganizationID, print);
                        }

                    }
                }
                else if (intraylist[0].referencetype == (int)Services.IntrayManager.EntryType.SalesOrder)
                {
                    SalesorderDataService podata = new SalesorderDataService() { Context = this.Context };
                    podata.Context = this.Context;
                    List<Salesorder> polist = podata.Search(new SalesorderCreteria() { id = intraylist[0].referenceid });
                    if (polist != null && polist.Count == 1)
                    {
                        if (oIntray.actiontype == 3)
                            polist[0].status = Salesorder.POStatus.REJECTED;
                        else if (oIntray.actiontype == 4)
                        {
                            if ((Services.ConfigManager.Instance(Context.OrganizationID).Config == null ||
                                Services.ConfigManager.Instance(Context.OrganizationID).Config.intray_customer == false))
                            {
                                polist[0].status = Salesorder.POStatus.APPROVED;
                            }
                            else
                            {
                                CustomerDataService customerDataService = new CustomerDataService() { Context = this.Context };
                                var customer = customerDataService.Search(new CustomerCreteria()
                                {
                                    id = polist[0].customerid
                                }).First();
                                if(customer.Activestatus != BaseEntity.ActiveStatus.Approved)
                                {
                                    throw new Exception("Unapproved / Deactivated Customers Sales order can't approved");
                                }
                            }
                        }
                        using (NpgsqlConnection conn = CreateConnection())
                        {
                            conn.Open();
                            transaction = conn.BeginTransaction();

                            podata.UpdateStatus(transaction, conn, polist[0]);
                            Update(transaction, conn, intraylist[0]);

                            transaction.Commit();
                            conn.Close();
                        }
                    }
                }
                else if (intraylist[0].referencetype == (int)Services.IntrayManager.EntryType.BarcodeGeneration)
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        Update(transaction, conn, intraylist[0]);
                        if (intraylist[0].ActionType == Intray.IntrayActionTypes.Approved)
                        {
                            ExecuteQuery("Update lrinvoice set barcodegenerationapproval=true where id=" + intraylist[0].referenceid + ";", null,
                                conn, transaction);
                        }
                        transaction.Commit();
                        conn.Close();
                    }
                }
            }
            else if (intraylist != null && intraylist.Count > 0)
            {
                intraylist[0].actiontype = oIntray.actiontype;
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Update(transaction, conn, intraylist[0]);
                    transaction.Commit();
                    conn.Close();
                }

            }
            return oIntray;
        }
        public Intray Update(Intray oIntray)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oIntray);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oIntray);
            }

            return oIntray;
        }

        public Intray Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Intray oIntray)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("referenceid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIntray.referenceid;
            command.Parameters.Add("referencetype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIntray.referencetype;
            command.Parameters.Add("typename", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oIntray.typename == null ? "" : oIntray.typename;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oIntray.isactive;
            command.Parameters.Add("actiontype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIntray.actiontype;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIntray.version;
            command.Parameters.Add("actionby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIntray.actionby;
            command.Parameters.Add("actionuser", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oIntray.actionuser == null ? "" : oIntray.actionuser;
            command.Parameters.Add("actionon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oIntray.actionon;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIntray.id;
            command.Parameters.Add("detail", NpgsqlTypes.NpgsqlDbType.Json).Value = oIntray.detail_json == null ? "null" : oIntray.detail_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oIntray.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oIntray;
        }


        public bool Delete(Intray oIntray)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oIntray);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oIntray);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Intray oIntray)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oIntray.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIntray.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oIntray.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIntray.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIntray.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
