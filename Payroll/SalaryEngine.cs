using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using InfyPOS.Common;
using System.Data;
using System.Linq;
using System.Web;
using Newtonsoft.Json;
using InfyPOS.Models;
using MathParserTK;
using InfyPOS.Services;

namespace InfyPOS.Data
{
    public class SalaryEngine
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<string, List<string>> formulacache;
        public static List<string> Parseformula(string Formula, string formulacodes)
        {
            if (formulacache == null)
                formulacache = new System.Collections.Concurrent.ConcurrentDictionary<string, List<string>>();

            List<string> result;
            if (!formulacache.TryGetValue(Formula + formulacodes, out result))
            {
                result = new List<string>();
                StringReader sr = new StringReader(Formula);
                int charactor = 0;
                string current = "";
                while ((charactor = sr.Read()) >= 0)
                {
                    if (char.IsLetter((char)charactor))
                    {
                        current += (char)charactor;
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(current))
                        {
                            if (!formulacodes.Contains("," + current.ToUpper() + ","))
                                result.Add(current);
                            current = "";
                        }
                    }
                }
                if (!string.IsNullOrEmpty(current) && !formulacodes.Contains("," + current.ToUpper() + ","))
                    result.Add(current);


                formulacache.TryAdd(Formula + formulacodes, result);
            }
            return result;
        }

        private long institutionid;
        public UserClaim Context;
        long organizationid;
        public SalaryEngine()
        {

        }

        public void Initialize(long organizationid, long institutionid)
        {
            this.organizationid = organizationid;
            this.institutionid = institutionid;
        }

        public void Initialize(long organizationid, UserClaim context)
        {
            this.Context = context;
            this.organizationid = context.OrganizationID;

        }


        public static string SerializeXML(object objectValue, Type type)
        {
            System.Xml.XmlDocument doc = new System.Xml.XmlDocument();
            XmlSerializer s = new XmlSerializer(type);
            System.IO.MemoryStream stream = new System.IO.MemoryStream();
            s.Serialize(stream, objectValue);
            stream.Position = 0;
            doc.Load(stream);
            return doc.InnerXml;
        }

        public static Object DeSerializeXML(string objectXML, Type type)
        {
            try
            {
                if (string.IsNullOrEmpty(objectXML)) return null;
                System.IO.StringReader read = new StringReader(objectXML);
                XmlSerializer s = new XmlSerializer(type);
                System.Xml.XmlReader reader = new System.Xml.XmlTextReader(read);
                return s.Deserialize(reader);
            }
            catch (Exception exp)
            {
                return null;
            }
        }



        //internal List<EmployeeSalaryConfig> BuildMasters(int month, int year, long companyid, long departmentid, byte roleid)
        //{
        //    string query = @"select Employee.Name, Reference.Name as Designation,Employee.EmployeeID,cast(0 as decimal) as LeaveDays,
        //    cast(0 as decimal) as Othours,cast(0 as decimal) as wdays,Employeement.DateOfJoining,Employeement.CurrentBasicPay,
        //    Employeement.CurrentOverTimePerHour,
        //    CurrentSalaryBreakup,Employee.EmployeeID from Employee
        //    inner join Employeement on Employeement.EmployeeID = Employee.EmployeeID
        //    left outer join Reference dpt on dpt.ReferenceOID = Employee.DepartmentID
        //    left outer join Reference on Reference.ReferenceOID = Employee.DesignationID
        //    where Employee.status = 0 and IsEmployeeResigned = 0";
        //    SortedList<string, object> param = new SortedList<string, object>();
        //    DateTime fromDate = new DateTime(year, month, 1);
        //    DateTime todate = fromDate.AddMonths(1).AddDays(-1);
        //    List<EmployeeSalaryConfig> retValue = new List<EmployeeSalaryConfig>();
        //    param.Add("fromdate", fromDate.AddMinutes(-1));
        //    param.Add("todate", todate.AddMinutes(1));
        //    if (departmentid > 0)
        //    {
        //        param.Add("DepartmentID", departmentid);
        //        query = query + " and Employee.DepartmentID = @DepartmentID";
        //    }else if(companyid>0)
        //    {
        //        param.Add("CompanyID", companyid.ToString());
        //        query = query + " and dpt.Value = @CompanyID";
        //    }
        //    if (roleid > 0)
        //    {
        //        param.Add("EmploymentRole", roleid);
        //        query = query + " and Employee.EmploymentRole = @EmploymentRole";
        //    }

        //    return BuildMasters(query, param);
        //}

        public class SalaryCustomConfig
        {
            public class WorkingDate
            {
                public enum WorkingDays_Mode
                {
                    AllDays,
                    Fixed
                }
                public enum WorkingDays_LeaveMode
                {
                    Include,
                    Exclude
                }
                public int fixeddays { get; set; }
                public WorkingDays_Mode mode { get; set; }
                public WorkingDays_LeaveMode sunday { get; set; }
                public WorkingDays_LeaveMode holiday { get; set; }
            }

            public class LeaveDate
            {
                public enum WorkingDays_LeaveMode
                {
                    Include,
                    Exclude
                }
                public int fixeddays { get; set; }
                public WorkingDays_LeaveMode sunday { get; set; }
                public WorkingDays_LeaveMode holiday { get; set; }
            }

            public class LeaveEncashments
            {
                public int EncashmentEligible { get; set; }
                public int LeaveEligible { get; set; }
                public enum WorkingDays_LeaveMode
                {
                    Include,
                    Exclude
                }
                public int fixeddays { get; set; }
                public WorkingDays_LeaveMode sunday { get; set; }
                public WorkingDays_LeaveMode holiday { get; set; }
            }


        }
        [Serializable]
        public class EmployeeSalaryConfig
        {
            public string SalaryBreakupJSON { get; set; }
            public long SalaryBreakupTime { get; set; }
            public decimal ApprovedLeaves { get; set; }

            public long ID { get; set; }
            public DateTime DateOfJoin { get; set; }
            public DateTime DateOfBirth { get; set; }
            public decimal BasicPay { get; set; }
            public decimal Advance { get; set; }
            public int OverTimeHour { get; set; }
            public decimal OverTimeRate { get; set; }
            public decimal OverTimePay { get; set; }

            public decimal WorkDayCount { get; set; }
            public string Name { get; set; }
            public string Designation { get; set; }
            public bool HasConflict { get; set; }
            public string EmployeeCode { get; set; }
            public long EmployeeID { get; set; }
            public decimal grosspay { get; set; }
            public decimal fixedda { get; set; }
            public decimal fixedbasic { get; set; }
            public long spldeductid { get; set; }
            public long splallowanceid { get; set; }
            public decimal CurrentOverTimePerHour { get; set; }
            public decimal LeaveDays { get; set; }
            public decimal resignjoin { get; set; }

            [JsonIgnore]
            [JsonProperty(Required = Required.Default)]
            public string pfandesi_json
            {
                get { return Newtonsoft.Json.JsonConvert.SerializeObject(pfandesi); }
                set
                {
                    if (value != null)
                        pfandesi = Newtonsoft.Json.JsonConvert.DeserializeObject<InfyPOS.Models.Employee.Pfandesi>(value);
                }
            }
            public InfyPOS.Models.Employee.Pfandesi pfandesi { get; set; }

            public SortedList<string, InfyPOS.Data.SalaryManager.SalaryHeader> SalaryBreakup { get; set; }
            public List<InfyPOS.Data.SalaryManager.SalaryHeader> defaultHeaders { get; set; }
            public decimal SundayLeaves { get; set; }
            public decimal HolidayLeaves { get; set; }
            public decimal HolidayAbsent { get; set; }
            public int LCMode { get; set; }

            public LCModes Encashmentmode
            {
                get { return (LCModes)LCMode; }
                set
                {
                    LCMode = (int)value;
                }
            }
            public enum LCModes
            {
                None,
                Addtional,
                Adjustment,
                Autometic,
                Fixed
            }
            public int SalaryMode { get; set; }
            public DateTime Relievingdate { get; set; }
            public bool Hasleft { get; set; }
            public bool Haswithheld { get; set; }
            public DateTime Withheldfrom { get; set; }
            public decimal fulldays { get; set; }
            public decimal halfday { get; set; }
            public int workingmode { get; set; }
            public long departmentid { get; set; }
            public long companyid { get; set; }
            public long divisionid { get; set; }
            public decimal HolidaySundayLeaves { get; set; }
            public decimal HolidaySundayAbsent { get; set; }
            public decimal LeaveTaken { get; set; }
            public bool eligiblepf { get; set; }
            public bool pfselfpaid { get; set; }
            public bool esiselfpaid { get; set; }
            public bool eligibleesi { get; set; }
            public bool encashment_adjustabsent { get; set; }
            public int encashment_permanent_extra { get; set; }
            public decimal LeaveEncashmentDays { get; set; }
            public decimal LeaveEncashmentLeaveSwapDays { get; set; }
            public decimal AcceptedWorkDays { get; set; }
            public decimal ComputedWorkDays { get; set; }
            public decimal incentive { get; set; }
            public decimal LeaveEncashment { get; set; }
            public long locationid { get; set; }
            public decimal HolidayLeaveEncashment { get; set; }
            public decimal HolidaySalary { get; set; }
            public decimal HolidaySalaryDays { get; set; }
            public decimal AdjustedJoiningResigning { get; set; }
            public int latecheckin { get; set; }
            public int earlycheckout { get; set; }
            public int morningovertime { get; set; }
            public int eveningovertime { get; set; }
            public decimal AbsentDays { get; set; }
            public decimal Additionalabsentdays { get; set; }
            public decimal firsthalf { get; set; }
            public long floorid { get; set; }
            public decimal sundayworking { get; set; }
            public decimal holidayworking { get; set; }
            public decimal holidaysundayworking { get; set; }
            public decimal Paydays { get; set; }
            public bool proratedaysbased { get; set; }
            public bool updatedafromstructure { get; set; }
            public bool updatebpfromstructure { get; set; }
            public int TotalDaysInMonth { get; internal set; }
            public decimal fullgrosspay { get; internal set; }
            public long defaultshiftid { get; internal set; }
            public decimal workinghours { get; internal set; }

            public void SetSalaryBreakup(bool isleavebased, bool disableformulaprocess, string source, SortedList<long, SalaryManager.SalaryHeader> defHeaders)
            {
                var employeeHeaders = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InfyPOS.Models.Employeesalarystructure.Configuration>>(source);
                if (employeeHeaders == null)
                    employeeHeaders = new List<Employeesalarystructure.Configuration>();
                SetSalaryBreakup(isleavebased, disableformulaprocess, employeeHeaders, defHeaders);
            }
            public void SetSalaryBreakup(bool isleavebased, bool disableformulaprocess, List<InfyPOS.Models.Employeesalarystructure.Configuration> employeeHeaders,
                SortedList<long, SalaryManager.SalaryHeader> defHeaders)
            {


                if (BasicPay == 0) BasicPay = fixedbasic;
                if (grosspay == 0) grosspay = fixedbasic + fixedda;
                //if (defHeaders.Values.Any(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.DearnessAllowance))
                //{
                //    long daid = defHeaders.Values.First(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.DearnessAllowance).HeaderID;
                //    if (employeeHeaders.Any(e => e.id == daid))
                //    {
                //        employeeHeaders.Find(e => e.id == daid).formula = fixedda.ToString();
                //    }
                //}

                //if (grosspay > fixedda + fixedbasic)
                //{
                //    if (employeeHeaders.Any(e => e.id == splallowanceid))
                //    {
                //        employeeHeaders.Find(e => e.id == splallowanceid).formula = (grosspay - (fixedda + fixedbasic)).ToString();
                //    }
                //}
                //else if (grosspay < fixedda + fixedbasic)
                //{
                //    if (employeeHeaders.Any(e => e.id == spldeductid))
                //    {
                //        employeeHeaders.Find(e => e.id == spldeductid).formula = (fixedda + fixedbasic- grosspay).ToString();
                //    }
                //}

                var missingheaders = defHeaders.ToList().FindAll(e => !employeeHeaders.Exists(x => x.id == e.Key));
                foreach (var item in defHeaders)
                {
                    if (!employeeHeaders.Exists(e => e.id == item.Key))
                        employeeHeaders.Add(new Employeesalarystructure.Configuration()
                        {
                            id = item.Key,
                            code = item.Value.Code,
                            formula = item.Value.Formula,
                            value = item.Value.Value
                        });
                }
                employeeHeaders.RemoveAll(e => !defHeaders.ContainsKey(e.id));

                if (!this.eligibleesi && employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) &&
                defHeaders[e.id].HeaderType == SalaryManager.SalaryHeaderTypes.ESI))
                {
                    string code = employeeHeaders.Find(e => defHeaders[e.id].HeaderType == SalaryManager.SalaryHeaderTypes.ESI).code;
                    foreach (var xheader in employeeHeaders)
                    {
                        if (xheader.code == code)
                            xheader.formula = "0.00";
                        else
                            xheader.formula = xheader.formula == null ? "0.00" : xheader.formula.Replace(code, "0.00");
                    }
                }

                if (this.eligibleesi)
                {
                    if (defHeaders.Values.ToList().Exists(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.ESI) &&
                        !employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) && defHeaders.ContainsKey(e.id)
                        && defHeaders[e.id].HeaderType == SalaryManager.SalaryHeaderTypes.ESI))
                    {
                        var itm = defHeaders.Values.ToList().Find(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.ESI);
                        if (itm != null)
                            employeeHeaders.Add(new InfyPOS.Models.Employeesalarystructure.Configuration()
                            {
                                code = itm.Code,
                                id = itm.HeaderID,
                                formula = itm.Formula
                            });
                    }
                    else
                    {
                        var eitm = employeeHeaders.Find(e => defHeaders[e.id].HeaderType == SalaryManager.SalaryHeaderTypes.ESI);
                        var itm = defHeaders.Values.ToList().Find(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.ESI);
                        if (eitm != null && itm != null)
                        {
                            if (eitm.formula == "" || eitm.formula == "0" || eitm.formula == "0.00")
                                eitm.formula = itm.Formula;
                        }
                    }
                }

                if (this.eligiblepf)
                {
                    if (defHeaders.Values.ToList().Exists(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.ProvidentFund) &&
                        !employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) && defHeaders[e.id].HeaderType == SalaryManager.SalaryHeaderTypes.ProvidentFund))
                    {
                        var itm = defHeaders.Values.ToList().Find(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.ProvidentFund);
                        if (itm != null)
                            employeeHeaders.Add(new InfyPOS.Models.Employeesalarystructure.Configuration()
                            {
                                code = itm.Code,
                                id = itm.HeaderID,
                                formula = itm.Formula
                            });
                    }
                    else
                    {
                        var eitm = employeeHeaders.Find(e => defHeaders[e.id].HeaderType == SalaryManager.SalaryHeaderTypes.ProvidentFund);
                        var itm = defHeaders.Values.ToList().Find(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.ProvidentFund);
                        if (eitm != null && itm != null)
                        {
                            if (eitm.formula == "" || eitm.formula == "0" || eitm.formula == "0.00")
                                eitm.formula = itm.Formula;
                        }
                    }
                }


                if (!this.eligiblepf && employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) && defHeaders[e.id].HeaderType == SalaryManager.SalaryHeaderTypes.ProvidentFund))
                {
                    string code = employeeHeaders.Find(e => defHeaders[e.id].HeaderType == SalaryManager.SalaryHeaderTypes.ProvidentFund).code;
                    foreach (var xheader in employeeHeaders)
                    {
                        if (xheader.code == code)
                            xheader.formula = "0.00";
                        else
                            xheader.formula = xheader.formula == null ? "0.00" : xheader.formula.Replace(code, "0.00");
                    }
                }


                if (!this.esiselfpaid && employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) && defHeaders[e.id].HeaderType == SalaryManager.SalaryHeaderTypes.ESI_Employee))
                {
                    string code = employeeHeaders.Find(e => defHeaders[e.id].HeaderType == SalaryManager.SalaryHeaderTypes.ESI_Employee).code;
                    foreach (var xheader in employeeHeaders)
                    {
                        if (xheader.code == code || string.IsNullOrEmpty(xheader.formula))
                            xheader.formula = "0.00";
                        else
                            xheader.formula = xheader.formula.Replace(code, "0.00");
                    }
                }

                if (this.esiselfpaid)
                {
                    if (defHeaders.Values.ToList().Exists(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.ESI_Employee) &&
                        !employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) && defHeaders[e.id].HeaderType == SalaryManager.SalaryHeaderTypes.ESI_Employee))
                    {
                        var itm = defHeaders.Values.ToList().Find(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.ESI_Employee);
                        if (itm != null)
                            employeeHeaders.Add(new InfyPOS.Models.Employeesalarystructure.Configuration()
                            {
                                code = itm.Code,
                                id = itm.HeaderID,
                                formula = itm.Formula
                            });
                    }
                    else
                    {
                        var eitm = employeeHeaders.Find(e => defHeaders[e.id].HeaderType == SalaryManager.SalaryHeaderTypes.ESI_Employee);
                        var itm = defHeaders.Values.ToList().Find(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.ESI_Employee);
                        if (eitm != null && itm != null)
                            if (eitm.formula == "" || eitm.formula == "0" || eitm.formula == "0.00")
                                eitm.formula = itm.Formula;
                    }
                }

                if (!this.pfselfpaid && employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) && defHeaders[e.id].HeaderType == SalaryManager.SalaryHeaderTypes.PF_Employee))
                {
                    string code = employeeHeaders.Find(e => defHeaders[e.id].HeaderType == SalaryManager.SalaryHeaderTypes.PF_Employee).code;
                    foreach (var xheader in employeeHeaders)
                    {
                        if (xheader.code == code)
                            xheader.formula = "0.00";
                        else
                            xheader.formula = xheader.formula.Replace(code, "0.00");
                    }
                }

                if (this.pfselfpaid)
                {
                    if (!employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) && defHeaders[e.id].HeaderType == SalaryManager.SalaryHeaderTypes.PF_Employee) &&
                        defHeaders.Values.ToList().Exists(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.PF_Employee))
                    {
                        var itm = defHeaders.Values.ToList().Find(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.PF_Employee);
                        if (itm != null)
                            employeeHeaders.Add(new InfyPOS.Models.Employeesalarystructure.Configuration()
                            {
                                code = itm.Code,
                                id = itm.HeaderID,
                                formula = itm.Formula
                            });
                    }
                    else if (employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) && defHeaders[e.id].HeaderType == SalaryManager.SalaryHeaderTypes.PF_Employee))
                    {
                        var eitm = employeeHeaders.Find(e => defHeaders[e.id].HeaderType == SalaryManager.SalaryHeaderTypes.PF_Employee);
                        var itm = defHeaders.Values.ToList().Find(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.PF_Employee);
                        if (eitm != null && itm != null)
                            if (string.IsNullOrEmpty(eitm.formula) || eitm.formula == "0" || eitm.formula == "0.00")
                                eitm.formula = itm.Formula;
                    }
                }

                defaultHeaders = new List<SalaryManager.SalaryHeader>();
                SalaryBreakup = new SortedList<string, InfyPOS.Data.SalaryManager.SalaryHeader>();


                string additionalcodes = "," + string.Join(",", defHeaders.Values.ToList().FindAll(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.FixedDeduction || e.HeaderType == SalaryManager.SalaryHeaderTypes.FixedAllowance || e.HeaderType == SalaryManager.SalaryHeaderTypes.FineDeduction).ConvertAll(e => e.Code));
                foreach (var xheader in employeeHeaders)
                {
                    if (!defHeaders.ContainsKey(xheader.id)) continue;

                    var header = defHeaders[xheader.id].Clone();
                    header.Formula = xheader.formula;
                    if (SalaryBreakup.ContainsKey(defHeaders[xheader.id].Code.ToUpper()))
                        SalaryBreakup.Remove(defHeaders[xheader.id].Code.ToUpper());

                    SalaryBreakup.Add(defHeaders[xheader.id].Code.ToUpper(), header);
                    header.HeaderType = defHeaders[xheader.id].HeaderType;
                    header.Description = defHeaders[xheader.id].Description;
                    header.Value = xheader.value;
                    header.SetAsIsNumber();
                    header.BuildFormula(additionalcodes);
                }
                HasConflict = PreprocessSalaryBreakup(isleavebased, disableformulaprocess, additionalcodes, 0);
            }

            private bool PreprocessSalaryBreakup(bool isleavebased, bool disableformulaprocess, string additionalcodes, int iteration)
            {
                if (iteration == 10 || disableformulaprocess) return true;
                bool modified = false;
                foreach (KeyValuePair<string, InfyPOS.Data.SalaryManager.SalaryHeader> kv in SalaryBreakup)
                {
                    if (kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.ProvidentFund ||
                        kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.ESI)
                    {
                        continue;
                    }
                    kv.Value.BuildFormula(additionalcodes);
                    if (kv.Value.formulaKeys.Count == 0)
                    {
                        if (kv.Value.IsFullAmount && !string.IsNullOrEmpty(kv.Value.Formula))
                        {
                            kv.Value.IsNumber = false;
                            //if (isleavebased)
                            //    kv.Value.Formula = "rnd(" + kv.Value.Formula + "-(" + kv.Value.Formula + "/days*leavedays))";
                            //else
                            //    kv.Value.Formula = "rnd(" + kv.Value.Formula + "/days*computedworkingdays)";
                        }
                        continue;
                    }
                    foreach (string s in kv.Value.formulaKeys.ToArray())
                    {
                        if (SalaryBreakup.ContainsKey(s))
                        {
                            if (SalaryBreakup[s].HeaderType == SalaryManager.SalaryHeaderTypes.LeaveEncashment)
                                SalaryBreakup[s].Formula = "leaveencashment";

                            if (SalaryBreakup[s].HeaderType == SalaryManager.SalaryHeaderTypes.Incentives)
                                SalaryBreakup[s].Formula = "incentives";

                            if (SalaryBreakup[s].HeaderType == SalaryManager.SalaryHeaderTypes.HolidaySalary)
                                SalaryBreakup[s].Formula = "holidayencashment";

                            modified = true;
                            if (SalaryBreakup[s].IsFullAmount || !SalaryBreakup[s].IsNumber ||
                                SalaryBreakup[s].HeaderType == SalaryManager.SalaryHeaderTypes.LeaveEncashment ||
                                SalaryBreakup[s].HeaderType == SalaryManager.SalaryHeaderTypes.Incentives ||
                                SalaryBreakup[s].HeaderType == SalaryManager.SalaryHeaderTypes.HolidaySalary ||
                                SalaryBreakup[s].HeaderType == SalaryManager.SalaryHeaderTypes.FixedAllowance ||
                                SalaryBreakup[s].HeaderType == SalaryManager.SalaryHeaderTypes.FixedDeduction ||
                                SalaryBreakup[s].HeaderType == SalaryManager.SalaryHeaderTypes.FineDeduction)
                            {
                                if (!string.IsNullOrEmpty(SalaryBreakup[s].Formula) && SalaryBreakup[s].Formula.Length > 0)
                                    kv.Value.Formula = kv.Value.Formula.Replace(s, "rnd(" + SalaryBreakup[s].Formula + ")");
                                else
                                    kv.Value.Formula = kv.Value.Formula.Replace(s, "0.00");

                            }
                            else if (!string.IsNullOrEmpty(SalaryBreakup[s].Formula) && SalaryBreakup[s].Formula.Length > 0)
                            {
                                var replacekey = disableformulaprocess ? s.ToLower() : SalaryBreakup[s].Formula;
                                if (isleavebased)
                                    kv.Value.Formula = kv.Value.Formula.Replace(s, "rnd(" + replacekey + "-(" + replacekey + "/days*leavedays))");
                                else
                                    kv.Value.Formula = kv.Value.Formula.Replace(s, "rnd(" + replacekey + "/days*computedworkingdays)");
                            }
                            kv.Value.BuildFormula(additionalcodes);
                            if (kv.Value.formulaKeys.Count == 1 && kv.Value.formulaKeys[0] == s)
                            {
                                kv.Value.Formula = "0.00";
                                kv.Value.formulaKeys.Clear();
                            }
                        }
                    }
                }
                if (modified)
                    return PreprocessSalaryBreakup(isleavebased, disableformulaprocess, additionalcodes, iteration++);

                return false;
            }
        }

        //int month;
        //int year;

        DateTime salarymonth;
        DateTime fromdate;
        DateTime todate;
        public void Initialize(int month, int year)
        {
            this.salarymonth = new DateTime(year, month, 1);
            fromdate = new DateTime(year, month, 1);
            todate = fromdate.AddMonths(1).AddDays(-1);
            //this.month = month;
            //this.year = year;
            //days = new DateTime(year, month, 1).AddMonths(1).AddDays(-1).Day;
        }

        public void Initialize(long organizationid, int month, int year, DateTime fromdate, DateTime todate)
        {
            this.organizationid = organizationid;
            if (!Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_payslipbydate)
            {
                fromdate = new DateTime(year, month, 1);
                todate = fromdate.AddMonths(1).AddDays(-1);
            }
            this.salarymonth = new DateTime(year, month, 1);
            //this.month = month;
            //this.year = year;
            this.fromdate = fromdate;
            this.todate = todate;
        }


        private SortedList<long, SalaryManager.SalaryHeader> defaultHeaders = null;
        public SortedList<long, SalaryManager.SalaryHeader> DefaultHeaders
        {
            get
            {
                if (defaultHeaders == null || defaultHeaders.Count == 0)
                    BuildSalaryHeaders();
                return defaultHeaders;
            }
        }

        private SortedList<string, SalaryManager.SalaryHeader> defaultCodes = null;
        public SortedList<string, SalaryManager.SalaryHeader> DefaultCodes
        {
            get
            {
                if (defaultCodes == null)
                    BuildSalaryHeaders();
                return defaultCodes;
            }
        }


        private void BuildSalaryHeaders()
        {
            ReferencelistDataService refData = new ReferencelistDataService() { Context = this.Context };
            var configList = refData.Search(new InfyPOS.Models.ReferencelistCreteria()
            {
                type = "PAYCATEGORY",
            });

            //System.IO.File.WriteAllText(@"C:\Logs\configList.json", Newtonsoft.Json.JsonConvert.SerializeObject(configList));
            //configList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InfyPOS.Models.Referencelist>>(System.IO.File.ReadAllText(@"D:\dbdata\configList.json"));

            defaultHeaders = new SortedList<long, SalaryManager.SalaryHeader>();
            defaultCodes = new SortedList<string, SalaryManager.SalaryHeader>();
            foreach (var dr in configList)
            {

                SalaryManager.SalaryHeader sh = new SalaryManager.SalaryHeader();
                sh.Description = dr.name;
                sh.HeaderID = dr.id;
                try
                {
                    sh.Code = dr.code;
                    sh.Formula = dr.additionalinfo != null ? dr.additionalinfo.formula : "";
                    sh.Max = dr.additionalinfo != null ? dr.additionalinfo.maxlimit : 0;
                    SalaryManager.SalaryHeaderTypes type;
                    if (Enum.TryParse<SalaryManager.SalaryHeaderTypes>(dr.additionalinfo.paytype, out type))
                    {
                        sh.HeaderType = type;
                    }
                    defaultHeaders.Add(sh.HeaderID, sh);
                    if (!string.IsNullOrEmpty(sh.Code) && !defaultCodes.ContainsKey(sh.Code))
                    {
                        defaultCodes.Add(sh.Code, sh);
                    }
                    sh.incentiveconfig = dr.additionalinfo.incentiveconfig;
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, sh.Description + "\r\n" + sh.Formula + "\r\n" + exp.Message + "\r\n" + exp.StackTrace);
                }
            }
        }

        public Dictionary<long, List<SalaryManager.ScheduleAmount>> BuildExternal(DateTime fromdate, DateTime todate
            , long employeeID)
        {



            Dictionary<long, List<SalaryManager.ScheduleAmount>> retValue = new Dictionary<long, List<SalaryManager.ScheduleAmount>>();
            SortedList<string, object> param = new SortedList<string, object>();
            InfyPOS.Common.BaseData baseData = new BaseData() { Context = new UserClaim() { OrganizationID = organizationid } };
            baseData.Context = this.Context;

            string query = @"select employeeid,amount,detail,schedulemonth,salaryheaderid from employeesalaryschedule
                            where employeesalaryschedule.isactive = true and isreceivable=false and 
                            schedulemonth>= @fromdate AND schedulemonth < @todate";

            param.Add("fromdate", fromdate);
            param.Add("todate", todate.AddDays(1));
            if (employeeID > 0)
            {
                param.Add("employeeID", employeeID);
                query = query + " and employeeid = @employeeid";
            }
            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    long empID = reader.GetInt64(0);
                    SalaryManager.ScheduleAmount sa = new SalaryManager.ScheduleAmount();
                    sa.Amount = reader.GetDecimal(1);
                    sa.AdditionalInfo = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    sa.Date = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3);
                    sa.HeaderID = reader.GetInt64(4);
                    if (!retValue.ContainsKey(empID))
                        retValue.Add(empID, new List<SalaryManager.ScheduleAmount>());
                    retValue[empID].Add(sa);
                }
            }
            return retValue;
        }

        public List<EmployeeSalaryConfig> BuildMasters(int month, int year, long employeeID, int salarymode, bool disableforumlaprocess)
        {
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_payslipbydate && fromdate.Year > 2000 && todate.Year > 2000)
            {
                return BuildMasters(fromdate, todate.AddDays(1), employeeID, salarymode, disableforumlaprocess);
            }
            else
            {
                DateTime fromDate = new DateTime(year, month, 1);
                DateTime toDate = fromDate.AddMonths(1);
                return BuildMasters(fromDate, toDate, employeeID, salarymode, disableforumlaprocess);
            }
        }
        public List<EmployeeSalaryConfig> BuildMasters(DateTime fromDate, DateTime todate, long employeeID, int salarymode, bool disableforumlaprocess)
        {
            string query = @"
                update employeeleave set isactive=false,modifiedby=-4 
                from employee 
                where employeeleave.fromdate>=@fromdate and employeeleave.fromdate<@todate and employee.id = employeeleave.employeeid and 
                employeeleave.fromdate<employee.dateofjoining  and employee.salarymode=@salarymode;

                update employeeinout set isactive=false,modifiedby=-4 
                from employee 
                where employeeinout.attendanceon>=@fromdate and employeeinout.attendanceon<@todate and employee.id = employeeinout.employeeid and 
                employeeinout.attendanceon<employee.dateofjoining and employee.salarymode=@salarymode;

                update employeeleave set isactive=false,modifiedby=-4 
                from employee 
                where employeeleave.fromdate>=@fromdate and employeeleave.fromdate<@todate and employee.hasleft and employee.relievingdate>'2000-01-01' 
                and employee.id = employeeleave.employeeid and employeeleave.fromdate>employee.relievingdate and employee.salarymode=@salarymode;

                update employeeinout set morningabsent=true,afternoonabsent =true where id in ( 
				select employeeinout.id from employeeinout 
				inner join employeeleave on employeeleave.isactive = true and employeeleave.employeeid = employeeinout.employeeid and 
				employeeleave.fromdate=employeeinout.attendanceon 
				where employeeinout.attendanceon>=@fromdate and employeeinout.attendanceon<@todate and employeeinout.isactive = true and employeeleave.days>=1 and 
				morningabsent=false and afternoonabsent=false);
				
				update employeeinout set morningabsent=true,afternoonabsent =false where id in ( 
				select employeeinout.id from employeeinout 
				inner join employeeleave on employeeleave.isactive = true and employeeleave.employeeid = employeeinout.employeeid and 
				employeeleave.fromdate=employeeinout.attendanceon 
				where employeeinout.attendanceon>=@fromdate and employeeinout.attendanceon<@todate and employeeinout.isactive = true and employeeleave.days=0.5 and 
                employeeleave.fromdatehalfday=1 and 
				morningabsent=false and afternoonabsent=false);
				
				update employeeinout set morningabsent=true,afternoonabsent =false where id in ( 
				select employeeinout.id from employeeinout 
				inner join employeeleave on employeeleave.isactive = true and employeeleave.employeeid = employeeinout.employeeid and 
				employeeleave.fromdate=employeeinout.attendanceon 
				where employeeinout.attendanceon>=@fromdate and employeeinout.attendanceon<@todate and employeeinout.isactive = true and employeeleave.days=0.5 and 
                employeeleave.todatehalfday=1 and
				morningabsent=false and afternoonabsent=false);

                with salaryheader as (
			    SELECT id, employeeid, fromdate, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			    FROM employeesalaryheader where fromdate<@todate)
			
			    select employee.name,referencelist.name as Designation,employee.id as EmployeeID,dateofjoining,
			    employeesalaryheader.basicpay,employeesalaryheader.grosspay,employeesalarystructure.fixedbasic,
                employeesalarystructure.fixedda,employeesalarystructure.deducttextsalaryid,
                0.00 as CurrentOverTimePerHour, employeesalaryheader.configuration as CurrentSalaryBreakup,
                Leave.LeaveDays,SundayLeaves, holidayLeaves.LeaveDays as HolidayLeaves, 
                holidaypayable.LeaveDays HolidayAbsent, employeesalarystructure.splallowanceid,employee.employeecode,
                lcmode,employee.salarymode,relievingdate,hasleft,haswithheld,withheldfrom,pfandesi,dateofbirth,
                fulldays,halfday,employee.workingmode,employee.departmentid,employee.sectionid,employee.companyid,
                holidayLeaves.SunDays HolidaySundayLeaves,
                holidaypayable.SunDays HolidaySundayAbsent,employeesalaryheader.eligibleesi,employeesalaryheader.eligiblepf,
                employeesalaryheader.pfselfpaid,employeesalaryheader.esiselfpaid,employee.locationid,
                holidayLeaves.leaveencashment HolidayLeaveEncashment,Leave.ApprovedLeaves,
                inout.latecheckin,inout.morningovertime,inout.eveningovertime, Leave.firsthalf,employee.floorid,inout.sundayworking,
                inout.holidayworking,inout.holidaysundayworking,inout.earlycheckout,defaultshiftid
                from employee 
                left outer join referencelist on referencelist.id = employee.desingnationid
                left outer join (
                                select employeeid,SUM(days) as LeaveDays,SUM(sundaydays) as SundayLeaves,sum(case when approvedby>0 then 1 else 0 end) ApprovedLeaves,  
                                Sum(case when fromdatehalfday > 0 then 0.5 else 0 end) as firsthalf from employeeleave 
                                inner join employee on employee.id = employeeleave.employeeid  and employee.salarymode=@salarymode
                                where EmployeeLeave.isactive and deductsalary=true 
                                AND EmployeeLeave.status in ('A','1H','2H','')
                                and fromdate>=dateofjoining and
                                (employee.hasleft = false or relievingdate>=fromdate) and
                                fromdate >=@fromdate and todate<@todate
                                group by EmployeeID
                                ) Leave on Leave.employeeid = employee.id
                           

                left outer join (select sum(case when morningabsent = false and afternoonabsent = false then 1 else 0 end) fulldays,
                                sum(case when morningabsent != afternoonabsent and (morningabsent = true or afternoonabsent = true) then 1 else 0 end) as halfday,
                                employeeid,
                                sum(case when morningabsent=false and afternoonabsent = false then earlycheckout/1 else 0 end) earlycheckout,
                                sum(case when morningabsent=false and afternoonabsent = false then latecheckin/1 else 0 end) latecheckin,
                                sum(case when morningabsent=false then morningovertime/1 else 0 end) morningovertime, 
                                sum(case when afternoonabsent=false then eveningovertime/1 else 0 end) eveningovertime,
                                sum(case when EXTRACT(DOW FROM attendanceon)=0 then 
								(case when morningabsent = false and afternoonabsent = false then 1 else (case when morningabsent = true and afternoonabsent = true then 0 else 0.5 end) end) else 0 end) as sundayworking,
								sum(case when holiday.id is not null then (case when morningabsent = false and afternoonabsent = false then 1 else (case when morningabsent = true and afternoonabsent = true then 0 else 0.5 end) end) else 0 end) as holidayworking,
                                sum(case when holiday.id is not null and EXTRACT(DOW FROM attendanceon)=0 then (case when morningabsent = false and afternoonabsent = false then 1 else (case when morningabsent = true and afternoonabsent = true then 0 else 0.5 end) end) else 0 end) as holidaysundayworking
                                from employeeinout
                                inner join employee on employee.id = employeeinout.employeeid   
                                left outer join holiday on holiday.holidayon = attendanceon and holiday.isactive = true
                                where employeeinout.isactive
                                and attendanceon>=dateofjoining and
                                (employee.hasleft = false or relievingdate>=attendanceon) and
                                attendanceon>=@fromdate and attendanceon<@todate
                                group by employeeid) inout
                            on inout.employeeid = employee.id

                left outer join (select employeeid,sum(days) as LeaveDays,sum(case when EXTRACT(DOW FROM fromdate)=0 then days else 0 end) as Sundays,
                sum(leaveencashment*days) leaveencashment  from holiday
	            inner join EmployeeLeave on EmployeeLeave.fromdate = holiday.holidayon and EmployeeLeave.isactive=true
                inner join employee on employee.id = employeeleave.employeeid and employee.salarymode=@salarymode
	            where holiday.isactive=true and fromdate>=@fromdate and todate<@todate and fromdate>=dateofjoining and
                                (employee.hasleft = false or relievingdate>=fromdate)
	            group by employeeid) holidayLeaves on holidayLeaves.employeeid = employee.id

                left outer join (select employeeid,sum(days) as LeaveDays,sum(case when EXTRACT(DOW FROM fromdate)=0 then days else 0 end) as Sundays  from holiday
	            inner join EmployeeLeave on EmployeeLeave.fromdate = holiday.holidayon and EmployeeLeave.isactive=true
                inner join employee on employee.id = employeeleave.employeeid and employee.salarymode=@salarymode
	            where holiday.isactive=true and payable=true and fromdate>=@fromdate and todate<@todate and fromdate>=dateofjoining and
                                (employee.hasleft = false or relievingdate>=fromdate)
	            group by employeeid) holidaypayable on holidaypayable.employeeid = employee.id
                
                left outer join salaryheader on salaryheader.ranknumber = 1 and salaryheader.employeeid = employee.id
			    left outer join employeesalaryheader on employeesalaryheader.id = salaryheader.id
                left outer join employeesalarystructure on employeesalarystructure.id = employeesalaryheader.salarystructureid
                where employee.isactive and (employee.hasleft = false or relievingdate>=@fromdate) and employee.DateOfJoining<@todate and employee.salarymode=@salarymode ";

            // and (dateofreleaving <DateOfJoining or dateofreleaving is null) 

            SortedList<string, object> param = new SortedList<string, object>();

            List<EmployeeSalaryConfig> retValue = new List<EmployeeSalaryConfig>();
            param.Add("fromdate", fromDate);
            param.Add("todate", todate);

            if (employeeSalaryConfig.fine_late_perminute > 0)
                param.Add("latecheckin", employeeSalaryConfig.fine_late_perminute);
            else
                param.Add("latecheckin", 1);

            if (employeeSalaryConfig.overtime_morning_perminute > 0)
                param.Add("morningovertime", employeeSalaryConfig.overtime_morning_perminute);
            else
                param.Add("morningovertime", 1);

            if (employeeSalaryConfig.overtime_evening_perminute > 0)
                param.Add("eveningovertime", employeeSalaryConfig.overtime_evening_perminute);
            else
                param.Add("eveningovertime", 1);
            //if (employeeSalaryConfig.workingmode > 0) 
            //    param.Add("workingmode", employeeSalaryConfig.workingmode);
            //else
            //    param.Add("workingmode", 0);

            if (employeeID > 0)
            {
                param.Add("employeeID", employeeID);
                query = query + " and employee.id = @employeeID";
            }
            if (employeeSalaryConfig.workingmode > 0)
            {
                param.Add("workingmode", employeeSalaryConfig.workingmode);
                query = query + " and employee.workingmode = @workingmode";
            }
            if (employeeSalaryConfig.floorid > 0)
            {
                param.Add("floorid", employeeSalaryConfig.floorid);
                query = query + " and employee.floorid = @floorid";
            }
            if (employeeSalaryConfig.companyid > 0)
            {
                param.Add("companyid", employeeSalaryConfig.companyid);
                query = query + " and employee.companyid = @companyid";
            }

            if (Context.accessiblelocation != null && !Context.accessiblelocation.Exists(x => x == -1))
            {
                query = query+(" and employee.locationid in ("+string.Join(",", Context.accessiblelocation)+")");
            }

            if (salarymode == 0)
            {
                query = query.Replace("and employee.salarymode=@salarymode", " ");
            }
            else
            {
                param.Add("salarymode", salarymode);
            }

            query += @" Order By case when employeecode ~'^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end";
            var result = BuildMasters(query, param, disableforumlaprocess);


            if (employeeSalaryConfig.fine_formula ||
                employeeSalaryConfig.overtime_formula)
            {
                ReferencelistDataService refData = new ReferencelistDataService() { Context = this.Context };
                var configList = refData.Search(new InfyPOS.Models.ReferencelistCreteria()
                {
                    type = "WORKINGHOURS",
                });
                result.GroupBy(x => x.defaultshiftid).ToList().ForEach(x =>
                {
                    x.ToList().ForEach(y => y.workinghours = employeeSalaryConfig.overtime_perdayhours);
                    if (x.Key > 0 &&
                        configList.Exists(y => y.id == x.Key))
                    {
                        var additionalinfo = configList.Find(y => y.id == x.Key).additionalinfo;
                        if (additionalinfo != null)
                        {
                            var fromtime = BaseEntity.ParseTime(additionalinfo.fromtime);
                            var totime = BaseEntity.ParseTime(additionalinfo.totime);
                            var totalhours = totime.Subtract(fromtime).TotalHours;
                            x.ToList().ForEach(y => y.workinghours = Convert.ToDecimal(totalhours));
                        }
                    }
                });
            }

            if (employeeSalaryConfig.incentive &&
                DefaultHeaders.Values.ToList().Exists(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.Incentives))
            {
                try
                {
                    InfyPOS.Payroll.IncentiveSalary incentiveservice = new InfyPOS.Payroll.IncentiveSalary(organizationid, fromDate, todate.AddDays(-1),
                               0, 0, 0, 0, true, employeeID, new List<int>() { 0 });

                    var incentivedata = incentiveservice.Compute(null, 0);

                    var incentconfig = DefaultHeaders.Values.ToList().Find(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.Incentives);

                    foreach (var incentivedataitem in incentivedata)
                    {
                        foreach (var emp in incentivedataitem.Employees)
                        {
                            if (result.Exists(e => e.EmployeeID == emp.employeeid))
                            {
                                var incentivevalue = result.Find(e => e.EmployeeID == emp.employeeid);
                                if (incentconfig.incentiveconfig != null)
                                {
                                    if (incentconfig.incentiveconfig.RemoveBarcode)
                                        emp.amount -= emp.itemincentive;
                                    if (incentconfig.incentiveconfig.RemovePrefix)
                                        emp.amount -= emp.prefixincentive;
                                    if (incentconfig.incentiveconfig.RemoveProductLevel)
                                        emp.amount -= emp.productlevelincentive;
                                    if (incentconfig.incentiveconfig.RemoveSettings)
                                        emp.amount -= emp.incentive;
                                }
                                incentivevalue.incentive = emp.amount;
                            }
                        }
                    }
                }
                catch (Exception exp)
                {

                }
            }
            return result;
        }

        private List<EmployeeSalaryConfig> BuildMasters(string query, SortedList<string, object> param, bool disableforumlaprocess)
        {
            List<EmployeeSalaryConfig> retValue = new List<EmployeeSalaryConfig>();
            BaseData baseData = new BaseData() { Context = new UserClaim() { OrganizationID = organizationid } };
            baseData.Context = this.Context;
            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    if (reader.IsDBNull(4))
                        continue;

                    EmployeeSalaryConfig entity = new EmployeeSalaryConfig();
                    entity.Name = reader.GetString(0);
                    entity.Designation = reader.IsDBNull(1) ? "" : reader.GetString(1);
                    entity.EmployeeID = reader.GetInt64(2);
                    entity.DateOfJoin = reader.IsDBNull(3) ? DateTime.Now.Date : reader.GetDateTime(3);
                    entity.BasicPay = reader.IsDBNull(4) ? 0 : reader.GetDecimal(4);
                    entity.grosspay = reader.IsDBNull(5) ? 0 : reader.GetDecimal(5);
                    entity.fixedbasic = reader.IsDBNull(6) ? 0 : reader.GetDecimal(6);
                    entity.fixedda = reader.IsDBNull(7) ? 0 : reader.GetDecimal(7);
                    entity.spldeductid = reader.IsDBNull(8) ? 0 : reader.GetInt64(8);
                    entity.LeaveDays = reader.IsDBNull(11) ? 0 : reader.GetDecimal(11);
                    entity.SundayLeaves = reader.IsDBNull(12) ? 0 : reader.GetDecimal(12);
                    entity.HolidayLeaves = reader.IsDBNull(13) ? 0 : reader.GetDecimal(13);
                    entity.HolidayAbsent = reader.IsDBNull(14) ? 0 : reader.GetDecimal(14);
                    entity.splallowanceid = reader.IsDBNull(15) ? 0 : reader.GetInt64(15);
                    entity.EmployeeCode = reader.IsDBNull(16) ? "" : reader.GetString(16);
                    entity.LCMode = reader.IsDBNull(17) ? 0 : reader.GetInt32(17);
                    entity.SalaryMode = reader.IsDBNull(18) ? 0 : reader.GetInt32(18);
                    entity.Relievingdate = reader.IsDBNull(19) ? DateTime.Now.Date : reader.GetDateTime(19);
                    entity.Hasleft = reader.IsDBNull(20) ? false : reader.GetBoolean(20);
                    entity.Haswithheld = reader.IsDBNull(21) ? false : reader.GetBoolean(21);
                    entity.Withheldfrom = reader.IsDBNull(22) ? DateTime.Now.Date : reader.GetDateTime(22);
                    entity.pfandesi_json = reader.IsDBNull(23) ? "" : reader.GetString(23);
                    entity.DateOfBirth = reader.IsDBNull(24) ? DateTime.Now.Date : reader.GetDateTime(24);

                    entity.fulldays = reader.IsDBNull(25) ? 0 : reader.GetDecimal(25);
                    entity.halfday = reader.IsDBNull(26) ? 0 : reader.GetDecimal(26);

                    entity.workingmode = reader.IsDBNull(27) ? 0 : reader.GetInt32(27);
                    entity.departmentid = reader.IsDBNull(28) ? 0 : reader.GetInt64(28);
                    entity.divisionid = reader.IsDBNull(29) ? 0 : reader.GetInt64(29);
                    entity.companyid = reader.IsDBNull(30) ? 0 : reader.GetInt64(30);

                    entity.HolidaySundayLeaves = reader.IsDBNull(31) ? 0 : reader.GetDecimal(31);
                    entity.HolidaySundayAbsent = reader.IsDBNull(32) ? 0 : reader.GetDecimal(32);

                    entity.eligibleesi = reader.IsDBNull(33) ? false : reader.GetBoolean(33);
                    entity.eligiblepf = reader.IsDBNull(34) ? false : reader.GetBoolean(34);
                    entity.pfselfpaid = reader.IsDBNull(35) ? false : reader.GetBoolean(35);
                    entity.esiselfpaid = reader.IsDBNull(36) ? false : reader.GetBoolean(36);
                    entity.locationid = reader.IsDBNull(37) ? 0 : reader.GetInt64(37);
                    entity.HolidayLeaveEncashment = reader.IsDBNull(38) ? 0 : reader.GetDecimal(38);
                    entity.ApprovedLeaves = reader.IsDBNull(39) ? 0 : reader.GetDecimal(39);

                    entity.latecheckin = reader.IsDBNull(40) ? 0 : reader.GetInt32(40);
                    entity.morningovertime = reader.IsDBNull(41) ? 0 : reader.GetInt32(41);
                    entity.eveningovertime = reader.IsDBNull(42) ? 0 : reader.GetInt32(42);
                    entity.firsthalf = reader.IsDBNull(43) ? 0 : reader.GetDecimal(43);
                    entity.floorid = reader.IsDBNull(44) ? 0 : reader.GetInt64(44);
                    entity.sundayworking = reader.IsDBNull(45) ? 0 : reader.GetDecimal(45);
                    entity.holidayworking = reader.IsDBNull(46) ? 0 : reader.GetDecimal(46);
                    entity.holidaysundayworking = reader.IsDBNull(47) ? 0 : reader.GetDecimal(47);
                    entity.earlycheckout = reader.IsDBNull(48) ? 0 : reader.GetInt32(48);
                    entity.defaultshiftid = reader.IsDBNull(49) ? 0 : reader.GetInt64(49);

                    //entity.fineday = reader.IsDBNull(43) ? 0 : reader.GetInt32(43);
                    try
                    {
                        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                        entity.SalaryBreakupJSON = reader.IsDBNull(10) ? "" : reader.GetString(10);
                        stopwatch.Start();
                        entity.SetSalaryBreakup(
                            (Models.EmployeeSalaryConfig.WorkDays_Mode)employeeSalaryConfig.workdays_mode == Models.EmployeeSalaryConfig.WorkDays_Mode.AllDays
                            , disableforumlaprocess, reader.IsDBNull(10) ? "" : reader.GetString(10), DefaultHeaders);
                        stopwatch.Stop();
                        if (stopwatch.ElapsedMilliseconds > 1000)
                        {
                            entity.SalaryBreakupTime = stopwatch.ElapsedMilliseconds;
                        }
                    }
                    catch (Exception exp)
                    {
                        throw exp;
                    }
                    retValue.Add(entity);
                }
            }



            //entity.DateOfJoin.Month == month o&& entity.DateOfJoin.Year == year
            return retValue;
        }
        public void Calculate(EmployeeSalaryConfig schConfigy)
        {

            MathParserTK.MathParser mathparser = new MathParserTK.MathParser(organizationid);
            mathparser.Specials.Add("bp", schConfigy.BasicPay.ToString());
            mathparser.Specials.Add("advance", "0");
            mathparser.Specials.Add("gp", "0");
            mathparser.Specials.Add("ot", "0");
            mathparser.Specials.Add("round", "0");
            if (schConfigy.SalaryBreakup.Values.ToList().Exists(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.LeaveEncashment))
                mathparser.Specials.Add("leaveencashment", schConfigy.LeaveEncashment.ToString("F0"));

            if (schConfigy.SalaryBreakup.Values.ToList().Exists(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.Incentives))
                mathparser.Specials.Add("incentives", schConfigy.incentive.ToString("F0"));

            decimal earnamt = 0;
            decimal dedamt = 0;
            InfyPOS.Data.SalaryManager.SalaryHeader roundingHeader = null;
            foreach (KeyValuePair<string, InfyPOS.Data.SalaryManager.SalaryHeader> kv in schConfigy.SalaryBreakup)
            {
                if (kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.Rounding)
                {
                    roundingHeader = kv.Value;
                    continue;
                }
                if (string.IsNullOrEmpty(kv.Value.Formula))
                    continue;

                double amt = 0;
                double baseAmt = 0;
                if (double.TryParse(kv.Value.Formula, out amt))
                {
                    baseAmt = amt;
                }
                else
                {
                    if (kv.Value.formulaKeys.Count > 0)
                    {

                        WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(kv.Value.formulaKeys[0] + " not able to calculate", WebAPI.Common.Logger.LogType.Error, kv.Value);
                        continue;
                    }
                    amt = mathparser.Parse(kv.Value.Formula, false);
                    baseAmt = amt;
                }
                kv.Value.Amount = Math.Round((decimal)Math.Abs(baseAmt), 0);

                if (kv.Value.IsDeduction)
                    dedamt += kv.Value.Amount;
                else
                    earnamt += kv.Value.Amount;
            }
            if (roundingHeader != null)
            {
                decimal netamt = earnamt - dedamt;
                decimal round = netamt % 10;
                roundingHeader.Amount = round != 0 ? 10 - round : 0;
            }
        }



        public bool includeSunday;
        List<Holiday> holidayList = new List<Holiday>();
        public class Holiday
        {
            public DateTime Date { get; set; }
            public bool Sunday
            {
                get
                {
                    return Date.DayOfWeek == DayOfWeek.Sunday;
                }
            }
            public bool Wages { get; set; }
            public int Encashment { get; set; }
        }
        /*private decimal WorkingDays()
        {
            includeSunday = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_workdaysmode == 1;
            InfyPOS.Common.BaseData baseData = new BaseData();
            baseData.Context = this.Context;
            var query = @"select holidayon,leaveencashment from holiday where holidayon >= @fromdate AND holidayon < @todate and isactive=true";

            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", fromdate);
            param.Add("todate", todate.AddDays(1));
            totalHoliday = 0;
            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    totalHolidayEncashment += (reader.IsDBNull(1) ? 0 : reader.GetInt32(1));
                    if ((reader.IsDBNull(1) ? 0 : reader.GetInt32(1)) > 0)
                        totalPayableHoliday++;
                    if (reader.GetDateTime(0).DayOfWeek == DayOfWeek.Sunday && !includeSunday)
                        continue;
                    totalHoliday++;
                    holidayList.Add(reader.GetDateTime(0));
                }
            }
            TotalHoliday = totalHoliday;
            DateTime monthstart = fromdate;
            DateTime monthend = todate;
            TotalSunday = !includeSunday ? SundayTotal() : 0;
            //nonpayableholiday = NonPayableHoliday();
            //payableholiday = PayableHoliday();
            totalHoliday += TotalSunday;
            TotalDays = (decimal)monthend.Subtract(monthstart).TotalDays + 1;
            TotalWorkingDays = (decimal)monthend.Subtract(monthstart).TotalDays + 1 - totalHoliday;
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_workdaysmode == 2)
                TotalWorkingDays = 30;
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_workdaysmode == 3)
                TotalWorkingDays = TotalDays;
            return TotalWorkingDays;
        }*/

        private int SundayTotal()
        {
            int sundays = 0;
            DateTime monthstart = fromdate;
            DateTime monthend = todate.AddDays(1);
            while (monthstart < monthend)
            {
                if (monthstart.DayOfWeek == DayOfWeek.Sunday)
                    sundays++;
                monthstart = monthstart.AddDays(1);
            }
            return sundays;
        }

        private int SundayTotal(DateTime stodate, bool isfrom)
        {
            int sundays = 0;
            DateTime monthstart = fromdate;
            DateTime monthend = todate;
            if (isfrom)
                monthstart = stodate;
            else
                monthend = stodate;
            while (monthstart <= monthend)
            {
                if (monthstart.DayOfWeek == DayOfWeek.Sunday)
                    sundays++;
                monthstart = monthstart.AddDays(1);
            }
            return sundays;
        }

        //private int PayableHoliday()
        //{
        //    InfyPOS.Common.BaseData baseData = new BaseData();
        //    baseData.Context = this.Context;

        //    string query = @"select count(*) from holiday where date_part('month', holidayon)= @month AND date_part('year', holidayon) = @year and isactive=true and payable=true";
        //    SortedList<string, object> param = new SortedList<string, object>();
        //    var fromDate = new DateTime(year, month, 1);
        //    param.Add("month", month);
        //    param.Add("year", year);

        //    using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param))
        //    {
        //        if (reader.Read())
        //        {
        //            return reader.GetInt32(0);
        //        }
        //    }
        //    return 0;
        //}

        //private int NonPayableHoliday()
        //{
        //    InfyPOS.Common.BaseData baseData = new BaseData();
        //    baseData.Context = this.Context;

        //    string query = @"select count(*) from holiday where date_part('month', holidayon)= @month AND date_part('year', holidayon) = @year and isactive=true and payable=false";
        //    SortedList<string, object> param = new SortedList<string, object>();
        //    var fromDate = new DateTime(year, month, 1);
        //    param.Add("month", month);
        //    param.Add("year", year);

        //    using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param))
        //    {
        //        if (reader.Read())
        //        {
        //            return reader.GetInt32(0);
        //        }
        //    }
        //    return 0;
        //}

        public EmployeeSalaryConfig CalculateDailywages(EmployeeSalaryConfig config, List<InfyPOS.Data.SalaryManager.ScheduleAmount> schConfig)
        {
            //config = Newtonsoft.Json.JsonConvert.DeserializeObject<EmployeeSalaryConfig>(System.IO.File.ReadAllText(@"D:\json.txt"));
            //WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(config.Name, WebAPI.Common.Logger.LogType.Info, config);
            DateTime monthlastday = new DateTime(config.DateOfJoin.Year, config.DateOfJoin.Month, 1).AddMonths(1).AddDays(-1);
            config.WorkDayCount = monthlastday.Day;

            decimal totalwordays = config.fulldays + (config.halfday * (decimal)0.5);

            decimal basicPay = Math.Round(config.BasicPay * totalwordays, 0);
            config.LeaveDays = config.WorkDayCount - totalwordays;

            basicPay = Math.Round(basicPay, 0);
            MathParserTK.MathParser mathparser = new MathParserTK.MathParser(organizationid);
            mathparser.Specials.Add("bp", basicPay.ToString());
            mathparser.Specials.Add("advance", "0");
            mathparser.Specials.Add("gp", "0");
            mathparser.Specials.Add("ot", Math.Round(config.OverTimePay, 0).ToString());
            mathparser.Specials.Add("round", "0");
            mathparser.Specials.Add("days", config.WorkDayCount.ToString());
            mathparser.Specials.Add("leavedays", config.LeaveDays.ToString());
            mathparser.Specials.Add("workdays", totalwordays.ToString());
            mathparser.Specials.Add("holidayabsent", config.HolidayAbsent.ToString());
            mathparser.Specials.Add("holidayleave", config.HolidayLeaves.ToString());

            decimal earnamt = 0;
            decimal dedamt = 0;
            InfyPOS.Data.SalaryManager.SalaryHeader roundingHeader = null;
            foreach (KeyValuePair<string, InfyPOS.Data.SalaryManager.SalaryHeader> kv in config.SalaryBreakup)
            {
                if (kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.Rounding)
                {
                    roundingHeader = kv.Value;
                    continue;
                }
                if (string.IsNullOrEmpty(kv.Value.Formula) || kv.Value.formulaKeys.Count > 0)
                    continue;
                double amt = 0;
                double baseAmt = 0;

                if (double.TryParse(kv.Value.Formula, out amt))
                {
                    if (kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.BasicPay)
                        baseAmt = (double)basicPay;
                    else
                        baseAmt = amt;

                    //if (!kv.Value.IsFullAmount)
                    //{
                    //    if (config.DateOfJoin.Month == month && config.DateOfJoin.Year == year)
                    //    {
                    //        baseAmt = (double)((baseAmt / days) * (double)(days - config.LeaveDays));
                    //    }
                    //    else if (config.LeaveDays > 0)
                    //    {
                    //        baseAmt = baseAmt - ((amt / days) * (double)config.LeaveDays);
                    //    }
                    //}
                    baseAmt = kv.Value.Code.Contains("ESI") ? Math.Ceiling(baseAmt) : Math.Round(baseAmt, 0);
                }
                else
                {
                    amt = mathparser.Parse(kv.Value.Formula, false);
                    baseAmt = kv.Value.Code.Contains("ESI") ? Math.Ceiling(amt) : Math.Round(amt, 0);
                }

                if (kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.ProvidentFund)
                {
                    if (config.DateOfBirth.Year > 1000 &&
                        config.DateOfBirth.AddYears(58) < salarymonth)
                    {
                        baseAmt = 0;
                    }
                }

                kv.Value.Amount = (decimal)baseAmt;
                //Math.Round((decimal)Math.Abs(baseAmt), 0);

                if (kv.Value.IsDeduction)
                    dedamt += kv.Value.Amount;
                else
                    earnamt += kv.Value.Amount;
            }

            if (schConfig != null && schConfig.Count > 0)
            {
                foreach (SalaryManager.ScheduleAmount sa in schConfig)
                {
                    if (DefaultHeaders.ContainsKey(sa.HeaderID) && config.SalaryBreakup.ContainsKey(DefaultHeaders[sa.HeaderID].Code.ToUpper()))
                    {
                        config.SalaryBreakup[DefaultHeaders[sa.HeaderID].Code.ToUpper()].Amount += sa.Amount;
                        dedamt += sa.Amount;
                    }
                    else if (DefaultHeaders.ContainsKey(sa.HeaderID))
                    {
                        SalaryManager.SalaryHeader sh = DefaultHeaders[sa.HeaderID].Clone();
                        config.SalaryBreakup.Add(sh.Code.ToUpper(), sh);
                        sh.Amount = sa.Amount;
                        dedamt += sa.Amount;
                    }
                }
            }

            if (roundingHeader != null)
            {
                decimal netamt = earnamt - dedamt;
                decimal round = netamt % 10;
                roundingHeader.Amount = round != 0 ? 10 - round : 0;
            }
            return config;
        }

        Models.EmployeeSalaryConfig employeeSalaryConfig;
        internal void SetConfiguration(Models.EmployeeSalaryConfig item)
        {
            this.employeeSalaryConfig = item;
            SortedList<string, object> param = new SortedList<string, object>();
            param.Add("fromdate", fromdate);
            param.Add("todate", todate.AddDays(1));
            computedConfig.totalholiday = 0;
            computedConfig.sundayholiday = 0;
            computedConfig.holidayencashment = 0;
            computedConfig.totalsunday = SundayTotal();
            var query = @"select holidayon,leaveencashment,payable from holiday where holidayon >= @fromdate and holidayon < @todate and isactive=true";
            InfyPOS.Common.BaseData baseData = new BaseData() { Context = new UserClaim() { OrganizationID = organizationid } };
            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    Holiday holiday = new Holiday();
                    holiday.Date = reader.GetDateTime(0);
                    holiday.Encashment = (reader.IsDBNull(1) ? 0 : reader.GetInt32(1));
                    holiday.Wages = reader.IsDBNull(2) ? false : reader.GetBoolean(2);
                    if ((reader.IsDBNull(1) ? 0 : reader.GetInt32(1)) > 0)
                        computedConfig.holidayencashment = computedConfig.holidayencashment + reader.GetInt32(1);
                    if (reader.GetDateTime(0).DayOfWeek == DayOfWeek.Sunday)
                        computedConfig.sundayholiday++;
                    computedConfig.totalholiday++;
                    if (holiday.Wages)
                        computedConfig.payableholiday++;
                    holidayList.Add(holiday);
                }
            }


            if (((Models.EmployeeSalaryConfig.WorkDays_Mode)item.workdays_mode == Models.EmployeeSalaryConfig.WorkDays_Mode.AllDays))
            {
                computedConfig.WorkingDays = (int)todate.Subtract(fromdate).TotalDays + 1;
                if (item.workdays_exceptsunday)
                {
                    if (item.workdays_exceptholiday)
                    {
                        if (item.proratedaysbased)
                        {
                            computedConfig.WorkingDays -= (computedConfig.totalholiday);
                        }
                        else
                        {
                            computedConfig.WorkingDays -= (computedConfig.totalholiday - computedConfig.sundayholiday);
                        }
                    }
                    computedConfig.WorkingDays -= SundayTotal();
                }
                else if (item.workdays_exceptholiday)
                {
                    computedConfig.WorkingDays -= (computedConfig.totalholiday);
                }
            }
            else
            {
                computedConfig.WorkingDays = item.workdays_fixed;
            }

            if (((Models.EmployeeSalaryConfig.LeaveDaysType)item.leave_mode == Models.EmployeeSalaryConfig.LeaveDaysType.FixedDays) ||
                ((Models.EmployeeSalaryConfig.LeaveDaysType)item.leave_mode == Models.EmployeeSalaryConfig.LeaveDaysType.Prorate))
            {
                computedConfig.eligibleleave = item.leave_fixeddays;
                computedConfig.eligibleleaveprorate = true;
            }
            else if (item.leave_includesunday)
            {
                computedConfig.eligibleleave = SundayTotal();
                if (item.leave_includeholiday)
                    computedConfig.eligibleleave += (computedConfig.totalholiday - computedConfig.sundayholiday);
            }
            else if (item.leave_includeholiday)
            {
                computedConfig.eligibleleave = computedConfig.totalholiday;
            }

            computedConfig.totaldays = (int)todate.Subtract(fromdate).TotalDays + 1;
        }

        public class ComputedConfig
        {
            public int WorkingDays { get; set; }
            public int totalholiday { get; set; }
            public int sundayholiday { get; set; }
            public int holidayencashment { get; set; }
            public int eligibleleave { get; set; }
            public int totaldays { get; set; }
            public int totalsunday { get; set; }
            public int payableholiday { get; set; }
            public bool eligibleleaveprorate { get; set; }
        }
        public ComputedConfig computedConfig = new ComputedConfig();

        public EmployeeSalaryConfig Calculate(EmployeeSalaryConfig config, List<InfyPOS.Data.SalaryManager.ScheduleAmount> schConfig)
        {
            if (config.SalaryMode == 1)
                return CalculateDailywages(config, schConfig);
            return CalculateMonthly(config, schConfig);
        }


        public bool TestMonthly(InfyPOS.Models.Employeesalaryheader bconfig)
        {
            try
            {
                DateTime monthlastday = todate;
                decimal basicPay = bconfig.basicpay;
                decimal grosspay = bconfig.grosspay;

                EmployeeSalaryConfig config = new EmployeeSalaryConfig();
                config.grosspay = bconfig.grosspay;
                config.BasicPay = bconfig.basicpay;
                config.SetSalaryBreakup(false, false, bconfig.configuration_json, DefaultHeaders);

                if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.LeaveEncashment))
                {
                    config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.LeaveEncashment).Formula = "0";
                }

                if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.Incentives))
                {
                    config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.Incentives).Formula = "0";
                }

                var days = computedConfig.WorkingDays;
                basicPay = Math.Round(basicPay, 0);
                grosspay = Math.Round(grosspay, 0);
                MathParserTK.MathParser mathparser = new MathParserTK.MathParser(organizationid);
                mathparser.Specials.Add("bp", basicPay.ToString());
                mathparser.Specials.Add("advance", "0");
                mathparser.Specials.Add("gp", grosspay.ToString());
                mathparser.Specials.Add("ot", "0");
                mathparser.Specials.Add("round", "0");
                mathparser.Specials.Add("days", days.ToString());
                mathparser.Specials.Add("leavedays", "0");
                mathparser.Specials.Add("workdays", days.ToString());
                mathparser.Specials.Add("holidayabsent", "0");
                mathparser.Specials.Add("holidayleave", "0");

                if (config.SalaryBreakup.Values.ToList().Exists(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.LeaveEncashment))
                {
                    mathparser.Specials.Add("leaveencashment", config.LeaveEncashment.ToString("F0"));
                    config.SalaryBreakup.Values.ToList().Find(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.LeaveEncashment).Formula = config.LeaveEncashment.ToString("F0");
                }

                if (config.SalaryBreakup.Values.ToList().Exists(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.Incentives))
                {
                    mathparser.Specials.Add("incentives", config.incentive.ToString("F0"));
                    config.SalaryBreakup.Values.ToList().Find(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.Incentives).Formula = config.incentive.ToString("F0");
                }

                decimal earnamt = 0;
                decimal dedamt = 0;
                InfyPOS.Data.SalaryManager.SalaryHeader roundingHeader = null;
                foreach (KeyValuePair<string, InfyPOS.Data.SalaryManager.SalaryHeader> kv in config.SalaryBreakup)
                {

                    if (kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.BasicPay)
                        kv.Value.Formula = "BP";

                    if (kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.Rounding)
                    {
                        roundingHeader = kv.Value;
                        continue;
                    }
                    if (string.IsNullOrEmpty(kv.Value.Formula))
                        continue;

                    if (kv.Value.formulaKeys.Count > 0)
                    {
                        foreach (var key in kv.Value.formulaKeys)
                            kv.Value.Formula = kv.Value.Formula.Replace(key, "0.00");
                    }

                    if (kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.FixedAllowance)
                    {
                        var formulaparts = kv.Value.Formula.Split(new char[] { '-', ':' });
                        kv.Value.Amount = (decimal)mathparser.Parse(formulaparts[0], false);
                        if (formulaparts.Length == 2)
                        {
                            int mindays = 0;
                            if (int.TryParse(formulaparts[1], out mindays) && mindays > 0 &&
                                days < mindays)
                            {
                                kv.Value.Amount = 0;
                            }
                        }
                    }
                    else
                    {
                        double amt = 0;
                        double baseAmt = 0;
                        if (double.TryParse(kv.Value.Formula, out amt))
                        {
                            baseAmt = amt;
                            baseAmt = kv.Value.Code.Contains("ESI") ? Math.Ceiling(baseAmt) : Math.Round(baseAmt, 0);
                        }
                        else
                        {
                            amt = mathparser.Parse(kv.Value.Formula, false);
                            baseAmt = kv.Value.Code.Contains("ESI") ? Math.Ceiling(amt) : Math.Round(amt, 0);
                        }
                        kv.Value.Amount = (decimal)baseAmt;
                    }

                    //Math.Round((decimal)Math.Abs(baseAmt), 0);

                    if (DefaultHeaders.ContainsKey(kv.Value.HeaderID) && DefaultHeaders[kv.Value.HeaderID].Max > 0 &&
                        DefaultHeaders[kv.Value.HeaderID].Max < kv.Value.Amount)
                    {
                        kv.Value.Amount = DefaultHeaders[kv.Value.HeaderID].Max;
                    }

                    if (kv.Value.IsDeduction)
                        dedamt += kv.Value.Amount;
                    else
                        earnamt += kv.Value.Amount;

                    if (bconfig.configuration.Exists(e => e.id == kv.Value.HeaderID))
                        bconfig.configuration.Find(e => e.id == kv.Value.HeaderID).value = kv.Value.Amount;
                }

            }
            catch (Exception exp)
            {
                return false;
            }
            return true;
        }

        public EmployeeSalaryConfig CalculateMonthly(EmployeeSalaryConfig config, List<SalaryManager.ScheduleAmount> schConfig)
        {
            InitializeContext(config);
            ComputeWorkDays(config);
            ApplyJoiningAndRelievingAdjustments(config);
            ComputeLeavesAndAbsents(config);
            ComputeLeaveEncashment(config);
            ComputeHolidaySalary(config);
            ComputeOvertime(config);
            ComputeFine(config);
            ProrateBasicAndGross(config);
            EvaluateSalaryBreakup(config, schConfig);
            FinalizeTotals(config);
            return config;
        }

        private void CalculateLeavesAndAbsents(EmployeeSalaryConfig config)
        {
            decimal absentDays = config.LeaveDays - computedConfig.eligibleleave;

            if (config.proratedaysbased || employeeSalaryConfig.workdays_mode != 2)
            {
                config.AbsentDays = absentDays;
                return;
            }

            if (employeeSalaryConfig.leave_absentadjustment)
            {
                decimal eligibleLeave = Math.Max(computedConfig.eligibleleave - config.LeaveDays, 0);
                absentDays = config.LeaveDays - eligibleLeave;
            }

            if (employeeSalaryConfig.workdays_exceptsunday)
                absentDays -= config.SundayLeaves;

            if (employeeSalaryConfig.workdays_exceptholiday)
                absentDays -= config.HolidayLeaves;

            config.AbsentDays = Math.Max(0, absentDays);
        }

        class SalaryFormulaEvaluator
        {
            private MathParser parser;

            public SalaryFormulaEvaluator(Dictionary<string, string> variables)
            {
                parser = new MathParser(0);
                foreach (var kv in variables)
                    parser.Specials.Add(kv.Key, kv.Value);
            }

            public decimal Evaluate(string formula) => (decimal)parser.Parse(formula, false);
        }

        private void InitializeContext(EmployeeSalaryConfig config)
        {
            config.WorkDayCount = computedConfig.WorkingDays;
            config.LeaveTaken = config.LeaveDays;
            config.fullgrosspay = config.grosspay;
        }

        private void ComputeWorkDays(EmployeeSalaryConfig config)
        {
            int totalDays = (int)todate.Subtract(fromdate).TotalDays + 1;
            config.TotalDaysInMonth = totalDays;
        }

        private void ComputeLeavesAndAbsents(EmployeeSalaryConfig config)
        {
            decimal absentDays = 0;

            //if (employeeSalaryConfig.workdays_exceptsunday)
            //    absentDays += config.SundayLeaves;

            if (computedConfig.eligibleleaveprorate)
            {
                var eligibleleave = Math.Floor((config.WorkDayCount - config.LeaveDays) / 7);
                if (employeeSalaryConfig.workdays_exceptholiday)
                    absentDays += config.holidayworking;

                if (employeeSalaryConfig.leave_fixeddays < eligibleleave)
                {
                    eligibleleave = employeeSalaryConfig.leave_fixeddays;
                }

                if (eligibleleave < config.LeaveDays)
                {
                    absentDays += config.LeaveDays - eligibleleave;
                }
                else if (eligibleleave - config.LeaveDays > absentDays)
                {
                    absentDays = 0;
                }
            }
            else
            {
                var paydays = config.TotalDaysInMonth - config.LeaveDays;
                if (paydays < config.WorkDayCount)
                {
                    absentDays = config.WorkDayCount - paydays;
                }
                //if (employeeSalaryConfig.workdays_exceptholiday)
                //    absentDays += config.holidayworking;

                //if (employeeSalaryConfig.workdays_exceptsunday)
                //    absentDays -= (config.SundayLeaves);

                //if (computedConfig.eligibleleave < config.LeaveDays)
                //{
                //    absentDays += config.LeaveDays - computedConfig.eligibleleave;
                //}
                //else
                //{
                //    if (computedConfig.eligibleleave - config.LeaveDays > absentDays)
                //    {
                //        absentDays = (computedConfig.eligibleleave - config.LeaveDays-absentDays)*-1;
                //    }
                //}
            }
            //decimal absentDays = config.LeaveDays - computedConfig.eligibleleave;
            ////if (!config.proratedaysbased)
            ////{
            ////    if (employeeSalaryConfig.workdays_mode == 2)
            ////    {
            ////        if (employeeSalaryConfig.workdays_exceptsunday)
            ////            absentDays -= config.SundayLeaves;

            ////        if (employeeSalaryConfig.workdays_exceptholiday)
            ////            absentDays -= config.HolidayLeaves;
            ////    }
            ////}

            //if (employeeSalaryConfig.leave_absentadjustment && absentDays>0)
            //{
            //    decimal eligibleLeave = Math.Max(computedConfig.eligibleleave - config.LeaveDays, 0);
            //    absentDays = config.LeaveDays - eligibleLeave;
            //}

            config.AbsentDays = absentDays;
        }

        private void ApplyJoiningAndRelievingAdjustments(EmployeeSalaryConfig config)
        {
            decimal adjustment = 0;

            if (config.DateOfJoin >= fromdate && config.DateOfJoin <= todate)
            {
                adjustment += (decimal)config.DateOfJoin.Subtract(fromdate).TotalDays;
                //if (!config.proratedaysbased)
                //{
                //    if (employeeSalaryConfig.workdays_exceptsunday)
                //    {
                //        adjustment -= SundayTotal(config.DateOfJoin, false);
                //    }

                //    if (employeeSalaryConfig.workdays_exceptholiday)
                //    {
                //        adjustment -= HolidayTotal(config.DateOfJoin, false, false, true);
                //    }
                //}
            }

            if (config.Hasleft && config.Relievingdate >= fromdate && config.Relievingdate <= todate)
            {
                adjustment += (decimal)todate.Subtract(config.Relievingdate).TotalDays;
                //if (!config.proratedaysbased)
                //{
                //    if (employeeSalaryConfig.workdays_exceptsunday)
                //        adjustment -= SundayTotal(config.Relievingdate, true);

                //    if (employeeSalaryConfig.workdays_exceptholiday)
                //        adjustment -= HolidayTotal(config.Relievingdate, true, false, true);
                //}
            }

            config.AdjustedJoiningResigning = adjustment;
        }

        private void ComputeLeaveEncashment(EmployeeSalaryConfig config)
        {
            int permanentextra = 0;
            if (config.workingmode == 3 && employeeSalaryConfig.encashment_permanent_extra > 0)
            {
                decimal eligibleDays = config.TotalDaysInMonth - config.LeaveDays - config.AdjustedJoiningResigning;
                if (eligibleDays >= employeeSalaryConfig.encashment_permanent_minimum)
                {
                    permanentextra = employeeSalaryConfig.encashment_permanent_extra;
                }
            }
            decimal baseValue = config.fullgrosspay;
            if (ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashmentmode == 1)
            {
                baseValue = config.BasicPay;
            }
            else if (config.fixedbasic + config.fixedda > config.fullgrosspay)
            {
                baseValue = config.fixedbasic + config.fixedda;
            }
            config.LeaveEncashmentDays = employeeSalaryConfig.encashment_fixeddays;
            config.LeaveEncashmentLeaveSwapDays = 0;
            config.LeaveEncashment = 0;

            // Eligibility check
            if (!employeeSalaryConfig.encashment_allemployees)
            {
                if (employeeSalaryConfig.encashment_eligiblemonth > 0 &&
                    config.DateOfJoin.AddMonths(employeeSalaryConfig.encashment_eligiblemonth) > fromdate)
                {
                    config.LeaveEncashmentDays = 0;
                }

                if (employeeSalaryConfig.encashment_swapmonth > 0 &&
                    config.DateOfJoin.AddMonths(employeeSalaryConfig.encashment_swapmonth) > fromdate)
                {
                    config.LeaveEncashmentLeaveSwapDays = employeeSalaryConfig.encashment_fixeddays;
                }
            }


            if (employeeSalaryConfig.encashment_mode == 3 &&
                config.workingmode == 3 &&
                employeeSalaryConfig.encashment_perday > 0)
            {
                decimal eligibleDays = config.TotalDaysInMonth - config.LeaveDays - config.AdjustedJoiningResigning;
                config.LeaveEncashmentDays = Math.Floor(eligibleDays / employeeSalaryConfig.encashment_perday);
            }

            if (config.LeaveEncashmentDays <= 0 && config.LeaveEncashmentLeaveSwapDays <= 0)
                return;

            decimal maxEncashmentDays = 0;

            if (employeeSalaryConfig.encashment_mode == 1)
            {
                decimal eligibleDays = config.TotalDaysInMonth - config.LeaveDays - config.AdjustedJoiningResigning;
                //if (employeeSalaryConfig.encashment_exceptsunday)
                //    eligibleDays -= config.SundayLeaves+ config.SundayLeaves;
                if (employeeSalaryConfig.encashment_exceptholiday)
                    eligibleDays -= config.holidayworking;

                if (employeeSalaryConfig.encashment_perday > 0)
                    maxEncashmentDays = eligibleDays / employeeSalaryConfig.encashment_perday;

                if (config.LeaveEncashmentDays > maxEncashmentDays + permanentextra)
                    config.LeaveEncashmentDays = Math.Floor(maxEncashmentDays + permanentextra);

                if (config.workingmode == 3 && permanentextra > 0)
                {
                    config.LeaveEncashmentDays += permanentextra;
                }
            }
            else
            {
                decimal maxDays = config.TotalDaysInMonth -
                                  HolidayTotal(config.DateOfJoin, true, false, false) -
                                  config.LeaveEncashmentDays;

                if (maxDays > (config.TotalDaysInMonth - config.AdjustedJoiningResigning))
                    config.LeaveEncashmentDays = 0;
            }

            // Adjust with absent
            if (employeeSalaryConfig.encashment_adjustabsent && config.LeaveEncashmentDays > 0 && config.LeaveDays > 0)
            {
                if (config.LeaveEncashmentDays >= config.AbsentDays)
                    config.LeaveEncashmentDays -= config.LeaveDays;
                else
                    config.LeaveEncashmentDays = 0;
            }

            // Apply swap limit
            if (employeeSalaryConfig.encashment_perday > 0 &&
                config.LeaveEncashmentLeaveSwapDays > maxEncashmentDays + permanentextra)
            {
                config.LeaveEncashmentLeaveSwapDays = maxEncashmentDays + permanentextra;
            }


            // Calculate value
            int noOfDays = GetEncashmentWorkingDays();
            if (noOfDays > 0)
            {
                config.LeaveEncashment = Math.Round(baseValue / noOfDays * config.LeaveEncashmentDays, 0);
                if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.LeaveEncashment))
                {
                    config.SalaryBreakup.Values
                        .First(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.LeaveEncashment)
                        .Formula = config.LeaveEncashment.ToString();
                }
            }
        }

        private int GetEncashmentWorkingDays()
        {
            int totalDays = (int)todate.Subtract(fromdate).TotalDays + 1;
            if (employeeSalaryConfig.encashment_workdays_mode == 1)
            {
                return employeeSalaryConfig.encashment_workdays_fixed;
            }

            if (employeeSalaryConfig.encashment_workdays_exceptsunday)
                totalDays -= SundayTotal(todate, false);
            if (employeeSalaryConfig.encashment_workdays_exceptholiday)
                totalDays -= HolidayTotal(todate, false, false, employeeSalaryConfig.encashment_workdays_exceptsunday);

            return totalDays;
        }

        private void ComputeHolidaySalary(EmployeeSalaryConfig config)
        {
            if (employeeSalaryConfig.holiday_permanentonly && config.workingmode != 3) return;

            config.HolidaySalaryDays = computedConfig.payableholiday + computedConfig.holidayencashment;

            // Adjust for new joiners
            if (config.DateOfJoin >= fromdate && config.DateOfJoin <= todate)
                config.HolidaySalaryDays -= HolidayTotal(config.DateOfJoin, false, true, employeeSalaryConfig.holiday_exceptsunday);

            // Adjust for employees who left
            if (config.Hasleft && config.Relievingdate >= fromdate && config.Relievingdate <= todate)
                config.HolidaySalaryDays -= HolidayTotal(config.Relievingdate, true, true, employeeSalaryConfig.holiday_exceptsunday);

            config.HolidaySalaryDays -= config.HolidayLeaves;

            //config.HolidaySalaryDays -= config.HolidayLeaveEncashment;

            // Additional absent handling
            if (employeeSalaryConfig.holiday_exceptsunday && !employeeSalaryConfig.leave_includesunday && !config.proratedaysbased)
            {
                config.Additionalabsentdays += config.sundayworking;
            }

            if (employeeSalaryConfig.holiday_exceptholiday && !employeeSalaryConfig.leave_includeholiday && !config.proratedaysbased)
            {
                config.Additionalabsentdays += (config.holidayworking - config.holidaysundayworking);
            }

            // Compute holiday salary if any days remain
            if (config.HolidaySalaryDays > 0)
            {
                int noOfDays = GetHolidayWorkingDays();

                if (employeeSalaryConfig.holiday_adjustabsent && config.HolidayLeaves > 0)
                {
                    config.HolidaySalaryDays -= config.HolidayLeaves;
                }

                if (noOfDays > 0)
                {
                    decimal baseValue = config.fullgrosspay;
                    if (ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashmentmode == 1)
                        baseValue = config.BasicPay;
                    else if (config.fixedbasic + config.fixedda > config.fullgrosspay)
                        baseValue = config.fixedbasic + config.fixedda;

                    config.HolidaySalary = Math.Round(baseValue / noOfDays * config.HolidaySalaryDays, 0, MidpointRounding.ToEven);

                    var holidayHeader = config.SalaryBreakup.Values.FirstOrDefault(e =>
                        e.HeaderType == SalaryManager.SalaryHeaderTypes.HolidaySalary);

                    if (holidayHeader != null)
                    {
                        holidayHeader.Amount = config.HolidaySalary;
                        holidayHeader.Formula = config.HolidaySalary.ToString();
                    }
                }
            }
        }

        private int GetHolidayWorkingDays()
        {
            int totalDays = (int)todate.Subtract(fromdate).TotalDays + 1;

            if (employeeSalaryConfig.holiday_mode == 1)
                return employeeSalaryConfig.holiday_fixeddays;

            if (employeeSalaryConfig.holiday_exceptsunday)
                totalDays -= SundayTotal(todate, false);

            if (employeeSalaryConfig.holiday_exceptholiday)
                totalDays -= HolidayTotal(todate, false, false, employeeSalaryConfig.holiday_exceptsunday);

            return totalDays;
        }

        private void ComputeFineDeductions(EmployeeSalaryConfig config)
        {
            decimal fine = 0;

            // 1. Deduction for unapproved leave days
            if (employeeSalaryConfig.fine_leave &&
                employeeSalaryConfig.fine_leave_perday > 0 &&
                config.LeaveDays > config.ApprovedLeaves)
            {
                decimal unapprovedLeaves = config.LeaveDays - config.ApprovedLeaves;
                fine += unapprovedLeaves * employeeSalaryConfig.fine_leave_perday;
            }

            // 2. Deduction for late check-ins
            if (employeeSalaryConfig.fine_late &&
                employeeSalaryConfig.fine_late_fee > 0 &&
                config.latecheckin > 0)
            {
                fine += config.latecheckin * employeeSalaryConfig.fine_late_fee;
            }

            // 3. Apply to FineDeduction header if present
            var fineHeader = config.SalaryBreakup.Values
                .FirstOrDefault(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.FineDeduction);

            if (fineHeader != null)
            {
                fineHeader.Formula = fine.ToString("F0");
                fineHeader.Amount = fine;
            }
        }

        private void ComputeFine(EmployeeSalaryConfig config)
        {
            decimal finePay = 0;

            bool useFormula = employeeSalaryConfig.fine_formula &&
                              config.fullgrosspay > 0 &&
                              config.BasicPay > 0;

            if (useFormula)
            {
                if (config.workinghours == 0) config.workinghours = (decimal) 9.5;
                decimal totalOTHours = Math.Round(
                    ((config.earlycheckout + config.latecheckin) * employeeSalaryConfig.fine_multiplier) / 60, 2);

                if (totalOTHours > 0)
                {
                    decimal paySource = employeeSalaryConfig.fine_source == 1 ? config.fullgrosspay : config.BasicPay;
                    decimal perHourPay = paySource / employeeSalaryConfig.fine_totaldays / config.workinghours;
                    finePay = Math.Round(perHourPay * totalOTHours, 0);
                }
                config.OverTimePay = finePay;

                var fineHeader = config.SalaryBreakup.Values
                    .FirstOrDefault(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.FineDeduction);

                if (fineHeader != null)
                {
                    fineHeader.Formula = finePay.ToString("F0");
                    fineHeader.Amount = finePay;
                }
            }
            else
            {
                ComputeFineDeductions(config);
            }
        }

        private void ComputeOvertime(EmployeeSalaryConfig config)
        {
            decimal overtimePay = 0;

            bool useFormula = employeeSalaryConfig.overtime_formula &&
                              config.fullgrosspay > 0 &&
                              config.BasicPay > 0;

            if (useFormula)
            {
                if (config.workinghours == 0) config.workinghours = (decimal)9.5;
                var overtime = ((employeeSalaryConfig.overtime_morning ? config.morningovertime : 0) +
                                (employeeSalaryConfig.overtime_evening ? config.eveningovertime : 0));

                decimal totalOTHours = Math.Round(
                    ((overtime) * employeeSalaryConfig.overtime_multiplier) / 60, 2);

                if (totalOTHours > 0)
                {
                    decimal paySource = employeeSalaryConfig.overtime_source == 1 ? config.fullgrosspay : config.BasicPay;
                    decimal perHourPay = paySource / employeeSalaryConfig.overtime_totaldays / config.workinghours;
                    overtimePay = Math.Round(perHourPay * totalOTHours, 0);
                }
            }
            else
            {
                if (employeeSalaryConfig.overtime_morning && employeeSalaryConfig.overtime_morning_perminute > 0)
                {
                    overtimePay += config.morningovertime * employeeSalaryConfig.overrime_morning_fee;
                }

                if (employeeSalaryConfig.overtime_evening && employeeSalaryConfig.overtime_evening_perminute > 0)
                {
                    overtimePay += config.eveningovertime * employeeSalaryConfig.overrime_evening_fee;
                }
            }

            config.OverTimePay = overtimePay;

            var otHeader = config.SalaryBreakup.Values
                .FirstOrDefault(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.Overtime);

            if (otHeader != null)
            {
                otHeader.Formula = overtimePay.ToString("F0");
                otHeader.Amount = overtimePay;
            }
        }

        private void ProrateBasicAndGross(EmployeeSalaryConfig config)
        {
            decimal basicPay = config.BasicPay;
            decimal grossPay = config.fullgrosspay;
            decimal absentDays = config.AbsentDays;
            decimal adjustedDays = config.AdjustedJoiningResigning;
            int totalDays = config.TotalDaysInMonth;
            decimal computedWorkDays = 0;

            if (employeeSalaryConfig.workdays_mode == 1) // Fixed workdays
            {
                var perDayBasic = basicPay / employeeSalaryConfig.workdays_fixed;
                var perDayGross = grossPay / employeeSalaryConfig.workdays_fixed;

                computedWorkDays = totalDays - (adjustedDays + absentDays);

                basicPay = Math.Round(perDayBasic * computedWorkDays, 0);
                grossPay = Math.Round(perDayGross * computedWorkDays, 0);
            }
            else if (config.proratedaysbased)
            {
                if (absentDays < 0) absentDays = 0;

                computedWorkDays = totalDays - (adjustedDays + absentDays);
                if (employeeSalaryConfig.workdays_exceptholiday)
                    computedWorkDays -= config.holidayworking;

                config.Paydays = computedWorkDays;

                basicPay = Math.Round(config.BasicPay / config.WorkDayCount * computedWorkDays, 0);
                grossPay = Math.Round(config.fullgrosspay / config.WorkDayCount * computedWorkDays, 0);
            }
            else
            {

                var paydays = config.TotalDaysInMonth - (config.LeaveDays + adjustedDays + (employeeSalaryConfig.holiday_exceptholiday ? config.holidayworking : 0));
                basicPay = Math.Round(((config.BasicPay / config.WorkDayCount) * paydays), 0);
                grossPay = Math.Round(((config.fullgrosspay / config.WorkDayCount) * paydays), 0);
                computedWorkDays = paydays;
                config.Paydays = computedWorkDays;

                //if (absentDays > 0 || adjustedDays > 0)
                //{
                //    var deduction = adjustedDays + absentDays;

                //    basicPay = Math.Round(basicPay - ((config.BasicPay / config.WorkDayCount) * deduction), 0);
                //    grossPay = Math.Round(grossPay - ((config.fullgrosspay / config.WorkDayCount) * deduction), 0);

                //    computedWorkDays = config.WorkDayCount - deduction;
                //    config.Paydays = computedWorkDays;
                //}
                //else
                //{
                //    if (computedConfig.eligibleleave > config.LeaveDays + config.holidayworking)
                //    {
                //        var deduction = config.WorkDayCount + (computedConfig.eligibleleave - config.LeaveDays) - config.holidayworking;
                //        basicPay = Math.Round(((config.BasicPay / config.WorkDayCount) * deduction), 0);
                //        grossPay = Math.Round(((config.fullgrosspay / config.WorkDayCount) * deduction), 0);
                //        computedWorkDays = deduction;
                //        config.Paydays = computedWorkDays;
                //    }
                //    else
                //    {
                //        computedWorkDays = config.WorkDayCount;
                //        config.Paydays = computedWorkDays;
                //    }
                //}
            }

            config.BasicPay = basicPay;
            config.grosspay = grossPay;
            config.ComputedWorkDays = computedWorkDays;
            config.AcceptedWorkDays = computedWorkDays;
        }

        private void EvaluateSalaryBreakup(EmployeeSalaryConfig config, List<SalaryManager.ScheduleAmount> schConfig)
        {
            decimal basicplusda = 0;
            if (config.SalaryBreakup.ToList().Exists(x => x.Value.HeaderType == SalaryManager.SalaryHeaderTypes.ProvidentFund && x.Value.Max < 0))
            {
                var basic = config.SalaryBreakup.ToList().Find(x => x.Value.HeaderType == SalaryManager.SalaryHeaderTypes.BasicPay);
                var da = config.SalaryBreakup.ToList().Find(x => x.Value.HeaderType == SalaryManager.SalaryHeaderTypes.DearnessAllowance);

                decimal amount = 0;
                if (decimal.TryParse(basic.Value.Formula, out amount))
                    basicplusda += amount;
                if (decimal.TryParse(da.Value.Formula, out amount))
                    basicplusda += amount;
            }

            var parser = new MathParserTK.MathParser(organizationid);
            AddStandardFormulasToParser(parser, config);

            // Add scheduled components (if any)
            ApplyScheduledAmounts(parser, config, schConfig);

            decimal earnTotal = 0;
            decimal dedTotal = 0;
            SalaryManager.SalaryHeader roundingHeader = null;
            SalaryManager.SalaryHeader specialdeduction = null;

            foreach (var kv in config.SalaryBreakup.OrderBy(e => e.Value.OrderID))
            {
                var header = kv.Value;
                if (header.HeaderType == SalaryManager.SalaryHeaderTypes.Rounding)
                {
                    roundingHeader = header;
                    continue;
                }

                if (header.HeaderType == SalaryManager.SalaryHeaderTypes.SpecialDeduction)
                {
                    if (header.Formula.Contains("AUTOCOMPUTE") &&
                        config.fixedbasic + config.fixedda > config.fullgrosspay)
                    {
                        specialdeduction = header;
                        break;
                    }
                }


                if (string.IsNullOrEmpty(header.Formula))
                    continue;

                decimal amount = 0;

                if (decimal.TryParse(header.Formula, out amount))
                {
                    amount = ApplyWorkdayProration(config, amount, header);
                }
                else
                {
                    amount = (decimal)parser.Parse(header.Formula, false);
                }

                header.Amount = header.Code.Contains("ESI") ? Math.Ceiling(amount) : Math.Round(amount, 0);

                if (DefaultHeaders.ContainsKey(kv.Value.HeaderID) && DefaultHeaders[kv.Value.HeaderID].Max > 0 &&
                    DefaultHeaders[kv.Value.HeaderID].Max < kv.Value.Amount)
                {
                    kv.Value.Amount = DefaultHeaders[kv.Value.HeaderID].Max;
                }


                if (DefaultHeaders.ContainsKey(kv.Value.HeaderID) && DefaultHeaders[kv.Value.HeaderID].Max < 0)
                {
                    var grosspercentage = Math.Abs(DefaultHeaders[kv.Value.HeaderID].Max);
                    if (basicplusda * grosspercentage / 100 < kv.Value.Amount)
                        kv.Value.Amount = Math.Round(basicplusda * grosspercentage / 100, MidpointRounding.ToEven);
                }

                if (!parser.Specials.ContainsKey(header.Code.ToLower()))
                    parser.Specials.Add(header.Code.ToLower(), header.Amount.ToString("F0"));

                if (header.IsDeduction)
                    dedTotal += header.Amount;
                else
                    earnTotal += header.Amount;
            }

            if (specialdeduction != null)
            {
                if (config.fixedbasic + config.fixedda > config.fullgrosspay)
                {
                    var stddeduction = config.fixedbasic + config.fixedda - config.fullgrosspay;
                    var paydays = config.TotalDaysInMonth - (config.LeaveDays + config.AdjustedJoiningResigning + (employeeSalaryConfig.holiday_exceptholiday ? config.holidayworking : 0));
                    var fullstddeduction = (stddeduction / config.WorkDayCount) * paydays;

                    if (employeeSalaryConfig.holiday_exceptholiday && config.holidayworking > 0)
                        fullstddeduction += (stddeduction / config.WorkDayCount) * config.holidayworking;

                    if (config.LeaveEncashmentDays > 0)
                        fullstddeduction += (stddeduction / 30) * config.LeaveEncashmentDays;

                    specialdeduction.Amount = Math.Round(fullstddeduction, 0, MidpointRounding.ToEven);
                    dedTotal += specialdeduction.Amount;
                }
            }

            // Final rounding
            if (roundingHeader != null)
            {
                decimal netPay = earnTotal - dedTotal;
                decimal round = netPay % 10;
                roundingHeader.Amount = round != 0 ? 10 - round : 0;
            }
        }

        private void AddStandardFormulasToParser(MathParserTK.MathParser parser, EmployeeSalaryConfig config)
        {
            parser.Specials.Add("bp", config.BasicPay.ToString("F0"));
            parser.Specials.Add("gp", config.grosspay.ToString("F0"));
            parser.Specials.Add("fgp", config.fullgrosspay.ToString("F0"));
            parser.Specials.Add("ot", Math.Round(config.OverTimePay, 0).ToString());
            parser.Specials.Add("leaveencashment", config.LeaveEncashment.ToString("F0"));
            parser.Specials.Add("holidayencashment", config.HolidaySalary.ToString("F0"));
            parser.Specials.Add("leavedays", (config.AbsentDays + config.AdjustedJoiningResigning).ToString());
            parser.Specials.Add("Absentdays", (config.AbsentDays + config.AdjustedJoiningResigning).ToString());
            parser.Specials.Add("workdays", config.WorkDayCount.ToString());
            parser.Specials.Add("days", config.WorkDayCount.ToString());
            parser.Specials.Add("computedworkingdays", config.ComputedWorkDays.ToString());
            parser.Specials.Add("sundayworking", config.sundayworking.ToString());
            parser.Specials.Add("holidayworking", config.holidayworking.ToString());
        }

        private void ApplyScheduledAmounts(MathParserTK.MathParser parser, EmployeeSalaryConfig config, List<SalaryManager.ScheduleAmount> schConfig)
        {
            if (schConfig == null || schConfig.Count == 0)
                return;

            foreach (var sa in schConfig)
            {
                if (!DefaultHeaders.ContainsKey(sa.HeaderID))
                    continue;

                var headerCode = DefaultHeaders[sa.HeaderID].Code.ToUpper();

                if (config.SalaryBreakup.ContainsKey(headerCode))
                {
                    var header = config.SalaryBreakup[headerCode];
                    header.Amount += sa.Amount;
                    header.Formula = header.Amount.ToString("F0");
                }
                else
                {
                    var newHeader = DefaultHeaders[sa.HeaderID].Clone();
                    newHeader.Amount = sa.Amount;
                    newHeader.Formula = sa.Amount.ToString("F0");
                    config.SalaryBreakup.Add(headerCode, newHeader);
                }

                parser.Specials[headerCode.ToLower()] = config.SalaryBreakup[headerCode].Amount.ToString("F0");
            }
        }

        private decimal ApplyWorkdayProration(EmployeeSalaryConfig config, decimal baseAmt, SalaryManager.SalaryHeader header)
        {
            if (header.IsFullAmount)
                return baseAmt;

            if (employeeSalaryConfig.workdays_mode == 1)
            {
                return baseAmt / employeeSalaryConfig.workdays_fixed * config.ComputedWorkDays;
            }
            else if (config.proratedaysbased)
            {
                return baseAmt / config.WorkDayCount * config.Paydays;
            }
            else
            {
                if (employeeSalaryConfig.workdays_exceptholiday && employeeSalaryConfig.workdays_exceptsunday)
                {
                    var paydays = config.TotalDaysInMonth - (config.LeaveDays + config.AdjustedJoiningResigning + (employeeSalaryConfig.holiday_exceptholiday ? config.holidayworking : 0));
                    return baseAmt / config.WorkDayCount * paydays;
                }

                if (computedConfig.eligibleleave > config.LeaveDays + config.AdjustedJoiningResigning + (employeeSalaryConfig.holiday_exceptholiday ? config.holidayworking : 0))
                {
                    var deduction = config.WorkDayCount + (computedConfig.eligibleleave - config.LeaveDays) - (employeeSalaryConfig.holiday_exceptholiday ? config.holidayworking : 0) - config.AdjustedJoiningResigning;
                    return Math.Round(((baseAmt / config.WorkDayCount) * deduction), 0);
                }
                if ((config.AdjustedJoiningResigning + config.AbsentDays) > config.WorkDayCount)
                    return 0;
                return baseAmt - (baseAmt / config.WorkDayCount * (config.AdjustedJoiningResigning + config.AbsentDays));
            }
        }

        private void FinalizeTotals(EmployeeSalaryConfig config)
        {
            // Adjust final working day counts
            if (config.AdjustedJoiningResigning > 0)
            {
                //config.WorkDayCount -= config.AdjustedJoiningResigning;
                config.AcceptedWorkDays = config.WorkDayCount - config.AdjustedJoiningResigning;
            }

            // Final fallback if not set
            if (config.AcceptedWorkDays <= 0 && config.ComputedWorkDays > 0)
            {
                config.AcceptedWorkDays = config.ComputedWorkDays;
            }

            // Ensure absent days stored cleanly
            config.AbsentDays = Math.Max(0, config.AbsentDays);
        }


        //public EmployeeSalaryConfig CalculateMonthly(EmployeeSalaryConfig config,
        //    List<InfyPOS.Data.SalaryManager.ScheduleAmount> schConfig)
        //{
        //    int totaldays = (int)todate.Subtract(fromdate).TotalDays + 1;
        //    var days = computedConfig.WorkingDays;
        //    config.WorkDayCount = computedConfig.WorkingDays;
        //    config.LeaveTaken = config.LeaveDays;
        //    decimal leavedayde = config.LeaveDays;
        //    DateTime monthlastday = todate;
        //    decimal basicPay = config.BasicPay;
        //    decimal grosspay = config.grosspay;
        //    decimal ctc = 0;
        //    decimal adjustedJoiningResigning = 0;
        //    decimal absentdays = config.LeaveDays - computedConfig.eligibleleave;

        //    if (!config.proratedaysbased)
        //    {
        //        if (employeeSalaryConfig.workdays_mode == 2 && employeeSalaryConfig.workdays_exceptsunday && employeeSalaryConfig.workdays_exceptholiday)
        //        {
        //            absentdays = config.LeaveDays;
        //        }
        //    }
        //    else
        //    {
        //        absentdays = config.LeaveDays;
        //    }
        //    if (employeeSalaryConfig.leave_absentadjustment)
        //    {
        //        if (employeeSalaryConfig.workdays_mode == 1)
        //        {
        //            var eligible = computedConfig.eligibleleave - config.LeaveDays;
        //            if (eligible <= 0) eligible = 0;
        //            absentdays = config.LeaveDays - eligible;
        //        }
        //        else
        //        {
        //            if ((config.WorkDayCount - config.LeaveDays) / 7 < absentdays)
        //            {
        //                var eligible = Math.Floor((config.WorkDayCount - config.LeaveDays) / 7);
        //                absentdays = config.LeaveDays - eligible;
        //            }
        //        }
        //    }

        //    //if (employeeSalaryConfig.holiday_exceptsunday && employeeSalaryConfig.holiday_exceptholiday)
        //    //{
        //    //    absentdays = config.LeaveDays - config.SundayLeaves - config.HolidaySundayLeaves;
        //    //}

        //    config.AbsentDays = absentdays;
        //    decimal workingdays = 0;
        //    if ((config.DateOfJoin >= fromdate && config.DateOfJoin <= todate) ||
        //        (config.Relievingdate >= fromdate && (config.Hasleft && config.Relievingdate <= todate)))
        //    {
        //        absentdays = config.LeaveDays;


        //        if (config.DateOfJoin >= fromdate && config.DateOfJoin <= todate)
        //        {
        //            TimeSpan experiance = monthlastday.Subtract(config.DateOfJoin);
        //            adjustedJoiningResigning += (decimal)config.DateOfJoin.Subtract(fromdate).TotalDays;

        //            if (!config.proratedaysbased)
        //            {
        //                if (employeeSalaryConfig.workdays_mode == 2 && employeeSalaryConfig.workdays_exceptsunday)
        //                {
        //                    adjustedJoiningResigning -= SundayTotal(config.DateOfJoin, false);
        //                }
        //                if (employeeSalaryConfig.workdays_mode == 2 && employeeSalaryConfig.workdays_exceptholiday)
        //                {
        //                    adjustedJoiningResigning -= HolidayTotal(config.DateOfJoin, false, false, true);
        //                }
        //            }
        //            //if (employeeSalaryConfig.leave_proportionate)
        //            //{
        //            //    if (((Models.EmployeeSalaryConfig.LeaveDaysType)employeeSalaryConfig.leave_mode == Models.EmployeeSalaryConfig.LeaveDaysType.FixedDays))
        //            //    {
        //            //        adjustjoiningresigningleave += ((int)(monthlastday.Subtract(fromdate).TotalDays / experiance.TotalDays * 100));
        //            //        absentdays -= ((int)(monthlastday.Subtract(fromdate).TotalDays / experiance.TotalDays * 100));
        //            //    }
        //            //}
        //        }

        //        if (config.Hasleft && config.Relievingdate >= fromdate && config.Relievingdate <= todate)
        //        {
        //            //TimeSpan experiance = config.Relievingdate.Subtract(fromdate);
        //            adjustedJoiningResigning += (decimal)todate.Subtract(config.Relievingdate).TotalDays;
        //            if (!config.proratedaysbased)
        //            {
        //                if (employeeSalaryConfig.workdays_mode == 2 && employeeSalaryConfig.workdays_exceptsunday)
        //                {
        //                    adjustedJoiningResigning -= SundayTotal(config.Relievingdate, true);
        //                }

        //                if (employeeSalaryConfig.workdays_mode == 2 && employeeSalaryConfig.workdays_exceptholiday)
        //                {
        //                    adjustedJoiningResigning -= HolidayTotal(config.Relievingdate, true, false, true);
        //                }
        //            }
        //            //if (config.DateOfJoin >= fromdate && config.DateOfJoin <= todate)
        //            //    experiance = config.Relievingdate.Subtract(config.DateOfJoin);

        //            monthlastday = config.Relievingdate;

        //            //if (employeeSalaryConfig.leave_proportionate)
        //            //{
        //            //    if (((Models.EmployeeSalaryConfig.LeaveDaysType)employeeSalaryConfig.leave_mode == Models.EmployeeSalaryConfig.LeaveDaysType.FixedDays))
        //            //    {
        //            //        adjustjoiningresigningleave += ((int)(monthlastday.Subtract(fromdate).TotalDays / experiance.TotalDays * 100));
        //            //        absentdays -= ((int)(monthlastday.Subtract(fromdate).TotalDays / experiance.TotalDays * 100));
        //            //    }
        //            //}
        //        }

        //    }
        //    workingdays = (totaldays - absentdays);

        //    if (!config.proratedaysbased)
        //    {
        //        if (employeeSalaryConfig.workdays_mode == 2 && employeeSalaryConfig.workdays_exceptsunday)
        //        {
        //            absentdays -= config.SundayLeaves;
        //            absentdays -= (config.HolidayAbsent - config.HolidaySundayAbsent);
        //        }

        //        if (employeeSalaryConfig.workdays_mode == 2 && employeeSalaryConfig.workdays_exceptholiday)
        //        {
        //            absentdays -= config.HolidayLeaves;
        //        }
        //    }

        //    decimal acceptedleavedays = 0;
        //    if (computedConfig.eligibleleave > config.LeaveDays)
        //    {
        //        acceptedleavedays = 0;
        //    }
        //    else
        //    {
        //        acceptedleavedays = config.LeaveDays - computedConfig.eligibleleave;
        //    }
        //    //workingdays = (totaldays - config.LeaveDays);

        //    if (config.fixedbasic > 0 && config.updatebpfromstructure)
        //    {
        //        config.BasicPay = config.fixedbasic;
        //        if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.BasicPay))
        //        {
        //            config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.BasicPay).Formula = config.BasicPay.ToString("F2");
        //        }
        //    }
        //    if (config.fixedda > 0 && config.updatedafromstructure)
        //    {
        //        if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.DearnessAllowance))
        //        {
        //            config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.DearnessAllowance).Formula = config.fixedda.ToString("F2");
        //        }
        //    }

        //    decimal leaveEncashmentBaseValue = config.grosspay;
        //    if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashmentmode == 1)
        //        leaveEncashmentBaseValue = config.BasicPay;

        //    config.LeaveEncashmentDays = employeeSalaryConfig.encashment_fixeddays;
        //    config.LeaveEncashmentLeaveSwapDays = 0;
        //    config.LeaveEncashment = 0;

        //    if (!employeeSalaryConfig.encashment_allemployees)
        //    {
        //        if (employeeSalaryConfig.encashment_eligiblemonth > 0 &&
        //            config.DateOfJoin.AddMonths(employeeSalaryConfig.encashment_eligiblemonth) > fromdate)
        //        {
        //            config.LeaveEncashmentDays = 0;
        //        }
        //        if (employeeSalaryConfig.encashment_swapmonth > 0 &&
        //            config.DateOfJoin.AddMonths(employeeSalaryConfig.encashment_swapmonth) > fromdate)
        //        {
        //            config.LeaveEncashmentLeaveSwapDays = employeeSalaryConfig.encashment_fixeddays;
        //        }
        //    }

        //    if (config.LeaveEncashmentDays > 0 || config.LeaveEncashmentLeaveSwapDays > 0)
        //    {
        //        decimal maxencashmentdays = 0;
        //        if (employeeSalaryConfig.encashment_mode == 1)
        //        {
        //            var eligibledays = (workingdays - adjustedJoiningResigning);
        //            if (employeeSalaryConfig.encashment_exceptsunday)
        //                eligibledays -= config.sundayworking;
        //            if (employeeSalaryConfig.encashment_exceptholiday)
        //                eligibledays -= config.holidayworking;

        //            maxencashmentdays = eligibledays / employeeSalaryConfig.encashment_perday;
        //            if (employeeSalaryConfig.encashment_perday > 0 && config.LeaveEncashmentDays > 0)
        //            {
        //                if (config.LeaveEncashmentDays > maxencashmentdays)
        //                    config.LeaveEncashmentDays = Math.Floor(maxencashmentdays);
        //            }
        //        }
        //        else
        //        {
        //            var maxworkingdays = totaldays - HolidayTotal(config.DateOfJoin, true, false, false) - config.LeaveEncashmentDays;
        //            if (maxworkingdays > (workingdays - adjustedJoiningResigning))
        //            {
        //                config.LeaveEncashmentDays = 0;
        //            }
        //        }

        //        if (employeeSalaryConfig.encashment_adjustabsent && config.LeaveEncashmentDays > 0 && config.LeaveDays > 0)
        //        {
        //            if (config.LeaveEncashmentDays >= absentdays)
        //            {
        //                config.LeaveEncashmentDays -= config.LeaveDays;
        //                //absentdays = 0;
        //            }
        //            else
        //            {
        //                config.LeaveEncashmentDays -= absentdays;
        //                config.LeaveEncashmentDays = 0;
        //            }
        //        }

        //        if (employeeSalaryConfig.encashment_perday > 0 && config.LeaveEncashmentLeaveSwapDays > 0)
        //        {
        //            if (config.LeaveEncashmentLeaveSwapDays > maxencashmentdays)
        //                config.LeaveEncashmentLeaveSwapDays = maxencashmentdays;
        //        }
        //    }

        //    if (config.proratedaysbased)
        //    {
        //        if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.SpecialDeduction) &&
        //            (config.fixedbasic + config.fixedda) > 0)
        //        {
        //            decimal spldeduction = 0;
        //            if (decimal.TryParse(config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.SpecialDeduction).Formula, out spldeduction) &&
        //                spldeduction != 0)
        //            {
        //                leaveEncashmentBaseValue = config.fixedbasic + config.fixedda;
        //            }
        //        }
        //    }

        //    if (config.LeaveEncashmentDays > 0)
        //    {
        //        int noofdays = 0;
        //        if (employeeSalaryConfig.encashment_workdays_mode == 1)
        //        {
        //            noofdays = employeeSalaryConfig.encashment_workdays_fixed;
        //        }
        //        else
        //        {
        //            noofdays = totaldays;
        //            if (employeeSalaryConfig.encashment_workdays_exceptsunday)
        //                noofdays -= SundayTotal(todate, false);
        //            if (employeeSalaryConfig.encashment_workdays_exceptholiday)
        //                noofdays -= HolidayTotal(todate, false, false, employeeSalaryConfig.encashment_workdays_exceptsunday);
        //        }
        //        if (noofdays > 0)
        //        {
        //            config.LeaveEncashment = Math.Round(leaveEncashmentBaseValue / noofdays * config.LeaveEncashmentDays, 0);
        //            if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.LeaveEncashment))
        //            {
        //                config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.LeaveEncashment).Formula = config.LeaveEncashment.ToString();
        //            }
        //        }
        //    }


        //    config.HolidaySalaryDays = computedConfig.payableholiday + computedConfig.holidayencashment;
        //    if (config.DateOfJoin >= fromdate && config.DateOfJoin <= todate)
        //    {
        //        config.HolidaySalaryDays -= HolidayTotal(config.DateOfJoin, true, true, employeeSalaryConfig.holiday_exceptsunday);
        //    }

        //    if (config.Hasleft && config.Relievingdate >= fromdate && config.Relievingdate <= todate)
        //    {
        //        config.HolidaySalaryDays -= HolidayTotal(config.Relievingdate, true, true, employeeSalaryConfig.holiday_exceptsunday);
        //    }

        //    if (config.HolidaySalaryDays > 0 &&
        //        config.proratedaysbased)
        //    {
        //        config.HolidaySalaryDays -= config.HolidayLeaves;
        //    }

        //    config.HolidaySalaryDays -= config.HolidayLeaveEncashment;
        //    config.Additionalabsentdays = 0;
        //    if (employeeSalaryConfig.holiday_exceptsunday)
        //    {
        //        if (!employeeSalaryConfig.leave_includesunday)
        //        {
        //            if (!config.proratedaysbased)
        //                absentdays += config.sundayworking;
        //            config.Additionalabsentdays += config.sundayworking;
        //        }
        //    }
        //    if (employeeSalaryConfig.holiday_exceptholiday)
        //    {
        //        if (!employeeSalaryConfig.leave_includeholiday)
        //        {
        //            if (!config.proratedaysbased)
        //                absentdays += (config.holidayworking - config.holidaysundayworking);
        //            config.Additionalabsentdays += (config.holidayworking - config.holidaysundayworking);
        //        }
        //    }
        //    if (config.HolidaySalaryDays > 0)
        //    {
        //        decimal noofdays = 0;
        //        if (employeeSalaryConfig.holiday_mode == 1)
        //        {
        //            noofdays = employeeSalaryConfig.holiday_fixeddays;
        //        }
        //        else
        //        {
        //            noofdays = totaldays;
        //            if (employeeSalaryConfig.holiday_exceptsunday)
        //                noofdays -= SundayTotal(todate, false);
        //            if (employeeSalaryConfig.holiday_exceptholiday)
        //                noofdays -= HolidayTotal(todate, false, false, employeeSalaryConfig.holiday_exceptsunday);
        //        }
        //        if (noofdays > 0)
        //        {
        //            if (employeeSalaryConfig.holiday_adjustabsent && absentdays > 0)
        //            {
        //                //if (noofdays >= absentdays)
        //                if (config.HolidayLeaves > 0)
        //                {
        //                    config.HolidaySalaryDays = config.HolidaySalaryDays - config.HolidayLeaves;
        //                }
        //            }

        //            //config.HolidaySalary = Math.Round(leaveEncashmentBaseValue / noofdays * (computedConfig.holidayencashment - config.HolidayLeaveEncashment), 0, MidpointRounding.ToEven);
        //            if (config.HolidaySalaryDays > 0)
        //            {
        //                config.HolidaySalary = Math.Round(leaveEncashmentBaseValue / noofdays * config.HolidaySalaryDays, 0, MidpointRounding.ToEven);
        //            }
        //            if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.HolidaySalary))
        //            {
        //                config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.HolidaySalary).Amount = config.HolidaySalary;
        //                config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.HolidaySalary).Formula = config.HolidaySalary.ToString();
        //            }
        //        }
        //    }


        //    if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.FineDeduction))
        //    {
        //        decimal fine = 0;
        //        if (employeeSalaryConfig.fine_leave && employeeSalaryConfig.fine_leave_perday > 0 && config.LeaveDays > config.ApprovedLeaves)
        //        {
        //            decimal dayleave = config.LeaveDays - config.firsthalf;


        //            fine += Math.Abs(dayleave - config.ApprovedLeaves) * employeeSalaryConfig.fine_leave_perday;
        //        }

        //        if (employeeSalaryConfig.fine_late && employeeSalaryConfig.fine_late_perminute > 0 && employeeSalaryConfig.fine_late_fee > 0)
        //        {
        //            fine += config.latecheckin * employeeSalaryConfig.fine_late_fee;
        //        }
        //        config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.FineDeduction).Formula = fine.ToString();
        //    }


        //    if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.Overtime))
        //    {
        //        decimal ot = 0;

        //        if (employeeSalaryConfig.overtime_formula &&
        //            grosspay>0 && basicPay>0)
        //        {
        //            var totalovertime = Math.Round(((config.morningovertime + config.eveningovertime) * employeeSalaryConfig.overtime_multiplier)/60,2);
        //            if (totalovertime > 0)
        //            {
        //                if(employeeSalaryConfig.overtime_source == 1)
        //                    ot = Math.Round((grosspay / employeeSalaryConfig.overtime_totaldays/ employeeSalaryConfig.overtime_perdayhours) * totalovertime, 0);
        //                else
        //                    ot = Math.Round((basicPay / employeeSalaryConfig.overtime_totaldays / employeeSalaryConfig.overtime_perdayhours) * totalovertime, 0);
        //            }
        //        }
        //        else
        //        {
        //            if (employeeSalaryConfig.overtime_morning && employeeSalaryConfig.overtime_morning_perminute > 0)
        //            {
        //                ot += config.morningovertime * employeeSalaryConfig.overrime_morning_fee;
        //            }

        //            if (employeeSalaryConfig.overtime_evening && employeeSalaryConfig.overtime_evening_perminute > 0)
        //            {
        //                ot += config.eveningovertime * employeeSalaryConfig.overrime_evening_fee;
        //            }
        //        }
        //        config.OverTimePay = ot;
        //        config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.Overtime).Formula = ot.ToString();
        //    }

        //    decimal computedworkingdays = 0;
        //    if (employeeSalaryConfig.workdays_mode == 1)
        //    {
        //        var perdaybasic = basicPay / employeeSalaryConfig.workdays_fixed;
        //        var perdaygross = grosspay / employeeSalaryConfig.workdays_fixed;
        //        if (absentdays > 0 || adjustedJoiningResigning > 0)
        //        {
        //            computedworkingdays = totaldays - (adjustedJoiningResigning + absentdays);
        //            basicPay = computedworkingdays * perdaybasic;
        //            grosspay = computedworkingdays * perdaygross;
        //            acceptedleavedays = (totaldays - computedworkingdays);
        //        }
        //        else
        //        {
        //            computedworkingdays = totaldays;
        //            absentdays = 0;
        //            acceptedleavedays = 0;
        //            basicPay = totaldays * perdaybasic;
        //            grosspay = totaldays * perdaygross;
        //        }
        //        config.Paydays = computedworkingdays;
        //    }
        //    else if (config.proratedaysbased)
        //    {
        //        if (absentdays < 0) absentdays = 0;
        //        if (employeeSalaryConfig.workdays_exceptholiday)
        //        {
        //            config.Paydays = totaldays - (adjustedJoiningResigning + absentdays);
        //            acceptedleavedays = (adjustedJoiningResigning + absentdays);
        //        }
        //        else
        //        {
        //            config.Paydays = totaldays - (adjustedJoiningResigning + absentdays + config.holidayworking);
        //            acceptedleavedays = (adjustedJoiningResigning + absentdays + config.holidayworking);
        //        }
        //        basicPay = Math.Round(config.BasicPay / config.WorkDayCount * (config.Paydays));
        //        grosspay = Math.Round(config.grosspay / config.WorkDayCount * (config.Paydays));
        //        //ctc
        //        if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.SpecialDeduction))
        //        {
        //            decimal spldeduction = 0;
        //            if (decimal.TryParse(config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.SpecialDeduction).Formula, out spldeduction) &&
        //                spldeduction != 0)
        //            {
        //                decimal.TryParse(config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.BasicPay).Formula, out spldeduction);
        //                ctc += spldeduction;
        //                decimal.TryParse(config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.DearnessAllowance).Formula, out spldeduction);
        //                ctc += spldeduction;
        //            }
        //        }
        //    }
        //    else
        //    {

        //        if (absentdays > 0 || adjustedJoiningResigning > 0)
        //        {
        //            basicPay = Math.Round(basicPay - ((config.BasicPay / config.WorkDayCount) * (adjustedJoiningResigning + absentdays)), 0);
        //            grosspay = Math.Round(grosspay - ((config.grosspay / config.WorkDayCount) * (adjustedJoiningResigning + absentdays)), 0);
        //            config.Paydays = config.WorkDayCount - (adjustedJoiningResigning + absentdays);
        //            acceptedleavedays = (adjustedJoiningResigning + absentdays);
        //        }
        //        else
        //        {
        //            absentdays = 0;
        //            acceptedleavedays = 0;
        //            basicPay = config.BasicPay;
        //            grosspay = config.grosspay;
        //            config.Paydays = config.WorkDayCount;
        //        }
        //    }

        //    basicPay = Math.Round(basicPay, 0);
        //    grosspay = Math.Round(grosspay, 0);
        //    MathParserTK.MathParser mathparser = new MathParserTK.MathParser(organizationid);
        //    mathparser.Specials.Add("bp", basicPay.ToString());
        //    mathparser.Specials.Add("advance", "0");
        //    mathparser.Specials.Add("gp", grosspay.ToString());
        //    mathparser.Specials.Add("fp", config.grosspay.ToString());
        //    mathparser.Specials.Add("ot", Math.Round(config.OverTimePay, 0).ToString());
        //    mathparser.Specials.Add("round", "0");
        //    mathparser.Specials.Add("days", days.ToString());
        //    mathparser.Specials.Add("leavedays", acceptedleavedays.ToString());
        //    mathparser.Specials.Add("Absentdays", absentdays.ToString());
        //    mathparser.Specials.Add("leaveencashment", config.LeaveEncashment.ToString());
        //    mathparser.Specials.Add("workdays", ((days - adjustedJoiningResigning) - config.LeaveTaken).ToString());
        //    mathparser.Specials.Add("computedworkingdays", computedworkingdays.ToString());
        //    mathparser.Specials.Add("holidayabsent", config.HolidayAbsent.ToString());
        //    mathparser.Specials.Add("holidayleave", config.HolidayLeaves.ToString());
        //    mathparser.Specials.Add("holidayworking", config.holidayworking.ToString());
        //    mathparser.Specials.Add("sundayworking", config.sundayworking.ToString());
        //    mathparser.Specials.Add("holidaysundayworking", (config.holidayworking + config.sundayworking).ToString());

        //    if (config.SalaryBreakup.Values.ToList().Exists(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.HolidaySalary))
        //    {
        //        mathparser.Specials.Add(config.SalaryBreakup.Values.ToList().Find(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.HolidaySalary).Code.ToLower(), config.HolidaySalary.ToString());
        //        mathparser.Specials.Add("holidayencashment", config.HolidaySalary.ToString("F0"));
        //        config.SalaryBreakup.Values.ToList().Find(e => e.HeaderType == SalaryManager.SalaryHeaderTypes.HolidaySalary).Formula = config.HolidaySalary.ToString("F0");
        //    }

        //    decimal earnamt = 0;
        //    decimal dedamt = 0;
        //    InfyPOS.Data.SalaryManager.SalaryHeader roundingHeader = null;

        //    if (schConfig != null && schConfig.Count > 0)
        //    {
        //        foreach (SalaryManager.ScheduleAmount sa in schConfig)
        //        {
        //            if (DefaultHeaders.ContainsKey(sa.HeaderID) && config.SalaryBreakup.ContainsKey(DefaultHeaders[sa.HeaderID].Code.ToUpper()))
        //            {
        //                config.SalaryBreakup[DefaultHeaders[sa.HeaderID].Code.ToUpper()].Formula = (sa.Amount + config.SalaryBreakup[DefaultHeaders[sa.HeaderID].Code.ToUpper()].Amount).ToString("F0");
        //                config.SalaryBreakup[DefaultHeaders[sa.HeaderID].Code.ToUpper()].Amount += sa.Amount;


        //                if (mathparser.Specials.ContainsKey(DefaultHeaders[sa.HeaderID].Code.ToLower()))
        //                    mathparser.Specials.Remove(DefaultHeaders[sa.HeaderID].Code.ToLower());

        //                mathparser.Specials.Add(DefaultHeaders[sa.HeaderID].Code.ToLower(), config.SalaryBreakup[DefaultHeaders[sa.HeaderID].Code.ToUpper()].Amount.ToString("F0"));
        //            }
        //            else if (DefaultHeaders.ContainsKey(sa.HeaderID))
        //            {
        //                SalaryManager.SalaryHeader sh = DefaultHeaders[sa.HeaderID].Clone();
        //                config.SalaryBreakup.Add(sh.Code.ToUpper(), sh);
        //                sh.Amount = sa.Amount;
        //                sh.Formula = sa.Amount.ToString("F0");
        //                if (mathparser.Specials.ContainsKey(DefaultHeaders[sa.HeaderID].Code.ToLower()))
        //                    mathparser.Specials.Remove(DefaultHeaders[sa.HeaderID].Code.ToLower());
        //                mathparser.Specials.Add(DefaultHeaders[sa.HeaderID].Code.ToLower(), sh.Amount.ToString("F0"));
        //            }
        //        }

        //    }

        //    foreach (KeyValuePair<string, InfyPOS.Data.SalaryManager.SalaryHeader> kv in config.SalaryBreakup.OrderBy(e => e.Value.OrderID))
        //    {
        //        if (kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.BasicPay)
        //            kv.Value.Formula = "BP";

        //        if (kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.Rounding)
        //        {
        //            roundingHeader = kv.Value;
        //            continue;
        //        }

        //        if (string.IsNullOrEmpty(kv.Value.Formula))
        //            continue;

        //        if (kv.Value.formulaKeys.Count > 0 && !config.proratedaysbased)
        //        {
        //            foreach (var key in kv.Value.formulaKeys)
        //                kv.Value.Formula = kv.Value.Formula.Replace(key, "0.00");
        //        }


        //        //if(kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.ProvidentFund)
        //        //{
        //        //    if(config.DateOfBirth.Year >1000 &&
        //        //        config.DateOfBirth.AddYears(58)<salarymonth)
        //        //    {
        //        //        baseAmt = 0;
        //        //    }
        //        //}

        //        if (kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.FixedAllowance)
        //        {
        //            var formulaparts = kv.Value.Formula.Split(new char[] { ':' });
        //            kv.Value.Amount = (decimal)mathparser.Parse(formulaparts[0], false);
        //            if (formulaparts.Length == 2)
        //            {
        //                int mindays = 0;
        //                if (int.TryParse(formulaparts[1], out mindays) && mindays > 0 &&
        //                    days < mindays)
        //                {
        //                    kv.Value.Amount = 0;
        //                }
        //            }

        //        }
        //        else if (kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.FineDeduction)
        //        {
        //            var formulaparts = kv.Value.Formula.Split(new char[] { ':' });
        //            kv.Value.Amount = (decimal)mathparser.Parse(formulaparts[0], false);
        //            if (formulaparts.Length == 2)
        //            {
        //                int mindays = 0;
        //                if (int.TryParse(formulaparts[1], out mindays) && mindays > 0 &&
        //                    days < mindays)
        //                {
        //                    kv.Value.Amount = 0;
        //                }
        //            }
        //            if (!mathparser.Specials.ContainsKey(kv.Value.Code))
        //                mathparser.Specials.Add(kv.Value.Code, kv.Value.Amount.ToString("F0"));
        //        }
        //        else if (kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.FixedDeduction)
        //        {
        //            var formulaparts = kv.Value.Formula.Split(new char[] { ':' });
        //            kv.Value.Amount = (decimal)mathparser.Parse(formulaparts[0], false);
        //            if (formulaparts.Length == 2)
        //            {
        //                int mindays = 0;
        //                if (int.TryParse(formulaparts[1], out mindays) && mindays > 0 &&
        //                    days < mindays)
        //                {
        //                    kv.Value.Amount = 0;
        //                }
        //            }
        //            if (!mathparser.Specials.ContainsKey(kv.Value.Code))
        //                mathparser.Specials.Add(kv.Value.Code, kv.Value.Amount.ToString("F0"));
        //        }
        //        else if (kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.Incentives)
        //        {
        //            var formulaparts = kv.Value.Formula.Split(new char[] { ':' });
        //            kv.Value.Amount = (decimal)mathparser.Parse(formulaparts[0], false);
        //            if (!mathparser.Specials.ContainsKey(kv.Value.Code))
        //                mathparser.Specials.Add(kv.Value.Code, kv.Value.Amount.ToString("F0"));
        //        }
        //        else if (kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.HolidaySalary ||
        //            kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.LoanDeduction ||
        //            kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.Advance)
        //        {
        //            var formulaparts = kv.Value.Formula.Split(new char[] { ':' });
        //            kv.Value.Amount = (decimal)mathparser.Parse(formulaparts[0], false);
        //            if (!mathparser.Specials.ContainsKey(kv.Value.Code))
        //                mathparser.Specials.Add(kv.Value.Code, kv.Value.Amount.ToString("F0"));
        //        }
        //        else
        //        {
        //            decimal amt = 0;
        //            decimal baseAmt = 0;
        //            if (decimal.TryParse(kv.Value.Formula, out amt))
        //            {
        //                baseAmt = amt;
        //                if (!kv.Value.IsFullAmount)
        //                {
        //                    if (employeeSalaryConfig.workdays_mode == 1)
        //                    {
        //                        baseAmt = baseAmt / employeeSalaryConfig.workdays_fixed * computedworkingdays;
        //                    }
        //                    else
        //                    {
        //                        if (config.proratedaysbased)
        //                        {
        //                            baseAmt = Math.Round(baseAmt / config.WorkDayCount * (config.Paydays), 0);
        //                        }
        //                        else
        //                        {
        //                            baseAmt = Math.Round(baseAmt - ((baseAmt / config.WorkDayCount) * (adjustedJoiningResigning + absentdays)), 0);
        //                        }
        //                    }
        //                }
        //                baseAmt = kv.Value.Code.Contains("ESI") ? Math.Ceiling(baseAmt) : Math.Round(baseAmt, 0);
        //            }
        //            else
        //            {
        //                amt = (decimal)mathparser.Parse(kv.Value.Formula, false);
        //                baseAmt = kv.Value.Code.Contains("ESI") ? Math.Ceiling(amt) : Math.Round(amt, 0);
        //            }
        //            if (!mathparser.Specials.ContainsKey(kv.Value.Code.ToLower()))
        //                mathparser.Specials.Add(kv.Value.Code.ToLower(), baseAmt.ToString("F0"));
        //            kv.Value.Amount = (decimal)baseAmt;
        //        }

        //        //Math.Round((decimal)Math.Abs(baseAmt), 0);

        //        if (DefaultHeaders.ContainsKey(kv.Value.HeaderID) && DefaultHeaders[kv.Value.HeaderID].Max > 0 &&
        //            DefaultHeaders[kv.Value.HeaderID].Max < kv.Value.Amount)
        //        {
        //            kv.Value.Amount = DefaultHeaders[kv.Value.HeaderID].Max;
        //        }


        //        if (config.proratedaysbased &&
        //            kv.Value.HeaderType == SalaryManager.SalaryHeaderTypes.SpecialDeduction &&
        //            kv.Value.Amount > 0 && ctc > 0)
        //        {
        //            kv.Value.Amount = Math.Round((ctc - config.grosspay) / config.WorkDayCount * (config.Paydays + config.holidayworking + config.LeaveEncashmentDays));
        //            /* ((kv.Value.Amount / config.Paydays) * config.WorkDayCount)
        //                config.LeaveEncashmentDays + config.holidayworking*/
        //        }

        //        if (kv.Value.IsDeduction)
        //            dedamt += kv.Value.Amount;
        //        else
        //            earnamt += kv.Value.Amount;
        //    }


        //    if (roundingHeader != null)
        //    {
        //        decimal netamt = earnamt - dedamt;
        //        decimal round = netamt % 10;
        //        roundingHeader.Amount = round != 0 ? 10 - round : 0;
        //    }

        //    if (workingdays > 0)
        //        config.AcceptedWorkDays = workingdays;

        //    if (adjustedJoiningResigning > 0)
        //    {
        //        config.WorkDayCount -= adjustedJoiningResigning;
        //        config.AdjustedJoiningResigning = adjustedJoiningResigning;
        //        config.AcceptedWorkDays = days - adjustedJoiningResigning;
        //    }
        //    config.AbsentDays = absentdays;
        //    return config;
        //}

        private int HolidayTotal(DateTime dateOfJoin, bool isfrom, bool onlypayable, bool exculedsunday)
        {
            if (isfrom)
                return holidayList.Count(e => e.Date >= dateOfJoin && (!onlypayable || e.Wages) && (!exculedsunday || e.Date.DayOfWeek != DayOfWeek.Sunday));
            else
                return holidayList.Count(e => e.Date <= dateOfJoin && (!onlypayable || e.Wages) && (!exculedsunday || e.Date.DayOfWeek != DayOfWeek.Sunday));
        }
    }

}
