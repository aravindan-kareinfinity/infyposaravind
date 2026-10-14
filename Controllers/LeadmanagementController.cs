
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
    [RoutePrefix("LeadmanagementService")]
    public class LeadmanagementServiceController : ApiController
    {
        [AllowAnonymous]
        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Leadmanagement>();
            result.Item = new Leadmanagement();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<LeadmanagementCreteria> request)
        {
            LeadmanagementDataService LeadmanagementData = new LeadmanagementDataService();
            List<Leadmanagement> result = await Task<List<Leadmanagement>>.Run(() => LeadmanagementData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Leadmanagement> request)
        {
            LeadmanagementDataService LeadmanagementData = new LeadmanagementDataService();
            if (request.Item.id <= 0)
            {
                Leadmanagement result = await Task<Leadmanagement>.Run(() => LeadmanagementData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Leadmanagement result = await Task<Leadmanagement>.Run(() => LeadmanagementData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Leadmanagement> request)
        {
            LeadmanagementDataService LeadmanagementData = new LeadmanagementDataService();
            Leadmanagement result = await Task<Leadmanagement>.Run(() => LeadmanagementData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Leadmanagement> request)
        {
            LeadmanagementDataService LeadmanagementData = new LeadmanagementDataService();
            Leadmanagement result = await Task<Leadmanagement>.Run(() => LeadmanagementData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Leadmanagement> request)
        {
            LeadmanagementDataService LeadmanagementData = new LeadmanagementDataService();
            bool result = await Task<bool>.Run(() => LeadmanagementData.Delete(request.Item));
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string id)
        {
            LeadmanagementDataService LeadmanagementData = new LeadmanagementDataService();
            LeadmanagementData.Context = new UserClaim()
            {
                OrganizationID = long.Parse(organizationid)
            };
            List<Leadmanagement> result = LeadmanagementData.Search(new LeadmanagementCreteria() { id = long.Parse(id) });
            if (result != null && result.Count > 0)
            {
                FileContentInfo byteArray = PrintPDF(long.Parse(organizationid), result[0]);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            return this.NotFound();
        }

        public FileContentInfo PrintPDF(long organizationid, Leadmanagement source)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var reportformats = config.Search(new ConfigurationCreteria
            {
                key = "LEADFORMPDF",
                organizationid = source.companyid
            });

            if (source.assignedbyid > 0)
            {
                UsersDataService usersDataService = new UsersDataService();
                var user = usersDataService.Search(new UsersCreteria()
                {
                    id = source.assignedbyid
                });
                if (user.Count > 0)
                    source.assignedby = user.First().title;
            }
            if (source.assignedid > 0)
            {
                UsersDataService usersDataService = new UsersDataService();
                var user = usersDataService.Search(new UsersCreteria()
                {
                    id = source.assignedid
                });
                if (user.Count > 0)
                    source.assignedto = user.First().title;
            }
            CompanyDataService companyDataService = new CompanyDataService();
            companyDataService.Context = config.Context;
            source.company = companyDataService.Search(new CompanyCreteria()
            {
                id = source.companyid
            }).First();
            source.company.parseaddress();
            CustomerDataService customerDataService = new CustomerDataService();
            customerDataService.Context = config.Context;
            source.customer = customerDataService.Search(new CustomerCreteria()
            {
                id = source.customerid
            }).First();

            if (reportformats.Count > 0)
            {
                ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
                labreport.format = ReportLibrary.DosFormatParser.ParseXML(config.Context.OrganizationID, reportformats[0].reportformat.format);
                labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
                labreport.Additional = new SortedDictionary<string, object>();

                ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
                printer.Report = labreport;
                labreport.DataSource = source;
                labreport.DataItems = source;

                List<string> imageids = labreport.format.GetImages();
                var imageData = new Common.ImageData();
                foreach (var imgid in imageids.FindAll(x => x.Length > 6))
                {
                    var file = imageData.GetData(imgid.LongFromBase62());
                    if (file != null && file.data != null)
                    {
                        var img = imageData.FromBytes(file.data);
                        labreport.Additional.Add("IMG-" + imgid, img);
                    }
                }
                if (!string.IsNullOrEmpty(source.company.logo))
                {
                    var file = imageData.GetData(source.company.logo.LongFromBase62());
                    if (file != null)
                        labreport.Additional.Add("LOGO", file.data);
                }

                using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
                {
                    var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                    fci.FileName = "leadform.pdf";
                    fci.Extension = "application/pdf";
                    return fci;
                }
            }
            else
            {
                throw new Exception("LEAD FORM NOT CONFIGURED FOR THE SELECTED COMPANY - " + source.company);
            }
            return null;
        }


    }
}