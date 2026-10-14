
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
    [RoutePrefix("CustomerpaymentService")]
    public class CustomerpaymentServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Customerpayment>();
            result.Item = new Customerpayment();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<CustomerpaymentCreteria> request)
        {
            CustomerpaymentDataService CustomerpaymentData = new CustomerpaymentDataService();
            List<Customerpayment> result = await Task<List<Customerpayment>>.Run(() => CustomerpaymentData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchCreditSaleCollection")]
        [WebAPI.InfyAuthorize(PermissionSet.settlementreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchCreditSaleCollection(ActionRequest<RetailReportCreteria> request)
        {
            CustomerpaymentDataService CustomerpaymentData = new CustomerpaymentDataService();
            List<SettlementReport> result = await Task<List<SettlementReport>>.Run(() => CustomerpaymentData.SearchCreditSaleCollection(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Customerpayment> request)
        {
            //throw new Exception("test");
            CustomerpaymentDataService CustomerpaymentData = new CustomerpaymentDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                Customerpayment result = await Task<Customerpayment>.Run(() => CustomerpaymentData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                Customerpayment result = await Task<Customerpayment>.Run(() => CustomerpaymentData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Customerpayment> request)
        {
            CustomerpaymentDataService CustomerpaymentData = new CustomerpaymentDataService();
            Customerpayment result = await Task<Customerpayment>.Run(() => CustomerpaymentData.Insert(request.Item));
            return this.Ok(result);
        }

        [Route("Print")]
        [WebAPI.InfyAuthorize(PermissionSet.customerpayment, InfyPOS.Common.Action.Print)]
        public async Task<IHttpActionResult> Print(ActionRequest<CustomerpaymentCreteria> request)
        {
            if (request.Item.id == 0) return this.NotFound();
            CustomerpaymentDataService CustomerpaymentData = new CustomerpaymentDataService();
            List<Customerpayment> result = await Task<List<Customerpayment>>.Run(() => CustomerpaymentData.Search(request.Item));
            if (result != null && result.Count > 0)
            {
                result.First().printdocument = Print(CustomerpaymentData.Context.OrganizationID, result.First());
                return this.Ok(result.First());
            }
            return this.NotFound();
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid,string id)
        {
            CustomerpaymentDataService CustomerpaymentData = new CustomerpaymentDataService();
            CustomerpaymentData.Context = new UserClaim()
            {
                OrganizationID = long.Parse(organizationid)
            };
            List<Customerpayment> result = CustomerpaymentData.Search(new CustomerpaymentCreteria() { id = long.Parse(id) });
            if (result != null && result.Count > 0)
            {
                FileContentInfo byteArray = PrintPDF(CustomerpaymentData.Context.OrganizationID, result[0]);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            return this.NotFound();
        }

        public string Print(long organizationid, Customerpayment source)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                key = "CUSTOMERRECEIPT",
                organizationid = source.companyid
            });

            if (reportformats.Count > 0)
            {
                try
                {
                    CompanyDataService companyDataService = new CompanyDataService();
                    source.company = companyDataService.Search(new CompanyCreteria()
                    {
                        id = source.companyid
                    }).First();
                    source.company.parseaddress();
                    CustomerDataService customerDataService = new CustomerDataService();
                    source.customer = customerDataService.Search(new CustomerCreteria()
                    {
                        id = source.customerid
                    }).First();

                    ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                    source.location = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = source.locationid
                    }).First();

                    
                    ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
                    ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid, new ReportLibrary.DotMatrixReporter.PrinterConfig());
                    labreport.format = ReportLibrary.DosFormatParser.ParseXML(customerDataService.Context.OrganizationID, reportformats[0].reportformat.format);

                    labreport.Additional = new SortedDictionary<string, object>();
                    var additional = new SortedDictionary<string, object>();
                    List<string> imageids = labreport.format.GetImages();
                    var imageData = new Common.ImageData();
                    if (!string.IsNullOrEmpty(source.company.logo))
                    {
                        var file = imageData.GetData(source.company.logo.LongFromBase62());
                        if (file != null)
                            additional.Add("LOGO", file.data);
                    }
                    foreach (var imgid in imageids.FindAll(x => x.Length > 6))
                    {
                        var file = imageData.GetData(imgid.LongFromBase62());
                        if (file != null && file.data != null)
                        {
                            var img = imageData.FromBytes(file.data);
                            labreport.Additional.Add("IMG-" + imgid, img);
                        }
                    }
                    var document = labreport.format.RenderBytes(new ImageData(), source, source.Customerinvoice, additional, true);
                    return Convert.ToBase64String(document);
                }
                catch (Exception exp)
                {
                    WebAPI.Common.Logger.Instance(new BaseData().Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, exp.StackTrace);
                    throw exp;
                }
            }
            return "";
        }

        public FileContentInfo PrintPDF(long organizationid, Customerpayment source)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var reportformats = config.Search(new ConfigurationCreteria
            {
                key = "CUSTOMERRECEIPTPDF",
                organizationid = source.companyid
            });

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
                labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, reportformats[0].reportformat.format);
                labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
                labreport.Additional = new SortedDictionary<string, object>();

                ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
                printer.Report = labreport;
                printer.EnablePageNumber = reportformats[0].reportformat.format.Contains("PageCount");
                labreport.DataSource = source;
                labreport.DataItems = source.Customerinvoice;

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
                    fci.FileName = "payment.pdf";
                    fci.Extension = "application/pdf";
                    return fci;
                }
            }
            else
            {
                throw new Exception("CUSTOMER PAYMENT RECEIPT NOT CONFIGURED FOR THE SELECTED COMPANY - " + source.company);
            }
            return null;
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Customerpayment> request)
        {
            CustomerpaymentDataService CustomerpaymentData = new CustomerpaymentDataService();
            Customerpayment result = await Task<Customerpayment>.Run(() => CustomerpaymentData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Customerpayment> request)
        {
            CustomerpaymentDataService CustomerpaymentData = new CustomerpaymentDataService();
            bool result = await Task<bool>.Run(() => CustomerpaymentData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("CustomerLedger/{companyid}/{locationid}/{id}/{from}/{to}")]
        public async Task<IHttpActionResult> CustomerLedger(string companyid,string locationid,
                                    string id, string from, string to)
        {
            CustomerpaymentDataService SupplierpaymentData = new CustomerpaymentDataService();
            PrintableLedger printable = await Task<FileContentInfo>.Run(() =>
            {
                long lngid = 0;
                long lngcmp = 0;
                long.TryParse(id, out lngid);
                long.TryParse(companyid, out lngcmp);
                return SupplierpaymentData.PrintSummary(true,lngid, lngcmp,
                BaseEntity.GetDataFromString(from), BaseEntity.GetDataFromString(to));
            });
            return this.Ok(printable);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintSummaryPDF/{organizationid}/{companyid}/{locationid}/{id}/{from}/{to}")]
        public async Task<IHttpActionResult> PrintSummaryPDF(string organizationid, string companyid,
           string locationid, string id, string from, string to)
        {
            CustomerpaymentDataService SupplierpaymentData = new CustomerpaymentDataService();
            SupplierpaymentData.Context = new UserClaim()
            {
                OrganizationID = long.Parse(organizationid)
            };
            try
            {

                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    PrintableLedger print = SupplierpaymentData.PrintSummary(true,long.Parse(id), long.Parse(companyid),
                      BaseEntity.GetDataFromString(from), BaseEntity.GetDataFromString(to));
                    LedgerServiceController ledgerServiceController = new LedgerServiceController();
                    return ledgerServiceController.PrintLedgerPDF(SupplierpaymentData.Context.OrganizationID, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(SupplierpaymentData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintCheuqeAdvicePDF/" + id);
                return this.Ok(exp);
            }
        }


    }
}