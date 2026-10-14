using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
namespace InfyPOS.Data
{
    public class EmployeeDataService : SAASData
    {
        const string SQL_Search = @" with salaryheader as (
			                                    SELECT id, employeeid, fromdate, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			                                    FROM employeesalaryheader)

                                    select bloodgroup.name bloodgroup,caste.name caste,employee.personal->>'religion' as religion,employee.id, employee.name, employee.surname,employee.contactno, employee.emailid, employee.desingnationid, employee.companyid, employee.roleid, employee.isactive, 
                                    employee.createdby, employee.modifiedby, employee.version, employee.createdon, employee.modifiedon, employee.dateofjoining,employee.dateofbirth, employee.relievingdate,employee.resignationdate,employee.resignreasonid, 
                                    employee.allowsystemaccess, employee.username, employee.workingmode, employee.salarymode, employee.employeecode, employee.cardcode, 
                                    employee.gender, employee.personal, employee.family, employee.releavingreason, employee.pfandesi, employee.presentaddress, employee.permanentaddress, 
                                    employee.aadharno, employee.pancardno, employee.remarks, employee.locationid, employee.floorid, employee.departmentid, employee.sectionid, 
                                    employee.bankid, employee.classification, employee.addionalinfo, employee.passport, employee.trainings, employee.accident, employee.disciplinary, employee.title,employee.refempid,
                                    educationandexperiance, license, castecategory, qualification, nationality, language, images_photo, images_signature, images_thumb, bank,hasleft,
                                    company.logo,company.name,designation.name as designation,date_part('year',age(dateofbirth)), employee.managerid,department.name departmentname,
                                    employeesalaryheader.salarystructureid,employeesalaryheader.grosspay,haswithheld, withheldfrom ,lcmode,defaultshiftid,hasot,hasmorningot,hasbeta,incentive
                                    from employee 
                                    left outer join company on employee.companyid = company.id
                                    left outer join salaryheader on salaryheader.ranknumber = 1 and salaryheader.employeeid = employee.id
                                    left outer join employeesalaryheader on employeesalaryheader.id = salaryheader.id
                                    left outer join referencelist designation on designation.id = desingnationid
                                    left outer join referencelist department on department.id = employee.departmentid
                                    left outer join referencelist bloodgroup on bloodgroup.id = (case when employee.personal->>'bloodgroup' ~ '^[0-9]+$' then cast(employee.personal->>'bloodgroup' as bigint) else 0 end)
                                    left outer join referencelist caste on caste.id = cast(employee.personal->>'caste' as bigint)
                                    where employee.id=@id";

        const string SQL_Search_Hide_PayInfo = @"select bloodgroup.name bloodgroup,caste.name caste,employee.personal->>'religion' as religion,employee.id, employee.name, employee.surname,employee.contactno, employee.emailid, employee.desingnationid, employee.companyid, employee.roleid, employee.isactive, 
                                    employee.createdby, employee.modifiedby, employee.version, employee.createdon, employee.modifiedon, employee.dateofjoining,employee.dateofbirth, employee.relievingdate,employee.resignationdate,employee.resignreasonid, 
                                    employee.allowsystemaccess, employee.username, employee.workingmode, employee.salarymode, employee.employeecode, employee.cardcode, 
                                    employee.gender, employee.personal, employee.family, employee.releavingreason, employee.pfandesi, employee.presentaddress, employee.permanentaddress, 
                                    employee.aadharno, employee.pancardno, employee.remarks, employee.locationid, employee.floorid, employee.departmentid, employee.sectionid, 
                                    employee.bankid, employee.classification, employee.addionalinfo, employee.passport, employee.trainings, employee.accident, employee.disciplinary, employee.title,employee.refempid,
                                    educationandexperiance, license, castecategory, qualification, nationality, language, images_photo, images_signature, images_thumb, bank,hasleft,
                                    company.logo,company.name,designation.name as designation,date_part('year',age(dateofbirth)), employee.managerid,department.name departmentname,
                                    haswithheld, withheldfrom ,lcmode,defaultshiftid,hasot,hasmorningot,hasbeta,incentive
                                    from employee 
                                    left outer join company on employee.companyid = company.id
                                    left outer join referencelist designation on designation.id = desingnationid
                                    left outer join referencelist department on department.id = employee.departmentid
                                    left outer join referencelist bloodgroup on bloodgroup.id = (case when employee.personal->>'bloodgroup' ~ '^[0-9]+$' then cast(employee.personal->>'bloodgroup' as bigint) else 0 end)
                                    left outer join referencelist caste on caste.id = cast(employee.personal->>'caste' as bigint)
                                    where employee.id=@id";


        const string SQL_Insert = @"INSERT INTO public.employee(name, contactno, desingnationid, companyid, roleid, isactive, createdby, modifiedby, version, createdon, modifiedon, dateofjoining,dateofbirth, 
                                    relievingdate,resignationdate,resignreasonid, allowsystemaccess, username, workingmode, salarymode, employeecode, cardcode, gender, personal, family, releavingreason, pfandesi, presentaddress, 
                                    permanentaddress, aadharno, pancardno, remarks, locationid, floorid, departmentid, sectionid, bankid, classification, addionalinfo, passport, trainings, accident, 
                                    disciplinary, educationandexperiance, license, castecategory, qualification, nationality, language, images_photo, images_signature, images_thumb, bank,hasleft,
                                    title,refempid,haswithheld, withheldfrom ,lcmode,defaultshiftid,hasot,hasmorningot,hasbeta,surname,emailid,managerid,incentive) VALUES(@name, @contactno, @desingnationid, @companyid, @roleid, @isactive, @createdby, @modifiedby, @version,
@createdon, @modifiedon, @dateofjoining,@dateofbirth, @relievingdate, @resignationdate,@resignreasonid, @allowsystemaccess, @username, @workingmode, @salarymode, @employeecode, @cardcode, @gender, @personal, @family, @releavingreason, @pfandesi,
@presentaddress, @permanentaddress, @aadharno, @pancardno, @remarks, @locationid, @floorid, @departmentid, @sectionid, @bankid, @classification, @addionalinfo, @passport, @trainings, @accident,
@disciplinary, @educationandexperiance, @license, @castecategory, @qualification, @nationality, @language, @images_photo, @images_signature, @images_thumb,
@bank,@hasleft,@title,@refempid,@haswithheld, @withheldfrom ,@lcmode,@defaultshiftid,@hasot,@hasmorningot,@hasbeta,@surname,@emailid,@managerid,@incentive) RETURNING id";


        const string SQL_Update = @"UPDATE public.employee SET name=@name, contactno=@contactno, desingnationid=@desingnationid, companyid=@companyid,managerid=@managerid, roleid=@roleid, modifiedby=@modifiedby, 
            version=@version+1, modifiedon=@modifiedon, dateofjoining=@dateofjoining, dateofbirth=@dateofbirth, relievingdate=@relievingdate, resignationdate=@resignationdate,resignreasonid=@resignreasonid, allowsystemaccess=@allowsystemaccess, username=@username, workingmode=@workingmode, 
salarymode=@salarymode, employeecode=@employeecode, cardcode=@cardcode, gender=@gender, personal=@personal, family=@family, releavingreason=@releavingreason, pfandesi=@pfandesi, 
presentaddress=@presentaddress, permanentaddress=@permanentaddress, aadharno=@aadharno, pancardno=@pancardno, remarks=@remarks, locationid=@locationid, floorid=@floorid,
departmentid=@departmentid, sectionid=@sectionid, bankid=@bankid, classification=@classification, addionalinfo=@addionalinfo, passport=@passport, trainings=@trainings, accident=@accident, 
disciplinary=@disciplinary, educationandexperiance=@educationandexperiance, license=@license, castecategory=@castecategory, qualification=@qualification, nationality=@nationality, language=@language,
images_photo=@images_photo, images_signature=@images_signature, images_thumb=@images_thumb, bank=@bank,hasleft=@hasleft,title=@title,refempid=@refempid,incentive=@incentive,
haswithheld=@haswithheld, withheldfrom=@withheldfrom ,lcmode=@lcmode,defaultshiftid=@defaultshiftid,hasot=@hasot,hasmorningot=@hasmorningot,hasbeta=@hasbeta,surname=@surname,emailid=@emailid
where id=@id and version=@version RETURNING version";

        const string SQL_Delete = @"update public.employee set isactive=false,version=@version+1,modifiedby=@modifiedby,modifiedon=@modifiedon where id=@id and version=@version";

        public List<Employee> Search(EmployeeCreteria _employee)
        {
            List<Employee> lst = new List<Employee>();
            NpgsqlConnection conn = CreateConnection();

            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                if (_employee.id != 0)
                {
                    command = GetCommand(_employee.hidesalaryinfo ? SQL_Search_Hide_PayInfo : SQL_Search, conn);
                    command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = _employee.id;
                }
                else if (_employee.ids != null && _employee.ids.Count > 0)
                {
                    QueryBuilder qb = new QueryBuilder();
                    if (_employee.hidesalaryinfo)
                        qb.CurrentQuery = SQL_Search_Hide_PayInfo.Substring(0, SQL_Search_Hide_PayInfo.IndexOf("where"));
                    else
                        qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddInParamer("employee.id", string.Join(",", _employee.ids));
                    qb.AddParameter("employee.isactive", true, "=");
                    qb.SortField = @"case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";
                    command = qb.GetCommand(conn);
                }
                else if (_employee.codes != null && _employee.codes.Count > 0)
                {
                    QueryBuilder qb = new QueryBuilder();
                    if (_employee.hidesalaryinfo)
                        qb.CurrentQuery = SQL_Search_Hide_PayInfo.Substring(0, SQL_Search_Hide_PayInfo.IndexOf("where"));
                    else
                        qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddInParamer("employee.employeecode", string.Join(",", _employee.codes.ConvertAll(x=>"'"+x+"'")));
                    qb.AddParameter("employee.isactive", true, "=");
                    qb.SortField = @"case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";
                    command = qb.GetCommand(conn);
                }
                else
                {
                    QueryBuilder qb = new QueryBuilder();
                    if (_employee.hidesalaryinfo)
                        qb.CurrentQuery = SQL_Search_Hide_PayInfo.Substring(0, SQL_Search_Hide_PayInfo.IndexOf("where"));
                    else
                        qb.CurrentQuery = SQL_Search.Substring(0, SQL_Search.IndexOf("where"));
                    qb.AddParameter("employee.isactive", true, "=");

                    if (!string.IsNullOrEmpty(_employee.name))
                    {
                        qb.AddParameter("employee.name", "%" + _employee.name + "%", " ilike ");
                    }
                    if (!string.IsNullOrEmpty(_employee.contactno))
                    {
                        qb.AddParameter("employee.contactno", "%" + _employee.contactno + "%", " ilike ");
                    }
                    if (!string.IsNullOrEmpty(_employee.employeecode))
                    {
                        qb.AddParameter("employee.employeecode", _employee.employeecode, " = ");
                    }
                    if (_employee.companyid > 0)
                        qb.AddParameter("employee.companyid", _employee.companyid, "=");

                    if (_employee.desingnationid > 0)
                    {
                        qb.AddParameter("employee.desingnationid", _employee.desingnationid, " = ");
                    }
                    if (_employee.departmentid > 0)
                    {
                        qb.AddParameter("employee.departmentid", _employee.departmentid, " = ");
                    }
                    if (_employee.floorid > 0)
                    {
                        qb.AddParameter("employee.floorid", _employee.floorid, " = ");
                    }
                    if (_employee.managerid > 0)
                    {
                        qb.AddParameter("employee.managerid", _employee.managerid, " = ");
                    }
                    if (_employee.sectionid > 0)
                    {
                        qb.AddParameter("employee.sectionid", _employee.sectionid, " = ");
                    }
                    if (_employee.salarymode > 0)
                    {
                        qb.AddParameter("employee.salarymode", _employee.salarymode, " = ");
                    }
                    if (_employee.locationid > 0)
                    {
                        qb.AddParameter("employee.locationid", _employee.locationid, " = ");
                    }

                    if (_employee.workingmode > 0)
                    {
                        qb.AddParameter("employee.workingmode", _employee.workingmode, " = ");
                    }

                    if (_employee.dobfromdate != DateTime.MinValue && _employee.dobtodate != DateTime.MinValue)
                    {
                        qb.AddParameter("employee.dateofbirth", "dateofbirthfrom", _employee.dobfromdate.Date, ">=");
                        qb.AddParameter("employee.dateofbirth", "dateofbirthto", _employee.dobtodate.AddDays(1).Date, "<");
                    }

                    if (_employee.anniversaryfromdate != DateTime.MinValue && _employee.anniversarytodate != DateTime.MinValue)
                    {
                        qb.AddParameter("cast(Personal->>'marrieddate' as timestamp without time zone)", "marrieddatefrom", _employee.dobfromdate.Date, ">=");
                        qb.AddParameter("cast(Personal->>'marrieddate' as timestamp without time zone)", "marrieddateto", _employee.dobtodate.AddDays(1).Date, "<");
                    }

                    if (_employee.onlysalesmen)
                        qb.Add2Query(@"cast(department.additionalinfo->>'issales' as varchar) = 'true'");

                    if (_employee.HasLastModifiedSearch)
                    {
                        qb.AddParameter("modifiedon", _employee.modifiedon, ">");
                    }
                    else
                    {
                        if (_employee.currentEmployees)
                        {
                            qb.AddConditionalParameter("(employee.hasleft = false or employee.relievingdate>@relievingdate)", "relievingdate", DateTimeNow.AddDays(-45));
                        }
                        else
                        {
                            if (!_employee.includeleft)
                                qb.AddParameter("employee.hasleft", _employee.hasleft, " = ");
                        }
                    }


                    if (Context.accessiblelocation != null && !Context.accessiblelocation.Exists(x => x == -1))
                    {
                        qb.AddInParamer("employee.locationid", string.Join(",", Context.accessiblelocation));
                    }

                    qb.SortField = @"case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";
                    command = qb.GetCommand(conn);

                    //WebAPI.Common.Logger.Instance(Context.OrganizationID).Log("EMPLOYEE", WebAPI.Common.Logger.LogType.Info, command.CommandText);
                }


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        Employee oemployee = new Employee();
                        oemployee.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oemployee.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        oemployee.contactno = reader["contactno"] == DBNull.Value ? "" : reader["contactno"].ToString();
                        oemployee.emailid = reader["emailid"] == DBNull.Value ? "" : reader["emailid"].ToString();
                        oemployee.desingnationid = reader["desingnationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["desingnationid"]);
                        oemployee.bloodgroup = reader["bloodgroup"] == DBNull.Value ? "" : reader["bloodgroup"].ToString();
                        oemployee.caste = reader["caste"] == DBNull.Value ? "" : reader["caste"].ToString();
                        oemployee.religion = reader["religion"] == DBNull.Value ? "" : reader["religion"].ToString();
                        oemployee.companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        oemployee.roleid = reader["roleid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["roleid"]);
                        oemployee.isactive = reader["isactive"] == DBNull.Value ? false : Convert.ToBoolean(reader["isactive"]);
                        oemployee.createdby = reader["createdby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["createdby"]);
                        oemployee.modifiedby = reader["modifiedby"] == DBNull.Value ? 0 : Convert.ToInt64(reader["modifiedby"]);
                        oemployee.version = reader["version"] == DBNull.Value ? 0 : Convert.ToInt32(reader["version"]);
                        oemployee.resignreasonid = reader["resignreasonid"] == DBNull.Value ? 0 : Convert.ToInt32(reader["resignreasonid"]);

                        oemployee.createdon = reader["createdon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["createdon"]);
                        oemployee.modifiedon = reader["modifiedon"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["modifiedon"]);
                        oemployee.dateofjoining = reader["dateofjoining"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofjoining"]);
                        oemployee.dateofbirth = reader["dateofbirth"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofbirth"]);
                        oemployee.relievingdate = reader["relievingdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["relievingdate"]);
                        oemployee.resignationdate = reader["resignationdate"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["resignationdate"]);

                        oemployee.allowsystemaccess = reader["allowsystemaccess"] == DBNull.Value ? false : Convert.ToBoolean(reader["allowsystemaccess"]);
                        oemployee.username = reader["username"] == DBNull.Value ? "" : reader["username"].ToString();
                        oemployee.incentive = reader["incentive"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentive"]);
                        oemployee.designation = reader["designation"] == DBNull.Value ? "" : reader["designation"].ToString();
                        oemployee.department = reader["departmentname"] == DBNull.Value ? "" : reader["departmentname"].ToString();
                        oemployee.workingmode = reader["workingmode"] == DBNull.Value ? 0 : Convert.ToInt64(reader["workingmode"]);
                        oemployee.salarymode = reader["salarymode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["salarymode"]);
                        oemployee.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        oemployee.cardcode = reader["cardcode"] == DBNull.Value ? "" : reader["cardcode"].ToString();
                        oemployee.gender = reader["gender"] == DBNull.Value ? "" : reader["gender"].ToString();
                        oemployee.personal_json = reader["personal"] == DBNull.Value ? "null" : reader["personal"].ToString();
                        oemployee.family_json = reader["family"] == DBNull.Value ? "null" : reader["family"].ToString();
                        oemployee.releavingreason = reader["releavingreason"] == DBNull.Value ? 0 : Convert.ToInt64(reader["releavingreason"]);
                        oemployee.pfandesi_json = reader["pfandesi"] == DBNull.Value ? "null" : reader["pfandesi"].ToString();
                        oemployee.pfno = (oemployee.pfandesi == null) ? "" : oemployee.pfandesi.pfno;
                        oemployee.esino = (oemployee.pfandesi == null) ? "" : oemployee.pfandesi.esino;
                        oemployee.pfuan = (oemployee.pfandesi == null) ? "" : oemployee.pfandesi.pfuan;
                        oemployee.presentaddress_json = reader["presentaddress"] == DBNull.Value ? "null" : reader["presentaddress"].ToString();
                        oemployee.permanentaddress_json = reader["permanentaddress"] == DBNull.Value ? "null" : reader["permanentaddress"].ToString();
                        oemployee.aadharno = reader["aadharno"] == DBNull.Value ? "" : reader["aadharno"].ToString();
                        oemployee.pancardno = reader["pancardno"] == DBNull.Value ? "" : reader["pancardno"].ToString();
                        oemployee.remarks_json = reader["remarks"] == DBNull.Value ? "null" : reader["remarks"].ToString();
                        oemployee.locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        oemployee.defaultshiftid = reader["defaultshiftid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["defaultshiftid"]); ;

                        if (!_employee.hidesalaryinfo)
                        {
                            oemployee.salarystructureid = reader["salarystructureid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salarystructureid"]);
                            oemployee.grosspay = reader["grosspay"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grosspay"]);
                            oemployee.osalarystructureid = reader["salarystructureid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salarystructureid"]);
                            oemployee.ogrosspay = reader["grosspay"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grosspay"]);
                        }

                        oemployee.managerid = reader["managerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["managerid"]);
                        oemployee.floorid = reader["floorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["floorid"]);
                        oemployee.departmentid = reader["departmentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["departmentid"]);
                        oemployee.sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                        oemployee.bankid = reader["bankid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["bankid"]);
                        oemployee.classification_json = reader["classification"] == DBNull.Value ? "null" : reader["classification"].ToString();
                        oemployee.addionalinfo_json = reader["addionalinfo"] == DBNull.Value ? "null" : reader["addionalinfo"].ToString();
                        oemployee.passport_json = reader["passport"] == DBNull.Value ? "null" : reader["passport"].ToString();
                        oemployee.trainings_json = reader["trainings"] == DBNull.Value ? "null" : reader["trainings"].ToString();
                        oemployee.accident_json = reader["accident"] == DBNull.Value ? "null" : reader["accident"].ToString();
                        oemployee.disciplinary_json = reader["disciplinary"] == DBNull.Value ? "null" : reader["disciplinary"].ToString();
                        oemployee.educationandexperiance_json = reader["educationandexperiance"] == DBNull.Value ? "null" : reader["educationandexperiance"].ToString();
                        oemployee.license_json = reader["license"] == DBNull.Value ? "null" : reader["license"].ToString();
                        oemployee.castecategory = reader["castecategory"] == DBNull.Value ? 0 : Convert.ToInt64(reader["castecategory"]);
                        oemployee.qualification = reader["qualification"] == DBNull.Value ? 0 : Convert.ToInt64(reader["qualification"]);
                        oemployee.nationality = reader["nationality"] == DBNull.Value ? 0 : Convert.ToInt64(reader["nationality"]);
                        oemployee.language = reader["language"] == DBNull.Value ? 0 : Convert.ToInt64(reader["language"]);
                        oemployee.images_photo = reader["images_photo"] == DBNull.Value ? new byte[0] : (byte[])reader["images_photo"];
                        oemployee.images_signature = reader["images_signature"] == DBNull.Value ? new byte[0] : (byte[])reader["images_signature"];
                        oemployee.images_thumb = reader["images_thumb"] == DBNull.Value ? new byte[0] : (byte[])reader["images_thumb"];
                        oemployee.bank_json = reader["bank"] == DBNull.Value ? "null" : reader["bank"].ToString();
                        oemployee.hasleft = reader["hasleft"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasleft"]);

                        oemployee.lcmode = reader["lcmode"] == DBNull.Value ? 0 : Convert.ToInt32(reader["lcmode"]);
                        oemployee.haswithheld = reader["haswithheld"] == DBNull.Value ? false : Convert.ToBoolean(reader["haswithheld"]);
                        oemployee.withheldfrom = reader["withheldfrom"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["withheldfrom"]);

                        oemployee.hasbeta = reader["hasbeta"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasbeta"]);
                        oemployee.hasot = reader["hasot"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasot"]);
                        oemployee.hasmorningot = reader["hasmorningot"] == DBNull.Value ? false : Convert.ToBoolean(reader["hasmorningot"]);
                        oemployee.title = reader["title"] == DBNull.Value ? 0 : Convert.ToInt64(reader["title"]);

                        oemployee.refempid = reader["refempid"] == DBNull.Value ? "" : reader["refempid"].ToString();

                        oemployee.surname = reader["surname"] == DBNull.Value ? "" : reader["surname"].ToString();

                        oemployee.companylogo = reader["logo"] == DBNull.Value ? "" : reader["logo"].ToString();
                        oemployee.date_part = reader["date_part"] == DBNull.Value ? "" : reader["date_part"].ToString();


                        if (_employee.CurrentAsOnDate.Year > 2000 && oemployee.hasleft)
                        {
                            if (oemployee.relievingdate < _employee.CurrentAsOnDate)
                                continue;
                        }
                        lst.Add(oemployee);
                    }
                }
                conn.Close();

                if (_employee.forapplication)
                {
                    CompanyDataService companydataservice = new CompanyDataService() { Context = this.Context };
                    var cmplist = companydataservice.Search(new CompanyCreteria()
                    {
                        id = lst[0].companyid
                    });
                    if (cmplist.Count == 0)
                        lst[0].company = cmplist[0];
                }
                if (lst.Count == 1 || _employee.showcontact)
                {
                    try
                    {
                        ReferencelistDataService refdata = new ReferencelistDataService() { Context = this.Context };
                        List<CustomReferenceItem> lstref = refdata.Search(new CustomReferenceItem()
                        {
                            type = "DESIGNATION,DEPARTMENT,EDUCATIONTYPE,DIVISION,BANK,FLOOR,CASTE,COMMUNITY,BLOODGROUP,STATE,CITY,DISTRICT"
                        });

                        foreach (Employee emp in lst)
                        {
                            if (emp.personal != null && lstref.Exists(e => !string.IsNullOrEmpty(emp.personal.community) && e.id == Convert.ToInt64(emp.personal.caste)))
                            {
                                emp.personal.castename = lstref.Find(e => !string.IsNullOrEmpty(emp.personal.community) && e.id == Convert.ToInt64(emp.personal.caste)).name;
                            }
                            if (emp.personal != null && lstref.Exists(e => !string.IsNullOrEmpty(emp.personal.community) && e.id == Convert.ToInt64(emp.personal.community)))
                            {
                                emp.personal.communityname = lstref.Find(e => !string.IsNullOrEmpty(emp.personal.community) && e.id == Convert.ToInt64(emp.personal.community)).name;
                            }
                            if (emp.personal != null && lstref.Exists(e => !string.IsNullOrEmpty(emp.personal.community) && e.id == Convert.ToInt64(emp.personal.bloodgroup)))
                            {
                                emp.personal.bloodgroupname = lstref.Find(e => !string.IsNullOrEmpty(emp.personal.community) && e.id == Convert.ToInt64(emp.personal.bloodgroup)).name;
                            }
                            if (emp.presentaddress != null && lstref.Exists(e => !string.IsNullOrEmpty(emp.presentaddress.city) && e.id == Convert.ToInt64(emp.presentaddress.city)))
                            {
                                emp.presentaddress.cityname = lstref.Find(e => !string.IsNullOrEmpty(emp.presentaddress.city) && e.id == Convert.ToInt64(emp.presentaddress.city)).name;
                            }
                            if (emp.presentaddress != null && lstref.Exists(e => !string.IsNullOrEmpty(emp.presentaddress.district) && e.id == Convert.ToInt64(emp.presentaddress.district)))
                            {
                                emp.presentaddress.districtname = lstref.Find(e => !string.IsNullOrEmpty(emp.presentaddress.district) && e.id == Convert.ToInt64(emp.presentaddress.district)).name;
                            }
                            if (emp.presentaddress != null && lstref.Exists(e => !string.IsNullOrEmpty(emp.presentaddress.state) && e.id == Convert.ToInt64(emp.presentaddress.state)))
                            {
                                emp.presentaddress.statename = lstref.Find(e => !string.IsNullOrEmpty(emp.presentaddress.state) && e.id == Convert.ToInt64(emp.presentaddress.state)).name;
                            }
                            if (emp.permanentaddress != null && lstref.Exists(e => !string.IsNullOrEmpty(emp.permanentaddress.city) && e.id == Convert.ToInt64(emp.permanentaddress.city)))
                            {
                                emp.permanentaddress.cityname = lstref.Find(e => !string.IsNullOrEmpty(emp.permanentaddress.city) && e.id == Convert.ToInt64(emp.permanentaddress.city)).name;
                            }
                            if (emp.permanentaddress != null && lstref.Exists(e => !string.IsNullOrEmpty(emp.permanentaddress.district) && e.id == Convert.ToInt64(emp.permanentaddress.district)))
                            {
                                emp.permanentaddress.districtname = lstref.Find(e => !string.IsNullOrEmpty(emp.permanentaddress.district) && e.id == Convert.ToInt64(emp.permanentaddress.district)).name;
                            }
                            if (emp.permanentaddress != null && lstref.Exists(e => !string.IsNullOrEmpty(emp.permanentaddress.state) && e.id == Convert.ToInt64(emp.permanentaddress.state)))
                            {
                                emp.permanentaddress.statename = lstref.Find(e => !string.IsNullOrEmpty(emp.permanentaddress.state) && e.id == Convert.ToInt64(emp.permanentaddress.state)).name;
                            }
                        }
                    }
                    catch (Exception exp)
                    {

                    }
                }
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _employee);
            }
            return lst;
        }

        public List<EmployeeHeader> SearchHeader(EmployeeCreteria _employee)
        {
            string sql = @"select employee.id,employeecode,employee.name,sectionid from employee where isactive=true ";

            List<EmployeeHeader> lst = new List<EmployeeHeader>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                if (_employee.onlysalesmen)
                    sql = @"select employee.id,employeecode,employee.name,sectionid from employee
                            inner join referencelist on referencelist.id = employee.departmentid
                            where cast(referencelist.additionalinfo->>'issales' as varchar) = 'true' ";

                qb.CurrentQuery = sql;
                if (_employee.id > 0)
                    qb.AddParameter("employee.id", _employee.id, " = ");

                if (_employee.standardroleid > 0)
                    qb.Add2Query(" desingnationid in (select id from referencelist where type = 'DESIGNATION' and referenceid=" + _employee.standardroleid + ")");

                if (!string.IsNullOrEmpty(_employee.employeecode))
                {
                    qb.AddParameter("employee.employeecode", _employee.employeecode, " = ");
                }

                if (_employee.currentEmployees)
                {
                    qb.AddConditionalParameter("(employee.hasleft = false or employee.relievingdate>@relievingdate)", "relievingdate", DateTimeNow.AddDays(-45));
                }
                else
                {
                    if (!_employee.includeleft)
                        qb.AddParameter("employee.hasleft", _employee.hasleft, " = ");
                }
                qb.SortField = @"case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";
                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        EmployeeHeader emp = new EmployeeHeader();
                        emp.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        emp.sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                        emp.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        emp.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        lst.Add(emp);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _employee);
            }
            return lst;

        }
        public List<UserEmployeeLink> GetUserName(List<long> ids)
        {
            string sql = @"select users.id, employee.name,users.employeeid from users left outer join employee on employee.id = users.employeeid";

            List<UserEmployeeLink> lst = new List<UserEmployeeLink>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;
                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = sql;
                if (ids != null && ids.Count > 0)
                    qb.AddInParamer("users.id", string.Join(",", ids));

                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        UserEmployeeLink emp = new UserEmployeeLink();
                        // User ID
                        emp.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        emp.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();
                        emp.employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        lst.Add(emp);
                    }
                }

                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, ids);
            }
            return lst;

        }
        private int DuplicateValidation(Employee oEmployee)
        {
            if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.employeeno_validation) return 0;
            if (string.IsNullOrEmpty(oEmployee.employeecode)) return 0;

            string _Query = @"select count(id) items from employee where employeecode=@employeecode and id != @id and isactive=true";
            int count = 0;
            using (NpgsqlConnection conn = CreateConnection())
            {
                conn.Open();
                NpgsqlCommand command = GetCommand(_Query, conn);
                command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.id;
                command.Parameters.Add("employeecode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.employeecode;
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        count = reader["items"] == DBNull.Value ? 0 : Convert.ToInt32(reader["items"]);
                    }
                }
                conn.Close();
            }
            return count;
        }

        public Employee Insert(Employee oEmployee)
        {

            NpgsqlTransaction transaction = null;
            try
            {
                var sequence = SequenceManager.Instance(Context.OrganizationID).Get("employee.employeecode", 0);
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
                    if (string.IsNullOrEmpty(oEmployee.employeecode))
                        oEmployee.employeecode = sequence.GetNextNumber(Context, conn, transaction);

                    if (DuplicateValidation(oEmployee) > 0)
                    {
                        throw new Exception("Employee Code/ID already exists. please check.");
                    }

                    Insert(transaction, conn, oEmployee);
                    transaction.Commit();
                    conn.Close();
                    if (oEmployee.id > 0)
                    {
                        Processors.Schedule_Actions.Instance.OnChange(Context.OrganizationID, oEmployee, Processors.Schedule_Actions.Actions.New);
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployee);
            }

            return oEmployee;
        }

        public Employee Insert(NpgsqlTransaction transaction, NpgsqlConnection conn, Employee oEmployee)
        {

            NpgsqlCommand command = GetCommand(SQL_Insert, conn, transaction);
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.name == null ? "" : oEmployee.name;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.contactno == null ? "" : oEmployee.contactno;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.emailid == null ? "" : oEmployee.emailid;
            command.Parameters.Add("desingnationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.desingnationid;
            command.Parameters.Add("defaultshiftid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.defaultshiftid;
            command.Parameters.Add("managerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.managerid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.companyid;
            command.Parameters.Add("roleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.roleid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployee.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployee.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("dateofjoining", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployee.dateofjoining;
            command.Parameters.Add("dateofbirth", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployee.dateofbirth;
            command.Parameters.Add("resignreasonid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployee.resignreasonid;
            command.Parameters.Add("relievingdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployee.relievingdate;
            command.Parameters.Add("resignationdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployee.resignationdate;
            command.Parameters.Add("allowsystemaccess", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployee.allowsystemaccess;
            command.Parameters.Add("username", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.username == null ? "" : oEmployee.username;
            command.Parameters.Add("workingmode", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.workingmode;
            command.Parameters.Add("incentive", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployee.incentive;
            command.Parameters.Add("salarymode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployee.salarymode;
            command.Parameters.Add("employeecode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.employeecode == null ? "" : oEmployee.employeecode;
            command.Parameters.Add("cardcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.cardcode == null ? "" : oEmployee.cardcode;
            command.Parameters.Add("gender", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.gender == null ? "" : oEmployee.gender;
            command.Parameters.Add("personal", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.personal_json == null ? "null" : oEmployee.personal_json;
            command.Parameters.Add("family", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.family_json == null ? "null" : oEmployee.family_json;
            command.Parameters.Add("releavingreason", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.releavingreason;
            command.Parameters.Add("pfandesi", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.pfandesi_json == null ? "null" : oEmployee.pfandesi_json;
            command.Parameters.Add("presentaddress", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.presentaddress_json == null ? "null" : oEmployee.presentaddress_json;
            command.Parameters.Add("permanentaddress", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.permanentaddress_json == null ? "null" : oEmployee.permanentaddress_json;
            command.Parameters.Add("aadharno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.aadharno == null ? "" : oEmployee.aadharno;
            command.Parameters.Add("pancardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.pancardno == null ? "" : oEmployee.pancardno;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.remarks_json == null ? "null" : oEmployee.remarks_json;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.locationid;
            command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.floorid;
            command.Parameters.Add("departmentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.departmentid;
            command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.sectionid;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.bank != null ? oEmployee.bank.bankid : 0;
            command.Parameters.Add("classification", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.classification_json == null ? "null" : oEmployee.classification_json;
            command.Parameters.Add("addionalinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.addionalinfo_json == null ? "null" : oEmployee.addionalinfo_json;
            command.Parameters.Add("passport", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.passport_json == null ? "null" : oEmployee.passport_json;
            command.Parameters.Add("trainings", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.trainings_json == null ? "null" : oEmployee.trainings_json;
            command.Parameters.Add("accident", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.accident_json == null ? "null" : oEmployee.accident_json;
            command.Parameters.Add("disciplinary", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.disciplinary_json == null ? "null" : oEmployee.disciplinary_json;
            command.Parameters.Add("educationandexperiance", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.educationandexperiance_json == null ? "null" : oEmployee.educationandexperiance_json;
            command.Parameters.Add("license", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.license_json == null ? "null" : oEmployee.license_json;
            command.Parameters.Add("castecategory", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.castecategory;
            command.Parameters.Add("qualification", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.qualification;
            command.Parameters.Add("nationality", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.nationality;
            command.Parameters.Add("language", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.language;
            command.Parameters.Add("hasleft", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployee.hasleft;
            command.Parameters.Add("title", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.title;
            command.Parameters.Add("refempid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.refempid == null ? "" : oEmployee.refempid;

            command.Parameters.Add("hasot", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployee.hasot;
            command.Parameters.Add("hasmorningot", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployee.hasmorningot;
            command.Parameters.Add("hasbeta", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployee.hasbeta;

            command.Parameters.Add("haswithheld", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployee.haswithheld;
            command.Parameters.Add("lcmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployee.lcmode;
            command.Parameters.Add("withheldfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployee.withheldfrom;
            command.Parameters.Add("surname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.surname == null ? "" : oEmployee.surname;


            if (oEmployee.images_photo == null)
            {
                command.Parameters.Add("images_photo", NpgsqlTypes.NpgsqlDbType.Bytea).Value = DBNull.Value;
            }
            else
            {
                command.Parameters.Add("images_photo", NpgsqlTypes.NpgsqlDbType.Bytea).Value = oEmployee.images_photo;
            }

            if (oEmployee.images_signature == null)
            {
                command.Parameters.Add("images_signature", NpgsqlTypes.NpgsqlDbType.Bytea).Value = DBNull.Value;
            }
            else
            {
                command.Parameters.Add("images_signature", NpgsqlTypes.NpgsqlDbType.Bytea).Value = oEmployee.images_signature;
            }

            if (oEmployee.images_thumb == null)
            {
                command.Parameters.Add("images_thumb", NpgsqlTypes.NpgsqlDbType.Bytea).Value = DBNull.Value;
            }
            else
            {
                command.Parameters.Add("images_thumb", NpgsqlTypes.NpgsqlDbType.Bytea).Value = oEmployee.images_thumb;
            }

            command.Parameters.Add("bank", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.bank_json == null ? "null" : oEmployee.bank_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmployee.id = Convert.ToInt64(reader[0].ToString());
                }
            }
            return oEmployee;
        }

        public Employee Update(Employee oEmployee)
        {

            if (DuplicateValidation(oEmployee) > 0)
            {
                throw new Exception("Employee Code/ID already exists. please check.");
            }

            NpgsqlTransaction transaction = null;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();

                    Update(transaction, conn, oEmployee);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployee);


            }

            return oEmployee;
        }

        public Employee Update(NpgsqlTransaction transaction, NpgsqlConnection conn, Employee oEmployee)
        {

            NpgsqlCommand command = GetCommand(SQL_Update, conn, transaction);
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.id;
            command.Parameters.Add("name", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.name == null ? "" : oEmployee.name;
            command.Parameters.Add("emailid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.emailid == null ? "" : oEmployee.emailid;
            command.Parameters.Add("contactno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.contactno == null ? "" : oEmployee.contactno;
            command.Parameters.Add("desingnationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.desingnationid;
            command.Parameters.Add("companyid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.companyid;
            command.Parameters.Add("managerid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.managerid;
            command.Parameters.Add("defaultshiftid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.defaultshiftid;
            command.Parameters.Add("roleid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.roleid;
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployee.isactive;
            command.Parameters.Add("createdby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.createdby;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployee.version;
            command.Parameters.Add("createdon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployee.createdon;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("dateofjoining", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployee.dateofjoining;
            command.Parameters.Add("dateofbirth", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployee.dateofbirth;
            command.Parameters.Add("relievingdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployee.relievingdate;
            command.Parameters.Add("resignationdate", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployee.resignationdate;
            command.Parameters.Add("resignreasonid", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployee.resignreasonid;
            command.Parameters.Add("allowsystemaccess", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployee.allowsystemaccess;
            command.Parameters.Add("username", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.username == null ? "" : oEmployee.username;
            command.Parameters.Add("workingmode", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.workingmode;
            command.Parameters.Add("incentive", NpgsqlTypes.NpgsqlDbType.Numeric).Value = oEmployee.incentive;
            command.Parameters.Add("salarymode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployee.salarymode;
            command.Parameters.Add("employeecode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.employeecode == null ? "" : oEmployee.employeecode;
            command.Parameters.Add("cardcode", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.cardcode == null ? "" : oEmployee.cardcode;
            command.Parameters.Add("gender", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.gender == null ? "" : oEmployee.gender;
            command.Parameters.Add("personal", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.personal_json == null ? "null" : oEmployee.personal_json;
            command.Parameters.Add("family", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.family_json == null ? "null" : oEmployee.family_json;
            command.Parameters.Add("releavingreason", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.releavingreason;
            command.Parameters.Add("pfandesi", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.pfandesi_json == null ? "null" : oEmployee.pfandesi_json;
            command.Parameters.Add("presentaddress", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.presentaddress_json == null ? "null" : oEmployee.presentaddress_json;
            command.Parameters.Add("permanentaddress", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.permanentaddress_json == null ? "null" : oEmployee.permanentaddress_json;
            command.Parameters.Add("aadharno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.aadharno == null ? "" : oEmployee.aadharno;
            command.Parameters.Add("pancardno", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.pancardno == null ? "" : oEmployee.pancardno;
            command.Parameters.Add("remarks", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.remarks_json == null ? "null" : oEmployee.remarks_json;
            command.Parameters.Add("locationid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.locationid;
            command.Parameters.Add("floorid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.floorid;
            command.Parameters.Add("departmentid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.departmentid;
            command.Parameters.Add("sectionid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.sectionid;
            command.Parameters.Add("bankid", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.bank != null ? oEmployee.bank.bankid : 0;
            command.Parameters.Add("classification", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.classification_json == null ? "null" : oEmployee.classification_json;
            command.Parameters.Add("addionalinfo", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.addionalinfo_json == null ? "null" : oEmployee.addionalinfo_json;
            command.Parameters.Add("passport", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.passport_json == null ? "null" : oEmployee.passport_json;
            command.Parameters.Add("trainings", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.trainings_json == null ? "null" : oEmployee.trainings_json;
            command.Parameters.Add("accident", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.accident_json == null ? "null" : oEmployee.accident_json;
            command.Parameters.Add("disciplinary", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.disciplinary_json == null ? "null" : oEmployee.disciplinary_json;
            command.Parameters.Add("educationandexperiance", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.educationandexperiance_json == null ? "null" : oEmployee.educationandexperiance_json;
            command.Parameters.Add("license", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.license_json == null ? "null" : oEmployee.license_json;
            command.Parameters.Add("castecategory", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.castecategory;
            command.Parameters.Add("qualification", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.qualification;
            command.Parameters.Add("nationality", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.nationality;
            command.Parameters.Add("language", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.language;
            command.Parameters.Add("hasleft", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployee.hasleft;
            command.Parameters.Add("title", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.title;
            command.Parameters.Add("haswithheld", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployee.haswithheld;
            command.Parameters.Add("lcmode", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployee.lcmode;
            command.Parameters.Add("withheldfrom", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = oEmployee.withheldfrom;
            command.Parameters.Add("hasot", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployee.hasot;
            command.Parameters.Add("hasmorningot", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployee.hasmorningot;
            command.Parameters.Add("hasbeta", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployee.hasbeta;
            command.Parameters.Add("refempid", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.refempid == null ? "" : oEmployee.refempid;
            command.Parameters.Add("surname", NpgsqlTypes.NpgsqlDbType.Varchar).Value = oEmployee.surname == null ? "" : oEmployee.surname;

            if (oEmployee.images_photo == null)
            {
                command.Parameters.Add("images_photo", NpgsqlTypes.NpgsqlDbType.Bytea).Value = DBNull.Value;
            }
            else
            {
                command.Parameters.Add("images_photo", NpgsqlTypes.NpgsqlDbType.Bytea).Value = oEmployee.images_photo;
            }

            if (oEmployee.images_signature == null)
            {
                command.Parameters.Add("images_signature", NpgsqlTypes.NpgsqlDbType.Bytea).Value = DBNull.Value;
            }
            else
            {
                command.Parameters.Add("images_signature", NpgsqlTypes.NpgsqlDbType.Bytea).Value = oEmployee.images_signature;
            }

            if (oEmployee.images_thumb == null)
            {
                command.Parameters.Add("images_thumb", NpgsqlTypes.NpgsqlDbType.Bytea).Value = DBNull.Value;
            }
            else
            {
                command.Parameters.Add("images_thumb", NpgsqlTypes.NpgsqlDbType.Bytea).Value = oEmployee.images_thumb;
            }

            command.Parameters.Add("bank", NpgsqlTypes.NpgsqlDbType.Json).Value = oEmployee.bank_json == null ? "null" : oEmployee.bank_json;
            using (System.Data.IDataReader reader = command.ExecuteReader())
            {
                if (reader.Read())
                {
                    oEmployee.version = Convert.ToInt32(reader[0].ToString());
                }
            }

            return oEmployee;
        }


        public bool Delete(Employee oEmployee)
        {

            NpgsqlTransaction transaction = null;
            bool hasSuccessCount = false;
            try
            {
                using (NpgsqlConnection conn = CreateConnection())
                {
                    conn.Open();
                    transaction = conn.BeginTransaction();
                    hasSuccessCount = Delete(transaction, conn, oEmployee);
                    transaction.Commit();
                    conn.Close();
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, oEmployee);



            }
            return hasSuccessCount;

        }

        public bool Delete(NpgsqlTransaction transaction, NpgsqlConnection conn, Employee oEmployee)
        {

            NpgsqlCommand command = GetCommand(SQL_Delete, conn, transaction);
            command.Parameters.Add("isactive", NpgsqlTypes.NpgsqlDbType.Boolean).Value = oEmployee.isactive;
            command.Parameters.Add("modifiedby", NpgsqlTypes.NpgsqlDbType.Bigint).Value = Context.UserID;
            command.Parameters.Add("modifiedon", NpgsqlTypes.NpgsqlDbType.Timestamp).Value = DateTimeNow;
            command.Parameters.Add("version", NpgsqlTypes.NpgsqlDbType.Integer).Value = oEmployee.version;
            command.Parameters.Add("id", NpgsqlTypes.NpgsqlDbType.Bigint).Value = oEmployee.id;
            int successCount = command.ExecuteNonQuery();

            return successCount > 0;
        }

    }

}
