
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
    [RoutePrefix("BOMConsumptionService")]
    public class BOMConsumptionServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<BOMConsumption>();
            result.Item = new BOMConsumption();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BOMConsumptionCreteria> request)
        {
            BOMConsumptionDataService BOMConsumptionData = new BOMConsumptionDataService();
            List<BOMConsumption> result = await Task<List<BOMConsumption>>.Run(() => BOMConsumptionData.Search(request.Item));
            return this.Ok(result);
        }

      

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<BOMConsumption> request)
        {
            BOMConsumptionDataService BOMConsumptionData = new BOMConsumptionDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                if (request.Item.BOMConsumptionitems != null)
                {
                    foreach (var item in request.Item.BOMConsumptionitems)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Create);
                    }
                }
                BOMConsumption result = await Task<BOMConsumption>.Run(() => BOMConsumptionData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                if (request.Item.BOMConsumptionitems != null)
                {
                    foreach (var item in request.Item.BOMConsumptionitems)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Update);
                    }
                }
                BOMConsumption result = await Task<BOMConsumption>.Run(() => BOMConsumptionData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("FindBomStock")]
        public async Task<IHttpActionResult> FindBomStock(ActionRequest<BomStockRequest> request)
        {
            BOMConsumptionDataService BOMConsumptionData = new BOMConsumptionDataService();
            decimal result = await Task<decimal>.Run(() => BOMConsumptionData.FindBomStock(request.Item));
            return this.Ok(result);
        }

        
        [HttpPost]
        [Route("ImportBOM")]
        public async Task<IHttpActionResult> ImportBOM(ActionRequest<BOMConsumption> request)
        {
            BOMConsumptionDataService BOMConsumptionData = new BOMConsumptionDataService();
            List<BOMConsumptionitems> result = await Task<List<BOMConsumptionitems>>.Run(() => BOMConsumptionData.ImportBOM(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<BOMConsumption> request)
        {
            BOMConsumptionDataService BOMConsumptionData = new BOMConsumptionDataService();
            BOMConsumption result = await Task<BOMConsumption>.Run(() => BOMConsumptionData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<BOMConsumption> request)
        {
            BOMConsumptionDataService BOMConsumptionData = new BOMConsumptionDataService();
            BOMConsumption result = await Task<BOMConsumption>.Run(() => BOMConsumptionData.Update(request.Item));
            return this.Ok(result);
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{mode}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid,string mode, string id,
            string id1 = null, string id2 = null, string id3 = null)
        {
            BOMConsumptionDataService BOMConsumptionData = new BOMConsumptionDataService();
            try
            {
                BOMConsumptionData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                long poid = 0;
                if (string.IsNullOrEmpty(id1) &&
                    string.IsNullOrEmpty(id2) &&
                    string.IsNullOrEmpty(id3))
                {
                    poid = long.Parse(id);
                    id = "";
                }
                else
                {
                    if (!string.IsNullOrEmpty(id1))
                        id += "/" + id1;
                    if (!string.IsNullOrEmpty(id2))
                        id += "/" + id2;
                    if (!string.IsNullOrEmpty(id3))
                        id += "/" + id3;
                }

                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    BOMConsumePrint print = null;
                    if (mode == "DC")
                    {
                        print = BOMConsumptionData.Print(new BOMConsumptionCreteria()
                        {
                            id = poid,
                            entryno = id
                        });
                    }
                    else
                    {
                        print = BOMConsumptionData.PrintSummary(new BOMConsumptionCreteria()
                        {
                            id = poid,
                            entryno = id
                        });
                    }
                    return PrintWorkOrder(BOMConsumptionData.Context.OrganizationID,mode, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(BOMConsumptionData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }

        public FileContentInfo PrintWorkOrder(long organizationid, string mode, BOMConsumePrint result)
        {
            bool hasSizeBased = true;
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = mode == "DC" ?"COMPONENTDC": "COMPONENTSTOCKDC",
                organizationid = result.companyid
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            //labreport.format.PageBody.WrapRow = true;

            if (hasSizeBased &&
                labreport.format.PageBody.Items[0].IsTable &&
            labreport.format.PageBody.Items[0].Items.Exists(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield))
            {
                List<ReportLibrary.DosFormat.Content> dynamiccolumns = new List<ReportLibrary.DosFormat.Content>();
                int index = 0;
                var rootcolumn = labreport.format.PageBody.Items[0].Items.First(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield);
              
                var totalwidth = labreport.format.PageBody.Items[0].Items.Sum(e => e.Length) - rootcolumn.Length;

                //labreport.format.PageBody.WrapRow = true;
                labreport.format.PageBody.Items[0].Items.InsertRange(labreport.format.PageBody.Items[0].Items.IndexOf(rootcolumn), dynamiccolumns);
                labreport.format.PageBody.Items[0].Items.Remove(rootcolumn);

                hasSizeBased = true;
            }


            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();


            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;
            labreport.DataItems = result.BOMConsumptionitems;
            printer.Title = result.company.name + " Delivery challon @ " + result.entryno + "-" + result.entrydate.ToString("dd-MM-yyyy");
            //printer.Keywords = result.supplier.name + "," + result.suppliercompany.name;

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

            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "workorder.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }


        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<BOMConsumption> request)
        {
            BOMConsumptionDataService BOMConsumptionData = new BOMConsumptionDataService();
            bool result = await Task<bool>.Run(() => BOMConsumptionData.Delete(request.Item));
            return this.Ok(result);
        }

       

    }
}