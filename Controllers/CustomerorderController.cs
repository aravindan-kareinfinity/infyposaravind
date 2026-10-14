
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
    [RoutePrefix("CustomerorderService")]
    public class CustomerorderServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Customerorder>();
            result.Item = new Customerorder();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<CustomerorderCreteria> request)
        {
            CustomerorderDataService CustomerorderData = new CustomerorderDataService();
            List<Customerorder> result = await Task<List<Customerorder>>.Run(() => CustomerorderData.Search(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("SearchAdvanceCollection")]
        [WebAPI.InfyAuthorize(PermissionSet.settlementreport, InfyPOS.Common.Action.View)]
        public async Task<IHttpActionResult> SearchAdvanceCollection(ActionRequest<RetailReportCreteria> request)
        {
            CustomerorderDataService CustomerorderData = new CustomerorderDataService();
            List<SettlementReport> result = await Task<List<SettlementReport>>.Run(() => CustomerorderData.SearchAdvanceCollection(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Customerorder> request)
        {
            CustomerorderDataService CustomerorderData = new CustomerorderDataService();
            if (request.Item.id <= 0)
            {
                Customerorder result = await Task<Customerorder>.Run(() => CustomerorderData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Customerorder result = await Task<Customerorder>.Run(() => CustomerorderData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Customerorder> request)
        {
            CustomerorderDataService CustomerorderData = new CustomerorderDataService();
            Customerorder result = await Task<Customerorder>.Run(() => CustomerorderData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Customerorder> request)
        {
            CustomerorderDataService CustomerorderData = new CustomerorderDataService();
            Customerorder result = await Task<Customerorder>.Run(() => CustomerorderData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("UpdateStatus")]
        public async Task<IHttpActionResult> UpdateStatus(ActionRequest<Customerorder> request)
        {
            CustomerorderDataService CustomerorderData = new CustomerorderDataService();
            Customerorder result = await Task<Customerorder>.Run(() => CustomerorderData.UpdateStatus(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Customerorder> request)
        {
            CustomerorderDataService CustomerorderData = new CustomerorderDataService();
            bool result = await Task<bool>.Run(() => CustomerorderData.Delete(request.Item));
            return this.Ok(result);
        }

        [Route("Print")]
        public async Task<IHttpActionResult> Print(ActionRequest<CustomerorderCreteria> request)
        {
            if (request.Item.id == 0) return this.NotFound();
            CustomerorderDataService CustomerorderData = new CustomerorderDataService();
            List<Customerorder> result = await Task<List<Customerorder>>.Run(() => CustomerorderData.Search(request.Item));
            if (result != null && result.Count > 0)
            {
                result.First().printdocument = Print(CustomerorderData.Context.OrganizationID, result.First());
                return this.Ok(result.First());
            }
            return this.NotFound();
        }
        public string Print(long organizationid, Customerorder source)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                key = "CUSTOMERORDER",
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

                    if (source.locationid > 0)
                    {
                        ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                        var location = referencelistDataService.Search(new ReferencelistCreteria()
                        {
                            id = source.locationid
                        }).First();

                        source.location = new CompanyLocation(location);

                        if (location.additionalinfo != null && location.additionalinfo.locationconfig != null
                            && location.additionalinfo.locationconfig.cityid > 0)
                        {
                            var cityinfo = referencelistDataService.Search(new ReferencelistCreteria()
                            {
                                id = location.additionalinfo.locationconfig.cityid
                            });
                            if (cityinfo.Count > 0)
                                source.location.cityname = cityinfo[0].name;
                        }
                        if (location.additionalinfo != null && location.additionalinfo.locationconfig != null
                            && location.additionalinfo.locationconfig.stateid > 0)
                        {
                            var stateinfo = referencelistDataService.Search(new ReferencelistCreteria()
                            {
                                id = location.additionalinfo.locationconfig.stateid
                            });
                            if (stateinfo.Count > 0)
                                source.location.statename = stateinfo[0].name;
                        }

                    }
                    if (source.paymentinfo != null && source.paymentinfo.Count > 0 && source.paymentinfo[0].counterid > 0)
                    {
                        ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                        var counterlist = referencelistDataService.Search(new ReferencelistCreteria()
                        {
                            ids = source.paymentinfo.ConvertAll(x => x.counterid)
                        });

                        if (counterlist?.Count > 0)
                            source.paymentinfo.ForEach(item =>
                                item.countername = counterlist.FirstOrDefault(e => e.id == item.counterid)?.name);
                    }

                    ReportLibrary.DefaultPageSettings setting = new ReportLibrary.DefaultPageSettings();
                    ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid, new ReportLibrary.DotMatrixReporter.PrinterConfig());
                    labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, reportformats[0].reportformat.format);

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
                    var document = labreport.format.RenderBytes(new ImageData(), source, source.orderitems, additional, true);
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

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}/{itemid?}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid,string id, string itemid = null)
        {
            long _itemid = 0;
            if (!string.IsNullOrEmpty(itemid))
            {
                long.TryParse(itemid, out _itemid);
            }
            CustomerorderDataService CustomerorderData = new CustomerorderDataService();
            CustomerorderData.Context = new UserClaim()
            {
                OrganizationID = long.Parse(organizationid)
            };
            List<Customerorder> result = CustomerorderData.Search(new CustomerorderCreteria() { id = long.Parse(id) });
            if (result != null && result.Count > 0)
            {
                FileContentInfo byteArray = PrintPDF(long.Parse(organizationid), result[0]);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            return this.NotFound();
        }


        public FileContentInfo PrintPDF(long organizationid, Customerorder source)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var reportformats = config.Search(new ConfigurationCreteria
            {
                key = "CUSTOMERORDERPDF",
                organizationid = source.companyid
            });

            if (source.companyid > 0)
            {
                CompanyDataService companyDataService = new CompanyDataService();
                companyDataService.Context = config.Context;
                source.company = companyDataService.Search(new CompanyCreteria()
                {
                    id = source.companyid
                }).First();
                source.company.parseaddress();
            }

            if (source.customerid > 0)
            {
                CustomerDataService customerDataService = new CustomerDataService();
                customerDataService.Context = config.Context;
                source.customer = customerDataService.Search(new CustomerCreteria()
                {
                    id = source.customerid
                }).First();
            }

            if (source.locationid > 0)
            {
                ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                var location = referencelistDataService.Search(new ReferencelistCreteria()
                {
                    id = source.locationid
                }).First();

                source.location = new CompanyLocation(location);

                if (location.additionalinfo != null && location.additionalinfo.locationconfig != null
                    && location.additionalinfo.locationconfig.cityid > 0)
                {
                    var cityinfo = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = location.additionalinfo.locationconfig.cityid
                    });
                    if (cityinfo.Count > 0)
                        source.location.cityname = cityinfo[0].name;
                }
                if (location.additionalinfo != null && location.additionalinfo.locationconfig != null
                    && location.additionalinfo.locationconfig.stateid > 0)
                {
                    var stateinfo = referencelistDataService.Search(new ReferencelistCreteria()
                    {
                        id = location.additionalinfo.locationconfig.stateid
                    });
                    if (stateinfo.Count > 0)
                        source.location.statename = stateinfo[0].name;
                }

            }

            if (reportformats.Count > 0)
            {
                ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
                labreport.format = ReportLibrary.DosFormatParser.ParseXML(config.Context.OrganizationID, reportformats[0].reportformat.format);
                labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
                labreport.Additional = new SortedDictionary<string, object>();

                ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
                printer.Report = labreport;
                labreport.DataSource = source;
                labreport.DataItems = source.orderitems;

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
                    fci.FileName = "order.pdf";
                    fci.Extension = "application/pdf";
                    return fci;
                }
            }
            else
            {
                throw new Exception("CUSTOMER ORDER RECEIPT NOT CONFIGURED FOR THE SELECTED COMPANY - " + source.company);
            }
            return null;
        }
    }
}