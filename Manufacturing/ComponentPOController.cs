
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
    [RoutePrefix("ComponentPOService")]
    public class ComponentPOServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<ComponentPO>();
            result.Item = new ComponentPO();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<ComponentPOCreteria> request)
        {
            ComponentPODataService ComponentPOData = new ComponentPODataService();
            List<ComponentPO> result = await Task<List<ComponentPO>>.Run(() => ComponentPOData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Report")]
        public async Task<IHttpActionResult> Report(ActionRequest<ComponentPOCreteria> request)
        {
            ComponentPODataService ComponentPOData = new ComponentPODataService();
            if (request.Item.mode == 0)
            {
                List<ComponentPOReportSummary> result = await Task<List<ComponentPOReportSummary>>.Run(() => ComponentPOData.ReportSummary(request.Item));
                return this.Ok(result);
            }
            else
            {
                List<ComponentPOReportDetail> result = await Task<List<ComponentPOReportDetail>>.Run(() => ComponentPOData.ReportDetail(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("SearchPO")]
        public async Task<IHttpActionResult> SearchPO(ActionRequest<ComponentPOCreteria> request)
        {
            ComponentPODataService ComponentPOData = new ComponentPODataService();
            List<ComponentPOItemDetail> result = await Task<List<ComponentPOItemDetail>>.Run(() => ComponentPOData.SearchPO(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<ComponentPO> request)
        {
            ComponentPODataService ComponentPOData = new ComponentPODataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                if (request.Item.ComponentPOitems != null)
                {
                    foreach (var item in request.Item.ComponentPOitems)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Create);
                    }
                }
                ComponentPO result = await Task<ComponentPO>.Run(() => ComponentPOData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                if (request.Item.ComponentPOitems != null)
                {
                    foreach (var item in request.Item.ComponentPOitems)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Update);
                    }
                }
                ComponentPO result = await Task<ComponentPO>.Run(() => ComponentPOData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<ComponentPO> request)
        {
            ComponentPODataService ComponentPOData = new ComponentPODataService();
            ComponentPO result = await Task<ComponentPO>.Run(() => ComponentPOData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<ComponentPO> request)
        {
            ComponentPODataService ComponentPOData = new ComponentPODataService();
            ComponentPO result = await Task<ComponentPO>.Run(() => ComponentPOData.Update(request.Item));
            return this.Ok(result);
        }

        //[HttpPost]
        //[Route("UpdateStatus")]
        //public async Task<IHttpActionResult> UpdateStatus(ActionRequest<ComponentPO> request)
        //{
        //    ComponentPODataService ComponentPOData = new ComponentPODataService();
        //    ComponentPO result = await Task<ComponentPO>.Run(() => ComponentPOData.UpdateStatus(request.Item));
        //    return this.Ok(result);
        //}

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<ComponentPO> request)
        {
            ComponentPODataService ComponentPOData = new ComponentPODataService();
            bool result = await Task<bool>.Run(() => ComponentPOData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Cancel")]
        public async Task<IHttpActionResult> Cancel(ActionRequest<ComponentPO> request)
        {
            ComponentPODataService ComponentPOData = new ComponentPODataService();
            bool result = await Task<bool>.Run(() => ComponentPOData.Cancel(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Complete")]
        public async Task<IHttpActionResult> Complete(ActionRequest<ComponentPO> request)
        {
            ComponentPODataService ComponentPOData = new ComponentPODataService();
            bool result = await Task<bool>.Run(() => ComponentPOData.Complete(request.Item));
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("Publish/{organizationid}/{id}")]
        public async Task<IHttpActionResult> Publish(string organizationid, string id)
        {
            ComponentPODataService ComponentPOData = new ComponentPODataService() { Context = new UserClaim() { OrganizationID = long.Parse(organizationid) } };
            bool result = await Task<bool>.Run(() =>
            {
                ComponentPOPrint print = ComponentPOData.Print(new ComponentPO() { id = long.Parse(id) });
                return Publish(ComponentPOData.Context.OrganizationID, print);
            });
            return this.Ok(result);
        }

        private bool Publish(long organizationid, ComponentPOPrint print)
        {
            if (!string.IsNullOrEmpty(print.supplier.contactnumber) && print.supplier.contactnumber.Length >= 10 &&
                !string.IsNullOrEmpty(InfyPOS.Services.ConfigManager.Instance(organizationid).Config.hostedserver))
            {
                string licensekey = LicenseManager.License.Key.Replace("-", "");
                InfyPOS.Controllers.ImageController.HashDetail hashDetail = new ImageController.HashDetail(organizationid, ImageController.HashDetail.GetLongHash(licensekey), print.id);
                string smscontent = print.company.name + " PO (Order No " + print.pono + "/" + print.podate.ToString("dd-MM-yyyy") + ")\r\n";
                smscontent += InfyPOS.Services.ConfigManager.Instance(organizationid).Config.hostedserver + (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.hostedserver.EndsWith("/") ? "" : "/") + "File/PO/" + LicenseManager.License.Key.Replace("-", "") + "/" + System.Web.HttpUtility.UrlEncode(hashDetail.Hash);
                return InfyPOS.Services.SMSManager.Instance.SendSMS(organizationid, print.supplier.contactnumber, smscontent);
            }
            return false;
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintNOPDF/{organizationid}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> PrintNOPDF(string organizationid, string id,
            string id1 = null, string id2 = null, string id3 = null)
        {
            try
            {
                if (!string.IsNullOrEmpty(id1))
                    id += "/" + id1;
                if (!string.IsNullOrEmpty(id2))
                    id += "/" + id2;
                if (!string.IsNullOrEmpty(id3))
                    id += "/" + id3;

                ComponentPODataService ComponentPOData = new ComponentPODataService();
                ComponentPOData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    ComponentPOPrint print = ComponentPOData.Print(new ComponentPO() { pono = id });
                    return PrintPO(ComponentPOData.Context.OrganizationID, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string id)
        {
            try
            {
                if (long.Parse(id) == 0) return null;
                ComponentPODataService ComponentPOData = new ComponentPODataService();
                ComponentPOData.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
                ComponentPOPrint print = ComponentPOData.Print(new ComponentPO() { id = long.Parse(id) });
                FileContentInfo byteArray = PrintPO(long.Parse(organizationid), print);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }
        public FileContentInfo PrintPO(long organizationid, ComponentPOPrint result)
        {
            bool hasSizeBased = true;
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "COMPPO",
                organizationid = result.companyid
            });

            if (lstconfig == null || lstconfig.Count == 0)
            {
                lstconfig = config.Search(new ConfigurationCreteria
                {
                    key = "COMPPO"
                });
            }
            if (lstconfig == null || lstconfig.Count == 0)
                return null;
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            //labreport.format.PageBody.WrapRow = true;
            if (labreport.format.PageBody.Items[0].Items.Exists(e => e.DataType == ReportLibrary.DOSReporter.DataType.Image))
            {
                if (result.ComponentPOitems.Any(e => !string.IsNullOrEmpty(e.imageid)))
                {
                    var availableheight = labreport.format.PageBody.Length * labreport.format.PageBody.LineHeight;
                    labreport.format.PageBody.LineHeight = 50;
                    labreport.format.PageBody.Length = (int)(availableheight / labreport.format.PageBody.LineHeight);
                }
                else
                {
                    labreport.format.PageBody.Items[0].Items.RemoveAll(e => e.DataType == ReportLibrary.DOSReporter.DataType.Image);
                }
            }

            if (lstconfig[0].reportformat.format.IndexOf("ColourBased") > 0)
            {
                result.ColourBased = result.ComponentPOitems.GroupBy(x => x.bomid).ToList().ConvertAll(e =>
                  {
                      return new ComponentPOitemsDetail()
                      {
                          amount = e.Sum(x => x.amount),
                          avgqty = e.Average(x => x.qty),
                          noofcolours = e.Count(),
                          rate = e.First().rate,
                          brandname = e.First().brandname,
                          designid = e.First().designid,
                          bomcode = e.First().bomcode,
                          qty = e.Sum(x => x.qty),
                          colourlist = string.Join(",", e.ToList().ConvertAll(x =>
                          {
                              if (string.IsNullOrEmpty(x.colourcode) ||
                                    x.colourcode.IndexOf("/") < 0)
                                  return "";
                              return x.colourcode.Substring(x.colourcode.IndexOf('/')+1);
                          }))
                      };
                  });
            }



            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;
            labreport.DataItems = result.ComponentPOitems;
            printer.Title = result.company.name + " Purchase Order @ " + result.pono + "-" + result.podate.ToString("dd-MM-yyyy");
            printer.Keywords = result.supplier.name + "," + result.suppliercompany.name;

            List<string> imageids = labreport.format.GetImages();
            var imageData = new Common.ImageData();
            foreach (var imgid in imageids.FindAll(x => x.Length > 6))
            {
                var file = imageData.GetData(imgid.LongFromBase62());
                if (file != null)
                {
                    try
                    {
                        var img = imageData.FromBytes(file.data);
                        labreport.Additional.Add("IMG-" + imgid, img);
                    }
                    catch (Exception exp)
                    { }
                }
            }

            if (!string.IsNullOrEmpty(result.company.logo))
            {
                var file = imageData.GetData(result.company.logo.LongFromBase62());
                if (file != null)
                {
                    var img = imageData.FromBytes(file.data);
                    labreport.Additional.Add("LOGO", img);
                }
            }

            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "po.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }

        //[HttpPost]
        //[Route("SearchPOSummary")]
        //public async Task<IHttpActionResult> SearchPOSummary(ActionRequest<ComponentPOCreteria> request)
        //{
        //    ComponentPODataService podata = new ComponentPODataService();
        //    List<ComponentPO> result = await Task<List<ComponentPO>>.Run(() => podata.SearchPOSummary(request.Item));
        //    return this.Ok(result);
        //}

        //[HttpPost]
        //[Route("SearchPendingPO")]
        //public async Task<IHttpActionResult> SearchPendingPO(ActionRequest<ComponentPOCreteria> request)
        //{
        //    ComponentPODataService podata = new ComponentPODataService();
        //    List<ComponentPODetails> result = await Task<List<ComponentPODetails>>.Run(() => podata.SearchPendingPO(request.Item));
        //    return this.Ok(result);
        //}

    }
}