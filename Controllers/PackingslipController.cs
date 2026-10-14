
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
    [RoutePrefix("PackingslipService")]
    public class PackingslipServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<Packingslip>();
            result.Item = new Packingslip();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<PackingslipCreteria> request)
        {
            PackingslipDataService PackingslipData = new PackingslipDataService();
            List<Packingslip> result = await Task<List<Packingslip>>.Run(() => PackingslipData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpGet]
        [Route("SearchPackingNo/{locationid}/{id}/{id1?}/{id2?}/{id3?}")]
        public async Task<IHttpActionResult> Barcode(string locationid, string id,
           string id1 = null, string id2 = null, string id3 = null)
        {
            PackingslipDataService StockData = new PackingslipDataService();
            if (!string.IsNullOrEmpty(id1))
                id += "/" + id1;
            if (!string.IsNullOrEmpty(id2))
                id += "/" + id2;
            if (!string.IsNullOrEmpty(id3))
                id += "/" + id3;

            if (id.IndexOf("~dot~") >= 0) id = id.Replace("~dot~", ".");
            if (id.IndexOf("~sls~") >= 0) id = id.Replace("~sls~", "/");
            var result = await Task<List<Packingslip>>.Run(() => StockData.Search(new PackingslipCreteria()
            {
                locationid = long.Parse(locationid),
                packingno = id
            }));
            return this.Ok(result.Count == 1 ? result.First() : null);
        }

        [HttpGet]
        [Route("UpdateBox/{id}")]
        public async Task<IHttpActionResult> UpdateBox(string id)
        {
            PackingslipDataService StockData = new PackingslipDataService();
            var result = await Task<Packingslip>.Run(() => StockData.UpdateBox(long.Parse(id)));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ItemReport")]
        public async Task<IHttpActionResult> ItemReport(ActionRequest<PackingslipCreteria> request)
        {
            PackingslipDataService PackingslipData = new PackingslipDataService();
            List<PSSummaryItem> result = await Task<List<PSSummaryItem>>.Run(() => PackingslipData.ItemReport(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("SummaryReport")]
        public async Task<IHttpActionResult> SummaryReport(ActionRequest<PackingslipCreteria> request)
        {
            PackingslipDataService PackingslipData = new PackingslipDataService();
            List<PSSummary> result = await Task<List<PSSummary>>.Run(() => PackingslipData.SummaryReport(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ComboReport")]
        public async Task<IHttpActionResult> ComboReport(ActionRequest<PackingslipCreteria> request)
        {
            PackingslipDataService PackingslipData = new PackingslipDataService();
            if (request.Item.auditactiontype == 9)
            {
                List<PSOrphan> result = await Task<List<PSOrphan>>.Run(() => PackingslipData.ComboOrphanReport(request.Item));
                return this.Ok(result);
            }
            else
            {
                List<PSComboSummaryItem> result = await Task<List<PSComboSummaryItem>>.Run(() => PackingslipData.ComboReport(request.Item));
                return this.Ok(result);
            }
        }


        [HttpPost]
        [Route("GenerateCombo")]
        public async Task<IHttpActionResult> GenerateCombo(ActionRequest<ComboPack> request)
        {
            PackingslipDataService PackingslipData = new PackingslipDataService();
            List<Barcodedgoods> result = await Task<List<Barcodedgoods>>.Run(() => PackingslipData.GenerateCombo(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<Packingslip> request)
        {
            PackingslipDataService PackingslipData = new PackingslipDataService();
            if (request.Item.id <= 0)
            {
                Packingslip result = await Task<Packingslip>.Run(() => PackingslipData.Insert(request.Item));
                return this.Ok(result);
            }
            else
            {
                Packingslip result = await Task<Packingslip>.Run(() => PackingslipData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{printtemplate}/{id}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string printtemplate, string id)
        {
            try
            {
                var poid = long.Parse(id);

                PackingslipDataService PurchaseorderData = new PackingslipDataService();
                PurchaseorderData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {
                    var print = PurchaseorderData.Print(new PackingslipCreteria()
                    {
                        id = poid
                    });
                    return PrintJobOrder(PurchaseorderData.Context.OrganizationID, printtemplate, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(long.Parse(organizationid)).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }
        public FileContentInfo PrintJobOrder(long organizationid, string templateid, Packingslip result)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService() { Context = new UserClaim() { OrganizationID = organizationid } }; ;
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "PACKINGSLIP",
                organizationid = result.companyid
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;

            if (lstconfig.Exists(x => x.id.ToString() == templateid))
            {
                lstconfig.RemoveAll(x => x.id.ToString() != templateid);
            }
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            //labreport.format.PageBody.WrapRow = true;

            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();
            if (labreport.format.PageHeader != null &&
                    !string.IsNullOrEmpty(labreport.format.PageHeader.Style))
            {
                labreport.BodyStyle.Font = labreport.format.GetStyle(labreport.format.PageHeader.Style);
                labreport.HeaderStyle.Font = new System.Drawing.Font(labreport.BodyStyle.Font, System.Drawing.FontStyle.Bold);
                labreport.TotalStyle.Font = new System.Drawing.Font(labreport.BodyStyle.Font, System.Drawing.FontStyle.Bold);
            }

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;
            labreport.DataItems = result.barcodes;

            if (lstconfig[0].reportformat.format.Contains("barcodesordered"))
            {
                int sno = 1;
                result.barcodesordered = result.barcodes.OrderBy(x => x.productname).ThenBy(y => y.sizename).ToList();
                result.barcodesordered.ForEach(x => x.sno = sno++);
            }

            if (lstconfig[0].reportformat.format.Contains("sampleorder") && 
                !string.IsNullOrEmpty(result.sono))
            {
                MfgsampleorderDataService mfgsampleorderDataService = new MfgsampleorderDataService() { Context = new UserClaim() { OrganizationID = organizationid } };
                var sampleorders = mfgsampleorderDataService.Search(new MfgsampleorderCreteria()
                {
                    sono = result.sono
                });
                if(sampleorders.Count>0)
                {
                    result.mfgsampleorder = sampleorders[0];
                    if (result.barcodes != null)
                        result.barcodes.ForEach(x => x.parent = result);
                }
            }


            if (labreport.format.PageBody.Items[0].IsTable &&
                labreport.format.PageBody.Items[0].Items.Exists(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield))
            {
                var sizeList = result.barcodes.GroupBy(e => new { e.sizename }).ToList();
                var sizeExistOrder = WebAPI.Data.Utility.GetSizeList(sizeList.ConvertAll(e => String.IsNullOrEmpty(e.Key.sizename) ? "" : e.Key.sizename.ToUpper()));
                if (Services.ConfigManager.Instance(organizationid).Config.po_autocolumn)
                {
                    var sizelist = WebAPI.Data.Utility.GetSizes();
                    var newsizeExistOrder = result.barcodes.GroupBy(e => string.IsNullOrEmpty(e.sizename) ? "" : e.sizename.ToUpper()).ToList().ConvertAll(e => e.Key);

                    if (newsizeExistOrder.Count > 4)
                        sizeExistOrder = newsizeExistOrder;

                    sizeExistOrder.Sort(delegate (string a, string b)
                    {
                        var aindex = sizelist.IndexOf(a);
                        var bindex = sizelist.IndexOf(b);
                        return aindex.CompareTo(bindex);
                    });
                }

                int maxcolumn = Services.ConfigManager.Instance(organizationid).Config.po_maxcolumn > 0 ? Services.ConfigManager.Instance(organizationid).Config.po_maxcolumn : 16;
                if (sizeExistOrder != null && maxcolumn > sizeExistOrder.Count)
                {
                    if (sizeExistOrder != null && sizeExistOrder.Count != 0)
                    {
                        {
                            var currentbarcodelist = result.barcodes;
                            result.barcodes = new List<Packingslip.Barcodes>();
                            List<ReportLibrary.DosFormat.Content> dynamiccolumns = new List<ReportLibrary.DosFormat.Content>();
                            int index = 0;
                            var rootcolumn = labreport.format.PageBody.Items[0].Items.First(e => e.Type == ReportLibrary.DosFormat.CotnentType.Dynamicfield);
                            if (rootcolumn.Text.IndexOf(',') > 0)
                            {
                                sizeExistOrder = new List<string>(rootcolumn.Text.Split(','));
                            }
                            sizeExistOrder.Add("Total");
                            var totalwidth = labreport.format.PageBody.Items[0].Items.Sum(e => e.Length) - rootcolumn.Length;
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
                            labreport.format.PageBody.Items[0].Items.InsertRange(labreport.format.PageBody.Items[0].Items.IndexOf(rootcolumn), dynamiccolumns);
                            labreport.format.PageBody.Items[0].Items.Remove(rootcolumn);
                            foreach (var group in currentbarcodelist.GroupBy(x => new { x.uanitem.code, x.colourname, x.productname, x.mrp, x.price }))
                            {
                                var item = new Packingslip.Barcodes();
                                item.itemdesigncode = group.Key.code;
                                item.productname = group.Key.productname;
                                item.colourname = group.Key.colourname;
                                item.hsncode = group.First().hsncode;
                                item.mrp = group.First().mrp;
                                item.price = group.First().price;
                                item.jumpqty = new List<SalesorderitemsDetail.SizeQty>();
                                foreach (var sitem in sizeExistOrder)
                                {
                                    decimal qty = 0;
                                    if (sitem == "Total")
                                    {
                                        qty = group.Sum(e => e.qty);
                                    }
                                    else if (group.Any(e => e.sizename == sitem))
                                    {
                                        qty = group.Sum(e => e.sizename == sitem ? e.qty : 0);
                                    }
                                    item.jumpqty.Add(new SalesorderitemsDetail.SizeQty()
                                    {
                                        name = sitem,
                                        oqty = qty
                                    });
                                }
                                item.qty = item.jumpqty.Find(x => x.name == "Total").oqty;
                                item.sno = result.barcodes.Count + 1;
                                result.barcodes.Add(item);
                            }
                            result.barcodes = result.barcodes.OrderBy(x => x.itemdesigncode).ToList();
                            int sno = 1;
                            result.barcodes.ForEach(x => x.sno = sno++);
                            result.barcodesordered = result.barcodes.OrderBy(x => x.sno).ToList();
                            labreport.DataItems = result.barcodes;
                        }
                 
                    }
                }
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

            using (System.IO.MemoryStream stream = printer.Print(labreport.format.GetDefaultSetting()))
            {
                var fci = new FileContentInfo(new ByteArrayContent(stream.ToArray()));
                fci.FileName = "joborder.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }


        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<Packingslip> request)
        {
            PackingslipDataService PackingslipData = new PackingslipDataService();
            Packingslip result = await Task<Packingslip>.Run(() => PackingslipData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<Packingslip> request)
        {
            PackingslipDataService PackingslipData = new PackingslipDataService();
            Packingslip result = await Task<Packingslip>.Run(() => PackingslipData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<Packingslip> request)
        {
            PackingslipDataService PackingslipData = new PackingslipDataService();
            bool result = await Task<bool>.Run(() => PackingslipData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}