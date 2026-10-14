using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class EmployeesalaryscheduleDataService : SAASData
    {
        const string SQL_Search = @"select employeesalaryschedule.id, employee.isactive, employee.modifiedon, employee.createdon, employee.createdby, employee.modifiedby, employeesalaryschedule.version, employee.id as employeeid, isreceivable, amount, detail, 
                                      schedulemonth, salaryheaderid,employeecode,employee.name,employee.departmentid,employee.sectionid,employee.floorid,parentid,detail,referencelist.name as designation,payateom,employeesalaryschedule.remarks,company.name companyname
                                      from employeesalaryschedule
                                      left outer join employee on employee.id = employeesalaryschedule.employeeid
                                      left outer join referencelist on referencelist.id = employee.desingnationid
                                      left outer join company on company.id = employee.companyid
                                      where employee.isactive = true and employeesalaryschedule.isactive = true and (employeesalaryschedule.id=@id or parentid=@id)";

        const string SQL_SearchSchedule = @"select employeesalaryschedule.id,employeesalaryschedule.employeeid, amount, schedulemonth, salaryheaderid,payateom,employeesalaryschedule.remarks
                                      from employeesalaryschedule
                                      where isreceivable=false and schedulemonth>=@fromdate and schedulemonth<@todate and employeesalaryschedule.isactive = true";

        const string SQL_SearchADVANCE = @"select employeesalaryschedule.id, employee.isactive, employee.modifiedon, employee.createdon, employee.createdby, employee.modifiedby, employee.version, employee.id as employeeid, isreceivable, amount, detail, 
                                      schedulemonth, salaryheaderid,employeecode,employee.name,parentid,detail,referencelist.name as designation,payateom,employeesalaryschedule.remarks,company.name companyname
                                      from employee
                                      left outer join employeesalaryschedule on employeesalaryschedule.employeeid = employee.id and source=1 and isreceivable = true 
                                      and employeesalaryschedule.salaryheaderid=@salaryheaderid and employeesalaryschedule.schedulemonth=@schedulemonth and employeesalaryschedule.isactive=true
                                      left outer join referencelist on referencelist.id = employee.desingnationid
                                      left outer join company on company.id = employee.companyid";

        const string SQL_SearchADVANCESUMMARY = @"
                                      with current as (
                                      select sum(amount) amount,employeeid from employeesalaryschedule where isactive = true and source=1 and isreceivable = true and 
                                      schedulemonth>=@fromdate and schedulemonth<@todate and salaryheaderid=@salaryheaderid
                                      group by employeeid)

                                      select employee.id as employeeid, employeecode,employee.name,referencelist.name as designation,current.amount
                                      from employee
                                      left outer join current on current.employeeid = employee.id
                                      left outer join referencelist on referencelist.id = employee.desingnationid
                                      where employee.isactive = true ";

        const string SQL_SearchPendingAdvance = @"select schedulemonth,employee.id employeeid, employeecode, employee.name, referencelist.name description,
                                    (case when isreceivable then amount else 0 end) receivable,
									(case when isreceivable then 0 else amount end) paid
                                    from employeesalaryschedule
                                    inner join employee on
                                    employeesalaryschedule.employeeid = employee.id
                                    inner join referencelist on referencelist.id = employeesalaryschedule.salaryheaderid
                                    where employeesalaryschedule.isactive = true
                                    and schedulemonth >= @fromdate and schedulemonth<@todate";

        const string SQL_SearchPendingAdvanceSummary = @"select employee.id employeeid,employeecode,employee.name,referencelist.name description,
                                    sum(case when isreceivable then amount else 0 end) receivable,
									sum(case when isreceivable then 0 else amount end) paid
									from employeesalaryschedule 
									inner join employee on 
                                    employeesalaryschedule.employeeid = employee.id 
									inner join referencelist on referencelist.id = employeesalaryschedule.salaryheaderid
                                    where employeesalaryschedule.isactive = true  
									and schedulemonth >= @fromdate and schedulemonth < @todate";

        const string SQL_SearchPendingAdvanceOpening = @"select employee.id employeeid,employeecode,employee.name,referencelist.name description,
                                    sum(case when isreceivable then amount else 0 end) receivable,
									sum(case when isreceivable then 0 else amount end) paid
									from employeesalaryschedule 
									inner join employee on 
                                    employeesalaryschedule.employeeid = employee.id 
									inner join referencelist on referencelist.id = employeesalaryschedule.salaryheaderid
                                    where employeesalaryschedule.isactive = true  
									and schedulemonth < @fromdate";

        const string SQL_Insert = @"INSERT INTO public.employeesalaryschedule(isactive, modifiedon, createdon, createdby, modifiedby, version, employeeid, isreceivable, amount, detail, schedulemonth, salaryheaderid,parentid,source,payateom,remarks,locationid,companyid) 
                                VALUES(@isactive, @modifiedon, @createdon, @createdby, @modifiedby, @version, @employeeid, @isreceivable, @amount, @detail, @schedulemonth, @salaryheaderid,@parentid,@source,@payateom,@remarks,@locationid,@companyid) RETURNING id";



        const string SQL_Update = @"UPDATE public.employeesalaryschedule SET modifiedon=@modifiedon, modifiedby=@modifiedby, version=@version+1, employeeid=@employeeid, isreceivable=@isreceivable, amount=@amount, detail=@detail, 
                                    schedulemonth=@schedulemonth, salaryheaderid=@salaryheaderid,payateom=@payateom,remarks=@remarks where id=@id and version=@version RETURNING version";


        public List<Employeesalaryschedule> SaveFine(EmployeeAdvance advance)
        {
            SalaryManager salaryManager = new SalaryManager() { Context = this.Context };
            salaryManager.Initialize(this.Context.OrganizationID, 0);
            var fineconfig = salaryManager.DefaultHeaders.Values.ToList().FindAll(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.FineDeduction);
            if (fineconfig.Count == 0) return new List<Employeesalaryschedule>();
            advance.salaryheaderid = fineconfig.First().HeaderID;

            List<Employeesalaryschedule> items = new List<Employeesalaryschedule>();
            foreach (var item in advance.schedules)
            {
                Employeesalaryschedule newitem = new Employeesalaryschedule();
                newitem.isactive = true;
                newitem.modifiedon = DateTimeNow;
                newitem.createdon = DateTimeNow;
                newitem.createdby = Context.UserID;
                newitem.modifiedby = Context.UserID;
                newitem.companyid = advance.companyid;
                newitem.locationid = advance.locationid;
                newitem.version = 1;
                newitem.employeeid = item.employeeid;
                newitem.isreceivable = true;
                newitem.amount = item.amount;
                newitem.remarks = advance.remarks;
                newitem.schedulemonth = item
                    .advancedate.ToLocalTime().Date;
                newitem.salaryheaderid = advance.salaryheaderid;
                newitem.payateom = advance.payateom;
                items.Add(newitem);
                newitem = new Employeesalaryschedule();
                newitem.isactive = true;
                newitem.remarks = advance.remarks;
                newitem.modifiedon = DateTimeNow;
                newitem.createdon = DateTimeNow;
                newitem.createdby = Context.UserID;
                newitem.modifiedby = Context.UserID;
                newitem.version = 1;
                newitem.employeeid = item.employeeid;
                newitem.isreceivable = false;
                newitem.amount = item.amount;
                newitem.schedulemonth = item.advancedate.ToLocalTime().Date;
                newitem.salaryheaderid = advance.salaryheaderid;
                newitem.payateom = advance.payateom;
                items.Add(newitem);
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    DeleteAll(transaction, conn, advance, ScheduleType.Fine);
                    long parentid = 0;
                    foreach (var item in items)
                    {
                        if (parentid != 0)
                        {
                            item.parentid = parentid;
                            item.locationid = advance.locationid;
                            item.companyid = advance.companyid;
                            Insert(transaction, conn, item, ScheduleType.Fine);
                            parentid = 0;
                        }
                        else
                        {
                            item.locationid = advance.locationid;
                            item.companyid = advance.companyid;
                            Insert(transaction, conn, item, ScheduleType.Fine);
                            parentid = item.id;
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
                List<Employeesalaryschedule> results = new List<Employeesalaryschedule>();

                results.AddRange(Search(new EmployeesalaryscheduleCreteria()
                {
                    id = items.First().id,
                    mode = EmployeesalaryscheduleCreteria.FilterMode.AdvancePaid
                }));

                return results;

            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, advance);
                advance.ErrorMessage = exp.Message;
            }
            return null;
        }

        public List<Employeesalaryschedule> SaveAdvance(EmployeeAdvance advance)
        {
            SortedDictionary<long, Employee> employeeDetails = new SortedDictionary<long, Employee>();
            using (NpgsqlConnection conn = CreateConnection())
            {
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = "Select employee.id,employee.locationid,employee.companyid from employee";
                qb.AddInParamer("employee.id", string.Join(",", advance.schedules.ConvertAll(e=>e.employeeid)));
                var command = qb.GetCommand(conn);
                conn.Open();
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        var employeeid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        var locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        var companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        employeeDetails.Add(employeeid, new Employee()
                        {
                            id = employeeid,
                            locationid = locationid,
                            companyid = companyid
                        });
                    }
                }
                conn.Close();
            }
            List<Employeesalaryschedule> items = new List<Employeesalaryschedule>();
            foreach (var item in advance.schedules)
            {
                Employeesalaryschedule newitem = new Employeesalaryschedule();
                newitem.isactive = true;
                newitem.modifiedon = DateTimeNow;
                newitem.createdon = DateTimeNow;
                newitem.createdby = 0;
                newitem.modifiedby = 0;
                newitem.version = 1;
                newitem.employeeid = item.employeeid;
                newitem.isreceivable = true;
                newitem.amount = item.amount;
                newitem.remarks = advance.remarks;

                newitem.schedulemonth = item
                    .advancedate.ToLocalTime().Date;
                newitem.salaryheaderid = advance.salaryheaderid;
                newitem.payateom = advance.payateom;
                items.Add(newitem);
                newitem = new Employeesalaryschedule();
                newitem.isactive = true;
                newitem.locationid = employeeDetails.ContainsKey(item.employeeid) ? employeeDetails[item.employeeid].locationid : advance.locationid;
                newitem.companyid = employeeDetails.ContainsKey(item.employeeid) ? employeeDetails[item.employeeid].companyid : advance.companyid;
                newitem.remarks = advance.remarks;
                newitem.modifiedon = DateTimeNow;
                newitem.createdon = DateTimeNow;
                newitem.createdby = Context.UserID;
                newitem.modifiedby = Context.UserID;
                newitem.version = 1;
                newitem.employeeid = item.employeeid;
                newitem.isreceivable = false;
                newitem.amount = item.amount;
                newitem.schedulemonth = item.advancedate.ToLocalTime().Date;
                newitem.salaryheaderid = advance.salaryheaderid;
                newitem.payateom = advance.payateom;
                items.Add(newitem);
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    DeleteAll(transaction, conn, advance, ScheduleType.AdvanceSingle);
                    long parentid = 0;
                    foreach (var item in items)
                    {
                        if (parentid != 0)
                        {
                            item.parentid = parentid;
                            Insert(transaction, conn, item, ScheduleType.AdvanceSingle);
                            parentid = 0;
                        }
                        else
                        {
                            Insert(transaction, conn, item, ScheduleType.AdvanceSingle);
                            parentid = item.id;
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
                List<Employeesalaryschedule> results = new List<Employeesalaryschedule>();

                results.AddRange(Search(new EmployeesalaryscheduleCreteria()
                {
                    id = items.First().id,
                    mode = EmployeesalaryscheduleCreteria.FilterMode.AdvancePaid
                }));

                return results;

            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, advance);
                advance.ErrorMessage = exp.Message;
            }
            return null;
        }

        public EmployeeScheduleDetail SearchAdvanceDetail(EmployeesalaryscheduleCreteria _employeesalaryschedule)
        {
            EmployeeScheduleDetail employeeScheduleDetail = null;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"select employee.id employeeid,employeecode,employee.name,
                                    sum(case when isreceivable then amount else 0 end) receivable,
									sum(case when isreceivable then 0 else amount end) paid
									from employeesalaryschedule 
									inner join employee on 
                                    employeesalaryschedule.employeeid = employee.id 
									inner join referencelist on referencelist.id = employeesalaryschedule.salaryheaderid
                                    where employeesalaryschedule.isactive = true  
									and schedulemonth < @fromdate";

                if (_employeesalaryschedule.employeeid > 0)
                    qb.AddParameter("employee.id", _employeesalaryschedule.employeeid, "=");

                if (_employeesalaryschedule.salaryheaderid > 0)
                    qb.AddParameter("employeesalaryschedule.salaryheaderid", _employeesalaryschedule.salaryheaderid, "=");

                command = qb.GetCommand(conn);

                if (_employeesalaryschedule.todate == DateTime.MinValue)
                    _employeesalaryschedule.todate = DateTimeNow.Date;

                command.CommandText += " group by employee.id,employeecode,employee.name,referencelist.name";

                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _employeesalaryschedule.fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _employeesalaryschedule.todate;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        employeeScheduleDetail = new EmployeeScheduleDetail();
                        employeeScheduleDetail.advancedetails = new List<EmployeeScheduleDetail.AdvanceDetails>();
                        employeeScheduleDetail.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        employeeScheduleDetail.paid = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        employeeScheduleDetail.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        employeeScheduleDetail.employeename = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        employeeScheduleDetail.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        employeeScheduleDetail.pendingamt = employeeScheduleDetail.receivable - employeeScheduleDetail.paid;
                        employeeScheduleDetail.paid = 0;
                        employeeScheduleDetail.receivable = 0;
                        employeeScheduleDetail.advancedetails.Add(new EmployeeScheduleDetail.AdvanceDetails()
                        {
                            date = _employeesalaryschedule.fromdate,
                            particulars = "Opening Balance",
                            loanamt = employeeScheduleDetail.pendingamt
                        });

                    }
                }


                qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchPendingAdvance;
                if (_employeesalaryschedule.employeeid > 0)
                    qb.AddParameter("employee.id", _employeesalaryschedule.employeeid, "=");

                if (_employeesalaryschedule.salaryheaderid > 0)
                    qb.AddParameter("employeesalaryschedule.salaryheaderid", _employeesalaryschedule.salaryheaderid, "=");

                command = qb.GetCommand(conn);

                if (_employeesalaryschedule.todate == DateTime.MinValue)
                    _employeesalaryschedule.todate = DateTimeNow.Date;

                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _employeesalaryschedule.fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _employeesalaryschedule.todate;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (employeeScheduleDetail == null)
                        {
                            employeeScheduleDetail = new EmployeeScheduleDetail();
                            employeeScheduleDetail.advancedetails = new List<EmployeeScheduleDetail.AdvanceDetails>();
                            employeeScheduleDetail.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                            employeeScheduleDetail.employeename = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                            employeeScheduleDetail.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        }

                        var paid = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        var receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        var particulars = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        var schedulemonth = reader["schedulemonth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["schedulemonth"]);

                        employeeScheduleDetail.advancedetails.Add(new EmployeeScheduleDetail.AdvanceDetails()
                        {
                            particulars = particulars,
                            loanamt = receivable,
                            deductionamt = paid,
                            date = schedulemonth
                        });

                    }
                }

                employeeScheduleDetail.advancedetails.Add(new EmployeeScheduleDetail.AdvanceDetails()
                {
                    particulars = "Closing Balance",
                    deductionamt = employeeScheduleDetail.advancedetails.Sum(e => e.loanamt - e.deductionamt)
                });

                conn.Close();

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _employeesalaryschedule);
            }
            return employeeScheduleDetail;
        }

        public List<EmployeesalaryscheduleDetails> SearchAdvanceSummary(EmployeesalaryscheduleCreteria _employeesalaryschedule)
        {
            List<EmployeesalaryscheduleDetails> lst = new List<EmployeesalaryscheduleDetails>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchPendingAdvanceOpening;
                if (_employeesalaryschedule.employeeid > 0)
                    qb.AddParameter("employee.id", _employeesalaryschedule.employeeid, "=");

                if (_employeesalaryschedule.salaryheaderid > 0)
                    qb.AddParameter("employeesalaryschedule.salaryheaderid", _employeesalaryschedule.salaryheaderid, "=");

                command = qb.GetCommand(conn);

                if (_employeesalaryschedule.todate == DateTime.MinValue)
                    _employeesalaryschedule.todate = DateTimeNow.Date;

                command.CommandText += " group by employee.id,employeecode,employee.name,referencelist.name";

                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _employeesalaryschedule.fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _employeesalaryschedule.todate;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        EmployeesalaryscheduleDetails oemployeesalaryschedule = new EmployeesalaryscheduleDetails();
                        oemployeesalaryschedule.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        oemployeesalaryschedule.paid = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        oemployeesalaryschedule.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        oemployeesalaryschedule.openingamt = oemployeesalaryschedule.receivable - oemployeesalaryschedule.paid;
                        oemployeesalaryschedule.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oemployeesalaryschedule.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        oemployeesalaryschedule.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        oemployeesalaryschedule.paid = 0;
                        oemployeesalaryschedule.receivable = 0;

                        lst.Add(oemployeesalaryschedule);
                    }
                }


                qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchPendingAdvanceSummary;
                if (_employeesalaryschedule.employeeid > 0)
                    qb.AddParameter("employee.id", _employeesalaryschedule.employeeid, "=");

                if (_employeesalaryschedule.salaryheaderid > 0)
                    qb.AddParameter("employeesalaryschedule.salaryheaderid", _employeesalaryschedule.salaryheaderid, "=");

                command = qb.GetCommand(conn);

                if (_employeesalaryschedule.todate == DateTime.MinValue)
                    _employeesalaryschedule.todate = DateTimeNow.Date;

                command.CommandText += " group by employee.id,employeecode,employee.name,referencelist.name";

                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _employeesalaryschedule.fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _employeesalaryschedule.todate.AddDays(1);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        EmployeesalaryscheduleDetails oemployeesalaryschedule = new EmployeesalaryscheduleDetails();
                        oemployeesalaryschedule.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        oemployeesalaryschedule.paid = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        oemployeesalaryschedule.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        oemployeesalaryschedule.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oemployeesalaryschedule.description = reader["description"] == DBNull.Value ? "" : reader["description"].ToString();
                        oemployeesalaryschedule.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();

                        if (lst.Exists(e => e.employeeid == oemployeesalaryschedule.employeeid &&
                         e.description == oemployeesalaryschedule.description))
                        {
                            oemployeesalaryschedule = lst.Find(e => e.employeeid == oemployeesalaryschedule.employeeid &&
                            e.description == oemployeesalaryschedule.description);
                            oemployeesalaryschedule.paid = reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                            oemployeesalaryschedule.receivable = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        }
                        else
                        {
                            lst.Add(oemployeesalaryschedule);
                        }
                    }
                }
                conn.Close();

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _employeesalaryschedule);
            }
            lst.ForEach(e => e.pendingamt = e.openingamt +
                            e.receivable - e.paid);
            return lst;
        }

        public EmployeeAdvance UpdateAdvance(EmployeeAdvance advance)
        {
            List<Employeesalaryschedule> items = new List<Employeesalaryschedule>();
            foreach (var item in advance.schedules)
            {
                Employeesalaryschedule newitem = new Employeesalaryschedule();
                newitem.isactive = true;
                newitem.modifiedon = DateTimeNow;
                newitem.createdon = DateTimeNow;
                newitem.createdby = 0;
                newitem.modifiedby = 0;
                newitem.version = 1;
                newitem.employeeid = item.employeeid;
                newitem.isreceivable = true;
                newitem.amount = item.amount;
                newitem.schedulemonth = item
                    .advancedate.ToLocalTime().Date;
                newitem.salaryheaderid = advance.salaryheaderid;
                newitem.payateom = advance.payateom;
                items.Add(newitem);
                newitem = new Employeesalaryschedule();
                newitem.isactive = true;
                newitem.modifiedon = DateTimeNow;
                newitem.createdon = DateTimeNow;
                newitem.createdby = Context.UserID;
                newitem.modifiedby = Context.UserID;
                newitem.version = 1;
                newitem.employeeid = item.employeeid;
                newitem.isreceivable = false;
                newitem.amount = item.amount;
                newitem.schedulemonth = item.advancedate.ToLocalTime().Date;
                newitem.salaryheaderid = advance.salaryheaderid;
                newitem.payateom = advance.payateom;
                items.Add(newitem);
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    DeleteAll(transaction, conn, advance, ScheduleType.AdvanceSingle);
                    long parentid = 0;
                    foreach (var item in items)
                    {
                        if (parentid != 0)
                        {
                            item.parentid = parentid;
                            Insert(transaction, conn, item, ScheduleType.AdvanceSingle);
                            parentid = 0;
                        }
                        else
                        {
                            Insert(transaction, conn, item, ScheduleType.AdvanceSingle);
                            parentid = item.id;
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, advance);
                advance.ErrorMessage = exp.Message;
            }
            return advance;
        }


        const string SQL_Delete = @"update public.employeesalaryschedule set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id or parentid=@id";

        public List<Employeesalaryschedule> SearchPendingBills(EmployeesalaryscheduleCreteria _employeesalaryschedule)
        {
            long advanceid = 0;
            using (var reader = ExecuteReader(@"select id from referencelist where type = 'PAYCATEGORY' and cast(additionalinfo->'paytype' as varchar(100)) = '""Advance""'", null))
            {
                if (reader.Read())
                {
                    if (!reader.IsDBNull(0))
                        advanceid = reader.GetInt64(0);
                }
            }
            List<Employeesalaryschedule> lst = new List<Employeesalaryschedule>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"select customerinvoice.id,customer.employeeid,employee.name,employeecode,billno,receivable,paid,transactiondate from customerinvoice 
                    inner join customer on customer.id = customerid
                    inner join employee on employee.id = customer.employeeid
                    where customerinvoice.isactive = true and customerid>0 and customertype=1 and receivable-paid>0 and (employeesalaryscheduleid = 0 or employeesalaryscheduleid is null)"; ;

                if (_employeesalaryschedule.from.Year > 2000 && _employeesalaryschedule.to.Year > 2000)
                {
                    qb.AddParameter("customerinvoice.transactiondate", "schedulemonthfrom", _employeesalaryschedule.from.Date, ">=");
                    qb.AddParameter("customerinvoice.transactiondate", "schedulemonthto", _employeesalaryschedule.to.AddDays(1), "<");
                }
                qb.SortField = @"case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";
                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employeesalaryschedule oemployeesalaryschedule = new Employeesalaryschedule();
                        oemployeesalaryschedule.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oemployeesalaryschedule.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        oemployeesalaryschedule.amount = reader["receivable"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["receivable"]);
                        oemployeesalaryschedule.amount -= reader["paid"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["paid"]);
                        oemployeesalaryschedule.detail = reader["billno"] == DBNull.Value ? "" : reader["billno"].ToString();
                        oemployeesalaryschedule.schedulemonth = reader["transactiondate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["transactiondate"]);
                        oemployeesalaryschedule.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oemployeesalaryschedule.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        oemployeesalaryschedule.salaryheaderid = advanceid;
                        lst.Add(oemployeesalaryschedule);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _employeesalaryschedule);
            }
            return lst;
        }

        public List<Employeesalaryschedule> PrepareForAdvance(EmployeesalaryscheduleCreteria _employeesalaryschedule, Npgsql.NpgsqlConnection conn)
        {
            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = SQL_SearchADVANCESUMMARY;


            if (_employeesalaryschedule.employeeid != 0)
                qb.AddParameter("employee.id", _employeesalaryschedule.employeeid, "=");

            if (_employeesalaryschedule.departmentid != 0)
                qb.AddParameter("employee.departmentid", _employeesalaryschedule.departmentid, "=");

            if (_employeesalaryschedule.floorid != 0)
                qb.AddParameter("employee.floorid", _employeesalaryschedule.floorid, "=");

            if (_employeesalaryschedule.sectionid != 0)
                qb.AddParameter("employee.sectionid", _employeesalaryschedule.sectionid, "=");

            if (_employeesalaryschedule.companyid != 0)
                qb.AddParameter("employee.companyid", _employeesalaryschedule.companyid, "=");
            
            if (_employeesalaryschedule.salarymode != 0)
                qb.AddParameter("employee.salarymode", _employeesalaryschedule.salarymode, "=");

            if (_employeesalaryschedule.workingmode != 0)
                qb.AddParameter("employee.workingmode", _employeesalaryschedule.workingmode, "=");

            if (_employeesalaryschedule.id == 0)
            {
                qb.AddORParameter("employee.hasleft", false, "=", "relievingdate>=@schedulemonth");
                if (_employeesalaryschedule.schedulemonth == DateTime.MinValue)
                    _employeesalaryschedule.schedulemonth = DateTimeNow;
            }
            qb.SortField = @"case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";
            var command = qb.GetCommand(conn);

            if (_employeesalaryschedule.id != 0)
            {
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _employeesalaryschedule.id;
            }
            else
            {
                command.Parameters.Add("salaryheaderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _employeesalaryschedule.salaryheaderid;
                command.Parameters.Add("schedulemonth", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _employeesalaryschedule.schedulemonth;
                command.Parameters.Add("hasleft", NpgsqlTypes.NpgsqlDbType.Boolean).Value = _employeesalaryschedule.showhasleft;

                DateTime fromdate = new DateTime(_employeesalaryschedule.searchyear, _employeesalaryschedule.searchmonth, 1);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = fromdate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = fromdate.AddMonths(1);
            }
            List<Employeesalaryschedule> lst = new List<Employeesalaryschedule>();
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Employeesalaryschedule oemployeesalaryschedule = new Employeesalaryschedule();
                    oemployeesalaryschedule.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                    oemployeesalaryschedule.currentamount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                    oemployeesalaryschedule.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                    oemployeesalaryschedule.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                    oemployeesalaryschedule.designation = reader["designation"] == DBNull.Value ? "" : reader["designation"].ToString();
                    lst.Add(oemployeesalaryschedule);
                }
            }
            return lst;
        }
        public List<Employeesalaryschedule> Search(EmployeesalaryscheduleCreteria _employeesalaryschedule)
        {
            if (_employeesalaryschedule.mode == EmployeesalaryscheduleCreteria.FilterMode.PendingBills)
                return SearchPendingBills(_employeesalaryschedule);

            List<Employeesalaryschedule> lst = new List<Employeesalaryschedule>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                if (_employeesalaryschedule.id != 0)
                {
                    qb.CurrentQuery = SQL_Search;

                }
                else if (_employeesalaryschedule.searchmode == 0)
                {
                    qb.CurrentQuery = _employeesalaryschedule.showallemployee ? SQL_SearchADVANCE : SQL_Search.Substring(0, SQL_Search.IndexOf("where"));

                    if (!_employeesalaryschedule.showallemployee)
                    {
                        qb.AddParameter("employeesalaryschedule.isactive", true, "=");
                        if (_employeesalaryschedule.from.Year > 2000 && _employeesalaryschedule.to.Year > 2000)
                        {
                            qb.AddParameter("employeesalaryschedule.schedulemonth", "schedulemonthfrom", _employeesalaryschedule.from.Date, ">=");
                            qb.AddParameter("employeesalaryschedule.schedulemonth", "schedulemonthto", _employeesalaryschedule.to.AddDays(1), "<");
                        }
                        else if (_employeesalaryschedule.searchmonth > 0 && _employeesalaryschedule.searchyear > 0)
                        {
                            DateTime fromdate = new DateTime(_employeesalaryschedule.searchyear, _employeesalaryschedule.searchmonth, 1);
                            qb.AddParameter("employeesalaryschedule.schedulemonth", "schedulemonthfrom", fromdate, ">=");
                            qb.AddParameter("employeesalaryschedule.schedulemonth", "schedulemonthto", fromdate.AddMonths(1), "<");
                        }
                    }
                }
                else
                {
                    EmployeesalaryscheduleCreteria.SearchMode mode = (EmployeesalaryscheduleCreteria.SearchMode)_employeesalaryschedule.searchmode;
                    if (mode == EmployeesalaryscheduleCreteria.SearchMode.Search)
                    {
                        qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                        qb.AddParameter("employeesalaryschedule.isactive", true, "=");
                        DateTime fromdate = new DateTime(_employeesalaryschedule.searchyear, _employeesalaryschedule.searchmonth, 1);
                        qb.AddParameter("employeesalaryschedule.schedulemonth", "schedulemonthfrom", fromdate, ">=");
                        qb.AddParameter("employeesalaryschedule.schedulemonth", "schedulemonthto", fromdate.AddMonths(1), "<");
                    }
                    else
                    {
                        return PrepareForAdvance(_employeesalaryschedule, conn);
                    }
                }

                if (_employeesalaryschedule.id <= 0)
                {
                    switch (_employeesalaryschedule.mode)
                    {
                        case EmployeesalaryscheduleCreteria.FilterMode.AdvancePaid:

                            qb.AddParameter("employeesalaryschedule.source", 1, "=");
                            qb.AddParameter("employeesalaryschedule.isreceivable", true, "=");
                            break;
                        case EmployeesalaryscheduleCreteria.FilterMode.AdvanceReceivble:
                            qb.AddParameter("employeesalaryschedule.source", 1, "=");
                            qb.AddParameter("employeesalaryschedule.isreceivable", false, "=");

                            break;
                        case EmployeesalaryscheduleCreteria.FilterMode.ScheduledPaid:
                            qb.AddParameter("employeesalaryschedule.source", 0, "=");
                            qb.AddParameter("employeesalaryschedule.isreceivable", true, "=");
                            break;
                        case EmployeesalaryscheduleCreteria.FilterMode.ScheduledReceivable:

                            qb.AddParameter("employeesalaryschedule.source", 0, "=");
                            qb.AddParameter("employeesalaryschedule.isreceivable", false, "=");
                            break;
                        case EmployeesalaryscheduleCreteria.FilterMode.OtherDeduction:
                            qb.AddParameter("employeesalaryschedule.source", 3, "=");
                            qb.AddParameter("employeesalaryschedule.isreceivable", false, "=");
                            break;
                    }

                    qb.AddParameter("employee.isactive", true, "=");

                    if (_employeesalaryschedule.employeeid != 0)
                        qb.AddParameter("employee.id", _employeesalaryschedule.employeeid, "=");

                    if (_employeesalaryschedule.departmentid != 0)
                        qb.AddParameter("employee.departmentid", _employeesalaryschedule.departmentid, "=");

                    if (_employeesalaryschedule.floorid != 0)
                        qb.AddParameter("employee.floorid", _employeesalaryschedule.floorid, "=");

                    if (_employeesalaryschedule.companyid != 0)
                        qb.AddParameter("employee.companyid", _employeesalaryschedule.companyid, "=");

                    if (_employeesalaryschedule.sectionid != 0)
                        qb.AddParameter("employee.sectionid", _employeesalaryschedule.sectionid, "=");

                    if (_employeesalaryschedule.salarymode != 0)
                        qb.AddParameter("employee.salarymode", _employeesalaryschedule.salarymode, "=");

                    if (_employeesalaryschedule.workingmode != 0)
                        qb.AddParameter("employee.workingmode", _employeesalaryschedule.workingmode, "=");

                    if (_employeesalaryschedule.salaryheaderid != 0)
                        qb.AddParameter("employeesalaryschedule.salaryheaderid", _employeesalaryschedule.salaryheaderid, "=");

                    if (_employeesalaryschedule.id == 0)
                    {
                        if (!_employeesalaryschedule.showhasleft)
                            qb.AddORParameter("employee.hasleft", false, "=", "relievingdate>=@schedulemonth");
                        else
                            qb.AddORParameter("employee.hasleft", true, "=", "relievingdate<@schedulemonth");
                    }

                    qb.SortField = @"case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";
                }
                command = qb.GetCommand(conn);

                if (_employeesalaryschedule.id != 0)
                {
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _employeesalaryschedule.id;
                }
                else
                {
                    command.Parameters.Add("salaryheaderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _employeesalaryschedule.salaryheaderid;
                    command.Parameters.Add("hasleft", NpgsqlTypes.NpgsqlDbType.Boolean).Value = _employeesalaryschedule.showhasleft;
                    if (_employeesalaryschedule.searchmode > 0)
                    {
                        command.Parameters.Add("schedulemonth", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _employeesalaryschedule.schedulemonth;
                        DateTime fromdate = new DateTime(_employeesalaryschedule.searchyear, _employeesalaryschedule.searchmonth, 1);
                        command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = fromdate;
                        command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = fromdate.AddMonths(1);
                        command.Parameters.Add("schedulemonth", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = fromdate.AddMonths(1).AddDays(1);
                    }
                    else if (_employeesalaryschedule.searchmonth > 0 && _employeesalaryschedule.searchyear > 0)
                    {
                        command.Parameters.Add("schedulemonth", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = new DateTime(_employeesalaryschedule.searchyear, _employeesalaryschedule.searchmonth, 1);
                    }
                    else
                    {
                        command.Parameters.Add("schedulemonth", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = _employeesalaryschedule.to.AddDays(1);
                    }
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employeesalaryschedule oemployeesalaryschedule = new Employeesalaryschedule();
                        oemployeesalaryschedule.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oemployeesalaryschedule.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oemployeesalaryschedule.payateom = reader["payateom"] == DBNull.Value ? false : Convert.ToBoolean(reader["payateom"]);

                        oemployeesalaryschedule.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oemployeesalaryschedule.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oemployeesalaryschedule.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oemployeesalaryschedule.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oemployeesalaryschedule.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oemployeesalaryschedule.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        oemployeesalaryschedule.parentid = reader["parentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["parentid"]);
                        oemployeesalaryschedule.isreceivable = reader["isreceivable"] == DBNull.Value ? false : Convert.ToBoolean(reader["isreceivable"]);
                        oemployeesalaryschedule.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        oemployeesalaryschedule.detail = reader["detail"] == DBNull.Value ? "" : reader["detail"].ToString();
                        oemployeesalaryschedule.schedulemonth = reader["schedulemonth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["schedulemonth"]);
                        oemployeesalaryschedule.salaryheaderid = reader["salaryheaderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salaryheaderid"]);

                        if (_employeesalaryschedule.mode == EmployeesalaryscheduleCreteria.FilterMode.AdvancePaid ||
                            _employeesalaryschedule.mode == EmployeesalaryscheduleCreteria.FilterMode.ScheduledPaid)
                        {
                            oemployeesalaryschedule.inwords = WebAPI.Data.Numberconvertor.rupees(oemployeesalaryschedule.amount);
                            oemployeesalaryschedule.companyname = reader["companyname"] == DBNull.Value ? "" : reader["companyname"].ToString();
                        }
                        //if (_employeesalaryschedule.showallemployee)
                        {
                            oemployeesalaryschedule.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                            oemployeesalaryschedule.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                            oemployeesalaryschedule.designation = reader["designation"] == DBNull.Value ? "" : reader["designation"].ToString();
                        }
                        lst.Add(oemployeesalaryschedule);
                    }
                }
                conn.Close();
                if (_employeesalaryschedule.id > 0)
                {
                    var parent = lst.Find(e => e.id == _employeesalaryschedule.id);
                    parent.deductionconfig = new List<Employeesalaryschedule.ScheduleItem>();
                    foreach (var item in lst.FindAll(e => e.id != _employeesalaryschedule.id))
                    {
                        Employeesalaryschedule.ScheduleItem oitems = new Employeesalaryschedule.ScheduleItem();
                        oitems.amount = item.amount;
                        oitems.schedulemonth = item.schedulemonth;
                        parent.deductionconfig.Add(oitems);
                    }
                    lst.RemoveAll(e => e.id != _employeesalaryschedule.id);
                }

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _employeesalaryschedule);
            }
            return lst;
        }


        public List<Employeesalaryschedule> SearchSchedule(DateTime fromDate, DateTime todate)
        {
            List<Employeesalaryschedule> lst = new List<Employeesalaryschedule>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = GetCommand(SQL_SearchSchedule, conn);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = fromDate;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = todate;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employeesalaryschedule oemployeesalaryschedule = new Employeesalaryschedule();
                        oemployeesalaryschedule.payateom = reader["payateom"] == DBNull.Value ? false : Convert.ToBoolean(reader["payateom"]);
                        oemployeesalaryschedule.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        oemployeesalaryschedule.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        oemployeesalaryschedule.schedulemonth = reader["schedulemonth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["schedulemonth"]);
                        oemployeesalaryschedule.salaryheaderid = reader["salaryheaderid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salaryheaderid"]);
                        lst.Add(oemployeesalaryschedule);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "fromdate " + fromDate + " todate " + todate);
            }
            return lst;
        }

        public Employeesalaryschedule Insert(Employeesalaryschedule oEmployeesalaryschedule)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Insert(transaction, conn, oEmployeesalaryschedule, 0);
                    foreach (Employeesalaryschedule.ScheduleItem oitem in oEmployeesalaryschedule.deductionconfig)
                    {
                        Employeesalaryschedule newitem = new Employeesalaryschedule();

                        newitem.isactive = oEmployeesalaryschedule.isactive;
                        newitem.modifiedon = oEmployeesalaryschedule.modifiedon;
                        newitem.createdon = oEmployeesalaryschedule.createdon;
                        newitem.createdby = oEmployeesalaryschedule.createdby;
                        newitem.modifiedby = oEmployeesalaryschedule.modifiedby;
                        newitem.version = oEmployeesalaryschedule.version;
                        newitem.employeeid = oEmployeesalaryschedule.employeeid;
                        newitem.isreceivable = false;
                        newitem.parentid = oEmployeesalaryschedule.id;
                        newitem.amount = oitem.amount;
                        newitem.detail = oEmployeesalaryschedule.detail;
                        newitem.schedulemonth = oitem.schedulemonth.ToLocalTime().Date;
                        newitem.salaryheaderid = oEmployeesalaryschedule.salaryheaderid;
                        newitem.payateom = oEmployeesalaryschedule.payateom;
                        Insert(transaction, conn, newitem, 0);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeesalaryschedule);


            }

            return oEmployeesalaryschedule;
        }

        public enum ScheduleType
        {
            General,
            AdvanceSingle,
            AdvanceMultiple,
            Fine
        }
        public Employeesalaryschedule Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeesalaryschedule oEmployeesalaryschedule, ScheduleType source)
        {
            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryschedule.isactive;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryschedule.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryschedule.locationid;
            command.Parameters.Add("source", NpgsqlTypes.NpgsqlDbType.Integer).Value = (int)source;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployeesalaryschedule.remarks == null ? "" : oEmployeesalaryschedule.remarks; ;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalaryschedule.version;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryschedule.employeeid;
            command.Parameters.Add("parentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryschedule.parentid;
            command.Parameters.Add("isreceivable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryschedule.isreceivable;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalaryschedule.amount;
            command.Parameters.Add("detail", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployeesalaryschedule.detail == null ? "" : oEmployeesalaryschedule.detail;
            command.Parameters.Add("schedulemonth", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalaryschedule.schedulemonth;
            command.Parameters.Add("salaryheaderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryschedule.salaryheaderid;
            command.Parameters.Add("payateom", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryschedule.payateom;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmployeesalaryschedule.id = Convert.ToInt64(reader[0].ToString());
                    if (oEmployeesalaryschedule.customerinvoiceid > 0)
                        ExecuteQuery("Update customerinvoice set employeesalaryscheduleid=" + oEmployeesalaryschedule.id + " where id=" + oEmployeesalaryschedule.customerinvoiceid, null);
                }
            }
            return oEmployeesalaryschedule;
        }

        public Employeesalaryschedule Update(Employeesalaryschedule oEmployeesalaryschedule)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oEmployeesalaryschedule);


                    // Delete all its child records
                    string SQL_DeleteAll = @"delete from public.employeesalaryschedule where parentid=@parentid";
                    NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
                    command.Parameters.Add("parentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryschedule.id;
                    int successCount = command.ExecuteNonQuery();

                    foreach (Employeesalaryschedule.ScheduleItem oitem in oEmployeesalaryschedule.deductionconfig)
                    {
                        Employeesalaryschedule newitem = new Employeesalaryschedule();

                        newitem.isactive = oEmployeesalaryschedule.isactive;
                        newitem.modifiedon = oEmployeesalaryschedule.modifiedon;
                        newitem.createdon = oEmployeesalaryschedule.createdon;
                        newitem.createdby = oEmployeesalaryschedule.createdby;
                        newitem.modifiedby = oEmployeesalaryschedule.modifiedby;
                        newitem.version = oEmployeesalaryschedule.version;
                        newitem.employeeid = oEmployeesalaryschedule.employeeid;
                        newitem.isreceivable = false;
                        newitem.parentid = oEmployeesalaryschedule.id;
                        newitem.amount = oitem.amount;
                        newitem.detail = oEmployeesalaryschedule.detail;
                        newitem.schedulemonth = oitem.schedulemonth.ToLocalTime().Date;
                        newitem.salaryheaderid = oEmployeesalaryschedule.salaryheaderid;
                        newitem.payateom = oEmployeesalaryschedule.payateom;
                        Insert(transaction, conn, newitem, 0);
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeesalaryschedule);


            }

            return oEmployeesalaryschedule;
        }

        public Employeesalaryschedule Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeesalaryschedule oEmployeesalaryschedule)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryschedule.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryschedule.isactive;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalaryschedule.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryschedule.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalaryschedule.version;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryschedule.employeeid;
            command.Parameters.Add("isreceivable", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryschedule.isreceivable;
            command.Parameters.Add("amount", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalaryschedule.amount;
            command.Parameters.Add("detail", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployeesalaryschedule.detail == null ? "" : oEmployeesalaryschedule.detail;
            command.Parameters.Add("schedulemonth", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalaryschedule.schedulemonth;
            command.Parameters.Add("salaryheaderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryschedule.salaryheaderid;
            command.Parameters.Add("payateom", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryschedule.payateom;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployeesalaryschedule.remarks == null ? "" : oEmployeesalaryschedule.remarks; ;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmployeesalaryschedule.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oEmployeesalaryschedule;
        }

        internal bool DeleteAll(List<Employeesalaryschedule> items)
        {
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var item in items)
                        Delete(transaction, conn, item);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
                return false;
            }
            return true;
        }
        public bool Delete(Employeesalaryschedule oEmployeesalaryschedule)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oEmployeesalaryschedule);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeesalaryschedule);
            }
            return hasSuccessCount;
        }

        public bool DeleteAll(NpgsqlTransaction transaction, NpgsqlConnection conn,
            EmployeeAdvance oEmployeesalaryschedule, ScheduleType sourcetype)
        {
            if (oEmployeesalaryschedule.searchmode == 3 && sourcetype != ScheduleType.Fine)
            {
                string SQL_DeleteAll = @"update employeesalaryschedule set isactive = false where source=1 and 
                salaryheaderid=@salaryheaderid and schedulemonth=@schedulemonth and employeeid=@employeeid";
                NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
                command.Parameters.Add("salaryheaderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryschedule.salaryheaderid;
                command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryschedule.schedules.First().employeeid;
                command.Parameters.Add("schedulemonth", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalaryschedule.schedules.First().advancedate;
                int successCount = command.ExecuteNonQuery();
                return successCount > 0;
            }
            else if (sourcetype == ScheduleType.Fine)
            {
                string SQL_DeleteAll = @"update employeesalaryschedule set isactive = false where source=3 and 
                salaryheaderid=@salaryheaderid and schedulemonth=@schedulemonth and companyid=@companyid and locationid=@locationid";
                NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
                command.Parameters.Add("salaryheaderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryschedule.salaryheaderid;
                command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryschedule.companyid;
                command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryschedule.locationid;
                command.Parameters.Add("schedulemonth", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalaryschedule.schedules.First().advancedate;
                int successCount = command.ExecuteNonQuery();
                return successCount > 0;
            }
            else
            {
                string SQL_DeleteAll = @"update employeesalaryschedule set isactive = false where source=1 and salaryheaderid=@salaryheaderid and schedulemonth=@schedulemonth";
                NpgsqlCommand command = GetCommand(SQL_DeleteAll, conn, transaction);
                command.Parameters.Add("salaryheaderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryschedule.salaryheaderid;
                command.Parameters.Add("schedulemonth", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalaryschedule.salarymonth.ToLocalTime().Date;
                int successCount = command.ExecuteNonQuery();
                return successCount > 0;
            }
            //return false;
        }


        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeesalaryschedule oEmployeesalaryschedule)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalaryschedule.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalaryschedule.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalaryschedule.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
