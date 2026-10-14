
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
using WebAPI.Data;

namespace InfyPOS.Controllers
{
    [RoutePrefix("EmployeeService")]
    public class EmployeeServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Employee>();
            result.Item = new Employee();
            result.Item.addionalinfo = new Employee.Addionalinfo();
            result.Item.bank = new Employee.Bank();
            result.Item.classification = new Employee.Classification();
            result.Item.disciplinary = new Employee.Disciplinary();
            result.Item.educationandexperiance = new Employee.EducationAndExperiance();
            result.Item.family = new Employee.Family();
            result.Item.license = new Employee.License();
            result.Item.passport = new Employee.Passport();
            result.Item.permanentaddress = new Employee.Permanentaddress();
            result.Item.personal = new Employee.Personal();
            result.Item.pfandesi = new Employee.Pfandesi();
            result.Item.presentaddress = new Employee.Presentaddress();
            result.Item.remarks = new Employee.Remarks();
            result.Item.trainings = new Employee.Trainings();
            return this.Ok(result);
        }

        [HttpGet]
        [Route("EmployeeCode/{organizationid}/{locationid}/{id}")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> EmployeeCode(string organizationid, string locationid, string id)
        {
            EmployeeCreteria cret = new EmployeeCreteria();
            cret.employeecode = id;
            //cret.currentEmployees = true;
            EmployeeDataService EmployeeData = new EmployeeDataService() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };

            // Restrict Sales Man code by Designation 
            if (InfyPOS.Services.ConfigManager.Instance(EmployeeData.Context.OrganizationID) != null &&
                InfyPOS.Services.ConfigManager.Instance(EmployeeData.Context.OrganizationID).LocationConfig[long.Parse(locationid)] != null)
                cret.desingnationid = InfyPOS.Services.ConfigManager.Instance(EmployeeData.Context.OrganizationID).LocationConfig[long.Parse(locationid)].salesman_designationid;

            int _locationid = 0;
            int.TryParse(locationid, out _locationid);

            // Restrict Sales man by Working Location
            if (InfyPOS.Services.ConfigManager.Instance(EmployeeData.Context.OrganizationID) != null &&
                InfyPOS.Services.ConfigManager.Instance(EmployeeData.Context.OrganizationID).LocationConfig[long.Parse(locationid)] != null &&
                InfyPOS.Services.ConfigManager.Instance(EmployeeData.Context.OrganizationID).LocationConfig[long.Parse(locationid)].system_enablelocbasedsalesman)
                cret.locationid = _locationid;

            List<Employee> result = await Task<List<Employee>>.Run(() => EmployeeData.Search(cret));
            if (result == null || result.Count == 0)
                return this.Ok(new Employee());

            if (result.Count > 1)
            {
                if (_locationid > 0 && result.Exists(e => e.locationid == _locationid))
                {
                    return this.Ok(result.Find(e => e.locationid == _locationid));
                }
            }

            return this.Ok(result[0]);
        }

        [HttpGet]
        [Route("Code/{organizationid}/{id}/{location=null}")]
        [AllowAnonymous]
        public async Task<IHttpActionResult> Search(string organizationid,string id,string location)
        {
            EmployeeCreteria cret = new EmployeeCreteria();
            cret.employeecode = id;
            EmployeeDataService EmployeeData = new EmployeeDataService() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            List<Employee> result = await Task<List<Employee>>.Run(() => EmployeeData.Search(cret));
            if (result == null || result.Count == 0)
                return this.Ok(new Employee());

            return this.Ok(result[0]);
        }

        [HttpPost]
        [Route("PrintBarcode")]
        public async Task<IHttpActionResult> PrintBarcode(ActionRequest<SalesmanPrintCreteria> request)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                id = request.Item.reportid
            });

            if (reportformats.Count >0)
            {
                InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode(request.Item.companytitle,
                             reportformats[0].reportformat.format, reportformats[0].reportformat.mode, reportformats[0].nooflines);

                List<Salesman2Print> salesman2print = new List<Salesman2Print>();
                var barcodelist = request.Item.items.ToList();
                for (int i = 0; i < request.Item.copy; i++)
                {
                    salesman2print.AddRange(barcodelist);
                }
                var barcodeList = salesman2print.OrderBy(e => e.id).ToList().ConvertAll(e => (Report.IBarcodeFormat)e);
                var response = barcodeprinter.Render(barcodeList);
                List<string> base64Response = new List<string>();
                foreach (var item in response)
                {
                    var bytes = System.Text.Encoding.ASCII.GetBytes(item);
                    base64Response.Add(Convert.ToBase64String(bytes));
                }
                return this.Ok(base64Response);
            }

            return this.NotFound();
        }

      

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<EmployeeCreteria> request)
        {
            EmployeeDataService EmployeeData = new EmployeeDataService();
            List<Employee> result = await Task<List<Employee>>.Run(() => EmployeeData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Employee> request)
        {
            EmployeeDataService EmployeeData = new EmployeeDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Employee result = await Task<Employee>.Run(() => EmployeeData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Employee result = await Task<Employee>.Run(() => EmployeeData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Employee> request)
        {
            EmployeeDataService EmployeeData = new EmployeeDataService();
            Employee result = await Task<Employee>.Run(() => EmployeeData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Employee> request)
        {
            EmployeeDataService EmployeeData = new EmployeeDataService();
            Employee result = await Task<Employee>.Run(() => EmployeeData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Employee> request)
        {
            EmployeeDataService EmployeeData = new EmployeeDataService();
            bool result = await Task<bool>.Run(() => EmployeeData.Delete(request.Item));
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{orgainzationid}/{id}")]
        public async Task<IHttpActionResult> PrintPDF(string orgainzationid,string id)
        {
            try
            {
                if (long.Parse(id) == 0) return null;
                EmployeeDataService EmployeeData = new EmployeeDataService();
                EmployeeData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(orgainzationid)
                };
                var result = EmployeeData.Search(new EmployeeCreteria() { id = long.Parse(id) });

                if (result.Count == 1 && result[0].companyid > 0)
                {
                    CompanyDataService companyDataService = new CompanyDataService();
                    result[0].company = companyDataService.Search(new CompanyCreteria()
                    {
                        id = result[0].companyid
                    }).First();
                    result[0].company.parseaddress();
                }
                FileContentInfo byteArray = PrintApplicationForm(EmployeeData.Context.OrganizationID, result.First());
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }
        private FileContentInfo PrintApplicationForm(long organizationid, Employee result)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "APPLICATIONFORM"
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;
            ReportLibrary.LaserReporter labreport = new ReportLibrary.LaserReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid,lstconfig[0].reportformat.format);
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;

            List<string> imageids = labreport.format.GetImages();
            var imageData = new Common.ImageData();
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                if (!string.IsNullOrEmpty(imgid))
                {
                    var file = imageData.GetData(imgid.LongFromBase62());
                    if (file != null)
                    {
                        var img = imageData.FromBytes(file.data);
                        labreport.Additional.Add("IMG-" + imgid, img);
                    }
                }
            }

            using (System.IO.MemoryStream stream = printer.PrintOnMillimeter(labreport.format.GetDefaultSetting()))
            {
                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "debitnote.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }
    }
}