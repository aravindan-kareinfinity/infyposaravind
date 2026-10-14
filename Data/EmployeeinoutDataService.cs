using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data

{
    public class EmployeeinoutDataService : SAASData
    {
        const string SQL_Search = @"select employeeinout.id, employee.name as employeename,employee.employeecode,
floor.name as floor, section.name as section,
                                    employeeinout.isactive, employeeinout.modifiedon, employeeinout.createdon, employeeinout.createdby, employeeinout.modifiedby, employeeinout.version, 
                                    employeeinout.employeeid, employeeinout.attendanceon, employeeinout.detail, employeeinout.morningovertime, employeeinout.eveningovertime, employeeinout.intime, employeeinout.outtime, 
                                    employeeinout.workinghours, employeeinout.code, employeeinout.morningabsent, employeeinout.afternoonabsent, employeeinout.latecheckin, employeeinout.earlycheckout, employeeinout.nonworking,
                                    employeeinout.valid,employeeinout.actualworking,employeeinout.mode,todayshiftid
                                    from employeeinout 
                                    INNER JOIN employee ON employeeinout.employeeid = employee.id 
						            left outer join referencelist floor on floor.id = floorid
					                left outer join referencelist section on section.id = sectionid 
                                    left outer join referencelist location on location.id = locationid
                                    where  employee.isactive and employeeinout.isactive=true and employeeinout.id=@id";

        const string SQL_Summary = @"select employee.name as employeename,employee.employeecode,floor.name as floor, section.name as section,
                                    sum(employeeinout.morningovertime) morningovertime, sum(employeeinout.eveningovertime) eveningovertime, 
                                    sum(employeeinout.workinghours) workinghours,  sum(case when employeeinout.morningabsent then 1 else 0 end) morningabsent, 
                                    sum(case when employeeinout.afternoonabsent then 1 else 0 end) afternoonabsent, sum(employeeinout.actualworking) actualworking,
                                    sum(employeeinout.latecheckin) latecheckin, sum(employeeinout.earlycheckout) earlycheckout, sum(employeeinout.nonworking) nonworking,
                                    sum(case when employeeinout.valid then 1 else 0 end) validcount,sum(case when employeeinout.valid then 0 else 1 end) invalidcount,sum(employeeinout.actualworking),
                                    count(*) noofdays
                                    from employeeinout 
                                    INNER JOIN employee ON employeeinout.employeeid = employee.id 
                                    left outer join referencelist floor on floor.id = floorid
					                left outer join referencelist section on section.id = sectionid 
                                    where  employee.isactive = true and employeeinout.isactive=true and employeeinout.id=@id";

        const string SQL_Insert = @"INSERT INTO public.employeeinout(isactive, modifiedon, createdon, createdby, modifiedby, version, employeeid, attendanceon, detail, morningovertime, eveningovertime, intime, outtime, 
workinghours, code, morningabsent, afternoonabsent, latecheckin, earlycheckout, nonworking, valid,actualworking,mode,todayshiftid) 
VALUES(@isactive, @modifiedon, @createdon, @createdby, @modifiedby, @version, @employeeid, @attendanceon, @detail, @morningovertime, @eveningovertime, @intime, @outtime, @workinghours, @code, @morningabsent,
@afternoonabsent, @latecheckin, @earlycheckout, @nonworking, @valid,@actualworking,@mode,@todayshiftid) RETURNING id";


        const string SQL_Update = @"UPDATE public.employeeinout SET modifiedon=@modifiedon, modifiedby=@modifiedby, version=@version+1, employeeid=@employeeid, attendanceon=@attendanceon,actualworking=@actualworking, 
detail=@detail, morningovertime=@morningovertime, eveningovertime=@eveningovertime, intime=@intime, outtime=@outtime, workinghours=@workinghours, code=@code, morningabsent=@morningabsent, afternoonabsent=@afternoonabsent, 
latecheckin=@latecheckin, earlycheckout=@earlycheckout, nonworking=@nonworking, valid=@valid,mode=@mode,todayshiftid=@todayshiftid where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.employeeinout set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";



        const string SQL_SelectExisting = @"Select employeeid,code from public.employeeinout where attendanceon=@attendanceon and mode=1 and isactive=true";

        const string SQL_Absenties = @"select employee.id employeeid, attendanceon, morningabsent, afternoonabsent,employeeinout.id inoutid,employeeleave.id as leaveid,
                                        floor.name as floor, section.name as section from employee
                                        LEFT OUTER JOIN employeeinout on employeeinout.employeeid= employee.id and attendanceon=@attendanceon and employeeinout.isactive=true
                                        left outer join employeeleave on employeeleave.employeeid= employee.id and fromdate =@attendanceon and employeeleave.isactive=true
                                        left outer join referencelist floor on floor.id = floorid
					                    left outer join referencelist section on section.id = sectionid 
                                        where employee.isactive = true and (hasleft = false or relievingdate>=@attendanceon) and (employeeinout.id is null or employeeinout.morningabsent = true or employeeinout.afternoonabsent = true)";

        const string SQL_AbsentiesSummary = @"select employee.id employeeid, attendanceon, morningabsent, afternoonabsent,employeeinout.id inoutid,employeeinout.id as leaveid from employee
                                        LEFT OUTER JOIN employeeinout on employeeinout.employeeid= employee.id and attendanceon=@attendanceon 
                                        where  employee.isactive = true and (hasleft = false or relievingdate>=@attendanceon)  and (employeeinout.id is null or employeeinout.morningabsent = true or employeeinout.afternoonabsent = true);
                                        select employeeid,fromdate,todate from employeeleave where isactive=true and (fromdate<=@attendanceon and todate>=@attdenanceon) and autogenerated=false;";

        public List<Employeeinout> Search(EmployeeinoutCreteria _employeeinout,
            NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            NpgsqlCommand command = null;
            List<Employeeinout> lst = new List<Employeeinout>();
            if (_employeeinout.id != 0)
            {
                command = transaction == null ? GetCommand(SQL_Search, conn) : GetCommand(SQL_Search, conn, transaction);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _employeeinout.id;
            }
            else
            {
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));


                if (!_employeeinout.showinactive)
                    qb.AddParameter("employeeinout.isactive", true, "=");

                if (_employeeinout.companyid != 0)
                    qb.AddParameter("employee.companyid", _employeeinout.companyid, "=");

                if (_employeeinout.employeeid != 0)
                    qb.AddParameter("employeeinout.employeeid", _employeeinout.employeeid, "=");

                if (!_employeeinout.showinactive)
                {
                    qb.Add2Query(@" (employee.hasleft = false or employee.relievingdate>=attendanceon) ");
                    qb.Add2Query(@" (attendanceon>=employee.dateofjoining) ");
                }

                if (_employeeinout.departmentid != 0)
                    qb.AddParameter("employee.departmentid", _employeeinout.departmentid, "=");
                if (_employeeinout.locationid != 0)
                    qb.AddParameter("employee.locationid", _employeeinout.locationid, "=");

                if (_employeeinout.workingmode != 0)
                    qb.AddParameter("employee.workingmode", _employeeinout.workingmode, "=");

                if (_employeeinout.salarymode != 0)
                    qb.AddParameter("employee.salarymode", _employeeinout.salarymode, "=");


                if (_employeeinout.ids != null && _employeeinout.ids.Count > 0)
                {
                    qb.AddInParamer("employeeinout.id", string.Join(",", _employeeinout.ids));
                }
                else
                {

                    if (_employeeinout.fromdate.Date == DateTime.MinValue &&
                        _employeeinout.attendanceon.Date != DateTime.MinValue)
                    {
                        qb.AddParameter("attendanceon", "fromdate", _employeeinout.attendanceon.Date, ">=");
                        qb.AddParameter("attendanceon", "todate", _employeeinout.attendanceon.Date.AddDays(1), "<");
                    }
                    else
                    {
                        qb.AddParameter("attendanceon", "fromdate", _employeeinout.fromdate.Date, ">=");
                        if (_employeeinout.fromdate.Date < _employeeinout.todate.Date.AddDays(1))
                        {
                            qb.AddParameter("attendanceon", "todate", _employeeinout.todate.Date.AddDays(1), "<");
                        }
                        else
                        {
                            qb.AddParameter("attendanceon", "todate", _employeeinout.fromdate.Date.AddDays(1), "<");
                        }
                    }
                }
                if (_employeeinout.floorid != 0)
                    qb.AddParameter("employee.floorid", _employeeinout.floorid, "=");

                if (_employeeinout.validmode == 1)
                    qb.AddParameter("valid", true, "=");
                if (_employeeinout.validmode == 2)
                    qb.AddParameter("valid", false, "=");
                if (_employeeinout.validmode == 3)
                    qb.AddParameter("latecheckin", 0, ">");
                if (_employeeinout.validmode == 4)
                    qb.AddParameter("earlycheckout", 0, ">");
                if (_employeeinout.validmode == 5)
                    qb.AddParameter("fromdate", 0, "=");
                if (_employeeinout.validmode == 7)
                    qb.AddORParameter("morningovertime", 0, ">", "eveningovertime>0");
                if (_employeeinout.validmode == 8)
                    qb.AddORParameter("latecheckin", 0, ">", "earlycheckout>0");

                qb.SortField = @"attendanceon desc, case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";

                //qb.Limit = 1000;
                command = transaction == null ? qb.GetCommand(conn) : qb.GetCommand(conn, transaction);
            }

            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    Employeeinout oemployeeinout = new Employeeinout();
                    oemployeeinout.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);

                    oemployeeinout.todayshiftid = reader["todayshiftid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["todayshiftid"]);

                    oemployeeinout.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                    oemployeeinout.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                    oemployeeinout.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                    oemployeeinout.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                    oemployeeinout.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                    oemployeeinout.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                    oemployeeinout.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                    oemployeeinout.attendanceon = reader["attendanceon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["attendanceon"]);
                    oemployeeinout.detail_json = reader["detail"] == DBNull.Value ? null : reader["detail"].ToString();
                    oemployeeinout.morningovertime = reader["morningovertime"] == DBNull.Value ? 0 : Convert.ToInt32(reader["morningovertime"]);
                    oemployeeinout.eveningovertime = reader["eveningovertime"] == DBNull.Value ? 0 : Convert.ToInt32(reader["eveningovertime"]);
                    oemployeeinout.intime = reader["intime"] == DBNull.Value ? 0 : Convert.ToInt32(reader["intime"]);
                    oemployeeinout.outtime = reader["outtime"] == DBNull.Value ? 0 : Convert.ToInt32(reader["outtime"]);
                    oemployeeinout.workinghours = reader["workinghours"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["workinghours"]);
                    oemployeeinout.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                    oemployeeinout.morningabsent = reader["morningabsent"] == DBNull.Value ? false : Convert.ToBoolean(reader["morningabsent"]);
                    oemployeeinout.afternoonabsent = reader["afternoonabsent"] == DBNull.Value ? false : Convert.ToBoolean(reader["afternoonabsent"]);
                    oemployeeinout.latecheckin = reader["latecheckin"] == DBNull.Value ? 0 : Convert.ToInt32(reader["latecheckin"]);
                    oemployeeinout.earlycheckout = reader["earlycheckout"] == DBNull.Value ? 0 : Convert.ToInt32(reader["earlycheckout"]);
                    oemployeeinout.nonworking = reader["nonworking"] == DBNull.Value ? 0 : Convert.ToInt32(reader["nonworking"]);
                    oemployeeinout.actualworking = reader["actualworking"] == DBNull.Value ? 0 : Convert.ToInt32(reader["actualworking"]);
                    oemployeeinout.valid = reader["valid"] == DBNull.Value ? false : Convert.ToBoolean(reader["valid"]);
                    oemployeeinout.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                    oemployeeinout.employeename = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();

                    oemployeeinout.section = reader["section"] == DBNull.Value ? "" : reader["section"].ToString();
                    oemployeeinout.floor = reader["floor"] == DBNull.Value ? "" : reader["floor"].ToString();
                    lst.Add(oemployeeinout);
                }
            }
            return lst;
        }
        public List<Employeeinout> Search(EmployeeinoutCreteria _employeeinout)
        {

            if (_employeeinout.validmode == 5)
            {
                _employeeinout.todate = _employeeinout.fromdate;
                return SearchAbsenties(_employeeinout);
            }

            if (_employeeinout.validmode == 6)
                return SearchSummary(_employeeinout).ConvertAll(e => e as Employeeinout);

            List<Employeeinout> lst = new List<Employeeinout>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                lst = Search(_employeeinout, conn, null);
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _employeeinout);
            }
            return lst;
        }



        //
        public List<EmployeeinoutSummary> SearchSummary(EmployeeinoutCreteria _employeeinout)
        {

            List<EmployeeinoutSummary> lst = new List<EmployeeinoutSummary>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_employeeinout.id != 0)
                {
                    command = GetCommand(SQL_Summary, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _employeeinout.id;
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    qb.CurrentQuery = SQL_Summary.Substring(0, SQL_Summary.IndexOf("where"));
                    qb.AddParameter("employeeinout.isactive", true, "=");

                    qb.Add2Query(@" (employee.hasleft = false or employee.relievingdate>=attendanceon) ");
                    qb.Add2Query(@" (attendanceon>=employee.dateofjoining) ");

                    if (_employeeinout.companyid != 0)
                        qb.AddParameter("employee.companyid", _employeeinout.companyid, "=");

                    if (_employeeinout.employeeid != 0)
                        qb.AddParameter("employeeinout.employeeid", _employeeinout.employeeid, "=");

                    if (_employeeinout.departmentid != 0)
                        qb.AddParameter("employee.departmentid", _employeeinout.departmentid, "=");
                    if (_employeeinout.locationid != 0)
                        qb.AddParameter("employee.locationid", _employeeinout.locationid, "=");

                    if (_employeeinout.workingmode != 0)
                        qb.AddParameter("employee.workingmode", _employeeinout.workingmode, "=");

                    if (_employeeinout.salarymode != 0)
                        qb.AddParameter("employee.salarymode", _employeeinout.salarymode, "=");

                    qb.AddParameter("attendanceon", "fromdate", _employeeinout.fromdate.Date, ">=");
                    qb.AddParameter("attendanceon", "todate", _employeeinout.todate.Date.AddDays(1), "<");

                    if (_employeeinout.floorid != 0)
                        qb.AddParameter("employee.floorid", _employeeinout.floorid, "=");



                    if (_employeeinout.validmode == 1)
                        qb.AddParameter("valid", true, "=");
                    if (_employeeinout.validmode == 2)
                        qb.AddParameter("valid", false, "=");
                    if (_employeeinout.validmode == 3)
                        qb.AddParameter("latecheckin", 0, ">");
                    if (_employeeinout.validmode == 4)
                        qb.AddParameter("earlycheckout", 0, ">");
                    if (_employeeinout.validmode == 5)
                        qb.AddParameter("fromdate", 0, "=");

                    qb.SortField = @"case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";
                    qb.GroupBY = "employee.name ,employee.employeecode,floor.name, section.name";

                    command = qb.GetCommand(conn);
                }

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        EmployeeinoutSummary oemployeeinout = new EmployeeinoutSummary();
                        oemployeeinout.morningovertime = reader["morningovertime"] == DBNull.Value ? 0 : Convert.ToInt32(reader["morningovertime"]);
                        oemployeeinout.eveningovertime = reader["eveningovertime"] == DBNull.Value ? 0 : Convert.ToInt32(reader["eveningovertime"]);
                        oemployeeinout.workinghours = reader["workinghours"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["workinghours"]);
                        oemployeeinout.morningabsentcount = reader["morningabsent"] == DBNull.Value ? 0 : Convert.ToInt32(reader["morningabsent"]);
                        oemployeeinout.afternoonabsentcount = reader["afternoonabsent"] == DBNull.Value ? 0 : Convert.ToInt32(reader["afternoonabsent"]);
                        oemployeeinout.latecheckin = reader["latecheckin"] == DBNull.Value ? 0 : Convert.ToInt32(reader["latecheckin"]);
                        oemployeeinout.earlycheckout = reader["earlycheckout"] == DBNull.Value ? 0 : Convert.ToInt32(reader["earlycheckout"]);
                        oemployeeinout.nonworking = reader["nonworking"] == DBNull.Value ? 0 : Convert.ToInt32(reader["nonworking"]);
                        oemployeeinout.actualworking = reader["actualworking"] == DBNull.Value ? 0 : Convert.ToInt32(reader["actualworking"]);
                        oemployeeinout.validcount = reader["validcount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["validcount"]);
                        oemployeeinout.noofdays = reader["noofdays"] == DBNull.Value ? 0 : Convert.ToInt32(reader["noofdays"]);
                        oemployeeinout.invalidcount = reader["invalidcount"] == DBNull.Value ? 0 : Convert.ToInt32(reader["invalidcount"]);
                        oemployeeinout.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        oemployeeinout.employeename = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();

                        oemployeeinout.floor = reader["floor"] == DBNull.Value ? "" : reader["floor"].ToString();
                        oemployeeinout.section = reader["section"] == DBNull.Value ? "" : reader["section"].ToString();

                        lst.Add(oemployeeinout);
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _employeeinout);
            }
            return lst;
        }




        public List<Employeeinout> SearchAbsenties(EmployeeinoutCreteria _employeeinout)
        {
            if (_employeeinout.todate == DateTime.MinValue)
                _employeeinout.todate = _employeeinout.fromdate;
            List<Employeeinout> lst = new List<Employeeinout>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = @"select employee.id employeeid, attendanceon, morningabsent, afternoonabsent,morningovertime,eveningovertime,
floor.name as floor, section.name as section,intime,outtime,workinghours,latecheckin,earlycheckout,nonworking,actualworking,valid,employeeinout.id inoutid,employeeinout.id as leaveid, employee.employeecode as employeecode,employee.name as name  from employee
                                        left outer join employeeinout on employeeinout.employeeid= employee.id and attendanceon = @attendanceon
						  left outer join referencelist floor on floor.id = floorid
					left outer join referencelist section on section.id = sectionid 
                                        where employee.isactive = true and (employee.hasleft = false or employee.relievingdate>=@attendanceon) and (employeeinout.id is null or employeeinout.morningabsent = true or employeeinout.afternoonabsent = true)";

                if (_employeeinout.fromdate.Date != _employeeinout.todate.Date)
                {
                    qb.CurrentQuery = @"select employee.id employeeid, attendanceon, morningabsent, afternoonabsent,morningovertime,eveningovertime,intime,outtime,workinghours,latecheckin,earlycheckout,nonworking,
                                        actualworking,valid,employeeinout.id inoutid,employeeinout.id as leaveid,
                                        floor.name as floor, section.name as section, employee.employeecode as employeecode,employee.name as name  from employeeinout
                                        inner join employee on employee.id = employeeinout.employeeid
						                left outer join referencelist floor on floor.id = floorid
					                    left outer join referencelist section on section.id = sectionid 

                                        where (employee.hasleft = false or employee.relievingdate>=@todate) and employeeinout.isactive=true and (employeeinout.morningabsent = true or employeeinout.afternoonabsent = true)";

                    qb.AddParameter("attendanceon", "fromdate", _employeeinout.fromdate.Date, ">=");
                    qb.AddParameter("attendanceon", "todate", _employeeinout.todate.Date.AddDays(1), "<");
                }

                qb.Add2Query(@" (employee.hasleft = false or employee.relievingdate>=attendanceon) ");
                qb.Add2Query(@" (attendanceon>=employee.dateofjoining) ");

                if (_employeeinout.companyid != 0)
                    qb.AddParameter("employee.companyid", _employeeinout.companyid, "=");

                if (_employeeinout.employeeid != 0)
                    qb.AddParameter("employee.id", _employeeinout.employeeid, "=");

                if (_employeeinout.departmentid != 0)
                    qb.AddParameter("employee.departmentid", _employeeinout.departmentid, "=");
                if (_employeeinout.locationid != 0)
                    qb.AddParameter("employee.locationid", _employeeinout.locationid, "=");

                if (_employeeinout.floorid != 0)
                    qb.AddParameter("employee.floorid", _employeeinout.floorid, "=");

                qb.SortField = @"case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";

                var command = qb.GetCommand(conn);
                command.Parameters.Add(new NpgsqlParameter()
                {
                    NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Timestamp,
                    ParameterName = "attendanceon",
                    NpgsqlValue = _employeeinout.fromdate.Date
                });

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employeeinout oemployeeinout = new Employeeinout();
                        oemployeeinout.id = reader["leaveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["leaveid"]);
                        //oemployeeinout.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        //oemployeeinout.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        //oemployeeinout.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        //oemployeeinout.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        //oemployeeinout.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        //oemployeeinout.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oemployeeinout.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        oemployeeinout.attendanceon = reader["attendanceon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["attendanceon"]);
                        //oemployeeinout.detail_json = reader["detail"] == DBNull.Value ? null : reader["detail"].ToString();
                        oemployeeinout.morningovertime = reader["morningovertime"] == DBNull.Value ? 0 : Convert.ToInt32(reader["morningovertime"]);
                        oemployeeinout.eveningovertime = reader["eveningovertime"] == DBNull.Value ? 0 : Convert.ToInt32(reader["eveningovertime"]);
                        oemployeeinout.intime = reader["intime"] == DBNull.Value ? 0 : Convert.ToInt32(reader["intime"]);
                        oemployeeinout.outtime = reader["outtime"] == DBNull.Value ? 0 : Convert.ToInt32(reader["outtime"]);
                        oemployeeinout.workinghours = reader["workinghours"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["workinghours"]);
                        //oemployeeinout.code = reader["code"] == DBNull.Value ? "" : reader["code"].ToString();
                        oemployeeinout.morningabsent = reader["morningabsent"] == DBNull.Value ? true : Convert.ToBoolean(reader["morningabsent"]);
                        oemployeeinout.afternoonabsent = reader["afternoonabsent"] == DBNull.Value ? true : Convert.ToBoolean(reader["afternoonabsent"]);
                        oemployeeinout.latecheckin = reader["latecheckin"] == DBNull.Value ? 0 : Convert.ToInt32(reader["latecheckin"]);
                        oemployeeinout.earlycheckout = reader["earlycheckout"] == DBNull.Value ? 0 : Convert.ToInt32(reader["earlycheckout"]);
                        oemployeeinout.nonworking = reader["nonworking"] == DBNull.Value ? 0 : Convert.ToInt32(reader["nonworking"]);
                        oemployeeinout.actualworking = reader["actualworking"] == DBNull.Value ? 0 : Convert.ToInt32(reader["actualworking"]);
                        oemployeeinout.valid = reader["valid"] == DBNull.Value ? false : Convert.ToBoolean(reader["valid"]);
                        oemployeeinout.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        oemployeeinout.employeename = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oemployeeinout.floor = reader["floor"] == DBNull.Value ? "" : reader["floor"].ToString();
                        oemployeeinout.section = reader["section"] == DBNull.Value ? "" : reader["section"].ToString();
                        if (oemployeeinout.intime == 0)
                        {
                            if (_employeeinout.fromdate.Date == DateTimeNow.Date)
                            {
                                if (oemployeeinout.afternoonabsent && DateTimeNow.Hour < 15)
                                    oemployeeinout.afternoonabsent = false;
                            }
                            lst.Add(oemployeeinout);
                        }
                    }
                }

                command = GetCommand(@"select employeeid,fromdate,todate from employeeleave where isactive=true and (fromdate<=@attendanceon and todate>=@attendanceon) and autogenerated=false;",
                    conn);
                command.Parameters.Add(new NpgsqlParameter()
                {
                    NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Timestamp,
                    ParameterName = "attendanceon",
                    NpgsqlValue = _employeeinout.fromdate.Date
                });

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        long employeeid = reader.GetInt64(0);
                        if (lst.Exists(e => e.employeeid == employeeid))
                            lst.Find(e => e.employeeid == employeeid).approved = true;
                    }
                }

                List<ShiftAllocation.Allocation> allocationList = new List<ShiftAllocation.Allocation>();
                command = GetCommand(@"select allocation from shiftallocation where isactive=true and (fromdate<=@attendanceon and todate>=@attendanceon);",
                   conn);
                command.Parameters.Add(new NpgsqlParameter()
                {
                    NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Timestamp,
                    ParameterName = "attendanceon",
                    NpgsqlValue = _employeeinout.fromdate.Date
                });
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        ShiftAllocation tShift = new ShiftAllocation();
                        tShift.allocation_json = reader.GetString(0);
                        allocationList.AddRange(tShift.allocation);
                    }
                }
                conn.Close();

                var shiftEmployees = allocationList.FindAll(e => e.shifts.Count > 0 && e.shifts.First().date.DayOfWeek == _employeeinout.fromdate.DayOfWeek).ToList();
                foreach (var emp in lst)
                {
                    if (shiftEmployees.Exists(e => e.employeeid == emp.employeeid))
                        emp.shift = true;
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _employeeinout);
            }
            return lst;
        }
        public Employeeinout Insert(Employeeinout oEmployeeinout, int mode)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                UpdateInoutTimings(oEmployeeinout);
                if (mode == 1)
                {
                    var existing = Search(new EmployeeinoutCreteria()
                    {
                        employeeid = oEmployeeinout.employeeid,
                        fromdate = oEmployeeinout.attendanceon.Date,
                        todate = oEmployeeinout.attendanceon.Date
                    });
                    if (existing.Count > 0)
                    {
                        existing[0].intime = oEmployeeinout.intime;
                        existing[0].outtime = oEmployeeinout.outtime;
                        existing[0].workinghours = oEmployeeinout.workinghours;
                        existing[0].nonworking = oEmployeeinout.nonworking;
                        existing[0].latecheckin = oEmployeeinout.latecheckin;
                        existing[0].earlycheckout = oEmployeeinout.earlycheckout;
                        existing[0].morningovertime = oEmployeeinout.morningovertime;
                        existing[0].eveningovertime = oEmployeeinout.eveningovertime;
                        existing[0].afternoonabsent = oEmployeeinout.afternoonabsent;
                        existing[0].morningabsent = oEmployeeinout.morningabsent;
                        return Update(existing[0]);
                    }
                }
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    Insert(transaction, conn, oEmployeeinout, mode);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeeinout);
            }

            return oEmployeeinout;
        }

        public void UpdateInoutTimings(Employeeinout oEmployeeinout)
        {
            InfyPOS.Data.ReferencelistDataService refList = new ReferencelistDataService() { Context = this.Context };
            //var workinghour = refList.Search(new ReferencelistCreteria()
            //{
            //    type = "WORKINGHOURS"
            //});


            var workinghours = DefaultWorkingHourse(oEmployeeinout.employeeid);
            if (workinghours.Item2 == null) return;

            var hourconfig = workinghours.Item2;
            var fromtime = BaseEntity.ParseTime(hourconfig.fromtime);
            var totime = BaseEntity.ParseTime(hourconfig.totime);
            var fullday = BaseEntity.ParseTime(hourconfig.eligbleday);
            var halfday = BaseEntity.ParseTime(hourconfig.present);
            if (fromtime.TotalMinutes != 0 && totime.TotalMinutes != 0)
            {
                DateTime start = oEmployeeinout.attendanceon.AddMinutes(oEmployeeinout.intime);
                DateTime end = oEmployeeinout.attendanceon.AddMinutes(oEmployeeinout.outtime);
                oEmployeeinout.workinghours = (decimal)end.Subtract(start).TotalHours;
                oEmployeeinout.morningovertime = 0;
                oEmployeeinout.eveningovertime = 0;
                oEmployeeinout.latecheckin = 0;
                oEmployeeinout.earlycheckout = 0;
                if (fullday.TotalMinutes > 0 && halfday.TotalMinutes > 0)
                {
                    oEmployeeinout.morningabsent = false;
                    oEmployeeinout.afternoonabsent = false;
                    if (end.Subtract(start).TotalMinutes < fullday.TotalMinutes)
                    {
                        if (halfday.TotalMinutes < end.Subtract(start).TotalMinutes)
                        {
                            var startDiff = Math.Abs(start.TimeOfDay.TotalMinutes - fromtime.TotalMinutes);
                            var endDiff = Math.Abs(end.TimeOfDay.TotalMinutes - totime.TotalMinutes);
                            if (startDiff < endDiff)
                            {
                                oEmployeeinout.afternoonabsent = true;
                            }
                            else
                            {
                                oEmployeeinout.morningabsent = true;
                            }
                        }
                        else
                        {
                            oEmployeeinout.morningabsent = true;
                            oEmployeeinout.afternoonabsent = true;
                        }
                    }
                }

                if (fromtime > start.TimeOfDay)
                    oEmployeeinout.morningovertime = (int)(fromtime - start.TimeOfDay).TotalMinutes;
                else if (!oEmployeeinout.morningabsent)
                    oEmployeeinout.latecheckin = (int)start.TimeOfDay.Subtract(fromtime).TotalMinutes;

                if (totime < end.TimeOfDay)
                    oEmployeeinout.eveningovertime = (int)(end.TimeOfDay - totime).TotalMinutes;
                else if (!oEmployeeinout.afternoonabsent)
                    oEmployeeinout.earlycheckout = (int)totime.Subtract(end.TimeOfDay).TotalMinutes;
            }
        }

        public Employeeinout Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeeinout oEmployeeinout, int mode)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            try
            {
                command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeinout.isactive;
                command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
                command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
                command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.version;
                command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeinout.employeeid;
                command.Parameters.Add("todayshiftid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeinout.todayshiftid;
                command.Parameters.Add("attendanceon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeeinout.attendanceon;
                command.Parameters.Add("detail", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployeeinout.detail_json == null ? "null" : oEmployeeinout.detail_json;
                command.Parameters.Add("morningovertime", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.morningovertime;
                command.Parameters.Add("eveningovertime", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.eveningovertime;
                command.Parameters.Add("intime", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.intime;
                command.Parameters.Add("outtime", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.outtime;
                command.Parameters.Add("workinghours", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeeinout.workinghours;
                command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployeeinout.code == null ? "" : oEmployeeinout.code;
                command.Parameters.Add("morningabsent", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeinout.morningabsent;
                command.Parameters.Add("afternoonabsent", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeinout.afternoonabsent;
                command.Parameters.Add("latecheckin", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.latecheckin;
                command.Parameters.Add("earlycheckout", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.earlycheckout;
                command.Parameters.Add("nonworking", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.nonworking;
                command.Parameters.Add("actualworking", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.actualworking;
                command.Parameters.Add("valid", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeinout.valid;
                command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = mode;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        oEmployeeinout.id = Convert.ToInt64(reader[0].ToString());
                    }
                }
                return oEmployeeinout;
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("InOut.Insert", WebAPI.Common.Logger.LogType.Info, "InOut.Insert", GetSQL(command));
                throw exp;
            }
        }

        public Employeeinout Update(Employeeinout oEmployeeinout)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                UpdateInoutTimings(oEmployeeinout);
                bool changeleave = false;
                var oldList = Search(new EmployeeinoutCreteria()
                {
                    id = oEmployeeinout.id
                });
                InfyPOS.Models.EmployeeLeave lr = null;
                EmployeeLeaveDataService edl = new EmployeeLeaveDataService() { Context = this.Context };
                edl.Context = this.Context;
                changeleave = (oldList.Count == 1 && (oldList[0].morningabsent != oEmployeeinout.morningabsent || oldList[0].afternoonabsent != oEmployeeinout.afternoonabsent));
                if (changeleave)
                {
                    var leavelist = edl.Search(new LeaverequestCreteria()
                    {
                        employeeid = oEmployeeinout.employeeid,
                        fromdate = oEmployeeinout.attendanceon,
                        searchmode = 1
                    });
                    if (leavelist.Count > 0)
                        lr = leavelist[0];
                }

                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    if (changeleave)
                    {
                        if (lr != null)
                        {
                            edl.Delete(transaction, conn, lr);
                        }

                        if (oEmployeeinout.afternoonabsent || oEmployeeinout.morningabsent)
                        {
                            EmployeeLeave ea = new EmployeeLeave();
                            ea.employeeid = oEmployeeinout.employeeid;
                            ea.fromdate = oEmployeeinout.attendanceon.Date;
                            ea.todate = oEmployeeinout.attendanceon.Date;

                            if (oEmployeeinout.morningabsent)
                            {
                                ea.fromdatehalfday = EmployeeLeave.DayMode.Morning;
                                ea.days += (decimal)0.5;
                            }
                            if (oEmployeeinout.afternoonabsent)
                            {
                                ea.fromdatehalfday = EmployeeLeave.DayMode.Evening;
                                ea.days += (decimal)0.5;
                            }
                            ea.autogenerated = true;
                            ea.version = 1;
                            ea.deductsalary = true;
                            ea.isactive = true;
                            ea.sundaydays = 0;
                            if (ea.todate.DayOfWeek == DayOfWeek.Sunday)
                                ea.sundaydays = ea.days;
                            edl.Context = this.Context;
                            edl.Insert(transaction, conn, ea, false);
                        }
                    }
                    Update(transaction, conn, oEmployeeinout);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeeinout);


            }

            return oEmployeeinout;
        }

        public Employeeinout Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeeinout oEmployeeinout)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeinout.id;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeinout.isactive;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeeinout.createdon;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeinout.createdby;
            command.Parameters.Add("todayshiftid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeinout.todayshiftid;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.version;
            command.Parameters.Add("employeeid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeinout.employeeid;
            command.Parameters.Add("attendanceon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployeeinout.attendanceon;
            command.Parameters.Add("detail", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployeeinout.detail_json == null ? "null" : oEmployeeinout.detail_json;
            command.Parameters.Add("morningovertime", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.morningovertime;
            command.Parameters.Add("eveningovertime", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.eveningovertime;
            command.Parameters.Add("intime", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.intime;
            command.Parameters.Add("outtime", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.outtime;
            command.Parameters.Add("workinghours", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployeeinout.workinghours;
            command.Parameters.Add("code", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployeeinout.code == null ? "" : oEmployeeinout.code;
            command.Parameters.Add("morningabsent", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeinout.morningabsent;
            command.Parameters.Add("afternoonabsent", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeinout.afternoonabsent;
            command.Parameters.Add("latecheckin", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.latecheckin;
            command.Parameters.Add("earlycheckout", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.earlycheckout;
            command.Parameters.Add("nonworking", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.nonworking;
            command.Parameters.Add("actualworking", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.actualworking;
            command.Parameters.Add("valid", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeinout.valid;
            command.Parameters.Add("mode", NpgsqlTypes.NpgsqlDbType.Integer).Value = 1;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmployeeinout.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oEmployeeinout;
        }


        public bool Delete(Employeeinout oEmployeeinout)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oEmployeeinout);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployeeinout);



            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Employeeinout oEmployeeinout)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployeeinout.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployeeinout.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployeeinout.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

        //private DateTime LockDate
        //{
        //    get
        //    {
        //        ReferencelistDataService refdata = new ReferencelistDataService() { Context = this.Context };
        //        var rows = refdata.Search(new ReferencelistCreteria()
        //        {
        //            type = "SETTINGS",
        //            code = "LOCKDATE"
        //        });
        //        if (rows != null && rows.Count > 0)
        //        {
        //            DateTime dt = BaseEntity.GetDataFromString(rows[0].name);
        //            return dt;
        //        }
        //        return DateTime.MinValue;
        //    }
        //}
        private static System.Globalization.CultureInfo enUS = new System.Globalization.CultureInfo("en-US");
        internal Tuple<long, Referencelist.Attribute, TimeSpan, TimeSpan> DefaultWorkingHourse(long employeeid)
        {
            string query = @"select defaultshiftid,additionalinfo from employee 
                            inner join referencelist on referencelist.id = defaultshiftid
                            where employee.id = @employeeid";

            var param = new SortedList<string, object>();
            param.Add("employeeid", employeeid);
            long defaultshiftid = 0;
            TimeSpan tsfrom = TimeSpan.Zero;
            TimeSpan tsto = TimeSpan.Zero;
            using (var reader = ExecuteReader(query, param))
            {
                Referencelist.Attribute additionalinfo = null;
                if (reader.Read())
                {
                    try
                    {
                        defaultshiftid = reader["defaultshiftid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["defaultshiftid"]);
                        additionalinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Referencelist.Attribute>(reader["additionalinfo"] == DBNull.Value ? "null" : reader["additionalinfo"].ToString());
                    }
                    catch (Exception exp)
                    {

                    }
                }
                if (additionalinfo != null)
                {
                    if (TimeSpan.TryParseExact(additionalinfo.fromtime, "HH:mm", enUS, out tsfrom) &&
                        TimeSpan.TryParseExact(additionalinfo.totime, "HH:mm", enUS, out tsto))
                    {
                        return new Tuple<long, Referencelist.Attribute, TimeSpan, TimeSpan>(defaultshiftid, additionalinfo, tsfrom, tsto);
                    }
                }
                return new Tuple<long, Referencelist.Attribute, TimeSpan, TimeSpan>(defaultshiftid, additionalinfo, tsfrom, tsto);
            }
        }


        public class EmployeeCode
        {
            public long employeeid { get; set; }
            public string code { get; set; }
            public long shiftid { get; set; }
            public bool hasleft { get; set; }
            public DateTime dateofjoining { get; set; }
            public DateTime relievingdate { get; set; }
        }

        private static object lockingPost = new object();
        public List<EmployeeinoutDirect> PostDirect(EmployeeinoutCreteriaDirect item)
        {
            if (string.IsNullOrEmpty(item.machineid) || string.IsNullOrEmpty(item.sourcekey))
            {
                throw new Exception("Un-Authroized Entries");
            }

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_locakdate.Year > 2000 &&
               Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_locakdate > item.date)
            {
                return new List<EmployeeinoutDirect>();
            }

            lock (lockingPost)
            {
                //WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(("PostDirect - " + item.machineid + " " + item.sourcekey+" "+item.date), WebAPI.Common.Logger.LogType.Error, "");

                NpgsqlConnection conn = null;
                NpgsqlTransaction transaction = null;

                try
                {
                    System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                    sw.Start();
                    long locationid = 0;
                    var parameters = new SortedList<string, object>();
                    SortedDictionary<string, EmployeeCode> empcodeid = new SortedDictionary<string, EmployeeCode>();
                    List<string> codes = new List<string>();
                    item.data.ForEach(e => codes.Add("'" + WebAPI.Data.Utility.SafeSQL(e.code) + "'"));
                    int length = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.employee_minimumcode;

                    string query = string.Format("select id,lpad(employeecode,{1},'{2}') employeecode,defaultshiftid,hasleft,dateofjoining,relievingdate from employee where isactive=true and lpad(employeecode,{1},'{2}') in ({0})",
                        string.Join(",", codes), length, new string('0', length));

                    if (length == 0)
                        query = string.Format("select id,employeecode,defaultshiftid,hasleft,dateofjoining,relievingdate from employee where isactive=true and employeecode in ({0})",
                        string.Join(",", codes));

                    if (!string.IsNullOrEmpty(item.location))
                    {
                        ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                        var locations = referencelistDataService.SearchList(new ReferencelistCreteria() { code = item.location });
                        if (locations.Count == 1)
                        {
                            query += " and locationid=" + locations[0].id;
                            locationid = locations[0].id;
                        }
                    }

                    using (var reader = ExecuteReader(query, new SortedList<string, object>()))
                    {
                        while (reader.Read())
                        {
                            if (!empcodeid.ContainsKey(reader.GetString(1)))
                            {
                                var empcode = new EmployeeCode()
                                {
                                    employeeid = reader.GetInt64(0),
                                    code = reader.GetString(1),
                                    shiftid = reader.IsDBNull(2) ? 0 : reader.GetInt64(2),
                                    hasleft = reader.IsDBNull(3) ? false : reader.GetBoolean(3),
                                    dateofjoining = reader.IsDBNull(4) ? DateTimeNow.Date : reader.GetDateTime(4),
                                    relievingdate = reader.IsDBNull(5) ? DateTimeNow.Date : reader.GetDateTime(5)
                                };
                                if ((empcode.hasleft && item.date.Date > empcode.relievingdate.Date) || empcode.dateofjoining.Date > item.date.Date) continue;
                                empcodeid.Add(reader.GetString(1), empcode);
                            }
                        }
                    }
                    sw.Stop();
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(("PostDirect - Data Preparation " + sw.Elapsed.Milliseconds), WebAPI.Common.Logger.LogType.Info, "");

                    var missingEmployeeCodes = item.data.FindAll(x => !empcodeid.ContainsKey(x.code));
                    if (missingEmployeeCodes.Count > 0)
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).PerformanceLog("Missing Employees from Attendance Card", WebAPI.Common.Logger.LogType.Info,
                            "POSTDirect", missingEmployeeCodes, 0, 0);
                    }
                    if (empcodeid.Count == 0)
                    {
                        throw new Exception("Employee code not synchronized with given TA data");
                    }

                    //var existingIO = Search(new EmployeeinoutCreteria()
                    //{
                    //    attendanceon = item.date,
                    //    locationid = locationid
                    //});
                    InfyPOS.Data.ReferencelistDataService refList = new ReferencelistDataService() { Context = this.Context };
                    var workinghour = refList.Search(new ReferencelistCreteria()
                    {
                        type = "WORKINGHOURS"
                    });

                    sw.Reset();
                    sw.Start();

                    List<Employeeinout> retvalue = new List<Employeeinout>();

                    using (conn = CreateConnection())
                    {
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(("PostDirect - Connecting database"), WebAPI.Common.Logger.LogType.Info, "");
                        conn.Open();
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(("PostDirect - Connected database"), WebAPI.Common.Logger.LogType.Info, "");


                        transaction = conn.BeginTransaction();

                        string SQL_DeleteAll_v1 = @"update public.employeeinout set isactive=false,modifiedon=@currentdatetime,version=9 where attendanceon=@attendanceon and mode=0 and isactive=true;";

                        string SQL_DeleteAll_v2 = @"update public.employeeleave set isactive=false,modifiedon=@currentdatetime,version=9 where autogenerated=true and fromdate=@attendanceon and isactive=true";
                        
                        if (locationid > 0)
                        {
                            SQL_DeleteAll_v1 = @"update public.employeeinout set isactive=false,modifiedon=@currentdatetime where attendanceon=@attendanceon 
                                              and mode=0 and isactive=true and employeeid in (select id from employee where locationid=@locationid);";

                            SQL_DeleteAll_v2 = @"update public.employeeleave set isactive=false,modifiedon=@currentdatetime where autogenerated=true 
                                       and fromdate=@attendanceon  and isactive=true and employeeid in (select id from employee where locationid=@locationid)";
                        }

                        NpgsqlCommand command = GetCommand(SQL_DeleteAll_v1, conn, transaction);
                        command.Parameters.Add(new NpgsqlParameter()
                        {
                            DbType = System.Data.DbType.DateTime,
                            ParameterName = "attendanceon",
                            NpgsqlValue = item.date.Date
                        });
                        command.Parameters.Add(new NpgsqlParameter()
                        {
                            DbType = System.Data.DbType.Int64,
                            ParameterName = "locationid",
                            NpgsqlValue = locationid
                        });
                        command.Parameters.Add(new NpgsqlParameter()
                        {
                            DbType = System.Data.DbType.DateTime,
                            ParameterName = "currentdatetime",
                            NpgsqlValue = DateTimeNow
                        });
                        var rows = command.ExecuteNonQuery();
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).PerformanceLog("PostDirect - Remove Old Auto Attendance", WebAPI.Common.Logger.LogType.Info,
                            "POSTDirect", item.date.Date.ToString("dd-MM-yyyy") + "-" + rows, 0, 0);

                        command = GetCommand(SQL_DeleteAll_v2, conn, transaction);
                        command.Parameters.Add(new NpgsqlParameter()
                        {
                            DbType = System.Data.DbType.DateTime,
                            ParameterName = "attendanceon",
                            NpgsqlValue = item.date.Date
                        });
                        command.Parameters.Add(new NpgsqlParameter()
                        {
                            DbType = System.Data.DbType.Int64,
                            ParameterName = "locationid",
                            NpgsqlValue = locationid
                        });
                        command.Parameters.Add(new NpgsqlParameter()
                        {
                            DbType = System.Data.DbType.DateTime,
                            ParameterName = "currentdatetime",
                            NpgsqlValue = DateTimeNow
                        });
                        rows = command.ExecuteNonQuery();
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).PerformanceLog("PostDirect - Remove Old Auto Leave", WebAPI.Common.Logger.LogType.Info,
                            "POSTDirect", item.date.Date.ToString("dd-MM-yyyy") + "-" + rows, 0, 0);

                        query = SQL_SelectExisting;
                        command = GetCommand(query, conn, transaction);
                        command.Parameters.Add(new NpgsqlParameter()
                        {
                            DbType = System.Data.DbType.DateTime,
                            ParameterName = "attendanceon",
                            NpgsqlValue = item.date.Date
                        });
                        command.Parameters.Add(new NpgsqlParameter()
                        {
                            DbType = System.Data.DbType.Int64,
                            ParameterName = "locationid",
                            NpgsqlValue = locationid
                        });
                        List<long> skippingemployeeid = new List<long>();
                        List<string> skipemployeecodes = new List<string>();
                        using (var reader = command.ExecuteReader())
                        {
                            while (reader.Read())
                            {
                                skippingemployeeid.Add(reader.GetInt64(0));
                                skipemployeecodes.Add(reader.GetString(1));
                            }
                        }

                        WebAPI.Common.Logger.Instance(Context.OrganizationID).PerformanceLog("PostDirect - Skip employees", WebAPI.Common.Logger.LogType.Info,
                            "POSTDirect", item.date.Date.ToString("dd-MM-yyyy") +"-"+ 
                            skippingemployeeid.Count+"-"+string.Join(",",skipemployeecodes) + "-" + string.Join(",", skippingemployeeid), 0, 0);

                        List<long> empids = new List<long>();
                        foreach (var ioitem in item.data)
                        {
                            if (!empcodeid.ContainsKey(ioitem.code))
                                continue;

                            if (skippingemployeeid.Contains(empcodeid[ioitem.code].employeeid)) continue;

                            var oEmployeeinout = new Employeeinout();
                            oEmployeeinout.version = 1;
                            oEmployeeinout.afternoonabsent = false;
                            oEmployeeinout.morningabsent = false;
                            if (item.date.Date == DateTimeNow.Date)
                            {
                                if (ioitem.afternoonabsent && DateTimeNow.Hour > 15)
                                    oEmployeeinout.afternoonabsent = ioitem.afternoonabsent;

                                if (ioitem.morningabsent && DateTimeNow.Hour > 11)
                                    oEmployeeinout.morningabsent = ioitem.morningabsent;
                            }
                            else
                            {
                                oEmployeeinout.afternoonabsent = ioitem.afternoonabsent;
                                oEmployeeinout.morningabsent = ioitem.morningabsent;
                            }
                            oEmployeeinout.attendanceon = item.date;
                            oEmployeeinout.code = ioitem.code;
                            oEmployeeinout.createdby = -1;
                            oEmployeeinout.createdon = DateTimeNow;
                            oEmployeeinout.detail = new Employeeinout.Detail()
                            {
                                inoutlog = ioitem.detail
                            };
                            oEmployeeinout.employeeid = empcodeid[ioitem.code].employeeid;
                            oEmployeeinout.eveningovertime = ioitem.eveningovertime;
                            oEmployeeinout.intime = ioitem.intime;
                            oEmployeeinout.isactive = true;
                            oEmployeeinout.modifiedby = -1;
                            oEmployeeinout.modifiedon = DateTimeNow;
                            oEmployeeinout.morningovertime = ioitem.morningovertime;
                            oEmployeeinout.eveningovertime = ioitem.eveningovertime;
                            oEmployeeinout.outtime = ioitem.outtime;
                            oEmployeeinout.workinghours = ioitem.workinghours;
                            oEmployeeinout.latecheckin = ioitem.latecheckin;
                            oEmployeeinout.earlycheckout = ioitem.earlycheckout;
                            oEmployeeinout.nonworking = ioitem.nonworking;
                            oEmployeeinout.actualworking = ioitem.actualworking;
                            oEmployeeinout.valid = ioitem.valid;
                            if (empids.Contains(oEmployeeinout.employeeid))
                                continue;

                            oEmployeeinout.todayshiftid = empcodeid[ioitem.code].shiftid;
                            empids.Add(oEmployeeinout.employeeid);

                            if (empcodeid[ioitem.code].shiftid > 0 && workinghour.Exists(e => e.id == empcodeid[ioitem.code].shiftid))
                            {
                                var hourconfig = workinghour.Find(e => e.id == empcodeid[ioitem.code].shiftid).additionalinfo;
                                var fromtime = BaseEntity.ParseTime(hourconfig.fromtime);
                                var totime = BaseEntity.ParseTime(hourconfig.totime);
                                var fullday = BaseEntity.ParseTime(hourconfig.eligbleday);
                                var halfday = BaseEntity.ParseTime(hourconfig.present);
                                if (fromtime.TotalMinutes != 0 && totime.TotalMinutes != 0)
                                {
                                    DateTime start = oEmployeeinout.attendanceon.AddMinutes(oEmployeeinout.intime);
                                    DateTime end = oEmployeeinout.attendanceon.AddMinutes(oEmployeeinout.outtime);
                                    oEmployeeinout.morningovertime = 0;
                                    oEmployeeinout.eveningovertime = 0;
                                    oEmployeeinout.latecheckin = 0;
                                    oEmployeeinout.earlycheckout = 0;
                                    if (fullday.TotalMinutes > 0 && halfday.TotalMinutes > 0)
                                    {
                                        oEmployeeinout.morningabsent = false;
                                        oEmployeeinout.afternoonabsent = false;
                                        if (end.Subtract(start).TotalMinutes < fullday.TotalMinutes)
                                        {
                                            if (halfday.TotalMinutes < end.Subtract(start).TotalMinutes)
                                            {
                                                var startDiff = Math.Abs(start.TimeOfDay.TotalMinutes - fromtime.TotalMinutes);
                                                var endDiff = Math.Abs(end.TimeOfDay.TotalMinutes - totime.TotalMinutes);
                                                if (startDiff < endDiff)
                                                {
                                                    oEmployeeinout.afternoonabsent = true;
                                                }
                                                else
                                                {
                                                    oEmployeeinout.morningabsent = true;
                                                }
                                            }
                                            else
                                            {
                                                oEmployeeinout.morningabsent = true;
                                                oEmployeeinout.afternoonabsent = true;
                                            }
                                        }
                                    }

                                    if (fromtime > start.TimeOfDay)
                                        oEmployeeinout.morningovertime = (int)(fromtime - start.TimeOfDay).TotalMinutes;
                                    else if (!oEmployeeinout.morningabsent)
                                        oEmployeeinout.latecheckin = (int)start.TimeOfDay.Subtract(fromtime).TotalMinutes;

                                    if (totime < end.TimeOfDay)
                                        oEmployeeinout.eveningovertime = (int)(end.TimeOfDay - totime).TotalMinutes;
                                    else if (!oEmployeeinout.afternoonabsent)
                                        oEmployeeinout.earlycheckout = (int)totime.Subtract(end.TimeOfDay).TotalMinutes;
                                }
                            }
                            //if (existingIO.Exists(x => x.employeeid == oEmployeeinout.employeeid))
                            //{
                            //    ioitem.id = existingIO.Find(x => x.employeeid == oEmployeeinout.employeeid).id;
                            //    Update(transaction, conn, oEmployeeinout);
                            //}
                            //else
                            //{
                                Insert(transaction, conn, oEmployeeinout, 0);
                                ioitem.id = oEmployeeinout.id;
                            //}
                            retvalue.Add(oEmployeeinout);
                        }

                        WebAPI.Common.Logger.Instance(Context.OrganizationID).PerformanceLog("PostDirect - Inserted", WebAPI.Common.Logger.LogType.Info,
                        "POSTDirect", item.date.Date.ToString("dd-MM-yyyy") + "-" + retvalue.Count, 0, 0);
                        RollDate(item.date, locationid, transaction, conn);
                        NonEntryAutoLeaves(item.date, locationid, transaction, conn);
                        transaction.Commit();
                        conn.Close();
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(("PostDirect - Completed"), WebAPI.Common.Logger.LogType.Info, "");
                    }
                }
                catch (Exception exp)
                {
                    if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    {
                        if (transaction != null)
                            transaction.Rollback();
                        conn.Close();
                    }
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, item);
                    if (exp.InnerException != null)
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp.InnerException, WebAPI.Common.Logger.LogType.Error, item);
                }
                return item.data;
            }
        }

        internal bool ShopOpeningChange(Models.Storetransaction storetransaction)
        {

            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_locakdate.Year > 2000 &&
               Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_locakdate > storetransaction.actiondate.Date)
            {
                return false;
            }

            lock (lockingPost)
            {
                NpgsqlConnection conn = null;
                NpgsqlTransaction transaction = null;

                try
                {


                    using (conn = CreateConnection())
                    {
                        conn.Open();
                        transaction = conn.BeginTransaction();


                        transaction.Commit();
                        conn.Close();
                    }
                }
                catch (Exception exp)
                {

                    if (conn != null && conn.State == System.Data.ConnectionState.Open)
                    {
                        if (transaction != null)
                            transaction.Rollback();
                        conn.Close();
                    }
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, storetransaction);
                    if (exp.InnerException != null)
                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp.InnerException, WebAPI.Common.Logger.LogType.Error, storetransaction);

                    return false;
                }
                return true;
            }
        }

        public bool StoreOpening(Models.Storetransaction storetransaction, NpgsqlConnection conn, NpgsqlTransaction transaction)
        {
            var existing = Search(new EmployeeinoutCreteria()
            {
                locationid = (int)storetransaction.locationid,
                fromdate = storetransaction.actiondate.Date,
                todate = storetransaction.actiondate.Date
            }, conn, transaction);

            InfyPOS.Data.ReferencelistDataService refList = new ReferencelistDataService() { Context = this.Context };
            var workinghour = refList.Search(new ReferencelistCreteria()
            {
                type = "WORKINGHOURS"
            });
            TimeSpan opening = TimeSpan.Zero;
            TimeSpan closing = TimeSpan.Zero;

            if (storetransaction.StoreOpeningAttribute != null)
            {
                opening = BaseEntity.ParseTime(storetransaction.StoreOpeningAttribute.opening);
                closing = BaseEntity.ParseTime(storetransaction.StoreOpeningAttribute.closing);
            }
            else
            {
                return true;
            }

            foreach (var oEmployeeinout in existing)
            {

                if (oEmployeeinout.todayshiftid > 0 && workinghour.Exists(e => e.id == oEmployeeinout.todayshiftid))
                {
                    var hourconfig = workinghour.Find(e => e.id == oEmployeeinout.todayshiftid).additionalinfo;
                    var fromtime = BaseEntity.ParseTime(hourconfig.fromtime);
                    var totime = BaseEntity.ParseTime(hourconfig.totime);
                    var fullday = BaseEntity.ParseTime(hourconfig.eligbleday);
                    var halfday = BaseEntity.ParseTime(hourconfig.present);
                    if (fromtime < opening || totime > closing)
                    {
                        if (fromtime < opening && opening != TimeSpan.Zero)
                            fromtime = opening;
                        if (totime > closing && closing != TimeSpan.Zero)
                            totime = closing;

                        if (fromtime.TotalMinutes != 0 && totime.TotalMinutes != 0)
                        {
                            DateTime start = oEmployeeinout.attendanceon.AddMinutes(oEmployeeinout.intime);
                            DateTime end = oEmployeeinout.attendanceon.AddMinutes(oEmployeeinout.outtime);
                            oEmployeeinout.morningovertime = 0;
                            oEmployeeinout.eveningovertime = 0;
                            oEmployeeinout.latecheckin = 0;
                            oEmployeeinout.earlycheckout = 0;

                            if (fullday.TotalMinutes > 0 && halfday.TotalMinutes > 0)
                            {
                                oEmployeeinout.morningabsent = false;
                                oEmployeeinout.afternoonabsent = false;
                                if (end.Subtract(start).TotalMinutes < fullday.TotalMinutes)
                                {
                                    if (halfday.TotalMinutes < end.Subtract(start).TotalMinutes)
                                    {
                                        var startDiff = Math.Abs(start.TimeOfDay.TotalMinutes - fromtime.TotalMinutes);
                                        var endDiff = Math.Abs(end.TimeOfDay.TotalMinutes - totime.TotalMinutes);
                                        if (startDiff < endDiff)
                                        {
                                            oEmployeeinout.afternoonabsent = true;
                                        }
                                        else
                                        {
                                            oEmployeeinout.morningabsent = true;
                                        }
                                    }
                                    else
                                    {
                                        oEmployeeinout.morningabsent = true;
                                        oEmployeeinout.afternoonabsent = true;
                                    }
                                }
                            }

                            if (fromtime > start.TimeOfDay)
                                oEmployeeinout.morningovertime = (int)(fromtime - start.TimeOfDay).TotalMinutes;
                            else if (!oEmployeeinout.morningabsent)
                                oEmployeeinout.latecheckin = (int)start.TimeOfDay.Subtract(fromtime).TotalMinutes;

                            if (totime < end.TimeOfDay)
                                oEmployeeinout.eveningovertime = (int)(end.TimeOfDay - totime).TotalMinutes;
                            else if (!oEmployeeinout.afternoonabsent)
                                oEmployeeinout.earlycheckout = (int)totime.Subtract(end.TimeOfDay).TotalMinutes;

                            Update(transaction, conn, oEmployeeinout);
                        }
                    }
                }
            }
            return true;
        }

        public List<Employeeinout> ResyncAbsents(EmployeeinoutCreteria creteria)
        {
            int modifiedcount = 0;
            var existing = Search(creteria);
            if (existing.Count > 0)
            {
                SortedDictionary<long, EmployeeCode> empcodeid = new SortedDictionary<long, EmployeeCode>();
                string query = string.Format("select id,defaultshiftid from employee where id in (" + string.Join(",", existing.ConvertAll(e => e.employeeid).Distinct())) + ")";
                using (var reader = ExecuteReader(query, new SortedList<string, object>()))
                {
                    while (reader.Read())
                    {
                        if (!empcodeid.ContainsKey(reader.GetInt64(0)))
                        {
                            var empcode = new EmployeeCode()
                            {
                                employeeid = reader.GetInt64(0),
                                shiftid = reader.IsDBNull(1) ? 0 : reader.GetInt64(1),
                            };
                            empcodeid.Add(reader.GetInt64(0), empcode);
                        }
                    }
                }
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    var transaction = conn.BeginTransaction();

                    InfyPOS.Data.ReferencelistDataService refList = new ReferencelistDataService() { Context = this.Context };
                    var workinghour = refList.Search(new ReferencelistCreteria()
                    {
                        type = "WORKINGHOURS"
                    });

                    foreach (var oEmployeeinout in existing)
                    {
                        if (oEmployeeinout.todayshiftid == 0 && empcodeid.ContainsKey(oEmployeeinout.employeeid))
                        {
                            oEmployeeinout.todayshiftid = empcodeid[oEmployeeinout.employeeid].shiftid;
                        }
                        if (oEmployeeinout.todayshiftid > 0 && workinghour.Exists(e => e.id == oEmployeeinout.todayshiftid))
                        {
                            var hourconfig = workinghour.Find(e => e.id == oEmployeeinout.todayshiftid).additionalinfo;
                            var fromtime = BaseEntity.ParseTime(hourconfig.fromtime);
                            var totime = BaseEntity.ParseTime(hourconfig.totime);
                            var fullday = BaseEntity.ParseTime(hourconfig.eligbleday);
                            var halfday = BaseEntity.ParseTime(hourconfig.present);

                            if (fromtime.TotalMinutes != 0 && totime.TotalMinutes != 0)
                            {
                                DateTime start = oEmployeeinout.attendanceon.AddMinutes(oEmployeeinout.intime);
                                DateTime end = oEmployeeinout.attendanceon.AddMinutes(oEmployeeinout.outtime);
                                oEmployeeinout.morningovertime = 0;
                                oEmployeeinout.eveningovertime = 0;
                                oEmployeeinout.latecheckin = 0;
                                oEmployeeinout.earlycheckout = 0;
                                oEmployeeinout.workinghours = (decimal)end.Subtract(start).TotalHours;

                                var afternoonabsent = oEmployeeinout.afternoonabsent;
                                var morningabsent = oEmployeeinout.morningabsent;
                                if (fullday.TotalMinutes > 0 && halfday.TotalMinutes > 0)
                                {
                                    oEmployeeinout.morningabsent = false;
                                    oEmployeeinout.afternoonabsent = false;
                                    if (end.Subtract(start).TotalMinutes < fullday.TotalMinutes)
                                    {
                                        if (halfday.TotalMinutes < end.Subtract(start).TotalMinutes)
                                        {
                                            var startDiff = Math.Abs(start.TimeOfDay.TotalMinutes - fromtime.TotalMinutes);
                                            var endDiff = Math.Abs(end.TimeOfDay.TotalMinutes - totime.TotalMinutes);
                                            if (startDiff < endDiff)
                                            {
                                                oEmployeeinout.afternoonabsent = true;
                                            }
                                            else
                                            {
                                                oEmployeeinout.morningabsent = true;
                                            }
                                        }
                                        else
                                        {
                                            oEmployeeinout.morningabsent = true;
                                            oEmployeeinout.afternoonabsent = true;
                                        }
                                    }
                                }

                                if (fromtime > start.TimeOfDay)
                                    oEmployeeinout.morningovertime = (int)(fromtime - start.TimeOfDay).TotalMinutes;
                                else if (!oEmployeeinout.morningabsent)
                                    oEmployeeinout.latecheckin = (int)start.TimeOfDay.Subtract(fromtime).TotalMinutes;

                                if (totime < end.TimeOfDay)
                                    oEmployeeinout.eveningovertime = (int)(end.TimeOfDay - totime).TotalMinutes;
                                else if (!oEmployeeinout.afternoonabsent)
                                    oEmployeeinout.earlycheckout = (int)totime.Subtract(end.TimeOfDay).TotalMinutes;

                                if (afternoonabsent != oEmployeeinout.afternoonabsent ||
                                    morningabsent != oEmployeeinout.morningabsent)
                                {
                                    modifiedcount++;
                                }
                                Update(transaction, conn, oEmployeeinout);
                            }

                        }
                    }
                    transaction.Commit();
                    conn.Close();
                }
            }
            return existing;
        }
        private void NonEntryAutoLeaves(DateTime date, long locationid, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {
            DateTime minDate = date.AddDays(-7);
            DateTime currentdate = date;
            try
            {

                while (minDate <= currentdate)
                {
                    currentdate = currentdate.AddDays(-1);
                    var param = new SortedList<string, object>();
                    param.Add("from", currentdate);
                    param.Add("locationid", locationid);
                    param.Add("to", currentdate.AddDays(1));
                    string query = "Select id from employeeinout where attendanceon>=@from and attendanceon<@to and isactive=true limit 1;";
                    if (locationid > 0)
                    {
                        query = "Select id from employeeinout where attendanceon>=@from and attendanceon<@to and isactive=true and (employeeid in (select id from employee where locationid=@locationid)) limit 1;";
                    }
                    using (var reader = ExecuteReader(query, param))
                    {
                        if (reader.Read() && Convert.ToInt32(reader[0]) > 0)
                            return;
                    }

                    RollDate(currentdate, locationid, transaction, conn);
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, currentdate);
            }
        }

        private void RollDate(DateTime itemdate, long locationid)
        {
            NpgsqlTransaction transaction = null;
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();
                transaction = conn.BeginTransaction();
                RollDate(itemdate, locationid, transaction, conn);
                transaction.Commit();
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null && conn.State == System.Data.ConnectionState.Open)
                {
                    if (transaction != null)
                        transaction.Rollback();
                    conn.Close();
                }
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, itemdate);
            }
        }
        private void RollDate(DateTime itemdate, long locationid, NpgsqlTransaction transaction, NpgsqlConnection conn)
        {

            EmployeeLeaveDataService empleavedata = new EmployeeLeaveDataService() { Context = this.Context };
            WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(("RollDate - Started for " + itemdate), WebAPI.Common.Logger.LogType.Info, "");
            int records = empleavedata.DeleteAutoGenerated(transaction, conn, locationid, itemdate.Date);
            WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(("RollDate - " + itemdate + " Deleted " + records), WebAPI.Common.Logger.LogType.Info, "");


            List<EmployeeLeave> retlist = new List<EmployeeLeave>();
            NpgsqlCommand command = GetCommand(SQL_Absenties, conn, transaction);
            if (locationid > 0)
                command = GetCommand(SQL_Absenties + " and employee.locationid=" + locationid, conn, transaction);
            command.Parameters.Add(new NpgsqlParameter()
            {
                NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Timestamp,
                ParameterName = "attendanceon",
                NpgsqlValue = itemdate
            });

            using (var reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    EmployeeLeave ea = new EmployeeLeave();
                    ea.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                    ea.fromdate = itemdate;
                    ea.todate = itemdate;
                    bool hasleave = false;
                    bool morningabsent = (reader["morningabsent"] == DBNull.Value ? false : Convert.ToBoolean(reader["morningabsent"]));
                    if (morningabsent)
                    {
                        ea.fromdatehalfday = EmployeeLeave.DayMode.Morning;
                        hasleave = true;
                        ea.days += (decimal)0.5;
                    }
                    var afternoonabsent = (reader["afternoonabsent"] == DBNull.Value ? false : Convert.ToBoolean(reader["afternoonabsent"]));
                    if (afternoonabsent)
                    {
                        ea.fromdatehalfday = EmployeeLeave.DayMode.Evening;
                        hasleave = true;
                        ea.days += (decimal)0.5;
                    }
                    if (afternoonabsent && morningabsent)
                    {
                        ea.fromdatehalfday = EmployeeLeave.DayMode.Both;
                        ea.todatehalfday = EmployeeLeave.DayMode.Both;
                    }
                    ea.id = reader["leaveid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["leaveid"]);
                    if (reader["inoutid"] == DBNull.Value || hasleave)
                    {
                        ea.deductsalary = true;
                        if (reader["inoutid"] == DBNull.Value)
                            ea.days = 1;
                        hasleave = true;
                    }
                    if (hasleave)
                    {
                        ea.autogenerated = true;
                        ea.version = 1;
                        ea.isactive = true;
                        retlist.Add(ea);
                    }
                }
            }

            WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(("RollDate - Absenties " + retlist.Count), WebAPI.Common.Logger.LogType.Info, "");

            if (retlist.Count > 0)
            {
                empleavedata.Context = new UserClaim()
                {
                    UserID = 0
                };

                string existingquery = "select employeeid,id from employeeleave where fromdate=@fromdate and employeeid in (" + string.Join(",", retlist.ConvertAll(x => x.employeeid)) + ")";
                command = GetCommand(existingquery, conn, transaction);
                command.Parameters.Add(new NpgsqlParameter()
                {
                    NpgsqlDbType = NpgsqlTypes.NpgsqlDbType.Timestamp,
                    ParameterName = "fromdate",
                    NpgsqlValue = itemdate
                });
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        retlist.Find(x => x.employeeid == reader.GetInt64(0)).id = reader.GetInt64(1);
                    }
                }

                foreach (var leave in retlist)
                {
                    if (leave.id == 0)
                        empleavedata.Insert(transaction, conn, leave, true);
                    else
                        empleavedata.Update(transaction, conn, leave);
                }
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(("RollDate - Completed " + retlist.Count), WebAPI.Common.Logger.LogType.Info, "");
            }

            StoretransactionDataService storetransaction = new StoretransactionDataService() { Context = this.Context };
            var storechanges = storetransaction.Search(new StoretransactionCreteria()
            {
                actiondate = itemdate.Date
            }, conn, transaction);

            foreach (var storechange in storechanges)
            {
                StoreOpening(storechange, conn, transaction);
            }
        }
        internal List<Employeeinout> SearchDirect(EmployeeinoutCreteriaDirect item)
        {
            throw new NotImplementedException();
        }

        public class EmployeeAbsent
        {
            public long employeeid { get; set; }
            public DateTime attendanceon { get; set; }
            public bool morningabsent { get; set; }
            public bool afternoonabsent { get; set; }
            public bool fulldayabsent { get; set; }
            public long leaveid { get; set; }
        }
        public bool RollDate(EmployeeinoutCreteriaDirect item)
        {
            long locationid = 0;
            if (!string.IsNullOrEmpty(item.location))
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService() { Context = this.Context };
                var locations = referencelistDataService.SearchList(new ReferencelistCreteria() { code = item.location });
                if (locations.Count == 1)
                {
                    locationid = locations[0].id;
                }
            }

            WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(("RollDate - " + item.machineid + " " + item.sourcekey + " " + item.date), WebAPI.Common.Logger.LogType.Error, "");
            RollDate(item.date, locationid);
            return true;
        }

    }

}
