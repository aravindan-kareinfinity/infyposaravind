using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class ShiftAllocationDataService : SAASData
    {
        const string SQL_Search = @"select id, departmentid, companyid, monthid, year,allocation, isactive, createdby, modifiedby, version, createdon, modifiedon,fromdate,todate from shiftallocation";

        const string SQL_SearchEmployee = @"select id,name,employeecode,hasleft,dateofjoining,relievingdate,sectionid,fixedleaveday from employee where employee.isactive = true and (employee.hasleft = false or employee.relievingdate>@todate)";

        const string SQL_Insert = @"
                        UPDATE public.shiftallocation set isactive=false where year=@year and monthid=@monthid;
                        INSERT INTO public.shiftallocation(departmentid, companyid, locationid, monthid, year,allocation,isactive,fromdate,todate,createdby, modifiedby, version, createdon, modifiedon,floorid,sectionid) 
                        VALUES(@departmentid, @companyid,@locationid, @monthid, @year,@allocation,@isactive,@fromdate, @todate, @createdby, @modifiedby, @version, @createdon, @modifiedon,@floorid,@sectionid) RETURNING id";


        const string SQL_Update = @"UPDATE public.shiftallocation SET departmentid=@departmentid,locationid=@locationid, companyid=@companyid,floorid=@floorid, monthid=@monthid, year=@year,allocation=@allocation,
                                        sectionid=@sectionid, isactive=@isactive, fromdate=@fromdate,todate=@todate, modifiedby=@modifiedby, version=@version+1, modifiedon=@modifiedon where id=@id and version=@version  RETURNING version";

        const string SQL_Delete = @"update public.shiftallocation set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version RETURNING version";

        const string SQL_DeleteBylocation = @"update public.shiftallocation set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon";

        public ShiftAllocation Search(ShiftAllocationCreteria _shiftallocation)
        {
            ShiftAllocation oshiftallocation = new ShiftAllocation();
            oshiftallocation.departmentid = _shiftallocation.departmentid;
            oshiftallocation.floorid = _shiftallocation.floorid;
            oshiftallocation.sectionid = _shiftallocation.sectionid;
            oshiftallocation.companyid = _shiftallocation.companyid;
            oshiftallocation.locationid = _shiftallocation.locationid;
            oshiftallocation.monthid = _shiftallocation.monthid;
            oshiftallocation.year = _shiftallocation.year;
            oshiftallocation.allocation = new List<ShiftAllocation.Allocation>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_Search;
                qb.AddParameter("isactive", true, "=");
                if (_shiftallocation.year != 0)
                    qb.AddParameter("year", _shiftallocation.year, "=");
                if (_shiftallocation.monthid != 0)
                    qb.AddParameter("monthid", _shiftallocation.monthid, "=");

                //if (!_shiftallocation.fetchall)
                //{
                //    if (_shiftallocation.departmentid != 0)
                //        qb.AddParameter("departmentid", _shiftallocation.departmentid, "=");

                //    if (_shiftallocation.floorid != 0)
                //        qb.AddParameter("floorid", _shiftallocation.floorid, "=");
                //    if (_shiftallocation.sectionid != 0)
                //        qb.AddParameter("sectionid", _shiftallocation.sectionid, "=");
                //    if (_shiftallocation.companyid != 0)
                //        qb.AddParameter("companyid", _shiftallocation.companyid, "=");

                if (_shiftallocation.locationid != 0)
                    qb.AddParameter("locationid", _shiftallocation.locationid, "=");

                //}
                qb.SortField = "id";
                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        if (oshiftallocation.id == 0)
                        {
                            oshiftallocation.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                            oshiftallocation.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                            oshiftallocation.allocation_json = reader["allocation"] == DBNull.Value ? "null" : reader["allocation"].ToString();
                            oshiftallocation.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"].ToString());
                            oshiftallocation.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"].ToString());
                            oshiftallocation.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"].ToString());
                            oshiftallocation.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"].ToString());
                            oshiftallocation.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"].ToString());
                        }
                        else
                        {
                            var lastAllocation = oshiftallocation.allocation;
                            oshiftallocation.allocation_json = reader["allocation"] == DBNull.Value ? "null" : reader["allocation"].ToString();
                            lastAllocation.RemoveAll(e => oshiftallocation.allocation.Exists(x => x.shifts != null && e.shifts.Count > 0 && x.employeeid == e.employeeid));
                            oshiftallocation.allocation.AddRange(lastAllocation);
                        }
                    }
                }

                //if (_shiftallocation.regenerate)
                //{
                //    var transaction = conn.BeginTransaction();
                //    Delete(transaction, conn, oshiftallocation);
                //    transaction.Commit();

                //    oshiftallocation.id = 0;
                //    oshiftallocation.allocation = new List<ShiftAllocation.Allocation>();
                //}


                command = null;
                qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SearchEmployee;

                if (_shiftallocation.locationid != 0)
                    qb.AddParameter("locationid", _shiftallocation.locationid, "=");

                if (!_shiftallocation.fetchallemployees)
                {
                    if (_shiftallocation.departmentid != 0)
                        qb.AddParameter("departmentid", _shiftallocation.departmentid, "=");

                    if (_shiftallocation.floorid != 0)
                        qb.AddParameter("floorid", _shiftallocation.floorid, "=");

                    if (_shiftallocation.sectionid != 0)
                        qb.AddParameter("sectionid", _shiftallocation.sectionid, "=");

                    if (_shiftallocation.companyid != 0)
                        qb.AddParameter("companyid", _shiftallocation.companyid, "=");

                    if (_shiftallocation.salarymode != 0)
                        qb.AddParameter("salarymode", _shiftallocation.salarymode, "=");

                    if (_shiftallocation.workingmode != 0)
                        qb.AddParameter("workingmode", _shiftallocation.workingmode, "=");
                }

                command = qb.GetCommand(conn);

                DateTime lastdate = new DateTime(_shiftallocation.year, _shiftallocation.monthid, 1);
                command.Parameters.AddWithValue("todate", lastdate);

                List<long> filteredEmpList = new List<long>();

                using (System.Data.IDataReader readeremp = command.ExecuteReader())
                {
                    while (readeremp.Read())
                    {
                        long employeeid = readeremp.GetInt64(0);
                        string employeename = readeremp.GetString(1);
                        string employeecode = readeremp.GetString(2);
                        bool hasleft = readeremp.IsDBNull(3) ? false : readeremp.GetBoolean(3);
                        DateTime dateofjoin = readeremp.IsDBNull(4) ? DateTime.MinValue : readeremp.GetDateTime(4);
                        DateTime dateofrelieaving = readeremp.IsDBNull(5) ? DateTime.MinValue : readeremp.GetDateTime(5);
                        filteredEmpList.Add(employeeid);
                        if (oshiftallocation.allocation.Exists(e => e.employeeid == employeeid))
                        {
                            var emp = oshiftallocation.allocation.Find(e => e.employeeid == employeeid);
                            emp.name = employeename;
                            emp.Nhasleft = hasleft;
                            emp.Ndateofjoin = dateofjoin;
                            emp.Ndateofrelieaving = dateofrelieaving;
                            emp.existing = true;
                            emp.sectionid = readeremp.IsDBNull(6) ? 0 : readeremp.GetInt64(6);
                            emp.fixedleaveday = readeremp.IsDBNull(7) ? 0 : readeremp.GetInt32(7);
                        }
                        else
                        {
                            oshiftallocation.allocation.Add(new ShiftAllocation.Allocation()
                            {
                                employeeid = employeeid,
                                name = employeename,
                                employeecode = employeecode,
                                hasleft = hasleft,
                                dateofjoin = dateofjoin,
                                dateofrelieaving = dateofrelieaving,
                                sectionid = readeremp.IsDBNull(6) ? 0 : readeremp.GetInt64(6),
                                fixedleaveday = readeremp.IsDBNull(7) ? 0 : readeremp.GetInt32(7),
                                existing = false
                            });
                        }
                    }
                }

                oshiftallocation.allocation.RemoveAll(e => !filteredEmpList.Contains(e.employeeid));
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _shiftallocation);
            }

            DateTime fromdate = new DateTime(_shiftallocation.year, _shiftallocation.monthid, 1);
            if (_shiftallocation.weekbased)
            {
                while (fromdate.DayOfWeek != DayOfWeek.Monday)
                    fromdate = fromdate.AddDays(1);
            }

            DateTime todate = (new DateTime(_shiftallocation.year, _shiftallocation.monthid, 1)).AddMonths(1).AddDays(-1);


            if (_shiftallocation.weekbased)
            {
                while (todate.DayOfWeek != DayOfWeek.Sunday)
                    todate = todate.AddDays(1);
            }
            else if (_shiftallocation.week4model)
            {
                todate = new DateTime(_shiftallocation.year, _shiftallocation.monthid, 28);
            }

            int noofsundays = 0;

            oshiftallocation.fromdate = fromdate.Date;
            oshiftallocation.todate = todate.Date;

            oshiftallocation.days = new List<DateTime>();
            while (fromdate <= todate)
            {
                noofsundays += fromdate.DayOfWeek == DayOfWeek.Sunday ? 1 : 0;
                oshiftallocation.days.Add(fromdate);
                fromdate = fromdate.AddDays(1);
            }

            oshiftallocation.maxemployeeperday = noofsundays;

            return oshiftallocation;
        }

        private class AllocationKeyVal
        {
            public DayOfWeek day { get; set; }
            public int allocated { get; set; }
            public int max { get; set; }
            public AllocationKeyVal(DayOfWeek day, int allocated)
            {
                this.day = day;
                this.allocated = allocated;
            }
        }

        internal ShiftAllocation Generate(ShiftAllocationCreteria item)
        {
            item.weekbased = false;
            item.week4model = true;
            //SortedList<DayOfWeek, int> splits = new SortedList<DayOfWeek, int>();
            //SortedList<DayOfWeek, List<ShiftAllocation.Allocation>> employeeallocation = new SortedList<DayOfWeek, List<ShiftAllocation.Allocation>>();

            //item.regenerate = true;
            ShiftAllocation result = null;
            if (!item.regenerate)
            {
                result = Search(item);
            }
            else
            {

                DateTime fromdate = new DateTime(item.year, item.monthid, 1);
                if (item.weekbased)
                {
                    while (fromdate.DayOfWeek != DayOfWeek.Monday)
                        fromdate = fromdate.AddDays(1);
                }
                item.fromdate = fromdate;
                DateTime todate = (new DateTime(item.year, item.monthid, 1)).AddMonths(1).AddDays(-1);
                if (item.weekbased)
                {
                    while (todate.DayOfWeek != DayOfWeek.Sunday)
                        todate = todate.AddDays(1);
                }
                else if (item.week4model)
                {
                    todate = new DateTime(item.year, item.monthid, 28);
                }
                item.todate = todate;

                //SortedList<DayOfWeek, double> splitpercentage = new SortedList<DayOfWeek, double>();
                //splitpercentage.Add(DayOfWeek.Monday, 10);
                //splitpercentage.Add(DayOfWeek.Tuesday, 16);
                //splitpercentage.Add(DayOfWeek.Wednesday, 16);
                //splitpercentage.Add(DayOfWeek.Thursday, 16);
                //splitpercentage.Add(DayOfWeek.Friday, 16);
                //splitpercentage.Add(DayOfWeek.Saturday, 16);
                //splitpercentage.Add(DayOfWeek.Sunday, 10);

                //List<AllocationKeyVal> allocatedemployees = new List<AllocationKeyVal>();

                //item.fetchallemployees = true;
                //result = Search(item);

                //allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Monday, 0));
                //allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Tuesday, 0));
                //allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Wednesday, 0));
                //allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Thursday, 0));
                //allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Friday, 0));
                //allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Saturday, 0));
                //allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Sunday, 0));
                //foreach (var aitem in result.allocation)
                //{
                //    if (aitem.shifts.Count > 0)
                //        allocatedemployees.Find(e => e.day == aitem.shifts.First().date.DayOfWeek).allocated++;
                //}

                //var countavailable = result.allocation.Count(e => e.shifts.Count > 0);
                //var countunavailable = result.allocation.Count(e => e.shifts.Count == 0);
                //if (countunavailable > 0)
                //{
                //    int expemployee = result.allocation.Count();
                //    foreach (var kv in splitpercentage)
                //    {
                //        double Percent = Math.Floor((double)(expemployee) * kv.Value / 100);
                //        allocatedemployees.Find(e => e.day == kv.Key).max = (int)Math.Round(Percent, 0);

                //        if (allocatedemployees.Find(e => e.day == kv.Key).max < allocatedemployees.Find(e => e.day == kv.Key).allocated)
                //            allocatedemployees.Find(e => e.day == kv.Key).max = allocatedemployees.Find(e => e.day == kv.Key).allocated;
                //    }

                //    if (allocatedemployees.Sum(e => e.max) < expemployee)
                //    {
                //        countavailable = expemployee - allocatedemployees.Sum(e => e.max);
                //        DayOfWeek current = DayOfWeek.Monday;
                //        if (countavailable > 0)
                //        {
                //            allocatedemployees = allocatedemployees.OrderBy(e => e.allocated).ToList();
                //            current = allocatedemployees.First().day;
                //        }

                //        for (int i = 0; i < countunavailable; i++)
                //        {
                //            allocatedemployees.Find(e => e.day == current).max += 1;
                //            current += 1;
                //            if (current == DayOfWeek.Saturday)
                //                current = DayOfWeek.Sunday;
                //        }
                //    }

                //    var availableresource = result.allocation.FindAll(e => e.shifts.Count == 0);
                //    var leftEmployees = result.allocation.FindAll(e => e.shifts.Count > 0 && e.Nhasleft && e.Ndateofrelieaving >= fromdate && e.Ndateofrelieaving <= todate);
                //    var newEmployees = availableresource.FindAll(e => e.Ndateofjoin >= fromdate && e.Ndateofjoin <= todate);
                //    if (leftEmployees.Count > 0 && newEmployees.Count > 0)
                //    {
                //        foreach (var leftemp in leftEmployees)
                //        {
                //            if (newEmployees.Count == 0)
                //            {
                //                availableresource = result.allocation.FindAll(e => e.shifts.Count == 0);
                //                break;
                //            }
                //            var newEmpAllocation = leftemp.shifts.ToArray().ToList();
                //            var newEmp = newEmployees.First();
                //            newEmp.shifts = newEmpAllocation;
                //            newEmployees.Remove(newEmp);
                //        }
                //    }
                //    Random rnd = new Random();
                //    foreach (var allocation in allocatedemployees)
                //    {
                //        while (allocation.max > allocation.allocated && availableresource.Count>0)
                //        {
                //            var empIndex = rnd.Next(0, availableresource.Count);
                //            var empAllocation = availableresource[empIndex];
                //            empAllocation.day = allocation.day;
                //            empAllocation.shifts = new List<ShiftAllocation.AllocationShift>();
                //            for (DateTime fdate = fromdate; fdate <= todate; fdate = fdate.AddDays(1))
                //            {
                //                if (fdate.DayOfWeek == allocation.day)
                //                {
                //                    empAllocation.shifts.Add(new ShiftAllocation.AllocationShift()
                //                    {
                //                        date = fdate,
                //                        shift = 0
                //                    });
                //                }
                //            }
                //            allocation.allocated++;
                //            availableresource.RemoveAt(empIndex);
                //        }
                //    }


                /*
                int index = 0;
                foreach (var kv in splits)
                {
                    employeeallocation.Add(kv.Key, new List<ShiftAllocation.Allocation>());
                    for (int i = 0; i < kv.Value; i++)
                    {
                        employeeallocation[kv.Key].Add(availableresource[index]);
                        availableresource.RemoveAt(index);
                    }
                }

                SortedList<DayOfWeek, List<ShiftAllocation.Allocation>> employeeallocationsuffled = new SortedList<DayOfWeek, List<ShiftAllocation.Allocation>>();
                int start = (item.year * 12 + item.monthid) % 7;
                for (int i = 0; i < 7; i++)
                {
                    DayOfWeek from = (DayOfWeek)i;
                    DayOfWeek to = (DayOfWeek)start;
                    employeeallocationsuffled.Add(to, employeeallocation[from]);
                    start++;
                    if (start == 7)
                        start = 0;
                }

                foreach (var kv in splits)
                {
                    if (employeeallocationsuffled[kv.Key].Count > kv.Value)
                    {
                        List<ShiftAllocation.Allocation> extra = new List<ShiftAllocation.Allocation>();
                        int extracount = employeeallocationsuffled[kv.Key].Count - kv.Value;
                        for (int i = 0; i < extracount; i++)
                        {
                            var last = employeeallocationsuffled[kv.Key].Last();
                            extra.Add(last);
                            employeeallocationsuffled[kv.Key].Remove(last);
                        }
                        start = (int)kv.Key;
                        start++;
                        if (start == 7)
                            start = 0;
                        for (int i = 0; i < 7; i++)
                        {

                            int remaining = splits[(DayOfWeek)start] - employeeallocationsuffled[(DayOfWeek)start].Count;
                            if (remaining > 0)
                            {
                                for (int r = 0; r < remaining; r++)
                                {
                                    if (extra.Count > 0)
                                    {
                                        var last = extra.Last();
                                        employeeallocationsuffled[(DayOfWeek)start].Add(last);
                                        extra.Remove(last);
                                    }
                                }
                            }
                            start++;
                            if (start == 7)
                                start = 0;
                        }

                        if (extra.Count > 0)
                        {
                            employeeallocationsuffled[kv.Key].AddRange(extra);
                        }
                    }
                }

                employeeallocation.Clear();
                foreach (var kv in employeeallocationsuffled)
                {
                    employeeallocation.Add(kv.Key, kv.Value);
                }
                */

                //for (; fromdate < todate; fromdate = fromdate.AddDays(1))
                //{
                //    if (employeeallocation.ContainsKey(fromdate.DayOfWeek))
                //    {
                //        employeeallocation[fromdate.DayOfWeek].ForEach(e => e.shifts.Add(new ShiftAllocation.AllocationShift()
                //        {
                //            date = fromdate,
                //            shift = 0
                //        }));
                //    }
                //}




                //if (result.id > 0)
                //{
                //    result.modifiedon = DateTimeNow;
                //    Update(result);
                //}
                //else
                //{
                item.fetchallemployees = true;
                result = Search(item);
                result.createdon = DateTimeNow;

                var fullAllocation = result.allocation;
                var newallocation = new List<ShiftAllocation.Allocation>();
                if (item.sectionid > 0)
                {
                    foreach (var groups in fullAllocation.GroupBy(e => e.sectionid))
                    {
                        result.allocation = groups.ToList();
                        GenerateShift(result, fromdate, todate);
                        newallocation.AddRange(result.allocation);
                    }
                }
                else
                {
                    result.allocation = fullAllocation.ToList();
                    GenerateShift(result, fromdate, todate);
                    newallocation.AddRange(result.allocation);
                }

                result.allocation = newallocation;
                result.allocation.ForEach(e => e.existing = true);
                result.allocation.ForEach(e =>
                {
                    e.existing = true;
                    if (e.shifts != null && e.shifts.Count > 0)
                        e.day = e.shifts.First().date.DayOfWeek;
                });

                Insert(result);
                item.fetchallemployees = false;
                result = Search(item);
            }

            result.summary = new List<ShiftAllocation.Summary>();
            foreach (string dowstring in Enum.GetNames(typeof(DayOfWeek)))
            {
                ShiftAllocation.Summary summary = new ShiftAllocation.Summary()
                {
                    day = dowstring,
                    days = "",
                    employees = new List<ShiftAllocation.Employee>()
                };

                List<DateTime> dates = new List<DateTime>();
                foreach (var allocationitem in result.allocation.FindAll(e => e.shifts != null && e.shifts.Count > 0 && dowstring == e.shifts.First().date.DayOfWeek.ToString()))
                {
                    summary.employees.Add(new ShiftAllocation.Employee()
                    {
                        employeeid = allocationitem.employeeid,
                        name = allocationitem.name,
                        employeecode = allocationitem.employeecode
                    });
                    foreach (var shift in allocationitem.shifts)
                    {
                        if (!dates.Contains(shift.date))
                            dates.Add(shift.date);
                    }
                }
                foreach (var days in dates)
                {
                    summary.days += (summary.days.Length > 0 ? "," : "") + days.ToString("dd");
                }
                if (summary.employees.Count > 0)
                    result.summary.Add(summary);
            }
            return result;
        }

        public void GenerateShift(ShiftAllocation result, DateTime fromdate, DateTime todate)
        {

            SortedList<DayOfWeek, double> splitpercentage = new SortedList<DayOfWeek, double>();
            splitpercentage.Add(DayOfWeek.Monday, 10);
            splitpercentage.Add(DayOfWeek.Tuesday, 16);
            splitpercentage.Add(DayOfWeek.Wednesday, 16);
            splitpercentage.Add(DayOfWeek.Thursday, 16);
            splitpercentage.Add(DayOfWeek.Friday, 16);
            splitpercentage.Add(DayOfWeek.Saturday, 16);
            splitpercentage.Add(DayOfWeek.Sunday, 10);

            List<AllocationKeyVal> allocatedemployees = new List<AllocationKeyVal>();


            allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Monday, 0));
            allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Tuesday, 0));
            allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Wednesday, 0));
            allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Thursday, 0));
            allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Friday, 0));
            allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Saturday, 0));
            allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Sunday, 0));
            foreach (var aitem in result.allocation)
            {
                if (aitem.shifts.Count > 0)
                    allocatedemployees.Find(e => e.day == aitem.shifts.First().date.DayOfWeek).allocated++;
            }

            var countavailable = result.allocation.Count(e => e.shifts.Count > 0);
            var countunavailable = result.allocation.Count(e => e.shifts.Count == 0);
            if (countunavailable > 0)
            {
                int expemployee = result.allocation.Count();
                foreach (var kv in splitpercentage)
                {
                    double Percent = Math.Floor((double)(expemployee) * kv.Value / 100);
                    allocatedemployees.Find(e => e.day == kv.Key).max = (int)Math.Round(Percent, 0);

                    if (allocatedemployees.Find(e => e.day == kv.Key).max < allocatedemployees.Find(e => e.day == kv.Key).allocated)
                        allocatedemployees.Find(e => e.day == kv.Key).max = allocatedemployees.Find(e => e.day == kv.Key).allocated;
                }

                if (allocatedemployees.Sum(e => e.max) < expemployee)
                {
                    countunavailable = expemployee - allocatedemployees.Sum(e => e.max);
                    DayOfWeek current = DayOfWeek.Monday;
                    if (countunavailable > 0)
                    {
                        allocatedemployees = allocatedemployees.OrderBy(e => e.allocated).ToList();
                        current = allocatedemployees.First().day;
                    }

                    for (int i = 0; i < countunavailable; i++)
                    {
                        allocatedemployees.Find(e => e.day == current).max += 1;
                        if (current == DayOfWeek.Saturday)
                            current = DayOfWeek.Sunday;
                        else
                            current += 1;
                    }
                }

                var availableresource = result.allocation.FindAll(e => e.shifts.Count == 0);
                var leftEmployees = result.allocation.FindAll(e => e.shifts.Count > 0 && e.Nhasleft && e.Ndateofrelieaving >= fromdate && e.Ndateofrelieaving <= todate);
                var newEmployees = availableresource.FindAll(e => e.Ndateofjoin >= fromdate && e.Ndateofjoin <= todate);
                if (leftEmployees.Count > 0 && newEmployees.Count > 0)
                {
                    foreach (var leftemp in leftEmployees)
                    {
                        if (newEmployees.Count == 0)
                        {
                            availableresource = result.allocation.FindAll(e => e.shifts.Count == 0);
                            break;
                        }
                        var newEmpAllocation = leftemp.shifts.ToArray().ToList();
                        var newEmp = newEmployees.First();
                        newEmp.shifts = newEmpAllocation;
                        newEmployees.Remove(newEmp);
                    }
                }
                Random rnd = new Random();
                foreach (var allocation in allocatedemployees)
                {
                    while (allocation.max > allocation.allocated && availableresource.Count > 0)
                    {
                        var empIndex = rnd.Next(0, availableresource.Count);
                        var empAllocation = availableresource[empIndex];
                        empAllocation.day = allocation.day;
                        empAllocation.shifts = new List<ShiftAllocation.AllocationShift>();
                        for (DateTime fdate = fromdate; fdate <= todate; fdate = fdate.AddDays(1))
                        {
                            if (fdate.DayOfWeek == allocation.day)
                            {
                                empAllocation.shifts.Add(new ShiftAllocation.AllocationShift()
                                {
                                    date = fdate,
                                    shift = 0
                                });
                            }
                        }
                        allocation.allocated++;
                        availableresource.RemoveAt(empIndex);
                    }
                }

                result.allocation.ForEach(e => e.existing = true);
            }
        }


        public ShiftAllocation Insert(ShiftAllocation oshiftallocation)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oshiftallocation);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oshiftallocation;
        }


        internal ShiftAllocation GenerateIrregular(ShiftAllocationCreteria item)
        {
            SortedList<DayOfWeek, double> splitpercentage = new SortedList<DayOfWeek, double>();
            splitpercentage.Add(DayOfWeek.Monday, 10);
            splitpercentage.Add(DayOfWeek.Tuesday, 16);
            splitpercentage.Add(DayOfWeek.Wednesday, 16);
            splitpercentage.Add(DayOfWeek.Thursday, 16);
            splitpercentage.Add(DayOfWeek.Friday, 16);
            splitpercentage.Add(DayOfWeek.Saturday, 16);
            splitpercentage.Add(DayOfWeek.Sunday, 10);

            List<AllocationKeyVal> allocatedemployees = new List<AllocationKeyVal>();


            SortedList<DayOfWeek, int> splits = new SortedList<DayOfWeek, int>();
            SortedList<DayOfWeek, List<ShiftAllocation.Allocation>> employeeallocation = new SortedList<DayOfWeek, List<ShiftAllocation.Allocation>>();

            item.regenerate = true;
            var result = Search(item);
            allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Monday, 0));
            allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Tuesday, 0));
            allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Wednesday, 0));
            allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Thursday, 0));
            allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Friday, 0));
            allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Saturday, 0));
            allocatedemployees.Add(new AllocationKeyVal(DayOfWeek.Sunday, 0));
            foreach (var aitem in result.allocation)
            {
                if (aitem.shifts.Count > 0)
                    allocatedemployees.Find(e => e.day == aitem.shifts.First().date.DayOfWeek).allocated++;
            }

            var countavailable = result.allocation.Count(e => e.shifts.Count > 0);
            var countunavailable = result.allocation.Count(e => e.shifts.Count == 0);
            if (countunavailable > 0)
            {
                int totemployee = 0;
                foreach (var kv in splitpercentage)
                {
                    double Percent = Math.Floor((double)(countunavailable + countavailable) * kv.Value / 100);
                    totemployee += (int)Math.Round(Percent, 0);
                    splits.Add(kv.Key, (int)Math.Round(Percent, 0));
                }

                DayOfWeek current = DayOfWeek.Sunday;
                if (countavailable > 0)
                {
                    current = allocatedemployees.OrderBy(e => e.allocated).First().day;
                }

                for (int i = 0; i < countunavailable - totemployee; i++)
                {
                    current += 1;
                    splits[current] += 1;
                    if (current == DayOfWeek.Saturday)
                        current = DayOfWeek.Sunday;
                }

                var availableresource = result.allocation.FindAll(e => e.shifts.Count == 0);

                int index = 0;
                foreach (var kv in splits)
                {
                    employeeallocation.Add(kv.Key, new List<ShiftAllocation.Allocation>());
                    for (int i = 0; i < kv.Value; i++)
                    {
                        employeeallocation[kv.Key].Add(availableresource[index]);
                        availableresource.RemoveAt(index);
                    }
                }

                SortedList<DayOfWeek, List<ShiftAllocation.Allocation>> employeeallocationsuffled = new SortedList<DayOfWeek, List<ShiftAllocation.Allocation>>();
                int start = (item.year * 12 + item.monthid) % 7;
                for (int i = 0; i < 7; i++)
                {
                    DayOfWeek from = (DayOfWeek)i;
                    DayOfWeek to = (DayOfWeek)start;
                    employeeallocationsuffled.Add(to, employeeallocation[from]);
                    start++;
                    if (start == 7)
                        start = 0;
                }
                foreach (var kv in splits)
                {
                    if (employeeallocationsuffled[kv.Key].Count > kv.Value)
                    {
                        List<ShiftAllocation.Allocation> extra = new List<ShiftAllocation.Allocation>();
                        int extracount = employeeallocationsuffled[kv.Key].Count - kv.Value;
                        for (int i = 0; i < extracount; i++)
                        {
                            var last = employeeallocationsuffled[kv.Key].Last();
                            extra.Add(last);
                            employeeallocationsuffled[kv.Key].Remove(last);
                        }
                        start = (int)kv.Key;
                        start++;
                        if (start == 7)
                            start = 0;
                        for (int i = 0; i < 7; i++)
                        {

                            int remaining = splits[(DayOfWeek)start] - employeeallocationsuffled[(DayOfWeek)start].Count;
                            if (remaining > 0)
                            {
                                for (int r = 0; r < remaining; r++)
                                {
                                    if (extra.Count > 0)
                                    {
                                        var last = extra.Last();
                                        employeeallocationsuffled[(DayOfWeek)start].Add(last);
                                        extra.Remove(last);
                                    }
                                }
                            }
                            start++;
                            if (start == 7)
                                start = 0;
                        }

                        if (extra.Count > 0)
                        {
                            employeeallocationsuffled[kv.Key].AddRange(extra);
                        }
                    }
                }

                employeeallocation.Clear();
                foreach (var kv in employeeallocationsuffled)
                {
                    employeeallocation.Add(kv.Key, kv.Value);
                }

                DateTime fromdate = result.fromdate;
                DateTime todate = result.todate;

                for (; fromdate < todate; fromdate = fromdate.AddDays(1))
                {
                    if (employeeallocation.ContainsKey(fromdate.DayOfWeek))
                    {
                        employeeallocation[fromdate.DayOfWeek].ForEach(e => e.shifts.Add(new ShiftAllocation.AllocationShift()
                        {
                            date = fromdate,
                            shift = 0
                        }));
                    }
                }

                if (result.id > 0)
                    Update(result);
                else
                    Insert(result);
            }


            result.summary = new List<ShiftAllocation.Summary>();
            foreach (string dowstring in Enum.GetNames(typeof(DayOfWeek)))
            {
                ShiftAllocation.Summary summary = new ShiftAllocation.Summary()
                {
                    day = dowstring,
                    days = "",
                    employees = new List<ShiftAllocation.Employee>()
                };

                List<DateTime> dates = new List<DateTime>();
                foreach (var allocationitem in result.allocation.FindAll(e => dowstring == e.shifts.First().date.DayOfWeek.ToString()))
                {
                    summary.employees.Add(new ShiftAllocation.Employee()
                    {
                        employeeid = allocationitem.employeeid,
                        name = allocationitem.name,
                        employeecode = allocationitem.employeecode
                    });
                    foreach (var shift in allocationitem.shifts)
                    {
                        if (!dates.Contains(shift.date))
                            dates.Add(shift.date);
                    }
                }
                foreach (var days in dates)
                {
                    summary.days += (summary.days.Length > 0 ? "," : "") + days.ToString("dd");
                }
                if (summary.employees.Count > 0)
                    result.summary.Add(summary);
            }
            return result;
        }




        public ShiftAllocation Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, ShiftAllocation oshiftallocation)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oshiftallocation.id;
            command.Parameters.Add("departmentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oshiftallocation.departmentid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oshiftallocation.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oshiftallocation.locationid;
            command.Parameters.Add("monthid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oshiftallocation.monthid;
            command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oshiftallocation.floorid;
            command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oshiftallocation.sectionid;
            command.Parameters.Add("year", NpgsqlTypes.NpgsqlDbType.Integer).Value = oshiftallocation.year;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = true;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oshiftallocation.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("allocation", NpgsqlTypes.NpgsqlDbType.Json).Value = oshiftallocation.allocation == null ? "null" : oshiftallocation.allocation_json;
            command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oshiftallocation.fromdate;
            command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oshiftallocation.todate;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oshiftallocation.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oshiftallocation;
        }

        public ShiftAllocation Update(ShiftAllocation oshiftallocation)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oshiftallocation);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);

            }

            return oshiftallocation;
        }

        public ShiftAllocation Update(NpgsqlTransaction transaction, NpgsqlConnection conn, ShiftAllocation oshiftallocation)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oshiftallocation.id;
            command.Parameters.Add("departmentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oshiftallocation.departmentid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oshiftallocation.companyid;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oshiftallocation.locationid;
            command.Parameters.Add("monthid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oshiftallocation.monthid;
            command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oshiftallocation.floorid;
            command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oshiftallocation.sectionid;
            command.Parameters.Add("year", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oshiftallocation.year;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oshiftallocation.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oshiftallocation.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oshiftallocation.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oshiftallocation.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("allocation", NpgsqlTypes.NpgsqlDbType.Json).Value = oshiftallocation.allocation_json == null ? "null" : oshiftallocation.allocation_json;
            command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oshiftallocation.fromdate;
            command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oshiftallocation.todate;

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oshiftallocation.version = Convert.ToInt32(reader[0]);
                }
            }

            return oshiftallocation;
        }


        public bool Delete(ShiftAllocation oshiftallocation)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oshiftallocation);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {

                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, ShiftAllocation oshiftallocation)
        {
            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            if (oshiftallocation.id == 0)
            {
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_DeleteBylocation;
                if (oshiftallocation.year != 0)
                    qb.AddParameter("year", oshiftallocation.year, "=");
                if (oshiftallocation.monthid != 0)
                    qb.AddParameter("monthid", oshiftallocation.monthid, "=");

                qb.AddParameter("locationid", oshiftallocation.locationid, "=");
                qb.AddParameter("sectionid", oshiftallocation.sectionid, "=");
                command = qb.GetCommand(conn, transaction);
            }
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oshiftallocation.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oshiftallocation.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oshiftallocation.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
