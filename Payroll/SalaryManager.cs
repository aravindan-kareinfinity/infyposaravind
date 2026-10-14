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

namespace InfyPOS.Data
{
    public class SalaryManager
    {
        private static System.Collections.Concurrent.ConcurrentDictionary<string, List<string>> formulacache;
        public static List<string> Parseformula(string Formula,string formulacodes)
        {
            if (formulacache == null)
                formulacache = new System.Collections.Concurrent.ConcurrentDictionary<string, List<string>>();

            List<string> result;
            if(!formulacache.TryGetValue(Formula+formulacodes, out result))
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
        public SalaryManager()
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

        public enum SalaryHeaderTypes
        {
            None,
            BasicPay,
            Earnings,
            ProvidentFund,
            VPF,
            ESI,
            IncomeTax,
            HouseRentAllowance,
            DearnessAllowance,
            SpecialAllowance,
            Deduction,
            MedicalInsurance,
            Overtime,
            Rounding,
            Advance,
            LoanDeduction,
            Bonus,
            Increment,
            LeaveEncashment,
            Conveyance,
            OtherAllowance,
            SpecialDeduction,
            ESI_Employee,
            PF_Employee,
            FixedAllowance,
            Incentives,
            FixedDeduction,
            HolidaySalary,
            FineDeduction
        }

        [Serializable]
        public class SalaryHeader
        {
            public int OrderID
            {
                get
                {
                    if (HeaderType == SalaryHeaderTypes.FixedDeduction || HeaderType == SalaryHeaderTypes.FixedAllowance ||
                        HeaderType == SalaryHeaderTypes.Incentives || HeaderType == SalaryHeaderTypes.MedicalInsurance)
                        return 1;

                    if (HeaderType == SalaryHeaderTypes.LeaveEncashment || HeaderType == SalaryHeaderTypes.Advance ||
                        HeaderType == SalaryHeaderTypes.IncomeTax || HeaderType == SalaryHeaderTypes.LoanDeduction ||
                        HeaderType == SalaryHeaderTypes.HolidaySalary || HeaderType == SalaryHeaderTypes.FineDeduction)
                        return 2;

                    if (HeaderType == SalaryHeaderTypes.BasicPay || HeaderType == SalaryHeaderTypes.DearnessAllowance ||
                        HeaderType == SalaryHeaderTypes.HouseRentAllowance || HeaderType == SalaryHeaderTypes.OtherAllowance ||
                        HeaderType == SalaryHeaderTypes.SpecialAllowance)
                        return 3;

                    if (!IsDeduction)
                        return 4;

                    return 5;
                }
            }
            [XmlIgnoreAttribute]
            public SalaryHeaderTypes HeaderType { get; set; }
            [XmlIgnoreAttribute]
            public decimal Max { get; set; }
            [XmlIgnoreAttribute]
            public bool IsDeduction
            {
                get
                {
                    return (HeaderType == SalaryHeaderTypes.Deduction ||
                        HeaderType == SalaryHeaderTypes.ESI ||
                        HeaderType == SalaryHeaderTypes.IncomeTax ||
                        HeaderType == SalaryHeaderTypes.LoanDeduction ||
                        HeaderType == SalaryHeaderTypes.MedicalInsurance ||
                        HeaderType == SalaryHeaderTypes.Advance ||
                        HeaderType == SalaryHeaderTypes.SpecialDeduction ||
                        HeaderType == SalaryHeaderTypes.FixedDeduction ||
                        HeaderType == SalaryHeaderTypes.ProvidentFund ||
                        HeaderType == SalaryHeaderTypes.FineDeduction);
                }
            }
            public string Description { get; set; }
            public bool IsExternal { get; set; }
            public string Code;
            public bool IsNumber;
            public SalaryHeader(System.Data.DataRow dr)
            {
                HeaderID = (long)dr["ReferenceOID"];
                Code = dr["Value"].ToString().Split('~')[0];
                Formula = dr["Value"].ToString().Split('~')[2];
                SetAsIsNumber();
            }

            
           
            const string keywords = ",BP,ADVANCE,OT,ROUND,RND,LEAVEDAYS,DAYS,LEAVEENCASHMENT,INCENTIVES,GP,FP,HOLIDAYENCASHMENT,WORKDAYS,HOLIDAYABSENT,HOLIDAYLEAVE,COMPUTEDWORKINGDAYS,HOLIDAYWORKING,SUNDAYWORKING,HOLIDAYSUNDAYWORKING,";
            public SalaryHeader()
            {
            }
            public SalaryHeader(long refno, string formula)
            {
                this.HeaderID = refno;
                this.Formula = formula;
                SetAsIsNumber();
            }
            public void SetAsIsNumber()
            {
                double number = 0;
                IsNumber = double.TryParse(Formula, out number);
            }
            public void BuildFormula(string formulacodes)
            {
                formulacodes += keywords;
                if (HeaderType == SalaryHeaderTypes.Incentives)
                    Formula = "INCENTIVES";
                if (HeaderType == SalaryHeaderTypes.LeaveEncashment)
                    Formula = "LEAVEENCASHMENT";
                if (HeaderType == SalaryHeaderTypes.HolidaySalary)
                    Formula = "HOLIDAYENCASHMENT";
                if (IsNumber || string.IsNullOrEmpty(Formula)) return;

                formulaKeys = SalaryManager.Parseformula(Formula, formulacodes);

                /*var keys = System.Text.RegularExpressions.Regex.Replace(Formula, "[^a-zA-Z]+", ",")
                    .Split(new string[] { ","}, StringSplitOptions.RemoveEmptyEntries).ToList().Distinct().ToList();

                foreach(var k in keys)
                {
                    if (!formulacodes.Contains("," + k.ToUpper() + ","))
                        formulaKeys.Add(k);
                }*/

                /*
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
                                formulaKeys.Add(current);
                            current = "";
                        }
                    }
                }
                if (!string.IsNullOrEmpty(current) && !formulacodes.Contains("," + current.ToUpper() + ","))
                    formulaKeys.Add(current);
                    */
            }
            public long HeaderID { get; set; }
            public string Formula { get; set; }
            [NonSerialized]
            [XmlIgnoreAttribute]
            public List<string> formulaKeys = new List<string>();
            public decimal Amount { get; set; }

            internal SalaryHeader Clone()
            {
                return MemberwiseClone() as SalaryHeader;
            }

            public bool IsFullAmount
            {
                get
                {
                    return HeaderType == SalaryHeaderTypes.Advance || HeaderType == SalaryHeaderTypes.Bonus
                        || HeaderType == SalaryHeaderTypes.IncomeTax || HeaderType == SalaryHeaderTypes.Increment
                        || HeaderType == SalaryHeaderTypes.LoanDeduction || HeaderType == SalaryHeaderTypes.MedicalInsurance
                        || HeaderType == SalaryHeaderTypes.Overtime || HeaderType == SalaryHeaderTypes.Rounding
                        || HeaderType == SalaryHeaderTypes.FineDeduction
                        || HeaderType == SalaryHeaderTypes.FixedAllowance || HeaderType == SalaryHeaderTypes.FixedDeduction
                        || HeaderType == SalaryHeaderTypes.LeaveEncashment || HeaderType == SalaryHeaderTypes.HolidaySalary;
                }
            }

            public decimal Value { get; set; }
            public Referencelist.IncentiveConfig incentiveconfig { get; set; }

            public override string ToString()
            {
                return Code;
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

        [Serializable]
        public class ScheduleAmount
        {
            public long HeaderID { get; set; }
            public decimal Amount { get; set; }
            public string AdditionalInfo { get; set; }

            public DateTime Date { get; set; }
        }

        [Serializable]
        public class EmployeeSalaryConfig
        {
            internal string SalaryBreakupJSON;
            internal long SalaryBreakupTime;

            public long ID { get; set; }
            public DateTime DateOfJoin { get; set; }
            public DateTime DateOfBirth { get; set; }
            public decimal BasicPay { get; set; }
            public decimal Advance { get; set; }
            public int OverTimeHour { get; set; }
            public decimal OverTimeRate { get; set; }
            public decimal OverTimePay
            {
                get
                {
                    return OverTimeRate * OverTimeHour;
                }
            }

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
            public decimal LeaveEncashmentDays { get; set; }
            public decimal AcceptedWorkDays { get; set; }
            public decimal incentive { get; set; }
            public decimal LeaveEncashment { get; set; }
            public long locationid { get; set; }
            public long floorid { get; set; }
            public decimal HolidayLeaveEncashment { get; set; }
            public decimal HolidaySalary { get; set; }
            public decimal HolidaySalaryDays { get; set; }
            public decimal AdjustedJoiningResigning { get; set; }

            public void SetSalaryBreakup(string source, SortedList<long, SalaryHeader> defHeaders)
            {
                var employeeHeaders = Newtonsoft.Json.JsonConvert.DeserializeObject<List<InfyPOS.Models.Employeesalarystructure.Configuration>>(source);
                if (employeeHeaders == null)
                    employeeHeaders = new List<Employeesalarystructure.Configuration>();
                SetSalaryBreakup(employeeHeaders, defHeaders);
            }
            public void SetSalaryBreakup(List<InfyPOS.Models.Employeesalarystructure.Configuration> employeeHeaders, SortedList<long, SalaryHeader> defHeaders)
            {


                if (BasicPay == 0) BasicPay = fixedbasic;
                if (grosspay == 0) grosspay = fixedbasic + fixedda;
                //if (defHeaders.Values.Any(e => e.HeaderType == SalaryHeaderTypes.DearnessAllowance))
                //{
                //    long daid = defHeaders.Values.First(e => e.HeaderType == SalaryHeaderTypes.DearnessAllowance).HeaderID;
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


                employeeHeaders.RemoveAll(e => !defHeaders.ContainsKey(e.id));

                if (!this.eligibleesi && employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) && defHeaders[e.id].HeaderType == SalaryHeaderTypes.ESI))
                {
                    string code = employeeHeaders.Find(e => defHeaders[e.id].HeaderType == SalaryHeaderTypes.ESI).code;
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
                    if (defHeaders.Values.ToList().Exists(e => e.HeaderType == SalaryHeaderTypes.ESI) &&
                        !employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) && defHeaders.ContainsKey(e.id) && defHeaders[e.id].HeaderType == SalaryHeaderTypes.ESI))
                    {
                        var itm = defHeaders.Values.ToList().Find(e => e.HeaderType == SalaryHeaderTypes.ESI);
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
                        var eitm = employeeHeaders.Find(e => defHeaders[e.id].HeaderType == SalaryHeaderTypes.ESI);
                        var itm = defHeaders.Values.ToList().Find(e => e.HeaderType == SalaryHeaderTypes.ESI);
                        if (eitm != null && itm != null)
                        {
                            if (eitm.formula == "" || eitm.formula == "0" || eitm.formula == "0.00")
                                eitm.formula = itm.Formula;
                        }
                    }
                }

                if (this.eligiblepf)
                {
                    if (defHeaders.Values.ToList().Exists(e => e.HeaderType == SalaryHeaderTypes.ProvidentFund) &&
                        !employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) && defHeaders[e.id].HeaderType == SalaryHeaderTypes.ProvidentFund))
                    {
                        var itm = defHeaders.Values.ToList().Find(e => e.HeaderType == SalaryHeaderTypes.ProvidentFund);
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
                        var eitm = employeeHeaders.Find(e => defHeaders[e.id].HeaderType == SalaryHeaderTypes.ProvidentFund);
                        var itm = defHeaders.Values.ToList().Find(e => e.HeaderType == SalaryHeaderTypes.ProvidentFund);
                        if (eitm != null && itm != null)
                        {
                            if (eitm.formula == "" || eitm.formula == "0" || eitm.formula == "0.00")
                                eitm.formula = itm.Formula;
                        }
                    }
                }


                if (!this.eligiblepf && employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) && defHeaders[e.id].HeaderType == SalaryHeaderTypes.ProvidentFund))
                {
                    string code = employeeHeaders.Find(e => defHeaders[e.id].HeaderType == SalaryHeaderTypes.ProvidentFund).code;
                    foreach (var xheader in employeeHeaders)
                    {
                        if (xheader.code == code)
                            xheader.formula = "0.00";
                        else
                            xheader.formula = xheader.formula == null ? "0.00" : xheader.formula.Replace(code, "0.00");
                    }
                }


                if (!this.esiselfpaid && employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) && defHeaders[e.id].HeaderType == SalaryHeaderTypes.ESI_Employee))
                {
                    string code = employeeHeaders.Find(e => defHeaders[e.id].HeaderType == SalaryHeaderTypes.ESI_Employee).code;
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
                    if (defHeaders.Values.ToList().Exists(e => e.HeaderType == SalaryHeaderTypes.ESI_Employee) &&
                        !employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) && defHeaders[e.id].HeaderType == SalaryHeaderTypes.ESI_Employee))
                    {
                        var itm = defHeaders.Values.ToList().Find(e => e.HeaderType == SalaryHeaderTypes.ESI_Employee);
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
                        var eitm = employeeHeaders.Find(e => defHeaders[e.id].HeaderType == SalaryHeaderTypes.ESI_Employee);
                        var itm = defHeaders.Values.ToList().Find(e => e.HeaderType == SalaryHeaderTypes.ESI_Employee);
                        if (eitm != null && itm != null)
                            if (eitm.formula == "" || eitm.formula == "0" || eitm.formula == "0.00")
                                eitm.formula = itm.Formula;
                    }
                }

                if (!this.pfselfpaid && employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) && defHeaders[e.id].HeaderType == SalaryHeaderTypes.PF_Employee))
                {
                    string code = employeeHeaders.Find(e => defHeaders[e.id].HeaderType == SalaryHeaderTypes.PF_Employee).code;
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
                    if (!employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) && defHeaders[e.id].HeaderType == SalaryHeaderTypes.PF_Employee) &&
                        defHeaders.Values.ToList().Exists(e => e.HeaderType == SalaryHeaderTypes.PF_Employee))
                    {
                        var itm = defHeaders.Values.ToList().Find(e => e.HeaderType == SalaryHeaderTypes.PF_Employee);
                        if (itm != null)
                            employeeHeaders.Add(new InfyPOS.Models.Employeesalarystructure.Configuration()
                            {
                                code = itm.Code,
                                id = itm.HeaderID,
                                formula = itm.Formula
                            });
                    }
                    else if (employeeHeaders.Exists(e => defHeaders.ContainsKey(e.id) && defHeaders[e.id].HeaderType == SalaryHeaderTypes.PF_Employee))
                    {
                        var eitm = employeeHeaders.Find(e => defHeaders[e.id].HeaderType == SalaryHeaderTypes.PF_Employee);
                        var itm = defHeaders.Values.ToList().Find(e => e.HeaderType == SalaryHeaderTypes.PF_Employee);
                        if (eitm != null && itm != null)
                            if (eitm.formula == "" || eitm.formula == "0" || eitm.formula == "0.00")
                                eitm.formula = itm.Formula;
                    }
                }

                defaultHeaders = new List<SalaryHeader>();
                SalaryBreakup = new SortedList<string, InfyPOS.Data.SalaryManager.SalaryHeader>();


                string additionalcodes = "," + string.Join(",", defHeaders.Values.ToList().FindAll(e => e.HeaderType == SalaryHeaderTypes.FixedDeduction || e.HeaderType == SalaryHeaderTypes.FixedAllowance || e.HeaderType == SalaryHeaderTypes.FineDeduction).ConvertAll(e => e.Code));
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
                HasConflict = PreprocessSalaryBreakup(additionalcodes, 0);
            }

            private bool PreprocessSalaryBreakup(string additionalcodes, int iteration)
            {
                if (iteration == 10) return true;
                bool modified = false;
                foreach (KeyValuePair<string, InfyPOS.Data.SalaryManager.SalaryHeader> kv in SalaryBreakup)
                {
                    kv.Value.BuildFormula(additionalcodes);
                    if (kv.Value.formulaKeys.Count == 0)
                    {
                        if (kv.Value.IsFullAmount && !string.IsNullOrEmpty(kv.Value.Formula))
                        {
                            kv.Value.IsNumber = false;
                            kv.Value.Formula = "rnd(" + kv.Value.Formula + "-(" + kv.Value.Formula + "/days*leavedays))";
                        }
                        continue;
                    }
                    foreach (string s in kv.Value.formulaKeys.ToArray())
                    {
                        if (SalaryBreakup.ContainsKey(s))
                        {
                            if (SalaryBreakup[s].HeaderType == SalaryHeaderTypes.LeaveEncashment)
                                SalaryBreakup[s].Formula = "leaveencashment";

                            if (SalaryBreakup[s].HeaderType == SalaryHeaderTypes.Incentives)
                                SalaryBreakup[s].Formula = "incentives";

                            if (SalaryBreakup[s].HeaderType == SalaryHeaderTypes.HolidaySalary)
                                SalaryBreakup[s].Formula = "holidayencashment";

                            modified = true;
                            if (SalaryBreakup[s].IsFullAmount || !SalaryBreakup[s].IsNumber ||
                                SalaryBreakup[s].HeaderType == SalaryHeaderTypes.LeaveEncashment ||
                                SalaryBreakup[s].HeaderType == SalaryHeaderTypes.Incentives ||
                                SalaryBreakup[s].HeaderType == SalaryHeaderTypes.HolidaySalary ||
                                SalaryBreakup[s].HeaderType == SalaryHeaderTypes.FixedAllowance ||
                                SalaryBreakup[s].HeaderType == SalaryHeaderTypes.FixedDeduction ||
                                SalaryBreakup[s].HeaderType == SalaryHeaderTypes.FineDeduction)
                            {
                                if (SalaryBreakup[s].Formula.Length > 0)
                                    kv.Value.Formula = kv.Value.Formula.Replace(s, "rnd(" + SalaryBreakup[s].Formula + ")");
                                else
                                    kv.Value.Formula = kv.Value.Formula.Replace(s, "");

                            }
                            else if (!string.IsNullOrEmpty(SalaryBreakup[s].Formula))
                            {
                                kv.Value.Formula = kv.Value.Formula.Replace(s, "rnd(" + SalaryBreakup[s].Formula + "-(" + SalaryBreakup[s].Formula + "/days*leavedays))");
                            }
                            kv.Value.BuildFormula(additionalcodes);
                            if (kv.Value.formulaKeys.Count == 1 && kv.Value.formulaKeys[0] == s)
                            {
                                kv.Value.Formula = "";
                                kv.Value.formulaKeys.Clear();
                            }
                        }
                    }
                }
                if (modified)
                    return PreprocessSalaryBreakup(additionalcodes, iteration++);

                return false;
            }
        }

        //int month;
        //int year;
        decimal days;
        DateTime salarymonth;
        DateTime fromdate;
        DateTime todate;
        public void Initialize(int month, int year)
        {
            this.salarymonth = new DateTime(year, month, 1);
            fromdate = new DateTime(year, month, 1);
            todate = fromdate.AddMonths(1).AddDays(-1);
            days = WorkingDays();
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
            days = WorkingDays();
        }


        private SortedList<long, SalaryHeader> defaultHeaders = null;
        public SortedList<long, SalaryHeader> DefaultHeaders
        {
            get
            {
                if (defaultHeaders == null || defaultHeaders.Count == 0)
                    BuildSalaryHeaders();
                return defaultHeaders;
            }
        }

        private SortedList<string, SalaryHeader> defaultCodes = null;
        public SortedList<string, SalaryHeader> DefaultCodes
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

            defaultHeaders = new SortedList<long, SalaryHeader>();
            defaultCodes = new SortedList<string, SalaryHeader>();
            foreach (var dr in configList)
            {

                SalaryHeader sh = new SalaryHeader();
                sh.Description = dr.name;
                sh.HeaderID = dr.id;
                try
                {
                    sh.Code = dr.code;
                    sh.Formula = dr.additionalinfo != null ? dr.additionalinfo.formula : "";
                    sh.Max = dr.additionalinfo != null ? dr.additionalinfo.maxlimit : 0;
                    SalaryHeaderTypes type;
                    if (Enum.TryParse<SalaryHeaderTypes>(dr.additionalinfo.paytype, out type))
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

        public Dictionary<long, List<ScheduleAmount>> BuildExternal(int month, int year, long employeeID)
        {

            DateTime fromDate = new DateTime(year, month, 1);

            Dictionary<long, List<ScheduleAmount>> retValue = new Dictionary<long, List<ScheduleAmount>>();
            SortedList<string, object> param = new SortedList<string, object>();
            InfyPOS.Common.BaseData baseData = new BaseData() { Context = new UserClaim() { OrganizationID = organizationid } };
            baseData.Context = this.Context;

            string query = @"select employeeid,amount,detail,schedulemonth,salaryheaderid from employeesalaryschedule
                            where employeesalaryschedule.isactive = true and isreceivable=false and date_part('month', schedulemonth)= @month AND date_part('year', schedulemonth) = @year";

            fromDate = new DateTime(year, month, 1);
            param.Add("month", month);
            param.Add("year", year);
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
                    ScheduleAmount sa = new ScheduleAmount();
                    sa.Amount = reader.GetDecimal(1);
                    sa.AdditionalInfo = reader.IsDBNull(2) ? "" : reader.GetString(2);
                    sa.Date = reader.IsDBNull(3) ? DateTime.MinValue : reader.GetDateTime(3);
                    sa.HeaderID = reader.GetInt64(4);
                    if (!retValue.ContainsKey(empID))
                        retValue.Add(empID, new List<ScheduleAmount>());
                    retValue[empID].Add(sa);
                }
            }
            return retValue;
        }

        public List<EmployeeSalaryConfig> BuildMasters(int month, int year, long employeeID, int salarymode)
        {
            if (Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_payslipbydate && fromdate.Year > 2000 && todate.Year > 2000)
            {
                return BuildMasters(fromdate, todate, employeeID, salarymode);
            }
            else
            {
                DateTime fromDate = new DateTime(year, month, 1);
                DateTime toDate = fromDate.AddMonths(1);
                return BuildMasters(fromDate, toDate, employeeID, salarymode);
            }
        }
        public List<EmployeeSalaryConfig> BuildMasters(DateTime fromDate, DateTime todate, long employeeID, int salarymode)
        {
            string query = @"
                update employeeleave set isactive=false,modifiedby=-4 
                from employee 
                where employeeleave.fromdate>=@fromdate and employee.id = employeeleave.employeeid and employeeleave.fromdate<employee.dateofjoining  and employee.salarymode=@salarymode;

                update employeeinout set isactive=false,modifiedby=-4 
                from employee 
                where employeeinout.attendanceon>=@fromdate and employee.id = employeeinout.employeeid and employeeinout.attendanceon<employee.dateofjoining and employee.salarymode=@salarymode;

                update employeeleave set isactive=false,modifiedby=-4 
                from employee 
                where employeeleave.fromdate>=@fromdate and employee.hasleft and employee.relievingdate>'2000-01-01' 
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
                Leave.LeaveDays,SundayLeaves, holidayLeaves.LeaveDays HolidayLeaves,
                holidaypayable.LeaveDays HolidayAbsent, employeesalarystructure.splallowanceid,employee.employeecode,
                lcmode,employee.salarymode,relievingdate,hasleft,haswithheld,withheldfrom,pfandesi,dateofbirth,
                fulldays,halfday,employee.workingmode,employee.departmentid,employee.sectionid,employee.companyid,
                holidayLeaves.SunDays HolidaySundayLeaves,
                holidaypayable.SunDays HolidaySundayAbsent,employeesalaryheader.eligibleesi,employeesalaryheader.eligiblepf,
                employeesalaryheader.pfselfpaid,employeesalaryheader.esiselfpaid,employee.locationid,
                holidayLeaves.leaveencashment HolidayLeaveEncashment,employee.floorid
                from employee 
                left outer join referencelist on referencelist.id = employee.desingnationid
                left outer join (
                                select employeeid,SUM(days) as LeaveDays,SUM(sundaydays) as SundayLeaves from employeeleave 
                                inner join employee on employee.id = employeeleave.employeeid  and employee.salarymode=@salarymode
                                where EmployeeLeave.isactive and deductsalary=true 
                                and fromdate>=dateofjoining and
                                (employee.hasleft = false or relievingdate>=fromdate) and
                                fromdate >=@fromdate and todate<@todate
                                group by EmployeeID
                            ) Leave
                            on Leave.employeeid = employee.id

                left outer join (select sum(case when morningabsent = false and afternoonabsent = false then 1 else 0 end) fulldays,
                                sum(case when morningabsent = true or afternoonabsent = true then 1 else 0 end) as halfday,
                                employeeid from employeeinout
                                inner join employee on employee.id = employeeinout.employeeid  and employee.salarymode=@salarymode
                                where employeeinout.isactive 
                                and attendanceon>=dateofjoining and
                                (employee.hasleft = false or relievingdate>=attendanceon) and
                                attendanceon>=@fromdate and attendanceon<@todate
                                group by employeeid) inout
                            on inout.employeeid = employee.id
                left outer join (select employeeid,sum(days) as LeaveDays,sum(case when EXTRACT(DOW FROM fromdate)=0 then days else 0 end) as Sundays,sum(leaveencashment*days) leaveencashment from holiday
	            inner join EmployeeLeave on EmployeeLeave.fromdate = holiday.holidayon and EmployeeLeave.isactive=true
                inner join employee on employee.id = employeeleave.employeeid and employee.salarymode=@salarymode
	            where holiday.isactive=true and fromdate>=@fromdate and todate<@todate and fromdate>=dateofjoining and
                                (employee.hasleft = false or relievingdate>=fromdate)
	            group by employeeid) holidayLeaves on holidayLeaves.employeeid = employee.id

                left outer join (select employeeid,sum(days) as LeaveDays,sum(case when EXTRACT(DOW FROM fromdate)=0 then days else 0 end) as Sundays from holiday
	            inner join EmployeeLeave on EmployeeLeave.fromdate = holiday.holidayon and EmployeeLeave.isactive=true
                inner join employee on employee.id = employeeleave.employeeid and employee.salarymode=@salarymode
	            where holiday.isactive=true and payable=true and fromdate>=@fromdate and todate<@todate and fromdate>=dateofjoining and
                                (employee.hasleft = false or relievingdate>=fromdate)
	            group by employeeid) holidaypayable on holidaypayable.employeeid = employee.id

                left outer join salaryheader on salaryheader.ranknumber = 1 and salaryheader.employeeid = employee.id
			    left outer join employeesalaryheader on employeesalaryheader.id = salaryheader.id
                left outer join employeesalarystructure on employeesalarystructure.id = employeesalaryheader.salarystructureid
                where employee.isactive and (employee.hasleft = false or relievingdate>=@fromdate) and employee.DateOfJoining<@todate and employee.salarymode=@salarymode";

            // and (dateofreleaving <DateOfJoining or dateofreleaving is null) 

            SortedList<string, object> param = new SortedList<string, object>();

            List<EmployeeSalaryConfig> retValue = new List<EmployeeSalaryConfig>();
            param.Add("fromdate", fromDate);
            param.Add("todate", todate);
            if (employeeID > 0)
            {
                param.Add("employeeID", employeeID);
                query = query + " and employee.id = @employeeID";
            }


            if (Context.accessiblelocation != null && !Context.accessiblelocation.Exists(x => x == -1))
            {
                query.Replace("and employee.salarymode=@salarymode", "and employee.salarymode=@salarymode and employee.locationid in (" + string.Join(",", Context.accessiblelocation)+")");
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
            var result = BuildMasters(query, param);


            if (DefaultHeaders.Values.ToList().Exists(e => e.HeaderType == SalaryHeaderTypes.Incentives))
            {
                try
                {
                    InfyPOS.Payroll.IncentiveSalary incentiveservice = new InfyPOS.Payroll.IncentiveSalary(organizationid, fromDate, todate.AddDays(-1),
                               0, 0, 0, 0, true, employeeID, new List<int>() { 0 });

                    var incentivedata = incentiveservice.Compute(null, 0);

                    var incentconfig = DefaultHeaders.Values.ToList().Find(e => e.HeaderType == SalaryHeaderTypes.Incentives);

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

        private List<EmployeeSalaryConfig> BuildMasters(string query, SortedList<string, object> param)
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
                    entity.floorid = reader.IsDBNull(39) ? 0 : reader.GetInt64(39);
                    try
                    {
                        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                        entity.SalaryBreakupJSON = reader.IsDBNull(10) ? "" : reader.GetString(10);
                        stopwatch.Start();
                        entity.SetSalaryBreakup(reader.IsDBNull(10) ? "" : reader.GetString(10), DefaultHeaders);
                        stopwatch.Stop();
                        if (stopwatch.ElapsedMilliseconds > 1000)
                        {
                            entity.SalaryBreakupTime = stopwatch.ElapsedMilliseconds;
                        }
                    }
                    catch(Exception exp)
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
            if (schConfigy.SalaryBreakup.Values.ToList().Exists(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment))
                mathparser.Specials.Add("leaveencashment", schConfigy.LeaveEncashment.ToString("F0"));

            if (schConfigy.SalaryBreakup.Values.ToList().Exists(e => e.HeaderType == SalaryHeaderTypes.Incentives))
                mathparser.Specials.Add("incentives", schConfigy.incentive.ToString("F0"));

            decimal earnamt = 0;
            decimal dedamt = 0;
            InfyPOS.Data.SalaryManager.SalaryHeader roundingHeader = null;
            foreach (KeyValuePair<string, InfyPOS.Data.SalaryManager.SalaryHeader> kv in schConfigy.SalaryBreakup)
            {
                if (kv.Value.HeaderType == SalaryHeaderTypes.Rounding)
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

        int totalHoliday = 0;
        int totalHolidayEncashment = 0;
        int totalPayableHoliday = 0;
        int totalSunday = 0;
        //int payableholiday = 0;
        //int nonpayableholiday = 0;
        public decimal TotalHoliday { get; set; }
        public decimal TotalDays { get; set; }
        public decimal TotalWorkingDays { get; set; }
        public int TotalSunday
        {
            get
            {
                return totalSunday;
            }

            set
            {
                totalSunday = value;
            }
        }

        public bool includeSunday;
        List<DateTime> holidayList = new List<DateTime>();
        private decimal WorkingDays()
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
            TotalDays = (decimal)monthend.Subtract(monthstart).TotalDays+1;
            TotalWorkingDays = (decimal)monthend.Subtract(monthstart).TotalDays + 1 - totalHoliday;
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_workdaysmode == 2)
                TotalWorkingDays = 30;
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_workdaysmode == 3)
                TotalWorkingDays = TotalDays;
            return TotalWorkingDays;
        }

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

        private int SundayTotal(DateTime todate, bool isfrom)
        {
            int sundays = 0;
            DateTime monthstart = fromdate;
            DateTime monthend = todate.AddDays(1);
            if (isfrom)
                monthstart = todate;
            else
                monthend = todate;
            while (monthstart < monthend)
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
                if (kv.Value.HeaderType == SalaryHeaderTypes.Rounding)
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
                    if (kv.Value.HeaderType == SalaryHeaderTypes.BasicPay)
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

                if (kv.Value.HeaderType == SalaryHeaderTypes.ProvidentFund)
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
                foreach (ScheduleAmount sa in schConfig)
                {
                    if (DefaultHeaders.ContainsKey(sa.HeaderID) && config.SalaryBreakup.ContainsKey(DefaultHeaders[sa.HeaderID].Code.ToUpper()))
                    {
                        config.SalaryBreakup[DefaultHeaders[sa.HeaderID].Code.ToUpper()].Amount += sa.Amount;
                        dedamt += sa.Amount;
                    }
                    else if (DefaultHeaders.ContainsKey(sa.HeaderID))
                    {
                        SalaryHeader sh = DefaultHeaders[sa.HeaderID].Clone();
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
                config.SetSalaryBreakup(bconfig.configuration_json, DefaultHeaders);

                if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment))
                {
                    config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment).Formula = "0";
                }

                if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryHeaderTypes.Incentives))
                {
                    config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryHeaderTypes.Incentives).Formula = "0";
                }


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

                if (config.SalaryBreakup.Values.ToList().Exists(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment))
                {
                    mathparser.Specials.Add("leaveencashment", config.LeaveEncashment.ToString("F0"));
                    config.SalaryBreakup.Values.ToList().Find(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment).Formula = config.LeaveEncashment.ToString("F0");
                }

                if (config.SalaryBreakup.Values.ToList().Exists(e => e.HeaderType == SalaryHeaderTypes.Incentives))
                {
                    mathparser.Specials.Add("incentives", config.incentive.ToString("F0"));
                    config.SalaryBreakup.Values.ToList().Find(e => e.HeaderType == SalaryHeaderTypes.Incentives).Formula = config.incentive.ToString("F0");
                }

                decimal earnamt = 0;
                decimal dedamt = 0;
                InfyPOS.Data.SalaryManager.SalaryHeader roundingHeader = null;
                foreach (KeyValuePair<string, InfyPOS.Data.SalaryManager.SalaryHeader> kv in config.SalaryBreakup)
                {

                    if (kv.Value.HeaderType == SalaryHeaderTypes.BasicPay)
                        kv.Value.Formula = "BP";

                    if (kv.Value.HeaderType == SalaryHeaderTypes.Rounding)
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

                    if (kv.Value.HeaderType == SalaryHeaderTypes.FixedAllowance)
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
        public EmployeeSalaryConfig CalculateMonthly(EmployeeSalaryConfig config, List<InfyPOS.Data.SalaryManager.ScheduleAmount> schConfig)
        {
            //config = Newtonsoft.Json.JsonConvert.DeserializeObject<EmployeeSalaryConfig>(System.IO.File.ReadAllText(@"D:\json.txt"));
            //WebAPI.Common.Logger.Instance(Context.OrganizationID).Log(config.Name, WebAPI.Common.Logger.LogType.Info, config);
            config.WorkDayCount = days;
            DateTime monthlastday = todate;
            decimal basicPay = config.BasicPay;
            decimal grosspay = config.grosspay;
            decimal leaveDay = config.LeaveDays;
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveeligiblemode != 0)
                leaveDay += (config.SundayLeaves + (config.HolidayLeaves - config.HolidaySundayLeaves));
            
            config.LeaveTaken = config.LeaveDays;
            decimal adjustedJoiningResigning = 0;
            decimal eligibleleave = 0;
            decimal eligibleleavedays = 0;
            decimal holidayencashment = 0;
            decimal leaveencashment = 0;
            int workingdays = -1;
            TimeSpan experiance = monthlastday.Subtract(config.DateOfJoin);

            if (config.DateOfJoin >= fromdate && config.DateOfJoin <= todate)
            {
                adjustedJoiningResigning += (decimal)config.DateOfJoin.Subtract(fromdate).TotalDays;
                var totalleaves = SundayTotal(config.DateOfJoin, true);
                totalleaves += HolidayTotay(config.DateOfJoin, true);

                if (config.Relievingdate >= fromdate && config.Relievingdate <= todate)
                {
                    totalleaves += HolidayTotay(config.Relievingdate, false);
                    totalleaves += SundayTotal(config.Relievingdate, false);
                    adjustedJoiningResigning += (int)monthlastday.Subtract(config.Relievingdate).TotalDays;
                    monthlastday = config.Relievingdate;
                }
                decimal workeddays = (decimal)monthlastday.Subtract(config.DateOfJoin).TotalDays + (decimal)1;

                workeddays -= (totalleaves + leaveDay);

                leaveDay = days - workeddays;
                //leaveDay = leaveDay + (config.DateOfJoin.Day - 1)+SundayTotal(;
            }

            if (config.Hasleft && config.Relievingdate >= fromdate && config.Relievingdate <= todate)
            {
                adjustedJoiningResigning += (int)monthlastday.Subtract(config.Relievingdate).TotalDays;
                var workeddays = (decimal) config.Relievingdate.Subtract(fromdate).TotalDays + (decimal)1 - (SundayTotal(config.Relievingdate, false) + HolidayTotay(config.Relievingdate, false) + leaveDay);
                leaveDay = days - workeddays;
                monthlastday = config.Relievingdate;
            }


            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveeligiblemode <= 0 && InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveeligibledays <= 0)
            {
                basicPay = Math.Round(basicPay - ((config.BasicPay / config.WorkDayCount) * leaveDay), 0);
                grosspay = Math.Round(grosspay - ((config.grosspay / config.WorkDayCount) * leaveDay), 0);
                config.LeaveDays = leaveDay;
            }
            else
            {
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveeligiblemode <= 0)
                {
                    eligibleleave = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveeligibledays;
                    var addleavedays = 0;
                    if (config.DateOfJoin >= fromdate && config.DateOfJoin <= todate)
                        addleavedays = (int)(config.DateOfJoin.Subtract(fromdate).TotalDays);

                    if (config.Relievingdate >= fromdate && config.Relievingdate <= todate)
                        addleavedays += (int)monthlastday.Subtract(config.Relievingdate).TotalDays+1;

                    if (addleavedays > 0 && eligibleleave > 0)
                        eligibleleave = Math.Floor((((int)monthlastday.Subtract(fromdate).TotalDays + 1) - addleavedays) / (((int)monthlastday.Subtract(fromdate).TotalDays + 1) / eligibleleave));
                    
                    workingdays = (int)monthlastday.Subtract(fromdate).TotalDays + 1 - (int)eligibleleave;
                }
                else if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveeligiblemode == 1)
                {
                    eligibleleave = TotalSunday;
                    workingdays = (int)monthlastday.Subtract(fromdate).TotalDays + 1 - (int)eligibleleave;
                    if (config.DateOfJoin >= fromdate && config.DateOfJoin <= todate)
                        eligibleleave -= SundayTotal(config.DateOfJoin, true);

                    if (config.Relievingdate >= fromdate && config.Relievingdate <= todate)
                        eligibleleave -= SundayTotal(config.Relievingdate, false);
                }
                else if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveeligiblemode == 2)
                {
                    eligibleleave = TotalSunday + TotalHoliday;
                    workingdays = (int)monthlastday.Subtract(fromdate).TotalDays + 1 - (int)eligibleleave;
                    if (config.DateOfJoin >= fromdate && config.DateOfJoin <= todate)
                    {
                        eligibleleave -= SundayTotal(config.DateOfJoin, true);
                        eligibleleave -= HolidayTotay(config.DateOfJoin, true);
                    }
                    if (config.Relievingdate >= fromdate && config.Relievingdate <= todate)
                    {
                        eligibleleave -= SundayTotal(config.Relievingdate, false);
                        eligibleleave -= HolidayTotay(config.Relievingdate, false);
                    }
                }
                else
                {
                    eligibleleave = 0;
                }
                config.LeaveDays += adjustedJoiningResigning;
                leaveDay = config.LeaveDays - eligibleleave;
                if (leaveDay < 0)
                {
                    eligibleleavedays = leaveDay * -1;
                    leaveDay = 0;
                }
                if (leaveDay >= config.HolidayAbsent)
                    leaveDay -= config.HolidayAbsent;

                config.LeaveDays = leaveDay;
                if (leaveDay > 0)
                {
                    basicPay = Math.Round(basicPay - ((config.BasicPay / config.WorkDayCount) * leaveDay), 0);
                    grosspay = Math.Round(grosspay - ((config.grosspay / config.WorkDayCount) * leaveDay), 0);
                }
                else
                {
                    basicPay = config.BasicPay;
                    grosspay = config.grosspay;
                }
            }

            decimal leaveEncashmentBaseValue = config.grosspay;
            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashmentmode == 1)
                leaveEncashmentBaseValue = config.BasicPay;

            if (config.DateOfJoin.AddMonths(1) < monthlastday)
            {
                //pakshi
                if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.LeaveEncashmentMode == Configuration.ApplicationSettings.leaveencashmentmode.Auto)
                {
                    if (config.Encashmentmode == EmployeeSalaryConfig.LCModes.Fixed)
                    {
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashmentadjust)
                            leaveDay -= config.HolidayAbsent;

                        decimal paydays = days - leaveDay;
                        var avg_days = (days - InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashment) / InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashment;

                        if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashmentadjust)
                        {
                            avg_days = (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashment * 7) / InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashment;
                        }
                        if (paydays / avg_days > 0)
                        {
                            var leaveSalaryDay = Math.Floor(paydays / avg_days);

                            if (leaveSalaryDay > InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashment)
                                leaveSalaryDay = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashment;

                            if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashmentadjust)
                                leaveSalaryDay += (totalHolidayEncashment - config.HolidayLeaveEncashment);

                            if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashmentadjust)
                            {
                                if (leaveDay > 0 && leaveSalaryDay > 0)
                                {
                                    if (leaveDay > leaveSalaryDay)
                                    {
                                        leaveDay -= leaveSalaryDay;
                                        leaveSalaryDay = 0;
                                    }
                                    else
                                    {
                                        leaveSalaryDay -= leaveDay;
                                        leaveDay = 0;
                                    }
                                }
                                basicPay = Math.Round(config.BasicPay - ((config.BasicPay / config.WorkDayCount) * leaveDay), 0);
                                grosspay = Math.Round(config.grosspay - ((config.grosspay / config.WorkDayCount) * leaveDay), 0);
                            }

                            decimal leavesalary = Math.Round(leaveEncashmentBaseValue / days * leaveSalaryDay, 0, MidpointRounding.ToEven);;
                            leaveencashment = leavesalary;
                            holidayencashment = Math.Round(leaveEncashmentBaseValue / days * (totalHolidayEncashment - config.HolidayLeaveEncashment), 0, MidpointRounding.ToEven);
                            if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment))
                            {
                                config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment).Formula = leavesalary.ToString();
                                config.LeaveEncashmentDays = leaveSalaryDay;
                                config.LeaveEncashment = leavesalary;
                                config.HolidaySalary = holidayencashment;
                                config.HolidaySalaryDays = totalHolidayEncashment - config.HolidayLeaveEncashment;
                            }
                        }
                    }
                    else
                    {
                        if (config.Encashmentmode == EmployeeSalaryConfig.LCModes.Addtional ||
                            (config.Encashmentmode == EmployeeSalaryConfig.LCModes.Autometic &&
                            experiance.TotalDays > 365))
                        {
                            decimal paydays = days - leaveDay;
                            if (paydays / 7 > 0)
                            {
                                var leaveSalaryDay = Math.Floor(paydays / 7);
                                if (leaveSalaryDay > InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashment)
                                    leaveSalaryDay = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashment;

                                if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashmentadjust)
                                    leaveSalaryDay += (totalHolidayEncashment - config.HolidayLeaveEncashment);
                                decimal leavesalary = Math.Round(leaveEncashmentBaseValue / days * leaveSalaryDay, 0, MidpointRounding.ToEven);;
                                leaveencashment = leavesalary;
                                holidayencashment = Math.Round(leaveEncashmentBaseValue / days * (totalHolidayEncashment - config.HolidayLeaveEncashment), 0, MidpointRounding.ToEven);
                                if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment))
                                {
                                    config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment).Formula = leavesalary.ToString();
                                    config.LeaveEncashmentDays = leaveSalaryDay;
                                    config.LeaveEncashment = leavesalary;
                                    config.HolidaySalary = holidayencashment;
                                    config.HolidaySalaryDays = totalHolidayEncashment - config.HolidayLeaveEncashment;
                                }
                            }
                        }
                        else if (config.Encashmentmode == EmployeeSalaryConfig.LCModes.Adjustment ||
                            (config.Encashmentmode == EmployeeSalaryConfig.LCModes.Autometic &&
                            experiance.TotalDays > 90))
                        {
                            decimal paydays = days - leaveDay;
                            if (paydays / 7 > 0)
                            {
                                decimal adjavail = Math.Floor(paydays / 7);
                                decimal leaveSalaryDay = (leaveDay - adjustedJoiningResigning) < adjavail ? adjavail - (leaveDay - adjustedJoiningResigning) : 0;

                                if (leaveSalaryDay > InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashment)
                                    leaveSalaryDay = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashment;

                                if (!InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashmentadjust)
                                    leaveSalaryDay += (totalHolidayEncashment - config.HolidayLeaveEncashment);
                                decimal leavesalary = Math.Round(leaveEncashmentBaseValue / days * leaveSalaryDay, 0, MidpointRounding.ToEven);
                                leaveencashment = leavesalary;
                                holidayencashment = Math.Round(leaveEncashmentBaseValue / days * totalHolidayEncashment, 0, MidpointRounding.ToEven);
                                if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment))
                                {
                                    config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment).Formula = leavesalary.ToString();
                                    config.LeaveEncashmentDays = leaveSalaryDay;
                                    config.LeaveEncashment = leavesalary;
                                    config.HolidaySalary = holidayencashment;
                                    config.HolidaySalaryDays = totalHolidayEncashment - config.HolidayLeaveEncashment;
                                }
                            }
                        }
                        else
                        {
                            if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment))
                            {
                                config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment).Formula = "0";
                            }
                        }
                    }
                }
                else
                {
                    if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment))
                    {
                        var forumula = config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment).Formula;

                        if (string.IsNullOrEmpty(forumula)) forumula = "0";
                        MathParserTK.MathParser mathParser = new MathParserTK.MathParser(organizationid);
                        var multiplier = mathParser.Parse(forumula, true);
                        if (multiplier <= 0) multiplier = 1;

                        decimal eligibleencashmentdays = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashment;
                        decimal available = 0;
                        //[{ 'id': 0, 'name': 'Fixed Days' }, { 'id': 1, 'name': 'Sundays' }, { 'id': 2, 'name': 'Sundays+Holidays' }, { 'id': 3, 'name': 'Auto' }, { 'id': 4, 'name': 'None' }];
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveeligiblemode == 0)
                            available = InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashment;
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveeligiblemode == 2)
                            available = TotalSunday;
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveeligiblemode == 3)
                            available = TotalSunday + TotalHoliday;
                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveeligiblemode == 4)
                            available = Math.Floor((config.WorkDayCount - leaveDay) / 7);

                        if (eligibleencashmentdays > available)
                            eligibleencashmentdays = available;

                        if ((totalHolidayEncashment - config.HolidayLeaveEncashment) > 0)
                        {
                            if (!config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryHeaderTypes.HolidaySalary))
                            {
                                eligibleencashmentdays += (totalHolidayEncashment - config.HolidayLeaveEncashment);
                            }
                        }
                       

                        if (InfyPOS.Services.ConfigManager.Instance(Context.OrganizationID).Config.payroll_leaveencashmentadjust)
                        {
                            eligibleencashmentdays -= (config.LeaveTaken- config.HolidayAbsent);
                        }
                        if (eligibleencashmentdays < 0)
                            eligibleencashmentdays = 0;

                        decimal leavesalary = Math.Round(leaveEncashmentBaseValue / days * eligibleencashmentdays * (decimal)multiplier, 0, MidpointRounding.ToEven);

                        leaveencashment = leavesalary;
                        config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment).Formula = leavesalary.ToString();
                        config.LeaveEncashmentDays = eligibleencashmentdays;
                        config.LeaveEncashment = leavesalary;
                    }

                    holidayencashment = Math.Round(leaveEncashmentBaseValue / days * (totalHolidayEncashment - config.HolidayLeaveEncashment), 0, MidpointRounding.ToEven);
                    config.HolidaySalary = holidayencashment;
                    config.HolidaySalaryDays = totalHolidayEncashment - config.HolidayLeaveEncashment;
                }
            }
            else
            {
                if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment))
                {
                    config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment).Formula = "0";
                }
            }

            if (config.SalaryBreakup.Values.Any(e => e.HeaderType == SalaryHeaderTypes.Incentives))
            {
                config.SalaryBreakup.Values.First(e => e.HeaderType == SalaryHeaderTypes.Incentives).Formula = "0";
            }


            basicPay = Math.Round(basicPay, 0);
            grosspay = Math.Round(grosspay, 0);
            MathParserTK.MathParser mathparser = new MathParserTK.MathParser(organizationid);
            mathparser.Specials.Add("bp", basicPay.ToString());
            mathparser.Specials.Add("advance", "0");
            mathparser.Specials.Add("gp", grosspay.ToString());
            mathparser.Specials.Add("fp", config.grosspay.ToString());
            mathparser.Specials.Add("ot", Math.Round(config.OverTimePay, 0).ToString());
            mathparser.Specials.Add("round", "0");
            mathparser.Specials.Add("days", days.ToString());
            mathparser.Specials.Add("leavedays", leaveDay.ToString());
            mathparser.Specials.Add("workdays", ((days - adjustedJoiningResigning) - config.LeaveTaken).ToString());
            mathparser.Specials.Add("holidayabsent", config.HolidayAbsent.ToString());
            mathparser.Specials.Add("holidayleave", config.HolidayLeaves.ToString());

            if (config.SalaryBreakup.Values.ToList().Exists(e => e.HeaderType == SalaryHeaderTypes.HolidaySalary))
            {
                mathparser.Specials.Add(config.SalaryBreakup.Values.ToList().Find(e => e.HeaderType == SalaryHeaderTypes.HolidaySalary).Code.ToLower(), config.HolidaySalary.ToString());
                mathparser.Specials.Add("holidayencashment", config.HolidaySalary.ToString("F0"));
                config.SalaryBreakup.Values.ToList().Find(e => e.HeaderType == SalaryHeaderTypes.HolidaySalary).Formula = config.HolidaySalary.ToString("F0");
            }


            if (config.SalaryBreakup.Values.ToList().Exists(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment))
            {
                mathparser.Specials.Add(config.SalaryBreakup.Values.ToList().Find(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment).Code.ToLower(), leaveencashment.ToString());
                mathparser.Specials.Add("leaveencashment", config.LeaveEncashment.ToString("F0"));
                config.SalaryBreakup.Values.ToList().Find(e => e.HeaderType == SalaryHeaderTypes.LeaveEncashment).Formula = config.LeaveEncashment.ToString("F0");
            }

            if (config.SalaryBreakup.Values.ToList().Exists(e => e.HeaderType == SalaryHeaderTypes.Incentives))
            {
                mathparser.Specials.Add("incentives", config.incentive.ToString("F0"));
                config.SalaryBreakup.Values.ToList().Find(e => e.HeaderType == SalaryHeaderTypes.Incentives).Formula = config.incentive.ToString("F0");
            }

            decimal earnamt = 0;
            decimal dedamt = 0;
            InfyPOS.Data.SalaryManager.SalaryHeader roundingHeader = null;

            if (schConfig != null && schConfig.Count > 0)
            {
                foreach (ScheduleAmount sa in schConfig)
                {
                    if (DefaultHeaders.ContainsKey(sa.HeaderID) && config.SalaryBreakup.ContainsKey(DefaultHeaders[sa.HeaderID].Code.ToUpper()))
                    {
                        config.SalaryBreakup[DefaultHeaders[sa.HeaderID].Code.ToUpper()].Formula = (sa.Amount + config.SalaryBreakup[DefaultHeaders[sa.HeaderID].Code.ToUpper()].Amount).ToString("F0");
                        config.SalaryBreakup[DefaultHeaders[sa.HeaderID].Code.ToUpper()].Amount += sa.Amount;


                        if (mathparser.Specials.ContainsKey(DefaultHeaders[sa.HeaderID].Code.ToLower()))
                            mathparser.Specials.Remove(DefaultHeaders[sa.HeaderID].Code.ToLower());

                        mathparser.Specials.Add(DefaultHeaders[sa.HeaderID].Code.ToLower(), config.SalaryBreakup[DefaultHeaders[sa.HeaderID].Code.ToUpper()].Amount.ToString("F0"));
                    }
                    else if (DefaultHeaders.ContainsKey(sa.HeaderID))
                    {
                        SalaryHeader sh = DefaultHeaders[sa.HeaderID].Clone();
                        config.SalaryBreakup.Add(sh.Code.ToUpper(), sh);
                        sh.Amount = sa.Amount;
                        sh.Formula = sa.Amount.ToString("F0");
                        if (mathparser.Specials.ContainsKey(DefaultHeaders[sa.HeaderID].Code.ToLower()))
                            mathparser.Specials.Remove(DefaultHeaders[sa.HeaderID].Code.ToLower());
                        mathparser.Specials.Add(DefaultHeaders[sa.HeaderID].Code.ToLower(), sh.Amount.ToString("F0"));
                    }
                }

            }

            foreach (KeyValuePair<string, InfyPOS.Data.SalaryManager.SalaryHeader> kv in config.SalaryBreakup.OrderBy(e => e.Value.OrderID))
            {
                if (kv.Value.HeaderType == SalaryHeaderTypes.BasicPay)
                    kv.Value.Formula = "BP";

                if (kv.Value.HeaderType == SalaryHeaderTypes.Rounding)
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


                //if(kv.Value.HeaderType == SalaryHeaderTypes.ProvidentFund)
                //{
                //    if(config.DateOfBirth.Year >1000 &&
                //        config.DateOfBirth.AddYears(58)<salarymonth)
                //    {
                //        baseAmt = 0;
                //    }
                //}

                if (kv.Value.HeaderType == SalaryHeaderTypes.FixedAllowance)
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
                else if (kv.Value.HeaderType == SalaryHeaderTypes.FineDeduction)
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
                    if (!mathparser.Specials.ContainsKey(kv.Value.Code))
                        mathparser.Specials.Add(kv.Value.Code, kv.Value.Amount.ToString("F0"));
                }
                else if (kv.Value.HeaderType == SalaryHeaderTypes.FixedDeduction)
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
                    if (!mathparser.Specials.ContainsKey(kv.Value.Code))
                        mathparser.Specials.Add(kv.Value.Code, kv.Value.Amount.ToString("F0"));
                }
                else if (kv.Value.HeaderType == SalaryHeaderTypes.Incentives)
                {
                    var formulaparts = kv.Value.Formula.Split(new char[] { '-', ':' });
                    kv.Value.Amount = (decimal)mathparser.Parse(formulaparts[0], false);
                }
                else
                {
                    double amt = 0;
                    double baseAmt = 0;
                    if (double.TryParse(kv.Value.Formula, out amt))
                    {
                        baseAmt = amt;

                        if (!kv.Value.IsFullAmount)
                        {
                            if (config.DateOfJoin >= fromdate && config.DateOfJoin <= todate)
                            {
                                baseAmt = (double)((baseAmt / (double)days) * (double)(days - leaveDay));
                            }
                            else if (leaveDay > 0)
                            {
                                baseAmt = baseAmt - ((amt / (double)days) * (double)leaveDay);
                            }
                        }
                        baseAmt = kv.Value.Code.Contains("ESI") ? Math.Ceiling(baseAmt) : Math.Round(baseAmt, 0);
                    }

                    else
                    {
                        amt = mathparser.Parse(kv.Value.Formula, false);
                        baseAmt = kv.Value.Code.Contains("ESI") ? Math.Ceiling(amt) : Math.Round(amt, 0);
                    }
                    kv.Value.Amount = (decimal)baseAmt;
                    if (mathparser.Specials.ContainsKey(kv.Value.Code.ToLower()))
                        mathparser.Specials.Remove(kv.Value.Code.ToLower());

                    mathparser.Specials.Add(kv.Value.Code.ToLower(), baseAmt.ToString("F0"));
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
            }



            if (roundingHeader != null)
            {
                decimal netamt = earnamt - dedamt;
                decimal round = netamt % 10;
                roundingHeader.Amount = round != 0 ? 10 - round : 0;
            }
            if (workingdays > 0)
                config.AcceptedWorkDays = workingdays;

            if (adjustedJoiningResigning > 0)
            {
                config.LeaveDays -= (adjustedJoiningResigning+eligibleleave);
                config.WorkDayCount -= adjustedJoiningResigning;
                config.AdjustedJoiningResigning = adjustedJoiningResigning;
                config.AcceptedWorkDays = TotalWorkingDays - adjustedJoiningResigning;
            }

            return config;
        }

        private int HolidayTotay(DateTime dateOfJoin, bool isfrom)
        {
            if (isfrom)
                return holidayList.Count(e => e >= dateOfJoin);
            else
                return holidayList.Count(e => e <= dateOfJoin);
        }
    }

}
