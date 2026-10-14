using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace InfyPOS.Payroll
{
    public class ESIPF : Common.BaseData
    {
        
        public ESIReport Compute(ESIRequest request)
        {
            ESIReport report = new ESIReport();
            Data.ReferencelistDataService referencelist = new Data.ReferencelistDataService() { Context = this.Context };
            var paycategorylist = referencelist.Search(new Models.ReferencelistCreteria()
            {
                type = "PAYCATEGORY"
            });

            List<long> esihead = new List<long>();
            List<long> pfhead = new List<long>();
            foreach(var item in paycategorylist)
            {
                if (item.additionalinfo != null && item.additionalinfo.includeinesi)
                    esihead.Add(item.id);

                if (item.additionalinfo != null && item.additionalinfo.includeinpf)
                    pfhead.Add(item.id);
            }

            Data.CompanyDataService companydata = new Data.CompanyDataService() { Context = this.Context };
            var complist = companydata.Search(new Models.CompanyCreteria()
            { id= request.companyid });

            report.salarymonth = new DateTime(request.year, request.month, 1);
            if (complist.Count ==1)
            {
                report.companyname = complist[0].name;
                report.esino = complist[0].esino;
                
            }

            Data.EmployeesalaryDataService salarydata = new Data.EmployeesalaryDataService() { Context = this.Context };
            var list = salarydata.SearchESIPF(request);
            foreach(var item in list)
            {
                item.esiearning = item.detail.Sum(e => esihead.Contains(e.id) ? e.amount : 0);
                item.pfearning = item.detail.Sum(e => pfhead.Contains(e.id) ? e.amount : 0);
                //if (item.pfandesi != null)
                //{
                //    item.esino = item.pfandesi.esino;
                //    item.pfno = item.pfandesi.pfno;
                //}
                item.pfpension =  Math.Round( item.pfearning * (decimal) 8.33 / 100,0);
                item.pfcontribution = item.pf - item.pfpension;

                if(item.dateofbirth != DateTime.MinValue && item.dateofbirth.AddYears(58)< report.salarymonth)
                {
                    item.pfpension = 0;
                    item.pfcontribution = item.pf;
                }

                item.earnings = item.detail.Sum(e => e.isdeduction ? 0 : e.amount);
                item.deductions = item.detail.Sum(e => e.isdeduction ? e.amount : 0);
                item.detail = null;

            }

            if (request.isESI)
                list.RemoveAll(e => e.esi == 0);
            else
                list.RemoveAll(e => e.pf == 0);
            report.items = list.ToList();
            return report;
        }
    }

    public class ESIRequest : InfyPOS.Common.BaseEntity
    {
        public bool isESI { get; set; }
        public int month { get; set; }
        public int year { get; set; }
        public long companyid { get; set; }
    }
    public class ESI
    {
        public int sno { get; set; }
        public string name { get; set; }
        public string esino { get; set; }
        public decimal daysworked { get; set; }
        public decimal esiearning { get; set; }
        public decimal esi { get; set; }
        public decimal pf { get; set; }
        public decimal leavedays { get; set; }
        public decimal workingdays { get; set; }
        public string employeecode { get; set; }
        public string pfuan { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string pfandesi_json
        {
            get { return Newtonsoft.Json.JsonConvert.SerializeObject(pfandesi); }
            set
            {
                if (value != null)
                    pfandesi = Newtonsoft.Json.JsonConvert.DeserializeObject<Models.Employee.Pfandesi>(value);
            }
        }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public Models.Employee.Pfandesi pfandesi { get; set; }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public string detail_json { get { return Newtonsoft.Json.JsonConvert.SerializeObject(detail); } set { detail = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Models.Employeesalary.Detail>>(value); } }
        [JsonIgnore]
        [JsonProperty(Required = Required.Default)]
        public List<Models.Employeesalary.Detail> detail { get; set; }
        public decimal pfearning { get;  set; }
        public string pfno { get;  set; }
        public DateTime dateofbirth { get;  set; }
        public decimal pfpension { get;  set; }
        public decimal pfcontribution { get;  set; }
        public decimal earnings { get;  set; }
        public decimal deductions { get;  set; }
        public bool hasleft { get; internal set; }
        public DateTime relievingdate { get; internal set; }
        public int resignreasonid { get; internal set; }
    }

    public class ESIReport
    {
        public List<ESI> items { get; set; }
        public DateTime salarymonth { get; set; }
        public string companyname { get; set; }
        public string esino { get; set; }
    }
}