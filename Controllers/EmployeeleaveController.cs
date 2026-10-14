
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using InfyPOS.Models;
using InfyPOS.Common;
using InfyPOS.Data;

namespace InfyPOS.Controllers
{
    [RoutePrefix("EmployeeleaveService")]
    public class EmployeeleaveController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<EmployeeLeave>();
            result.Item = new EmployeeLeave();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<LeaverequestCreteria> request)
        {
            if (request.Item.fromdate.Date == DateTime.MinValue || request.Item.todate.Date == DateTime.MinValue)
            {
                return this.NotFound();
            }
            EmployeeLeaveDataService LeaverequestData = new EmployeeLeaveDataService();
            EmployeeinoutDataService InoutData = new EmployeeinoutDataService();
            List<EmployeeLeave> result = await Task<List<EmployeeLeave>>.Run(() => LeaverequestData.Search(request.Item));
            List<Holiday> listofholidaydate = null;
            Data.HolidayDataService holidaydate = new HolidayDataService();

            DateTime fromdate = request.Item.fromdate;
            DateTime Todate = request.Item.todate;
            listofholidaydate = holidaydate.Search(new HolidayCreteria()
            {

            });
            if (request.Item.searchmode < 3)
                result = result.OrderBy(e => e.fromdate).ToList();

            if (request.Item.searchmode == 3)
            {
                List<EmployeeLeaveSummary> summary = new List<EmployeeLeaveSummary>();
                foreach (var kv in result.GroupBy(e => e.employeeid))
                {
                    summary.Add(new EmployeeLeaveSummary()
                    {
                        name = kv.First().employeename,
                        designation = kv.First().designation,
                        code = kv.First().code,
                        days = kv.Sum(e => e.days),
                        leaves = kv.OrderBy(e=>e.fromdate).ToList()
                    });
                }
                return this.Ok(summary);
            }
            else if (request.Item.searchmode == 4)
            {
                List<Employeeinout> inoutresult = await Task<List<Employeeinout>>.Run(() => InoutData.Search(new EmployeeinoutCreteria()
                {
                    fromdate = request.Item.fromdate,
                    todate = request.Item.todate,
                    employeeid = request.Item.employeeid
                }));

                List<Employee> listofemp = null;
                Data.EmployeeDataService employeedata = new EmployeeDataService();
                if (request.Item.employeeid == 0)
                {
                    listofemp = employeedata.Search(new EmployeeCreteria()
                    {
                        currentEmployees = true,
                        employeecode = request.Item.employeecode,
                        id = request.Item.employeeid,
                        departmentid = request.Item.departmentid,
                        workingmode = request.Item.workingmode,
                        locationid = request.Item.locationid,
                        companyid = request.Item.companyid,
                        sectionid = request.Item.sectionid,
                        salarymode = request.Item.salarymode,
                        floorid = request.Item.floorid
                    });

                    var ids = result.GroupBy(e => e.employeeid).ToList().ConvertAll<long>(x => x.Key);
                    ids.RemoveAll(e => listofemp.Exists(x => x.id == e));
                    if (ids.Count > 0)
                    {
                        var listofempMissing = employeedata.Search(new EmployeeCreteria()
                        {
                            ids = ids
                        });
                        listofemp.AddRange(listofempMissing);
                    }
                }
                else
                {
                    listofemp = employeedata.Search(new EmployeeCreteria()
                    {
                        id = request.Item.employeeid,
                        companyid = request.Item.companyid
                    });
                }

                EmployeeLeaveCheckList summary = new EmployeeLeaveCheckList();
                if (InoutData.DateTimeNow.Date < request.Item.todate.Date)
                    request.Item.todate = InoutData.DateTimeNow.Date;

                int days = request.Item.todate.Date.Subtract(request.Item.fromdate.Date).Days + 1;
                summary.days = new List<EmployeeLeaveCheckList.day>();
                summary.employees = new List<EmployeeLeaveCheckList.Employee>();
                for (DateTime dt = request.Item.fromdate.Date; dt <= request.Item.todate.Date; dt = dt.AddDays(1))
                    summary.days.Add(new EmployeeLeaveCheckList.day(dt, (int)dt.Subtract(request.Item.fromdate).Days));

                



                result = result.OrderBy(e => e.fromdate).ToList();

                foreach (var kv in result.GroupBy(e => e.employeeid))
                {
                    EmployeeLeaveCheckList.Employee emp = new EmployeeLeaveCheckList.Employee();
                    emp.morning = emptydays(days, false);
                    emp.evening = emptydays(days, false);
                    emp.leave = emptydays(days, false);
                    emp.eio = emptydays(days, false);
                    emp.status = emp.evening.ConvertAll(e => "P");

                    emp.id = kv.Key;
                    emp.name = kv.First().employeename;
                    emp.designation = kv.First().designation;
                    emp.code = kv.First().code;
                    emp.days = days;
                    emp.leavedays = kv.Sum(x => x.days);
                    emp.totaldays = summary.days.Count;
                    emp.unemploy = 0;
                    emp.absent = 0;

                    var employe = listofemp.Exists(e=>e.id==kv.Key) ? listofemp.Find(e => e.id == kv.Key) : new Employee();

                    foreach (var item in kv)
                    {
                        int index = (int)item.fromdate.Subtract(request.Item.fromdate).Days;
                        emp.morning[index] = true;
                        emp.evening[index] = true;
                        emp.leave[index] = true;

                        if(employe.dateofjoining>item.fromdate)
                        {
                            continue;
                        }

                        if (employe.hasleft && employe.relievingdate < item.fromdate)
                        {
                            continue;
                        }

                        if (item.fromdate == item.todate)
                        {
                            if (item.days == 1)
                            {
                                emp.morning[index] = true;
                                emp.evening[index] = true;
                                emp.absent++;
                                //var resultcout = result.Count;
                                if (item.status == "L")
                                {
                                    //int daay = item.fromdate.Day;
                                    emp.status[index] = "L";
                                }
                                else
                                {
                                    emp.status[index] = "A";
                                }
                                if (item.status == "OD")
                                {
                                    emp.status[index] = "OD";
                                }
                            }
                            else
                            {
                                if (item.fromdatehalfday == EmployeeLeave.DayMode.Morning ||
                                    item.todatehalfday == EmployeeLeave.DayMode.Morning)
                                {
                                    emp.morning[index] = false;
                                    emp.status[index] = "1H";
                                    emp.absent += (decimal)0.5;
                                    if (item.status == "OD")
                                    {
                                        emp.status[index] = "1OD";
                                    }
                                }
                                if (item.fromdatehalfday == EmployeeLeave.DayMode.Evening || 
                                    item.todatehalfday == EmployeeLeave.DayMode.Evening)
                                {
                                    emp.evening[index] = false;
                                    emp.status[index] = "2H";
                                    emp.absent += (decimal)0.5;
                                    if (item.status == "OD")
                                    {
                                        emp.status[index] = "2OD";
                                    }
                                }
                            }
                        }
                        else
                        {
                            for (DateTime ft = item.fromdate; ft < item.todate; ft = ft.AddDays(1))
                            {
                                if (request.Item.todate < ft) continue;
                                index = (int)ft.Subtract(request.Item.fromdate).Days;
                                if (ft == item.fromdate)
                                {
                                    emp.leave[index] = true;
                                    if (item.fromdatehalfday == EmployeeLeave.DayMode.Both)
                                    {
                                        emp.morning[index] = true;
                                        emp.evening[index] = true;
                                        emp.absent += 1;
                                        if (item.status == "L")
                                        {
                                            emp.status[index] = "L";
                                        }
                                        else
                                        {
                                            emp.status[index] = "A";
                                        }
                                        if (item.status == "OD")
                                        {
                                            emp.status[index] = "OD";
                                        }
                                    }
                                    else if (item.fromdatehalfday == EmployeeLeave.DayMode.Evening)
                                    {
                                        emp.morning[index] = false;
                                        emp.status[index] = "1H";
                                        emp.absent += (decimal)0.5;
                                        if (item.status == "OD")
                                        {
                                            emp.status[index] = "1OD";
                                        }
                                    }
                                    else
                                    {
                                        emp.evening[index] = false;
                                        emp.status[index] = "2H";
                                        emp.absent += (decimal)0.5;
                                        if (item.status == "OD")
                                        {
                                            emp.status[index] = "2OD";
                                        }
                                    }
                                }
                                else if (ft == item.todate)
                                {
                                    if (item.todatehalfday == EmployeeLeave.DayMode.Both)
                                    {
                                        emp.morning[index] = true;
                                        emp.evening[index] = true;
                                        emp.status[index] = "A";
                                        emp.absent += 1;
                                        if (item.status == "OD")
                                        {
                                            emp.status[index] = "OD";
                                        }
                                    }
                                    else if (item.todatehalfday == EmployeeLeave.DayMode.Evening)
                                    {
                                        emp.morning[index] = false;
                                        emp.status[index] = "1H";
                                        emp.absent += (decimal)0.5;
                                        if (item.status == "OD")
                                        {
                                            emp.status[index] = "1OD";
                                        }
                                    }
                                    else
                                    {
                                        emp.evening[index] = false;
                                        emp.status[index] = "2H";
                                        emp.absent += (decimal)0.5;
                                        if (item.status == "OD")
                                        {
                                            emp.status[index] = "2OD";
                                        }
                                    }
                                }
                                else
                                {
                                    emp.morning[index] = true;
                                    emp.evening[index] = true;
                                    emp.status[index] = "L";
                                    emp.absent += 1;
                                    if (item.status == "OD")
                                    {
                                        emp.status[index] = "OD";
                                    }
                                }
                            }
                        }
                    }

                    summary.employees.Add(emp);
                }

               


                //var defaultStatus = emptydays(days, true).ConvertAll(e => "A");
                foreach (var demp in listofemp)
                {
                    if (request.Item.todate < demp.dateofjoining)
                    {
                        summary.employees.RemoveAll(e => e.id == demp.id);
                        continue;
                    }
                    if (demp.hasleft && demp.relievingdate < request.Item.fromdate)
                    {
                        summary.employees.RemoveAll(e => e.id == demp.id);
                        continue;
                    }

                    if (!summary.employees.Exists(e => e.id == demp.id))
                    {
                        EmployeeLeaveCheckList.Employee emp = new EmployeeLeaveCheckList.Employee();
                        emp.morning = emptydays(days, false);
                        emp.evening = emptydays(days, false);
                        emp.leave = emptydays(days, false);
                        emp.status = emp.evening.ConvertAll(e => "P");
                        emp.eio = emptydays(days, false);
                        emp.id = demp.id;
                        emp.name = demp.name;
                        emp.designation = demp.designation;
                        emp.code = demp.employeecode;
                        emp.days = days;
                        emp.unemploy = 0;
                        emp.absent = 0;
                        emp.totaldays = summary.days.Count;
                        if (demp.dateofjoining > request.Item.fromdate)
                        {
                            int upto = (int)demp.dateofjoining.Subtract(request.Item.fromdate).TotalDays;
                            for (int i = 0; i < upto; i++)
                            {
                                emp.status[i] = "-";
                                emp.days--;
                                emp.unemploy++;
                            }
                        }

                        if (demp.hasleft && demp.relievingdate < request.Item.todate)
                        {
                            var i = (int)((demp.relievingdate.AddDays(1).Subtract(request.Item.fromdate)).TotalDays);
                            for (; i < emp.status.Count; i++)
                            {
                                emp.status[i] = "-";
                                emp.days--;
                                emp.unemploy++;
                            }
                        }
                        summary.employees.Add(emp);

                    }
                    else
                    {
                        var smry = summary.employees.Find(e => e.id == demp.id);
                        if (demp.dateofjoining > request.Item.fromdate)
                        {
                            int upto = (int)demp.dateofjoining.Subtract(request.Item.fromdate).TotalDays;
                            for (int i = 0; i < upto; i++)
                            {
                                smry.status[i] = "-";
                                smry.days--;
                                smry.unemploy++;
                            }
                        }

                        if (demp.hasleft && demp.relievingdate < request.Item.todate)
                        {
                            var i = (int)((demp.relievingdate.AddDays(1).Subtract(request.Item.fromdate)).TotalDays);
                            for (; i < smry.status.Count; i++)
                            {
                                smry.status[i] = "-";
                                smry.days--;
                                smry.unemploy++;
                            }
                        }
                    }
                }


                foreach (var item in inoutresult.GroupBy(e => e.employeeid))
                {
                    if (summary.employees.Exists(e => e.id == item.Key))
                    {
                        var empDays = summary.employees.Find(e => e.id == item.Key);
                        if (empDays.leaves == null)
                            empDays.leaves = new List<EmployeeLeaveCheckList.leave>();
                        foreach (var io in item)
                        {
                            int index = (int)io.attendanceon.Date.Subtract(request.Item.fromdate).Days;

                            if (!empDays.leave[index])
                            {
                                empDays.status[index] = "P";
                                empDays.eio[index] = true;
                                if (io.morningabsent && io.afternoonabsent)
                                {
                                    if (!empDays.morning[index] && !empDays.evening[index])
                                    {
                                        empDays.status[index] = "A";
                                        empDays.absent++;
                                        empDays.eio[index] = false;
                                        empDays.leaves.Add(new EmployeeLeaveCheckList.leave() {index = index, status = "A", date = io.attendanceon.Date, morning = true, evening = true });
                                    }
                                }
                                else if (io.morningabsent && !empDays.morning[index])
                                {
                                    empDays.morning[index] = true;
                                    empDays.status[index] = "1H";
                                    empDays.absent += (decimal)0.5;
                                    empDays.leaves.Add(new EmployeeLeaveCheckList.leave() { index = index, status = "1H", date = io.attendanceon.Date, morning = true, evening = false });
                                }
                                else if (io.afternoonabsent && !empDays.evening[index])
                                {
                                    empDays.evening[index] = true;
                                    empDays.status[index] = "2H";
                                    empDays.absent += (decimal)0.5;
                                    empDays.leaves.Add(new EmployeeLeaveCheckList.leave() { index = index, status = "2H", date = io.attendanceon.Date, morning = false, evening = true });
                                }
                            }
                            //else
                            //{
                            //    if (empDays.status[index] == "L")
                            //    {
                            //        empDays.status[index] = "L";
                            //        empDays.absent++;
                            //    }
                            //    else if (empDays.status[index] == "OD")
                            //    {
                            //        empDays.status[index] = "OD";
                            //        empDays.absent++;
                            //    }
                            //    else if (empDays.status[index] == "A")
                            //    {
                            //        empDays.status[index] = "A";
                            //        empDays.absent++;
                            //    }
                            //    else if (io.morningabsent || !empDays.morning[index])
                            //    {
                            //        empDays.status[index] = "1H";
                            //        empDays.absent += (decimal)0.5;
                            //    }
                            //    else if (io.afternoonabsent || !empDays.evening[index])
                            //    {
                            //        empDays.status[index] = "2H";
                            //        empDays.absent += (decimal)0.5;
                            //    }
                            //}
                        }
                    }
                }

                if (request.Item.workdays_exceptholiday == true)
                {
                    for (int i = 0; i < listofholidaydate.Count; i++)
                    {
                        var dateholiday = listofholidaydate[i].holidayon.Date;
                        if (dateholiday >= fromdate && dateholiday <= Todate)
                        {
                            if(summary.days.Exists(e=>e.date == dateholiday))
                            {
                                var index = summary.days.FindIndex(e => e.date == dateholiday);
                                foreach(var emp in summary.employees)
                                {
                                    emp.evening.RemoveAt(index);
                                    emp.morning.RemoveAt(index);
                                    emp.leave.RemoveAt(index);
                                    emp.status.RemoveAt(index);
                                }
                                summary.days.RemoveAt(index);
                            }
                        }
                    }
                    var sdays = summary.days.ToList();
                    foreach(var day in sdays)
                    {
                        if(day.date.DayOfWeek == DayOfWeek.Sunday)
                        {
                            if (summary.days.Exists(e => e.date == day.date))
                            {
                                var index = summary.days.FindIndex(e => e.date == day.date);
                                foreach (var emp in summary.employees)
                                {
                                    emp.evening.RemoveAt(index);
                                    emp.morning.RemoveAt(index);
                                    emp.leave.RemoveAt(index);
                                    emp.status.RemoveAt(index);
                                }
                                summary.days.RemoveAt(index);
                            }
                        }
                    }
                }

                summary.employees = summary.employees.OrderBy(e => Convert2Number(e.code)).ToList();
                return this.Ok(summary);
            }
            else
            {
                return this.Ok(result);
            }
        }

        private int Convert2Number(string code)
        {
            int icode;
            int.TryParse(code, out icode);
            return icode;
        }
        List<bool> emptydays(int days, bool defaultvalue)
        {
            List<bool> lst = new List<bool>();
            for (int i = 0; i < days; i++)
                lst.Add(defaultvalue);

            return lst;
        }

        [HttpPost]
        [Route("Direct")]
        //[WebAPI.InfyAuthorize(PermissionSet.EmployeeleaveController, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Direct(ActionRequest<DirectLeaveChange> request)
        {
            EmployeeLeaveDataService LeaverequestData = new EmployeeLeaveDataService();
            if (request.Item.employeeid == -1 && request.Item.employeeids != null &&
                request.Item.employeeids.Count > 0)
            {
                DirectLeaveChange result = await Task<DirectLeaveChange>.Run(() => LeaverequestData.DirectLeaveChangeAllEmployee(request.Item));
                return this.Ok(result);
            }
            else if (request.Item.employeeid > 0 && request.Item.dates != null &&
               request.Item.dates.Count > 0)
            {
                DirectLeaveChange result = await Task<DirectLeaveChange>.Run(() => LeaverequestData.DirectLeaveChangeAllDates(request.Item));
                return this.Ok(result);
            }
            else
            {
                DirectLeaveChange result = await Task<DirectLeaveChange>.Run(() => LeaverequestData.DirectLeaveChange(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Save")]
        //[WebAPI.InfyAuthorize(PermissionSet.EmployeeleaveController, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Save(ActionRequest<EmployeeLeave> request)
        {
            EmployeeLeaveDataService LeaverequestData = new EmployeeLeaveDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                EmployeeLeave result = await Task<EmployeeLeave>.Run(() => LeaverequestData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                EmployeeLeave result = await Task<EmployeeLeave>.Run(() => LeaverequestData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        //[WebAPI.InfyAuthorize(PermissionSet.EmployeeleaveController, InfyPOS.Common.Action.Create)]
        public async Task<IHttpActionResult> Insert(ActionRequest<EmployeeLeave> request)
        {
            EmployeeLeaveDataService LeaverequestData = new EmployeeLeaveDataService();
            EmployeeLeave result = await Task<EmployeeLeave>.Run(() => LeaverequestData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        //[WebAPI.InfyAuthorize(PermissionSet.EmployeeleaveController, InfyPOS.Common.Action.Modify)]
        public async Task<IHttpActionResult> Update(ActionRequest<EmployeeLeave> request)
        {
            EmployeeLeaveDataService LeaverequestData = new EmployeeLeaveDataService();
            EmployeeLeave result = await Task<EmployeeLeave>.Run(() => LeaverequestData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        //[WebAPI.InfyAuthorize(PermissionSet.EmployeeleaveController, InfyPOS.Common.Action.Delete)]
        public async Task<IHttpActionResult> Delete(ActionRequest<EmployeeLeave> request)
        {
            EmployeeLeaveDataService LeaverequestData = new EmployeeLeaveDataService();
            bool result = await Task<bool>.Run(() => LeaverequestData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}