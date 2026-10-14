
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
    [RoutePrefix("BOMinvoiceService")]
    public class BOMinvoiceServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<BOMinvoice>();
            result.Item = new BOMinvoice();
            return this.Ok(result);
        }

        
              [HttpPost]
        [Route("Report")]
        public async Task<IHttpActionResult> Report(ActionRequest<BOMinvoiceCreteria> request)
        {
            BOMinvoiceDataService BOMinvoiceData = new BOMinvoiceDataService();
            List<BOMInvoiceReportDetail> result = await Task<List<BOMInvoiceReportDetail>>.Run(() => BOMinvoiceData.ReportSummary(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BOMinvoiceCreteria> request)
        {
            BOMinvoiceDataService BOMinvoiceData = new BOMinvoiceDataService();
            List<BOMinvoice> result = await Task<List<BOMinvoice>>.Run(() => BOMinvoiceData.Search(request.Item));
            if (request.Item.lrid > 0 || (request.Item.id > 0 && result.Count == 1))
            {
                BOMinvoiceCover cover = new BOMinvoiceCover();
                cover.Items = result;
                cover.supportpo = result.Count > 0 && result[0].supportpo;
                cover.lrid = request.Item.lrid > 0 ? request.Item.lrid : result[0].lrid;
                BOMLrentryDataService LrentryinvoiceData = new BOMLrentryDataService();
                var lrinfo = LrentryinvoiceData.Search(new BOMLrentryCreteria()
                {
                    id = cover.lrid
                });

                if (lrinfo != null && lrinfo.Count == 1)
                {
                    cover.BOMLrentryno = lrinfo[0].BOMLrentryno;
                    cover.lrno = lrinfo[0].lrno;
                    cover.entrydate = result.Count > 0 ? result[0].entrydate : lrinfo[0].BOMLrentrydate;
                    cover.supplierid = lrinfo[0].supplierid;
                    cover.status = lrinfo[0].Status;
                    cover.modeofdelivery = lrinfo[0].modeofdelivery;
                    cover.invoicedate = lrinfo[0].invoicedate;
                    cover.invoiceno = lrinfo[0].invoiceno;
                    cover.retaillocationid = lrinfo[0].retaillocationid;
                    cover.noofpieces = lrinfo[0].noofpieces;
                    cover.noofbundels = lrinfo[0].noofbundels;
                    cover.transportid = lrinfo[0].transportid;
                    cover.premargin = lrinfo[0].premargin;
                    cover.lrdate = lrinfo[0].lrdate;
                    cover.companyid = lrinfo[0].lrcompanyid;
                    if (lrinfo[0].retaillocationid > 0)
                    {
                        ReferencelistDataService referencelistDataService = new ReferencelistDataService();
                        var locationinfo = referencelistDataService.Search(new ReferencelistCreteria()
                        {
                            id = lrinfo[0].retaillocationid
                        });
                        if (locationinfo != null && locationinfo.Count == 1 && locationinfo[0].additionalinfo != null &&
                            locationinfo[0].additionalinfo.parentid > 0)
                        {
                            cover.companyid = locationinfo[0].additionalinfo.parentid;
                        }
                    }
                    if (result.Count == 1)
                    {
                        cover.totalamount = result[0].bominvoicetaxitems.Sum(e => (e.type == BOMinvoicetax.EntryType.Taxable) ? e.amounton : 0);
                        cover.totaldiscount = result[0].bominvoicetaxitems.Sum(e => (e.type == BOMinvoicetax.EntryType.Taxable ||
                        e.type == BOMinvoicetax.EntryType.Discount ||
                        e.type == BOMinvoicetax.EntryType.DiscuntOnDiscount) ? e.discount : 0);
                        cover.totaltax = result[0].bominvoicetaxitems.Sum(e => e.taxamount);

                        cover.additionalamount = result[0].bominvoicetaxitems.Sum(e => (e.type == BOMinvoicetax.EntryType.Adjustment ||
                        e.type == BOMinvoicetax.EntryType.Courier || e.type == BOMinvoicetax.EntryType.Fright ||
                        e.type == BOMinvoicetax.EntryType.Insurance || e.type == BOMinvoicetax.EntryType.Packing ||
                        e.type == BOMinvoicetax.EntryType.ServiceCharge) ? e.amounton : 0);
                        cover.additionaltax = result[0].bominvoicetaxitems.Sum(e => (e.type == BOMinvoicetax.EntryType.Adjustment ||
                        e.type == BOMinvoicetax.EntryType.Courier || e.type == BOMinvoicetax.EntryType.Fright ||
                        e.type == BOMinvoicetax.EntryType.Insurance || e.type == BOMinvoicetax.EntryType.Packing ||
                        e.type == BOMinvoicetax.EntryType.ServiceCharge) ? e.taxamount : 0);
                        if (cover.totalamount - cover.totaldiscount > 0)
                            cover.addtionalcostpercentage = Math.Round((cover.additionalamount + cover.additionaltax) / (cover.totalamount - cover.totaldiscount) * 100, 4);
                    }
                }

                if (request.Item.id > 0)
                {
                    if (result[0].suppliercompanyid > 0)
                    {
                        SuppliercompanyDataService sDataService = new SuppliercompanyDataService();
                        List<Suppliercompany> suppliercompany = await Task<List<Suppliercompany>>.Run(() => sDataService.Search(new SuppliercompanyCreteria()
                        { id = result[0].suppliercompanyid }));
                        if (suppliercompany.Count == 1)
                            cover.suppliercompanyname = suppliercompany[0].name;
                    }

                    if (result[0].companyid > 0)
                    {
                        CompanyDataService sDataService = new CompanyDataService();
                        List<Company> buyingcompany = await Task<List<Company>>.Run(() => sDataService.Search(new CompanyCreteria()
                        {
                            id = result[0].companyid
                        }
                        ));
                        if (buyingcompany.Count == 1)
                        {
                            cover.companyname = buyingcompany[0].name;
                            cover.companytitle = buyingcompany[0].title;
                            cover.companyid = buyingcompany[0].id;
                            cover.internalbuyer = buyingcompany[0].internalbuyer;
                        }
                    }
                    result[0].bominvoiceitems = result[0].bominvoiceitems.OrderBy(e => e.sno).ThenBy(x => x.id).ToList();
                }
                return this.Ok(cover);
            }
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<BOMinvoiceCover> request)
        {
            BOMinvoiceDataService BOMinvoiceData = new BOMinvoiceDataService();

            if (request.Item.Items != null)
                request.Item.Items.ForEach(e => e.ChangeStatus(e.id > 0 ? BaseEntity.DataStatus.Update : BaseEntity.DataStatus.Create));

            foreach (var item in request.Item.Items)
            {
                item.ChangeStatus(item.id > 0 ? BaseEntity.DataStatus.Update : BaseEntity.DataStatus.Create);
                if (item.bominvoicetaxitems != null)
                    item.bominvoicetaxitems.ForEach(e => e.ChangeStatus(e.id > 0 ? BaseEntity.DataStatus.Update : BaseEntity.DataStatus.Create));
            }


            BOMinvoiceCover result = await Task<BOMinvoiceCover>.Run(() => BOMinvoiceData.Update(request.Item));
            return this.Ok(result);

        }

        [HttpPost]
        [Route("SearchBarcode")]
        public async Task<IHttpActionResult> SearchBarcode(ActionMulitipleResponse<BOMinvoiceItemBarcode> request)
        {
            BOMinvoiceDataService BOMinvoiceData = new BOMinvoiceDataService();
            try
            {
                List<BOMinvoiceItemBarcode> result = await Task<List<BOMinvoiceItemBarcode>>.Run(() => CreateBarcodeList(BOMinvoiceData.Context.OrganizationID,request.Items));
                return this.Ok(result);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(BOMinvoiceData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
                return this.InternalServerError();
            }
        }


        private List<BOMinvoiceItemBarcode> CreateBarcodeList(long organizationid,List<BOMinvoiceItemBarcode> items)
        {
            List<long> productids = new List<long>();
            ProductsDataService productsDataService = new ProductsDataService();
            productsDataService.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            List<BOMinvoiceItemBarcode> retlist = new List<BOMinvoiceItemBarcode>();
            StockDataService stockDataService = new StockDataService();
            stockDataService.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            foreach (var item in items)
            {
                if (item.code == "null")
                    continue;

                if (!productids.Contains(item.productid))
                    productids.Add(item.productid);

                List<string> jsonbarcode = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(item.code);

                var products = productsDataService.Search(new ProductsCreteria()
                {
                    id = item.productid
                });

                var barcodeConfig = InfyPOS.Processors.BarcodeManager.Instance(productsDataService.Context.OrganizationID).Get(products[0].barcodemode, item.companyid);
                List<string> barcodelist = stockDataService.GetAllBarcodes(item.id);
                foreach (var bitem in barcodelist)
                {
                    retlist.Add(new BOMinvoiceItemBarcode()
                    {
                        code = bitem,
                        name = item.name,
                        id = item.id,
                        productid = item.productid,
                        qty = item.qty,
                        sno = item.sno,
                        no = barcodeConfig.GetValue(bitem)
                    });
                }


                //var barcodeConfig = InfyPOS.Processors.BarcodeManager.Instance.Get(products[0].barcodemode, item.companyid);
                //foreach (var bitem in jsonbarcode)
                //{
                //    var codes = bitem.Split('-');
                //    if (codes.Length == 1)
                //    {


                //        retlist.Add(new BOMinvoiceItemBarcode()
                //        {
                //            code = codes[0],
                //            name = item.name,
                //            id = item.id,
                //            productid = item.productid,
                //            qty = item.qty,
                //            sno = item.sno,
                //            no = barcodeConfig.GetValue(codes[0])
                //        });

                //    }
                //    else
                //    {
                //        var from = barcodeConfig.GetValue(codes[0]);
                //        var to = barcodeConfig.GetValue(codes[1]);
                //        for (int id = from; id <= to; id++)
                //        {
                //            retlist.Add(new BOMinvoiceItemBarcode()
                //            {
                //                code = barcodeConfig.GetValue(codes[0], id),
                //                name = item.name,
                //                id = item.id,
                //                productid = item.productid,
                //                qty = item.qty,
                //                sno = item.sno,
                //                no = id
                //            });
                //        }
                //    }
                //}
            }


            var blist = stockDataService.Search(new StockCreteria()
            {
                barcodes = retlist.ConvertAll<string>(e => "'" + e.code + "'").ToList()
            });
            foreach (var item in retlist)
            {
                if (blist.Exists(e => e.barcode == item.code))
                {
                    var bitem = blist.Find(e => e.barcode == item.code);
                    item.price = bitem.price;
                    item.qty = bitem.qty;
                    item.imageid = bitem.imageid;
                }
            }
            if (productids.Count > 0)
            {
                var list = productsDataService.GetSellingModes(productids);
                foreach (var item in list)
                    retlist.Where(e => e.productid == item.Key).ToList().ForEach(e => e.sellingmode = item.Value);
            }
            return retlist;
        }

        [HttpPost]
        [Route("SaveInvoiceItems")]
        public async Task<IHttpActionResult> SaveInvoiceItems(ActionRequest<BOMinvoiceDetail> request)
        {
            BOMinvoiceDataService BOMinvoiceData = new BOMinvoiceDataService();
            if (request.Item.id <= 0)
            {
                var result = BOMinvoiceData.InsertDirectPurchase(request.Item);
                return this.Ok(result);
            }
            else
            {
                var result = BOMinvoiceData.UpdateDirectPurchase(request.Item);
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<BOMinvoiceDetail> request)
        {
            BOMinvoiceDataService BOMinvoiceData = new BOMinvoiceDataService();
            BOMinvoice result = await Task<BOMinvoice>.Run(() => BOMinvoiceData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<BOMinvoiceDetail> request)
        {
            BOMinvoiceDataService BOMinvoiceData = new BOMinvoiceDataService();
            BOMinvoice result = await Task<BOMinvoice>.Run(() => BOMinvoiceData.Update(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("DeleteAll")]
        public async Task<IHttpActionResult> Delete(ActionRequest<BOMinvoiceCover> request)
        {
            BOMinvoiceDataService BOMinvoiceData = new BOMinvoiceDataService();
            bool result = await Task<bool>.Run(() => BOMinvoiceData.Delete(request.Item));
            return this.Ok(result);
        }


        [HttpPost]
        [Route("PrintBarcode")]
        public async Task<IHttpActionResult> PrintBarcode(ActionRequest<BOMPrint> request)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            var reportformats = config.Search(new ConfigurationCreteria
            {
                id = request.Item.reportid
            });

            List<string> base64Response = new List<string>();
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            System.Text.StringBuilder asb = new System.Text.StringBuilder();

            InfyPOS.Report.CitizenBarcode barcodeprinter = new Report.CitizenBarcode("",
                reportformats[0].reportformat.format, reportformats[0].reportformat.mode, reportformats[0].nooflines);

           
            try
            {
                List<Report.IBarcodeFormat> barcodeList = new List<Report.IBarcodeFormat>();
                foreach (var bcode in request.Item.items.OrderBy(x => x.sno))
                {
                    for (var i = 0; i < bcode.pqty; i++)
                    {
                        barcodeList.Add(bcode);
                    }
                }
                var response = barcodeprinter.Render(barcodeList.ConvertAll(x => x as Report.IBarcodeFormat));
                foreach (var item in response)
                {
                    asb.AppendLine(item);
                    asb.Append(";");
                    var bytes = System.Text.Encoding.ASCII.GetBytes(item);
                    sb.Append(Convert.ToBase64String(bytes));
                    sb.Append(";");
                }
                base64Response.Add(sb.ToString());
            }catch(Exception exp)
            {
                WebAPI.Common.Logger.Instance(config.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, request);
            }
            return this.Ok(base64Response);
        }

        [AllowAnonymous]
        [HttpGet]
        [Route("PrintPDF/{organizationid}/{id}")]
        public async Task<IHttpActionResult> PrintPDF(string organizationid, string id)
        {
            BOMinvoiceDataService BOMConsumptionData = new BOMinvoiceDataService();
            try
            {
                BOMConsumptionData.Context = new UserClaim()
                {
                    OrganizationID = long.Parse(organizationid)
                };
                long invoiceid = long.Parse(id);

                FileContentInfo result = await Task<FileContentInfo>.Run(() =>
                {

                    var print = BOMConsumptionData.Print(new BOMinvoiceCreteria() { id = invoiceid });
                    return PrintWorkOrder(BOMConsumptionData.Context.OrganizationID, print);
                });
                return new FileActionResult(false, "", result.Extension, result.Content);
            }
            catch (Exception exp)
            {
                WebAPI.Common.Logger.Instance(BOMConsumptionData.Context.OrganizationID).Log(exp, WebAPI.Common.Logger.LogType.Error, "PrintPDF/" + id);
                return this.Ok(exp);
            }
        }

        public FileContentInfo PrintWorkOrder(long organizationid, BOMinvoiceDetail result)
        {
            bool hasSizeBased = true;
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = "BOMINVOICE",
                organizationid = result.companyid
            });

            if (lstconfig == null || lstconfig.Count == 0)
                return null;
            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            //labreport.format.PageBody.WrapRow = true;

            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();


            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;
            labreport.DataItems = result.bominvoiceitems.OrderBy(x=>x.sno).ToList();
            printer.Title = result.companyname+ " Delivery challon @ " + result.invoiceentryno + "-" + result.entrydate.ToString("dd-MM-yyyy");

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
                fci.FileName = "bominvoice.pdf";
                fci.Extension = "application/pdf";
                return fci;
            }

        }



        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<BOMinvoice> request)
        {
            BOMinvoiceDataService BOMinvoiceData = new BOMinvoiceDataService();
            bool result = await Task<bool>.Run(() => BOMinvoiceData.Delete(request.Item));
            return this.Ok(result);
        }
    }
}