using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class EmployeesalaryDataService : SAASData
    {
        const string SQL_Search = @"select employeesalary.id, employeesalary.isactive, employeesalary.modifiedon, employeesalary.createdon, employeesalary.createdby, 
                                    employeesalary.modifiedby, employeesalary.version,employeeid,netsalary, leavedays, workingdays, pf, esi,earnings,deductions,
                                    detail, salarymonth , employee.name as name, employeeid,referencelist.name designation,
                                    employeesalary.salarymode,employeesalary.workingmode,employeesalary.departmentid,
                                    employeesalary.divisionid,employeesalary.companyid,additionalinfo,company.name companyname
                                    from employeesalary 
                                    inner join employee on employee.id = employeesalary.employeeid
                                    left outer join referencelist on referencelist.id = employee.desingnationid
                                    left outer join company on company.id = employee.companyid
                                    where id=@id";

        const string SQL_Insert = @"delete from public.employeesalary where employeeid=@employeeid and salarymonth=@salarymonth;
                                    INSERT INTO public.employeesalary(isactive, modifiedon, createdon, createdby, modifiedby, version, employeeid, netsalary, 
                                    leavedays, workingdays, pf, esi, detail, salarymonth,earnings,deductions,paydate,salarymode,workingmode,departmentid,divisionid,companyid,additionalinfo) 
                                    VALUES(@isactive, @modifiedon, @createdon, @createdby, @modifiedby, @version, @employeeid, @netsalary, @leavedays, @workingdays, @pf, @esi,
                                    @detail, @salarymonth,@earnings,@deductions,@paydate,@salarymode,@workingmode,@departmentid,@divisionid,@companyid,@additionalinfo) RETURNING id";

        const string SQL_Update = @"UPDATE public.employeesalary SET modifiedon=@modifiedon, modifiedby=@modifiedby, version=@version+1, employeeid=@employeeid, netsalary=@netsalary, leavedays=@leavedays, 
                                    workingdays=@workingdays, pf=@pf, esi=@esi, detail=@detail, additionalinfo=@additionalinfo, salarymonth=@salarymonth,earnings=@earnings,deductions=@deductions 
                                    where id=@id and version=@version RETURNING version";


        const string SQL_HoldSalary = @"select employeeid,netsalary, salarymonth,employee.name as name,referencelist.name designation,employeesalary.settlementdate,employeesalary.holddate
                                    from employeesalary 
                                    inner join employee on employee.id = employeesalary.employeeid
                                    left outer join referencelist on referencelist.id = employee.desingnationid";

        const string SQL_EmployeeGeneral = @"select employeecode,section.name sectionname,employee.name, gender, 
        dateofbirth, dateofjoining, relievingdate, hasleft, cast(personal->> 'marrieddate' as date) marriagedate,
        designation.name designation,department.name department,community.name community,floor.name floor,
        location.name locationname,cast(personal->> 'religion' as varchar) religion,caste.name caste,company.name company,
        age(case when hasleft then relievingdate else current_date end, dateofjoining)::varchar service,
        age(current_date, dateofbirth)::varchar employeeage from employee 
        left outer join company on company.id = employee.companyid
        left outer join referencelist section on section.id = employee.sectionid
		left outer join referencelist designation on designation.id = employee.desingnationid
		left outer join referencelist department on department.id = employee.departmentid
		left outer join referencelist floor on floor.id = employee.floorid
		left outer join referencelist location on location.id = employee.locationid
		--left outer join referencelist religion on religion.id = cast(personal->> 'religion' as bigint) 
		left outer join referencelist community on community.id = cast(personal->> 'community' as bigint) 
		left outer join referencelist caste on caste.id = cast(personal->> 'caste' as bigint) 
        where employee.isactive=true {0}";

        const string SQL_BirthdayByRange = @"select employeecode,section.name sectionname,employee.name, gender, 
        dateofbirth, dateofjoining, relievingdate, hasleft, cast(personal->> 'marrieddate' as date) marriagedate,
        designation.name designation,department.name department,community.name community,floor.name floor,
        location.name locationname,cast(personal->> 'religion' as varchar) religion,caste.name caste,company.name company,
        age(case when hasleft then relievingdate else current_date end, dateofjoining)::varchar service,
        age(current_date, dateofbirth)::varchar employeeage from employee 
        left outer join company on company.id = employee.companyid
        left outer join referencelist section on section.id = employee.sectionid
		left outer join referencelist designation on designation.id = employee.desingnationid
		left outer join referencelist department on department.id = employee.departmentid
		left outer join referencelist floor on floor.id = employee.floorid
		left outer join referencelist location on location.id = employee.locationid
		--left outer join referencelist religion on religion.id = cast(personal->> 'religion' as bigint) 
		left outer join referencelist community on community.id = cast(personal->> 'community' as bigint) 
		left outer join referencelist caste on caste.id = cast(personal->> 'caste' as bigint) 
        where employee.isactive=true
        and dateofbirth>@fromdate and dateofbirth<@todate
        {0}
        order by dateofbirth";

        const string SQL_Birthday = @"select employeecode,section.name sectionname,employee.name, gender, 
        dateofbirth, dateofjoining, relievingdate, hasleft, cast(personal->> 'marrieddate' as date) marriagedate,
        designation.name designation,department.name department,community.name community,floor.name floor,
        location.name locationname,cast(personal->> 'religion' as varchar) religion,caste.name caste,company.name company,
        age(case when hasleft then relievingdate else current_date end, dateofjoining)::varchar service,
        age(current_date, dateofbirth)::varchar employeeage from employee 
        left outer join company on company.id = employee.companyid
        left outer join referencelist section on section.id = employee.sectionid
		left outer join referencelist designation on designation.id = employee.desingnationid
		left outer join referencelist department on department.id = employee.departmentid
		left outer join referencelist floor on floor.id = employee.floorid
		left outer join referencelist location on location.id = employee.locationid
		--left outer join referencelist religion on religion.id = cast(personal->> 'religion' as bigint) 
		left outer join referencelist community on community.id = cast(personal->> 'community' as bigint) 
		left outer join referencelist caste on caste.id = cast(personal->> 'caste' as bigint) 
        where employee.isactive=true
        and dateofbirth>'1904-01-01' 
        and ('1904-'||to_char(dateofbirth,'MM-dd'))::date >= ('1904-'||to_char(@fromdate::date,'MM-dd'))::date
        and ('1904-'||to_char(dateofbirth,'MM-dd'))::date < ('1904-'||to_char(@todate::date,'MM-dd'))::date
        {0}
        order by dateofbirth";

        const string SQL_ServicecompletionByRanges = @"select employeecode,section.name sectionname,employee.name, gender, dateofbirth, dateofjoining, relievingdate,
        hasleft, cast(personal->> 'marrieddate' as date) marriagedate, age(case when hasleft then relievingdate else current_date end, dateofjoining)::varchar service,
        designation.name designation,department.name department,community.name community,floor.name floor,
        location.name locationname,cast(personal->> 'religion' as varchar) religion,caste.name caste,company.name company,
        age(case when hasleft then relievingdate else current_date end, dateofjoining)::varchar service,
        age(current_date, dateofbirth)::varchar employeeage 
        from employee 
        left outer join company on company.id = employee.companyid
        left outer join referencelist section on section.id = employee.sectionid
		left outer join referencelist designation on designation.id = employee.desingnationid
		left outer join referencelist department on department.id = employee.departmentid
		left outer join referencelist floor on floor.id = employee.floorid
		left outer join referencelist location on location.id = employee.locationid
		--left outer join referencelist religion on religion.id = cast(personal->> 'religion' as bigint) 
		left outer join referencelist community on community.id = cast(personal->> 'community' as bigint) 
		left outer join referencelist caste on caste.id = cast(personal->> 'caste' as bigint) 
        where employee.isactive=true and hasleft=true and relievingdate>=@fromdate and relievingdate<@todate
        {0}
        order by relievingdate";

        const string SQL_Servicecompletion = @"select employeecode,section.name sectionname,employee.name, gender, dateofbirth, dateofjoining, relievingdate,
        hasleft, cast(personal->> 'marrieddate' as date) marriagedate, age(case when hasleft then relievingdate else current_date end, dateofjoining)::varchar service,
        designation.name designation,department.name department,community.name community,floor.name floor,
        location.name locationname,cast(personal->> 'religion' as varchar) religion,caste.name caste,company.name company,
        age(case when hasleft then relievingdate else current_date end, dateofjoining)::varchar service,
        age(current_date, dateofbirth)::varchar employeeage 
        from employee 
        left outer join company on company.id = employee.companyid
        left outer join referencelist section on section.id = employee.sectionid
		left outer join referencelist designation on designation.id = employee.desingnationid
		left outer join referencelist department on department.id = employee.departmentid
		left outer join referencelist floor on floor.id = employee.floorid
		left outer join referencelist location on location.id = employee.locationid
		--left outer join referencelist religion on religion.id = cast(personal->> 'religion' as bigint) 
		left outer join referencelist community on community.id = cast(personal->> 'community' as bigint) 
		left outer join referencelist caste on caste.id = cast(personal->> 'caste' as bigint) 
        where employee.isactive=true and hasleft=true
        and relievingdate>'1904-01-01' 
        and ('1904-'||to_char(relievingdate,'MM-dd'))::date >= ('1904-'||to_char(@fromdate::date,'MM-dd'))::date
        and ('1904-'||to_char(relievingdate,'MM-dd'))::date < ('1904-'||to_char(@todate::date,'MM-dd'))::date
        {0}
        order by relievingdate";



        const string SQL_MarriagedayByRange = @"select employeecode, section.name sectionname,employee.name, gender, dateofbirth, dateofjoining, relievingdate,
        hasleft , cast(personal->> 'marrieddate' as date) marriagedate,
        designation.name designation,department.name department,community.name community,floor.name floor,
        location.name locationname,cast(personal->> 'religion' as varchar) religion,caste.name caste,company.name company,
        age(case when hasleft then relievingdate else current_date end, dateofjoining)::varchar service,
        age(current_date, dateofbirth)::varchar employeeage from employee 
        left outer join company on company.id = employee.companyid
        left outer join referencelist section on section.id = employee.sectionid
		left outer join referencelist designation on designation.id = employee.desingnationid
		left outer join referencelist department on department.id = employee.departmentid
		left outer join referencelist floor on floor.id = employee.floorid
		left outer join referencelist location on location.id = employee.locationid
		--left outer join referencelist religion on religion.id = cast(personal->> 'religion' as bigint) 
		left outer join referencelist community on community.id = cast(personal->> 'community' as bigint) 
		left outer join referencelist caste on caste.id = cast(personal->> 'caste' as bigint) 
        where employee.isactive=true
        and cast(personal->> 'marrieddate' as date)>@fromdate and cast(personal->> 'marrieddate' as date)<@todate
        {0}
        order by cast(personal->> 'marrieddate' as date)";

        const string SQL_Marriageday = @"select employeecode, section.name sectionname,employee.name, gender, dateofbirth, dateofjoining, relievingdate,
        hasleft , cast(personal->> 'marrieddate' as date) marriagedate,
        designation.name designation,department.name department,community.name community,floor.name floor,
        location.name locationname,cast(personal->> 'religion' as varchar) religion,caste.name caste,company.name company,
        age(case when hasleft then relievingdate else current_date end, dateofjoining)::varchar service,
        age(current_date, dateofbirth)::varchar employeeage from employee 
        left outer join company on company.id = employee.companyid
        left outer join referencelist section on section.id = employee.sectionid
		left outer join referencelist designation on designation.id = employee.desingnationid
		left outer join referencelist department on department.id = employee.departmentid
		left outer join referencelist floor on floor.id = employee.floorid
		left outer join referencelist location on location.id = employee.locationid
		--left outer join referencelist religion on religion.id = cast(personal->> 'religion' as bigint) 
		left outer join referencelist community on community.id = cast(personal->> 'community' as bigint) 
		left outer join referencelist caste on caste.id = cast(personal->> 'caste' as bigint) 
        where employee.isactive=true
        and cast(personal->> 'marrieddate' as date)>'1904-01-01' 
        and ('1904-'||to_char(cast(personal->> 'marrieddate' as date),'MM-dd'))::date >= ('1904-'||to_char(@fromdate::date,'MM-dd'))::date
        and ('1904-'||to_char(cast(personal->> 'marrieddate' as date),'MM-dd'))::date < ('1904-'||to_char(@todate::date,'MM-dd'))::date
        {0}
        order by cast(personal->> 'marrieddate' as date)";

        const string SQL_DateofJoiningByRange = @"select employeecode, section.name sectionname,employee.name, gender, 
        dateofbirth, dateofjoining, relievingdate, hasleft, cast(personal->> 'marrieddate' as date) marriagedate,
        designation.name designation,department.name department,community.name community,floor.name floor,
        location.name locationname,cast(personal->> 'religion' as varchar) religion,caste.name caste,company.name company,
        age(case when hasleft then relievingdate else current_date end, dateofjoining)::varchar service,
        age(current_date, dateofbirth)::varchar employeeage from employee 
        left outer join company on company.id = employee.companyid
        left outer join referencelist section on section.id = employee.sectionid
		left outer join referencelist designation on designation.id = employee.desingnationid
		left outer join referencelist department on department.id = employee.departmentid
		left outer join referencelist floor on floor.id = employee.floorid
		left outer join referencelist location on location.id = employee.locationid
		--left outer join referencelist religion on religion.id = cast(personal->> 'religion' as bigint) 
		left outer join referencelist community on community.id = cast(personal->> 'community' as bigint) 
		left outer join referencelist caste on caste.id = cast(personal->> 'caste' as bigint) 
        where employee.isactive=true
        and dateofjoining>@fromdate and dateofjoining<@todate
        {0}
        order by dateofjoining";

        const string SQL_DateofJoining = @"select employeecode, section.name sectionname,employee.name, gender, 
        dateofbirth, dateofjoining, relievingdate, hasleft, cast(personal->> 'marrieddate' as date) marriagedate,
        designation.name designation,department.name department,community.name community,floor.name floor,
        location.name locationname,cast(personal->> 'religion' as varchar) religion,caste.name caste,company.name company,
        age(case when hasleft then relievingdate else current_date end, dateofjoining)::varchar service,
        age(current_date, dateofbirth)::varchar employeeage from employee 
        left outer join company on company.id = employee.companyid
        left outer join referencelist section on section.id = employee.sectionid
		left outer join referencelist designation on designation.id = employee.desingnationid
		left outer join referencelist department on department.id = employee.departmentid
		left outer join referencelist floor on floor.id = employee.floorid
		left outer join referencelist location on location.id = employee.locationid
		--left outer join referencelist religion on religion.id = cast(personal->> 'religion' as bigint) 
		left outer join referencelist community on community.id = cast(personal->> 'community' as bigint) 
		left outer join referencelist caste on caste.id = cast(personal->> 'caste' as bigint) 
        where employee.isactive=true
        and dateofjoining>'1904-01-01' 
        and ('1904-'||to_char(dateofjoining,'MM-dd'))::date >= ('1904-'||to_char(@fromdate::date,'MM-dd'))::date
        and ('1904-'||to_char(dateofjoining,'MM-dd'))::date < ('1904-'||to_char(@todate::date,'MM-dd'))::date
        {0}
        order by dateofjoining";

        const string SQL_Searchleaving = @"select employeecode, section.name sectionname,employee.name, gender, 
        dateofbirth, dateofjoining, relievingdate, hasleft, cast(personal->> 'marrieddate' as date) marriagedate,
        designation.name designation,department.name department,community.name community,floor.name floor,
        location.name locationname,cast(personal->> 'religion' as varchar) religion,caste.name caste,company.name company,
        age(case when hasleft then relievingdate else current_date end, dateofjoining)::varchar service,
        age(current_date, dateofbirth)::varchar employeeage from employee 
        left outer join company on company.id = employee.companyid
        left outer join referencelist section on section.id = employee.sectionid
		left outer join referencelist designation on designation.id = employee.desingnationid
		left outer join referencelist department on department.id = employee.departmentid
		left outer join referencelist floor on floor.id = employee.floorid
		left outer join referencelist location on location.id = employee.locationid
		--left outer join referencelist religion on religion.id = cast(personal->> 'religion' as bigint) 
		left outer join referencelist community on community.id = cast(personal->> 'community' as bigint) 
		left outer join referencelist caste on caste.id = cast(personal->> 'caste' as bigint) 
        where employee.isactive=true
        and relievingdate>'1904-01-01' 
        and ('1904-'||to_char(relievingdate,'MM-dd'))::date >= ('1904-'||to_char(@fromdate::date,'MM-dd'))::date
        and ('1904-'||to_char(relievingdate,'MM-dd'))::date < ('1904-'||to_char(@todate::date,'MM-dd'))::date
        {0}
        order by relievingdate";


        const string SQL_SearchleavingByRange = @"select employeecode, section.name sectionname,employee.name, gender, 
        dateofbirth, dateofjoining, relievingdate, hasleft, cast(personal->> 'marrieddate' as date) marriagedate,
        designation.name designation,department.name department,community.name community,floor.name floor,
        location.name locationname,cast(personal->> 'religion' as varchar) religion,caste.name caste,company.name company,
        age(case when hasleft then relievingdate else current_date end, dateofjoining)::varchar service,
        age(current_date, dateofbirth)::varchar employeeage from employee 
        left outer join company on company.id = employee.companyid
        left outer join referencelist section on section.id = employee.sectionid
		left outer join referencelist designation on designation.id = employee.desingnationid
		left outer join referencelist department on department.id = employee.departmentid
		left outer join referencelist floor on floor.id = employee.floorid
		left outer join referencelist location on location.id = employee.locationid
		--left outer join referencelist religion on religion.id = cast(personal->> 'religion' as bigint) 
		left outer join referencelist community on community.id = cast(personal->> 'community' as bigint) 
		left outer join referencelist caste on caste.id = cast(personal->> 'caste' as bigint) 
        where employee.isactive=true
        and relievingdate>=@fromdate and relievingdate<@todate
        {0}
        order by relievingdate";

        const string SQL_MonthlyDOJ = @"select employeecode, referencelist.name sectionname,employee.name, gender,  
        dateofjoining, hasleft,age(case when hasleft then relievingdate else current_date end, dateofjoining)::varchar service from employee 
        left outer join referencelist on referencelist.id = employee.sectionid
        where employee.isactive=true and hasleft=false and dateofjoining>'1900-01-01'
        and dateofjoining >= @monthfromdate
        and dateofjoining < @monthtodate";

        const string SQL_MonthlyDOL = @"select employeecode, referencelist.name sectionname,employee.name, gender, 
        relievingdate, hasleft,age(case when hasleft then relievingdate else current_date end, dateofjoining)::varchar service from employee 
        left outer join referencelist on referencelist.id = employee.sectionid
        where employee.isactive=true and hasleft=true and relievingdate>'1900-01-01'
        and relievingdate >= @monthfromdate
        and relievingdate < @monthtodate {0}
        order by relievingdate";

   const string SQL_EmployeeAvailable = @"with totalemployee as
   (
    select employee.sectionid,employee.locationid,count(*) total from employee
    where employee.isactive = true and (employee.hasleft = false or employee.relievingdate > @fromdate)
    group by employee.sectionid,employee.locationid), availableemployee as

    (select employee.sectionid,employee.locationid, count(*) absent from employee
    left outer join employeeinout on employeeinout.employeeid = employee.id and attendanceon = @fromdate and employeeinout.isactive=true
    left outer join employeeleave on employeeleave.isactive =true and employeeleave.employeeid = employee.id and (fromdate<= @fromdate and todate >= @fromdate) and employeeleave.isactive=true
    where employee.isactive = true and(employee.hasleft = false or employee.relievingdate > @fromdate)
    and (employeeleave.days>0 and (employeeinout.id is null))
    group by employee.sectionid,employee.locationid
    )
    select referencelist.name,location.name ""location"",te.total,ae.absent from totalemployee te
    left outer join referencelist on referencelist.id = te.sectionid
    left outer join referencelist location on location.id = te.locationid
    left outer join availableemployee ae on ae.sectionid = te.sectionid and ae.locationid = te.locationid;

    select location.name ""location"",referencelist.name section,employee.name absent from employee
    left outer join referencelist on referencelist.id = employee.sectionid
    left outer join referencelist location on location.id = employee.locationid
    left outer join employeeinout on employeeinout.employeeid= employee.id and attendanceon = @fromdate and employeeinout.isactive=true
    where employee.isactive = true and (employee.hasleft = false or employee.relievingdate>@fromdate) 
    and (employeeinout.id is null or employeeinout.morningabsent = true or employeeinout.afternoonabsent = true);

    select sum(case when afternoonabsent = true then 1 else 0 end) afternoonabsent,
    sum(case when morningabsent = true then 1 else 0 end) morningabsent,
    sum(case when latecheckin >0 then 1 else 0 end) latecheckin,
    sum(case when earlycheckout >0 then 1 else 0 end) earlycheckout,
    sum(case when morningovertime >0 then 1 else 0 end) morningovertime,
    sum(case when eveningovertime >0 then 1 else 0 end) eveningovertime
    from employeeinout 
    inner join employee on employee.id = employeeinout.employeeid
    where attendanceon = @fromdate and employee.isactive = true and employeeinout.isactive=true";

        public List<Employeesalary> Generate(EmployeesalaryCreteria item)
        {
            List<Employeesalary> salaryList = new List<Employeesalary>();
            DateTime date = new DateTime(item.year, item.month, 1);
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_locakdate.Year > 2000 &&
               Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_locakdate > date)
            {
                throw new Exception("Locked date");
            }
            item.fromdate = item.fromdate.Date;
            item.todate = item.todate.Date;
            InfyPOS.Data.SalaryManager salary = new InfyPOS.Data.SalaryManager() { Context = this.Context };
            salary.Initialize(Context.OrganizationID, date.Month, date.Year, item.fromdate, item.todate);
            List<InfyPOS.Data.SalaryManager.EmployeeSalaryConfig> list = salary.BuildMasters(date.Month, date.Year, item.employeeid, item.salarymode);
            if(item.floorid>0)
            {
                list = list.FindAll(e => e.floorid == item.floorid);
            }
            Dictionary<long, List<InfyPOS.Data.SalaryManager.ScheduleAmount>> loanConig = salary.BuildExternal(date.Month, date.Year, item.employeeid);


            foreach (InfyPOS.Data.SalaryManager.EmployeeSalaryConfig config in list)
            {
                try
                {
                    if (config.Encashmentmode == InfyPOS.Data.SalaryManager.EmployeeSalaryConfig.LCModes.Autometic)
                    {
                        if (date.AddYears(-1) > config.DateOfJoin)
                        {
                            config.Encashmentmode = InfyPOS.Data.SalaryManager.EmployeeSalaryConfig.LCModes.Addtional;
                        }
                        else if (date.AddMonths(-3) > config.DateOfJoin)
                        {
                            config.Encashmentmode = InfyPOS.Data.SalaryManager.EmployeeSalaryConfig.LCModes.Adjustment;
                        }
                        else
                        {
                            config.Encashmentmode = InfyPOS.Data.SalaryManager.EmployeeSalaryConfig.LCModes.None;
                        }
                    }
                    InfyPOS.Data.SalaryManager.EmployeeSalaryConfig nconfig = salary.Calculate(config, !loanConig.ContainsKey(config.EmployeeID) ? null : loanConig[config.EmployeeID]);
                    Employeesalary es = new Employeesalary();
                    es.additionalinfo = new Employeesalary.AdditionalInfo();
                    es.additionalinfo.encashmentmode = config.Encashmentmode.ToString();
                    es.additionalinfo.totaldays = salary.TotalDays;
                    es.additionalinfo.holidays = salary.TotalHoliday;
                    es.additionalinfo.sundays = salary.TotalSunday;
                    es.additionalinfo.workingdays = salary.TotalWorkingDays - config.AdjustedJoiningResigning;
                    es.additionalinfo.leavetaken = nconfig.LeaveTaken;

                    es.additionalinfo.leavetotal = nconfig.LeaveDays;
                    es.additionalinfo.leaveencashmentdays = nconfig.LeaveEncashmentDays;
                    es.additionalinfo.acceptedworkdays = nconfig.AcceptedWorkDays > 0 ? nconfig.AcceptedWorkDays : (nconfig.WorkDayCount - nconfig.LeaveDays);

                    es.additionalinfo.sundayleaves = nconfig.SundayLeaves;
                    es.additionalinfo.holidayleaves = nconfig.HolidayLeaves;
                    es.additionalinfo.holidayabsent = nconfig.HolidayAbsent;
                    es.additionalinfo.holidaysundayabsent = nconfig.HolidaySundayAbsent;
                    es.additionalinfo.holidaysundayleaves = nconfig.HolidaySundayLeaves;
                    es.additionalinfo.gross = nconfig.grosspay;
                    es.additionalinfo.basicpay = nconfig.BasicPay;
                    es.additionalinfo.doj = nconfig.DateOfJoin.ToString("dd-MM-yyyy");
                    es.additionalinfo.dor = nconfig.Hasleft ? nconfig.Relievingdate.ToString("dd-MM-yyyy") : "";


                    es.employeeid = nconfig.EmployeeID;
                    if (nconfig.SalaryBreakup.Any(e => e.Value.HeaderType == InfyPOS.Data.SalaryManager.SalaryHeaderTypes.ESI))
                        es.esi = nconfig.SalaryBreakup.First(e => e.Value.HeaderType == InfyPOS.Data.SalaryManager.SalaryHeaderTypes.ESI).Value.Amount;
                    if (nconfig.SalaryBreakup.Any(e => e.Value.HeaderType == InfyPOS.Data.SalaryManager.SalaryHeaderTypes.ProvidentFund))
                        es.pf = nconfig.SalaryBreakup.First(e => e.Value.HeaderType == InfyPOS.Data.SalaryManager.SalaryHeaderTypes.ProvidentFund).Value.Amount;

                    es.detail = new List<Employeesalary.Detail>();
                    foreach (var sitem in nconfig.SalaryBreakup)
                    {
                        Employeesalary.Detail detail = new Employeesalary.Detail();
                        detail.id = sitem.Value.HeaderID;
                        detail.amount = sitem.Value.Amount;
                        detail.value = sitem.Value.Value;
                        detail.isdeduction = sitem.Value.IsDeduction;
                        es.detail.Add(detail);
                    }
                    es.salarymode = config.SalaryMode;
                    es.workingmode = config.workingmode;
                    es.departmentid = config.departmentid;
                    es.floorid = config.floorid;
                    es.divisionid = config.divisionid;
                    es.companyid = config.companyid;
                    es.holddate = config.Haswithheld ? config.Withheldfrom : DateTime.MinValue;
                    es.settlementdate = config.Haswithheld ? DateTime.MinValue : item.paydate.Date;
                    es.name = nconfig.Name;
                    es.designation = nconfig.Designation;
                    es.employeeno = nconfig.EmployeeCode;
                    es.leavedays = nconfig.LeaveDays;
                    es.netsalary = nconfig.SalaryBreakup.Sum(e => e.Value.IsDeduction ? e.Value.Amount * -1 : e.Value.Amount);
                    es.salarymonth = date;
                    if (nconfig.AcceptedWorkDays > 0)
                    {
                        es.workingdays = es.additionalinfo.acceptedworkdays;
                    }
                    else
                    {
                        es.workingdays = nconfig.WorkDayCount;
                    }
                    es.earnings = es.detail.Sum(e => e.isdeduction ? 0 : e.amount);
                    es.deductions = es.detail.Sum(e => e.isdeduction ? e.amount : 0);
                    es.paydate = item.paydate.Date;
                    salaryList.Add(es);
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, config);
                }
            }
            //if(item.save)
            {
                NpgsqlTransaction transaction = null;
                try
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                        foreach (var salaryitem in salaryList)
                        {
                            salaryitem.version = 1;
                            salaryitem.isactive = true;
                            salaryitem.createdby = Context.UserID;
                            salaryitem.modifiedby = Context.UserID;
                            salaryitem.createdon = DateTimeNow;
                            salaryitem.modifiedon = DateTimeNow;
                            Insert(transaction, conn, salaryitem);
                        }
                        transaction.Commit();
                        conn.Close();
                    }
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                }
            }

            if (list.Exists(e => e.Haswithheld))
            {
                salaryList.RemoveAll(e => list.Exists(ex => ex.Haswithheld
                && ex.EmployeeID == e.employeeid &&
                ex.Withheldfrom < e.salarymonth.AddMonths(1)));
            }
            return salaryList;
        }

        public SortedList<long, InfyPOS.Data.SalaryManager.SalaryHeader> DefaultSalaryHeaders()
        {
            ReferencelistDataService refData = new ReferencelistDataService() { Context = this.Context };
            refData.Context = this.Context;
            var configList = refData.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "PAYCATEGORY",
            });

            //System.IO.File.WriteAllText(@"C:\Logs\configList.json", Newtonsoft.Json.JsonConvert.SerializeObject(configList));
            //configList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InfyPOS.Models.Referencelist>>(System.IO.File.ReadAllText(@"D:\dbdata\configList.json"));

            var defaultHeaders = new SortedList<long, InfyPOS.Data.SalaryManager.SalaryHeader>();

            foreach (var dr in configList)
            {

                InfyPOS.Data.SalaryManager.SalaryHeader sh = new InfyPOS.Data.SalaryManager.SalaryHeader();
                sh.Description = dr.name;
                sh.HeaderID = dr.id;
                try
                {
                    sh.Code = dr.code;
                    sh.Formula = dr.additionalinfo != null ? dr.additionalinfo.formula : "";
                    InfyPOS.Data.SalaryManager.SalaryHeaderTypes type;
                    if (Enum.TryParse<InfyPOS.Data.SalaryManager.SalaryHeaderTypes>(dr.additionalinfo.paytype, out type))
                    {
                        sh.HeaderType = type;
                    }
                    defaultHeaders.Add(sh.HeaderID, sh);

                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, sh.Description + "\r\n" + sh.Formula + "\r\n" + exp.Message + "\r\n" + exp.StackTrace);
                }
            }
            return defaultHeaders;
        }
        public List<HRReport> SearchHoldsalary(HRReportCreteria item)
        {
            List<HRReport> lst = new List<HRReport>();
            DateTime today = item.fromdate.Date;
            DateTime nextdate = item.todate.AddDays(1).Date;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_HoldSalary;
                if (item.fromdate.Date != DateTime.MinValue)
                    qb.AddParameter("holdate", "fromdate", item.fromdate.Date, ">=");

                if (item.todate.Date != DateTime.MinValue)
                    qb.AddParameter("holdate", "todate", item.todate.AddDays(1).Date, "<");

                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        HRReport oitem = new HRReport();
                        oitem.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        oitem.employeename = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oitem.designation = reader["designation"] == DBNull.Value ? "" : reader["designation"].ToString();
                        oitem.withheldfrom = reader["holddate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["holddate"]);
                        oitem.settlementdate = reader["settlementdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["settlementdate"]);
                        oitem.hassettled = oitem.settlementdate != DateTime.MinValue;
                        lst.Add(oitem);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return lst;
        }

        const string SQL_Delete = @"update public.employeesalary set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";


        public List<Employeesalary> Search(EmployeesalaryCreteria _employeesalary)
        {
            string SQL_SearchPayslip = @"select employeesalary.id, employeesalary.isactive, employeesalary.modifiedon, employeesalary.createdon, employeesalary.createdby, 
                                    employeesalary.modifiedby, employeesalary.version,employeeid,netsalary, leavedays, workingdays, pf, esi, holddate,haswithheld,withheldfrom,company.name as companyname,
                                    detail, salarymonth , employee.name as name,employee.employeecode,employee.sectionid,employee.locationid,employee.dateofjoining,employee.relievingdate,
                                    employee.departmentid,employee.floorid, employee.pfandesi,employeeid,employeesalary.departmentid,referencelist.name designation,employeesalary.additionalinfo,employee.workingmode
                                    from employeesalary 
                                    inner join employee on employee.id = employeesalary.employeeid
                                    left outer join referencelist on referencelist.id = employee.desingnationid
                                    left outer join company on company.id = employee.companyid
                                    where employeesalary.isactive = true and 
                                    (employee.hasleft = false or relievingdate>=salarymonth)";


            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = SQL_SearchPayslip;
            if (_employeesalary.ids != null && _employeesalary.ids.Count > 0)
            {
                qb.AddInParamer("employeesalary.id", string.Join(",", _employeesalary.ids));
            }
            else
            {
                qb.AddParameter("date_part('month', salarymonth)", "month", _employeesalary.month, "=");
                qb.AddParameter("date_part('year', salarymonth)", "year", _employeesalary.year, "=");
            }
            if (_employeesalary.filtermode == (int)EmployeesalaryCreteria.FilterModes.Left)
                qb.AddParameter("employee.hasleft", true, "=");

            if (_employeesalary.filtermode == (int)EmployeesalaryCreteria.FilterModes.WithHeld)
                qb.AddParameter("employee.haswithheld", true, "=");

            if (_employeesalary.employeeid != 0)
                qb.AddParameter("employeesalary.employeeid", _employeesalary.employeeid, "=");

            if (_employeesalary.salarymode != 0)
                qb.AddParameter("employeesalary.salarymode", _employeesalary.salarymode, "=");

            if (_employeesalary.workingmode != 0)
                qb.AddParameter("employeesalary.workingmode", _employeesalary.workingmode, "=");

            if (_employeesalary.companyid != 0)
                qb.AddParameter("employeesalary.companyid", _employeesalary.companyid, "=");


            if (_employeesalary.departmentid != 0)
                qb.AddParameter("employeesalary.departmentid", _employeesalary.departmentid, "=");

            if (_employeesalary.locationid != 0)
                qb.AddParameter("employee.locationid", _employeesalary.locationid, "=");

            if (_employeesalary.divisionid != 0)
                qb.AddParameter("employeesalary.sectionid", _employeesalary.divisionid, "=");

            if (_employeesalary.floorid != 0)
                qb.AddParameter("employee.floorid", _employeesalary.floorid, "=");

            if (Context.accessiblelocation != null && !Context.accessiblelocation.Exists(x => x == -1))
            {
                qb.AddInParamer("employee.locationid", string.Join(",", Context.accessiblelocation));
            }

            qb.SortField = @" case when employeecode ~'^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";

            List<Employeesalary> lst = new List<Employeesalary>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = qb.GetCommand(conn);
                //command.Parameters.Add("institutionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.GetInstitutionID(_employeesalary.institutionid);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employeesalary oemployeesalary = new Employeesalary();
                        Employee emp = new Employee();
                        oemployeesalary.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oemployeesalary.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oemployeesalary.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oemployeesalary.relievingdate = reader["relievingdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["relievingdate"]);
                        oemployeesalary.dateofjoining = reader["dateofjoining"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofjoining"]);
                        oemployeesalary.sectionid = reader["sectionid"] == DBNull.Value ? "" : reader["sectionid"].ToString();
                        emp.pfandesi_json = reader["pfandesi"] == DBNull.Value ? "null" : reader["pfandesi"].ToString();
                        oemployeesalary.pfno = (emp.pfandesi == null) ? "" : emp.pfandesi.pfno;
                        oemployeesalary.esino = (emp.pfandesi == null) ? "" : emp.pfandesi.esino;
                        oemployeesalary.pfuan = (emp.pfandesi == null) ? "" : emp.pfandesi.pfuan;
                        oemployeesalary.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oemployeesalary.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oemployeesalary.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oemployeesalary.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oemployeesalary.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        oemployeesalary.netsalary = reader["netsalary"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["netsalary"]);
                        oemployeesalary.leavedays = reader["leavedays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["leavedays"]);
                        oemployeesalary.workingdays = reader["workingdays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["workingdays"]);
                        oemployeesalary.pf = reader["pf"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pf"]);
                        oemployeesalary.esi = reader["esi"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["esi"]);
                        oemployeesalary.detail_json = reader["detail"] == DBNull.Value ? "null" : reader["detail"].ToString();
                        oemployeesalary.additionalinfo_json = reader["additionalinfo"] == DBNull.Value ? "null" : reader["additionalinfo"].ToString();
                        oemployeesalary.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oemployeesalary.employeeno = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        oemployeesalary.designation = reader["designation"] == DBNull.Value ? "" : reader["designation"].ToString();
                        oemployeesalary.companyname = reader["companyname"] == DBNull.Value ? "" : reader["companyname"].ToString();
                        oemployeesalary.earnings = oemployeesalary.detail.Sum(e => e.isdeduction ? 0 : e.amount);
                        oemployeesalary.deductions = oemployeesalary.detail.Sum(e => e.isdeduction ? e.amount : 0);
                        //oemployeesalary.pfno = reader["pfno"] == DBNull.Value ? "" : reader["pfno"].ToString();
                        oemployeesalary.salarymonth = reader["salarymonth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["salarymonth"]);
                        oemployeesalary.earnings = oemployeesalary.detail.Sum(e => e.isdeduction ? 0 : e.amount);
                        oemployeesalary.deductions = oemployeesalary.detail.Sum(e => e.isdeduction ? e.amount : 0);
                        oemployeesalary.inwords = WebAPI.Data.Numberconvertor.rupees(oemployeesalary.netsalary);
                        oemployeesalary.holddate = reader["salarymonth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["salarymonth"]);
                        oemployeesalary.haswithheld = reader["haswithheld"] == DBNull.Value ? false : Convert.ToBoolean(reader["haswithheld"]);
                        oemployeesalary.withheldfrom = reader["withheldfrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["withheldfrom"]);
                        oemployeesalary.workingmode = reader["workingmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["workingmode"]);
                        if (_employeesalary.filtermode == (int)EmployeesalaryCreteria.FilterModes.Payable)
                        {
                            if (oemployeesalary.haswithheld && oemployeesalary.withheldfrom < oemployeesalary.salarymonth.AddMonths(1))
                                continue;
                        }
                        lst.Add(oemployeesalary);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _employeesalary);
            }
            return lst;
        }

        internal bool UpdateDetail(List<EmployeeSalaryUpdation> items)
        {
            var searchresult = Search(new EmployeesalaryCreteria()
            {
                ids = items.ConvertAll(e => e.id)
            });

            NpgsqlTransaction transaction = null;
            try
            {
                var defaultsalaryheaders = DefaultSalaryHeaders();
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    foreach (var item in searchresult)
                    {
                        if (items.Exists(e => e.id == item.id))
                        {
                            bool changed = false;
                            foreach (var change in items.FindAll(e => e.id == item.id))
                            {
                                bool isdeduction = false;
                                if (defaultsalaryheaders.ContainsKey(change.headerid))
                                {
                                    if (defaultsalaryheaders[change.headerid].HeaderType == InfyPOS.Data.SalaryManager.SalaryHeaderTypes.ProvidentFund)
                                        item.pf = change.amount;
                                    if (defaultsalaryheaders[change.headerid].HeaderType == InfyPOS.Data.SalaryManager.SalaryHeaderTypes.ESI)
                                        item.esi = change.amount;

                                    isdeduction = defaultsalaryheaders[change.headerid].IsDeduction;
                                }
                                if (item.detail.Exists(e => e.id == change.headerid))
                                {
                                    var itemchange = item.detail.Find(e => e.id == change.headerid);
                                    if (itemchange.amount == change.amount) continue;

                                    itemchange.amount = change.amount;
                                    itemchange.isdeduction = isdeduction;
                                    changed = true;
                                }
                                else
                                {
                                    changed = true;
                                    item.detail.Add(new Employeesalary.Detail()
                                    {
                                        id = change.headerid,
                                        amount = change.amount,
                                        isdeduction = isdeduction
                                    });
                                }
                            }
                            if (changed)
                            {
                                item.earnings = item.detail.Sum(e => e.isdeduction ? 0 : e.amount);
                                item.deductions = item.detail.Sum(e => e.isdeduction ? e.amount : 0);
                                item.netsalary = item.earnings - item.deductions;
                                Update(transaction, conn, item);
                            }
                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
                return true;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, items);
                return false;
            }
        }

        public List<Payroll.SalarySummary.Item> SearchSummary(Payroll.SalarySummaryRequest request)
        {
            DateTime from = request.from.AddDays(request.from.Day * -1);
            DateTime to = request.to.AddDays(request.to.Day * -1).AddMonths(1);

            List<Payroll.SalarySummary.Item> lst = new List<Payroll.SalarySummary.Item>();

            string companysearch = @"select salarymonth,sum(netsalary) as netsalary,sum(pf) as pf,sum(esi) as esi,sum(earnings) as earnings,sum(deductions) as deductions from employeesalary
                                    where salarymonth>=@salarymonthfrom and salarymonth<@salarymonthto
                                    group by salarymonth";

            string fullsearch = @"with salarysummary as
                                    (
                                    select employeeid,sum(netsalary) as netsalary,sum(pf) as pf,sum(esi) as esi,sum(earnings) as earnings,sum(deductions) as deductions from employeesalary
                                    where salarymonth>=@salarymonthfrom and salarymonth<@salarymonthto
                                    group by employeeid
                                    )
                                    select salarysummary.*,employee.employeecode,employee.name from salarysummary
                                    inner join employee on employee.id=salarysummary.employeeid
                                    order by case when employeecode ~'^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";

            string employeesearch = @"select salarymonth,sum(netsalary) as netsalary,sum(pf) as pf,sum(esi) as esi,sum(earnings) as earnings,sum(deductions) as deductions from employeesalary
                                    where salarymonth>=@salarymonthfrom and salarymonth<@salarymonthto and employeeid=@employeeid
                                    group by salarymonth";
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = null;
                if (request.mode == Payroll.SalarySummaryRequest.Mode.CompanySummary)
                    command = GetCommand(companysearch, conn);
                else
                    command = GetCommand(request.employeeid > 0 ? employeesearch : fullsearch, conn);
                command.Parameters.Add("salarymonthfrom", NpgsqlTypes.NpgsqlDbType.Date).Value = from;
                command.Parameters.Add("salarymonthto", NpgsqlTypes.NpgsqlDbType.Date).Value = to;
                command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = request.employeeid;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Payroll.SalarySummary.Item oemployeesalary = new Payroll.SalarySummary.Item();
                        oemployeesalary.pf = reader["pf"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pf"]);
                        oemployeesalary.esi = reader["esi"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["esi"]);

                        oemployeesalary.earnings = reader["earnings"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["esi"]);
                        oemployeesalary.deductions = reader["deductions"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["esi"]);

                        if (request.mode == Payroll.SalarySummaryRequest.Mode.CompanySummary || request.employeeid > 0)
                            oemployeesalary.salarymonth = reader["salarymonth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["salarymonth"]);

                        if (request.mode == Payroll.SalarySummaryRequest.Mode.EmployeeSummary && request.employeeid == 0)
                        {
                            oemployeesalary.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                            oemployeesalary.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        }

                        lst.Add(oemployeesalary);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
            }
            return lst;

        }
        public List<InfyPOS.Payroll.ESI> SearchESIPF(InfyPOS.Payroll.ESIRequest _employeesalary)
        {
            string SQL_SearchPayslip = @"select employee.employeecode,name,workingdays,leavedays,pf,esi,pfandesi,detail,dateofbirth,hasleft,resignreasonid,relievingdate from employeesalary
                                        inner join employee on employee.id = employeesalary.employeeid
                                        where employee.isactive = true and employeesalary.isactive = true ";


            QueryBuilder qb = new QueryBuilder();
            qb.CurrentQuery = SQL_SearchPayslip;
            qb.AddParameter("date_part('month', salarymonth)", "month", _employeesalary.month, "=");
            qb.AddParameter("date_part('year', salarymonth)", "year", _employeesalary.year, "=");

            if (Context.accessiblelocation != null && !Context.accessiblelocation.Exists(x => x == -1))
            {
                qb.AddInParamer("employee.locationid", string.Join(",", Context.accessiblelocation));
            }

            if (_employeesalary.companyid != 0)
                qb.AddParameter("employeesalary.companyid", _employeesalary.companyid, "=");

            qb.SortField = @" case when employeecode ~'^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";

            List<InfyPOS.Payroll.ESI> lst = new List<InfyPOS.Payroll.ESI>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = qb.GetCommand(conn);
                //command.Parameters.Add("institutionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.GetInstitutionID(_employeesalary.institutionid);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employee emp = new Employee();
                        InfyPOS.Payroll.ESI oemployeesalary = new InfyPOS.Payroll.ESI();
                        oemployeesalary.leavedays = reader["leavedays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["leavedays"]);
                        oemployeesalary.workingdays = reader["workingdays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["workingdays"]);
                        oemployeesalary.pf = reader["pf"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["pf"]);
                        oemployeesalary.esi = reader["esi"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["esi"]);
                        oemployeesalary.detail_json = reader["detail"] == DBNull.Value ? "null" : reader["detail"].ToString();
                        emp.pfandesi_json = reader["pfandesi"] == DBNull.Value ? "null" : reader["pfandesi"].ToString();
                        oemployeesalary.pfuan = (emp.pfandesi_json == null) ? "" : emp.pfandesi.pfuan;
                        oemployeesalary.esino = (emp.pfandesi_json == null) ? "" : emp.pfandesi.esino;
                        oemployeesalary.pfno = (emp.pfandesi_json == null) ? "" : emp.pfandesi.pfno;
                        oemployeesalary.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oemployeesalary.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        oemployeesalary.dateofbirth = reader["dateofbirth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofbirth"]);
                        oemployeesalary.hasleft = reader["hasleft"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasleft"]);
                        oemployeesalary.relievingdate = reader["relievingdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["relievingdate"]);
                        oemployeesalary.resignreasonid = reader["resignreasonid"] == DBNull.Value ? -1 : Convert.ToInt32(reader["resignreasonid"]);
                        lst.Add(oemployeesalary);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _employeesalary);
            }
            return lst;
        }

        public Employeesalary Insert(Employeesalary oEmployeesalary)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);

                    Insert(transaction, conn, oEmployeesalary);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeesalary);

            }

            return oEmployeesalary;
        }

        public Employeesalary Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeesalary oEmployeesalary)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalary.isactive;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalary.version;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalary.employeeid;
            command.Parameters.Add("netsalary", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalary.netsalary;
            command.Parameters.Add("leavedays", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalary.leavedays;
            command.Parameters.Add("workingdays", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalary.workingdays;
            command.Parameters.Add("pf", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalary.pf;
            command.Parameters.Add("esi", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalary.esi;
            command.Parameters.Add("detail", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployeesalary.detail_json == null ? "null" : oEmployeesalary.detail_json;
            command.Parameters.Add("additionalinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployeesalary.additionalinfo_json == null ? "null" : oEmployeesalary.additionalinfo_json;
            command.Parameters.Add("salarymonth", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalary.salarymonth;
            command.Parameters.Add("earnings", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalary.earnings;
            command.Parameters.Add("deductions", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalary.deductions;
            command.Parameters.Add("paydate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalary.paydate;
            command.Parameters.Add("salarymode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalary.salarymode;
            command.Parameters.Add("workingmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalary.workingmode;
            command.Parameters.Add("departmentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalary.departmentid;
            command.Parameters.Add("divisionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalary.divisionid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalary.companyid;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmployeesalary.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oEmployeesalary;
        }

        public Employeesalary Update(Employeesalary oEmployeesalary)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oEmployeesalary);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeesalary);

            }

            return oEmployeesalary;
        }

        public Employeesalary Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeesalary oEmployeesalary)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalary.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalary.isactive;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalary.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalary.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalary.version;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalary.employeeid;
            command.Parameters.Add("netsalary", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalary.netsalary;
            command.Parameters.Add("leavedays", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalary.leavedays;
            command.Parameters.Add("workingdays", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalary.workingdays;
            command.Parameters.Add("pf", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalary.pf;
            command.Parameters.Add("esi", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalary.esi;

            command.Parameters.Add("earnings", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalary.earnings;
            command.Parameters.Add("deductions", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeesalary.deductions;

            command.Parameters.Add("detail", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployeesalary.detail_json == null ? "null" : oEmployeesalary.detail_json;
            command.Parameters.Add("additionalinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployeesalary.additionalinfo_json == null ? "null" : oEmployeesalary.additionalinfo_json;
            command.Parameters.Add("salarymonth", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeesalary.salarymonth;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmployeesalary.version = Convert.ToInt32(reader[0].ToString());
                }
            }
            return oEmployeesalary;
        }


        public bool Delete(Employeesalary oEmployeesalary)
        {
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_locakdate.Year > 2000 &&
               Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_locakdate > oEmployeesalary.paydate)
            {
                throw new Exception("Locked date");
            }
            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oEmployeesalary);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeesalary);


            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeesalary oEmployeesalary)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeesalary.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeesalary.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeesalary.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        public List<HRReport> SearchHRReports(HRReportCreteria _reportcreteria)
        {
            List<HRReport> list = new List<HRReport>();
            NpgsqlCommand command = null;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                string condition = "";
                switch (_reportcreteria.employeemode)
                {
                    case "LEFT":
                        condition = "and hasleft=true";
                        break;
                    case "ACTIVE":
                        condition = "and hasleft=false";
                        break;
                }
                if(_reportcreteria.companyid>0)
                {
                    condition += " and companyid=" + _reportcreteria.companyid;
                }
                if (_reportcreteria.departmentid > 0)
                {
                    condition += " and departmentid=" + _reportcreteria.departmentid;
                }
                if (_reportcreteria.sectionid > 0)
                {
                    condition += " and sectionid=" + _reportcreteria.sectionid;
                }
                if (_reportcreteria.floorid > 0)
                {
                    condition += " and floorid=" + _reportcreteria.floorid;
                }
                if (_reportcreteria.locationid > 0)
                {
                    condition += " and locationid=" + _reportcreteria.locationid;
                }

                if (Context.accessiblelocation != null && !Context.accessiblelocation.Exists(x => x == -1))
                {
                    condition += " and employee.locationid in ("+string.Join(",", Context.accessiblelocation)+")";
                }

                switch (_reportcreteria.reporttype)
                {
                    case "BIRTHDAY":
                        command = GetCommand(string.Format(_reportcreteria.allyears ? SQL_Birthday : SQL_BirthdayByRange, condition), conn);
                        break;
                    case "GENERAL":
                        command = GetCommand(string.Format(SQL_EmployeeGeneral, condition), conn);
                        break;
                    case "MARIAGE":
                        command = GetCommand(string.Format(_reportcreteria.allyears ? SQL_Marriageday : SQL_MarriagedayByRange, condition), conn);
                        break;
                    case "JOINDDAY":
                        command = GetCommand(string.Format(_reportcreteria.allyears ? SQL_DateofJoining : SQL_DateofJoiningByRange, condition), conn);
                        break;
                    case "RELIEVING":
                        command = GetCommand(string.Format(_reportcreteria.allyears ? SQL_Searchleaving :SQL_SearchleavingByRange, condition), conn);
                        break;
                    case "SERVICECOMPLETION":
                        command = GetCommand(string.Format(_reportcreteria.allyears ? SQL_Servicecompletion: SQL_ServicecompletionByRanges, condition), conn);
                        break;
                }
                DateTime today = _reportcreteria.fromdate.Date;
                DateTime nextdate = _reportcreteria.todate.AddDays(1).Date;
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        HRReport oitem = new HRReport();
                        oitem.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        oitem.employeename = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oitem.sectionname = reader["sectionname"] == DBNull.Value ? "" : reader["sectionname"].ToString();
                        oitem.dateofbirth = reader["dateofbirth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofbirth"]);
                        oitem.dateofmarriage = reader["marriagedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["marriagedate"]);
                        oitem.dateofrelieving = reader["relievingdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["relievingdate"]);
                        oitem.gender = reader["gender"] == DBNull.Value ? "" : reader["gender"].ToString();
                        oitem.designation = reader["designation"] == DBNull.Value ? "" : reader["designation"].ToString();
                        oitem.department = reader["department"] == DBNull.Value ? "" : reader["department"].ToString();
                        oitem.community = reader["community"] == DBNull.Value ? "" : reader["community"].ToString();
                        oitem.floor = reader["floor"] == DBNull.Value ? "" : reader["floor"].ToString();
                        oitem.locationname = reader["locationname"] == DBNull.Value ? "" : reader["locationname"].ToString();
                        oitem.religion = reader["locationname"] == DBNull.Value ? "" : reader["religion"].ToString();
                        oitem.caste = reader["locationname"] == DBNull.Value ? "" : reader["caste"].ToString();
                        oitem.company = reader["company"] == DBNull.Value ? "" : reader["company"].ToString();
                        oitem.hasleft = reader["hasleft"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasleft"]);
                        oitem.service = reader["service"] == DBNull.Value ? "" : reader["service"].ToString();
                        oitem.employeeage = reader["employeeage"] == DBNull.Value ? "" : reader["employeeage"].ToString();

                        list.Add(oitem);
                    }
                }
                conn.Close();

            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }
            return list;
        }

        //public List<HRReport> SearchBirthday(HRReportCreteria _reportcreteria)
        //{
        //    List<HRReport> lst = new List<HRReport>();
        //    DateTime today = _reportcreteria.fromdate.Date;
        //    DateTime nextdate = _reportcreteria.todate.AddDays(1).Date;
        //    NpgsqlConnection conn = CreateConnection();
        //    try
        //    {
        //        conn.Open();

        //        NpgsqlCommand command = null;

        //        if (_reportcreteria.ismarriageday)
        //            command = GetCommand(SQL_Marriageday, conn);
        //        else
        //            command = GetCommand(SQL_Birthday, conn);
        //        command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
        //        command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
        //        using (System.Data.IDataReader reader = command.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                HRReport oitem = new HRReport();
        //                oitem.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
        //                oitem.employeename = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
        //                oitem.dateofbirth = reader["dateofbirth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofbirth"]);
        //                oitem.dateofmarriage = reader["marriagedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["marriagedate"]);
        //                if (_reportcreteria.ismarriageday)
        //                    oitem.mode = 2;
        //                else
        //                    oitem.mode = 1;
        //                lst.Add(oitem);
        //            }
        //        }
        //        conn.Close();
        //    }
        //    catch (Exception exp)
        //    {
        //        if (conn != null) conn.Close();
        //        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
        //    }
        //    return lst;
        //}

        //public List<HRReport> SearchJoiningday(HRReportCreteria _reportcreteria)
        //{
        //    List<HRReport> lst = new List<HRReport>();
        //    DateTime today = _reportcreteria.fromdate.Date;
        //    DateTime nextdate = _reportcreteria.todate.AddDays(1).Date;
        //    NpgsqlConnection conn = CreateConnection();
        //    try
        //    {
        //        conn.Open();

        //        NpgsqlCommand command = null;

        //        if (_reportcreteria.isrelievingday)
        //            command = GetCommand(SQL_Searchleaving, conn);
        //        else
        //            command = GetCommand(SQL_SearchJoining, conn);
        //        command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
        //        command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
        //        using (System.Data.IDataReader reader = command.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                HRReport oitem = new HRReport();
        //                oitem.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
        //                oitem.employeename = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
        //                oitem.dateofjoining = reader["dateofjoining"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofjoining"]);
        //                oitem.dateofrelieving = reader["relievingdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["relievingdate"]);

        //                lst.Add(oitem);
        //            }
        //        }
        //        conn.Close();
        //    }
        //    catch (Exception exp)
        //    {
        //        if (conn != null) conn.Close();
        //        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
        //    }
        //    return lst;
        //}

        //public List<HRReport> SearchServiceCompletion(HRReportCreteria _reportcreteria)
        //{
        //    List<HRReport> lst = new List<HRReport>();
        //    DateTime today = _reportcreteria.fromdate.Date;
        //    DateTime nextdate = _reportcreteria.todate.AddDays(1).Date;
        //    NpgsqlConnection conn = CreateConnection();
        //    try
        //    {
        //        conn.Open();

        //        NpgsqlCommand command = null;

        //        command = GetCommand(SQL_Servicecompletion, conn);
        //        command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
        //        using (System.Data.IDataReader reader = command.ExecuteReader())
        //        {
        //            while (reader.Read())
        //            {
        //                HRReport oitem = new HRReport();
        //                oitem.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
        //                oitem.employeename = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
        //                oitem.dateofjoining = reader["dateofjoining"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofjoining"]);
        //                oitem.service = reader["service"] == DBNull.Value ? "" : reader["service"].ToString();
        //                lst.Add(oitem);
        //            }
        //        }
        //        conn.Close();
        //    }
        //    catch (Exception exp)
        //    {
        //        if (conn != null) conn.Close();
        //        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
        //    }
        //    return lst;
        //}

        public List<EmployeeBankreport> SearchBankReport(EmployeeBankCriteria creteria)
        {
            string SQL_Searchbankrept = @"select employee.id, employee.employeecode, employee.name, employee.bank,
            employeesalary.netsalary, referencelist.name as bankname  from  employeesalary
            inner join employee  on employee.id = employeesalary.employeeid 
            left outer join referencelist on employee.bankid = referencelist.id 
            where employeesalary.isactive = true and employeesalary.netsalary>0 ";

            QueryBuilder qbs = new QueryBuilder();
            qbs.CurrentQuery = SQL_Searchbankrept;
            if (creteria.ids != null && creteria.ids.Count > 0)
            {
                qbs.AddInParamer("employee.id", string.Join(",", creteria.ids));
            }
            else
            {
                qbs.AddParameter("date_part('month', salarymonth)", "month", creteria.month, "=");
                qbs.AddParameter("date_part('year', salarymonth)", "year", creteria.year, "=");
            }

            if (creteria.employeeid != 0)
                qbs.AddParameter("employee.id", creteria.employeeid, "=");

            if (creteria.companyid != 0)
                qbs.AddParameter("employee.companyid", creteria.companyid, "=");

            if (Context.accessiblelocation != null && !Context.accessiblelocation.Exists(x => x == -1))
            {
                qbs.AddInParamer("employee.locationid", string.Join(",", Context.accessiblelocation));
            }

            List<EmployeeBankreport> lst = new List<EmployeeBankreport>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                NpgsqlCommand command = qbs.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        EmployeeBankreport oemployeesalary = new EmployeeBankreport();
                        oemployeesalary.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oemployeesalary.netsalary = reader["netsalary"] == DBNull.Value ? 0 : Convert.ToInt64(reader["netsalary"]);
                        oemployeesalary.bank_json = reader["bank"] == DBNull.Value ? "null" : reader["bank"].ToString();
                        oemployeesalary.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oemployeesalary.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        oemployeesalary.bankname = reader["bankname"] == DBNull.Value ? "" : reader["bankname"].ToString();
                        oemployeesalary.accountname = (oemployeesalary.bank_json == null) ? "" : oemployeesalary.bank.accountname;
                        oemployeesalary.accountno = (oemployeesalary.bank_json == null) ? "" : oemployeesalary.bank.accountno;
                        oemployeesalary.ifsc = (oemployeesalary.bank_json == null) ? "" : oemployeesalary.bank.ifsc;
                        lst.Add(oemployeesalary);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, creteria);
            }
            return lst;
        }

        public List<Employeesalary> GenerateCustom(EmployeeSalaryConfig item)
        {
            List<Employeesalary> salaryList = new List<Employeesalary>();
            Data.EmployeeLeaveDataService employeeleave = new EmployeeLeaveDataService() { Context = this.Context };


            //employeeleavelst = EmployeeLeaveDataService.Search(new LeaverequestCreteria()
            //      {

            //      });
            DateTime fromdate = new DateTime();
            DateTime todate = new DateTime();
            if (item.bydate == false)
            {
                fromdate = new DateTime(item.year, item.month, 1);
                todate = fromdate.AddMonths(1).AddDays(-1);
            }
            else
            {
                fromdate = item.generatefrom;
                todate = item.generateto;
            }
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_locakdate.Year > 2000 &&
               Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_locakdate > fromdate)
            {
                throw new Exception("Locked date");
            }
            InfyPOS.Data.SalaryEngine salary = new InfyPOS.Data.SalaryEngine() { Context = this.Context };
            salary.Initialize(Context.OrganizationID, fromdate.Month, fromdate.Year, fromdate, todate);
            salary.SetConfiguration(item);

            List<InfyPOS.Data.SalaryEngine.EmployeeSalaryConfig> list = salary.BuildMasters(fromdate, 
                todate.AddDays(1),
                item.employeeid, 
                item.salarymode,
                item.proratedaysbased);

            Dictionary<long, List<InfyPOS.Data.SalaryManager.ScheduleAmount>> loanConig = salary.BuildExternal(fromdate, todate, item.employeeid);
            

            EmployeesalarystructureDataService employeesalarystructureDataService = new EmployeesalarystructureDataService() { Context = this.Context };
            var emplooyeesalarystructures = employeesalarystructureDataService.Search(new EmployeesalarystructureCreteria());

            foreach (InfyPOS.Data.SalaryEngine.EmployeeSalaryConfig config in list)
            {
                if (config.Encashmentmode == InfyPOS.Data.SalaryEngine.EmployeeSalaryConfig.LCModes.Autometic)
                {
                    if (fromdate.AddYears(-1) > config.DateOfJoin)
                    {
                        config.Encashmentmode = InfyPOS.Data.SalaryEngine.EmployeeSalaryConfig.LCModes.Addtional;
                    }
                    else if (fromdate.AddMonths(-3) > config.DateOfJoin)
                    {
                        config.Encashmentmode = InfyPOS.Data.SalaryEngine.EmployeeSalaryConfig.LCModes.Adjustment;
                    }
                    else
                    {
                        config.Encashmentmode = InfyPOS.Data.SalaryEngine.EmployeeSalaryConfig.LCModes.None;
                    }
                }
                config.proratedaysbased = item.proratedaysbased;
                config.updatedafromstructure = item.updatedafromstructure;
                config.updatebpfromstructure = item.updatebpfromstructure;
                var nconfig = salary.Calculate(config, loanConig.ContainsKey(config.EmployeeID) ? loanConig[config.EmployeeID] : null);
                Employeesalary es = new Employeesalary();
                es.additionalinfo = new Employeesalary.AdditionalInfo();
                es.additionalinfo.encashmentmode = config.Encashmentmode.ToString();
                es.additionalinfo.totaldays = salary.computedConfig.totaldays;
                es.additionalinfo.holidays = salary.computedConfig.totalholiday;
                es.additionalinfo.sundays = salary.computedConfig.totalsunday;
                es.additionalinfo.workingdays = config.WorkDayCount;

                es.additionalinfo.absentdays = config.AbsentDays - config.Additionalabsentdays;
                es.additionalinfo.additionalabsentdays = config.Additionalabsentdays;
                
                es.additionalinfo.leavetaken = nconfig.LeaveTaken;

                es.additionalinfo.leavetotal = nconfig.LeaveDays;
                es.additionalinfo.leaveencashmentdays = nconfig.LeaveEncashmentDays;
                es.additionalinfo.acceptedworkdays = nconfig.AcceptedWorkDays > 0 ? nconfig.AcceptedWorkDays : (nconfig.WorkDayCount - nconfig.LeaveDays);
                es.additionalinfo.paydays = nconfig.Paydays;
                es.additionalinfo.sundayleaves = nconfig.SundayLeaves;
                es.additionalinfo.holidayleaves = nconfig.HolidayLeaves;
                es.additionalinfo.holidayabsent = nconfig.HolidayAbsent;
                es.additionalinfo.holidaysundayabsent = nconfig.HolidaySundayAbsent;
                es.additionalinfo.holidaysundayleaves = nconfig.HolidaySundayLeaves;
                es.additionalinfo.gross = nconfig.grosspay;
                es.additionalinfo.basicpay = nconfig.BasicPay;
                es.additionalinfo.doj = nconfig.DateOfJoin.ToString("dd-MM-yyyy");
                es.additionalinfo.dor = nconfig.Hasleft ? nconfig.Relievingdate.ToString("dd-MM-yyyy") : "";

                es.employeeid = nconfig.EmployeeID;
                if (nconfig.SalaryBreakup.Any(e => e.Value.HeaderType == InfyPOS.Data.SalaryManager.SalaryHeaderTypes.ESI))
                    es.esi = nconfig.SalaryBreakup.First(e => e.Value.HeaderType == InfyPOS.Data.SalaryManager.SalaryHeaderTypes.ESI).Value.Amount;
                if (nconfig.SalaryBreakup.Any(e => e.Value.HeaderType == InfyPOS.Data.SalaryManager.SalaryHeaderTypes.ProvidentFund))
                    es.pf = nconfig.SalaryBreakup.First(e => e.Value.HeaderType == InfyPOS.Data.SalaryManager.SalaryHeaderTypes.ProvidentFund).Value.Amount;

                es.detail = new List<Employeesalary.Detail>();
                foreach (var sitem in nconfig.SalaryBreakup)
                {
                    Employeesalary.Detail detail = new Employeesalary.Detail();
                    detail.id = sitem.Value.HeaderID;
                    detail.amount = sitem.Value.Amount;
                    detail.value = sitem.Value.Value;
                    detail.isdeduction = sitem.Value.IsDeduction;
                    es.detail.Add(detail);
                }
                es.salarymode = config.SalaryMode;
                es.workingmode = config.workingmode;
                es.departmentid = config.departmentid;
                es.divisionid = config.divisionid;
                es.companyid = config.companyid;
                es.holddate = config.Haswithheld ? config.Withheldfrom : DateTime.MinValue;
                es.settlementdate = config.Haswithheld ? DateTime.MinValue : item.paydate.Date;
                es.name = nconfig.Name;
                es.designation = nconfig.Designation;
                es.employeeno = nconfig.EmployeeCode;
                es.leavedays = config.LeaveDays;
                es.presentdays = (salary.computedConfig.totaldays - config.LeaveTaken) - config.AdjustedJoiningResigning;
                es.netsalary = nconfig.SalaryBreakup.Sum(e => e.Value.IsDeduction ? e.Value.Amount * -1 : e.Value.Amount);
                es.salarymonth = new DateTime(todate.Year, todate.Month, 1);
                es.workingdays = nconfig.WorkDayCount;
                es.earnings = es.detail.Sum(e => e.isdeduction ? 0 : e.amount);
                es.deductions = es.detail.Sum(e => e.isdeduction ? e.amount : 0);
                es.paydate = item.paydate.Date;
                salaryList.Add(es);
            }


            {
                NpgsqlTransaction transaction = null;
                try
                {
                    using (NpgsqlConnection conn = CreateConnection())
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();
                        NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                        foreach (var salaryitem in salaryList)
                        {
                            salaryitem.version = 1;
                            salaryitem.isactive = true;
                            salaryitem.createdby = Context.UserID;
                            salaryitem.modifiedby = Context.UserID;
                            salaryitem.createdon = DateTimeNow;
                            salaryitem.modifiedon = DateTimeNow;
                            Insert(transaction, conn, salaryitem);
                        }
                        transaction.Commit();
                        conn.Close();
                    }
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                }
            }
            if (list.Exists(e => e.Haswithheld))
            {
                salaryList.RemoveAll(e => list.Exists(ex => ex.Haswithheld
                && ex.EmployeeID == e.employeeid &&
                ex.Withheldfrom < e.salarymonth.AddMonths(1)));
            }
            return salaryList;
        }
        public HRDashboard Dashboard(HRReportCreteria _reportcreteria)
        {
            HRDashboard lst = new HRDashboard();
            NpgsqlConnection conn = CreateConnection();
            List<HRReport> lstbirthday = new List<HRReport>();
            List<HRReport> lstmarriageday = new List<HRReport>();
            List<HRReport> lstjoiningday = new List<HRReport>();
            List<HRReport> lstmonthjoined = new List<HRReport>();
            List<HRReport> lstmonthrelieved = new List<HRReport>();
            List<HRReport> lstempsection = new List<HRReport>();
            DateTime today = _reportcreteria.fromdate.Date;
            DateTime nextdate = _reportcreteria.todate.AddDays(1).Date;

            try
            {
                var locationfilter = "";
                if (!Context.accessiblelocation.Exists(e => e == -1))
                {
                    if (Context.accessiblelocation.Count == 1)
                        locationfilter = " and employee.locationid = " + Context.accessiblelocation[0].ToString();
                    if (Context.accessiblelocation.Count > 1)
                        locationfilter = " and employee.locationid in (" + string.Join(",", Context.accessiblelocation) + ")";
                }

                if (!Context.accessiblecompany.Exists(e => e == -1))
                {
                    if (Context.accessiblecompany.Count == 1)
                        locationfilter = " and employee.companyid = " + Context.accessiblecompany[0].ToString();
                    if (Context.accessiblecompany.Count > 1)
                        locationfilter = " and employee.companyid in (" + string.Join(",", Context.accessiblecompany) + ")";
                }

                conn.Open();
                NpgsqlCommand command = null;
                command = GetCommand(string.Format(SQL_Birthday, " and hasleft = false"+ locationfilter), conn);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        HRReport oitem = new HRReport();
                        oitem.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        oitem.employeename = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oitem.sectionname = reader["sectionname"] == DBNull.Value ? "" : reader["sectionname"].ToString();
                        oitem.dateofbirth = reader["dateofbirth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofbirth"]);
                        oitem.dateofmarriage = reader["marriagedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["marriagedate"]);
                        oitem.service = "Happy Birthday";
                        oitem.mode = 1;
                        lstbirthday.Add(oitem);
                    }
                }

                command = GetCommand(string.Format(SQL_Marriageday, " and hasleft = false" + locationfilter), conn);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        HRReport oitem = new HRReport();
                        oitem.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        oitem.employeename = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oitem.sectionname = reader["sectionname"] == DBNull.Value ? "" : reader["sectionname"].ToString();
                        oitem.dateofmarriage = reader["marriagedate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["marriagedate"]);
                        oitem.service = "Happy Wedding Anniversary";
                        oitem.mode = 2;
                        lstmarriageday.Add(oitem);
                    }
                }

                command = GetCommand(string.Format(SQL_DateofJoining, " and hasleft = false" + locationfilter), conn);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        HRReport oitem = new HRReport();
                        oitem.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        oitem.employeename = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oitem.sectionname = reader["sectionname"] == DBNull.Value ? "" : reader["sectionname"].ToString();
                        oitem.dateofjoining = reader["dateofjoining"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofjoining"]);
                        oitem.service = "Joining day Wishes";
                        oitem.mode = 3;
                        lstjoiningday.Add(oitem);
                    }
                }

                command = GetCommand(SQL_MonthlyDOJ + locationfilter, conn);

                var monthfrom = new DateTime(today.Year, today.Month, today.Day);

                command.Parameters.Add("monthfromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = monthfrom;
                command.Parameters.Add("monthtodate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = monthfrom.AddMonths(1).AddDays(1);

                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        HRReport oitem = new HRReport();
                        oitem.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        oitem.employeename = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oitem.sectionname = reader["sectionname"] == DBNull.Value ? "" : reader["sectionname"].ToString();
                        oitem.service = reader["service"] == DBNull.Value ? "" : reader["service"].ToString();
                        oitem.dateofjoining = reader["dateofjoining"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofjoining"]);
                        oitem.service = "New Joinee";
                        oitem.mode = 4;
                        lstmonthjoined.Add(oitem);
                    }
                }

                command = GetCommand(string.Format(SQL_MonthlyDOL, locationfilter), conn);
                command.Parameters.Add("monthfromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = monthfrom;
                command.Parameters.Add("monthtodate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = monthfrom.AddMonths(1).AddDays(1);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        HRReport oitem = new HRReport();
                        oitem.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        oitem.employeename = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oitem.service = reader["service"] == DBNull.Value ? "" : reader["service"].ToString();
                        oitem.sectionname = reader["sectionname"] == DBNull.Value ? "" : reader["sectionname"].ToString();
                        oitem.relievingdate = reader["relievingdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["relievingdate"]);
                        oitem.service = "Left Employee";
                        oitem.mode = 5;
                        lstmonthrelieved.Add(oitem);
                    }
                }

                var newquery = SQL_EmployeeAvailable;
                if (DateTimeNow.Date == today.Date)
                {
                    if (DateTimeNow.TimeOfDay.Hours > 14)
                    {
                        newquery = SQL_EmployeeAvailable.Replace("(employeeinout.id is null or employeeinout.morningabsent = true or employeeinout.afternoonabsent = true)",
                                    "(employeeinout.id is null) or employeeinout.afternoonabsent = true");
                    }
                    else
                    {
                        newquery = SQL_EmployeeAvailable.Replace("(employeeinout.id is null or employeeinout.morningabsent = true or employeeinout.afternoonabsent = true)",
                                    "(employeeinout.id is null) or employeeinout.morningabsent = true");
                    }
                }

                command = GetCommand(newquery + locationfilter, conn);
                command.Parameters.Add("fromdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = today;
                command.Parameters.Add("todate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = nextdate;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        HRReport oitem = new HRReport();
                        oitem.sectionname = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oitem.locationname = reader["location"] == DBNull.Value ? "" : reader["location"].ToString();
                        oitem.total = reader["total"] == DBNull.Value ? 0 : Convert.ToInt32(reader["total"]);
                        oitem.absent = reader["absent"] == DBNull.Value ? 0 : Convert.ToInt32(reader["absent"]);
                        oitem.service = "Sectionwise";
                        oitem.absentemployees = new List<string>();
                        lstempsection.Add(oitem);
                    }

                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            var section = reader["section"] == DBNull.Value ? "" : reader["section"].ToString();
                            var location = reader["location"] == DBNull.Value ? "" : reader["location"].ToString();
                            var employeename = reader["absent"] == DBNull.Value ? "" : reader["absent"].ToString();
                            if (lstempsection.Exists(e => e.sectionname == section && e.locationname == location))
                                lstempsection.Find(e => e.sectionname == section && e.locationname == location).absentemployees.Add(employeename);
                        }
                    }

                    lst.absentemployee = lstempsection.Sum(e => e.absent);
                    lst.totalemployee = lstempsection.Sum(e => e.total);
                    CustomerwalkinsDataService customerwalkinsDataService = new CustomerwalkinsDataService() { Context = this.Context };
                    lst.totalwalkin = customerwalkinsDataService.GetWalkinCount(today);
                    if (reader.NextResult())
                    {
                        while (reader.Read())
                        {
                            lst.afternoonabsent = reader["afternoonabsent"] == DBNull.Value ? 0 : Convert.ToInt32(reader["afternoonabsent"]);
                            lst.morningabsent = reader["morningabsent"] == DBNull.Value ? 0 : Convert.ToInt32(reader["morningabsent"]);
                            lst.latecheckin = reader["latecheckin"] == DBNull.Value ? 0 : Convert.ToInt32(reader["latecheckin"]);
                            lst.earlycheckout = reader["earlycheckout"] == DBNull.Value ? 0 : Convert.ToInt32(reader["earlycheckout"]);
                            lst.morningovertime = reader["morningovertime"] == DBNull.Value ? 0 : Convert.ToInt32(reader["morningovertime"]);
                            lst.eveningovertime = reader["eveningovertime"] == DBNull.Value ? 0 : Convert.ToInt32(reader["eveningovertime"]);
                        }
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _reportcreteria);
            }

            lst.list.Add(new HRDashboard.HRInfo()
            {
                name = "Birthday",
                count = lstbirthday.Count,
                detail = lstbirthday
            });
            lst.list.Add(new HRDashboard.HRInfo()
            {
                name = "Marriage",
                count = lstmarriageday.Count,
                detail = lstmarriageday
            });
            lst.list.Add(new HRDashboard.HRInfo()
            {
                name = "New Joinings - Today",
                count = lstjoiningday.Count,
                detail = lstjoiningday
            });
            lst.list.Add(new HRDashboard.HRInfo()
            {
                name = "New Joinings - Month",
                count = lstmonthjoined.Count,
                detail = lstmonthjoined
            });
            lst.list.Add(new HRDashboard.HRInfo()
            {
                name = "Employees Resigned - Month",
                count = lstmonthrelieved.Count,
                detail = lstmonthrelieved
            });
            lst.list.Add(new HRDashboard.HRInfo()
            {
                name = "Sectionwise Employees",
                count = lstempsection.Sum(e => e.total),
                detail = lstempsection
            });

            return lst;
        }

        internal EmployeeSalaryHeaderSchedule SaveSchedule(EmployeeSalaryHeaderSchedule item)
        {
            List<Employeesalaryschedule> lst = new List<Employeesalaryschedule>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                string SQL_SearchSchedule = @"select employeesalaryschedule.id,employeesalaryschedule.employeeid, amount
                                              from employeesalaryschedule
                                              where EXTRACT(MONTH FROM schedulemonth)=@month and EXTRACT(YEAR FROM schedulemonth)=@year and employeesalaryschedule.isactive = true
                                              and isreceivable=false and source=0
                                              and salaryheaderid=@salaryheaderid and employeeid in (" + string.Join(",", item.schedules.ConvertAll(x => x.employeeid)) + ")";

                NpgsqlCommand command = GetCommand(SQL_SearchSchedule, conn);
                command.Parameters.Add("month", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.month;
                command.Parameters.Add("year", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.year;
                command.Parameters.Add("salaryheaderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.headerid;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employeesalaryschedule oemployeesalaryschedule = new Employeesalaryschedule();
                        oemployeesalaryschedule.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oemployeesalaryschedule.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        oemployeesalaryschedule.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        lst.Add(oemployeesalaryschedule);
                    }
                }
                lst.ForEach(x =>
                {
                    if (item.schedules.Exists(y => y.employeeid == x.employeeid))
                    {
                        var schedule = item.schedules.Find(y => y.employeeid == x.employeeid);
                        schedule.id = x.id;
                    }
                });

                var transaction = conn.BeginTransaction();
                var param = new SortedList<string, object>();
                param.Add("id", (long)0);
                param.Add("amount", (decimal)0);
                foreach (var itm in item.schedules.FindAll(x=>x.id>0))
                {
                    param["id"] = itm.id;
                    param["amount"] = itm.amount;
                    ExecuteQuery("update employeesalaryschedule set amount=@amount where id=@id", param, conn, transaction);
                }
                EmployeesalaryscheduleDataService employeesalaryscheduleDataService = new EmployeesalaryscheduleDataService() { Context = this.Context };
                foreach (var itm in item.schedules.FindAll(x => x.id == 0))
                {
                    Employeesalaryschedule oEmployeesalaryschedule = new Employeesalaryschedule();
                    oEmployeesalaryschedule.isactive = true;
                    oEmployeesalaryschedule.companyid = item.companyid;
                    oEmployeesalaryschedule.locationid = item.locationid;
                    oEmployeesalaryschedule.version = 1;
                    oEmployeesalaryschedule.employeeid = itm.employeeid;
                    oEmployeesalaryschedule.isactive = true;
                    oEmployeesalaryschedule.isreceivable = false;
                    oEmployeesalaryschedule.amount = itm.amount;
                    oEmployeesalaryschedule.schedulemonth = new DateTime(item.year, item.month, 1);
                    oEmployeesalaryschedule.salaryheaderid = item.headerid;
                    employeesalaryscheduleDataService.Insert(transaction, conn, oEmployeesalaryschedule, EmployeesalaryscheduleDataService.ScheduleType.General);
                    itm.id = oEmployeesalaryschedule.id;
                }
                foreach (var itm in item.schedules.FindAll(x=>x.salaryid>0))
                {
                    var salaries = Search(new EmployeesalaryCreteria() { id = itm.salaryid });
                    if(salaries.Count == 1)
                    {
                        if(salaries[0].detail.Exists(x=>x.id == item.headerid))
                        {
                            salaries[0].earnings -= salaries[0].detail.Find(x => x.id == item.headerid).amount;
                            salaries[0].detail.Find(x => x.id == item.headerid).amount = itm.amount;
                            salaries[0].earnings += salaries[0].detail.Find(x => x.id == item.headerid).amount;
                        }
                        Update(transaction, conn, salaries[0]);
                    }
                }
                transaction.Commit();
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            return item;
        }

        internal EmployeeSalaryHeaderSchedule LoadSchedule(EmployeeSalaryHeaderSchedule item)
        {
            List<Employeesalaryschedule> lst = new List<Employeesalaryschedule>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                string SQL_SearchSchedule = @"select employeesalaryschedule.id,employeesalaryschedule.employeeid, amount
                                              from employeesalaryschedule
                                              where EXTRACT(MONTH FROM schedulemonth)=@month and EXTRACT(YEAR FROM schedulemonth)=@year and employeesalaryschedule.isactive = true
                                              and salaryheaderid=@salaryheaderid and employeeid in (" + string.Join(",",item.schedules.ConvertAll(x=>x.employeeid))+")";

                NpgsqlCommand command = GetCommand(SQL_SearchSchedule, conn);
                command.Parameters.Add("month", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.month;
                command.Parameters.Add("year", NpgsqlTypes.NpgsqlDbType.Integer).Value = item.year;
                command.Parameters.Add("salaryheaderid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = item.headerid;

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employeesalaryschedule oemployeesalaryschedule = new Employeesalaryschedule();
                        oemployeesalaryschedule.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oemployeesalaryschedule.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        oemployeesalaryschedule.amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        lst.Add(oemployeesalaryschedule);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
            }
            
            lst.ForEach(x =>
            {
                if (item.schedules.Exists(y => y.employeeid == x.employeeid))
                {
                    var schedule = item.schedules.Find(y => y.employeeid == x.employeeid);
                    schedule.id = x.id;
                    schedule.amount = x.amount;
                }
            });

            return item;
        }

        public class HRDashboard : BaseEntity
        {
            public class HRInfo
            {
                public string name { get; set; }
                public int count { get; set; }
                public List<HRReport> detail { get; set; }
            }
            public List<HRInfo> list = new List<HRInfo>();
            public int totalwalkin { get; set; }
            public int totalemployee { get; set; }
            public int absentemployee { get; set; }
            public int afternoonabsent { get; set; }
            public int morningabsent { get; set; }
            public int latecheckin { get; set; }
            public int earlycheckout { get; set; }
            public int morningovertime { get; set; }
            public int eveningovertime { get; set; }
        }
        public class HRReport : BaseEntity
        {
            public string employeecode { get; set; }
            public string employeename { get; set; }
            public string sectionname { get; set; }
            public DateTime dateofbirth { get; set; }
            public DateTime dateofmarriage { get; set; }
            public DateTime withheldfrom { get; set; }
            public DateTime settlementdate { get; set; }
            public DateTime dateofjoining { get; set; }
            public DateTime relievingdate { get; set; }
            public DateTime dateofrelieving { get; set; }
            public string employeeage { get; set; }
            public string service { get; set; }
            public int mode { get; set; }
            public string gender { get; set; }
            public string age { get; set; }
            public string designation { get; set; }
            public bool hassettled { get; set; }
            public bool hasleft { get; set; }
            public string department { get; set; }
            public string community { get; set; }
            public string floor { get; set; }
            public string locationname { get; set; }
            public string religion { get; set; }
            public string caste { get; set; }
            public string company { get; set; }
            public int absent { get; set; }
            public int total { get; set; }
            public List<string> absentemployees { get; set; }
        }
        public class HRReportCreteria : BaseEntity
        {
            public bool allyears { get; set; }
            public string reporttype { get; set; }
            public string employeemode { get; set; }
            public DateTime fromdate { get; set; }
            public DateTime todate { get; set; }
            public bool SearchCondition { get; set; }

            public long companyid { get; set; }
            public long departmentid { get; set; }
            public long sectionid { get; set; }
            public long floorid { get; set; }
            public long locationid { get; set; }
                                       
        }
    }

}
