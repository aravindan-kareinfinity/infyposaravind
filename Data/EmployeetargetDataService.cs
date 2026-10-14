using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;


namespace InfyPOS.Data

{
    public class EmployeetargetDataService : BaseData
    {
        const string SQL_Search = @"select id, employeeid, version, sectionid, locationid, year, month, salary, sales, target, dailysales, dailytarget, isactive, createdby, modifiedby, createdon, modifiedon,attribs,ratio from employeetarget where id=@id";

        const string SQL_Insert = @"
        DELETE FROM  public.employeetarget where employeeid=@employeeid and locationid=@locationid and sectionid=@sectionid and year=@year and month=@month;
        INSERT INTO public.employeetarget(employeeid, version, sectionid, locationid, year, month, salary, sales, target, dailysales, dailytarget, isactive, createdby, modifiedby, createdon, modifiedon,attribs,ratio) 
        VALUES(@employeeid, @version, @sectionid, @locationid, @year, @month, @salary, @sales, @target, @dailysales, @dailytarget, @isactive, @createdby, @modifiedby, @createdon, @modifiedon,@attribs,@ratio) RETURNING id";

        const string SQL_Update = @"UPDATE public.employeetarget SET employeeid=@employeeid, version=@version+1, sectionid=@sectionid, locationid=@locationid, year=@year, month=@month, salary=@salary, 
        sales=@sales, target=@target, dailysales=@dailysales, dailytarget=@dailytarget, modifiedby=@modifiedby, modifiedon=@modifiedon,attribs=@attribs,ratio=@ratio where id=@id and version=@version RETURNING version";
        const string SQL_Delete = @"update public.employeetarget set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Employeetarget> Search(EmployeetargetCreteria _employeetarget)
        {
            List<Employeetarget> lst = new List<Employeetarget>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_employeetarget.id != 0)
                {
                    command = new NpgsqlCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _employeetarget.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    if (_employeetarget.datebased)
                    {
                        qb.AddParameter("TO_DATE(year||'-'||month||'-01','YYYY-MM-DD')", "fromdate",
                            _employeetarget.targetfrom.Date, ">=");
                        qb.AddParameter("TO_DATE(year||'-'||month||'-01','YYYY-MM-DD')", "todate",
                            _employeetarget.targetto.Date.AddDays(1), "<");
                    }
                    if (_employeetarget.locationid == -9)
                    {
                        qb.Add2Query("employeeid=0 and sectionid=0 and locationid>0");
                    }
                    else if (_employeetarget.locationid == -10)
                    {
                        qb.Add2Query("employeeid=0 and sectionid>0 and locationid>0");
                    }
                    else
                    {
                        if (_employeetarget.sectionid > 0)
                        {
                            qb.AddParameter("sectionid", _employeetarget.sectionid, "=");
                        }
                        else if (_employeetarget.sectionids != null &&
                            _employeetarget.sectionids.Count > 0)
                        {
                            qb.AddInParamer("sectionid", String.Join(",", _employeetarget.sectionids));
                        }

                        if (_employeetarget.employeeid > 0)
                        {
                            qb.AddParameter("employeeid", _employeetarget.employeeid, "=");
                        }
                        else if (_employeetarget.employeeids != null &&
                            _employeetarget.employeeids.Count > 0)
                        {
                            qb.AddInParamer("employeeid", String.Join(",", _employeetarget.employeeids));
                        }
                    }
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employeetarget oemployeetarget = new Employeetarget();
                        oemployeetarget.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oemployeetarget.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        oemployeetarget.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oemployeetarget.sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                        oemployeetarget.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oemployeetarget.year = reader["year"] == DBNull.Value ? 0 : Convert.ToInt32(reader["year"]);
                        oemployeetarget.month = reader["month"] == DBNull.Value ? 0 : Convert.ToInt32(reader["month"]);
                        oemployeetarget.salary = reader["salary"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salary"]);
                        oemployeetarget.sales = reader["sales"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["sales"]);
                        oemployeetarget.target = reader["target"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["target"]);
                        oemployeetarget.dailysales = reader["dailysales"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dailysales"]);
                        oemployeetarget.dailytarget = reader["dailytarget"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["dailytarget"]);
                        oemployeetarget.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oemployeetarget.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oemployeetarget.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oemployeetarget.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oemployeetarget.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oemployeetarget.attribs_json = reader["attribs"] == DBNull.Value ? "null" : reader["attribs"].ToString();
                        oemployeetarget.ratio = reader["ratio"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ratio"]);
                        lst.Add(oemployeetarget);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _employeetarget);
            }
            return lst;
        }

        public List<Employeetarget> SaveAll(List<Employeetarget> oEmployeetarget)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var item in oEmployeetarget)
                    {
                        if (item.id > 0)
                        {
                            item.modifiedby = Context.UserID;
                            item.modifiedon = DateTimeNow;
                            Update(transaction, conn, item);
                        }
                        else
                        {
                            item.createdby = Context.UserID;
                            item.createdon = DateTimeNow;
                            item.modifiedby = Context.UserID;
                            item.modifiedon = DateTimeNow;
                            Insert(transaction, conn, item);
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeetarget);
            }

            return oEmployeetarget;
        }

        public Employeetarget Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeetarget oEmployeetarget)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeetarget.employeeid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeetarget.version;
            command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeetarget.sectionid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeetarget.locationid;
            command.Parameters.Add("year", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeetarget.year;
            command.Parameters.Add("month", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeetarget.month;
            command.Parameters.Add("salary", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeetarget.salary;
            command.Parameters.Add("sales", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeetarget.sales;
            command.Parameters.Add("target", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeetarget.target;
            command.Parameters.Add("attribs", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployeetarget.attribs_json == null ? "null" : oEmployeetarget.attribs_json;
            command.Parameters.Add("ratio", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeetarget.ratio;
            command.Parameters.Add("dailysales", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeetarget.dailysales;
            command.Parameters.Add("dailytarget", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeetarget.dailytarget;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeetarget.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeetarget.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeetarget.modifiedby;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeetarget.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeetarget.modifiedon;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmployeetarget.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oEmployeetarget;
        }

        public Employeetarget Update(Employeetarget oEmployeetarget)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oEmployeetarget);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeetarget);
            }

            return oEmployeetarget;
        }

        public Employeetarget Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeetarget oEmployeetarget)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeetarget.id;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeetarget.employeeid;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeetarget.version;
            command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeetarget.sectionid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeetarget.locationid;
            command.Parameters.Add("year", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeetarget.year;
            command.Parameters.Add("month", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeetarget.month;
            command.Parameters.Add("salary", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeetarget.salary;
            command.Parameters.Add("sales", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeetarget.sales;
            command.Parameters.Add("target", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeetarget.target;
            command.Parameters.Add("ratio", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeetarget.ratio;
            command.Parameters.Add("dailysales", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeetarget.dailysales;
            command.Parameters.Add("dailytarget", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeetarget.dailytarget;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeetarget.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeetarget.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeetarget.modifiedby;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeetarget.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeetarget.modifiedon;
            command.Parameters.Add("attribs", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployeetarget.attribs_json == null ? "null" : oEmployeetarget.attribs_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmployeetarget.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oEmployeetarget;
        }


        public bool Delete(List<Employeetarget> oEmployeetarget)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var item in oEmployeetarget)
                        hasSuccessCount = Delete(transaction, conn, item);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeetarget);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeetarget oEmployeetarget)
        {

            NpgsqlCommand command = new NpgsqlCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeetarget.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeetarget.modifiedby;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.TimestampTZ).Value = oEmployeetarget.modifiedon;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeetarget.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeetarget.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal List<EmployeetargetDetail> Incentive(EmployeetargetCreteria item)
        {
            var results = Search(new EmployeetargetCreteria()
            {
                datebased = true,
                targetfrom = item.targetfrom.Date,
                targetto = item.targetto.Date,
                sectionid = item.sectionid,
                employeeid = item.employeeid,
                locationid = item.locationid
            }).ConvertAll(x => new EmployeetargetDetail()
            {
                attribs = x.attribs,
                dailysales = x.dailysales,
                dailytarget = x.dailytarget,
                employeeid = x.employeeid,
                id = x.id,
                locationid = x.locationid,
                month = x.month,
                ratio = x.ratio,
                salary = x.salary,
                sales = 0,
                sectionid = x.sectionid,
                target = x.target,
                year = x.year
            });

            foreach (var gitem in results.GroupBy(x => new
            {
                x.sectionid,
                mode = item.overrideincentive ? item.incentivemode : (x.attribs != null ? x.attribs.incentivemode : 0),
                freshday = item.overrideincentive && item.incentivemode == 0 ? 0 : (x.attribs.incentivemode == 1 ? x.attribs.cnf_freshday : 0),
                freshdate = item.overrideincentive && item.incentivemode == 0 ? DateTime.MinValue : (x.attribs.incentivemode == 2 ? x.attribs.cnf_freshdate : DateTime.MinValue)
            }))
            {

                var query = @"select date_part('month',billdate) ""month"",
            date_part('year',billdate) ""year"",billitems.salesmanid,
            salescounterid sectionid,bill.locationid,
            {0}
            from billitems
            inner join bill on bill.id = billid
            inner join salablegoods on salablegoods.id = salablegoodsid
            inner join stock on stock.id = salablegoods.stockid
            inner join products on products.id = stock.productid 
            left outer join lrinvoiceitems on lrinvoiceitems.id = invoiceitemid
            left outer join lrinvoice on lrinvoice.id = lrinvoiceid
            where billdate>=@fromdate and billdate<@todate AND 
            billitems.isactive=true and billitems.salesmanid>0 
            and bill.locationid = @locationid and salescounterid = @sectionid 
            and bill.billtype != 9 and bill.isactive=true and billitems.salesmanid = @salesmanid
            group by date_part('month',billdate),date_part('year',billdate),
            billitems.salesmanid,salescounterid,bill.locationid";

                var condition = "";
                var inentivemode = item.overrideincentive ? item.incentivemode : gitem.Key.mode;

                if (item.billsource == 1)
                {
                    if (inentivemode == 0)
                    {
                        condition = ("sum(billitems.receivable) receivable");
                    }
                    else if (inentivemode == 1)
                    {
                        condition = (@"sum(case when((EXTRACT(epoch FROM(SELECT(bill.billdate - lrinvoice.entrydate))) / 86400)) > @agedays then billitems.receivable else 0 end) oldsales,
                                 sum(case when((EXTRACT(epoch FROM(SELECT(bill.billdate - lrinvoice.entrydate))) / 86400)) > @agedays then 0 else billitems.receivable end) freshsales");
                    }
                    else
                    {
                        condition = (@"sum(case when lrinvoice.entrydate > @agedate then 0 else billitems.receivable end) freshsales,
                                 sum(case when lrinvoice.entrydate > @agedate then billitems.receivable else 0 end) oldsales");
                    }
                }
                else
                {
                    if (inentivemode == 0)
                    {
                        condition = ("sum(billitems.qty* salablegoods.price) receivable");
                    }
                    else if (inentivemode == 1)
                    {
                        condition = (@"sum(case when((EXTRACT(epoch FROM(SELECT(bill.billdate - lrinvoice.entrydate))) / 86400)) > @agedays then billitems.qty* salablegoods.price else 0 end) oldsales,
                                 sum(case when((EXTRACT(epoch FROM(SELECT(bill.billdate - lrinvoice.entrydate))) / 86400)) > @agedays then 0 else billitems.qty*salablegoods.price end) freshsales");
                    }
                    else
                    {
                        condition = (@"sum(case when lrinvoice.entrydate > @agedate then 0 else billitems.receivable end) freshsales,
                                 sum(case when lrinvoice.entrydate > @agedate then billitems.receivable else 0 end) oldsales");
                    }
                }

                if (item.sectionid <= 0)
                    query = query.Replace(" and salescounterid = @sectionid", "");
                if (item.locationid <= 0)
                    query = query.Replace(" and bill.locationid = @locationid", "");
                if (item.employeeid <= 0)
                    query = query.Replace(" and billitems.salesmanid = @salesmanid", "");
                if (item.locationid <= 0)
                    query = query.Replace("and billitems.salesmanid>0", "");

                query = String.Format(query, condition);
                var param = new SortedList<string, object>();
                param.Add("locationid", item.locationid);
                param.Add("salesmanid", item.employeeid);
                param.Add("sectionid", gitem.Key.sectionid);
                param.Add("agedate", gitem.Key.freshdate);
                param.Add("agedays", gitem.Key.freshday);
                param.Add("fromdate", item.targetfrom.Date);
                param.Add("todate", item.targetto.Date.AddDays(1));

                query.Replace("and salescounterid = @sectionid", "and billitems.salesmanid in (" + string.Join(",", gitem.ToList().ConvertAll(x => x.employeeid)) + ")");

                List<SalesInfo> salesInfos = new List<SalesInfo>();
                using (var reader = ExecuteReader(query, param))
                {
                    while (reader.Read())
                    {
                        SalesInfo salesInfo = new SalesInfo();
                        salesInfo.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        salesInfo.sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                        salesInfo.salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]);
                        salesInfo.month = reader["month"] == DBNull.Value ? 0 : Convert.ToInt32(reader["month"]);
                        salesInfo.year = reader["year"] == DBNull.Value ? 0 : Convert.ToInt32(reader["year"]);
                        if (inentivemode == 0)
                        {
                            salesInfo.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        }
                        else
                        {
                            salesInfo.freshsales = reader["freshsales"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["freshsales"]);
                            salesInfo.oldsales = reader["oldsales"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["oldsales"]);
                            salesInfo.receivable = salesInfo.freshsales + salesInfo.oldsales;
                        }
                        salesInfos.Add(salesInfo);
                    }
                }

                if (item.locationid >= 0)
                {
                    var extrasalesmen = salesInfos.ConvertAll(x => x.salesmanid).FindAll(y => !results.Exists(z => z.employeeid == y));
                    if (extrasalesmen.Count > 0 && item.sectionid > 0)
                    {
                        var missingresults = Search(new EmployeetargetCreteria()
                        {
                            datebased = true,
                            targetfrom = item.targetfrom.Date,
                            targetto = item.targetto.Date,
                            employeeids = extrasalesmen
                        }).ConvertAll(x => new EmployeetargetDetail()
                        {
                            attribs = x.attribs,
                            dailysales = x.dailysales,
                            dailytarget = x.dailytarget,
                            employeeid = x.employeeid,
                            id = x.id,
                            locationid = x.locationid,
                            month = x.month,
                            ratio = x.ratio,
                            salary = x.salary,
                            sales = 0,
                            sectionid = x.sectionid,
                            target = x.target,
                            year = x.year
                        });
                        results.AddRange(missingresults);
                    }

                    foreach (var eitem in gitem)
                    {
                        var sinfo = salesInfos.FindAll(x => x.salesmanid == eitem.employeeid);
                        if (sinfo.Count() == 1)
                        {
                            eitem.sales = sinfo[0].receivable;
                            if (eitem.target > 0)
                            {
                                eitem.acheivedtarget = Math.Round(eitem.sales / eitem.target * 100, 2);
                                if (inentivemode > 0)
                                {
                                    eitem.freshsales = sinfo[0].freshsales;
                                    eitem.oldsales = sinfo[0].oldsales;

                                    if (!item.overrideincentive &&
                                        eitem.attribs.cnf_freshdaypercentage > 0)
                                    {
                                        eitem.frstarget = Math.Round(eitem.target * eitem.attribs.cnf_freshdaypercentage / 100, 0);
                                        eitem.oldtarget = eitem.target - Math.Round(eitem.target * eitem.attribs.cnf_freshdaypercentage / 100, 0);
                                        eitem.acheivedftarget = Math.Round(eitem.freshsales / eitem.frstarget * 100, 2);
                                        eitem.acheivedotarget = Math.Round(eitem.oldsales / eitem.oldtarget * 100, 2);
                                    }

                                    if (item.overrideincentive &&
                                        item.freshdaypercentage > 0)
                                    {
                                        eitem.frstarget = Math.Round(eitem.target * item.freshdaypercentage / 100, 0);
                                        eitem.oldtarget = eitem.target - Math.Round(eitem.target * item.freshdaypercentage / 100, 0);
                                        eitem.acheivedftarget = Math.Round(eitem.freshsales / eitem.frstarget * 100, 2);
                                        eitem.acheivedotarget = Math.Round(eitem.oldsales / eitem.oldtarget * 100, 2);
                                    }

                                    if (!item.overrideincentive &&
                                        eitem.attribs.eligiblepercentage > 0)
                                    {
                                        if (eitem.acheivedtarget < eitem.attribs.eligiblepercentage)
                                        {
                                            continue;
                                        }
                                        if (eitem.acheivedftarget < eitem.attribs.eligiblepercentage &&
                                            eitem.acheivedotarget < eitem.attribs.eligiblepercentage)
                                        {
                                            continue;
                                        }
                                    }

                                    if (item.overrideincentive &&
                                         item.eligiblepercentage > 0)
                                    {
                                        if (eitem.acheivedtarget < item.eligiblepercentage)
                                        {
                                            continue;
                                        }
                                        if (eitem.acheivedftarget < item.eligiblepercentage &&
                                            eitem.acheivedotarget < item.eligiblepercentage)
                                        {
                                            continue;
                                        }
                                    }

                                    if (!item.overrideincentive)
                                    {
                                        eitem.incentive = Math.Round(eitem.freshsales * eitem.attribs.inc_freshpercentage / 100);
                                        eitem.incentive += Math.Round(eitem.oldsales * eitem.attribs.inc_oldpercentage / 100);
                                    }
                                    else
                                    {
                                        eitem.incentive = Math.Round(eitem.freshsales * item.inc_freshpercentage / 100);
                                        eitem.incentive += Math.Round(eitem.oldsales * item.inc_oldpercentage / 100);
                                    }
                                }
                                else
                                {
                                    if (!item.overrideincentive)
                                        eitem.incentive = Math.Round(eitem.sales * eitem.attribs.inc_stdpercentage / 100);
                                    else
                                        eitem.incentive = Math.Round(eitem.sales * item.inc_stdpercentage / 100);
                                }
                            }
                        }
                        else if (sinfo.Count() > 1)
                        {
                            eitem.sales = sinfo.Sum(x => x.receivable);
                            eitem.hasmultisection = true;
                            if (eitem.target > 0)
                            {
                                if (sinfo.Exists(x => x.sectionid == gitem.Key.sectionid))
                                {
                                    sinfo = sinfo.FindAll(x => x.sectionid == gitem.Key.sectionid);
                                }
                                eitem.acheivedtarget = Math.Round(eitem.sales / eitem.target * 100, 2);
                                if (inentivemode > 0)
                                {
                                    eitem.freshsales = sinfo[0].freshsales;
                                    eitem.oldsales = sinfo[0].oldsales;

                                    if (!item.overrideincentive &&
                                        eitem.attribs.cnf_freshdaypercentage > 0)
                                    {
                                        eitem.frstarget = Math.Round(eitem.target * eitem.attribs.cnf_freshdaypercentage / 100, 0);
                                        eitem.oldtarget = eitem.target - Math.Round(eitem.target * eitem.attribs.cnf_freshdaypercentage / 100, 0);
                                        eitem.acheivedftarget = Math.Round(eitem.freshsales / eitem.frstarget * 100, 2);
                                        eitem.acheivedotarget = Math.Round(eitem.oldsales / eitem.oldtarget * 100, 2);
                                    }

                                    if (item.overrideincentive &&
                                        item.freshdaypercentage > 0)
                                    {
                                        eitem.frstarget = Math.Round(eitem.target * item.freshdaypercentage / 100, 0);
                                        eitem.oldtarget = eitem.target - Math.Round(eitem.target * item.freshdaypercentage / 100, 0);
                                        eitem.acheivedftarget = Math.Round(eitem.freshsales / eitem.frstarget * 100, 2);
                                        eitem.acheivedotarget = Math.Round(eitem.oldsales / eitem.oldtarget * 100, 2);
                                    }

                                    if (!item.overrideincentive &&
                                        eitem.attribs.eligiblepercentage > 0)
                                    {
                                        if (eitem.acheivedtarget < eitem.attribs.eligiblepercentage)
                                        {
                                            continue;
                                        }
                                        if (eitem.acheivedftarget < eitem.attribs.eligiblepercentage &&
                                            eitem.acheivedotarget < eitem.attribs.eligiblepercentage)
                                        {
                                            continue;
                                        }
                                    }

                                    if (item.overrideincentive &&
                                         item.eligiblepercentage > 0)
                                    {
                                        if (eitem.acheivedtarget < item.eligiblepercentage)
                                        {
                                            continue;
                                        }
                                        if (eitem.acheivedftarget < item.eligiblepercentage &&
                                            eitem.acheivedotarget < item.eligiblepercentage)
                                        {
                                            continue;
                                        }
                                    }

                                    if (!item.overrideincentive)
                                    {
                                        eitem.incentive = Math.Round(eitem.freshsales * eitem.attribs.inc_freshpercentage / 100);
                                        eitem.incentive += Math.Round(eitem.oldsales * eitem.attribs.inc_oldpercentage / 100);
                                    }
                                    else
                                    {
                                        eitem.incentive = Math.Round(eitem.freshsales * item.inc_freshpercentage / 100);
                                        eitem.incentive += Math.Round(eitem.oldsales * item.inc_oldpercentage / 100);
                                    }
                                }
                                else
                                {
                                    if (!item.overrideincentive)
                                        eitem.incentive = Math.Round(eitem.sales * eitem.attribs.inc_stdpercentage / 100);
                                    else
                                        eitem.incentive = Math.Round(eitem.sales * item.inc_stdpercentage / 100);
                                }
                            }
                        }
                    }
                    EmployeeDataService employeeDataService = new EmployeeDataService() { Context = this.Context };
                    var headers = employeeDataService.SearchHeader(new EmployeeCreteria() { sectionid = item.sectionid });
                    foreach (var titem in results)
                    {
                        var head = headers.Find(x => x.id == titem.employeeid);
                        if (head != null)
                        {
                            titem.employeecode = head.employeecode;
                            titem.name = head.name;
                            if (titem.hasmultisection)
                            {
                                titem.name = head.name + " (Multiple)";
                            }
                        }
                    }
                }
                else if (item.locationid == -10) //sectionbased
                {
                    var saleslist = salesInfos.GroupBy(x => new { x.locationid,x.month,x.year,x.sectionid }).Select(x => new
                    {
                        locationid = x.Key.locationid,
                        sectionid = x.Key.sectionid,
                        month = x.Key.month,
                        year = x.Key.year,
                        receivable = x.Sum(y => y.receivable),
                        oldsales = x.Sum(y => y.oldsales),
                        freshsales = x.Sum(y => y.freshsales)
                    });
                    foreach (var titem in saleslist)
                    {
                        if (results.Exists(x => x.locationid == titem.locationid
                            && x.sectionid == titem.sectionid 
                            && x.month == titem.month
                            && x.year == titem.year))
                        {
                            var eitem = results.Find(x => x.locationid == titem.locationid
                            && x.sectionid == titem.sectionid
                            && x.month == titem.month
                            && x.year == titem.year);
                            //eitem.freshsales = titem.freshsales;
                            //eitem.oldsales = titem.oldsales;
                            eitem.sales = titem.receivable;
                            if (eitem.target > 0)
                                eitem.acheivedtarget = Math.Round(eitem.sales / eitem.target * 100, 2);
                            else
                                eitem.acheivedtarget = 0;                            //eitem.frstarget = Math.Round(eitem.target * eitem.attribs.cnf_freshdaypercentage / 100, 0);
                            //eitem.oldtarget = eitem.target - Math.Round(eitem.target * eitem.attribs.cnf_freshdaypercentage / 100, 0);
                            //eitem.acheivedftarget = Math.Round(eitem.freshsales / eitem.frstarget * 100, 2);
                            //eitem.acheivedotarget = Math.Round(eitem.oldsales / eitem.oldtarget * 100, 2);
                        }
                        else
                        {
                            EmployeetargetDetail eitem = new EmployeetargetDetail() { locationid = titem.locationid,
                                sectionid = titem.sectionid,month =titem.month,year = titem.year };
                            //eitem.freshsales = titem.freshsales;
                            //eitem.oldsales = titem.oldsales;
                            eitem.sales = titem.receivable;
                            if (eitem.target > 0)
                                eitem.acheivedtarget = Math.Round(eitem.sales / eitem.target * 100, 2);
                            else
                                eitem.acheivedtarget = 0;
                            //eitem.frstarget = Math.Round(eitem.target * eitem.attribs.cnf_freshdaypercentage / 100, 0);
                            //eitem.oldtarget = eitem.target - Math.Round(eitem.target * eitem.attribs.cnf_freshdaypercentage / 100, 0);
                            //eitem.acheivedftarget = Math.Round(eitem.freshsales / eitem.frstarget * 100, 2);
                            //eitem.acheivedotarget = Math.Round(eitem.oldsales / eitem.oldtarget * 100, 2);
                            results.Add(eitem);
                        }
                    }
                }
                else if (item.locationid == -9) //locationbased
                {
                    var saleslist = salesInfos.GroupBy(x => new { x.locationid,x.month,x.year }).Select(x => new
                    {
                        locationid = x.Key.locationid,
                        month = x.Key.month,
                        year = x.Key.year,
                        receivable = x.Sum(y => y.receivable),
                        oldsales = x.Sum(y => y.oldsales),
                        freshsales = x.Sum(y => y.freshsales)
                    });
                    foreach (var titem in saleslist)
                    {
                        if(results.Exists(x=>x.locationid == titem.locationid && x.month == titem.month
                            && x.year == titem.year))
                        {
                            var eitem = results.Find(x => x.locationid == titem.locationid && x.month == titem.month
                            && x.year == titem.year);
                            //eitem.freshsales = titem.freshsales;
                            //eitem.oldsales = titem.oldsales;
                            eitem.sales = titem.receivable;
                            eitem.acheivedtarget = Math.Round(eitem.sales / eitem.target * 100, 2);
                            //eitem.frstarget = Math.Round(eitem.target * eitem.attribs.cnf_freshdaypercentage / 100, 0);
                            //eitem.oldtarget = eitem.target - Math.Round(eitem.target * eitem.attribs.cnf_freshdaypercentage / 100, 0);
                            //eitem.acheivedftarget = Math.Round(eitem.freshsales / eitem.frstarget * 100, 2);
                            //eitem.acheivedotarget = Math.Round(eitem.oldsales / eitem.oldtarget * 100, 2);
                        }
                        else
                        {
                            EmployeetargetDetail eitem = new EmployeetargetDetail() { locationid = titem.locationid, month = titem.month, year = titem.year };
                            //eitem.freshsales = titem.freshsales;
                            //eitem.oldsales = titem.oldsales;
                            eitem.sales = titem.receivable;
                            eitem.acheivedtarget = Math.Round(eitem.sales / eitem.target * 100, 2);
                            //eitem.frstarget = Math.Round(eitem.target * eitem.attribs.cnf_freshdaypercentage / 100, 0);
                            //eitem.oldtarget = eitem.target - Math.Round(eitem.target * eitem.attribs.cnf_freshdaypercentage / 100, 0);
                            //eitem.acheivedftarget = Math.Round(eitem.freshsales / eitem.frstarget * 100, 2);
                            //eitem.acheivedotarget = Math.Round(eitem.oldsales / eitem.oldtarget * 100, 2);
                            results.Add(eitem);
                        }
                    }
                }
            }
            return results;
        }

        internal List<EmployeetargetDetail> Build4Section(EmployeetargetCreteria item)
        {
            var response = new List<EmployeetargetDetail>();
            string query = @"select id,name from referencelist where type='DIVISION';
                             select id,name from referencelist where type='LOCATION'
                             and cast(additionalinfo->>'iswarehouse' as boolean)=false";

            if (item.locationlist != null && item.locationlist.Count > 0)
                query += " and referencelist.id in (" + string.Join(",", item.locationlist) + ")";

            var param = new SortedList<string, object>();
            param.Add("locationid", item.locationid);
            param.Add("fromdate", item.sourcefrom.Date);
            param.Add("todate", item.sourceto.Date.AddDays(1));

            List<EmployInfo> employInfos = new List<EmployInfo>();
            SortedDictionary<string, long> sections = new SortedDictionary<string, long>();
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    var name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                    var sectionid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    sections.Add(name, sectionid);
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        var name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        var locationid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        foreach (var sitem in sections)
                        {
                            EmployInfo employInfo = new EmployInfo();
                            employInfo.sectionid = sitem.Value;
                            employInfo.locationid = locationid;
                            employInfo.locationname = name;
                            employInfo.sectionname = sitem.Key;
                            employInfos.Add(employInfo);
                        }
                    }
                }
            }


            query = @"select date_part('month',billdate) ""month"",
            date_part('year',billdate) ""year"",
            salescounterid sectionid,bill.locationid,
            sum(billitems.receivable) receivable,
            sum(billitems.qty*salablegoods.price) 
            beforediscount from billitems
            inner join bill on bill.id = billid
            inner join salablegoods on salablegoods.id = salablegoodsid
            inner join stock on stock.id = salablegoods.stockid
            inner join products on products.id = stock.productid 
            where billdate>=@fromdate and billdate<@todate AND billitems.ISACTIVE=true
            and bill.billtype != 9 and bill.isactive=true {0}
            group by date_part('month',billdate),date_part('year',billdate),salescounterid,bill.locationid";

            if (item.locationlist != null && item.locationlist.Count > 0)
                query = String.Format(query, " and bill.locationid in  (" + string.Join(",", item.locationlist) + ")");
            else
                query = String.Format(query, "");

            List<SalesInfo> salesInfos = new List<SalesInfo>();
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    SalesInfo salesInfo = new SalesInfo();
                    salesInfo.sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                    salesInfo.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                    salesInfo.month = reader["month"] == DBNull.Value ? 0 : Convert.ToInt32(reader["month"]);
                    salesInfo.year = reader["year"] == DBNull.Value ? 0 : Convert.ToInt32(reader["year"]);
                    if (item.billsource == 1)
                    {
                        salesInfo.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                    }
                    else
                    {
                        salesInfo.receivable = reader["beforediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["beforediscount"]);
                    }
                    salesInfos.Add(salesInfo);
                }
            }


            var fromdate = item.targetfrom.Date;
            var sfromdate = item.sourcefrom.Date;
            while (fromdate < item.targetto.Date)
            {
                int year = fromdate.Year;
                int month = fromdate.Month;
                foreach (var eitem in employInfos)
                {
                    var eidetail = new EmployeetargetDetail()
                    {
                        month = month,
                        year = year,
                        version = 1,
                        isactive = true,
                        sectionid = eitem.sectionid,
                        locationid = eitem.locationid,
                        attribs = new Employeetarget.Attribs()
                        {
                            cnf_freshdate = item.freshdate,
                            cnf_freshday = item.freshday,
                            cnf_monthdays = item.monthdays,
                            incentivemode = item.incentivemode,
                            eligiblepercentage = item.eligiblepercentage,
                            cnf_freshdaypercentage = item.freshdaypercentage,
                            inc_freshpercentage = item.inc_freshpercentage,
                            inc_oldpercentage = item.inc_oldpercentage,
                            inc_stdpercentage = item.inc_stdpercentage,
                        }
                    };
                    SalesInfo si = salesInfos.Find(x => x.sectionid == eitem.sectionid && x.locationid == eitem.locationid &&
                    x.month == sfromdate.Month && x.year == sfromdate.Year);
                    if (si != null)
                    {
                        eidetail.sales = si.receivable;
                        eidetail.target = si.receivable;
                        if (eidetail.target > 0)
                        {
                            if (item.targetpercentage > 0)
                                eidetail.target = Math.Round(eidetail.target + (eidetail.target * item.targetpercentage / 100), 0);

                            if (item.monthdays > 0)
                            {
                                eidetail.dailysales = Math.Round(eidetail.salary / item.monthdays, 0);
                                eidetail.dailytarget = Math.Round(eidetail.target / item.monthdays, 0);
                            }
                        }
                        response.Add(eidetail);
                    }
                }
                fromdate = fromdate.AddMonths(1);
                sfromdate = sfromdate.AddMonths(1);
            }

            var existing = Search(new EmployeetargetCreteria()
            {
                datebased = true,
                targetfrom = item.targetfrom.Date,
                targetto = item.targetto.Date
            });

            if (existing.Count > 0)
            {
                foreach (var eitem in existing)
                {
                    var citem = response.Find(x => x.locationid == eitem.locationid &&
                    x.sectionid == eitem.sectionid &&
                    x.month == eitem.month && x.year == eitem.year);
                    if (citem != null)
                    {
                        citem.otarget = eitem.target;
                        citem.id = eitem.id;
                        citem.createdby = eitem.createdby;
                        citem.createdon = eitem.createdon;
                    }
                }
            }

            return response;
        }

        internal List<EmployeetargetDetail> Build4Locations(EmployeetargetCreteria item)
        {
            var response = new List<EmployeetargetDetail>();
            string query = @"select id,name from referencelist where type='LOCATION' and cast(additionalinfo->>'iswarehouse' as boolean)=false";

            if (item.locationlist != null && item.locationlist.Count > 0)
                query += " and referencelist.id in (" + string.Join(",", item.locationlist) + ")";

            var param = new SortedList<string, object>();
            param.Add("locationid", item.locationid);
            param.Add("fromdate", item.sourcefrom.Date);
            param.Add("todate", item.sourceto.Date.AddDays(1));

            List<EmployInfo> employInfos = new List<EmployInfo>();
            SortedDictionary<string, long> sections = new SortedDictionary<string, long>();
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    var name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                    var locationid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);

                    EmployInfo employInfo = new EmployInfo();
                    employInfo.locationid = locationid;
                    employInfo.locationname = name;
                    employInfos.Add(employInfo);
                }
            }


            query = @"select date_part('month',billdate) ""month"",
            date_part('year',billdate) ""year"",bill.locationid,
            sum(billitems.receivable) receivable,
            sum(billitems.qty*salablegoods.price) 
            beforediscount from billitems
            inner join bill on bill.id = billid
            inner join salablegoods on salablegoods.id = salablegoodsid
            where billdate>=@fromdate and billdate<@todate AND billitems.ISACTIVE=true
            and bill.billtype != 9 and bill.isactive=true {0}
            group by date_part('month',billdate),date_part('year',billdate),bill.locationid";

            if (item.locationlist != null && item.locationlist.Count > 0)
                query = String.Format(query, " and bill.locationid in  (" + string.Join(",", item.locationlist) + ")");
            else
                query = String.Format(query, "");

            List<SalesInfo> salesInfos = new List<SalesInfo>();
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    SalesInfo salesInfo = new SalesInfo();
                    salesInfo.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                    salesInfo.month = reader["month"] == DBNull.Value ? 0 : Convert.ToInt32(reader["month"]);
                    salesInfo.year = reader["year"] == DBNull.Value ? 0 : Convert.ToInt32(reader["year"]);
                    if (item.billsource == 1)
                    {
                        salesInfo.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                    }
                    else
                    {
                        salesInfo.receivable = reader["beforediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["beforediscount"]);
                    }
                    salesInfos.Add(salesInfo);
                }
            }


            var fromdate = item.targetfrom.Date;
            var sfromdate = item.sourcefrom.Date;
            while (fromdate < item.targetto.Date)
            {
                int year = fromdate.Year;
                int month = fromdate.Month;
                foreach (var eitem in employInfos)
                {
                    var eidetail = new EmployeetargetDetail()
                    {
                        month = month,
                        year = year,
                        version = 1,
                        isactive = true,
                        locationid = eitem.locationid,
                        attribs = new Employeetarget.Attribs()
                        {
                            cnf_freshdate = item.freshdate,
                            cnf_freshday = item.freshday,
                            cnf_monthdays = item.monthdays,
                            incentivemode = item.incentivemode,
                            eligiblepercentage = item.eligiblepercentage,
                            cnf_freshdaypercentage = item.freshdaypercentage,
                            inc_freshpercentage = item.inc_freshpercentage,
                            inc_oldpercentage = item.inc_oldpercentage,
                            inc_stdpercentage = item.inc_stdpercentage,
                        }
                    };
                    SalesInfo si = salesInfos.Find(x => x.locationid == eitem.locationid &&
                    x.month == sfromdate.Month && x.year == sfromdate.Year);
                    if (si != null)
                    {
                        eidetail.sales = si.receivable;
                        eidetail.target = si.receivable;
                        if (eidetail.target > 0)
                        {
                            if (item.targetpercentage > 0)
                                eidetail.target = Math.Round(eidetail.target + (eidetail.target * item.targetpercentage / 100), 0);

                            if (item.monthdays > 0)
                            {
                                eidetail.dailysales = Math.Round(eidetail.salary / item.monthdays, 0);
                                eidetail.dailytarget = Math.Round(eidetail.target / item.monthdays, 0);
                            }
                        }
                        response.Add(eidetail);
                    }
                }
                fromdate = fromdate.AddMonths(1);
                sfromdate = sfromdate.AddMonths(1);
            }

            var existing = Search(new EmployeetargetCreteria()
            {
                datebased = true,
                targetfrom = item.targetfrom.Date,
                targetto = item.targetto.Date
            });

            if (existing.Count > 0)
            {
                foreach (var eitem in existing)
                {
                    var citem = response.Find(x => x.locationid == eitem.locationid &&
                    x.sectionid == 0 && x.employeeid == 0 &&
                    x.month == eitem.month && x.year == eitem.year);
                    if (citem != null)
                    {
                        citem.otarget = eitem.target;
                        citem.id = eitem.id;
                        citem.createdby = eitem.createdby;
                        citem.createdon = eitem.createdon;
                    }
                }
            }

            return response;
        }

        internal List<EmployeetargetDetail> Build(EmployeetargetCreteria item)
        {
            var response = new List<EmployeetargetDetail>();
            string query = @"with salaryheader as (SELECT id, employeeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber FROM employeesalaryheader)
select employee.id employeeid,employee.name,employee.employeecode,
employeesalaryheader.grosspay,sectionid from employee
left outer join salaryheader on salaryheader.employeeid = employee.id 
left outer join employeesalaryheader on employeesalaryheader.id = salaryheader.id
inner join referencelist on referencelist.id = employee.departmentid
where employee.isactive=true and (salaryheader.ranknumber = 1 or salaryheader.ranknumber is null)
and  cast(referencelist.additionalinfo->>'issales' as varchar) = 'true'
and (employee.hasleft = false or employee.relievingdate>=current_date)";

            if (item.locationid > 0)
                query += " and employee.locationid=@locationid";
            var param = new SortedList<string, object>();
            param.Add("locationid", item.locationid);
            param.Add("fromdate", item.sourcefrom.Date);
            param.Add("todate", item.sourceto.Date.AddDays(1));

            List<EmployInfo> employInfos = new List<EmployInfo>();
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    EmployInfo employInfo = new EmployInfo();
                    employInfo.sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                    employInfo.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                    employInfo.name = reader["name"] == DBNull.Value ? "" : Convert.ToString(reader["name"]);
                    employInfo.employeecode = reader["employeecode"] == DBNull.Value ? "" : Convert.ToString(reader["employeecode"]);
                    employInfo.grosspay = reader["grosspay"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grosspay"]);
                    employInfos.Add(employInfo);
                }
            }

            if (item.sectionids != null && item.sectionids.Count > 0)
                employInfos.RemoveAll(x => !item.sectionids.Contains(x.sectionid));

            if (item.sectionid > 0)
                employInfos.RemoveAll(x => item.sectionid != x.sectionid);

            foreach (var eitem in employInfos.GroupBy(x => x.sectionid))
            {
                var totalgross = eitem.Sum(x => x.grosspay);
                foreach (var emp in eitem)
                {
                    if (emp.grosspay > 0)
                    {
                        emp.sectionratio = Math.Round(emp.grosspay / totalgross * 100, 2);
                    }
                }
            }

            query = @"select date_part('month',billdate) ""month"",
            date_part('year',billdate) ""year"",
            salescounterid sectionid,bill.locationid,
            sum(billitems.receivable) receivable,
            sum(billitems.qty*salablegoods.price) 
            beforediscount from billitems
            inner join bill on bill.id = billid
            inner join salablegoods on salablegoods.id = salablegoodsid
            inner join stock on stock.id = salablegoods.stockid
            inner join products on products.id = stock.productid 
            where billdate>=@fromdate and billdate<@todate AND billitems.ISACTIVE=true
            and bill.billtype != 9 and bill.isactive=true {0}
            group by date_part('month',billdate),date_part('year',billdate),salescounterid,bill.locationid";

            if (item.locationid > 0)
                query = String.Format(query, " and bill.locationid = @locationid");
            else
                query = String.Format(query, "");

            List<SalesInfo> salesInfos = new List<SalesInfo>();
            using (var reader = ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    SalesInfo salesInfo = new SalesInfo();
                    salesInfo.sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                    salesInfo.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                    salesInfo.month = reader["month"] == DBNull.Value ? 0 : Convert.ToInt32(reader["month"]);
                    salesInfo.year = reader["year"] == DBNull.Value ? 0 : Convert.ToInt32(reader["year"]);
                    if (item.billsource == 1)
                    {
                        salesInfo.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                    }
                    else
                    {
                        salesInfo.receivable = reader["beforediscount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["beforediscount"]);
                    }
                    salesInfos.Add(salesInfo);
                }
            }

            //foreach (var eitem in salesInfos.GroupBy(x => x.sectionid))
            //{
            //    var totalreceivable = eitem.Sum(x => x.receivable);
            //    var totalbeforediscount = eitem.Sum(x => x.beforediscount);
            //    foreach (var emp in eitem)
            //    {
            //        if(item.billsource && emp.receivable>0)
            //        {
            //            emp.sectionratio = Math.Round(emp.receivable / totalbeforediscount * 100, 2);
            //        }
            //        if (!item.billsource && emp.beforediscount > 0)
            //        {
            //            emp.sectionratio = Math.Round(emp.beforediscount / totalbeforediscount * 100, 2);
            //        }
            //    }
            //}

            var fromdate = item.targetfrom.Date;
            var sfromdate = item.sourcefrom.Date;
            while (fromdate < item.targetto.Date)
            {
                int year = fromdate.Year;
                int month = fromdate.Month;
                foreach (var eitem in employInfos)
                {
                    var eidetail = new EmployeetargetDetail()
                    {
                        month = month,
                        year = year,
                        version = 1,
                        isactive = true,
                        sectionid = eitem.sectionid,
                        employeeid = eitem.employeeid,
                        name = eitem.name,
                        employeecode = eitem.employeecode,
                        salary = eitem.grosspay,
                        ratio = eitem.sectionratio,
                        attribs = new Employeetarget.Attribs()
                        {
                            cnf_freshdate = item.freshdate,
                            cnf_freshday = item.freshday,
                            cnf_monthdays = item.monthdays,
                            incentivemode = item.incentivemode,
                            eligiblepercentage = item.eligiblepercentage,
                            cnf_freshdaypercentage = item.freshdaypercentage,
                            inc_freshpercentage = item.inc_freshpercentage,
                            inc_oldpercentage = item.inc_oldpercentage,
                            inc_stdpercentage = item.inc_stdpercentage,
                        }
                    };
                    SalesInfo si = salesInfos.Find(x => x.sectionid == eitem.sectionid &&
                    x.month == sfromdate.Month && x.year == sfromdate.Year);
                    if (si != null)
                    {
                        eidetail.sales = si.receivable;
                        eidetail.target = Math.Round(si.receivable * eitem.sectionratio / 100, 2);
                        if (eidetail.target > 0)
                        {
                            if (item.targetpercentage > 0)
                                eidetail.target = Math.Round(eidetail.target + (eidetail.target * item.targetpercentage / 100), 0);

                            if (item.monthdays > 0)
                            {
                                eidetail.dailysales = Math.Round(eidetail.salary / item.monthdays, 0);
                                eidetail.dailytarget = Math.Round(eidetail.target / item.monthdays, 0);
                            }
                        }
                        response.Add(eidetail);
                    }
                }
                fromdate = fromdate.AddMonths(1);
                sfromdate = sfromdate.AddMonths(1);
            }

            var existing = Search(new EmployeetargetCreteria()
            {
                datebased = true,
                targetfrom = item.targetfrom.Date,
                targetto = item.targetto.Date
            });

            if (existing.Count > 0)
            {
                foreach (var eitem in existing)
                {
                    var citem = response.Find(x => x.employeeid == eitem.employeeid &&
                    x.month == eitem.month && x.year == eitem.year);
                    if (citem != null)
                    {
                        citem.otarget = eitem.target;
                        citem.id = eitem.id;
                        citem.createdby = eitem.createdby;
                        citem.createdon = eitem.createdon;
                    }
                }
            }

            return response;
        }


        public class EmployInfo
        {
            public long employeeid { get; set; }
            public string name { get; set; }
            public string employeecode { get; set; }
            public decimal sectionratio { get; set; }
            public decimal grosspay { get; set; }
            public long sectionid { get; set; }
            public long locationid { get; set; }
            public string sectionname { get; set; }
            public string locationname { get; set; }
        }
        public class SalesInfo
        {
            public int month { get; set; }
            public int year { get; set; }
            public long locationid { get; set; }
            public long sectionid { get; set; }
            public long salesmanid { get; set; }
            public decimal receivable { get; set; }
            public decimal freshsales { get; set; }
            public decimal oldsales { get; set; }
        }
    }

}
