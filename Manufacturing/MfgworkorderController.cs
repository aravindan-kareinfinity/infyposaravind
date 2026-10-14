
using InfyPOS.Common;
using InfyPOS.Data;
using InfyPOS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using WebAPI.Data;
namespace InfyPOS.Controllers
{
    [RoutePrefix("MfgworkorderService")]
    public class MfgworkorderServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Mfgworkorder>();
            result.Item = new Mfgworkorder();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<MfgworkorderCreteria> request)
        {
            MfgworkorderDataService MfgworkorderData = new MfgworkorderDataService();
            List<Mfgworkorder> result = await Task<List<Mfgworkorder>>.Run(() => MfgworkorderData.Search(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("Report")]
        public async Task<IHttpActionResult> Report(ActionRequest<MfgworkorderCreteria> request)
        {
            MfgworkorderDataService MfgworkorderData = new MfgworkorderDataService();
            List<MfgworkorderSummary> result = await Task<List<MfgworkorderSummary>>.Run(() => MfgworkorderData.Report(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Mfgworkorder> request)
        {
            MfgworkorderDataService MfgworkorderData = new MfgworkorderDataService();
            if (request.Item.id <= 0)
            {
                Mfgworkorder result = await Task<Mfgworkorder>.Run(() => MfgworkorderData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Mfgworkorder result = await Task<Mfgworkorder>.Run(() => MfgworkorderData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Mfgworkorder> request)
        {
            MfgworkorderDataService MfgworkorderData = new MfgworkorderDataService();
            Mfgworkorder result = await Task<Mfgworkorder>.Run(() => MfgworkorderData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Mfgworkorder> request)
        {
            MfgworkorderDataService MfgworkorderData = new MfgworkorderDataService();
            Mfgworkorder result = await Task<Mfgworkorder>.Run(() => MfgworkorderData.Update(request.Item));
            return this.Ok(result);
        }


        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}/{template?}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string id, string template = null)
        {
            MfgworkorderDataService PurchaseorderData = new MfgworkorderDataService();
            try
            {
                PurchaseorderData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                long poid = 0;
                long templateid = 0;
                poid = long.Parse(id);
                if(!string.IsNullOrEmpty(template))
                {
                    templateid = long.Parse(template);
                }

                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    MfgworkorderPrint print = PurchaseorderData.Print(new MfgworkorderCreteria()
                    {
                        id = poid,
                        workorderno = id
                    });
                    return PrintWorkOrder(PurchaseorderData.Context.OrganizationID, templateid, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(PurchaseorderData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }

        public FileContentInfo PrintWorkOrder(long organizationid,long templateid, MfgworkorderPrint result)
        {
            bool hasSizeBased = true;
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "WORKORDER",
                organizationid = result.companyid
            });

            if (lstconfig == null || lstconfig.Count == 0)
                lstconfig = config.Search(new ConfigurationCreteria
                {
                    key = "WORKORDER"
                });
            if (lstconfig == null || lstconfig.Count == 0)
                return null;

            if(lstconfig.Count>0 && templateid>0)
                lstconfig = lstconfig.FindAll(x=>x.id == templateid);

            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            //labreport.format.PageBody.WrapRow = true;

            ReportLibrary.DosFormat.Line table = labreport.format.PageBody.Items.Exists(e => e.IsTable) ?
                labreport.format.PageBody.Items.Find(e => e.IsTable) : null;

            if (hasSizeBased && table != null &&
            table.Items.Exists(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield))
            {
                List<ReportLibrary.DosFormat.Content> dynamiccolumns = new List<ReportLibrary.DosFormat.Content>();
                int index = 0;
                var rootcolumn = table.Items.First(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield);

                if (result.sizelist == null)
                {
                    result.sizelist = new List<Mfgworkorder.SizeInfo>();
                    foreach (var orderitem in result.Mfgworkorderitems)
                    {
                        result.sizelist.AddRange(orderitem.sizelist.ConvertAll(x => new Mfgworkorder.SizeInfo()
                        {
                            sizeid = x.sizeid,
                            sizeindex = x.sizeindex
                        }));
                    }
                    var sizelist = new List<Mfgworkorder.SizeInfo>();
                    foreach (var sizegroup in result.sizelist.GroupBy(e => new { e.sizeid }))
                    {
                        sizelist.Add(new Mfgworkorder.SizeInfo()
                        {
                            sizeid = sizegroup.Key.sizeid,
                            sizeindex = sizegroup.OrderBy(x => x.sizeindex).First().sizeindex
                        });
                    }
                    result.sizelist = sizelist;
                    ReferencelistDataService referencelistDataService = new ReferencelistDataService()
                    {
                        Context = new UserClaim() { OrganizationID = organizationid }
                    };
                    var sizenames = referencelistDataService.SearchList(new ReferencelistCreteria()
                    {
                        ids = sizelist.ConvertAll(e => e.sizeid)
                    });
                    result.sizelist.ForEach(e =>
                    {
                        if (sizenames.Exists(x => x.id == e.sizeid))
                        {
                            e.name = sizenames.Find(x => x.id == e.sizeid).name;
                        }
                    });
                }
                var sizeExistOrder = WebAPI.Data.Utility.GetSizeList(result.sizelist.ConvertAll(e => e.name.ToUpper()));
                if (rootcolumn.Text.IndexOf(',') > 0)
                {
                    sizeExistOrder = new List<string>(rootcolumn.Text.Split(','));
                }

                var correctsizes = sizeExistOrder.FindAll(x => result.sizelist.Exists(y => y.name.ToUpper() == x.ToUpper()));
                if (correctsizes.Count == 0)
                {
                    sizeExistOrder = result.sizelist.ConvertAll(x => x.name).ToList();
                }
                sizeExistOrder.Add("Total");
                var totalwidth = table.Items.Sum(e => e.Length) - rootcolumn.Length;

                labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
                var colwidth = ((labreport.DefaultPageSettings.MarginBound.Width - totalwidth) / (sizeExistOrder.Count));
                foreach (var sitem in sizeExistOrder)
                {
                    ReportLibrary.DosFormat.Content content = new ReportLibrary.DosFormat.Content();
                    content.Type = ReportLibrary.DosFormat.CotnentType.Field;
                    content.Text = sitem;
                    content.Length = colwidth;
                    if (sitem == "Total")
                        content.Length = rootcolumn.Length;
                    content.Align = rootcolumn.Align;
                    content.Format = rootcolumn.Format;
                    content.Total = rootcolumn.Total;
                    content.Name = string.Format(rootcolumn.Name, index);
                    index++;
                    dynamiccolumns.Add(content);
                }
                //labreport.format.PageBody.WrapRow = true;
                table.Items.InsertRange(table.Items.IndexOf(rootcolumn), dynamiccolumns);
                table.Items.Remove(rootcolumn);

                foreach (var item in result.Mfgworkorderitems)
                {
                    item.sizelist.ForEach(x =>
                    {
                        if (result.sizelist.Exists(y => y.sizeid == x.sizeid))
                            x.name = result.sizelist.Find(y => y.sizeid == x.sizeid).name;
                    });
                    item.jumpqty = new List<PurchaseorderitemsDetail.SizeQty>();
                    foreach (var sitem in sizeExistOrder)
                    {
                        decimal qty = 0;
                        if (sitem == "Total")
                        {
                            qty = item.sizelist.Sum(e => e.qty);
                        }
                        else if (item.sizelist.Any(e => e.name == sitem))
                        {
                            qty = item.sizelist.First(e => e.name == sitem).qty;
                        }
                        item.jumpqty.Add(new PurchaseorderitemsDetail.SizeQty()
                        {
                            name = sitem,
                            qty = qty
                        });
                    }
                }
                hasSizeBased = true;
            }


            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();


            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;
            labreport.DataItems = result.Mfgworkorderitems;
            printer.Title = result.company.name + " Work Order @ " + result.workorderno + "-" + result.wodate.ToString("dd-MM-yyyy");
            printer.Keywords = result.supplier.name;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            int sno = 1;
            result.Mfgworkorderitems.ForEach(x =>
            {
                x.sno = sno++;
            });
            if (result.Mfgworkorderitems != null)
            {
                sno = 1;
                result.Mfgworkorderitems.ForEach(x =>
                {
                    x.sno = sno++;
                });
            }

            if (InfyPOS.Services.ConfigManager.Instance(organizationid).Config.mfg_mode == 6)
            {


                sno = 1;
                List<Mfgworkorderitemsizes> mfgworkorderitemsizes = new List<Mfgworkorderitemsizes>();
                result.Mfgworkorderitems.ForEach(x =>
                {
                    if (x.sizelist != null)
                    {
                        x.sizelist.ForEach(y =>
                        {
                            y.sno = sno++;
                        });
                        mfgworkorderitemsizes.AddRange(x.sizelist);
                    }
                });
                result.Mfgworkorderitemsizes = mfgworkorderitemsizes;
                labreport.DataItems = mfgworkorderitemsizes;
                ReferencelistDataService referencelistDataService = new ReferencelistDataService()
                {
                    Context = new UserClaim() { OrganizationID = organizationid }
                };
                var sizenames = referencelistDataService.SearchList(new ReferencelistCreteria()
                {
                    ids = mfgworkorderitemsizes.ConvertAll(e => e.sizeid)
                });
                mfgworkorderitemsizes.ForEach(e =>
                {
                    if (sizenames.Exists(x => x.id == e.sizeid))
                    {
                        e.name = sizenames.Find(x => x.id == e.sizeid).name;
                    }
                });
            }

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
            if(result.mfgsamplesizes != null && result.mfgsamplesizes.Count>0)
                result.mfgsamplesizes.RemoveAll(x => x.qty == 0);
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
        public async Task<IHttpActionResult> Delete(ActionRequest<Mfgworkorder> request)
        {
            MfgworkorderDataService MfgworkorderData = new MfgworkorderDataService();
            bool result = await Task<bool>.Run(() => MfgworkorderData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}