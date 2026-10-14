using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using InfyPOS.Payroll;

namespace InfyPOS.Data

{
    public class IncentivesDataService : SAASData
    {
        const string SQL_Search = @"select id, month, year, datatype, data, parameter, isactive, createdby, modifiedby, version, createdon, modifiedon,locationid from incentives where id=@id";


        const string SQL_Insert = @"delete from incentives where year=@year and month=@month and datatype=@datatype and locationid=@locationid;
        INSERT INTO public.incentives(month, year, datatype, data, parameter, isactive, createdby, modifiedby, version, createdon, modifiedon,locationid) VALUES(@month, @year, @datatype, @data, @parameter, @isactive, @createdby, @modifiedby, @version, @createdon, @modifiedon,@locationid) RETURNING id";


        const string SQL_Update = @"UPDATE public.incentives SET month=@month, year=@year, datatype=@datatype, data=@data, parameter=@parameter, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon where id=@id and version=@version RETURNING version";



        const string SQL_Delete = @"update public.incentives set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Incentives.SummaryData> SearchSummary(IncentivesCreteria _incentives)
        {
            List<Incentives.SummaryData> result = new List<Incentives.SummaryData>();
            _incentives.CalculationType = Incentives.Calculationtype.beta;
            var beta = Search(_incentives);
            if (beta != null && beta.Count == 1)
            {
                var betadata = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Payroll.Beta.BetaData>>(beta[0].data_json);
                foreach (var item in betadata)
                {
                    result.Add(new Incentives.SummaryData()
                    {
                        beta = item.Beta,
                        employeeid = item.EmployeeID,
                        employeecode = item.Code,
                        employeename = item.Name
                    });
                }
            }

            _incentives.CalculationType = Incentives.Calculationtype.overtime;
            var ot = Search(_incentives);

            if (ot != null && ot.Count == 1)
            {
                var otdata = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Payroll.Overtime.OvertimeData>>(ot[0].data_json);
                foreach (var item in otdata)
                {
                    if (result.Exists(e => e.employeeid == item.EmployeeID))
                    {
                        result.Find(e => e.employeeid == item.EmployeeID).ot = item.Overtime;
                    }
                    else
                    {
                        result.Add(new Incentives.SummaryData()
                        {
                            ot = item.Overtime,
                            employeecode = item.EmployeeCode,
                            employeename = item.Name
                        });
                    }
                }
            }




            _incentives.CalculationType = Incentives.Calculationtype.leavesalary;
            var leave = Search(_incentives);
            if (ot != null && ot.Count == 1)
            {
                var leavedata = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Incentives.LeaveData>>(leave[0].data_json);
                foreach (var item in leavedata)
                {
                    if (result.Exists(e => e.employeeid == item.employeeid))
                    {
                        result.Find(e => e.employeeid == item.employeeid).leavesalary = item.sundaysalary + item.holidaysalary;
                    }
                    else
                    {
                        result.Add(new Incentives.SummaryData()
                        {
                            leavesalary = item.sundaysalary + item.holidaysalary,
                            employeecode = item.employeecode,
                            employeename = item.employeename
                        });
                    }
                }
            }

            return result;

        }



        public List<Incentives> Search(IncentivesCreteria _incentives)
        {
            List<Incentives> lst = new List<Incentives>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_incentives.id != 0)
                {
                    command = GetCommand(SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _incentives.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("isactive", true, "=");

                    qb.AddParameter("datatype", _incentives.datatype, "=");
                    qb.AddParameter("year", _incentives.year, "=");
                    qb.AddParameter("month", _incentives.month, "=");
                    qb.AddParameter("locationid", _incentives.locationid, "=");
                    //if (!string.IsNullOrEmpty(_incentives.Code))
                    //    qb.AddParameter("code", _incentives.Code, "=");

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Incentives oincentives = new Incentives();
                        oincentives.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oincentives.month = reader["month"] == DBNull.Value ? 0 : Convert.ToInt32(reader["month"]);
                        oincentives.year = reader["year"] == DBNull.Value ? 0 : Convert.ToInt32(reader["year"]);
                        oincentives.datatype = reader["datatype"] == DBNull.Value ? 0 : Convert.ToInt32(reader["datatype"]);
                        oincentives.data_json = reader["data"] == DBNull.Value ? null : reader["data"].ToString();
                        oincentives.parameter_json = reader["parameter"] == DBNull.Value ? null : reader["parameter"].ToString();
                        oincentives.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oincentives.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oincentives.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oincentives.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oincentives.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oincentives.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        lst.Add(oincentives);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _incentives);
            }
            return lst;
        }

        internal List<InfyPOS.Payroll.IncentiveSalary.IncentiveData.Item> GenerateIncentive(IncentivesCreteria config)
        {
            if (config.searchmodes != null && config.searchmodes.Count == 0)
                config.searchmodes.Add(0);
            InfyPOS.Payroll.IncentiveSalary incentiveservice = null;

            if (config.bydate)
            {
                incentiveservice = new IncentiveSalary(Context.OrganizationID, config.fromdate, config.todate,
                   config.departmentid, config.sectionid, config.companyid, config.locationid, config.includesales,
                   config.employeeid, config.searchmodes);
            }
            else
            {
                incentiveservice = new IncentiveSalary(Context.OrganizationID, config.month, config.year,
                    config.departmentid, config.sectionid, config.companyid, config.locationid, config.includesales,
                   config.employeeid, config.searchmodes);
            }

            var salaryResult = incentiveservice.Compute(Context.accessiblelocation, config.locationid);
            IncentiveActual<InfyPOS.Payroll.IncentiveSalary.IncentiveData> data = new IncentiveActual<InfyPOS.Payroll.IncentiveSalary.IncentiveData>();
            data.month = config.bydate ? config.todate.Month : config.month;
            data.year = config.bydate ? config.todate.Year : config.year;
            data.CalculationType = Incentives.Calculationtype.incentive;
            data.data = salaryResult;
            data.locationid = config.locationid;
            var exist = Search(new IncentivesCreteria()
            {
                CalculationType = Incentives.Calculationtype.incentive,
                month = config.bydate ? config.todate.Month : config.month,
                year = config.bydate ? config.todate.Year : config.year,
                locationid = config.locationid
            });

            if (config.bydate)
            {
                if (Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_incentiveminimumtarget > 0 &&
                    salaryResult.Exists(e => e.Employees.Exists(x => x.Salebyages != null && x.Salebyages.Count > 0)))
                {
                    salaryResult.ForEach(e =>
                    {
                        e.Employees.ForEach(x =>
                        {
                            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_incentiveminimumtarget > x.salesvalue)
                            {
                                x.incentive = 0;
                            }
                        });

                        e.Employees.RemoveAll(ex => ex.salesvalue == 0 && ex.incentive == 0);
                    });
                }
                return ConverToIncentiveList(config, salaryResult);
            }
            if (exist == null || exist.Count == 0)
            {
                Insert(data);
                return ConverToIncentiveList(config, salaryResult);
            }
            else
            {
                List<IncentiveSalary.IncentiveData> incentivedata = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IncentiveSalary.IncentiveData>>(exist[0].data_json);
                if (config.departmentid == 0)
                {
                    if (config.includesales)
                        incentivedata.Clear();
                    else
                    {
                        incentivedata.RemoveAll(e => !e.IsSales);
                        if (incentivedata.Count == 1)
                        {
                            foreach (var incentive in salaryResult)
                            {
                                incentivedata[0].Employees.RemoveAll(e => incentive.Employees.Exists(f => f.employeeid == e.employeeid));
                            }
                        }
                    }
                }
                else
                {
                    incentivedata.RemoveAll(e => e.departmentid == config.departmentid);
                }

                //if(incentivedata.Count == 1 && incentivedata[0].IsSales)
                //{
                //    salaryResult.ForEach(e => incentivedata[0].Employees.RemoveAll(x => e.Employees.Exists(y => y.employeeid == x.employeeid)));
                //}

                incentivedata.RemoveAll(e => salaryResult.Exists(s => s.departmentid == e.departmentid));

                incentivedata.AddRange(salaryResult);
                exist[0].data_json = Newtonsoft.Json.JsonConvert.SerializeObject(incentivedata);
                Update(exist[0]);
                return ConverToIncentiveList(config, incentivedata);
            }

        }

        public List<InfyPOS.Payroll.IncentiveSalary.IncentiveData.Item> ConverToIncentiveList(IncentivesCreteria config, List<InfyPOS.Payroll.IncentiveSalary.IncentiveData> source)
        {
            long departmentid = config.departmentid;
            long employeesectionid = config.employeesectionid;
            long employeefloorid = config.employeefloorid;
            var result = new List<InfyPOS.Payroll.IncentiveSalary.IncentiveData.Item>();
            foreach (var item in source)
            {
                if (item.departmentid != departmentid && departmentid != 0) continue;
                result.AddRange(item.Employees);

                item.Employees.ForEach(e =>
                {
                    e.departmentid = item.departmentid;
                    e.amount = Math.Round(e.amount, 0);
                    e.incentive = Math.Round(e.incentive, 3);
                    e.salesvalue = Math.Round(e.salesvalue, 0);
                    if (e.Salebyages != null)
                        e.Salebyages.ForEach(ex => ex.incentive = Math.Round(ex.incentive, 0));
                }
               );
            }
            result.Sort(delegate (InfyPOS.Payroll.IncentiveSalary.IncentiveData.Item a, InfyPOS.Payroll.IncentiveSalary.IncentiveData.Item b)
            {
                decimal avalue = 0;
                decimal bvalue = 0;
                decimal.TryParse(a.employeecode, out avalue);
                decimal.TryParse(b.employeecode, out bvalue);
                return avalue.CompareTo(bvalue);
            });

            if (employeesectionid > 0)
                result.RemoveAll(e => e.sectionid != employeesectionid);

            if (employeefloorid > 0)
                result.RemoveAll(e => e.floorid != employeefloorid);

            return result;
        }
        internal List<Overtime.OvertimeData> GenerateOT(IncentivesCreteria config)
        {
            InfyPOS.Payroll.Overtime otservice = new Overtime();
            var salaryREsult = otservice.Compute(this.Context.OrganizationID, config);

            IncentiveActual<Overtime.OvertimeData> data = new IncentiveActual<Overtime.OvertimeData>();
            data.month = config.month;
            data.year = config.year;
            data.CalculationType = Incentives.Calculationtype.overtime;
            data.data = salaryREsult;
            Insert(data);
            return salaryREsult;
        }

        internal List<Beta.BetaData> GenerateBeta(IncentivesCreteria config)
        {
            InfyPOS.Payroll.Beta betaservice = new Beta(config, this.Context.OrganizationID);
            var salaryREsult = betaservice.Compute();

            IncentiveActual<Beta.BetaData> data = new IncentiveActual<Beta.BetaData>();
            data.month = config.month;
            data.year = config.year;
            data.CalculationType = Incentives.Calculationtype.beta;
            data.data = salaryREsult;
            Insert(data);
            return salaryREsult;
        }

        internal List<Bonus.BounsData> GenerateBonus(IncentivesCreteria config)
        {
            InfyPOS.Payroll.Bonus otservice = new Bonus(config, this.Context.OrganizationID);
            var salaryREsult = otservice.Compute();

            IncentiveActual<Bonus.BounsData> data = new IncentiveActual<Bonus.BounsData>();
            data.month = config.month;
            data.year = config.year;
            data.CalculationType = Incentives.Calculationtype.bonus;
            data.data = salaryREsult;
            Insert(data);
            return salaryREsult;
        }

        public List<Incentives.LeaveData> GenerateLeave(IncentivesCreteria config)
        {
            InfyPOS.Payroll.LeaveSalary salary = new Payroll.LeaveSalary(config.month, config.year, this.Context.OrganizationID);
            var salaryREsult = salary.Process(config.employeeid);
            IncentiveActual<Incentives.LeaveData> data = new IncentiveActual<Incentives.LeaveData>();
            data.month = config.month;
            data.year = config.year;
            data.CalculationType = Incentives.Calculationtype.leavesalary;
            data.data = salaryREsult;
            data.totalsunday = salary.TotalSunday;
            data.totalholiday = salary.TotalHoliday;
            Insert(data);
            return salaryREsult;
        }

        public Incentives Insert(Incentives oIncentives)
        {
            oIncentives.isactive = true;
            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oIncentives);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oIncentives);


            }

            return oIncentives;
        }

        public Incentives Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Incentives oIncentives)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("month", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIncentives.month;
            command.Parameters.Add("year", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIncentives.year;
            command.Parameters.Add("datatype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIncentives.datatype;
            command.Parameters.Add("data", NpgsqlTypes.NpgsqlDbType.Json).Value = oIncentives.data_json == null ? "null" : oIncentives.data_json;
            command.Parameters.Add("parameter", NpgsqlTypes.NpgsqlDbType.Json).Value = oIncentives.parameter_json == null ? "null" : oIncentives.parameter_json;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oIncentives.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIncentives.version;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIncentives.locationid;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oIncentives.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oIncentives;
        }

        public Incentives Update(Incentives oIncentives)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oIncentives);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oIncentives);


            }

            return oIncentives;
        }

        public Incentives Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Incentives oIncentives)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIncentives.id;
            command.Parameters.Add("month", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIncentives.month;
            command.Parameters.Add("year", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIncentives.year;
            command.Parameters.Add("datatype", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIncentives.datatype;
            command.Parameters.Add("data", NpgsqlTypes.NpgsqlDbType.Json).Value = oIncentives.data_json == null ? "null" : oIncentives.data_json;
            command.Parameters.Add("parameter", NpgsqlTypes.NpgsqlDbType.Json).Value = oIncentives.parameter_json == null ? "null" : oIncentives.parameter_json;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oIncentives.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIncentives.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIncentives.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oIncentives.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oIncentives.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oIncentives;
        }


        public bool Delete(Incentives oIncentives)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oIncentives);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oIncentives);



            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Incentives oIncentives)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oIncentives.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oIncentives.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oIncentives.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        internal void Update(RetailReportCreteria item, List<IncentiveValue> saleValues)
        {
            IncentiveActual<InfyPOS.Payroll.IncentiveSalary.IncentiveData> data = new IncentiveActual<IncentiveSalary.IncentiveData>();
            data.month = item.fromdate.Month;
            data.year = item.fromdate.Year;
            data.locationid = item.locationid;
            data.CalculationType = Incentives.Calculationtype.incentive;
            var datas = new List<IncentiveSalary.IncentiveData>();
            datas.Add(new IncentiveSalary.IncentiveData());
            datas[0].departmentid = saleValues.First().departmentid;
            datas[0].Amount = saleValues.Sum(e => e.amount);
            datas[0].IsSales = true;
            datas[0].SalesValue = saleValues.GroupBy(e => e.sectionid).ToList().ConvertAll<IncentiveSalary.IncentiveData.salevalue>(x => new IncentiveSalary.IncentiveData.salevalue()
            {
                sectionid = x.Key,
                amount = x.Sum(y => y.amount)
            });
            datas[0].Employees = saleValues.ConvertAll<IncentiveSalary.IncentiveData.Item>(e =>
                    new IncentiveSalary.IncentiveData.Item()
                    {
                        amount = e.amount,
                        employeecode = e.employeecode,
                        employeeid = e.employeeid,
                        employeename = e.employeename,
                        sectionid = e.sectionid,
                        days = e.days,
                        payratio = Math.Round(e.payratio, 2),
                        ratio = Math.Round(e.payratio, 2)
                    });

            datas[0].SalesValue = new List<IncentiveSalary.IncentiveData.salevalue>();
            var exist = Search(new IncentivesCreteria()
            {
                CalculationType = Incentives.Calculationtype.incentive,
                month = item.fromdate.Month,
                year = item.fromdate.Year,
                locationid = item.locationid
            });
            if (exist == null || exist.Count == 0)
            {
                data.data = datas;
                Insert(data);
            }
            else
            {
                List<IncentiveSalary.IncentiveData> incentivedata = Newtonsoft.Json.JsonConvert.DeserializeObject<List<IncentiveSalary.IncentiveData>>(exist[0].data_json);
                incentivedata.RemoveAll(e => e.departmentid == datas[0].departmentid);
                incentivedata.RemoveAll(e => e.IsSales);
                foreach (var idata in incentivedata)
                {
                    saleValues.RemoveAll(e => idata.Employees.Exists(y => y.employeeid == e.employeeid));
                }
                incentivedata.Add(datas[0]);
                exist[0].data_json = Newtonsoft.Json.JsonConvert.SerializeObject(incentivedata);
                Update(exist[0]);
            }
        }

        internal List<IncentiveSalary.IncentiveData.MfgIncentive> MfgIncentive(IncentivesCreteria item)
        {
            List<IncentiveSalary.IncentiveData.MfgIncentive> rows = new List<IncentiveSalary.IncentiveData.MfgIncentive>();
            string query = @"select productid,products.name,sum(billitems.qty) qty,
sum(billitems.receivable) amount from billitems
inner join bill on bill.id = billitems.billid
inner join products on products.id = productid
where bill.billtype != 9 and bill.isactive=true
and billdate>=@fromdate and billdate<@todate
group by productid,products.name";
            var param = new SortedList<string, object>();
            param.Add("fromdate", item.fromdate.Date);
            param.Add("todate", item.todate.Date.AddDays(1));
            using (var reader = ExecuteReader(query, param))
            {
                while(reader.Read())
                {
                    IncentiveSalary.IncentiveData.MfgIncentive mfgIncentive = new IncentiveSalary.IncentiveData.MfgIncentive();
                    mfgIncentive.product = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                    mfgIncentive.qty = reader["qty"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["qty"]);
                    mfgIncentive.productid = reader["productid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["productid"]);
                    mfgIncentive.sales = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                    rows.Add(mfgIncentive);
                }
            }

            ReferencelistDataService referencelistDataService = new ReferencelistDataService();
            var config = referencelistDataService.Search(new ReferencelistCreteria()
            {
                type = "INCENTIVE-PRODUCT-MFG"
            });
            if(config.Count>0 &&
                config[0].additionalinfo.salesincentive != null &&
                config[0].additionalinfo.salesincentive.Count>0)
            {
                foreach(var conf in config[0].additionalinfo.salesincentive)
                {
                    var row = rows.Find(x => x.productid == conf.referenceid);
                    if(row != null)
                    {
                        row.percentage = conf.incentive;
                        row.incentive = conf.value ? (conf.incentive * row.qty) :
                           (conf.incentive * row.sales/100);
                    }
                }
            }
            return rows;
        }

    }

}
