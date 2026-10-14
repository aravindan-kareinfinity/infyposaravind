
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
using System.Management;
using WebAPI.Data;

namespace InfyPOS.Controllers
{
    [RoutePrefix("Mfg_SamplesService")]
    public class Mfg_SamplesServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Mfg_Samples>();
            result.Item = new Mfg_Samples();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Summary")]
        public async Task<IHttpActionResult> Summary(ActionRequest<Mfg_SamplesCreteria> request)
        {
            Mfg_SamplesDataService Mfg_SamplesData = new Mfg_SamplesDataService();
            if(request.Item.showdetail)
            {
                MfgReportServiceController.StockReport result = await Task<MfgReportServiceController.StockReport>.Run(() => Mfg_SamplesData.SearchDesignwiseStock(request.Item));
                return this.Ok(result);
            }
            else
            {
                List<Mfg_SampleSummary> result = await Task<List<Mfg_SampleSummary>>.Run(() => Mfg_SamplesData.Summary(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Stock")]
        public async Task<IHttpActionResult> Stock(ActionRequest<Mfg_SamplesCreteria> request)
        {
            Mfg_SamplesDataService Mfg_SamplesData = new Mfg_SamplesDataService();
            Mfg_SampleStock result = await Task<Mfg_SampleStock>.Run(() => Mfg_SamplesData.Stock(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<Mfg_SamplesCreteria> request)
        {
            Mfg_SamplesDataService Mfg_SamplesData = new Mfg_SamplesDataService();
            List<Mfg_Samples> result = await Task<List<Mfg_Samples>>.Run(() => Mfg_SamplesData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Mfg_Samples> request)
        {
            Mfg_SamplesDataService Mfg_SamplesData = new Mfg_SamplesDataService();
            if (request.Item.id <= 0)
            {
                Mfg_Samples result = await Task<Mfg_Samples>.Run(() => Mfg_SamplesData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Mfg_Samples result = await Task<Mfg_Samples>.Run(() => Mfg_SamplesData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Mfg_Samples> request)
        {
            Mfg_SamplesDataService Mfg_SamplesData = new Mfg_SamplesDataService();
            Mfg_Samples result = await Task<Mfg_Samples>.Run(() => Mfg_SamplesData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Mfg_Samples> request)
        {
            Mfg_SamplesDataService Mfg_SamplesData = new Mfg_SamplesDataService();
            Mfg_Samples result = await Task<Mfg_Samples>.Run(() => Mfg_SamplesData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Mfg_Samples> request)
        {
            Mfg_SamplesDataService Mfg_SamplesData = new Mfg_SamplesDataService();
            bool result = await Task<bool>.Run(() => Mfg_SamplesData.Delete(request.Item));
            return this.Ok(result);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string id)
        {
            try
            {
                if (long.Parse(id) == 0) return null;
                Mfg_SamplesDataService ComponentPOData = new Mfg_SamplesDataService();
                ComponentPOData.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
                var print = ComponentPOData.Search(new Mfg_SamplesCreteria() { id = long.Parse(id) ,includeitems =true });
                
                FileContentInfo byteArray = PrintPO(long.Parse(organizationid), print[0]);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }
        public FileContentInfo PrintPO(long organizationid, Mfg_Samples result)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "SAMPLEPRINT"
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;

            result.IsIssue = result.qty > 0;
            if(result.qty < 0)
            {
                result.sampleitems.ForEach(x=>x.qty = Math.Abs(x.qty));
            }
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);

            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");

            if (labreport.format.PageBody != null &&
                labreport.format.PageBody.Items[0].IsTable &&
                labreport.format.PageBody.Items[0].Items.Exists(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield))
            {
                var sizeList = result.sampleitems.GroupBy(e => new { e.size }).ToList();
                var sizeExistOrder = WebAPI.Data.Utility.GetSizeList(sizeList.ConvertAll(e => e.Key.size.ToUpper()));
                if (Services.ConfigManager.Instance(organizationid).Config.po_autocolumn)
                {
                    var sizelist = WebAPI.Data.Utility.GetSizes();
                    sizeExistOrder = result.sampleitems.GroupBy(e => e.size.ToUpper()).ToList().ConvertAll(e => e.Key);
                    sizeExistOrder.Sort(delegate (string a, string b)
                    {
                        var aindex = sizelist.IndexOf(a);
                        var bindex = sizelist.IndexOf(b);
                        return aindex.CompareTo(bindex);
                    });
                }

                List<ReportLibrary.DosFormat.Content> dynamiccolumns = new List<ReportLibrary.DosFormat.Content>();
                int index = 0;
                var rootcolumn = labreport.format.PageBody.Items[0].Items.First(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield);
                if (rootcolumn.Text.IndexOf(',') > 0)
                {
                    sizeExistOrder = new List<string>(rootcolumn.Text.Split(','));
                }
                sizeExistOrder.Add("Total");
                var totalwidth = labreport.format.PageBody.Items[0].Items.Sum(e => e.Length) - rootcolumn.Length;
                int diff = 0;
                int colwidth = 0;
                bool rootwidth = false;
                labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
                if (rootcolumn.Length > totalwidth)
                {
                    rootwidth = true;
                    colwidth = (rootcolumn.Length * 2 - (labreport.format.PageBody.Items[0].Items.Sum(e => e.Length))) / sizeExistOrder.Count;
                }
                else
                {
                    colwidth = ((labreport.DefaultPageSettings.MarginBound.Width - totalwidth) / sizeExistOrder.Count);
                    diff = (labreport.DefaultPageSettings.MarginBound.Width - totalwidth) - (colwidth * sizeExistOrder.Count);
                }
                foreach (var sitem in sizeExistOrder)
                {
                    ReportLibrary.DosFormat.Content content = new ReportLibrary.DosFormat.Content();
                    content.Type = ReportLibrary.DosFormat.CotnentType.Field;
                    content.Text = sitem;
                    content.Length = colwidth;
                    if (sitem == "Total" && !rootwidth)
                        content.Length = (int)(rootcolumn.Length);
                    content.Align = rootcolumn.Align;
                    content.Format = rootcolumn.Format;
                    content.Total = rootcolumn.Total;
                    content.Name = string.Format(rootcolumn.Name, index);
                    index++;
                    dynamiccolumns.Add(content);
                }
                //labreport.format.PageBody.WrapRow = true;
                labreport.format.PageBody.Items[0].Items.InsertRange(labreport.format.PageBody.Items[0].Items.IndexOf(rootcolumn), dynamiccolumns);
                labreport.format.PageBody.Items[0].Items.Remove(rootcolumn);
                labreport.format.PageBody.Items[0].Items.Last().Length += diff;

                var groupedList = result.sampleitems.GroupBy(x => new { x.designcode });
                result.sampleitems = new List<Mfg_Samples>();
                foreach (var group in groupedList)
                {
                    var item = group.First();
                    item.jumpqty = new List<Mfg_Samples.SizeQty>();
                    foreach (var sitem in sizeExistOrder)
                    {
                        decimal qty = 0;
                        if (sitem == "Total")
                        {
                            qty = group.Sum(e => e.qty);
                        }
                        else if (group.Any(e => e.size == sitem))
                        {
                            qty = group.Sum(e => e.size == sitem ? e.qty : 0);
                        }
                        item.jumpqty.Add(new Mfg_Samples.SizeQty()
                        {
                            name = sitem,
                            oqty = qty
                        });
                    }
                    item.qty = group.Sum(x => x.qty);
                    item.orderid = result.sampleitems.Count + 1;
                    result.sampleitems.Add(item);
                }

            }

            labreport.DataSource = result;
            labreport.DataItems = result.sampleitems;
            printer.Title = " Samples @ " + result.receiptno + "-" + result.entrydate.ToString("dd-MM-yyyy");

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
                fci.FileName = "swatchandfolder.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }
    }
}