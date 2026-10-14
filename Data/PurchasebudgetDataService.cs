using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class PurchasebudgetDataService : BaseData
    {
        const string SQL_Search = @"select budgets, version, isactive, createdon, modifiedon, createby, modifiedby, budgetid, employeeid, pfrom, pto, sfrom, sto, budget, id, status, blockreason from purchasebudget where id=@id";


        const string SQL_Insert = @"INSERT INTO public.purchasebudget(budgets, version, isactive, createdon, modifiedon, createby, modifiedby, budgetid, employeeid, pfrom, pto, sfrom, sto, budget, status, blockreason) VALUES(@budgets, @version, @isactive, @createdon, @modifiedon, @createby, @modifiedby, @budgetid, @employeeid, @pfrom, @pto, @sfrom, @sto, @budget, @status, @blockreason) RETURNING id";


        const string SQL_Update = @"UPDATE public.purchasebudget SET budgets=@budgets, version=@version+1, modifiedon=@modifiedon, createby=@createby, modifiedby=@modifiedby, budgetid=@budgetid, employeeid=@employeeid, pfrom=@pfrom, pto=@pto, sfrom=@sfrom, sto=@sto, budget=@budget, status=@status, blockreason=@blockreason where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.purchasebudget set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Purchasebudget> Search(PurchasebudgetCreteria _purchasebudget)
        {
            List<Purchasebudget> lst = new List<Purchasebudget>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_purchasebudget.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _purchasebudget.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    if (_purchasebudget.budgetid > 0)
                        qb.AddParameter("budgetid", _purchasebudget.budgetid, "=");

                    if (_purchasebudget.budgetids != null &&
                        _purchasebudget.budgetids.Count > 0)
                    {
                        qb.AddInParamer("budgetid", String.Join(",", _purchasebudget.budgetids));
                    }
                    if (_purchasebudget.managerids != null &&
                        _purchasebudget.managerids.Count > 0)
                    {
                        qb.AddInParamer("employeeid", String.Join(",", _purchasebudget.managerids));
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Purchasebudget opurchasebudget = new Purchasebudget();
                        opurchasebudget.budgets_json = reader["budgets"] == DBNull.Value ? null : reader["budgets"].ToString();
                        opurchasebudget.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        opurchasebudget.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        opurchasebudget.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        opurchasebudget.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        opurchasebudget.createdby = reader["createby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createby"]);
                        opurchasebudget.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        opurchasebudget.budgetid = reader["budgetid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["budgetid"]);
                        opurchasebudget.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        opurchasebudget.pfrom = reader["pfrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["pfrom"]);
                        opurchasebudget.pto = reader["pto"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["pto"]);
                        opurchasebudget.sfrom = reader["sfrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["sfrom"]);
                        opurchasebudget.sto = reader["sto"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["sto"]);
                        opurchasebudget.budget = reader["budget"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["budget"]);
                        opurchasebudget.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        opurchasebudget.status = reader["status"] == DBNull.Value ? 0 : Convert.ToInt32(reader["status"]);
                        opurchasebudget.blockreason = reader["blockreason"] == DBNull.Value ? "" : reader["blockreason"].ToString();
                        if (opurchasebudget.budgets != null && opurchasebudget.budgets.Count > 0)
                            opurchasebudget.alloted = opurchasebudget.budgets.Sum(x => x.value);
                        lst.Add(opurchasebudget);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _purchasebudget);
            }
            return lst;
        }

        public Purchasebudget Insert(Purchasebudget oPurchasebudget)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oPurchasebudget);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPurchasebudget);
            }

            return oPurchasebudget;
        }

        public Purchasebudget Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Purchasebudget oPurchasebudget)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("budgets", NpgsqlTypes.NpgsqlDbType.Json).Value = oPurchasebudget.budgets_json == null ? "null" : oPurchasebudget.budgets_json;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchasebudget.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPurchasebudget.isactive;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchasebudget.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchasebudget.modifiedon;
            command.Parameters.Add("createby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchasebudget.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchasebudget.modifiedby;
            command.Parameters.Add("budgetid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchasebudget.budgetid;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchasebudget.employeeid;
            command.Parameters.Add("pfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchasebudget.pfrom;
            command.Parameters.Add("pto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchasebudget.pto;
            command.Parameters.Add("sfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchasebudget.sfrom;
            command.Parameters.Add("sto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchasebudget.sto;
            command.Parameters.Add("budget", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchasebudget.budget;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchasebudget.status;
            command.Parameters.Add("blockreason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchasebudget.blockreason == null ? "" : oPurchasebudget.blockreason;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPurchasebudget.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oPurchasebudget;
        }

        public Purchasebudget Update(Purchasebudget oPurchasebudget)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oPurchasebudget);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPurchasebudget);
            }

            return oPurchasebudget;
        }

        public Purchasebudget Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Purchasebudget oPurchasebudget)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("budgets", NpgsqlTypes.NpgsqlDbType.Json).Value = oPurchasebudget.budgets_json == null ? "null" : oPurchasebudget.budgets_json;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchasebudget.version;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPurchasebudget.isactive;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchasebudget.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchasebudget.modifiedon;
            command.Parameters.Add("createby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchasebudget.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchasebudget.modifiedby;
            command.Parameters.Add("budgetid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchasebudget.budgetid;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchasebudget.employeeid;
            command.Parameters.Add("pfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchasebudget.pfrom;
            command.Parameters.Add("pto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchasebudget.pto;
            command.Parameters.Add("sfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchasebudget.sfrom;
            command.Parameters.Add("sto", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oPurchasebudget.sto;
            command.Parameters.Add("budget", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oPurchasebudget.budget;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchasebudget.id;
            command.Parameters.Add("status", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchasebudget.status;
            command.Parameters.Add("blockreason", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oPurchasebudget.blockreason == null ? "" : oPurchasebudget.blockreason;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oPurchasebudget.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oPurchasebudget;
        }


        public bool Delete(Purchasebudget oPurchasebudget)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oPurchasebudget);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oPurchasebudget);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Purchasebudget oPurchasebudget)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oPurchasebudget.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchasebudget.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oPurchasebudget.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oPurchasebudget.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oPurchasebudget.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal int SaveAll(List<Purchasebudget> items)
        {
            NpgsqlTransaction transaction = null;
            int count = 0;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    foreach (var item in items)
                    {
                        if (item.id > 0)
                        {
                            item.ChangeStatus(BaseEntity.DataStatus.Update);
                            Update(transaction, conn, item);
                        }
                        else
                        {
                            item.ChangeStatus(BaseEntity.DataStatus.Create);
                            Insert(transaction, conn, item);
                        }
                        count++;
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
            }
            return count;
        }

        internal List<Purchasebudget.ReportItem> Report(PurchasebudgetCreteria item)
        {
            string query = @"with purchaseorders
as
(
select budgetid,employeeid,purchaseorderitems.productid,
purchaseorderitems.id,
(purchaseorderitems.oqty) ordered,
(purchaseorderitems.rqty) received,
(purchaseorderitems.price*purchaseorderitems.oqty) amount from purchasebudget
inner join purchaseorder on 
purchaseorder.shopcontactemployeeid = purchasebudget.employeeid and purchaseorder.potype = purchasebudget.budgetid
and purchaseorder.isactive=true
inner join purchaseorderitems on purchaseorderitems.purchaseorderid = purchaseorder.id
and purchaseorderitems.isactive=true
where purchaseorder.podate>=@sfrom and purchaseorder.podate<@sto {0}
),purchase as
(
	select purchaseorderitems.id,lrinvoiceitems.buyingqty,lrinvoiceitems.amount,
	lrinvoice.potypeid,shopcontactemployeeid employeeid from referencelist potype
	inner join lrinvoice  on lrinvoice.potypeid = potypeid and lrinvoice.isactive=true
	inner join lrinvoiceitems on lrinvoiceitems.lrinvoiceid = lrinvoice.id and lrinvoiceitems.isactive=true
	LEFT OUTER JOIN LATERAL json_array_elements (case when lrinvoiceitems.poinfo is null 
	or lrinvoiceitems.poinfo::text ='null' then '[]'::json else lrinvoiceitems.poinfo::json end) schemelist ON true
	inner join purchaseorderitems on purchaseorderitems.id =  cast(schemelist ->> 'purchaseorderitemsid' as bigint)	
	inner join purchaseorder on purchaseorder.id = purchaseorderitems.purchaseorderid
    where purchaseorder.podate>=@sfrom and purchaseorder.podate<@sto {0}
)";

            if(item.reportid == PurchasebudgetCreteria.Report.Budget)
            {
                query += @",productsummary as (
                                select budgetid,sum(ordered) orderqty,sum(received)
	                            receivedqty,sum(purchaseorders.amount) ordervalue,
                                sum(purchase.buyingqty) purchaseqty,sum(purchase.amount) purchasevalue from purchaseorders
                                left outer join purchase on purchase.id = purchaseorders.id
                                group by  budgetid
                            )
                            select productsummary.* from productsummary";
            } else  if (item.reportid == PurchasebudgetCreteria.Report.Manager)
            {
                query += @",productsummary as (
                                select budgetid,purchaseorders.employeeid,sum(ordered) orderqty,sum(received)
	                            receivedqty,sum(purchaseorders.amount) ordervalue,
                                sum(purchase.buyingqty) purchaseqty,sum(purchase.amount) purchasevalue from purchaseorders
                                left outer join purchase on purchase.id = purchaseorders.id
                                group by  budgetid,purchaseorders.employeeid
                            )

                            select productsummary.*,employee.name manager from productsummary
                            inner join employee on employee.id = productsummary.employeeid";
            }else 
            {
                query += @",productsummary as (
                                select budgetid,purchaseorders.employeeid,productid,sum(ordered) orderqty,sum(received)
	                            receivedqty,sum(purchaseorders.amount) ordervalue,
                                sum(purchase.buyingqty) purchaseqty,sum(purchase.amount) purchasevalue from purchaseorders
                                left outer join purchase on purchase.id = purchaseorders.id
                                group by  budgetid,purchaseorders.employeeid,purchaseorders.productid
                            )

                            select productsummary.*,employee.name manager,products.name product from productsummary
                            inner join employee on employee.id = productsummary.employeeid
                            inner join products on products.id = productsummary.productid";
            }

            List<string> conditions = new List<string>();
            if (item.managerids != null && item.managerids.Count > 0)
            {
                conditions.Add("purchaseorder.shopcontactemployeeid in (" + string.Join(",", item.managerids) + ")");
            }
            if (item.budgetids != null && item.budgetids.Count > 0)
            {
                conditions.Add("purchaseorder.potype in (" + string.Join(",", item.budgetids) + ")");
            }
            if (item.managerids != null && item.productids.Count > 0)
            {
                conditions.Add("purchaseorderitems.productid in (" + string.Join(",", item.productids) + ")");
            }
            query = string.Format(query, string.Join(" and ", conditions));
            
            var config = Search(item);
            if (config.Count == 0) return new List<Purchasebudget.ReportItem>();

            ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
            var budgets = referencelistDataService.SearchList(new ReferencelistCreteria()
            {
                ids = config.ConvertAll(x => x.budgetid).Distinct().ToList()
            });

            var param = new SortedList<string, object>();
            param.Add("sfrom", item.sfrom);
            param.Add("sto", item.sto);
            List<Purchasebudget.ReportItem> results = new List<Purchasebudget.ReportItem>();
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    long budgetid = reader["budgetid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["budgetid"]);
                    long employeeid = 0;
                    long productid = 0;

                    Purchasebudget.ReportItem reportItem = new Purchasebudget.ReportItem();
                    reportItem.budgetname = budgets.Exists(e => e.id == budgetid) ? budgets.Find(e => e.id == budgetid).name : "";
                    if (item.reportid == PurchasebudgetCreteria.Report.Manager ||
                        item.reportid == PurchasebudgetCreteria.Report.Product)
                    {
                        employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        reportItem.employee = reader["manager"] == DBNull.Value ? "" : Convert.ToString(reader["manager"]);
                    }
                    if (item.reportid == PurchasebudgetCreteria.Report.Product)
                    {
                        productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                        reportItem.product = reader["product"] == DBNull.Value ? "" : Convert.ToString(reader["product"]);
                    }
                    

                    reportItem.orderqty = reader["orderqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["orderqty"]);
                    reportItem.ordervalue = reader["ordervalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ordervalue"]);
                    reportItem.receivedqty = reader["receivedqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivedqty"]);

                    reportItem.purchaseqty = reader["purchaseqty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchaseqty"]);
                    reportItem.purchasevalue = reader["purchasevalue"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["purchasevalue"]);
                    if (item.reportid == PurchasebudgetCreteria.Report.Budget)
                    {
                        reportItem.configbudget = config.Sum(x => x.budgetid == budgetid ? x.budget : 0);
                        reportItem.configbudgetproducts = config.Sum(x => (x.budgetid == budgetid && x.budgets != null) ? x.budgets.Sum(y=>y.value) : 0);
                    }
                    else if(item.reportid == PurchasebudgetCreteria.Report.Manager ||
                        item.reportid == PurchasebudgetCreteria.Report.Product)
                    {
                        if (config.Exists(x => x.employeeid == employeeid && x.budgetid == budgetid))
                        {
                            var purchasebudget = config.Find(x => x.employeeid == employeeid && x.budgetid == budgetid);
                            reportItem.configbudget = purchasebudget.budget;
                            reportItem.configbudgetproducts = purchasebudget.budgets.Sum(x => x.value);
                            if (item.reportid != PurchasebudgetCreteria.Report.Product &&
                                purchasebudget.budgets != null)
                            {
                                if (purchasebudget.budgets.Exists(x => x.productid == productid))
                                {
                                    var productbuget = purchasebudget.budgets.Find(x => x.productid == productid);
                                    reportItem.productbudgetconfig = productbuget.value;
                                }
                            }
                        }
                    }
                    results.Add(reportItem);
                }
            }
            return results;
        }
    }

}
