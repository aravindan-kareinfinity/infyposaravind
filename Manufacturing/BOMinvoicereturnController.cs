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
    [RoutePrefix("bominvoicereturnService")]
    public class bominvoicereturnServiceController : ApiController
    {

        [HttpGet]
        [Route("Entity")]
        public async Task<IHttpActionResult> Entity()
        {
            var result = new ActionRequest<BOMInvoicereturn>();
            result.Item = new BOMInvoicereturn();
            return this.Ok(result);
        }

        [HttpPost]
        [Route("SearchItems")]
        public async Task<IHttpActionResult> SearchItems(ActionRequest<BOMInvoicereturnProductsCreteria> request)
        {
            BOMInvoicereturn result = await Task<BOMInvoicereturn>.Run(() => SearchItems(request.Item));
            return this.Ok(result);
        }

        private BOMInvoicereturn SearchItems(BOMInvoicereturnProductsCreteria creteria)
        {

            BominvoiceitemsDataService invoiceitemsdataservice = new BominvoiceitemsDataService();
            TaxDataService taxdataservice = new TaxDataService();
            List<BOMInvoicereturnitems> resultItems = new List<BOMInvoicereturnitems>();
            BOMInvoicereturn returninvoice = new BOMInvoicereturn();
            BOMinvoiceDataService invoicedataservice = new BOMinvoiceDataService();
            returninvoice.BOMinvoicereturnitems = resultItems;
            creteria.returndate = (creteria.returndate == DateTime.MinValue ? invoicedataservice.DateTimeNow : creteria.returndate);


            List<BOMinvoice> invoiceList = invoicedataservice.Search(new BOMinvoiceCreteria()
            {
                supplierid = creteria.supplierid,
                invoiceno = creteria.invoiceno,
                BOMLrentryno = creteria.lrentryno,
                barcode = creteria.barcode,
                LimitOne = true
                //fromdate = creteria.returndate.AddMonths(-12),
                //todate = creteria.returndate
            });

            if (invoiceList.Count > 0)
            {
                var invoice = invoiceList.OrderByDescending(e => e.invoicedate).First();
                returninvoice.supplierid = invoice.supplierid;
                returninvoice.suppliercompanyid = invoice.suppliercompanyid;

                var invoiceitems = invoiceitemsdataservice.SearchAll(invoice.id);


                var taxlist = taxdataservice.Search(new TaxCreteria());


                foreach (var invitem in invoiceitems)
                {
                    if (creteria.mode == "damage" &&
                        invitem.damagedqty - invitem.returnqty <= 0)
                        continue;

                    BOMInvoicereturnitems item = new BOMInvoicereturnitems();
                    item.suppliercompanyid = invoice.suppliercompanyid;
                    item.supplierid = invoice.supplierid;
                    item.isinterstatesale = invoice.isinterstatesale;
                    item.companyid = invoice.companyid;
                    item.colourid = invitem.colourid;
                    item.bomcategoryid = invitem.bomcategoryid;
                    item.bominvoiceitemsid = invitem.id;
                    item.bomid = invitem.bomid;
                    item.sno = invitem.sno;
                    item.bominwardcode = invitem.bominwardcode;
                    if (invitem.buyingprice == 0)
                    {
                        invitem.buyingprice = invitem.basebuyingprice;
                        if (invitem.purchasediscount > 0)
                            invitem.buyingprice -= invitem.purchasediscount / invitem.buyingqty;
                    }
                    if (creteria.returntype == Lrinvoicereturn.ReturnTypes.DebitNote)
                    {
                        item.qty = creteria.mode == "damage" ? invitem.damagedqty : (invitem.buyingqty - (invitem.sellableqty + invitem.returnqty));
                        item.rate = invitem.buyingprice + invitem.purchasediscount;
                        item.discountpercentage = invitem.purchasediscountpercentage;
                        item.discount = invitem.purchasediscount;
                    }
                    else
                    {
                        item.qty = invitem.buyingqty - (invitem.damagedqty + invitem.returnqty);
                        item.rate = invitem.buyingprice;
                        item.discountpercentage = 0;
                        item.discount = 0;
                    }
                    item.invoiceno = invoice.invoiceno;
                    item.invoicedate = invoice.invoicedate;
                    if (item.qty <= 0) continue;
                    item.taxid = invitem.taxid;
                    item.taxpercentage = invitem.purchasetaxpercentage;
                    item.tax = Math.Round(invitem.buyingprice * item.taxpercentage / 100, 2);
                    resultItems.Add(item);
                }


                if (invoice != null && invoice.bominvoicetaxitems != null &&
                    invoice.bominvoicetaxitems.Exists(e => e.type == BOMinvoicetax.EntryType.Packing))
                {
                    returninvoice.packingcharge = invoice.bominvoicetaxitems.Find(e => e.type == BOMinvoicetax.EntryType.Packing).netamount;
                }
                if (invoice != null && invoice.bominvoicetaxitems != null &&
                    invoice.bominvoicetaxitems.Exists(e => e.type == BOMinvoicetax.EntryType.Fright))
                {
                    returninvoice.freightamt = invoice.bominvoicetaxitems.Find(e => e.type == BOMinvoicetax.EntryType.Fright).amounton;
                    returninvoice.freighttaxamt = invoice.bominvoicetaxitems.Find(e => e.type == BOMinvoicetax.EntryType.Fright).taxamount;
                    returninvoice.freighttaxid = invoice.bominvoicetaxitems.Find(e => e.type == BOMinvoicetax.EntryType.Fright).taxid;
                }
            }


            var lrinvoiceitemsids = returninvoice.BOMinvoicereturnitems.ConvertAll(e => e.bominvoiceitemsid).Distinct();
            SortedDictionary<long, decimal> discountlist = invoiceitemsdataservice.SearchAllDiscount(lrinvoiceitemsids.ToList());
            foreach (var discout in discountlist)
            {
                var items = returninvoice.BOMinvoicereturnitems.FindAll(e => e.bominvoiceitemsid == discout.Key);
                foreach (var item in items)
                {
                    item.discount = discout.Value;
                    item.discountpercentage = Math.Round(item.discount / item.rate * 100, 4);
                    item.tax = Math.Round(item.tax * item.discountpercentage / 100, 4);
                }
            }
            return returninvoice;
        }


        [HttpPost]
        [Route("Search")]
        public async Task<IHttpActionResult> Search(ActionRequest<BOMInvoicereturnCreteria> request)
        {
            BOMInvoicereturnDataService LrinvoicereturnData = new BOMInvoicereturnDataService();
            List<BOMInvoicereturn> result = await Task<List<BOMInvoicereturn>>.Run(() => LrinvoicereturnData.Search(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public async Task<IHttpActionResult> Save(ActionRequest<BOMInvoicereturn> request)
        {
            BOMInvoicereturnDataService LrinvoicereturnData = new BOMInvoicereturnDataService();
            if (request.Item.id <= 0)
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Create);
                if (request.Item.BOMinvoicereturnitems != null)
                {
                    foreach (var item in request.Item.BOMinvoicereturnitems)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Create);
                    }
                }
                if (request.Item.BOMinvoicereturnitems.GroupBy(e => new { e.companyid, e.supplierid, e.suppliercompanyid }).Count() == 1)
                {
                    if (request.Item.companyid == 0)
                    {
                        request.Item.companyid = request.Item.BOMinvoicereturnitems[0].companyid;
                        request.Item.suppliercompanyid = request.Item.BOMinvoicereturnitems[0].suppliercompanyid;
                        request.Item.supplierid = request.Item.BOMinvoicereturnitems[0].supplierid;
                        request.Item.isinterstatesale = request.Item.BOMinvoicereturnitems[0].isinterstatesale;
                    }
                    else
                    {
                        request.Item.BOMinvoicereturnitems.ForEach(x =>
                        {
                            x.suppliercompanyid = request.Item.suppliercompanyid;
                            x.companyid = request.Item.companyid;
                            x.supplierid = request.Item.supplierid;
                            x.isactive = true;
                        });
                    }
                    BOMInvoicereturn result = await Task<BOMInvoicereturn>.Run(() => LrinvoicereturnData.Insert(request.Item));
                    return this.Ok(result);
                }
                else
                {
                    List<BOMInvoicereturn> result = await Task<List<BOMInvoicereturn>>.Run(() => LrinvoicereturnData.InsertAll(request.Item));
                    return this.Ok(result);
                }
            }
            else
            {
                request.Item.ChangeStatus(BaseEntity.DataStatus.Update);
                if (request.Item.BOMinvoicereturnitems != null)
                {
                    foreach (var item in request.Item.BOMinvoicereturnitems)
                    {
                        item.ChangeStatus(BaseEntity.DataStatus.Update);
                    }
                }
                BOMInvoicereturn result = await Task<BOMInvoicereturn>.Run(() => LrinvoicereturnData.Update(request.Item));
                return this.Ok(result);
            }
        }

        [HttpPost]
        [Route("Insert")]
        public async Task<IHttpActionResult> Insert(ActionRequest<BOMInvoicereturn> request)
        {
            BOMInvoicereturnDataService LrinvoicereturnData = new BOMInvoicereturnDataService();
            BOMInvoicereturn result = await Task<BOMInvoicereturn>.Run(() => LrinvoicereturnData.Insert(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Update")]
        public async Task<IHttpActionResult> Update(ActionRequest<BOMInvoicereturn> request)
        {
            BOMInvoicereturnDataService LrinvoicereturnData = new BOMInvoicereturnDataService();
            BOMInvoicereturn result = await Task<BOMInvoicereturn>.Run(() => LrinvoicereturnData.Update(request.Item));
            return this.Ok(result);
        }
        [HttpPost]
        [Route("UpdateLRInfo")]
        public async Task<IHttpActionResult> UpdateLRInfo(ActionRequest<BOMInvoicereturn> request)
        {
            BOMInvoicereturnDataService LrinvoicereturnData = new BOMInvoicereturnDataService();
            BOMInvoicereturn result = await Task<BOMInvoicereturn>.Run(() => LrinvoicereturnData.UpdateLRInfo(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("ClosedSupplier")]
        public async Task<IHttpActionResult> ClosedSupplier(ActionRequest<BOMInvoicereturn> request)
        {
            BOMInvoicereturnDataService LrinvoicereturnData = new BOMInvoicereturnDataService();
            BOMInvoicereturn result = await Task<BOMInvoicereturn>.Run(() => LrinvoicereturnData.ClosedSupplier(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Delete")]
        public async Task<IHttpActionResult> Delete(ActionRequest<BOMInvoicereturn> request)
        {
            BOMInvoicereturnDataService LrinvoicereturnData = new BOMInvoicereturnDataService();
            bool result = await Task<bool>.Run(() => LrinvoicereturnData.Delete(request.Item));
            return this.Ok(result);
        }

        [HttpPost]
        [Route("Print")]
        public async Task<IHttpActionResult> Print(ActionRequest<BOMInvoicereturn> request)
        {
            BOMInvoicereturnDataService LrinvoicereturnData = new BOMInvoicereturnDataService();
            BOMInvoicereturnPrint result = await Task<BOMInvoicereturnPrint>.Run(() => LrinvoicereturnData.Print(request.Item));
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
                BOMInvoicereturnDataService LrinvoicereturnData = new BOMInvoicereturnDataService();
                LrinvoicereturnData.Context = new UserClaim() { OrganizationID = long.Parse(organizationid) };
                BOMInvoicereturnPrint result = LrinvoicereturnData.Print(new BOMInvoicereturn() { id = long.Parse(id) });
                FileContentInfo byteArray = PrintDebitNote(long.Parse(organizationid), result);
                return new FileActionResult(false, "", byteArray.Extension, byteArray.Content);
            }
            catch (Exception exp)
            {
                return this.Ok(exp);
            }
        }
        private FileContentInfo PrintDebitNote(long organizationid, BOMInvoicereturnPrint result)
        {
            InfyPOS.Data.ConfigurationDataService config = new ConfigurationDataService();
            config.Context = new UserClaim()
            {
                OrganizationID = organizationid
            };
            string key = result.returntype == 0 ? "BOMDEBITNOTE" : "BOMCREDITNOTE";
            var lstconfig = config.Search(new ConfigurationCreteria
            {
                key = key,
                organizationid = result.companyid
            });

            if (lstconfig == null || lstconfig.Count == 0)
            {
                lstconfig = config.Search(new ConfigurationCreteria
                {
                    key = key
                });
            }
            if (lstconfig == null || lstconfig.Count == 0)
                return null;

            ReportLibrary.DotMatrixReporter labreport = new ReportLibrary.DotMatrixReporter(organizationid);
            labreport.format = ReportLibrary.DosFormatParser.ParseXML(organizationid, lstconfig[0].reportformat.format);
            labreport.DefaultPageSettings = labreport.format.GetDefaultSetting();
            labreport.Additional = new SortedDictionary<string, object>();

            ReportLibrary.PDFPrinter printer = new ReportLibrary.PDFPrinter();
            printer.Report = labreport;
            printer.EnablePageNumber = lstconfig[0].reportformat.format.Contains("PageCount");
            labreport.DataSource = result;

            labreport.DataItems = result.BOMinvoicereturnitems;
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
    }
}