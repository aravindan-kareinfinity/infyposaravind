using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using InfyPOS.Common;
using Npgsql;
using ClosedXML.Excel;
using System.Data;
using System.Net.Http;
using InfyPOS.Controllers;
using System.ComponentModel;
using System.Reflection;
using Newtonsoft.Json;

namespace InfyPOS.Data
{
    public class ExcelExportDataService : SAASData
    {
        public class ExcelExportCreteria : BaseEntity
        {
            public long companyid { get; set; }
            public string file { get; set; }
            public int month { get; set; }
            public int year { get; set; }
            public string form { get; set; }
        }

        public FileContentInfo FORM(ExcelExportCreteria _creteria)
        {
            System.Net.Http.ByteArrayContent response = null;
            switch (_creteria.form)
            {
                case "FORMP":
                    response = FORMP(_creteria);
                    break;
                case "FORMQ":
                    response = FORMQ(_creteria);
                    break;
                case "FORMR":
                    response = FORMR(_creteria);
                    break;
                case "FORMS":
                    response = FORMS(_creteria);
                    break;
                case "FORMESI":
                    response = FORMESI(_creteria);
                    break;

            }

            var fci = new FileContentInfo(response);

            fci.FileName = _creteria.form + ".xlsx";
            fci.Extension = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

            return fci;
        }
        public System.Net.Http.ByteArrayContent FORMP(ExcelExportCreteria _creteria)
        {
            _creteria.file = System.Web.Hosting.HostingEnvironment.MapPath("~/Resource/" + _creteria.form + ".xlsx");
            string worksheetname = _creteria.form;
            if (string.IsNullOrEmpty(_creteria.file))
                throw new Exception("Invalid file name.");

            // Replace with data method
            List<FORMP> formdata = GetPFormData(_creteria);
            try
            {
                var imageData = new ImageData();
                //imageData.Context = new UserClaim(_creteria.companyid);
                
                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(_creteria.file), true))
                {
                    //Create a new DataTable.
                    DataTable dt = new DataTable();

                    using (var workBook = new ClosedXML.Excel.XLWorkbook(ms))
                    {
                        //Read the first Sheet from Excel file.
                        IXLWorksheet workSheet = workBook.Worksheet(worksheetname);
                        var lastColumn = workSheet.LastCellUsed().Address.ColumnNumber;
                        int lastRow = workSheet.LastCellUsed().Address.RowNumber;
                        if (lastColumn > 100) lastColumn = 100;
                        if (lastRow > 100) lastRow = 100;
                        //Loop through the Worksheet rows.
                        foreach (IXLRow row in workSheet.Rows())
                        {
                            if (row.RangeAddress.FirstAddress.RowNumber > lastRow) break;
                            if (row.FirstCell().Address.RowNumber == 12)
                            {
                                var lastcell = row.LastCellUsed();
                                if (lastcell == null)
                                {
                                    lastcell = row.Cell(20);
                                }
                                var rngData = workSheet.Range(row.FirstCell().Address, lastcell.Address);

                                //newrow = rngData;
                                List<string> colvalues = new List<string>();
                                foreach (var cell in row.Cells())
                                {
                                    if (cell.Value != null)
                                        colvalues.Add(cell.Value.ToString());
                                }

                                int rowcount = 12;
                                int sno = 0;
                                foreach (FORMP form in formdata)
                                {
                                    int i = 0;
                                    IXLRows newrow = workSheet.Row(rowcount++).InsertRowsBelow(1);

                                    foreach (var str in colvalues)
                                    {
                                        sno++;
                                        form.slno = sno.ToString();

                                        string newstring = "";
                                        if (str == "$SLNO$")
                                            newstring = form.slno;
                                        else if (str == "$EMPNAME$")
                                            newstring = form.name;
                                        else if (str == "$EMPFATHERNAME$")
                                            newstring = (form.fathername == null) ? "" : form.fathername.ToUpper();
                                        else if (str == "$EMPNO$")
                                            newstring = form.employeecode;
                                        else if (str == "$EMPDESIGNATION$")
                                            newstring = form.desingnation;
                                        else if (str == "$ADVPAYMENTDATE$")
                                            newstring = form.schedulemonth;
                                        else if (str == "$ADVAMOUNT$")
                                            newstring = form.amount;
                                        else if (str == "$ADVNOOFINSTL$")
                                            newstring = form.advrecoverynoofinstl;
                                        else if (str == "$ADVRECOVERYDATE$")
                                            newstring = form.advrecoverycomplndate;
                                        else if (str == "$DAMAGE$")
                                            newstring = form.damagecaused;
                                        else if (str == "$SHOWCASEDATE$")
                                            newstring = form.deddateofshowcause;
                                        else if (str == "$Deduction$")
                                            newstring = form.dedamount;
                                        else if (str == "$NOOFINSTALLMENTS$")
                                            newstring = form.dedrecoverynoofinstl;
                                        else if (str == "$DeductionDATE$")
                                            newstring = form.deddateofcompletion;
                                        else if (str == "$OMISSIONDATE$")
                                            newstring = form.fineact;
                                        else if (str == "$DATEOFSHOWCAUSE$")
                                            newstring = form.finedateofshowcause;
                                        else if (str == "$FINEAMOUNT$")
                                            newstring = form.fineamount;
                                        else if (str == "$FINECOMPLETEDDATE$")
                                            newstring = form.finedateofcompletion;
                                        else if (str == "$SIGNORTHUMBIMP$")
                                            newstring = form.signorthumb;
                                        else if (str == "$REMARKS$")
                                            newstring = form.remark;
                                        setValue(newrow, i++, newstring);
                                    }
                                }
                                workSheet.Row(12).Delete();
                            }
                            else
                            {
                                foreach (var cell in row.CellsUsed())
                                {
                                    if (cell.Address.ColumnNumber > lastColumn) break;
                                    if (!cell.HasFormula && cell.Value != null)
                                    {
                                        string val = cell.Value.ToString();
                                        val = val.Replace("$COMPANYNAME$", "PAKSHI K RAMANATHAM CHETTIAR SON");
                                        val = val.Replace("$DATE$", DateTime.Now.ToString("dd-MMM-yyyy"));
                                        cell.Value = val;
                                    }

                                }
                            }

                        }
                        using (System.IO.MemoryStream outms = new System.IO.MemoryStream())
                        {
                            workBook.SaveAs(outms);
                            return new ByteArrayContent(outms.ToArray());
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return null;
        }

        public System.Net.Http.ByteArrayContent FORMQ(ExcelExportCreteria _creteria)
        {

            _creteria.file = System.Web.Hosting.HostingEnvironment.MapPath("~/Resource/" + _creteria.form + ".xlsx");
            string worksheetname = _creteria.form;
            if (string.IsNullOrEmpty(_creteria.file))
                throw new Exception("Invalid file name.");

            // Replace with data method
            List<FORMQ> formdata = GetQFormData(_creteria);
            try
            {
                var imageData = new ImageData();
                //imageData.Context = new UserClaim(_creteria.companyid);

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(_creteria.file), true))
                {
                    //Create a new DataTable.
                    DataTable dt = new DataTable();

                    using (var workBook = new ClosedXML.Excel.XLWorkbook(ms))
                    {
                        //Read the first Sheet from Excel file.
                        IXLWorksheet workSheet = workBook.Worksheet(worksheetname);
                        var lastColumn = workSheet.LastCellUsed().Address.ColumnNumber;
                        int lastRow = workSheet.LastCellUsed().Address.RowNumber;
                        if (lastColumn > 100) lastColumn = 100;
                        if (lastRow > 100) lastRow = 100;
                        //Loop through the Worksheet rows.
                        foreach (IXLRow row in workSheet.Rows())
                        {
                            if (row.RangeAddress.FirstAddress.RowNumber > lastRow) break;
                            if (row.FirstCell().Address.RowNumber == 11)
                            {
                                var lastcell = row.LastCellUsed();
                                if (lastcell == null)
                                {
                                    lastcell = row.Cell(50);
                                }
                                var rngData = workSheet.Range(row.FirstCell().Address, lastcell.Address);
                                //newrow = rngData;
                                List<string> colvalues = new List<string>();
                                foreach (var cell in row.Cells())
                                {
                                    if (cell.Value != null)
                                        colvalues.Add(cell.Value.ToString());
                                }
                                int rowcount = 11;
                                int sno = 0;
                                foreach (FORMQ form in formdata)
                                {
                                    sno++;
                                    form.slno = sno.ToString();
                                    IXLRows newrow = workSheet.Row(rowcount++).InsertRowsBelow(1);
                                    int i = 0;
                                    foreach (var str in colvalues)
                                    {
                                        string newstring = "";
                                        //DateTime date;
                                        if (str == "$SLNO$")
                                            newstring = form.slno;
                                        else if (str == "$EMPNAME$")
                                            newstring = form.name;
                                        else if (str == "$DOJ$")
                                            newstring = form.dateofjoining.ToString("dd/MM/yyyy");
                                        else if (str == "$DOB$")
                                            newstring = form.dob.ToString("dd/MM/yyyy");
                                        else if (str == "$DESIGNATION$")
                                            newstring = form.desingnationid;
                                        else if (str == "$OPENINGSL$")
                                            newstring = form.balancesl;
                                        else if (str == "$OPENINGCL$")
                                            newstring = form.balancesl;
                                        else if (str == "$OPENINGPL$")
                                            newstring = form.balancesl;
                                        else if (str == "$AVAILEDSL$")
                                            newstring = form.availsl;
                                        else if (str == "$AVAILEDCL$")
                                            newstring = form.availcl;
                                        else if (str == "$AVAILEDPL$")
                                            newstring = form.availpl;
                                        else if (str == "$BALANCESL$")
                                            newstring = form.balancesl;
                                        else if (str == "$BALANCECL$")
                                            newstring = form.balancecl;
                                        else if (str == "$BALANCEPL$")
                                            newstring = form.balancepl;
                                        else if (str == "$OVERTIME$")
                                            newstring = form.overtime;
                                        else if (str == "$WORKINGHOURS$")
                                            newstring = form.workinghours;
                                        else if (str == "$MATERNITY$")
                                            newstring = form.maternityleaveavailed;
                                        else if (str.StartsWith("$") && str.EndsWith("$"))
                                        {
                                            var daystring = str.Substring(1, str.Length - 2);
                                            if (form.dayvalue != null && form.dayvalue.ContainsKey(daystring))
                                                newstring = form.dayvalue[daystring];
                                            else
                                            {
                                                Int32 day;
                                                Int32.TryParse(daystring, out day);
                                                DateTime salarydate = new DateTime(_creteria.year, _creteria.month, 1).AddMinutes(DateTime.Now.Subtract(DateTime.UtcNow).TotalMinutes);
                                                if (form.dateofjoining <= salarydate)
                                                    newstring = "8";

                                            }
                                        }
                                        setValue(newrow, i++, newstring);
                                    }
                                }
                                workSheet.Row(11).Delete();
                            }
                            else
                            {
                                foreach (var cell in row.CellsUsed())
                                {
                                    if (cell.Address.ColumnNumber > lastColumn) break;
                                    if (!cell.HasFormula && cell.Value != null)
                                    {
                                        string val = cell.Value.ToString();
                                        val = val.Replace("$COMPANYNAME$", "PAKSHI K RAMANATHAM CHETTIAR SON");
                                        val = val.Replace("$DATE$", DateTime.Now.ToString("dd-MMM-yyyy"));
                                        cell.Value = val;
                                    }

                                }
                            }

                        }
                        using (System.IO.MemoryStream outms = new System.IO.MemoryStream())
                        {
                            workBook.SaveAs(outms);
                            return new ByteArrayContent(outms.ToArray());
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return null;
        }

        public System.Net.Http.ByteArrayContent FORMR(ExcelExportCreteria _creteria)
        {
            _creteria.file = System.Web.Hosting.HostingEnvironment.MapPath("~/Resource/" + _creteria.form + ".xlsx");
            string worksheetname = _creteria.form; if (string.IsNullOrEmpty(_creteria.file))

                throw new Exception("Invalid file name.");

            // Replace with data method
            List<FORMR> formdata = GetRFormData(_creteria);
            try
            {
                var imageData = new ImageData();
                //imageData.Context = new UserClaim(_creteria.companyid);

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(_creteria.file), true))
                {
                    //Create a new DataTable.
                    DataTable dt = new DataTable();

                    using (var workBook = new ClosedXML.Excel.XLWorkbook(ms))
                    {
                        //Read the first Sheet from Excel file.
                        IXLWorksheet workSheet = workBook.Worksheet(worksheetname);
                        var lastColumn = workSheet.LastCellUsed().Address.ColumnNumber;
                        int lastRow = workSheet.LastCellUsed().Address.RowNumber;
                        if (lastColumn > 100) lastColumn = 100;
                        if (lastRow > 100) lastRow = 100;
                        //Loop through the Worksheet rows.
                        foreach (IXLRow row in workSheet.Rows())
                        {
                            if (row.RangeAddress.FirstAddress.RowNumber > lastRow) break;
                            if (row.FirstCell().Address.RowNumber == 10)
                            {
                                var lastcell = row.LastCellUsed();
                                if (lastcell == null)
                                {
                                    lastcell = row.Cell(24);
                                }
                                var rngData = workSheet.Range(row.FirstCell().Address, lastcell.Address);
                                //newrow = rngData;
                                List<string> colvalues = new List<string>();
                                foreach (var cell in row.Cells())
                                {
                                    if (cell.Value != null)
                                        colvalues.Add(cell.Value.ToString());
                                }
                                int rowcount = 10;
                                int sno = 0;
                                foreach (FORMR form in formdata)
                                {
                                    sno++;
                                    form.slno = sno.ToString();
                                    IXLRows newrow = workSheet.Row(rowcount++).InsertRowsBelow(1);
                                    int i = 0;
                                    foreach (var str in colvalues)
                                    {
                                        string newstring = "";
                                        if (str == "$SLNO$")
                                            newstring = form.slno;
                                        else if (str == "$EMPNAME$")
                                            newstring = form.name;
                                        else if (str == "$GENDER$")
                                            newstring = form.gender;
                                        else if (str == "$DESIGNATION$")
                                            newstring = form.desingnationid;
                                        else if (str == "$MONTHLYRATED$")
                                            newstring = form.monthly_rated;
                                        else if (str == "$WAGESPERIOD$")
                                            newstring = form.salarymonth;
                                        else if (str == "$WORKINGDAYS$")
                                            newstring = form.workingdays;
                                        else if (str == "$UNITWORKINGDAYS$")
                                            newstring = form.leavedays;
                                        else if (str == "$NETSALARY$")
                                            newstring = form.netsalary;
                                        else if (str == "$OTRATE$")
                                            newstring = form.ot_rate;
                                        else if (str == "$BASICPAY$")
                                            newstring = form.basicpay;
                                        else if (str == "$DEARNESSALLOWANCE$")
                                            newstring = form.dearness_allowance;
                                        else if (str == "$OTHERALLOWANCE$")
                                            newstring = form.other_allowance;
                                        else if (str == "$OTEARNED$")
                                            newstring = form.ot_earned;
                                        else if (str == "$LEAVECASH$")
                                            newstring = form.leavewages;
                                        else if (str == "$GROSSAMOUNT$")
                                            newstring = form.grosspay;
                                        else if (str == "$PF$")
                                            newstring = form.pf;
                                        else if (str == "$ESI$")
                                            newstring = form.esi;
                                        else if (str == "$OTHERDeduction$")
                                            newstring = form.otherdeduction;
                                        else if (str == "$FINES$")
                                            newstring = form.fines;
                                        else if (str == "$NETSALARY$")
                                            newstring = form.netsalary;
                                        else if (str == "$SIGNORTHUMBIMP$")
                                            newstring = form.sign_with_thumbdetails;
                                        else if (str == "$UNPAIDAMOUNTS$")
                                            newstring = form.totalunpaidamt;
                                        setValue(newrow, i++, newstring);
                                    }
                                }
                                workSheet.Row(10).Delete();
                            }
                            else
                            {
                                foreach (var cell in row.CellsUsed())
                                {
                                    if (cell.Address.ColumnNumber > lastColumn) break;
                                    if (!cell.HasFormula && cell.Value != null)
                                    {
                                        string val = cell.Value.ToString();
                                        val = val.Replace("$COMPANYNAME$", "PAKSHI K RAMANATHAM CHETTIAR SON");
                                        val = val.Replace("$DATE$", DateTime.Now.ToString("dd-MMM-yyyy"));
                                        cell.Value = val;
                                    }

                                }
                            }

                        }
                        using (System.IO.MemoryStream outms = new System.IO.MemoryStream())
                        {
                            workBook.SaveAs(outms);
                            return new ByteArrayContent(outms.ToArray());
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return null;
        }

        public System.Net.Http.ByteArrayContent FORMS(ExcelExportCreteria _creteria)
        {
            _creteria.file = System.Web.Hosting.HostingEnvironment.MapPath("~/Resource/" + _creteria.form + ".xlsx");
            string worksheetname = _creteria.form;
            if (string.IsNullOrEmpty(_creteria.file))
                throw new Exception("Invalid file name.");

            // Replace with data method
            List<FORMS> Sformdata = GetSFormData(_creteria);
            try
            {
                var imageData = new ImageData();
                //imageData.Context = new UserClaim(_creteria.companyid);

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(_creteria.file), true))
                {
                    //Create a new DataTable.
                    DataTable dt = new DataTable();

                    using (var workBook = new ClosedXML.Excel.XLWorkbook(ms))
                    {
                        //Read the first Sheet from Excel file.
                        IXLWorksheet workSheet = workBook.Worksheet(worksheetname);
                        var lastColumn = workSheet.LastCellUsed().Address.ColumnNumber;
                        int lastRow = workSheet.LastCellUsed().Address.RowNumber;
                        if (lastColumn > 100) lastColumn = 100;
                        if (lastRow > 100) lastRow = 100;
                        //Loop through the Worksheet rows.
                        foreach (IXLRow row in workSheet.Rows())
                        {
                            if (row.RangeAddress.FirstAddress.RowNumber > lastRow) break;
                            if (row.FirstCell().Address.RowNumber == 12)
                            {
                                var lastcell = row.LastCellUsed();
                                if(lastcell == null)
                                {
                                    lastcell = row.Cell(17);
                                }
                                var rngData = workSheet.Range(row.FirstCell().Address, lastcell.Address);

                                //newrow = rngData;
                                List<string> colvalues = new List<string>();
                                foreach (var cell in row.Cells())
                                {
                                    if (cell.Value != null)
                                        colvalues.Add(cell.Value.ToString());
                                }
                                int rowcount = 12;
                                int sno = 0;
                                foreach (FORMS form in Sformdata)
                                {
                                    sno++;
                                    form.slno = sno.ToString();
                                    IXLRows newrow = workSheet.Row(rowcount++).InsertRowsBelow(1);
                                    int i = 0;
                                    foreach (var str in colvalues)
                                    {
                                        string newstring = "";
                                        if (str == "$SLNO$")
                                            newstring = form.slno;
                                        else if (str == "$EMPNAME$")
                                            newstring = form.name;
                                        else if (str == "$GENDER$")
                                            newstring = form.gender;
                                        else if (str == "$EMPFATHERNAME$")
                                            newstring = form.fathername;
                                        else if (str == "$DESIGNATION$")
                                            newstring = form.desingnationid;
                                        else if (str == "$EMPNO$")
                                            newstring = form.employeecode;
                                        else if (str == "$DOJ$")
                                            newstring = form.dateofjoining;
                                        else if (str == "$ADULT$")
                                            newstring = form.adult_child;
                                        else if (str == "$SHIFTNO$")
                                            newstring = form.shiftno;
                                        else if (str == "$TIMEOFCOMMENCEMENT$")
                                            newstring = form.timeof_commomcement;
                                        else if (str == "$RESTINTERVAL$")
                                            newstring = form.rest_interval;
                                        else if (str == "$TIMEWICHWORK$")
                                            newstring = form.time_wichwork;
                                        else if (str == "$WEEKHOLIDAYS$")
                                            newstring = form.week_holiday;
                                        else if (str == "$CLASSOFWORKERS$")
                                            newstring = form.class_workers;
                                        else if (str == "$MAXWAG$")
                                            newstring = form.earnings;
                                        else if (str == "$MINWAG$")
                                            newstring = form.rateofwages_min;

                                        setValue(newrow, i++, newstring);
                                    }
                                }
                                workSheet.Row(12).Delete();
                            }
                            else
                            {
                                foreach (var cell in row.CellsUsed())
                                {
                                    if (cell.Address.ColumnNumber > lastColumn) break;
                                    if (!cell.HasFormula && cell.Value != null)
                                    {
                                        string val = cell.Value.ToString();
                                        val = val.Replace("$COMPANYNAME$", "PAKSHI K RAMANATHAM CHETTIAR SON");
                                        val = val.Replace("$DATE$", DateTime.Now.ToString("dd-MMM-yyyy"));
                                        cell.Value = val;
                                    }

                                }
                            }

                        }
                        using (System.IO.MemoryStream outms = new System.IO.MemoryStream())
                        {
                            workBook.SaveAs(outms);
                            return new ByteArrayContent(outms.ToArray());
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return null;
        }

        public System.Net.Http.ByteArrayContent FORMESI(ExcelExportCreteria _creteria)
        {
            _creteria.file = System.Web.Hosting.HostingEnvironment.MapPath("~/Resource/" + _creteria.form + ".xlsx");
            string worksheetname = _creteria.form;
            if (string.IsNullOrEmpty(_creteria.file))
                throw new Exception("Invalid file name.");

            // Replace with data method
            List<FORMESI> ESIformdata = GetESIFormData(_creteria);
            try
            {
                var imageData = new ImageData();
                //imageData.Context = new UserClaim(_creteria.companyid);

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream(System.IO.File.ReadAllBytes(_creteria.file), true))
                {
                    //Create a new DataTable.
                    DataTable dt = new DataTable();

                    using (var workBook = new ClosedXML.Excel.XLWorkbook(ms))
                    {
                        //Read the first Sheet from Excel file.
                        IXLWorksheet workSheet = workBook.Worksheet(worksheetname);
                        var lastColumn = workSheet.LastCellUsed().Address.ColumnNumber;
                        int lastRow = workSheet.LastCellUsed().Address.RowNumber;
                        if (lastColumn > 100) lastColumn = 100;
                        if (lastRow > 100) lastRow = 100;
                        //Loop through the Worksheet rows.
                        foreach (IXLRow row in workSheet.Rows())
                        {
                            if (row.RangeAddress.FirstAddress.RowNumber > lastRow) break;
                            if (row.FirstCell().Address.RowNumber == 12)
                            {
                                var lastcell = row.LastCellUsed();
                                if (lastcell == null)
                                {
                                    lastcell = row.Cell(17);
                                }
                                var rngData = workSheet.Range(row.FirstCell().Address, lastcell.Address);

                                //newrow = rngData;
                                List<string> colvalues = new List<string>();
                                foreach (var cell in row.Cells())
                                {
                                    if (cell.Value != null)
                                        colvalues.Add(cell.Value.ToString());
                                }
                                int rowcount = 3;
                                int sno = 0;
                                foreach (FORMESI form in ESIformdata)
                                {
                                    sno++;
                                    form.slno = sno.ToString();
                                    IXLRows newrow = workSheet.Row(rowcount++).InsertRowsBelow(1);
                                    int i = 0;
                                    foreach (var str in colvalues)
                                    {
                                        string newstring = "";
                                        if (str == "$SLNO$")
                                            newstring = form.slno;
                                        else if (str == "$IPNUMBER$")
                                            newstring = form.employeecode;
                                        else if (str == "$EMPNAME$")
                                            newstring = form.name;
                                        else if (str == "$WORKINGDAYS$")
                                            newstring = form.workingdays;
                                        else if (str == "$TOTALWAGES$")
                                            newstring = form.totalwages;
                                        else if (str == "$REASONCODE$")
                                            newstring = form.reasoncode;
                                        else if (str == "$DATEOFLEAVING$")
                                            newstring = form.dateofleaving;
                                        setValue(newrow, i++, newstring);
                                    }
                                }
                                workSheet.Row(3).Delete();
                            }
                            else
                            {
                                foreach (var cell in row.CellsUsed())
                                {
                                    if (cell.Address.ColumnNumber > lastColumn) break;
                                    if (!cell.HasFormula && cell.Value != null)
                                    {
                                        string val = cell.Value.ToString();
                                        val = val.Replace("$COMPANYNAME$", "PAKSHI K RAMANATHAM CHETTIAR SON");
                                        val = val.Replace("$DATE$", DateTime.Now.ToString("dd-MMM-yyyy"));
                                        cell.Value = val;
                                    }

                                }
                            }

                        }
                        using (System.IO.MemoryStream outms = new System.IO.MemoryStream())
                        {
                            workBook.SaveAs(outms);
                            return new ByteArrayContent(outms.ToArray());
                        }
                    }
                }
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, null);
            }
            return null;
        }


        private void setValue(IXLRows newrow, int index, string value)
        {
            int i = 0;
            foreach (var cell in newrow.Cells())
            {
                if (index == i++)
                {
                    cell.Value = value;
                    return;
                }
            }

        }

        const string SQL_PForm = @"select employee.name,employee.employeecode, employee.gender,
                                   employee.desingnationid, referencelist.name designation, employee.personal, 
                                   employeesalaryschedule.schedulemonth,employeesalaryschedule.amount, employeesalaryschedule.isreceivable, employeesalaryschedule.parentid
                                   from employee inner join referencelist on employee.desingnationid = referencelist.id and type = 'DESIGNATION' 
                                   left outer join employeesalaryschedule on employeesalaryschedule.employeeid=employee.id and employeesalaryschedule.isreceivable = false";

        private List<FORMP> GetPFormData(ExcelExportCreteria _creteria)
        {
            List<FORMP> lst = new List<FORMP>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_PForm;

                //qb.AddParameter("parentid", 0, ">");

                DateTime schedulemonth = new DateTime(_creteria.year, _creteria.month, 1).AddMinutes(DateTime.Now.Subtract(DateTime.UtcNow).TotalMinutes);

                //qb.AddParameter("schedulemonth", schedulemonth, "=");
                if (_creteria.companyid > 0)
                    qb.AddParameter("employee.companyid", "companyid", _creteria.companyid, "=");


                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        FORMP oformr = new FORMP();
                        Employee emp = new Employee();
                        //oformr.slno = reader["id"] == DBNull.Value ? null : reader["id"].ToString();
                        oformr.name = reader["name"] == DBNull.Value ? null : reader["name"].ToString();
                        emp.personal_json = reader["personal"] == DBNull.Value ? "null" : reader["personal"].ToString();
                        oformr.fathername = (emp.personal == null) ? "" : emp.personal.fathername;
                        emp.gender = reader["gender"] == DBNull.Value ? "" : reader["gender"].ToString();
                        oformr.employeecode = reader["employeecode"] == DBNull.Value ? null : reader["employeecode"].ToString();
                        oformr.desingnation = reader["designation"] == DBNull.Value ? null : reader["designation"].ToString();
                        oformr.schedulemonth = reader["schedulemonth"] == DBNull.Value ? null : Convert.ToDateTime(reader["schedulemonth"]).ToString("dd/MM/yyyy");
                        oformr.amount = reader["amount"] == DBNull.Value ? null : reader["amount"].ToString();
                        oformr.advrecoverynoofinstl = "1";
                        oformr.advrecoverycomplndate = reader["schedulemonth"] == DBNull.Value ? null : Convert.ToDateTime(reader["schedulemonth"]).ToString("dd/MM/yyyy");
                        lst.Add(oformr);

                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _creteria);
            }
            return lst;

        }

        const string SQL_QForm = @"select employee.id, employee.name,employee.gender,referencelist.name designation,employee.dateofbirth,
                                    employeesalary.salarymonth,employeesalary.workingdays,referencelist.name designation,
                                    employeesalary.leavedays,employee.dateofjoining,employee.relievingdate,hasleft
                                    from employee inner join referencelist on employee.desingnationid = referencelist.id and type = 'DESIGNATION' 
                                    left outer join employeesalary on employeesalary.employeeid = employee.id
                                    left outer join employeesalaryheader on employeesalaryheader.employeeid = employee.id";

        const string SQL_QFormDays = @"select employeeid,todate,fromdate,days,fromdatehalfday,todatehalfday,sundaydays
                                    from employeeleave ";


        private List<FORMQ> GetQFormData(ExcelExportCreteria _creteria)
        {
            List<FORMQ> lst = new List<FORMQ>();
            NpgsqlConnection conn = CreateConnection();
            var fromdate = new DateTime(_creteria.year, _creteria.month, 1);
            var todate = fromdate.AddMonths(1);
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_QForm;

                DateTime salarymonth = new DateTime(_creteria.year, _creteria.month, 1); //.AddMinutes(DateTime.Now.Subtract(DateTime.UtcNow).TotalMinutes);

                qb.AddParameter("salarymonth", salarymonth, "=");
                //if (_creteria.companyid > 0)
                    //    qb.AddParameter("employee.companyid", "companyid", _creteria.companyid, "=");
                qb.SortField = "employee.employeecode";
                //qb.Limit = 1;

                    command = qb.GetCommand(conn);


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        FORMQ oformq = new FORMQ();
                        oformq.employeeid = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                        oformq.name = reader["name"] == DBNull.Value ? null : reader["name"].ToString();
                        oformq.dob = reader["dateofbirth"] == DBNull.Value ? DateTime.MinValue : (DateTime)reader["dateofbirth"];
                        oformq.dateofjoining = reader["dateofjoining"] == DBNull.Value ? DateTime.MinValue : Convert.ToDateTime(reader["dateofjoining"]);
                        oformq.relievingdate = reader["relievingdate"] == DBNull.Value ? DateTime.MinValue : (DateTime)reader["relievingdate"];
                        oformq.hasleft = reader["hasleft"] == DBNull.Value ? false : (bool)reader["hasleft"];
                        //oformq.dob = reader["dateofbirth"] == DBNull.Value ? DateTime.MinValue : (DateTime)reader["dateofbirth"];
                        oformq.desingnationid = reader["designation"] == DBNull.Value ? null : reader["designation"].ToString();
                        //oformq.workinghours = reader["workinghours"] == DBNull.Value ? null : reader["workinghours"].ToString();
                         oformq.openingsl = "0";
                         oformq.openingcl = "0";
                         oformq.openingpl = "0";
                         oformq.availsl = "1";
                         oformq.availcl = "1";
                         oformq.availpl = "1";
                         oformq.balancesl = "0";
                         oformq.balancecl = "0";
                         oformq.balancepl = "0";
                        oformq.dayvalue = new SortedDictionary<string, string>();
                        lst.Add(oformq);
                    }
                }

                qb = new QueryBuilder();
                qb.CurrentQuery = SQL_QFormDays;



                qb.AddParameter("fromdate", fromdate, ">=");
                qb.AddParameter("todate", todate, "<");

                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        //employeeid,todate,fromdate,days,fromdatehalfday,todatehalfday,sundaydays
                        var employeeid = reader.GetInt64(0);
                        var _fromdate = reader.GetDateTime(1);
                        var _todate = reader.GetDateTime(2);
                        var days = reader.GetDecimal(3);
                        var fromhalfdate = reader.GetInt32(4);
                        var todatehalfday = reader.GetInt32(5);
                        var sundaydays = (reader[6] == DBNull.Value) ? 0 : reader.GetDecimal(6);
                        if(lst.Exists(e=>e.employeeid == employeeid))
                        {
                            var form = lst.Find(e => e.employeeid == employeeid);
                            
                            for(var date=_fromdate;date<=_todate;date = date.AddDays(1))
                            {
                                if(date == _fromdate)
                                {
                                    if (fromhalfdate == 1)
                                        form.dayvalue.Add(date.Day.ToString(), "½A+4");
                                    else
                                        form.dayvalue.Add(date.Day.ToString(), "A");
                                }
                                else if(date == _todate)
                                {
                                    if(todatehalfday == 1)
                                        form.dayvalue.Add(date.Day.ToString(), "4+½A");
                                    else
                                        form.dayvalue.Add(date.Day.ToString(), "A");
                                }
                                else
                                {
                                    form.dayvalue.Add(date.Day.ToString(), "A");
                                }
                            }
                        }
                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _creteria);
            }

            for(;fromdate<todate; fromdate = fromdate.AddDays(1))
            {
                foreach(var em in lst)
                {
                    if(em.dayvalue != null && !em.dayvalue.ContainsKey(fromdate.Day.ToString()))
                    {
                        if (fromdate < em.dateofjoining || fromdate > em.relievingdate)
                            continue;

                        em.dayvalue.Add(fromdate.Day.ToString(), "8");
                    }
                }
            }
            return lst;

        }

        const string SQL_RForm = @"select employee.name as employeename,employee.gender,referencelist.name,employee.employeecode,
                                    employeesalary.salarymonth,employeesalary.workingdays,employeesalary.detail,
                                    employeesalary.leavedays,employeesalary.earnings,employeesalary.deductions,employeesalary.pf,employeesalary.esi,employeesalary.netsalary,
                                    employeesalaryheader.basicpay,employeesalaryheader.grosspay
                                    from employee  inner join referencelist on employee.desingnationid = referencelist.id and type = 'DESIGNATION' 
                                    left outer join employeesalary on employeesalary.employeeid = employee.id
                                    left outer join employeesalaryheader on employeesalaryheader.employeeid = employee.id ";

        private List<InfyPOS.Data.SalaryManager.SalaryHeader> DefaultSalaryHeaders()
        {
            ReferencelistDataService refData = new ReferencelistDataService();
            refData.Context = this.Context;
            var configList = refData.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "PAYCATEGORY",
            });

            //System.IO.File.WriteAllText(@"C:\Logs\configList.json", Newtonsoft.Json.JsonConvert.SerializeObject(configList));
            //configList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InfyPOS.Models.Referencelist>>(System.IO.File.ReadAllText(@"D:\dbdata\configList.json"));

            var defaultHeaders = new SortedList<long, InfyPOS.Data.SalaryManager.SalaryHeader>();
            var defaultCodes = new SortedList<string, InfyPOS.Data.SalaryManager.SalaryHeader>();
            foreach (var dr in configList)
            {

                InfyPOS.Data.SalaryManager.SalaryHeader sh = new InfyPOS.Data.SalaryManager.SalaryHeader();
                sh.Description = dr.name;
                sh.HeaderID = dr.id;
                try
                {
                    sh.Code = dr.code;
                    sh.Formula = dr.additionalinfo.formula;
                    InfyPOS.Data.SalaryManager.SalaryHeaderTypes type;
                    if (Enum.TryParse<InfyPOS.Data.SalaryManager.SalaryHeaderTypes>(dr.additionalinfo.paytype, out type))
                    {
                        sh.HeaderType = type;
                    }
                    defaultHeaders.Add(sh.HeaderID, sh);
                    if (!string.IsNullOrEmpty(sh.Code) && !defaultCodes.ContainsKey(sh.Code))
                    {
                        defaultCodes.Add(sh.Code, sh);
                    }

                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, sh.Description + "\r\n" + sh.Formula + "\r\n" + exp.Message + "\r\n" + exp.StackTrace);
                }
            }
            return defaultHeaders.Values.ToList();
        }

        private List<FORMR> GetRFormData(ExcelExportCreteria _creteria)
        {
            var headers = DefaultSalaryHeaders();
            List<FORMR> lst = new List<FORMR>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_RForm;

                // qb.AddParameter("employee.isactive", true, "=");

                DateTime salarymonth = new DateTime(_creteria.year, _creteria.month, 1);

                qb.AddParameter("salarymonth", salarymonth, "=");
                //if (_creteria.companyid > 0)
                //    qb.AddParameter("employee.companyid", "companyid", _creteria.companyid, "=");


                command = qb.GetCommand(conn);

                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        FORMR oformr = new FORMR();
                        //Employeesalary emps = new Employeesalary();
                        //oformr.slno = reader["slno"] == DBNull.Value ? null : reader["slno"].ToString();
                        oformr.name = reader["employeename"] == DBNull.Value ? null : reader["employeename"].ToString();
                        oformr.gender = reader["gender"] == DBNull.Value ? null : reader["gender"].ToString();
                        oformr.desingnationid = reader["name"] == DBNull.Value ? null : reader["name"].ToString();
                        oformr.salarymonth = reader["salarymonth"] == DBNull.Value ? null : reader["salarymonth"].ToString();
                        oformr.workingdays = reader["workingdays"] == DBNull.Value ? null : reader["workingdays"].ToString();
                        oformr.leavedays = reader["leavedays"] == DBNull.Value ? null : reader["leavedays"].ToString();
                        oformr.detail_json = reader["detail"] == DBNull.Value ? "null" : reader["detail"].ToString();
                        var bplist = headers.FindAll(h => h.HeaderType == InfyPOS.Data.SalaryManager.SalaryHeaderTypes.BasicPay).ToList();

                        oformr.basicpay  = oformr.detail.Sum(e => bplist.Exists(x => x.HeaderID == e.id) ? e.amount : 0).ToString("N0");
                        var dalist = headers.FindAll(h => h.HeaderType == InfyPOS.Data.SalaryManager.SalaryHeaderTypes.DearnessAllowance).ToList();

                        oformr.dearness_allowance = oformr.detail.Sum(e => dalist.Exists(x => x.HeaderID == e.id) ? e.amount : 0).ToString("N0");
                        var oalist = headers.FindAll(h => h.HeaderType == InfyPOS.Data.SalaryManager.SalaryHeaderTypes.OtherAllowance).ToList();

                        var pf = reader["pf"] == DBNull.Value ? 0 : (decimal)reader["pf"];
                        var esi = reader["esi"] == DBNull.Value ? 0 : (decimal)reader["esi"];
                        var otherdeduction = reader["deductions"] == DBNull.Value ? 0 : (decimal)reader["deductions"];

                        //oformr.other_allowance = oformr.detail.Sum(e => oalist.Exists(x => x.HeaderID == e.id) ? e.amount : 0).ToString("N0");
                        //oformr.otherdeduction= MathParserTK(reader["pf"] == DBNull.Value ? null : reader["pf"].ToString())-(reader["esi"] == DBNull.Value ? null : reader["esi"].ToString());
                        oformr.basicpay = reader["basicpay"] == DBNull.Value ? null : reader["basicpay"].ToString();
                        //oformr.dearness_allowance = reader["dearness_allowance"] == DBNull.Value ? null : reader["dearness_allowance"].ToString();
                        oformr.pf = reader["pf"] == DBNull.Value ? null : reader["pf"].ToString();
                        oformr.esi = reader["esi"] == DBNull.Value ? null : reader["esi"].ToString();
                        oformr.netsalary = reader["netsalary"] == DBNull.Value ? null : reader["netsalary"].ToString();
                        oformr.earnings = reader["earnings"] == DBNull.Value ? null : reader["earnings"].ToString();
                        oformr.ot_earned = "0";
                        oformr.ot_rate = "0";
                        oformr.monthly_rated = "MONTHLY";
                        lst.Add(oformr);

                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _creteria);
            }
            return lst;

        }


        const string SQL_SForm = @"select employee.name as employeename,employee.gender,employee.personal,referencelist.name,employee.employeecode,
                                    employee.dateofjoining,employeesalary.earnings,employeesalary.netsalary
                                    from employee inner join referencelist on employee.desingnationid = referencelist.id and type = 'DESIGNATION' 
                                    left outer join employeesalary on employeesalary.employeeid = employee.id
                                    left outer join employeesalaryheader on employeesalaryheader.employeeid = employee.id";

        private List<FORMS> GetSFormData(ExcelExportCreteria _creteria)
        {
            List<FORMS> lst = new List<FORMS>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_SForm;
                // qb.AddParameter("employee.isactive", true, "=");
                //DateTime salarymonth = new DateTime(_creteria.year, _creteria.month, 1).AddMinutes(DateTime.Now.Subtract(DateTime.UtcNow).TotalMinutes);
                DateTime salarymonth = new DateTime(_creteria.year, _creteria.month, 1);
                qb.AddParameter("salarymonth", salarymonth, "=");
                //if (_creteria.companyid > 0)
                //    qb.AddParameter("employee.companyid", "companyid", _creteria.companyid, "=");


                command = qb.GetCommand(conn);

                qb.SortField = "employee.employeecode";


                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        FORMS oforms = new FORMS();
                        Employee emp = new Employee();
                        //oforms.slno = reader["id"] == DBNull.Value ? null : reader["id"].ToString();
                        oforms.name = reader["employeename"] == DBNull.Value ? null : reader["employeename"].ToString();
                        oforms.gender = reader["gender"] == DBNull.Value ? null : reader["gender"].ToString()[0].ToString();
                        emp.personal_json = reader["personal"] == DBNull.Value ? "null" : reader["personal"].ToString();
                        oforms.fathername = (emp.personal == null) ? "" : emp.personal.fathername;
                        //oforms.fathername = reader["fathername"] == DBNull.Value ? null : reader["fathername"].ToString();
                        oforms.desingnationid = reader["name"] == DBNull.Value ? null : reader["name"].ToString();
                        //oforms.empno = reader["empno"] == DBNull.Value ? null : reader["empno"].ToString();
                        oforms.employeecode = reader["employeecode"] == DBNull.Value ? null : reader["employeecode"].ToString();
                        oforms.dateofjoining = reader["dateofjoining"] == DBNull.Value ? null : reader["dateofjoining"].ToString();
                        oforms.earnings = reader["earnings"] == DBNull.Value ? null : reader["earnings"].ToString();
                        oforms.shiftno = "GENERAL";
                        oforms.adult_child = "ADULT";
                        oforms.timeof_commomcement = "9:15";
                        oforms.rest_interval="04:00";
                        oforms.time_wichwork = "08:00";
                        oforms.week_holiday = "SUNDAY";
                        oforms.class_workers="NORMAL";
                        lst.Add(oforms);

                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _creteria);
            }
            return lst;

        }


        const string SQL_ESIForm = @"select employee.name as employeename,employee.employeecode,employee.relievingdate,
                                    employee.dateofjoining,employeesalary.earnings,employeesalary.netsalary,employeesalary.workingdays
                                    from employee left outer join employeesalary on employeesalary.employeeid = employee.id";

        private List<FORMESI> GetESIFormData(ExcelExportCreteria _creteria)
        {
            List<FORMESI> lst = new List<FORMESI>();
            NpgsqlConnection conn = CreateConnection();
            try
            {
                conn.Open();

                NpgsqlCommand command = null;

                QueryBuilder qb = new QueryBuilder();
                qb.CurrentQuery = SQL_ESIForm;

                //qb.AddParameter("parentid", 0, ">");

                DateTime schedulemonth = new DateTime(_creteria.year, _creteria.month, 1).AddMinutes(DateTime.Now.Subtract(DateTime.UtcNow).TotalMinutes);

                //qb.AddParameter("schedulemonth", schedulemonth, "=");
                if (_creteria.companyid > 0)
                    qb.AddParameter("employee.companyid", "companyid", _creteria.companyid, "=");


                command = qb.GetCommand(conn);
                using (System.Data.IDataReader reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        FORMESI oformesi = new FORMESI();
                        //oformr.slno = reader["id"] == DBNull.Value ? null : reader["id"].ToString();
                        oformesi.name = reader["name"] == DBNull.Value ? null : reader["name"].ToString();
                        oformesi.employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        oformesi.earnings = reader["earnings"] == DBNull.Value ? null : reader["earnings"].ToString();
                        oformesi.relievingdate = reader["relievingdate"] == DBNull.Value ? null : Convert.ToDateTime(reader["relievingdate"]).ToString("dd/MM/yyyy");
                        oformesi.dateofjoining = reader["dateofjoining"] == DBNull.Value ? null : Convert.ToDateTime(reader["dateofjoining"]).ToString("dd/MM/yyyy");
                        oformesi.netsalary = reader["netsalary"] == DBNull.Value ? null : reader["netsalary"].ToString();
                        oformesi.workingdays = reader["workingdays"] == DBNull.Value ? null : reader["workingdays"].ToString();
                        lst.Add(oformesi);

                    }
                }
                conn.Close();
            }
            catch (Exception exp)
            {
                if (conn != null) conn.Close();
                WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, _creteria);
            }
            return lst;

        }


    }

    public class FORMP
    {

        [Description("$SLNO$")]
        public string slno { get; set; }
        [Description("$EMPNAME$")]
        public string name { get; set; }
        [Description("$EMPFATHERNAME$")]
        public string fathername { get; set; }
        public string employeecode { get; set; }
        public string desingnation { get; set; }

        public string schedulemonth { get; set; }
        public string amount { get; set; }
        public string advrecoverynoofinstl { get; set; }
        public string advrecoverycomplndate { get; set; }

        public string damagecaused { get; set; }
        public string deddateofshowcause { get; set; }
        public string dedamount { get; set; }
        public string dedrecoverynoofinstl { get; set; }
        public string deddateofcompletion { get; set; }

        public string fineact { get; set; }
        public string finedateofshowcause { get; set; }
        public string fineamount { get; set; }
        public string finedateofcompletion { get; set; }
        public string signorthumb { get; set; }

        public string remark { get; set; }
    }

    public class FORMQ
    {
        public FORMQ()
        {
          
        }

        [Description("$SLNO$")]
        public string slno { get; set; }
        [Description("$EMPNAME$")]
        public string name { get; set; }
        [Description("$doj$")]
        public DateTime dateofjoining { get; set; }
        public DateTime dob { get; set; }
        public DateTime relievingdate { get; set; }
        public bool hasleft { get; set; }
        public string desingnationid { get; set; }

        public string openingsl { get; set; }
        public string openingcl { get; set; }
        public string openingpl { get; set; }
        public string availsl { get; set; }
        public string availcl { get; set; }
        public string availpl { get; set; }
        public string balancesl { get; set; }
        public string balancecl { get; set; }
        public string balancepl { get; set; }

        public SortedDictionary<string,string> dayvalue { get; set; }
        public string overtime { get; set; }
        public string workinghours { get; set; }
        public string maternityleaveavailed { get; set; }
        public long employeeid { get; set; }
    }

    public class FORMR
    {

        [Description("$SLNO$")]
        public string slno { get; set; }
        [Description("$EMPNAME$")]
        public string name { get; set; }
        [Description("$GENDER$")]
        public string gender { get; set; }
        [Description("$DESIGNATION$")]
        public string desingnationid { get; set; }

        public string monthly_rated { get; set; }
        public string salarymonth { get; set; }
        public string workingdays { get; set; }
        public string leavedays { get; set; }

        public string earnings { get; set; }
        public string ot_rate { get; set; }
        public string basicpay { get; set; }
        public string dearness_allowance { get; set; }
        public string other_allowance { get; set; }

        public string ot_earned { get; set; }
        public string leavewages { get; set; }
        public string grosspay { get; set; }
        public string pf { get; set; }
        public string esi { get; set; }
        public string otherdeduction { get; set; }
        public string fines { get; set; }
        public string netsalary { get; set; }
        public string sign_with_thumbdetails { get; set; }
        public string totalunpaidamt { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string detail_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(detail); } set { detail = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Employeesalary.Detail>>(value); } }
        public List<Employeesalary.Detail> detail { get; set; }

    }

    public class FORMS
    {
        public FORMS()
        {
            adult_child = "Adult";
            week_holiday = "Sunday";
            shiftno = "Normal";
            

        }
        [Description("$SLNO$")]
        public string slno { get; set; }
        [Description("$EMPNAME$")]
        public string name { get; set; }
        [Description("$GENDER$")]
        public string gender { get; set; }
        [Description("$EMPFATHERNAME$")]
        public string fathername { get; set; }
        [Description("$DESIGNATION$")]
        public string desingnationid { get; set; }
        public string employeecode { get; set; }
        public string dateofjoining { get; set; }

        public string adult_child { get; set; }
        public string shiftno { get; set; }
        public string timeof_commomcement { get; set; }
        public string rest_interval { get; set; }

        public string time_wichwork { get; set; }
        public string week_holiday { get; set; }
        public string class_workers { get; set; }
        public string earnings { get; set; }
        public string rateofwages_min { get; set; }
    }

    public class FORMESI
    {
       
        [Description("$SLNO$")]
        public string slno { get; set; }
        [Description("$EMPNAME$")]
        public string name { get; set; }
        public string employeecode  { get; set; }
        public string earnings      { get; set; }
        public string relievingdate { get; set; }
        public string dateofjoining { get; set; }
        public string netsalary     { get; set; }
        public string workingdays { get; set; }
        public string totalwages { get; set; }
        public string reasoncode { get; set; }
        public string dateofleaving { get; set; }


    }


}
