using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using InfyPOS.Models;
using static InfyPOS.Data.SalaryManager;

namespace InfyPOS.Payroll
{
    public class IncentiveSalary
    {
        public class IncentiveData
        {
            public long departmentid { get; set; }
            public string department { get; set; }
            public decimal Amount { get; set; }
            public decimal fixedpercentage { get; set; }
            public decimal dynamicpercentage { get; set; }
            public bool IsSales { get; set; }
            public List<Models.Referencelist.SalesIncentive> salesIncentives { get; set; }
            public IncentiveData()
            {
                Employees = new List<Item>();
                SalesValue = new List<salevalue>();
            }
            public List<Item> Employees { get; set; }
            public List<salevalue> SalesValue { get; set; }
            
            public class MfgIncentive
            {
                public string product { get; set; }
                public decimal percentage { get; set; }
                public decimal sales { get; set; }
                public decimal qty { get; set; }
                public decimal incentive { get; set; }
                public long productid { get; set; }
            }
            public class Item
            {
                public string employeename { get; set; }
                public string employeecode { get; set; }
                public DateTime dateofjoining { get; set; }
                public DateTime relievingdate { get; set; }
                public long sectionid { get; set; }
                public long departmentid { get; set; }
                public long companyid { get; set; }
                public long inchargeid { get; set; }
                public long employeeid { get; set; }
                public decimal ratio { get; set; }
                public decimal days { get; set; }
                public decimal amount { get; set; }
                public decimal grosspay { get; set; }
                public decimal payratio { get; set; }
                public long locationid { get; set; }
                public long floorid { get; set; }
                public decimal salestarget { get; set; }
                public decimal incentivepercentage { get; set; }
                public decimal incentiveratio { get; set; }
                public decimal salesminimumtarget { get; set; }

                public int incentivesource { get; set; }
                public int salestargetmethod { get; set; }
                public int incentivevaluetype { get; set; }
                public int incentivecalculationsource { get; set; }
                public int salestargetminimummode { get; set; }
                public decimal sectionincentive { get; set; }
                public decimal salesvalue { get; set; }
                public decimal itemincentive { get; set; }
                public decimal itemsalevalue { get; set; }
                public decimal incentivecount { get; set; }
                public int teammembers { get; set; }
                public List<salebyage> Salebyages { get; set; }
                public decimal incentive { get; set; }
                public bool IsIncharge { get; set; }
                public bool IsSalesmen { get; set; }
                public decimal earnings { get; set; }
                public decimal allocation { get; set; }
                public decimal totalpay { get; set; }
                public decimal distpercentage { get; set; }
                public IncentiveSources Incentivesource
                {
                    get
                    {
                        return (IncentiveSources)incentivesource;
                    }
                }

                public CalculationSources CalculationSource
                {
                    get
                    {
                        return (CalculationSources)incentivecalculationsource;
                    }
                }

                public TargetMinumumMethods TargetMinumumMethod
                {
                    get
                    {
                        return (TargetMinumumMethods)salestargetminimummode;
                    }
                }

                public ValueTypes ValueType
                {
                    get
                    {
                        return (ValueTypes)incentivevaluetype;
                    }
                }

                public enum TargetMethods
                {
                    NoTarget,
                    Fixed,
                    NoOfWorkingsXGross,
                    FixedDaysXGross
                }

                public TargetMethods TargetMethod
                {
                    get
                    {
                        return (TargetMethods)salestargetmethod;
                    }
                }

                public decimal productlevelincentive { get; set; }
                public decimal productlevelcount { get; set; }
                public decimal productlevelvalue { get; set; }
                public string remarks { get; set; }

                public decimal rolesupervisorincentive { get; set;  }
                public decimal rolesalesmenincentive { get; set; }
                public decimal prefixincentive { get; set; }
                public decimal prefixcount { get; set; }
                public decimal prefixvalue { get; set; }

                public decimal smincentive { get; set; }
                public decimal smcount { get; set; }
                public decimal smvalue { get; set; }

                public decimal salesqty { get; set; }
                public decimal workingdays { get; set; }
                public decimal additionaldiscount { get; set; }
                public decimal schemediscount { get; set; }
                public decimal totalworkingdays { get; set; }
                public decimal employeetailoringincentive { get; set; }
                public decimal employeeagerangeincentive { get; set; }
                public decimal productagepricerangeincentive { get; set; }
                public decimal productagerangeincentive { get; set; }
                public decimal productpricerangeincentive { get; set; }
                public decimal sectionagerangeincentive { get; set; }
                public decimal sectionpricerange { get; set; }
                public decimal salesslabincentive { get; set; }
                public decimal targetslapincentive { get; set; }
                public decimal targetincentive { get; set; }
                public decimal companyincentive { get; set; }
                public decimal fixedincentive { get; set; }
                public bool saleremoved { get; set; }
                public decimal targetamount { get; internal set; }
                public List<KeyValuePair<string, string>> notes { get;  set; }
                public decimal employeetailoringvalue { get; internal set; }
                public decimal employeetailoringqty { get; internal set; }

                public enum ValueTypes
                {
                    None,
                    Value,
                    Quantity
                }

                public enum TargetMinumumMethods
                {
                    Actual,
                    FixedValue,
                    PercentageOfTarget
                }

                public enum CalculationSources
                {
                    All,
                    Fixed,
                    ProductAgeSlab,
                    SectionAgeSlab,
                    ProductPriceSlab,
                    SectionPriceSlab,
                    TargetSlab,
                    ValueSlab,
                    Company,
                    ProductAgeAndPrice,
                    EmployeeProductAge,
                    EmployeeSection
                }


                public enum IncentiveSources
                {
                    None,
                    TotalSales,
                    LocationSales,
                    SectionSales,
                    FloorSales,
                    TeamSales,
                    Salesmen,
                    Company
                }

                private void AddIncentive(IncentiveEngine.IncentiveSource incentiveSource, decimal incentive)
                {

                    switch (incentiveSource)
                    {
                        case IncentiveEngine.IncentiveSource.EmployeeAgeRange:
                            employeeagerangeincentive += incentive;
                            break;
                        case IncentiveEngine.IncentiveSource.ProductAgeRange:
                            productagerangeincentive += incentive;
                            break;
                        case IncentiveEngine.IncentiveSource.ProductPriceRange:
                            productpricerangeincentive += incentive;
                            break;
                        case IncentiveEngine.IncentiveSource.ProductPriceAgeRange:
                            productagepricerangeincentive += incentive;
                            break;
                        case IncentiveEngine.IncentiveSource.SectionAgeRange:
                            sectionagerangeincentive += incentive;
                            break;
                        case IncentiveEngine.IncentiveSource.SectionPriceRange:
                            sectionpricerange += incentive;
                            break;
                    }
                }
                internal void AddSalebyages(IncentiveEngine.IncentiveSource incentiveSource,
                    List<IncentiveEngine.Salespersondata> ageresut)
                {
                    if (ageresut.Count == 0) return;
                    if (Salebyages == null) Salebyages = new List<salebyage>();
                    if (ValueType == ValueTypes.None || ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Value)
                    {
                        var ageincentive = ageresut.Sum(ex =>
                        {
                            var salevalue = ex.salevalue;
                            if (ex.range.value)
                                return ex.range.incentive * salevalue;
                            return ex.range.incentive * salevalue / 100;
                        });
                        ageincentive = Math.Round(ageincentive);
                        incentive += ageincentive;
                        amount += ageincentive;
                        AddIncentive(incentiveSource, ageincentive);

                        foreach (var item in ageresut.GroupBy(e => string.Format("{0} - {1}", e.range.start, e.range.end)))
                        {
                            Salebyages.Add(new salebyage()
                            {
                                name = item.Key,
                                from = item.First().range.start,
                                to = item.First().range.end,
                                incentivecount = Math.Round(item.Sum(e => e.saleqty)),
                                salevalue = Math.Round(item.Sum(e => e.salevalue)),
                                incentive = Math.Round(item.Sum(e => e.range.value ? (e.range.incentive * e.salevalue) : (e.range.incentive * e.salevalue / 100)))
                            });
                        }
                    }
                    else if (ValueType == IncentiveSalary.IncentiveData.Item.ValueTypes.Quantity)
                    {
                        var ageincentive = ageresut.Sum(ex =>
                        {
                            return ex.range.incentive * ex.saleqty;
                        });
                        foreach (var item in ageresut.GroupBy(e => string.Format("{0} - {1}", e.range.start, e.range.end)))
                        {
                            Salebyages.Add(new salebyage()
                            {
                                name = item.Key,
                                from = item.First().range.start,
                                to = item.First().range.end,
                                incentivecount = Math.Round(item.Sum(e => e.saleqty)),
                                salevalue = Math.Round(item.Sum(e => e.salevalue)),
                                incentive = Math.Round(item.Sum(e => e.range.incentive * e.saleqty))
                            });
                        }
                        incentive += ageincentive;
                        amount += ageincentive;
                        AddIncentive(incentiveSource, ageincentive);
                    }
                }
            }

            public class salebyage
            {
                public int from { get; set; }
                public int to { get; set; }
                public decimal salevalue { get; set; }
                public decimal incentive { get; set; }
                public decimal incentivecount { get; set; }
                public string name { get; set; }
            }

            public class salevalue
            {
                public long sectionid { get; set; }
                public long companyid { get; set; }
                public decimal amount { get; set; }
                public string age { get; set; }
                public long locationid { get; set; }
                public long floorid { get; set; }
                public decimal schemediscount { get; set; }
                public decimal additionaldiscount { get; set; }
                public long employeeid { get; set; }
            }
        }

        int month;
        int year;
        int days;
        long departmentid;
        long sectionid;
        long companyid;
        long locationid;
        long employeeid;
        bool includesales;
        List<int> searchmodes = new List<int>();
        decimal totalSales = 0;
        DateTime fromdate;
        DateTime todate;
        long organizationid;
        bool bydate;
        public IncentiveSalary(long organizationid, DateTime fromdate, DateTime todate, long departmentid,long sectionid,
            long companyid, long locationid, bool includesales, long employeeid, List<int> searchmodes)
        {
            this.bydate = true;
            this.companyid = companyid;
            this.departmentid = departmentid;
            this.locationid = locationid;
            this.includesales = includesales;
            this.fromdate = fromdate;
            this.todate = todate;
            this.employeeid = employeeid;
            this.sectionid = sectionid;
            this.searchmodes = searchmodes;
            this.organizationid = organizationid;
        }


        public IncentiveSalary(long organizationid, int month, int year, long departmentid,long sectionid,
            long companyid, long locationid, bool includesales, long employeeid, List<int> searchmodes)
        {
            this.organizationid = organizationid;
            this.month = month;
            this.year = year;
            this.employeeid = employeeid;
            this.sectionid = sectionid;
            fromdate = new DateTime(year, month, 1);
            todate = fromdate.AddMonths(1);
            this.companyid = companyid;
            this.departmentid = departmentid;
            this.locationid = locationid;
            this.includesales = includesales;
            this.searchmodes = searchmodes;
            days = WorkingDays();
        }

        private int WorkingDays()
        {
            int sundays = 0;
            DateTime monthstart = new DateTime(year, month, 1);
            DateTime monthend = monthstart.AddMonths(1);
            while (monthstart < monthend)
            {
                monthstart = monthstart.AddDays(1);
                if (monthstart.DayOfWeek == DayOfWeek.Sunday)
                    sundays++;
            }
            return sundays;
        }

        public List<IncentiveData> Compute(List<long> accessiblelocation, long locationid)
        {
            totalSales = 0;
            List<IncentiveData> result = null;
            /*            { 'id': 0, 'name': 'None' },
            { 'id': 4, 'name': 'Fixed %' },
            { 'id': 1, 'name': 'Target Based' },
            { 'id': 2, 'name': 'Section Price Slab Based' },
            { 'id': 8, 'name': 'Section Age Slab Based' },
            { 'id': 3, 'name': 'Product Price Slab Based' },
            { 'id': 7, 'name': 'Product Age Slab Based' },
            { 'id': 5, 'name': 'Employee And Product Age' },
            { 'id': 6, 'name': 'Employee Incentive Config' },*/

            if (Services.ConfigManager.Instance(this.organizationid).Config.incentivemode == 6)
            {
                IncentiveEngine incentiveEngine = new IncentiveEngine(organizationid, fromdate, todate, departmentid,
                    sectionid,
                    companyid, accessiblelocation, locationid, includesales, employeeid, searchmodes);
                result = incentiveEngine.Compute();

                //if(searchmodes.Exists(e => e == 0) || searchmodes.Exists(e => e == 5))
                //{
                //    var ageresult = ComputeEmployeProductAge(accessiblelocation, locationid);
                //    foreach(var dpt in ageresult)
                //    {
                //        if(result.Exists(e=>e.departmentid == dpt.departmentid))
                //        {
                //            var actualemployees = result.Find(e => e.departmentid == dpt.departmentid).Employees;
                //            var tempemployees = result.Find(e => e.departmentid == dpt.departmentid).Employees;
                //            foreach(var emp in tempemployees) 
                //            {
                                
                //            }
                //        }
                //    }
                //}
            }
            //employee product age
            else if (Services.ConfigManager.Instance(this.organizationid).Config.incentivemode == 5)
            {
                result = ComputeEmployeProductAge(accessiblelocation, locationid);
            }
            //Product Age Slab Based
            else if (Services.ConfigManager.Instance(this.organizationid).Config.incentivemode == 7)
            {
                result = BuildSalesByProductAgeMasters(accessiblelocation, locationid);
                var totalsale = result.Sum(e => e.SalesValue.Sum(ex => ex.amount));
                List<IncentiveData.Item> allEmployees = new List<IncentiveData.Item>();
                foreach (var dpt in result)
                {
                    allEmployees.AddRange(dpt.Employees);
                }

                foreach (var item in allEmployees.GroupBy(e => e.inchargeid))
                {
                    if (item.Key > 0 && allEmployees.Exists(e => e.employeeid == item.Key))
                    {
                        allEmployees.Find(e => e.employeeid == item.Key).IsIncharge = true;
                    }

                    item.Where(e => e.Salebyages != null && e.Salebyages.Count > 0).ToList().ForEach(x =>
                    {
                        x.IsSalesmen = true;
                    });
                }


                foreach (var dpt in result)
                {
                    if (dpt.IsSales)
                    {
                        foreach (var employee in dpt.Employees)
                        {
                            if (employee.ratio == 0) employee.ratio = 1;
                            if (employee.incentive == 0) employee.incentive = 1;

                            if (employee.Salebyages != null && employee.Salebyages.Count > 0)
                            {
                                employee.amount = employee.Salebyages.Sum(e => e.incentive) * employee.incentive;
                                employee.IsSalesmen = true;
                            }
                        }
                    }
                    else
                    {
                        var incentive = Math.Round(totalsale * (dpt.fixedpercentage + dpt.dynamicpercentage) / 100, 0);
                        foreach (var employee in dpt.Employees)
                        {
                            if (employee.ratio == 0) employee.ratio = 1;
                            if (employee.incentive == 0) employee.incentive = 1;

                            if (employee.ratio == 0) employee.ratio = 1;
                            if (employee.incentive == 0) employee.incentive = 1;

                            if (employee.Salebyages != null && employee.Salebyages.Count > 0)
                            {
                                employee.amount = employee.Salebyages.Sum(e => e.incentive) * employee.incentive;
                                employee.IsSalesmen = true;
                            }
                        }
                    }
                }
            }
            //fixed
            else if (Services.ConfigManager.Instance(this.organizationid).Config.incentivemode == 4)
            {
                //result.RemoveAll(e => !e.IsSales);
                //if(result.Count == 1)
                //{

                result = BuildSalesMasters(accessiblelocation, locationid);
                result.ForEach(x => x.Employees.RemoveAll(e => e.salesvalue == 0));
                result.RemoveAll(e => e.Employees == null || e.Employees.Count == 0);

                decimal incentiveratio = locationid != 0 && Services.ConfigManager.Instance(this.organizationid).LocationConfig.ContainsKey(locationid) &&
                    Services.ConfigManager.Instance(this.organizationid).LocationConfig[locationid].payroll_fixedincentive != 0 ?
                    Services.ConfigManager.Instance(this.organizationid).LocationConfig[locationid].payroll_fixedincentive :
                    Services.ConfigManager.Instance(this.organizationid).Config.payroll_fixedincentive;

                result.ForEach(x => x.Employees.ForEach(e =>
                {
                    if (e.ratio == 0) e.ratio = 1;
                    e.ratio = incentiveratio * e.ratio;

                    if (InfyPOS.Services.ConfigManager.Instance(this.organizationid).Config.payroll_incentive_removebarcode)
                        e.amount = Math.Round((e.salesvalue - e.itemsalevalue) * e.ratio / 100, 0) + e.itemincentive;
                    else
                        e.amount = Math.Round((e.salesvalue) * e.ratio / 100, 0) + e.itemincentive;
                }));
            }
            else
            {
                result = BuildMasters(accessiblelocation, locationid);
                if (departmentid > 0)
                    result.RemoveAll(e => e.departmentid != departmentid);

                SortedDictionary<decimal, decimal> saleValues = new SortedDictionary<decimal, decimal>();
                if (includesales)
                {
                    if (result.Exists(e => e.IsSales))
                    {
                        var saledept = result.Find(e => e.IsSales);
                        if (saledept.fixedpercentage == 0 &&
                            saledept.salesIncentives != null &&
                            saledept.salesIncentives.Count > 0)
                        {
                            var employees = result.Find(e => e.IsSales).Employees;
                            foreach (var incharge in employees.GroupBy(e => e.inchargeid))
                            {
                                if (incharge.Key > 0 && employees.Exists(e => e.employeeid == incharge.Key))
                                {
                                    employees.Find(e => e.employeeid == incharge.Key).amount = incharge.Sum(e => e.amount);
                                }
                            }
                            foreach (var item in employees)
                            {
                                if (item.salestarget != 0 && item.salesvalue != 0)
                                {
                                    var achived = item.salesvalue / item.salestarget * 100;
                                    var applied = false;
                                    foreach (var salestarget in saledept.salesIncentives)
                                    {
                                        if (salestarget.from <= achived && salestarget.to >= achived)
                                        {
                                            applied = true;
                                            item.amount = Math.Round((item.salesvalue - item.itemsalevalue) * salestarget.incentive / 100, 0) + item.itemincentive;
                                            break;
                                        }
                                    }
                                    if (!applied)
                                    {
                                        item.amount = Math.Round((item.salesvalue - item.itemsalevalue) * saledept.salesIncentives.Last().incentive / 100, 0) + item.itemincentive;
                                    }
                                }
                            }
                        }
                        else if (Services.ConfigManager.Instance(this.organizationid).Config.incentivesalemode == 2)
                        {
                            ComputeBySaleDistribution(result);
                        }
                        else if (Services.ConfigManager.Instance(this.organizationid).Config.incentivesalemode == 3)
                        {
                            ComputeBySectionwiseIncentives(result);
                        }
                    }
                }
                else
                {
                    result.RemoveAll(e => e.IsSales);
                }

                if (result.Exists(e => !e.IsSales))
                {
                    foreach (var item in result.FindAll(e => !e.IsSales))
                    {
                        decimal totalpay = item.Employees.Sum(e => e.earnings);
                        foreach (var emp in item.Employees)
                        {
                            emp.payratio = (emp.earnings / totalpay) * 100 * emp.ratio;
                            emp.totalpay = totalpay;
                        }

                        decimal totalpayratio = item.Employees.Sum(e => e.payratio);
                        var allocation = Math.Round(totalSales * (item.dynamicpercentage + item.fixedpercentage) / 100, 0);
                        foreach (var emp in item.Employees)
                        {
                            emp.allocation = allocation;
                            emp.incentivepercentage = item.dynamicpercentage + item.fixedpercentage;
                            //emp.amount += emp.itemincentive;
                            if (totalpayratio > 0 && emp.payratio > 0 && allocation > 0)
                            {
                                emp.amount = Math.Round(allocation / totalpayratio * emp.payratio, 0);
                                emp.distpercentage = Math.Round(emp.amount / allocation * 100, 2);
                            }
                        }
                    }
                }
            }

            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                locationid = locationid,
                type = "INCENTIVE-TARGET"
            });
            if (config.Count == 0)
            {
                config = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    type = "INCENTIVE-TARGET"
                });
                config.RemoveAll(e => e.locationid != 0);
            }
            if (config.Count > 0 && config[0].additionalinfo != null &&
                config[0].additionalinfo.salesincentive != null)
            {
                if (config[0].additionalinfo.salesincentive.Exists(e => e.TgtSource == Referencelist.SalesIncentive.TargetSource.Prefix))
                {
                    var targetconfig = config[0].additionalinfo.salesincentive.FindAll(e => e.referenceid != 0 && e.TgtSource == Referencelist.SalesIncentive.TargetSource.Prefix);
                    var globalConfig = config[0].additionalinfo.salesincentive.FindAll(e => e.referenceid == 0);
                    var allprefixlist = config[0].additionalinfo.salesincentive.FindAll(e => e.TgtSource == Referencelist.SalesIncentive.TargetSource.Prefix).ConvertAll(e => e.prefix);
                    var newList = allprefixlist.FindAll(e => e.IndexOf(',') > 0);
                    foreach (var prefix in newList)
                    {
                        allprefixlist.Remove(prefix);
                        foreach (var sprefix in prefix.Split(','))
                        {
                            if (!allprefixlist.Contains(sprefix))
                                allprefixlist.Add(sprefix);
                        }
                    }
                    var prefixwiseSales = FindSalesValueByPrefix(allprefixlist);
                    result.ForEach(e => e.Employees.ForEach(x =>
                    {
                        var saleList = prefixwiseSales.FindAll(emp => emp.employeeid == x.employeeid);
                        if (saleList.Count > 0)
                        {
                            if (targetconfig.Exists(r => r.referenceid == x.employeeid))
                            {
                                ApplyPrefixTarget(x, saleList,
                                    targetconfig.FindAll(r => r.referenceid == x.employeeid));
                            }
                            else if (globalConfig != null)
                            {
                                ApplyPrefixTarget(x, saleList,
                                    globalConfig);
                            }
                        }
                    }));
                }
                else
                {
                    var targetconfig = config[0].additionalinfo.salesincentive;
                    var globalConfig = targetconfig.FindAll(e => e.referenceid == 0);
                    //FindSalesValueByAge
                    result.ForEach(e => e.Employees.ForEach(x =>
                      {
                          if (targetconfig.Exists(r => r.referenceid == x.employeeid))
                          {
                              ApplyTarget(x, targetconfig.FindAll(r => r.referenceid == x.employeeid));
                          }
                          else if (globalConfig != null)
                          {
                              ApplyTarget(x, globalConfig);
                          }
                      }));
                }
            }
            return result;
        }

        private List<IncentiveData> ComputeEmployeProductAge(List<long> accessiblelocation, long locationid)
        {
            List<IncentiveData> result = null;
            result = BuildSalesByAgeMasters(accessiblelocation, locationid);
            var totalsale = result.Sum(e => e.SalesValue.Sum(ex => ex.amount));
            List<IncentiveData.Item> allEmployees = new List<IncentiveData.Item>();
            foreach (var dpt in result)
            {
                allEmployees.AddRange(dpt.Employees);
            }

            foreach (var item in allEmployees.GroupBy(e => e.inchargeid))
            {
                if (item.Key > 0 && allEmployees.Exists(e => e.employeeid == item.Key))
                {
                    allEmployees.Find(e => e.employeeid == item.Key).IsIncharge = true;
                }

                item.Where(e => e.Salebyages != null && e.Salebyages.Count > 0).ToList().ForEach(x =>
                {
                    x.IsSalesmen = true;
                });
            }


            //allEmployees.FindAll(e => !e.IsIncharge && !e.IsSalesmen).ForEach(x => x.IsSalesmen = true);
            //allEmployees.FindAll(e => e.IsIncharge && e.IsSalesmen).ForEach(x => x.IsSalesmen = false);

            foreach (var dpt in result)
            {
                if (dpt.IsSales)
                {
                    foreach (var employee in dpt.Employees)
                    {
                        if (employee.ratio == 0) employee.ratio = 1;
                        if (employee.incentive == 0) employee.incentive = 1;

                        if (employee.Salebyages != null && employee.Salebyages.Count > 0)
                        {
                            employee.amount = employee.Salebyages.Sum(e => e.incentive) * employee.incentive;
                            employee.IsSalesmen = true;
                        }
                    }
                }
                else
                {
                    var incentive = Math.Round(totalsale * (dpt.fixedpercentage + dpt.dynamicpercentage) / 100, 0);
                    foreach (var employee in dpt.Employees)
                    {
                        if (employee.ratio == 0) employee.ratio = 1;
                        if (employee.incentive == 0) employee.incentive = 1;
                        if (!employee.IsIncharge && !employee.IsSalesmen)
                        {
                            employee.salesvalue = totalsale;
                            if (IncentiveSlaps.ContainsKey(employee.employeeid))
                            {
                                if (IncentiveSlaps[employee.employeeid].Exists(e => e.start <= totalsale && e.end >= totalsale))
                                {
                                    employee.incentive = IncentiveSlaps[employee.employeeid].Find(e => e.start <= totalsale && e.end >= totalsale).incentive * employee.incentive;
                                    employee.amount = Math.Round(totalsale * employee.incentive / 100, 0);
                                }
                                else
                                {
                                    employee.incentive = IncentiveSlaps[employee.employeeid].OrderByDescending(e => e.end).First().incentive * employee.incentive;
                                    employee.amount = Math.Round(totalsale * employee.incentive / 100, 0);
                                }
                            }
                            else if (IncentiveSlaps.ContainsKey(-2))
                            {
                                if (IncentiveSlaps[-2].Exists(e => e.start <= totalsale && e.end >= totalsale))
                                {
                                    employee.incentive = IncentiveSlaps[-2].Find(e => e.start <= totalsale && e.end >= totalsale).incentive * employee.incentive;
                                    employee.amount = Math.Round(totalsale * employee.incentive / 100, 0);
                                }
                                else
                                {
                                    employee.incentive = IncentiveSlaps[-2].OrderByDescending(e => e.end).First().incentive;
                                    employee.amount = Math.Round(totalsale * employee.incentive, 0);
                                }
                            }
                            else
                            {
                                employee.incentive = (dpt.fixedpercentage + dpt.dynamicpercentage) * employee.incentive;
                                employee.amount = Math.Round(totalsale * employee.incentive / 100, 0); ;
                            }
                        }
                        else if (employee.IsSalesmen)
                        {
                            if (employee.ratio == 0) employee.ratio = 1;
                            if (employee.incentive == 0) employee.incentive = 1;

                            if (employee.Salebyages != null && employee.Salebyages.Count > 0)
                            {
                                employee.amount = employee.Salebyages.Sum(e => e.incentive) * employee.incentive;
                                employee.IsSalesmen = true;
                            }
                        }
                    }
                }
            }

            foreach (var item in allEmployees.GroupBy(e => e.inchargeid))
            {
                if (item.Key > 0 && allEmployees.Exists(e => e.employeeid == item.Key))
                {
                    var employee = allEmployees.Find(e => e.employeeid == item.Key);
                    if (employee.incentive == 0) employee.incentive = 1;
                    employee.salesvalue += item.Sum(e => e.Salebyages == null ? 0 : e.Salebyages.Sum(x => x.salevalue));

                    if (IncentiveSlaps.ContainsKey(employee.employeeid))
                    {
                        if (IncentiveSlaps[employee.employeeid].Exists(e => e.start <= employee.salesvalue && e.end >= employee.salesvalue))
                        {
                            employee.incentive = IncentiveSlaps[employee.employeeid].Find(e => e.start <= employee.salesvalue && e.end >= employee.salesvalue).incentive * employee.incentive;
                            employee.amount = Math.Round(employee.salesvalue * employee.incentive / 100, 0);
                        }
                        else
                        {
                            employee.amount = Math.Round(employee.salesvalue * employee.incentive / 100, 0);
                            employee.incentive = IncentiveSlaps[employee.employeeid].OrderByDescending(e => e.end).First().incentive;
                        }
                    }
                    else if (IncentiveSlaps.ContainsKey(-1))
                    {
                        if (IncentiveSlaps[-1].Exists(e => e.start <= employee.salesvalue && e.end >= employee.salesvalue))
                        {
                            employee.incentive = IncentiveSlaps[-1].Find(e => e.start <= employee.salesvalue && e.end >= employee.salesvalue).incentive * employee.incentive;
                            employee.amount = Math.Round(employee.salesvalue * employee.incentive / 100, 0);
                        }
                        else
                        {
                            employee.incentive = IncentiveSlaps[-1].OrderByDescending(e => e.end).First().incentive * employee.incentive;
                            employee.amount = Math.Round(employee.salesvalue * employee.incentive / 100, 0);
                        }
                    }
                }
            }
            return result;
        }

        private void ApplyPrefixTarget(IncentiveData.Item employee,
            List<EmployeeByProductAge> salebyprefix, List<Referencelist.SalesIncentive> config)
        {
            decimal targetincentive = 0;
            decimal totalsalevalue = 0;
            decimal totalsaleqty = 0;
            foreach (var item in config)
            {
                var prefixlist = new List<string>(item.prefix.Split(','));
                var salesvalue = salebyprefix.Sum(e => prefixlist.Exists(x => x == e.name) ? e.salevalue : 0);
                var salesqty = salebyprefix.Sum(e => prefixlist.Exists(x => x == e.name) ? e.incentivecount : 0);
                totalsalevalue += salesvalue;
                totalsaleqty += salesqty;

                if (item.TgtComputation == Referencelist.SalesIncentive.TargetComputation.AdditionalSaleValueSingleSlab &&
                    item.from <= salesvalue && item.to >= salesvalue)
                {
                    if (item.value)
                        targetincentive = targetincentive <= item.incentive? item.incentive: targetincentive;
                    else if(Math.Round((salesvalue * item.incentive / 100), 0)>targetincentive)
                        targetincentive = Math.Round((salesvalue * item.incentive / 100), 0);
                }
                else if (item.TgtComputation == Referencelist.SalesIncentive.TargetComputation.AdditionalSaleValueMultipleSlab &&
                    item.from <= salesvalue && item.to >= salesvalue)
                {
                    if (item.value)
                        targetincentive += item.incentive;
                    else
                    {
                        if (salesvalue >= item.to)
                        {
                            targetincentive += Math.Round(((item.to - item.from) * item.incentive / 100), 0);
                        }
                        else
                        {
                            targetincentive += Math.Round(((salesvalue - item.from) * item.incentive / 100), 0);
                        }
                    }
                }
                if (item.TgtComputation == Referencelist.SalesIncentive.TargetComputation.AdditionalPerPieceValueSingleSlab &&
                    item.from <= salesqty && item.to >= salesqty)
                {
                    if(targetincentive< salesqty * item.incentive)
                       targetincentive = salesqty * item.incentive;
                }
                else if (item.TgtComputation == Referencelist.SalesIncentive.TargetComputation.AdditionalSaleValueMultipleSlab &&
                    item.from <= salesqty && item.to >= salesqty)
                {
                    if (item.value)
                        targetincentive += item.incentive;
                    else
                    {
                        if (salesqty >= item.to)
                        {
                            targetincentive += Math.Round(((item.to - item.from) * item.incentive / 100), 0);
                        }
                        else
                        {
                            targetincentive += Math.Round(((salesqty - item.from) * item.incentive / 100), 0);
                        }
                    }
                }

            }


            if (config.Exists(e => e.TgtComputation == Referencelist.SalesIncentive.TargetComputation.Minimum))
            {
                var minimum = config.Find(e => e.TgtComputation == Referencelist.SalesIncentive.TargetComputation.Minimum);

                if ((minimum.value && minimum.from < totalsaleqty) || (!minimum.value && minimum.from < totalsalevalue))
                {
                    employee.saleremoved = true;
                    employee.amount = 0;
                    employee.incentive = 0;
                    if (employee.Salebyages != null)
                        employee.Salebyages.ForEach(e => e.incentive = 0);
                    return;
                }
            }
            employee.targetincentive = Math.Round(targetincentive, 0);
            //employee.incentive += Math.Round(targetincentive, 0);
        }

        private List<EmployeeByProductAge> FindSalesValueByPrefix(List<string> allprefixlist)
        {
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var prefix in allprefixlist.Distinct().OrderByDescending(e=>e.Length))
            {
                conditionlist.AppendLine("when substring(billitems.barcode,0," + (prefix.Length + 1) + ") ilike '" + WebAPI.Data.Utility.SafeSQL(prefix) + "' then '" + WebAPI.Data.Utility.SafeSQL(prefix) + "'");
            }

            var prefixfield = " case " + conditionlist.ToString() + " else 'UNKNOWN' end ";

            string query = string.Format(@"select {0} prefix,sum(billitems.qty) saleqty,
                            sum(billitems.receivable) as salevalue,salesmanid
                            from billitems
                            inner join bill on bill.id = billitems.billid
                            left outer join stock on stock.id = billitems.stockid
                            left outer join products on products.id = stock.productid
                            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate
                            and bill.isb2b=false and bill.billtype != 9 and billdate<@todate and salescounterid=@sectionid
							group by {0},salesmanid", prefixfield);

            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var param = new SortedList<string, object>();
            param.Add("fromdate", fromdate);
            param.Add("todate", todate.AddDays(1));
            param.Add("sectionid", sectionid);
            List<EmployeeByProductAge> result = new List<EmployeeByProductAge>();
            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication)
                query = query.Replace("and bill.isb2b=false", "");
            if (sectionid <= 0)
                query = query.Replace("and salescounterid=@sectionid", "");

            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    if ("UNKNOWN" == (reader["prefix"] != DBNull.Value ? Convert.ToString(reader["prefix"]) : ""))
                        continue;

                    result.Add(new EmployeeByProductAge()
                    {
                        employeeid = reader["salesmanid"] != DBNull.Value ? Convert.ToInt64(reader["salesmanid"]) : 0,
                        name = reader["prefix"] != DBNull.Value ? Convert.ToString(reader["prefix"]) : "",
                        incentivecount = reader["saleqty"] != DBNull.Value ? Convert.ToDecimal(reader["saleqty"]) : 0,
                        salevalue = reader["salevalue"] != DBNull.Value ? Convert.ToDecimal(reader["salevalue"]) : 0
                    });
                }
            }
            return result;
        }

        public class EmployeeByProductAge : IncentiveData.salebyage
        {
            public long employeeid { get; set; }
        }
        private EmployeeByProductAge FindSalesValueByAge(long employeeid, decimal fromage, decimal toage)
        {
            string query = @"select sum(billitems.qty) saleqty,
                            sum(billitems.receivable) as salevalue
                            from billitems
                            inner join bill on bill.id = billitems.billid
                            inner join stock on stock.id = billitems.stockid
                            inner join products on products.id = stock.productid
                            left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
                            left outer join lrinvoice on lrinvoice.id = lrinvoiceid
                            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate
                            and salesmanid = @salesmanid and bill.isb2b=false and bill.billtype != 9 and salescounterid=@sectionid
							and ((EXTRACT(epoch FROM(SELECT(bill.billdate - lrinvoice.entrydate))) / 86400))>@from and 
							((EXTRACT(epoch FROM(SELECT(bill.billdate - lrinvoice.entrydate))) / 86400))<@to";
            List<IncentiveData> retValue = new List<IncentiveData>();
            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var param = new SortedList<string, object>();
            param.Add("fromdate", fromdate);
            param.Add("todate", todate);
            param.Add("from", fromage);
            param.Add("to", toage);
            param.Add("salesmanid", employeeid);
            param.Add("sectionid", sectionid);
            EmployeeByProductAge result = null;
            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication)
                query = query.Replace("and bill.isb2b=false", "");
            if (sectionid <= 0)
                query = query.Replace("and salescounterid=@sectionid", "");

            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param, 10000))
            {
                if (reader.Read())
                {
                    result = new EmployeeByProductAge()
                    {
                        incentivecount = reader["saleqty"] != DBNull.Value ? Convert.ToDecimal(reader["saleqty"]) : 0,
                        salevalue = reader["salevalue"] != DBNull.Value ? Convert.ToDecimal(reader["salevalue"]) : 0
                    };
                }
            }
            return result;
        }
        private void ApplyTarget(IncentiveData.Item employee, List<Referencelist.SalesIncentive> list)
        {
            if (list.Exists(e => e.TgtComputation == Referencelist.SalesIncentive.TargetComputation.Minimum))
            {
                var minimum = list.Find(e => e.TgtComputation == Referencelist.SalesIncentive.TargetComputation.Minimum);
                if (minimum.from < employee.salesvalue)
                {
                    employee.saleremoved = true;
                    employee.incentive = 0;
                    employee.amount = 0;
                    if (employee.Salebyages != null)
                        employee.Salebyages.ForEach(x => x.incentive = 0);
                    return;
                }
            }

            var salesvalue = employee.salesvalue;
            var salesqty = employee.salesqty;
            decimal targetincentive = 0;

            if (list.Exists(e => e.TgtSource == Referencelist.SalesIncentive.TargetSource.ProductAge))
            {

                if (employee.Salebyages == null || employee.Salebyages.Count == 0) return;
                var ageCondition = list.Find(e => e.TgtSource == Referencelist.SalesIncentive.TargetSource.ProductAge);
                if (!employee.Salebyages.Exists(e => ageCondition.fromprice == e.from &&
                     ageCondition.toprice == e.to))
                {
                    var agesales = FindSalesValueByAge(employee.employeeid, ageCondition.fromprice, ageCondition.toprice);
                    if (agesales == null) return;
                    salesvalue = agesales.salevalue;
                    salesqty = agesales.incentivecount;
                }
                salesvalue = employee.Salebyages.FindAll(e => ageCondition.fromprice == e.from &&
                     ageCondition.toprice == e.to).Sum(e => e.salevalue);
                salesqty = employee.Salebyages.FindAll(e => ageCondition.fromprice == e.from &&
                     ageCondition.toprice == e.to).Sum(e => e.incentivecount);
            }

            if (list.Exists(e => e.TgtComputation == Referencelist.SalesIncentive.TargetComputation.AdditionalSaleValueSingleSlab && e.from <= salesvalue))
            {
                var incentivepercentage = list.FindAll(e => e.TgtComputation == Referencelist.SalesIncentive.TargetComputation.AdditionalSaleValueSingleSlab && e.from <= salesvalue).OrderBy(e => e.from).Last().incentive;
                targetincentive = Math.Round(incentivepercentage * salesvalue / 100, 0);

            }
            else if (list.Exists(e => e.TgtComputation == Referencelist.SalesIncentive.TargetComputation.AdditionalPerPieceValueSingleSlab && e.from <= salesqty))
            {
                var incentivepercentage = list.FindAll(e => e.TgtComputation == Referencelist.SalesIncentive.TargetComputation.AdditionalPerPieceValueSingleSlab && e.from <= salesqty).OrderBy(e => e.from).Last().incentive;
                targetincentive = Math.Round(incentivepercentage * salesqty, 0);

            }
            else if (list.Exists(e => e.TgtComputation == Referencelist.SalesIncentive.TargetComputation.AdditionalSaleValueMultipleSlab && e.from <= salesvalue))
            {
                var incentivepercentagelist = list.FindAll(e => e.TgtComputation == Referencelist.SalesIncentive.TargetComputation.AdditionalSaleValueMultipleSlab && e.from <= salesvalue).OrderBy(e => e.from);
                foreach (var iconfig in incentivepercentagelist)
                {
                    if (iconfig.to >= salesvalue)
                    {
                        targetincentive += Math.Round(iconfig.incentive * (iconfig.to - iconfig.from) / 100, 0);
                    }
                    else
                    {
                        targetincentive += Math.Round(iconfig.incentive * (salesvalue - iconfig.from) / 100, 0);
                    }
                }
            }
            else if (list.Exists(e => e.TgtComputation == Referencelist.SalesIncentive.TargetComputation.AdditionalPerPieceValueMultipleSlab && e.from <= salesqty))
            {
                var incentivepercentagelist = list.FindAll(e => e.TgtComputation == Referencelist.SalesIncentive.TargetComputation.AdditionalPerPieceValueMultipleSlab && e.from <= salesqty).OrderBy(e => e.from);
                foreach (var iconfig in incentivepercentagelist)
                {
                    if (iconfig.to >= salesqty)
                    {
                        targetincentive += (iconfig.incentive * (iconfig.to - iconfig.from));
                    }
                    else
                    {
                        targetincentive += (iconfig.incentive * (salesqty - iconfig.from));
                    }
                }
            }
            employee.targetincentive = Math.Round(targetincentive, 0);
        }

        public class Range
        {
            public int start { get; set; }
            public int end { get; set; }
            public string id { get; set; }
            public decimal incentive { get; set; }
        }
        private string getRangeCase(List<Range> rangelist, string field, string typeCast)
        {
            System.Text.StringBuilder conditionlist = new System.Text.StringBuilder();
            foreach (var rng in rangelist)
            {
                conditionlist.AppendLine("when " + field + " >= " + rng.start + typeCast + " AND " + field + "<" + rng.end + typeCast + " then '" + rng.id + "'");
            }
            return " case " + conditionlist.ToString() + " else 'unknown' end ";
        }

        public SortedDictionary<long, List<Range>> IncentiveSlaps { get; set; }

        private List<IncentiveData> BuildSalesByProductAgeMasters(List<long> accessiblelocation, long locationid)
        {
            IncentiveSlaps = new SortedDictionary<long, List<Range>>();
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                locationid = locationid,
                type = "INCENTIVE-PRODUCT-AGE"
            });

            if (config.First().additionalinfo != null && config.First().additionalinfo.salesincentive != null && config.First().additionalinfo.salesincentive.Count > 0)
            {
                List<Range> rangelist = new List<Range>();
                foreach (var item in config.First().additionalinfo.salesincentive.OrderBy(e => e.from))
                {
                    var rng = new Range()
                    {
                        start = (int)item.from,
                        end = (int)item.to,
                        incentive = item.incentive,
                        id = rangelist.Count.ToString()
                    };
                    rangelist.Add(rng);
                }
                string field = getRangeCase(rangelist, "((EXTRACT(epoch FROM(SELECT(bill.billdate - lrinvoice.entrydate))) / 86400))", "::integer");

                string query = @"
            select id,additionalinfo,name from referencelist where type='DEPARTMENT';

            with salaryheader as (
			SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@todate)

            select name as employeename,employeecode,0 as workingdays,0 as leavedays,
            1 as incentiveratio,0 as salestarget,incentive,managerid,
            employee.departmentid,employee.companyid,employee.sectionid,employee.id as employeeid,
            0 as grosspay,employee.locationid,0::bigint as inchargeid,0 as incentivesource,
            0 as incentivepercentage,employee.floorid
            from employee
            where hasleft=false and isactive=true and 
            (employee.id=@employeeid or employee.id in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) 
            order by case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end;

            with salaryheader as (
			SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@todate)

            select sum(billitems.receivable) as amount,salescounterid as sectionid,bill.companyid,bill.locationid,bill.floorid from billitems
            inner join bill on bill.id = billitems.billid
            inner join stock on stock.id = billitems.stockid
            inner join products on products.id = stock.productid
            where bill.isactive=true and billitems.isactive=true and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and bill.locationid=@locationid and bill.billtype != 9 and bill.isb2b=false and salescounterid=@sectionid
            and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) 
            group by salescounterid,bill.companyid,bill.locationid,bill.floorid;

            with salaryheader as (
			SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@todate)

            select {0} as Aging,sum(billitems.receivable) as amount,salesmanid from billitems
            inner join bill on bill.id = billitems.billid
            left outer join stock on stock.id = billitems.stockid
            left outer join products on products.id = stock.productid
            left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
            left outer join lrinvoice on lrinvoice.id = lrinvoiceid
            left outer join incentiveconfig on incentiveconfig.salablegoodsid = billitems.salablegoodsid
            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and bill.locationid=@locationid and incentiveconfig.salablegoodsid is null and bill.billtype != 9 and bill.isb2b=false and salescounterid=@sectionid
            and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) 
            group by salesmanid,{0};

            with salaryheader as (
			SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@todate)

            select sum(billitems.receivable) as amount,
            count(*) as incentivecount,
            sum(billitems.qty*incentiveconfig.incentive::numeric) as incentive,
            sum(case when coalesce(incentiveconfig.percentage::numeric,0::numeric)>0::numeric then billitems.receivable::numeric*coalesce(incentiveconfig.percentage::numeric,0::numeric)/100::numeric else 0::numeric end) as incentivebypercentage,
            salesmanid from billitems
            inner join bill on bill.id = billitems.billid
            left outer join stock on stock.id = billitems.stockid
            left outer join products on products.id = stock.productid
            inner join incentiveconfig on incentiveconfig.salablegoodsid = billitems.salablegoodsid and incentiveconfig.isactive = true
            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and bill.locationid=@locationid  and bill.billtype != 9 and bill.isb2b=false and salescounterid=@sectionid
            and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) 
            group by salesmanid";

                SortedList<string, object> param = new SortedList<string, object>();

                if (fromdate == DateTime.MinValue || todate == DateTime.MinValue)
                {
                    DateTime fromDate = new DateTime(year, month, 1);
                    param.Add("salarymonth", fromDate.Date);
                    param.Add("fromdate", fromDate.Date);
                    param.Add("locationid", locationid);
                    param.Add("sectionid", sectionid);
                    param.Add("todate", fromDate.Date.AddMonths(1).AddDays(1));
                }
                else
                {
                    param.Add("salarymonth", fromdate);
                    param.Add("fromdate", fromdate);
                    param.Add("locationid", locationid);
                    param.Add("sectionid", sectionid);
                    param.Add("todate", todate.Date.AddDays(1));
                }
                param.Add("employeeid", employeeid);
                if (employeeid > 0)
                {
                    query = query.Replace("and bill.locationid=@locationid", "");
                    query = query.Replace("hasleft=false and isactive=true and ", "");
                }

                if (locationid == 0)
                {
                    if (accessiblelocation == null || accessiblelocation.Contains(-1))
                    {
                        query = query.Replace("and bill.locationid=@locationid", "");
                    }
                    else
                    {
                        query = query.Replace("and bill.locationid=@locationid", "and bill.locationid in (" + string.Join(",", accessiblelocation) + ")");
                    }
                }
                return BuildMastersByAge(string.Format(query, field), param, rangelist);
            }
            return null;
        }
        private List<IncentiveData> BuildSalesByAgeMasters(List<long> accessiblelocation, long locationid)
        {
            IncentiveSlaps = new SortedDictionary<long, List<Range>>();
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var config = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                locationid = locationid,
                type = "INCENTIVE-EMPLOYEE-AGE"
            });

            if(config.Count == 0)
            {
                config = referencelistDataService.Search(new Models.ReferencelistCreteria()
                {
                    locationid = 0,
                    type = "INCENTIVE-EMPLOYEE-AGE"
                });
                if (config.Exists(e => e.locationid == 0))
                {
                    config.RemoveAll(e => e.locationid != 0);
                }
            }

            if (config.Count == 0)
            {
                return new List<IncentiveData>();
            }
            if (config.First().additionalinfo != null && config.First().additionalinfo.salesincentive != null && config.First().additionalinfo.salesincentive.Count > 0)
            {
                List<Range> rangelist = new List<Range>();
                foreach (var item in config.First().additionalinfo.salesincentive.OrderBy(e => e.from))
                {
                    var rng = new Range()
                    {
                        start = (int)item.from,
                        end = (int)item.to,
                        incentive = item.incentive,
                        id = rangelist.Count.ToString()
                    };
                    if (item.referenceid == 0)
                    {
                        rangelist.Add(rng);
                    }
                    else
                    {
                        if (!IncentiveSlaps.ContainsKey(item.referenceid))
                            IncentiveSlaps.Add(item.referenceid, new List<Range>());
                        IncentiveSlaps[item.referenceid].Add(rng);
                    }
                    //else if (item.referenceid == -1)
                    //{
                    //    SupervisorSlaps.Add(rng);
                    //}
                    //else if (item.referenceid == -2)
                    //{
                    //    ManagementSlaps.Add(rng);
                    //}
                }
                string field = getRangeCase(rangelist, "((EXTRACT(epoch FROM(SELECT(bill.billdate - lrinvoice.entrydate))) / 86400))", "::integer");



                string query = @"
            select id,additionalinfo,name from referencelist where type='DEPARTMENT';

            with salaryheader as (
			SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@todate)

            select name as employeename,employeecode,0 as workingdays,0 as leavedays,
            1 as incentiveratio,0 as salestarget,incentive,managerid,
            employee.departmentid,employee.companyid,employee.sectionid,employee.id as employeeid,
            0 as grosspay,employee.locationid,0::bigint as inchargeid,0 as incentivesource,
            0 as incentivepercentage,employee.floorid
            from employee
            where hasleft=false and isactive=true 
            and (employee.id=@employeeid or employee.id in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) 
            order by case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end;

            select sum(billitems.receivable) as amount,salescounterid as sectionid,bill.companyid,bill.locationid,bill.floorid from billitems
            inner join bill on bill.id = billitems.billid
            left outer join stock on stock.id = billitems.stockid
            left outer join products on products.id = stock.productid
            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and bill.locationid=@locationid and bill.billtype != 9 and bill.isb2b=false and salescounterid=@sectionid
            group by salescounterid,bill.companyid,bill.locationid,bill.floorid;

            select {0} as Aging,sum(billitems.receivable) as amount,salesmanid from billitems
            inner join bill on bill.id = billitems.billid
            left outer join stock on stock.id = billitems.stockid
            left outer join products on products.id = stock.productid
            left outer join lrinvoiceitems on lrinvoiceitems.id = stock.invoiceitemid
            left outer join lrinvoice on lrinvoice.id = lrinvoiceid
            left outer join incentiveconfig on incentiveconfig.salablegoodsid = billitems.salablegoodsid
            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and bill.locationid=@locationid and incentiveconfig.salablegoodsid is null and bill.billtype != 9 and bill.isb2b=false and salescounterid=@sectionid
            group by salesmanid,{0};

            select sum(billitems.receivable) as amount,
            count(*) as incentivecount,
            sum(billitems.qty*incentiveconfig.incentive::numeric) as incentive,
            sum(case when coalesce(incentiveconfig.percentage::numeric,0::numeric)>0::numeric then billitems.receivable::numeric*coalesce(incentiveconfig.percentage::numeric,0::numeric)/100::numeric else 0::numeric end) as incentivebypercentage,
            salesmanid from billitems
            inner join bill on bill.id = billitems.billid
            left outer join stock on stock.id = billitems.stockid
            left outer join products on products.id = stock.productid
            inner join incentiveconfig on incentiveconfig.salablegoodsid = billitems.salablegoodsid and incentiveconfig.isactive = true
            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and bill.locationid=@locationid  and bill.billtype != 9  and bill.isb2b=false and salescounterid=@sectionid
            group by salesmanid";

                SortedList<string, object> param = new SortedList<string, object>();

                if (fromdate == DateTime.MinValue || todate == DateTime.MinValue)
                {
                    DateTime fromDate = new DateTime(year, month, 1);
                    param.Add("salarymonth", fromDate.Date);
                    param.Add("fromdate", fromDate.Date);
                    param.Add("locationid", locationid);
                    param.Add("sectionid", sectionid);
                    param.Add("todate", fromDate.Date.AddMonths(1).AddDays(1));
                }
                else
                {
                    param.Add("salarymonth", fromdate);
                    param.Add("fromdate", fromdate);
                    param.Add("locationid", locationid);
                    param.Add("sectionid", sectionid);
                    param.Add("todate", todate.Date.AddDays(1));
                }

                if (employeeid > 0)
                {
                    param.Add("employeeid", employeeid);
                    query = query.Replace("and bill.locationid=@locationid", "");
                    query = query.Replace("hasleft=false and isactive=true and", "");
                }

                if (employeeid == 0)
                {
                    query = query.Replace("and (employee.id=@employeeid or employee.id in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "");
                }
                if (locationid == 0)
                {
                    if (accessiblelocation == null || accessiblelocation.Contains(-1))
                    {
                        query = query.Replace("and bill.locationid=@locationid", "");
                    }
                    else
                    {
                        query = query.Replace("and bill.locationid=@locationid", "and bill.locationid in (" + string.Join(",", accessiblelocation) + ")");
                    }
                }
                return BuildMastersByAge(string.Format(query, field), param, rangelist);
            }
            return null;
        }

        private List<IncentiveData> BuildSalesMasters(List<long> accessiblelocation, long locationid)
        {
            string query = @"
            select id,additionalinfo,name from referencelist where type='DEPARTMENT';

            with salaryheader as (
			SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@todate)

            select name as employeename,employeecode,0 as workingdays,0 as leavedays,
            1 as incentiveratio,0 as salestarget,
            employee.departmentid,employee.companyid,employee.sectionid,employee.id as employeeid,
            0 as grosspay,0 as earnings,employee.locationid,0::bigint as inchargeid,
            0 as incentivesource,0 as incentivepercentage,employee.floorid
            from employee
            where hasleft=false and isactive=true and 
            (employee.id=@employeeid or employee.id in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) 
            order by case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end;

            with salaryheader as (
			SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@todate)

            select sum(billitems.receivable) as amount,salescounterid as sectionid,bill.companyid,bill.locationid,bill.floorid from billitems
            inner join bill on bill.id = billitems.billid
            left outer join stock on stock.id = billitems.stockid
            left outer join products on products.id = stock.productid
            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and bill.locationid=@locationid  and bill.billtype != 9  and bill.isb2b=false and salescounterid=@sectionid
            and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) 
            group by salescounterid,bill.companyid,bill.locationid,bill.floorid;

            with salaryheader as (
			SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@todate)

            select sum(billitems.receivable) as amount,salesmanid from billitems
            inner join bill on bill.id = billitems.billid
            left outer join stock on stock.id = billitems.stockid
            left outer join products on products.id = stock.productid
            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) 
            and bill.locationid=@locationid  and bill.billtype != 9 and salescounterid=@sectionid and bill.isb2b=false group by salesmanid;

            with salaryheader as (
			SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@todate)

            select sum(billitems.receivable) as amount,
            count(*) as incentivecount,
            sum(billitems.qty*incentiveconfig.incentive::numeric) as incentive,
            sum(case when coalesce(incentiveconfig.percentage::numeric,0::numeric)>0::numeric then billitems.receivable::numeric*coalesce(incentiveconfig.percentage::numeric,0::numeric)/100::numeric else 0::numeric end) as incentivebypercentage,
            salesmanid from billitems
            inner join bill on bill.id = billitems.billid
            left outer join stock on stock.id = billitems.stockid
            left outer join products on products.id = stock.productid
            inner join incentiveconfig on incentiveconfig.salablegoodsid = billitems.salablegoodsid and incentiveconfig.isactive = true
            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid)) 
            and bill.locationid=@locationid and bill.billtype != 9 and salescounterid=@sectionid and bill.isb2b=false
            group by salesmanid
            ";

            SortedList<string, object> param = new SortedList<string, object>();

            param.Add("salarymonth", new DateTime(fromdate.Year, fromdate.Month, 1, 0, 0, 0));
            param.Add("fromdate", fromdate);
            param.Add("locationid", locationid);
            param.Add("sectionid", sectionid);
            param.Add("todate", todate.Date.AddDays(1));
            param.Add("employeeid", employeeid);
            if (employeeid == 0)
            {
                query = query.Replace("and (salesmanid=@employeeid or salesmanid in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "");
                query = query.Replace("and (employee.id = @employeeid or employee.id in (select employeeid from salaryheader where ranknumber = 1 and inchargeid=@employeeid))", "");
            }

            if (companyid == 0)
            {
                query = query.Replace("and bill.companyid=@companyid", "");
            }

            if (locationid == 0)
            {
                if (accessiblelocation == null || accessiblelocation.Contains(-1))
                {
                    query = query.Replace("and bill.locationid=@locationid", "");
                }
                else
                {
                    query = query.Replace("and bill.locationid=@locationid", "and bill.locationid in (" + string.Join(",", accessiblelocation) + ")");
                }
            }

            return BuildMasters(query, param);
        }

        private void ComputeBySectionwiseIncentives(List<IncentiveData> result)
        {
            Data.ReferencelistDataService referencelistDataService = new Data.ReferencelistDataService() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            var sections = referencelistDataService.Search(new Models.ReferencelistCreteria()
            {
                type = "DIVISION"
            });

            var saledpt = result.Find(e => e.IsSales);


            foreach (var item in saledpt.Employees.GroupBy(e => e.sectionid))
            {
                if (!sections.Exists(e => e.id == item.Key)) continue;

                var sectionsalevalue = saledpt.SalesValue.Sum(e => e.sectionid == item.Key ? e.amount : 0);
                var section = sections.Find(e => e.id == item.Key);
                var sectionincentive = section.additionalinfo.dynamicincentive + section.additionalinfo.fixedincentive;
                if (sectionincentive == 0)
                    sectionincentive = saledpt.dynamicpercentage + saledpt.fixedpercentage;
                var totalpay = item.Sum(e => e.earnings);
                var allocation = Math.Round(sectionsalevalue * (sectionincentive) / 100, 0);
                var totaldays = item.Sum(e => e.days);
                foreach (var emp in item)
                {
                    emp.allocation = allocation;
                    emp.incentivepercentage = sectionincentive;
                    emp.totalpay = totalpay;
                    if (emp.days > 0 && totaldays > 0)
                    {
                        emp.payratio = emp.days / totaldays * 100 * emp.ratio;
                    }
                }

                var totalpayratio = item.Sum(e => e.payratio);
                foreach (var emp in item)
                {
                    emp.distpercentage = (emp.payratio / totalpayratio * 100);
                    emp.amount = (emp.distpercentage * allocation / 100) + emp.itemincentive;
                }
            }
        }

        private void ComputeBySaleDistribution(List<IncentiveData> result)
        {
            var salevaluelist = result.Find(e => e.IsSales).SalesValue;
            foreach (var item in result)
            {
                var emplist = item.Employees;
                var allocation = Math.Round(totalSales * (item.dynamicpercentage + item.fixedpercentage) / 100, 0);
                item.Amount += allocation;

                SortedDictionary<long, decimal> sectionvisesale = new SortedDictionary<long, decimal>();
                foreach (var section in salevaluelist.GroupBy(e => e.sectionid))
                {
                    var sectionemp = emplist.Where(e => e.sectionid == section.Key);
                    var sectionsalvalue = section.Sum(e => e.amount);
                    decimal totalpayratio = sectionemp.Sum(e => e.payratio);
                    decimal saleratio = sectionsalvalue / totalSales * 100;
                    foreach (var emp in sectionemp)
                    {
                        emp.amount = Math.Round((allocation * saleratio / 100) / totalpayratio * emp.payratio, 0);
                    }
                }
            }
        }

        private List<IncentiveData> BuildMasters(List<long> accessiblelocation, long locationid)
        {
            string query = @"
            select id,additionalinfo,name from referencelist where type='DEPARTMENT';

            with salaryheader as (
			SELECT id, employeeid, fromdate, inchargeid, row_number() OVER (PARTITION BY employeeid ORDER BY fromdate DESC,id DESC) as ranknumber  
			FROM employeesalaryheader where fromdate<=@todate)

            select name as employeename,employeecode,workingdays,leavedays,employeesalaryheader.incentiveratio,employeesalaryheader.salestarget,employeesalary.earnings,
            employee.departmentid,employee.companyid,employee.sectionid,employee.id as employeeid,grosspay,employee.locationid,salaryheader.inchargeid,
            employeesalaryheader.incentivesource,employeesalaryheader.incentivepercentage,employee.floorid
            from employee
            inner join salaryheader on salaryheader.ranknumber = 1 and salaryheader.employeeid = employee.id    
            left outer join employeesalary on employeesalary.employeeid = employee.id and salarymonth=@salarymonth
            left outer join employeesalaryheader on employeesalaryheader.id = salaryheader.id
            where (employee.hasleft = false or relievingdate>=@fromdate) and employee.isactive = true
            order by case when employeecode ~ '^[0-9]+$' then cast(employeecode as numeric) else cast(0 as numeric) end;

            select sum(billitems.receivable) as amount,salescounterid as sectionid,bill.companyid,bill.locationid,bill.floorid from billitems
            inner join bill on bill.id = billitems.billid
            left outer join stock on stock.id = billitems.stockid
            left outer join products on products.id = stock.productid
            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and bill.locationid=@locationid  and bill.billtype != 9  and bill.isb2b=false and salescounterid=@sectionid
            group by salescounterid,bill.companyid,bill.locationid,bill.floorid;

            select sum(billitems.receivable) as amount,salesmanid from billitems
            inner join bill on bill.id = billitems.billid
            left outer join stock on stock.id = billitems.stockid
            left outer join products on products.id = stock.productid
            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and bill.locationid=@locationid and bill.billtype != 9 and salescounterid=@sectionid and bill.isb2b=false group by salesmanid;

            select sum(billitems.receivable) as amount,
            count(*) as incentivecount,
            sum(billitems.qty*incentiveconfig.incentive::numeric) as incentive,
            sum(case when coalesce(incentiveconfig.percentage::numeric,0::numeric)>0::numeric then billitems.receivable::numeric*coalesce(incentiveconfig.percentage::numeric,0::numeric)/100::numeric else 0::numeric end) as incentivebypercentage,
            salesmanid from billitems
            inner join bill on bill.id = billitems.billid
            left outer join stock on stock.id = billitems.stockid
            left outer join products on products.id = stock.productid
            inner join incentiveconfig on incentiveconfig.salablegoodsid = billitems.salablegoodsid and incentiveconfig.isactive = true
            where bill.isactive=true and billitems.isactive=true  and billitems.qty!=0 and billdate>=@fromdate and billdate<@todate 
            and bill.locationid=@locationid and bill.billtype != 9 and salescounterid=@sectionid and bill.isb2b=false
            group by salesmanid";

            SortedList<string, object> param = new SortedList<string, object>();


            param.Add("salarymonth", fromdate);
            param.Add("fromdate", fromdate.Date);
            param.Add("locationid", locationid);
            param.Add("sectionid", sectionid);
            param.Add("todate", todate.Date.AddDays(1));
            if (locationid == 0)
            {
                if (accessiblelocation == null || accessiblelocation.Contains(-1))
                {
                    query = query.Replace("and bill.locationid=@locationid", "");
                }
                else
                {
                    query = query.Replace("and bill.locationid=@locationid", "and bill.locationid in (" + string.Join(",", accessiblelocation) + ")");
                }
            }

            return BuildMasters(query, param);
        }



        private List<IncentiveData> BuildMasters(string query, SortedList<string, object> param)
        {
            List<IncentiveData> retValue = new List<IncentiveData>();
            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };

            var fromdate = (DateTime)param["fromdate"];
            var todate = (DateTime)param["todate"];
            var days = (decimal)todate.Subtract(fromdate).TotalDays;
            if(InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication)
                query = query.Replace("and bill.isb2b=false", "");
            if (sectionid <= 0)
                query = query.Replace("and salescounterid=@sectionid", "");

            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param, 10000))
            {
                while (reader.Read())
                {
                    //name as employeename,referencelist.name departmentname,additionalinfo,workingdays,leavedays,employeesalaryheader.incentiveratio
                    Models.Referencelist reflist = new Models.Referencelist();
                    reflist.additionalinfo_json = reader["additionalinfo"] == DBNull.Value ? "null" : reader["additionalinfo"].ToString();
                    reflist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    reflist.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();

                    if (this.departmentid != 0 && reflist.id != this.departmentid) continue;

                    IncentiveData config = new IncentiveData();
                    config.department = reflist.name;
                    config.departmentid = reflist.id;
                    if (reflist.additionalinfo != null)
                    {
                        decimal percentage = reflist.additionalinfo.fixedincentive + reflist.additionalinfo.dynamicincentive;
                        //decimal.TryParse(reflist.additionalinfo.fixedincentive, out percentage);
                        config.fixedpercentage = percentage;
                        config.dynamicpercentage = reflist.additionalinfo.dynamicincentive;
                        config.IsSales = reflist.additionalinfo.issales;
                        config.salesIncentives = reflist.additionalinfo.salesincentive;
                    }
                    retValue.Add(config);
                }

                if (Services.ConfigManager.Instance(this.organizationid).Config.incentivemode == 4 &&
                    retValue.Count == 0)
                {

                    IncentiveData config = new IncentiveData();
                    config.department = "Sales";
                    config.departmentid = 0;
                    var incentiveratio = locationid != 0 &&
                        Services.ConfigManager.Instance(this.organizationid).LocationConfig.ContainsKey(locationid) &&
                       Services.ConfigManager.Instance(this.organizationid).LocationConfig[locationid].incentiveskipmonths != 0 ?
                       Services.ConfigManager.Instance(this.organizationid).LocationConfig[locationid].incentiveskipmonths :
                       Services.ConfigManager.Instance(this.organizationid).Config.incentiveskipmonths;

                    config.fixedpercentage = incentiveratio;
                    config.dynamicpercentage = 0;
                    config.IsSales = true;
                    config.salesIncentives = new List<Models.Referencelist.SalesIncentive>();
                    retValue.Add(config);
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        //name as employeename,referencelist.name departmentname,additionalinfo,workingdays,leavedays,employeesalaryheader.incentiveratio
                        string employeename = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();
                        string employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        decimal workingdays = reader["workingdays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["workingdays"]);
                        decimal leavedays = reader["leavedays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["leavedays"]);

                        decimal incentivepercentage = reader["incentivepercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentivepercentage"]);
                        int incentivesource = reader["incentivesource"] == DBNull.Value ? 0 : Convert.ToInt32(reader["incentivesource"]);

                        decimal incentiveratio = reader["incentiveratio"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentiveratio"]);
                        decimal grosspay = reader["grosspay"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grosspay"]);
                        decimal earnings = reader["earnings"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["earnings"]);
                        long departmentid = reader["departmentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["departmentid"]);
                        long sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                        long employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        long companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        long locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        long floorid = reader["floorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["floorid"]);
                        long inchargeid = reader["inchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["inchargeid"]);
                        decimal salestarget = reader["salestarget"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestarget"]);
                        if (incentiveratio <= 0) incentiveratio = 1;
                        if (companyid != 0 && this.companyid != 0 && companyid != this.companyid) continue;
                        //if (departmentid != 0 && this.departmentid != 0 && departmentid != this.departmentid) continue;
                        if (locationid != 0 && this.locationid != 0 && locationid != this.locationid) continue;

                        if (!retValue.Exists(e => e.departmentid == departmentid))
                        {
                            retValue.Add(new IncentiveData()
                            {
                                departmentid = departmentid,
                                department = "Unknown"
                            });
                        }
                        if (
                            (Services.ConfigManager.Instance(this.organizationid).Config.incentivemode == 5 ||
                            Services.ConfigManager.Instance(this.organizationid).Config.incentivemode == 4 || workingdays > 0))
                        {
                            retValue.Find(e => e.departmentid == departmentid).Employees.Add(new IncentiveData.Item()
                            {
                                employeename = employeename,
                                employeeid = employeeid,
                                days = days - leavedays,
                                incentivepercentage = incentivepercentage,
                                incentivesource = incentivesource,
                                //payratio = workingdays <= 0 ? 1 : ((incentiveratio * (100 - (leavedays / workingdays) * 100) / 100)),
                                payratio = workingdays <= 0 ? 1 : (((days - leavedays) / workingdays)),
                                ratio = incentiveratio,
                                sectionid = sectionid,
                                floorid = floorid,
                                departmentid = departmentid,
                                companyid = companyid,
                                employeecode = employeecode,
                                grosspay = grosspay,
                                earnings = earnings,
                                salestarget = salestarget,
                                inchargeid = inchargeid
                            });
                        }
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        long sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                        long companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        long floorid = reader["floorid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["floorid"]);
                        long locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        decimal amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        totalSales += amount;
                        if (retValue.Exists(e => e.IsSales))
                        {
                            retValue.First(e => e.IsSales).SalesValue.Add(new IncentiveData.salevalue()
                            {
                                amount = amount,
                                companyid = companyid,
                                sectionid = sectionid,
                                locationid = locationid,
                                floorid = floorid
                            });
                        }
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        long salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]);
                        decimal amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        foreach (var dpt in retValue)
                        {
                            if (dpt.Employees.Exists(e => e.employeeid == salesmanid))
                            {
                                dpt.Employees.FindAll(e => e.employeeid == salesmanid).ForEach(e => e.salesvalue = amount);
                            }
                        }
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        long salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]);
                        decimal amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        decimal incentive = reader["incentive"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentive"]);
                        decimal incentivebypercentage = reader["incentivebypercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentivebypercentage"]);
                        decimal incentivecount = reader["incentivecount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentivecount"]);
                        foreach (var dpt in retValue)
                        {
                            var employee = dpt.Employees.FindAll(e => e.employeeid == salesmanid);
                            if (employee.Count > 0)
                            {
                                employee.ForEach(e =>
                                {
                                    //e.salesvalue += amount;
                                    e.itemincentive += incentive + incentivebypercentage;
                                    e.itemsalevalue += amount;
                                    e.incentivecount += incentivecount;
                                });
                                break;
                            }
                        }
                    }
                }
            }
            return retValue;
        }


        private List<IncentiveData> BuildMastersByAge(string query, SortedList<string, object> param, List<Range> rangelist)
        {
            List<IncentiveData> retValue = new List<IncentiveData>();
            InfyPOS.Common.BaseData baseData = new Common.BaseData() { Context = new Common.UserClaim() { OrganizationID = organizationid } };
            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.system_isb2bapplication)
                query = query.Replace("and bill.isb2b=false", "");
            if (sectionid <= 0)
                query = query.Replace("and salescounterid=@sectionid", "");


            using (System.Data.IDataReader reader = baseData.ExecuteReader(query, param))
            {
                while (reader.Read())
                {
                    //name as employeename,referencelist.name departmentname,additionalinfo,workingdays,leavedays,employeesalaryheader.incentiveratio
                    Models.Referencelist reflist = new Models.Referencelist();
                    reflist.additionalinfo_json = reader["additionalinfo"] == DBNull.Value ? "null" : reader["additionalinfo"].ToString();
                    reflist.id = reader["id"] == DBNull.Value ? 0 : Convert.ToInt64(reader["id"]);
                    reflist.name = reader["name"] == DBNull.Value ? "" : reader["name"].ToString();

                    if (this.departmentid != 0 && reflist.id != this.departmentid) continue;

                    IncentiveData config = new IncentiveData();
                    config.department = reflist.name;
                    config.departmentid = reflist.id;
                    if (reflist.additionalinfo != null)
                    {
                        decimal percentage = reflist.additionalinfo.fixedincentive + reflist.additionalinfo.dynamicincentive;
                        //decimal.TryParse(reflist.additionalinfo.fixedincentive, out percentage);
                        config.fixedpercentage = percentage;
                        config.dynamicpercentage = reflist.additionalinfo.dynamicincentive;
                        config.IsSales = reflist.additionalinfo.issales;
                        config.salesIncentives = reflist.additionalinfo.salesincentive;
                    }
                    retValue.Add(config);
                }

                if (retValue.Count == 0)
                {

                    IncentiveData config = new IncentiveData();
                    config.department = "Sales";
                    config.departmentid = 0;

                    config.fixedpercentage = 1;
                    config.dynamicpercentage = 0;
                    config.IsSales = true;
                    config.salesIncentives = new List<Models.Referencelist.SalesIncentive>();
                    retValue.Add(config);
                }
                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        //name as employeename,referencelist.name departmentname,additionalinfo,workingdays,leavedays,employeesalaryheader.incentiveratio
                        string employeename = reader["employeename"] == DBNull.Value ? "" : reader["employeename"].ToString();
                        string employeecode = reader["employeecode"] == DBNull.Value ? "" : reader["employeecode"].ToString();
                        decimal workingdays = reader["workingdays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["workingdays"]);
                        decimal leavedays = reader["leavedays"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["leavedays"]);
                        decimal incentiveratio = reader["incentiveratio"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentiveratio"]);
                        decimal grosspay = reader["grosspay"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["grosspay"]);
                        decimal incentive = reader["incentive"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentive"]);

                        long departmentid = reader["departmentid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["departmentid"]);
                        long sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                        long employeeid = reader["employeeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["employeeid"]);
                        long companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        long locationid = reader["locationid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["locationid"]);
                        long inchargeid = reader["inchargeid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["inchargeid"]);
                        long managerid = reader["managerid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["managerid"]);
                        decimal salestarget = reader["salestarget"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["salestarget"]);
                        if (incentiveratio <= 0) incentiveratio = 1;
                        if (companyid != 0 && this.companyid != 0 && companyid != this.companyid) continue;
                        //if (departmentid != 0 && this.departmentid != 0 && departmentid != this.departmentid) continue;
                        if (locationid != 0 && this.locationid != 0 && locationid != this.locationid) continue;

                        if (retValue.Exists(e => e.departmentid == departmentid) &&
                            (Services.ConfigManager.Instance(this.organizationid).Config.incentivemode == 4 ||
                            Services.ConfigManager.Instance(this.organizationid).Config.incentivemode == 5 ||
                            workingdays > 0))
                        {
                            retValue.Find(e => e.departmentid == departmentid).Employees.Add(new IncentiveData.Item()
                            {
                                employeename = employeename,
                                employeeid = employeeid,
                                days = workingdays - leavedays,
                                payratio = workingdays <= 0 ? 1 : ((incentiveratio * (100 - (leavedays / workingdays) * 100) / 100)),
                                ratio = incentiveratio,
                                sectionid = sectionid,
                                departmentid = departmentid,
                                companyid = companyid,
                                employeecode = employeecode,
                                grosspay = grosspay,
                                salestarget = salestarget,
                                inchargeid = inchargeid > 0 ? inchargeid : managerid,
                                incentive = incentive
                            });
                        }
                    }
                }

                if (!retValue.Exists(e => e.IsSales))
                    retValue.Add(new IncentiveData()
                    {
                        IsSales = true,
                        department = "Sales"
                    });

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        long sectionid = reader["sectionid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["sectionid"]);
                        long companyid = reader["companyid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["companyid"]);
                        decimal amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        totalSales += amount;
                        if (retValue.Exists(e => e.IsSales))
                        {
                            retValue.First(e => e.IsSales).SalesValue.Add(new IncentiveData.salevalue()
                            {
                                amount = amount,
                                companyid = companyid,
                                sectionid = sectionid
                            });
                        }
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        string age = reader["Aging"] == DBNull.Value ? "" : reader["Aging"].ToString();
                        long salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]);
                        decimal amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);

                        if (retValue.Exists(e => e.IsSales))
                        {

                            var employee = retValue.First(e => e.IsSales).Employees.FindAll(e => e.employeeid == salesmanid);
                            if (employee.Count == 0)
                            {
                                foreach (var ei in retValue.Where(e => !e.IsSales))
                                {
                                    if (ei.Employees.Exists(e => e.employeeid == salesmanid))
                                    {
                                        retValue.Find(e => e.IsSales).Employees.Add(ei.Employees.Find(e => e.employeeid == salesmanid));
                                        ei.Employees.RemoveAll(e => e.employeeid == salesmanid);
                                    }
                                }
                                employee = retValue.First(e => e.IsSales).Employees.FindAll(e => e.employeeid == salesmanid);
                            }
                            employee.ForEach(e => e.salesvalue += amount);
                            foreach (var emp in employee)
                            {
                                if (emp.Salebyages == null) emp.Salebyages = new List<IncentiveData.salebyage>();
                                if (age == "unknown")
                                {
                                    emp.Salebyages.Add(new IncentiveData.salebyage()
                                    {
                                        name = "No Config",
                                        from = 0,
                                        to = 0,
                                        salevalue = amount
                                    });
                                }
                                else
                                {
                                    var r = rangelist.Find(e => e.id == age);
                                    emp.Salebyages.Add(new IncentiveData.salebyage()
                                    {
                                        name = string.Format("{0} - {1}", r.start, r.end),
                                        from = r.start,
                                        to = r.end,
                                        salevalue = amount,
                                        incentive = Math.Round(amount * r.incentive / 100, 0)
                                    });

                                }
                            }
                        }
                    }
                }

                if (reader.NextResult())
                {
                    while (reader.Read())
                    {
                        long salesmanid = reader["salesmanid"] == DBNull.Value ? 0 : Convert.ToInt64(reader["salesmanid"]);
                        decimal amount = reader["amount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["amount"]);
                        decimal incentive = reader["incentive"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentive"]);
                        decimal incentivebypercentage = reader["incentivebypercentage"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentivebypercentage"]);
                        decimal incentivecount = reader["incentivecount"] == DBNull.Value ? 0 : Convert.ToDecimal(reader["incentivecount"]);
                        if (retValue.Exists(e => e.IsSales))
                        {

                            var employee = retValue.First(e => e.IsSales).Employees.FindAll(e => e.employeeid == salesmanid);
                            if (employee.Count == 0)
                            {
                                foreach (var ei in retValue.Where(e => !e.IsSales))
                                {
                                    if (ei.Employees.Exists(e => e.employeeid == salesmanid))
                                    {
                                        retValue.Find(e => e.IsSales).Employees.Add(ei.Employees.Find(e => e.employeeid == salesmanid));
                                        ei.Employees.RemoveAll(e => e.employeeid == salesmanid);
                                    }
                                }
                                employee = retValue.First(e => e.IsSales).Employees.FindAll(e => e.employeeid == salesmanid);
                            }
                            employee.ForEach(e => e.salesvalue += amount);
                            foreach (var emp in employee)
                            {
                                if (emp.Salebyages == null) emp.Salebyages = new List<IncentiveData.salebyage>();
                                emp.Salebyages.Add(new IncentiveData.salebyage()
                                {
                                    name = "Star",
                                    from = -1,
                                    to = -1,
                                    salevalue = amount,
                                    incentive = incentive + incentivebypercentage,
                                    incentivecount = incentivecount
                                });
                            }
                        }
                    }
                }

            }
            return retValue;
        }

    }
}