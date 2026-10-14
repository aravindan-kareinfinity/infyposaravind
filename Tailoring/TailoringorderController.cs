
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
    [RoutePrefix("TailoringorderService")]
    public class TailoringorderServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Tailoringorder>();
            result.Item = new Tailoringorder();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<TailoringorderCreteria> request)
        {
            TailoringorderDataService TailoringorderData = new TailoringorderDataService();
            List<Tailoringorder> result = await Task<List<Tailoringorder>>.Run(() => TailoringorderData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchPayment")]
        public async Task<IHttpActionResult> SearchPayment(ActionRequest<TailoringorderCreteria> request)
        {
            TailoringorderDataService TailoringorderData = new TailoringorderDataService();
            List<TailoringitemPayments> result = await Task<List<TailoringitemPayments>>.Run(() => TailoringorderData.SearchPayment(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchInbox")]
        public async Task<IHttpActionResult> SearchInbox(ActionRequest<TailoringorderCreteria> request)
        {
            
            if (request.Item.status == 8)
            {
                TailoringpaymentDataService tailoringpaymentDataService =new TailoringpaymentDataService();
                List<TailoringPaymentDetail> result = await Task<List<TailoringPaymentDetail>>.Run(() => tailoringpaymentDataService.Search(new TailoringpaymentCreteria()
                {
                    companyid = request.Item.companyid,
                    locationid =request.Item.locationid,
                    fromdate = request.Item.fromdate,
                    todate = request.Item.todate,
                    tailorid = request.Item.tailorid
                }));
                return this.Ok(result);
            }
            else
            {
                TailoringorderDataService TailoringorderData = new TailoringorderDataService();
                List<TailoringitemInbox> result = await Task<List<TailoringitemInbox>>.Run(() => TailoringorderData.SearchInbox(request.Item));
                return this.Ok(result);
            }
        }


        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Tailoringorder> request)
        {
            TailoringorderDataService TailoringorderData = new TailoringorderDataService();
            if (request.Item.id <= 0)
            {
                Tailoringorder result = await Task<Tailoringorder>.Run(() =>
                {
                    TailoringorderData.Insert(request.Item);
                    if(request.Item.id>0)
                        request.Item.printdocument = PrintDocument(request.Item, "TAILORING");
                    return request.Item;
                });
                return this.Ok(result);
            }
            else
            {
                Tailoringorder result = await Task<Tailoringorder>.Run(() => TailoringorderData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Deliver")]
        public async Task<IHttpActionResult> Deliver(ActionRequest<Tailoringorder> request)
        {
            TailoringorderDataService TailoringorderData = new TailoringorderDataService();
            Tailoringorder result = await Task<Tailoringorder>.Run(() => TailoringorderData.Deliver(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Tailoringorder> request)
        {
            TailoringorderDataService TailoringorderData = new TailoringorderDataService();
            Tailoringorder result = await Task<Tailoringorder>.Run(() =>
            {
                TailoringorderData.Insert(request.Item);
                return request.Item;
            });
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Tailoringorder> request)
        {
            TailoringorderDataService TailoringorderData = new TailoringorderDataService();
            Tailoringorder result = await Task<Tailoringorder>.Run(() => TailoringorderData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Tailoringorder> request)
        {
            TailoringorderDataService TailoringorderData = new TailoringorderDataService();
            bool result = await Task<bool>.Run(() => TailoringorderData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("PrintPayment")]
        public async Task<IHttpActionResult> PrintPayment(ActionRequest<Tailoringpayment> request)
        {
            TailoringpaymentDataService TailoringorderData = new TailoringpaymentDataService();
            FileContentInfo result = await Task<FileContentInfo>.Run(() =>
            {
                TailoringPaymentPrint torder = TailoringorderData.Print(request.Item.paymentcode);
                return Print(TailoringorderData.Context.OrganizationID, torder);
            });
            return new FileActionResult(false, "", result.Extension, result.Content);
        }


        [HttpPost]
        [Route("CancelPayment")]
        public async Task<IHttpActionResult> CancelPayment(ActionMulitpleRequest<Tailoringpayment> request)
        {
            TailoringpaymentDataService TailoringorderData = new TailoringpaymentDataService();
            bool result = await Task<bool>.Run(() =>
            {
                return TailoringorderData.Cancel(request.Items);
            });
            return this.Ok(result);
        }

        [HttpGet]
        [Route("Print/{id}")]
        public async Task<IHttpActionResult> Print(string id)
        {
            TailoringorderDataService TailoringorderData = new TailoringorderDataService();
            string result = await Task<string>.Run(() =>
            {
                var torder = TailoringorderData.Search(new TailoringorderCreteria() { id = long.Parse(id) }).First();
                return PrintDocument(torder, "TAILORING");
            });
            return this.Ok(result);
        }

        public FileContentInfo Print(long organizationid, TailoringPaymentPrint result)
        {
            if (result == null)
            {
                var fci = new FileContentInfo(new ByteArrayContent(new byte[0]));
                fci.FileName = "debitnote.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "TAILORINGPAYMENT",
                organizationid = result.companyid
            });

            if (lstconfig == null || lstconfig.Count == 0)
            {
                lstconfig = config.Search(new ConfigurationCreteria
                {
                    key = "TAILORINGPAYMENT",
                });
            }
            if (lstconfig == null || lstconfig.Count == 0)
            {
                var fci = new FileContentInfo(new ByteArrayContent(new byte[0]));
                fci.FileName = "tailoringpayment.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;

            labreport.DataItems = result.items;
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

            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "debitnote.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }
        public string PrintDocument(Tailoringorder tailoringorder, string key)
        {

            var odelivery = Newtonsoft.Json.JsonConvert.DeserializeObject<TailoringorderPrint>(Newtonsoft.Json.JsonConvert.SerializeObject(tailoringorder));

            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                key = key,
                locationid = odelivery.locationid,
                organizationid = 0
            });
            if (reportformats.Count == 0)
                reportformats = config.Search(new ConfigurationCreteria
                {
                    key = key,
                    organizationid = 0
                });
            if (reportformats.Count == 0)
                return "";
            var additional = new SortedDictionary<string, object>();
            var imageData = new Common.ImageData() { Context = new UserClaim() { OrganizationID = config.Context.OrganizationID } };
            ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(imageData.Context.OrganizationID, new ReportLibrary.DotMatrixReporter.PrinterConfig());
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(imageData.Context.OrganizationID, reportformats[0].reportformat.format);

            List<string> imageids = labreport.format.GetImages();
            labreport.Additional = new SortedDictionary<string, object>();
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null && file.data != null)
                {
                    additional.Add("IMG-" + imgid, file.data);
                }
            }

            ReferencelistDataService referencelistDataService = new ReferencelistDataService();
            var location = referencelistDataService.Search(new ReferencelistCreteria()
            {
                id = odelivery.locationid
            }).First();

            odelivery.location = new CompanyLocation(location);
            if (odelivery.customerid > 0)
            {
                CustomerDataService customerDataService = new CustomerDataService();
                odelivery.customer = customerDataService.Search(new CustomerCreteria()
                {
                    id = odelivery.customerid
                }).First();
            }
            else
            {
                odelivery.customer = new Customer();
            }

            if (odelivery.companyid > 0)
            {
                CompanyDataService customerDataService = new CompanyDataService();
                odelivery.company = customerDataService.Search(new CompanyCreteria()
                {
                    id = odelivery.companyid
                }).First();
            }
            else
            {
                odelivery.company = new Company();
            }


            if (odelivery.receivedby > 0)
            {
                EmployeeDataService employeeDataService = new EmployeeDataService();
                var emplist = employeeDataService.SearchHeader(new EmployeeCreteria() { id = odelivery.receivedby });
                if (emplist.Count > 0)
                    odelivery.receivedemployee = emplist[0].name;
            }
            
            if (reportformats[0].reportformat.format.Contains("PDF='true'"))
            {
                ReportLibrary.JSONPrinter printer = new ReportLibrary.JSONPrinter();
                printer.Report = labreport;
                printer.IsThermalPrint = false;
                printer.EnablePageNumber = false;
                var document = printer.Print(labreport.format.GetDefaultSetting());
                document.noofcopy = reportformats[0].noofcopy;
                return Convert.ToBase64String(System.Text.UnicodeEncoding.Unicode.GetBytes(Newtonsoft.Json.JsonConvert.SerializeObject(document)));
            }
            else
            {
                byte[] document = null;

                document = labreport.format.RenderBytes(new ImageData(), odelivery, odelivery.Tailoringitems, additional, true);

                using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
                {
                    System.IO.BinaryWriter bw = new System.IO.BinaryWriter(ms);
                    bw.Write(document);
                    int c = 1;
                    while (c < reportformats[0].noofcopy)
                    {
                        bw.Write(document);
                        c++;
                    }
                    return ms.Length > 0 ? Convert.ToBase64String(ms.ToArray()) : "";
                }
            }

           
            //return Convert.ToBase64String(document);
        }
    }
}