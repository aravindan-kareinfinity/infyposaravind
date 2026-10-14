using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class EmployeesalaryheaderDataService : SAASData
    {
        const string SQL_Search = @"select id,employeeid, fromdate, iscurrent, basicpay, eligibleesi, eligiblepf, configuration, isactive,incentiveratio,salarystructureid, createdby, modifiedby, version, createdon, modifiedon, pfselfpaid, esiselfpaid, grosspay,hasarriyar,applyfrom,lastgross,inchargeid,ctc from employeesalaryheader where id=@id";
        const string SQL_SearchBySection = @"with salaryheader as (
                                            SELECT id, employeeid, fromdate, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber FROM employeesalaryheader)
                                            select employeesalaryheader.id,employeesalaryheader.version, salaryheader.ranknumber,employee.id employeeid,employee.name,employee.employeecode,employee.gender,
                                            employeesalaryheader.basicpay,employee.dateofjoining, employeesalaryheader.grosspay,employeesalarystructure.name structurename, pfselfpaid,esiselfpaid,eligibleesi,eligiblepf,
                                            salaryheader.id salaryheaderid,salaryheader.fromdate,incentiveratio,salarystructureid,hasarriyar,applyfrom,inchargeid,employeesalaryheader.ctc,
                                            employeesalaryheader.incentivesource,employeesalaryheader.incentivepercentage,employeesalaryheader.salestargetmethod,
                                            employeesalaryheader.incentivevaluetype,employeesalaryheader.incentivecalculationsource,employeesalaryheader.salestargetminimummode,
                                            employeesalaryheader.salesminimumtarget,salestarget,lastgross {0} from employee
                                            left outer join salaryheader on salaryheader.employeeid = employee.id 
                                            left outer join employeesalaryheader on employeesalaryheader.id = salaryheader.id
                                            left outer join employeesalarystructure on employeesalarystructure.id = salarystructureid
                                            where employee.isactive=true and (salaryheader.ranknumber = 1 or salaryheader.ranknumber is null)";

        const string SQL_Insert = @"INSERT INTO public.employeesalaryheader(employeeid, fromdate, iscurrent, basicpay, eligibleesi, eligiblepf, configuration, isactive, createdby, modifiedby, version, createdon, modifiedon, pfselfpaid, esiselfpaid, grosspay,incentiveratio,salarystructureid,hasarriyar,applyfrom,lastgross,inchargeid,ctc)
                                    VALUES(@employeeid, @fromdate, @iscurrent, @basicpay, @eligibleesi, @eligiblepf, @configuration, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon, @pfselfpaid, @esiselfpaid, @grosspay,@incentiveratio,@salarystructureid,@hasarriyar,@applyfrom,@lastgross,@inchargeid,@ctc) RETURNING id";


        const string SQL_Update = @"UPDATE public.employeesalaryheader SET employeeid=@employeeid, fromdate=@fromdate,salarystructureid=@salarystructureid, iscurrent=@iscurrent, basicpay=@basicpay, eligibleesi=@eligibleesi, eligiblepf=@eligiblepf, configuration=@configuration,
                                    modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon, pfselfpaid=@pfselfpaid, esiselfpaid=@esiselfpaid, grosspay=@grosspay,ctc=@ctc,
                                    hasarriyar=@hasarriyar,applyfrom=@applyfrom,lastgross=@lastgross,inchargeid=@inchargeid where id=@id and version=@version RETURNING version";



        const string SQL_UpdateIncentive = @"UPDATE public.employeesalaryheader SET incentiveratio=@incentiveratio, modifiedby=@modifiedby, modifiedon=@modifiedon,incentivepercentage=@incentivepercentage,
        incentivesource=@incentivesource,salestargetmethod=@salestargetmethod,incentivevaluetype=@incentivevaluetype,incentivecalculationsource=@incentivecalculationsource,salestarget=@salestarget,
        salestargetminimummode=@salestargetminimummode,salesminimumtarget=@salesminimumtarget where id in ({0});";

        const string SQL_UpdateIncharges = @"UPDATE public.employeesalaryheader SET inchargeid=@inchargeid where id in ({0});";

        const string SQL_UpdateESI = @"UPDATE public.employeesalaryheader SET eligibleesi=@eligibleesi,esiselfpaid=@esiselfpaid, modifiedby=@modifiedby, modifiedon=@modifiedon where id in ({0});";

        const string SQL_UpdatePF = @"UPDATE public.employeesalaryheader SET eligiblepf=@eligiblepf,pfselfpaid=@pfselfpaid,modifiedby=@modifiedby, modifiedon=@modifiedon where id in ({0});";


        internal bool SaveIncentiveConfig(IncentiveConfig oEmployeesalaryheader)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    List<long> empids = oEmployeesalaryheader.ids.FindAll(e => e.salaryheaderid == 0).ConvertAll(e => e.employeeid);
                    List<long> shids = oEmployeesalaryheader.ids.FindAll(e => e.salaryheaderid > 0).ConvertAll(e => e.salaryheaderid);

                    foreach (var id in empids)
                    {
                        var item = Insert(transaction, conn, new Employeesalaryheader()
                        {
                            employeeid = id,
                            fromdate = DateTimeNow.Date.AddMonths(-6),
                            createdby = Context.UserID,
                            modifiedby = Context.UserID,
                            createdon = DateTimeNow,
                            modifiedon = DateTimeNow,
                            version = 1,
                            isactive = true
                        });
                        shids.Add(item.id);
                    }

                    string updatequery = "";
                    if (oEmployeesalaryheader.changeincentiveconfig)
                        updatequery = SQL_UpdateIncentive;
                    if (oEmployeesalaryheader.changesupervisor)
                        updatequery += SQL_UpdateIncharges;

                    if (string.IsNullOrEmpty(updatequery)) return false;

                    NpgsqlCommand command = GetCommand(string.Format(updatequery, string.Join(",", shids)), conn, transaction);
                    command.Parameters.Add("incentiveratio", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalaryheader.incentiveratio;
                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("inchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryheader.supervisorid;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                    command.Parameters.Add("incentivesource", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalaryheader.incentivesource;
                    command.Parameters.Add("incentivecalculationsource", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalaryheader.incentivecalculationsource;
                    command.Parameters.Add("salestargetmethod", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalaryheader.salestargetmethod;
                    command.Parameters.Add("incentivevaluetype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalaryheader.incentivevaluetype;
                    command.Parameters.Add("salestargetminimummode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalaryheader.salestargetminimummode;
                    command.Parameters.Add("incentivepercentage", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalaryheader.incentivepercentage;
                    command.Parameters.Add("salesminimumtarget", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalaryheader.salesminimumtarget;
                    command.Parameters.Add("salestarget", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalaryheader.salestarget;
                    command.ExecuteNonQuery();


                    transaction.Commit();
                    conn.Close();
                }
                return true;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeesalaryheader);
            }
            return false;
        }


        internal List<EmployeeSalaryInfo> SaveAll(List<EmployeeSalaryInfo> items)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                if (items == null) return new List<EmployeeSalaryInfo>();
                items.RemoveAll(e => e.employeeid == 0);

                var existings = SearchALL(new EmployeesalaryheaderCreteria()
                {


                });

                SalaryManager salaryManager = new SalaryManager() { Context = this.Context };
                salaryManager.Initialize(DateTimeNow.Month, DateTimeNow.Year);

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var item in items)
                    {
                        salaryManager.TestMonthly(item);
                        if (item.id <= 0)
                        {
                            item.eligibleesi = true;
                            item.eligiblepf = true;
                            item.ChangeStatus(BaseEntity.DataStatus.Create, Context);
                            //item.fromdate = new DateTime(DateTimeNow.Year,DateTimeNow.Month,1);
                            Insert(transaction, conn, item);
                        }
                        else
                        {
                            if (existings.Exists(e => e.employeeid == item.employeeid))
                            {
                                var exist = existings.Find(e => e.employeeid == item.employeeid);
                                item.id = exist.id;
                                item.pfselfpaid = exist.pfselfpaid;
                                item.esiselfpaid = exist.esiselfpaid;
                                item.eligibleesi = exist.eligibleesi;
                                item.eligiblepf = exist.eligiblepf;
                                if (exist.inchargeid <= 0)
                                    item.inchargeid = exist.inchargeid;
                                if (item.fromdate <= DateTime.MinValue)
                                    item.fromdate = exist.fromdate;
                                item.incentivesource = exist.incentivesource;
                                item.salestargetmethod = exist.salestargetmethod;
                                if (item.ctc <= 0)
                                    item.ctc = exist.ctc;
                                if (item.grosspay <= 0)
                                {
                                    item.grosspay = exist.grosspay;
                                    if (item.ctc <= 0)
                                        item.ctc = exist.grosspay;
                                }
                                item.incentiveratio = exist.incentiveratio;
                                item.incentivepercentage = exist.incentivepercentage;
                                if(item.salarystructureid<=0)
                                    item.salarystructureid = exist.salarystructureid;
                                item.salestarget = exist.salestarget;
                                item.createdby = item.createdby;
                                item.createdon = item.createdon;
                                item.modifiedby = Context.UserID;
                                item.modifiedon = DateTimeNow;
                                item.version = exist.version;
                                item.ChangeStatus(BaseEntity.DataStatus.Update, Context);
                                Update(transaction, conn, item);
                            }
                            else
                            {
                                item.id = 0;
                                item.version = 1;
                                item.ChangeStatus(BaseEntity.DataStatus.Create, Context);
                                Insert(transaction, conn, item);
                            }
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
                return null;
            }
            return items;

        }

        const string SQL_Delete = @"Delete from employeesalaryheader where id=@id and version=@version";

        const string SQL_MultipleDelete = @"Delete from employeesalaryheader where fromdate=@fromdate and hasarriyar=@hasarriyar and applyfrom=@applyfrom";

        public List<EmployeeSalaryInfo> SearchALL(EmployeesalaryheaderCreteria _employeesalaryheader)
        {
            List<EmployeeSalaryInfo> lst = new List<EmployeeSalaryInfo>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = string.Format(SQL_SearchBySection, _employeesalaryheader.showconfig ? ",employeesalaryheader.configuration" : "");
                // qb.AddParameter("employee.isactive", true, "=");

                if (_employeesalaryheader.showallyears)
                    qb.CurrentQuery = qb.CurrentQuery.Replace("and (salaryheader.ranknumber = 1 or salaryheader.ranknumber is null)", "");
                else if (_employeesalaryheader.showunassigned)
                    qb.CurrentQuery = qb.CurrentQuery.Replace("and (salaryheader.ranknumber = 1 or salaryheader.ranknumber is null)", " and salaryheader.ranknumber is null");

                if (_employeesalaryheader.departmentid > 0)
                {
                    qb.AddParameter("departmentid", _employeesalaryheader.departmentid, "=");
                }

                if (_employeesalaryheader.companyid > 0)
                {
                    qb.AddParameter("employee.companyid", _employeesalaryheader.companyid, "=");
                }

                if (_employeesalaryheader.sectionid > 0)
                {
                    qb.AddParameter("sectionid", _employeesalaryheader.sectionid, "=");
                }

                if (_employeesalaryheader.workingmode > 0)
                {
                    qb.AddParameter("employee.workingmode", _employeesalaryheader.workingmode, "=");
                }

                if (_employeesalaryheader.salarymode > 0)
                {
                    qb.AddParameter("employee.salarymode", _employeesalaryheader.salarymode, "=");
                }

                if (!_employeesalaryheader.showallyears)
                    qb.AddParameter("employee.hasleft", _employeesalaryheader.showhasleft, "=");

                if (_employeesalaryheader.dojfromdate.Year > 1900)
                {
                    qb.AddParameter("employee.dateofjoining", _employeesalaryheader.dojfromdate, ">=");
                }

                if (_employeesalaryheader.dojtodate.Year > 1900)
                {
                    qb.AddParameter("employee.dateofjoining", _employeesalaryheader.dojtodate.AddDays(1), "<");
                }

                if (_employeesalaryheader.employeeid > 0)
                {
                    qb.AddParameter("employee.id", _employeesalaryheader.employeeid, "=");
                }

                if (!string.IsNullOrEmpty(_employeesalaryheader.employeecode))
                {
                    qb.AddParameter("employeecode", _employeesalaryheader.employeecode, "=");
                }

                if (_employeesalaryheader.showpf)
                    qb.AddParameter("pfselfpaid", _employeesalaryheader.showpf, "=");
                if (_employeesalaryheader.showpf)
                    qb.AddParameter("esiselfpaid", _employeesalaryheader.showesi, "=");
                if (_employeesalaryheader.showpf)
                    qb.AddParameter("eligibleesi", _employeesalaryheader.pfeligible, "=");
                if (_employeesalaryheader.showpf)
                    qb.AddParameter("eligiblepf", _employeesalaryheader.esieligible, "=");


                if (Context.accessiblelocation != null && !Context.accessiblelocation.Exists(x => x == -1))
                {
                    qb.AddInParamer("employee.locationid", string.Join(",", Context.accessiblelocation));
                }

                qb.SortField = @"case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";

                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        EmployeeSalaryInfo oemployeesalaryheader = new EmployeeSalaryInfo();
                        oemployeesalaryheader.salaryheaderid = reader["salaryheaderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salaryheaderid"]);
                        oemployeesalaryheader.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        //oemployeesalaryheader.designationid = reader["designationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["designationid"]);
                        oemployeesalaryheader.basicpay = reader["basicpay"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basicpay"]);
                        oemployeesalaryheader.grosspay = reader["grosspay"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grosspay"]);

                        oemployeesalaryheader.ctc = reader["ctc"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ctc"]);

                        oemployeesalaryheader.incentiveratio = reader["incentiveratio"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentiveratio"]);
                        oemployeesalaryheader.name = reader["name"] == DBNull.Value ? null : reader["name"].ToString();
                        oemployeesalaryheader.employeecode = reader["employeecode"] == DBNull.Value ? null : reader["employeecode"].ToString();
                        oemployeesalaryheader.fromdate = reader["fromdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["fromdate"]);
                        oemployeesalaryheader.dateofjoining = reader["dateofjoining"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofjoining"]);
                        oemployeesalaryheader.salarystructureid = reader["salarystructureid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salarystructureid"]);
                        oemployeesalaryheader.structurename = reader["structurename"] == DBNull.Value ? null : reader["structurename"].ToString();
                        oemployeesalaryheader.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oemployeesalaryheader.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oemployeesalaryheader.eligibleesi = reader["eligibleesi"] == DBNull.Value ? false : Convert.ToBoolean(reader["eligibleesi"]);
                        oemployeesalaryheader.eligiblepf = reader["eligiblepf"] == DBNull.Value ? false : Convert.ToBoolean(reader["eligiblepf"]);
                        oemployeesalaryheader.pfselfpaid = reader["pfselfpaid"] == DBNull.Value ? false : Convert.ToBoolean(reader["pfselfpaid"]);
                        oemployeesalaryheader.esiselfpaid = reader["esiselfpaid"] == DBNull.Value ? false : Convert.ToBoolean(reader["esiselfpaid"]);

                        oemployeesalaryheader.hasarriyar = reader["hasarriyar"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasarriyar"]);
                        oemployeesalaryheader.lastgross = reader["lastgross"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["lastgross"]);
                        oemployeesalaryheader.inchargeid = reader["inchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["inchargeid"]);
                        oemployeesalaryheader.applyfrom = reader["applyfrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["applyfrom"]);

                        oemployeesalaryheader.incentivepercentage = reader["incentivepercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentivepercentage"]);
                        oemployeesalaryheader.incentivesource = reader["incentivesource"] == DBNull.Value ? 0 : Convert.ToInt32(reader["incentivesource"]);
                        oemployeesalaryheader.salestargetmethod = reader["salestargetmethod"] == DBNull.Value ? 0 : Convert.ToInt32(reader["salestargetmethod"]);
                        oemployeesalaryheader.salestarget = reader["salestarget"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestarget"]);

                        oemployeesalaryheader.incentivecalculationsource = reader["incentivecalculationsource"] == DBNull.Value ? 0 : Convert.ToInt32(reader["incentivecalculationsource"]);
                        oemployeesalaryheader.incentivevaluetype = reader["incentivevaluetype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["incentivevaluetype"]);
                        oemployeesalaryheader.salestargetminimummode = reader["salestargetminimummode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["salestargetminimummode"]);
                        oemployeesalaryheader.salesminimumtarget = reader["salesminimumtarget"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salesminimumtarget"]);

                        if (_employeesalaryheader.showconfig)
                            oemployeesalaryheader.configuration_json = reader["configuration"] == DBNull.Value ? null : reader["configuration"].ToString();
                        lst.Add(oemployeesalaryheader);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _employeesalaryheader);
            }
            return lst;
        }


        public List<Employeesalaryheader> Search(EmployeesalaryheaderCreteria _employeesalaryheader)
        {
            List<Employeesalaryheader> lst = new List<Employeesalaryheader>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_employeesalaryheader.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _employeesalaryheader.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");
                    if (Context.accessiblelocation != null && !Context.accessiblelocation.Exists(x => x == -1))
                    {
                        qb.AddInParamer("employee.locationid", string.Join(",", Context.accessiblelocation));
                    }
                    if (_employeesalaryheader.employeeid > 0)
                        qb.AddParameter("employeeid", _employeesalaryheader.employeeid, "=");
                    else
                        return new List<Employeesalaryheader>();
                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employeesalaryheader oemployeesalaryheader = new Employeesalaryheader();
                        oemployeesalaryheader.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oemployeesalaryheader.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        oemployeesalaryheader.fromdate = reader["fromdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["fromdate"]);
                        oemployeesalaryheader.iscurrent = reader["iscurrent"] == DBNull.Value ? false : Convert.ToBoolean(reader["iscurrent"]);
                        oemployeesalaryheader.basicpay = reader["basicpay"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["basicpay"]);
                        oemployeesalaryheader.incentiveratio = reader["incentiveratio"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentiveratio"]);
                        oemployeesalaryheader.eligibleesi = reader["eligibleesi"] == DBNull.Value ? false : Convert.ToBoolean(reader["eligibleesi"]);
                        oemployeesalaryheader.eligiblepf = reader["eligiblepf"] == DBNull.Value ? false : Convert.ToBoolean(reader["eligiblepf"]);
                        oemployeesalaryheader.configuration_json = reader["configuration"] == DBNull.Value ? null : reader["configuration"].ToString();
                        oemployeesalaryheader.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oemployeesalaryheader.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oemployeesalaryheader.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oemployeesalaryheader.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oemployeesalaryheader.ctc = reader["ctc"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["ctc"]);

                        oemployeesalaryheader.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oemployeesalaryheader.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oemployeesalaryheader.pfselfpaid = reader["pfselfpaid"] == DBNull.Value ? false : Convert.ToBoolean(reader["pfselfpaid"]);
                        oemployeesalaryheader.esiselfpaid = reader["esiselfpaid"] == DBNull.Value ? false : Convert.ToBoolean(reader["esiselfpaid"]);
                        oemployeesalaryheader.grosspay = reader["grosspay"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grosspay"]);
                        oemployeesalaryheader.salarystructureid = reader["salarystructureid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salarystructureid"]);
                        oemployeesalaryheader.hasarriyar = reader["hasarriyar"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasarriyar"]);
                        oemployeesalaryheader.lastgross = reader["lastgross"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["lastgross"]);
                        oemployeesalaryheader.inchargeid = reader["inchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["inchargeid"]);
                        oemployeesalaryheader.applyfrom = reader["applyfrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["applyfrom"]);

                        //oemployeesalaryheader.incentivepercentage = reader["incentivepercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentivepercentage"]);
                        //oemployeesalaryheader.incentivesource = reader["incentivesource"] == DBNull.Value ? 0 : Convert.ToInt32(reader["incentivesource"]);
                        //oemployeesalaryheader.salestargetmethod = reader["salestargetmethod"] == DBNull.Value ? 0 : Convert.ToInt32(reader["salestargetmethod"]);
                        //oemployeesalaryheader.salestarget = reader["salestarget"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestarget"]);

                        lst.Add(oemployeesalaryheader);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _employeesalaryheader);
            }
            return lst;
        }

        public Employeesalaryheader Insert(Employeesalaryheader oEmployeesalaryheader)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    Insert(transaction, conn, oEmployeesalaryheader);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeesalaryheader);


            }

            return oEmployeesalaryheader;
        }

        public Employeesalaryheader Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeesalaryheader oEmployeesalaryheader)
        {
            SalaryManager salaryManager = new SalaryManager() { Context = this.Context };
            salaryManager.Initialize(DateTimeNow.Month, DateTimeNow.Year);
            salaryManager.TestMonthly(oEmployeesalaryheader);
            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryheader.id;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryheader.employeeid;
            command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalaryheader.fromdate;
            command.Parameters.Add("iscurrent", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.iscurrent;
            command.Parameters.Add("basicpay", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalaryheader.basicpay;
            command.Parameters.Add("incentiveratio", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalaryheader.incentiveratio;
            command.Parameters.Add("eligibleesi", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.eligibleesi;
            command.Parameters.Add("eligiblepf", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.eligiblepf;
            command.Parameters.Add("configuration", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployeesalaryheader.configuration_json == null ? "null" : oEmployeesalaryheader.configuration_json;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalaryheader.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("pfselfpaid", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.pfselfpaid;
            command.Parameters.Add("esiselfpaid", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.esiselfpaid;
            command.Parameters.Add("grosspay", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalaryheader.grosspay;
            command.Parameters.Add("inchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryheader.inchargeid;
            command.Parameters.Add("hasarriyar", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.hasarriyar;
            command.Parameters.Add("lastgross", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalaryheader.lastgross;
            command.Parameters.Add("applyfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalaryheader.applyfrom;
            command.Parameters.Add("salarystructureid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryheader.salarystructureid;
            command.Parameters.Add("ctc", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalaryheader.ctc;
            command.Parameters.Add("salestarget", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalaryheader.salestarget;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmployeesalaryheader.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oEmployeesalaryheader;
        }

        internal bool SaveESIPFConfig(IncentiveConfig oEmployeesalaryheader)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    List<long> empids = oEmployeesalaryheader.ids.FindAll(e => e.salaryheaderid == 0).ConvertAll(e => e.employeeid);
                    List<long> shids = oEmployeesalaryheader.ids.FindAll(e => e.salaryheaderid > 0).ConvertAll(e => e.salaryheaderid);

                    foreach (var id in empids)
                    {
                        var item = Insert(transaction, conn, new Employeesalaryheader()
                        {
                            employeeid = id,
                            fromdate = DateTimeNow.Date.AddMonths(-6),
                            createdby = Context.UserID,
                            modifiedby = Context.UserID,
                            createdon = DateTimeNow,
                            modifiedon = DateTimeNow,
                            version = 1,
                            isactive = true
                        });
                        shids.Add(item.id);
                    }

                    string updatequery = "";
                    if (oEmployeesalaryheader.changepf)
                        updatequery = SQL_UpdatePF;
                    if (oEmployeesalaryheader.changeesi)
                        updatequery += SQL_UpdateESI;

                    if (string.IsNullOrEmpty(updatequery)) return false;

                    NpgsqlCommand command = GetCommand(string.Format(updatequery, string.Join(",", shids)), conn, transaction);

                    command.Parameters.Add("eligibleesi", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.eligibleesi;
                    command.Parameters.Add("esiselfpaid", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.esiselfpaid;

                    command.Parameters.Add("eligiblepf", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.eligiblepf;
                    command.Parameters.Add("pfselfpaid", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.pfselfpaid;

                    command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                    command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;

                    command.ExecuteNonQuery();


                    transaction.Commit();
                    conn.Close();
                }
                return true;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeesalaryheader);
            }
            return false;
        }
        public Employeesalaryheader Update(Employeesalaryheader oEmployeesalaryheader)
        {
            SalaryManager salaryManager = new SalaryManager() { Context = this.Context };
            salaryManager.Initialize(DateTimeNow.Month, DateTimeNow.Year);
            salaryManager.TestMonthly(oEmployeesalaryheader);
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oEmployeesalaryheader);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeesalaryheader);
            }

            return oEmployeesalaryheader;
        }

        public Employeesalaryheader Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeesalaryheader oEmployeesalaryheader)
        {
            NpgsqlCommand command = GetCommand(oEmployeesalaryheader.version == 0 ? SQL_Update.Replace("and version=@version", "") : SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryheader.id;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryheader.employeeid;
            command.Parameters.Add("salarystructureid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryheader.salarystructureid;
            command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalaryheader.fromdate;
            command.Parameters.Add("iscurrent", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.iscurrent;
            command.Parameters.Add("basicpay", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalaryheader.basicpay;
            command.Parameters.Add("incentiveratio", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalaryheader.incentiveratio;
            command.Parameters.Add("eligibleesi", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.eligibleesi;
            command.Parameters.Add("eligiblepf", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.eligiblepf;
            command.Parameters.Add("configuration", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployeesalaryheader.configuration_json == null ? "null" : oEmployeesalaryheader.configuration_json;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryheader.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalaryheader.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalaryheader.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("pfselfpaid", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.pfselfpaid;
            command.Parameters.Add("esiselfpaid", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.esiselfpaid;
            command.Parameters.Add("grosspay", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalaryheader.grosspay;
            command.Parameters.Add("hasarriyar", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.hasarriyar;
            command.Parameters.Add("lastgross", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalaryheader.lastgross;
            command.Parameters.Add("applyfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalaryheader.applyfrom;
            command.Parameters.Add("inchargeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryheader.inchargeid;
            command.Parameters.Add("ctc", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalaryheader.ctc;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmployeesalaryheader.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oEmployeesalaryheader;
        }


        public bool Delete(Employeesalaryheader oEmployeesalaryheader)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oEmployeesalaryheader);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeesalaryheader);
            }
            return hasSuccessCount;
        }


        internal int MultipleDelete(Employeesalaryheader oEmployeesalaryheader)
        {
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(SQL_MultipleDelete, conn);
                command.Parameters.Add("hasarriyar", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.hasarriyar;
                command.Parameters.Add("applyfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalaryheader.applyfrom;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalaryheader.fromdate;
                return command.ExecuteNonQuery();
            }
        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeesalaryheader oEmployeesalaryheader)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryheader.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalaryheader.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryheader.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
