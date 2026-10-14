using InfyPOS.Common;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
namespace InfyPOS.Models
{
    public class EmployeeHeader : BaseEntity
    {
        public long sectionid { get; set; }
        public string name { get; set; }
        public long id { get; set; }
        public string employeecode { get; set; }
    }

    public class UserEmployeeLink : EmployeeHeader
    {
        public long employeeid { get; set; }
    }
    public class Employee : EmployeeHeader
    {
        public List<string> codes { get; set; }
        public List<long> ids { get; set; }
        public long title { get; set; }
        public string titlename
        {
            get
            {
                if (title == 0) return "Mr.";
                else if (title == 1) return "Mrs.";
                else return "Ms";
            }
        }
        public string emailid { get; set; }
        public string contactno { get; set; }
        public long desingnationid { get; set; }
        public long companyid { get; set; }
        public string refempid { get; set; }
        public long roleid { get; set; }
        public bool isactive { get; set; }
        public DateTime dateofbirth { get; set; }
        public bool hasot { get; set; }

        public bool hasmorningot { get; set; }

        public bool hasbeta { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public int resignreasonid { get; set; }
        public DateTime createdon { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime dateofjoining { get; set; }
        public DateTime relievingdate { get; set; }
        public bool allowsystemaccess { get; set; }
        public string username { get; set; }
        public long workingmode { get; set; }
        public long workinghour { get; set; }
        public string date_part { get; set; }
        public int salarymode { get; set; }
        public string surname { get; set; }
        public long managerid { get; set; }

        public long defaultshiftid { get; set; }

        public string cardcode { get; set; }
        public string gender { get; set; }
        public string bloodgroup { get; set; }
        public string caste { get; set; }
        public string religion { get; set; }
        public int monthlymandatorydays { get; set; }
        public bool hasleft { get; set; }
        public string companylogo { get; set; }

        public string pfno { get; set; }
        public string esino { get; set; }
        public string pfuan { get; set; }




        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string personal_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(personal); }
            set
            {
                if (value != null)
                    personal = Newtonsoft.Json.JsonConvert.DeserializeObject<Personal>(value);
            }
        }

        public int lcmode { get; set; }

        public bool haswithheld { get; set; }

        public DateTime withheldfrom { get; set; }
        public Personal personal { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string family_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(family); }
            set
            {
                if (value != null)
                    family = Newtonsoft.Json.JsonConvert.DeserializeObject<Family>(value);
            }

        }
        public Family family { get; set; }
        public long releavingreason { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string pfandesi_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(pfandesi); }
            set
            {
                if (value != null)
                    pfandesi = Newtonsoft.Json.JsonConvert.DeserializeObject<Pfandesi>(value);
            }
        }
        public Pfandesi pfandesi { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string presentaddress_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(presentaddress); }
            set
            {
                if (value != null)
                    presentaddress = Newtonsoft.Json.JsonConvert.DeserializeObject<Presentaddress>(value);
            }
        }
        public Presentaddress presentaddress { get; set; }
        public string emppresentaddress
        {
            get
            {
                if (presentaddress != null) return presentaddress.formatedaddress;
                return "";
            }
        }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string permanentaddress_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(permanentaddress); }
            set
            {
                if (value != null)
                    permanentaddress = Newtonsoft.Json.JsonConvert.DeserializeObject<Permanentaddress>(value);
            }
        }
        public Permanentaddress permanentaddress { get; set; }
        public string emppermanentaddress
        {
            get
            {
                if (permanentaddress != null) return permanentaddress.formatedaddress;
                return "";
            }
        }
        public string aadharno { get; set; }
        public string pancardno { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string remarks_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(remarks); }
            set
            {
                if (value != null)
                    remarks = Newtonsoft.Json.JsonConvert.DeserializeObject<Remarks>(value);
            }
        }
        public Remarks remarks { get; set; }
        public long locationid { get; set; }
        public long floorid { get; set; }
        public long departmentid { get; set; }
        public long sectionid { get; set; }
        public string sectionname { get; set; }
        public long bankid { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string classification_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(classification); }
            set
            {
                if (value != null)
                    classification = Newtonsoft.Json.JsonConvert.DeserializeObject<Classification>(value);

                if (classification == null)
                    classification = new Classification();
            }
        }
        public Classification classification { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string addionalinfo_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(addionalinfo); }
            set
            {
                if (value != null)
                    addionalinfo = Newtonsoft.Json.JsonConvert.DeserializeObject<Addionalinfo>(value);

                if (addionalinfo == null)
                    addionalinfo = new Addionalinfo();
            }
        }
        public Addionalinfo addionalinfo { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string passport_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(passport); }
            set
            {
                if (value != null)
                    passport = Newtonsoft.Json.JsonConvert.DeserializeObject<Passport>(value);

            }
        }
        public Passport passport { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string trainings_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(trainings); }
            set
            {
                if (value != null)
                    trainings = Newtonsoft.Json.JsonConvert.DeserializeObject<Trainings>(value);

                if (trainings == null)
                    trainings = new Trainings();
            }
        }
        public Trainings trainings { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string accident_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(accident); }
            set
            {
                if (value != null)
                    accident = Newtonsoft.Json.JsonConvert.DeserializeObject<Accident>(value);
                if (accident == null)
                    accident = new Accident();
            }
        }
        public Accident accident { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string disciplinary_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(disciplinary); }
            set
            {
                if (value != null)
                    disciplinary = Newtonsoft.Json.JsonConvert.DeserializeObject<Disciplinary>(value);
                if (disciplinary == null)
                    disciplinary = new Disciplinary();
            }
        }
        public Disciplinary disciplinary { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string educationandexperiance_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(educationandexperiance); }
            set
            {
                if (value != null)
                    educationandexperiance = Newtonsoft.Json.JsonConvert.DeserializeObject<EducationAndExperiance>(value);

                if (educationandexperiance == null)
                    educationandexperiance = new EducationAndExperiance();

                if (educationandexperiance.items == null)
                    educationandexperiance.items = new EducationAndExperiance.item[0];

                foreach (var itm in educationandexperiance.items)
                {
                    if (!string.IsNullOrEmpty(itm.mode))
                    {
                        if (itm.mode.ToLower() == "true")
                            itm.mode = "Education";
                        if (itm.mode.ToLower() == "false")
                            itm.mode = "Experiance";
                    }
                }
            }
        }
        public EducationAndExperiance educationandexperiance { get; set; }
        [JsonIgnore]
        public List<EducationAndExperiance.item> education {

            get
            {
                if (educationandexperiance == null || educationandexperiance.items == null) return new List<EducationAndExperiance.item>();
                int sno = 1;
                var list = educationandexperiance.items.Where(e => e.mode == "Education").ToList();
                list.ForEach(e => e.sno = sno++);

                return list;

            }


        }

        [JsonIgnore]
        public List<EducationAndExperiance.item> experiance
        {
            get
            {
                if (educationandexperiance == null || educationandexperiance.items == null) return new List<EducationAndExperiance.item>();
                int sno = 1;
                var list = educationandexperiance.items.Where(e => e.mode != "Education").ToList();
                list.ForEach(e => e.sno = sno++);

                return list;

            }

        }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string license_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(license); }
            set
            {
                if (value != null)
                    license = Newtonsoft.Json.JsonConvert.DeserializeObject<License>(value);
            }
        }
        public License license { get; set; }
        public long castecategory { get; set; }
        public long qualification { get; set; }
        public long nationality { get; set; }
        public long language { get; set; }
        public byte[] images_photo { get; set; }
        public byte[] images_signature { get; set; }
        public byte[] images_thumb { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string bank_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(bank); }
            set
            {
                if (value != null)
                    bank = Newtonsoft.Json.JsonConvert.DeserializeObject<Bank>(value);

                if (bank == null)
                    bank = new Bank();
            }
        }
        public Bank bank { get; set; }

        public string salaryconfig_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(salaryconfig); }
            set
            {
                if (value != null)
                    salaryconfig = Newtonsoft.Json.JsonConvert.DeserializeObject<List<SalaryItem>>(value);

                if (salaryconfig == null)
                    salaryconfig = new List<SalaryItem>();
            }
        }
        public List<SalaryItem> salaryconfig { get; set; }
        public Company company { get; set; }
        public long salarystructureid { get; set; }
        public decimal grosspay { get; set; }
        public decimal ogrosspay { get; set; }
        public long osalarystructureid { get; set; }
        public string designation { get; set; }
        public string department { get; set; }
        public DateTime resignationdate { get; set; }
        public decimal incentive { get; set; }
        public override string GetMobileNo()
        {
            return contactno;
        }
        public override string GetEmailID()
        {
            return emailid;
        }
        public class SalaryItem
        {
            public long id { get; set; }
            public string formula { get; set; }
            public DateTime from { get; set; }
        }

        public class Personal
        {
            public string bloodgroup { get; set; }
            public string bloodgroupname { get; set; }
            public DateTime dob { get; set; }
            public string fathername { get; set; }
            public string mothername { get; set; }
            public string spousename { get; set; }
            public bool married { get; set; }
            public string maritalstatus { get; set; }
            public string caste { get; set; }
            public string castename { get; set; }
            public string community { get; set; }
            public string communityname { get; set; }
            public string languagesknown { get; set; }
            public string religion { get; set; }
            public string nationality { get; set; }
            public DateTime marrieddate { get; set; }
            public string aadharno { get; set; }
            public string smartcardno { get; set; }
            public string img_idcard { get; set; }
            public string img_photo { get; set; }
            public string img_signature { get; set; }
            public string img_thumb { get; set; }
            public string img_aadhar { get; set; }
            public string img_bankbook { get; set; }
            public string strength { get; set; }
            public string expdesignation { get; set; }
            public string expsalary { get; set; }
            public bool livewithmother { get; set; }
            public bool livewithfather { get; set; }
            public bool alivewithfather { get; set; }
            public bool alivewithmother { get; set; }
            public bool alivewithwife { get; set; }
            public string nprno { get; set; }

            public string slivewithmother
            {
                get
                {
                    return livewithmother ? "YES" : "NO";
                }
            }
            public string slivewithfather
            {
                get
                {
                    return livewithfather ? "YES" : "NO";
                }
            }
            public string slivewithwife
            {
                get
                {
                    return livewithwife ? "YES" : "NO";
                }
            }

            public string fatherno { get; set; }
            public string motherno { get; set; }
            public string spouseno { get; set; }
            public bool livewithwife { get; set; }
            public string panno { get; set; }

        }
        public class Family
        {
            public class item
            {
                public string name { get; set; }
                public string age { get; set; }
                public string gender { get; set; }
                public string qualification { get; set; }
                public string relation { get; set; }
                public string remarks { get; set; }
                public bool dependent { get; set; }
                public DateTime dateofbirth { get; set; }
            }
            public item[] items { get; set; }
        }
        public class Pfandesi
        {
            public string pfno { get; set; }
            public string pfuan { get; set; }
            public string esino { get; set; }
            public bool esienabled { get; set; }
            public bool pfenabled { get; set; }
            public DateTime esienabledon { get; set; }
            public DateTime esienabledto { get; set; }
            public DateTime pffrom { get; set; }
            public DateTime pfto { get; set; }
            public DateTime restrictpffrom { get; set; }
            public DateTime restrictpfto { get; set; }
            public DateTime zeropffrom { get; set; }
            public DateTime zeropfto { get; set; }
            public DateTime pfenabledon { get; set; }
        }

        public class address
        {
            public string residentno { get; set; }
            public string residentname { get; set; }
            public string street { get; set; }
            public string area { get; set; }
            public string village { get; set; }
            public string city { get; set; }
            public string district { get; set; }
            public string state { get; set; }
            public string pincode { get; set; }
            public string cityname { get; set; }
            public string districtname { get; set; }
            public string statename { get; set; }
        }

        public class Presentaddress : address
        {
            public string formatedaddress
            {
                get
                {
                    return string.Format("{0} ,{1} ,{2} ,{3} ,{4} ,{5} ,{6} ,{7} ,{8}", residentno, residentname, street, area, village, cityname, districtname, statename, pincode);
                }
            }
        }
        public class Permanentaddress : address
        {
            public string formatedaddress
            {
                get
                {
                    return string.Format("{0} ,{1} ,{2} ,{3} ,{4} ,{5} ,{6} ,{7} ,{8}", residentno, residentname, street, area, village, cityname, districtname, statename, pincode);
                }
            }
        }

        public class Remarks
        {

            public string text { get; set; }
        }

        public class keyvalue
        {
            public string key { get; set; }
            public string value { get; set; }
            public DateTime recordedon { get; set; }
        }

        public class Classification
        {
            public item[] items { get; set; }
            public class item
            {
                public string validfrom { get; set; }
                public string validto { get; set; }
                public string designation { get; set; }
                public string department { get; set; }
                public string section { get; set; }
                public string floor { get; set; }
                public string division { get; set; }
                public string location { get; set; }
            }
        }

        public class Addionalinfo
        {
            public keyvalue[] items { get; set; }
        }
        public class Passport
        {
            public string number { get; set; }
            public string issuedat { get; set; }
            public DateTime issuedon { get; set; }
            public DateTime expiryon { get; set; }
        }
        public class Trainings
        {
            public class item
            {
                public string probationname { get; set; }
                public DateTime confirmationdate { get; set; }
                public DateTime completiondate { get; set; }
                public DateTime givenon { get; set; }
                public string remark { get; set; }
                public string noticeperiod { get; set; }
            }
            public item[] items { get; set; }
        }
        public class Accident
        {
            public class item
            {
                public string name { get; set; }
                public DateTime accidenton { get; set; }
                public string hospital { get; set; }
                public string bodylocation { get; set; }
                public string accidentlocation { get; set; }
            }
            public item[] items { get; set; }
        }
        public class Disciplinary
        {
            public class item
            {
                public string name { get; set; }
                public string identifiedby { get; set; }
                public DateTime occuredon { get; set; }
                public string remark { get; set; }
            }
            public item[] items { get; set; }
        }
        public class EducationAndExperiance
        {
            public item[] items { get; set; }
            public class item
            {
                public long educationtype { get; set; }
                public string name { get; set; }
                public string institution { get; set; }
                public string remark { get; set; }
                public string year { get; set; }
                public string mode { get; set; }
                public string place { get; set; }
                public decimal salary { get; set; }
                [JsonIgnore]
                public int sno { get; set; }
            }
        }
        public class License
        {
            public string no { get; set; }
            public DateTime issuedon { get; set; }
            public string issuedat { get; set; }
            public DateTime expiryon { get; set; }
        }
        public class Bank
        {
            public long bankid { get; set; }
            public string accountname { get; set; }
            public string ifsc { get; set; }
            public string accountno { get; set; }
        }
    }
    public enum LeaveEncashmentMode
    {
        None,
        Additional,
        Adjustable
    }
    public class EmployeeBankreport : BaseEntity
    {

        public long id { get; set; }
        public bool isactive { get; set; }
        public DateTime modifiedon { get; set; }
        public DateTime createdon { get; set; }
        public long createdby { get; set; }
        public long modifiedby { get; set; }
        public int version { get; set; }
        public long employeeid { get; set; }
        public string accountno { get; set; }
        public decimal netsalary { get; set; }

        public string name { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string detail_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(detail); } set { detail = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Detail>>(value); } }
        public List<Detail> detail { get; set; }

        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public DateTime salarymonth { get; set; }
        public string employeeno { get; set; }
        public string designation { get; set; }
        public string jobtitle { get; set; }
        public string pfno { get; set; }
        public long companyid { get; set; }
        public string companyname { get; set; }

        public string employeecode { get; set; }

        public string bankname { get; set; }

        public string bank_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(bank); }
            set
            {
                if (value != null)
                    bank = Newtonsoft.Json.JsonConvert.DeserializeObject<Bank>(value);

                if (bank == null)
                    bank = new Bank();
            }
        }
        public Bank bank { get; set; }

        public string accountname { get; set; }
        public string ifsc { get; set; }

        public class Bank
        {

            public string accountno { get; set; }
            public string accountname { get; set; }
            public string ifsc { get; set; }
        }
        public class Detail
        {
            public long id { get; set; }
            public decimal amount { get; set; }
            public bool isdeduction { get; set; }
            public decimal value { get; set; }
        }
    }
    public class EmployeeBankCriteria : BaseEntity
    {
        public long employeeid { get; set; }
        public long companyid { get; set; }
        public int filtermode { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public long floorid { get; set; }
        public List<long> ids { get; set; }
        public DateTime fromdate { get; set; }
        public DateTime todate { get; set; }
    }
    public class EmployeeCreteria : Employee
    {
        public DateTime CurrentAsOnDate { get; set; }
        public bool forapplication { get; set; }
        public bool includeleft { get; set; }
        public bool showcontact { get; set; }
        public bool onlysalesmen { get; set; }
        public bool onlypurchasemanager { get; set; }
        public bool currentEmployees { get; set; }
        public DateTime dobfromdate { get; set; }
        public DateTime dobtodate { get; set; }
        public DateTime anniversaryfromdate { get; set; }
        public DateTime anniversarytodate { get; set; }
        public bool hidesalaryinfo { get; set; }
        public long standardroleid { get; set; }
    }

    public class SalesmanPrintCreteria : BaseEntity
    {
        public int copy { get; set; }
        public long reportid { get; set; }
        public List<Salesman2Print> items { get; set; }
        public string companytitle { get; set; }

    }

    public class Salesman2Print : BaseEntity, Report.IBarcodeFormat
    {
        public bool SpecialField(string field, SortedDictionary<string, Object> data)
        {
            if (data.ContainsKey(field)) return true;
            try
            {
                var fields = field.Split(':');

                // SPL:SPLIT:NAME:23:1
                if (fields[1] == "SPLIT" && fields.Length == 5)
                {
                    if (KeyValue.ContainsKey(fields[2]))
                    {
                        int length = 0;
                        int position = 0;
                        if (int.TryParse(fields[3], out length) &&
                            int.TryParse(fields[4], out position))
                        {
                            var fullvalue = KeyValue[fields[2]].ToString();
                            int startindex = (position - 1) * length;
                            if (fullvalue.Length < startindex)
                            {
                                data.Add(field, "");
                                return true;
                            }
                            if (startindex + length > fullvalue.Length)
                            {
                                length = fullvalue.Length - startindex;
                            }
                            data.Add(field, fullvalue.Substring(startindex, length));
                            return true;
                        }
                    }
                }

            }
            catch (Exception exp)
            {

            }
            return false;
        }
        public string code { get; set; }
        public string name { get; set; }
        public string id { get; set; }

        // these two properties for vouche print
        public decimal value { get; set; }
        public DateTime validupto { get; set; }

        public string ncode {
            get
            {
                int numcode = 0;
                int.TryParse(code, out numcode);
                return numcode.ToString("N0");
            }
        }

        public SortedDictionary<string, object> KeyValue
        {
            get
            {
                SortedDictionary<string, object> ret = new SortedDictionary<string, object>();
                ret.Add("CODE", code);
                ret.Add("NCODE", ncode);
                ret.Add("ID", id);
                ret.Add("NAME", name);
                ret.Add("VALUE", value.ToString("F0"));
                ret.Add("VALIDTO", validupto.ToString("dd-MM-yyyy"));
                return ret;
            }

        }

        public SortedDictionary<string, string> KeyFormat
        {
            get
            {
                SortedDictionary<string, string> ret = new SortedDictionary<string, string>();
                ret.Add("ID", "N0");
                ret.Add("CODE", "N0");
                ret.Add("VALUE", "F0");
                ret.Add("VALIDTO", "dd-MM-yyyy");
                return ret;
            }
        }


    }

    public class Employeedetails : Employee
    {
        public string logo { get; set; }
    }
}
